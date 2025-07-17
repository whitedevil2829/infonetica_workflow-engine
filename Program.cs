
// Program.cs
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WorkflowEngine;
using WorkflowEngine.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<WorkflowStore>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

// --- Data Store ---
// public class WorkflowStore
// {
//     public Dictionary<string, WorkflowDefinition> Definitions { get; } = new();
//     public Dictionary<string, WorkflowInstance> Instances { get; } = new();
// }

// --- Helper Functions ---
bool IsValidWorkflowDef(WorkflowDefinition def, out string error)
{
    error = null;
    // Unique State IDs
    var stateIds = new HashSet<string>();
    foreach (var s in def.States)
        if (!stateIds.Add(s.Id)) { error = "Duplicate State IDs."; return false; }
    // Must have exactly one initial state
    if (def.States.Count(s => s.IsInitial) != 1) { error = "Must have exactly one initial state."; return false; }
    // All toState and fromStates must exist in States
    var validIds = def.States.Select(s => s.Id).ToHashSet();
    foreach (var a in def.Actions)
    {
        if (!validIds.Contains(a.ToState)) { error = $"Action {a.Id} toState invalid."; return false; }
        if (!a.FromStates.All(validIds.Contains)) { error = $"Action {a.Id} fromStates invalid."; return false; }
    }
    return true;
}

// --- API Endpoints ---

// 1. Create Workflow Definition
app.MapPost("/workflows", (WorkflowDefinition def, WorkflowStore store) =>
{
    if (store.Definitions.ContainsKey(def.Id))
        return Results.BadRequest("WorkflowDefinition with same Id exists.");

    if (!IsValidWorkflowDef(def, out var error))
        return Results.BadRequest(error);

    store.Definitions[def.Id] = def;
    return Results.Created($"/workflows/{def.Id}", def);
});

// 2. Get Workflow Definition
app.MapGet("/workflows/{id}", (string id, WorkflowStore store) =>
    store.Definitions.TryGetValue(id, out var def)
        ? Results.Ok(def)
        : Results.NotFound("WorkflowDefinition not found.")
);

// 3. List Workflow Definitions
app.MapGet("/workflows", (WorkflowStore store) =>
    Results.Ok(store.Definitions.Values)
);

// 4. Create Workflow Instance
app.MapPost("/workflows/{id}/instances", (string id, WorkflowStore store) =>
{
    if (!store.Definitions.TryGetValue(id, out var def))
        return Results.NotFound("WorkflowDefinition not found.");

    var initial = def.States.FirstOrDefault(s => s.IsInitial && s.Enabled);
    if (initial == null) return Results.BadRequest("No enabled initial state.");

    var instance = new WorkflowInstance
    {
        Id = Guid.NewGuid().ToString(),
        WorkflowDefinitionId = id,
        CurrentState = initial.Id,
    };
    store.Instances[instance.Id] = instance;
    return Results.Created($"/instances/{instance.Id}", instance);
});

// 5. Get Instance State & History
app.MapGet("/instances/{id}", (string id, WorkflowStore store) =>
    store.Instances.TryGetValue(id, out var inst)
        ? Results.Ok(inst)
        : Results.NotFound("Instance not found.")
);

// 6. List Instances
app.MapGet("/instances", (WorkflowStore store) =>
    Results.Ok(store.Instances.Values)
);

// 7. Execute Action on Instance
app.MapPost("/instances/{instanceId}/actions/{actionId}", (string instanceId, string actionId, WorkflowStore store) =>
{
    if (!store.Instances.TryGetValue(instanceId, out var inst))
        return Results.NotFound("Instance not found.");

    if (!store.Definitions.TryGetValue(inst.WorkflowDefinitionId, out var def))
        return Results.NotFound("WorkflowDefinition not found.");

    var currState = def.States.FirstOrDefault(s => s.Id == inst.CurrentState);
    if (currState == null || !currState.Enabled)
        return Results.BadRequest("Current state is invalid or disabled.");

    if (currState.IsFinal)
        return Results.BadRequest("Instance is at a final state.");

    var action = def.Actions.FirstOrDefault(a => a.Id == actionId);
    if (action == null) return Results.BadRequest("Action not found.");
    if (!action.Enabled) return Results.BadRequest("Action is disabled.");
    if (!action.FromStates.Contains(inst.CurrentState))
        return Results.BadRequest("Action cannot be executed from current state.");

    var toState = def.States.FirstOrDefault(s => s.Id == action.ToState && s.Enabled);
    if (toState == null)
        return Results.BadRequest("Target state is invalid or disabled.");

    // Move instance
    var history = new ActionHistoryItem
    {
        ActionId = action.Id,
        Timestamp = DateTime.UtcNow,
        FromState = inst.CurrentState,
        ToState = toState.Id
    };
    inst.CurrentState = toState.Id;
    inst.History.Add(history);

    return Results.Ok(inst);
});

app.Run();
