// Program.cs
// Author: Shivam Rai
// ROll NO. 23MF10038
// For: Infonetica Workflow Engine Assignment (July 2025)
// Notes: This is a minimal backend API for creating and running workflows as state machines.

//        Comments added throughout to explain my reasoning and design decisions.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WorkflowEngine;
using WorkflowEngine.Models;

// --- Setup and Dependency Injection ---
var builder = WebApplication.CreateBuilder(args);
// Swagger is helpful for quick API testing (no Postman needed)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// This singleton just keeps everything in RAM for now
builder.Services.AddSingleton<WorkflowStore>();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();

// ------------------------------
// Helper: Validate workflow definition before storing it.
// Blocks bad configs early: duplicate state IDs, missing initial state, bad transitions.
// Returns error as string, just to keep things simple.
bool IsValidWorkflowDef(WorkflowDefinition def, out string error)
{
    error = null;

    // Check for duplicate state IDs (can cause bugs in lookup)
    var stateIds = new HashSet<string>();
    foreach (var state in def.States)
    {
        if (!stateIds.Add(state.Id))
        {
            error = "Two states have the same ID. IDs must be unique.";
            return false;
        }
    }

    // Must have exactly one initial state
    if (def.States.Count(s => s.IsInitial) != 1)
    {
        error = "A workflow must have exactly one initial state.";
        return false;
    }

    // Make sure all action transitions are valid (no typos in state IDs)
    var validStateIds = def.States.Select(s => s.Id).ToHashSet();
    foreach (var action in def.Actions)
    {
        if (!validStateIds.Contains(action.ToState))
        {
            error = $"Action '{action.Name}' has an invalid 'toState'.";
            return false;
        }
        if (!action.FromStates.All(validStateIds.Contains))
        {
            error = $"Action '{action.Name}' has an invalid 'fromStates' list.";
            return false;
        }
    }
    return true;
}

// ------------------------------
// API ENDPOINTS
// ------------------------------

// Add a new workflow definition (states + actions at once)
app.MapPost("/workflows", (WorkflowDefinition def, WorkflowStore store) =>
{
    // Don't let people accidentally overwrite an existing workflow
    if (store.Definitions.ContainsKey(def.Id))
        return Results.BadRequest("A workflow with this ID already exists. Please use a different ID.");

    // Do a full structure validation
    if (!IsValidWorkflowDef(def, out var error))
        return Results.BadRequest(error);

    store.Definitions[def.Id] = def;
    return Results.Created($"/workflows/{def.Id}", def);
});

// Get a workflow definition by its ID (simple lookup)
app.MapGet("/workflows/{id}", (string id, WorkflowStore store) =>
    store.Definitions.TryGetValue(id, out var def)
        ? Results.Ok(def)
        : Results.NotFound("No workflow found with that ID.")
);

// List all workflow definitions (just dumps everything)
app.MapGet("/workflows", (WorkflowStore store) =>
    Results.Ok(store.Definitions.Values)
);

// Start a workflow instance (picks the initial state automatically)
app.MapPost("/workflows/{id}/instances", (string id, WorkflowStore store) =>
{
    if (!store.Definitions.TryGetValue(id, out var def))
        return Results.NotFound("No workflow with that ID.");

    // Only create if there's a valid enabled initial state
    var initialState = def.States.FirstOrDefault(s => s.IsInitial && s.Enabled);
    if (initialState == null)
        return Results.BadRequest("Workflow doesn't have an enabled initial state.");

    // Create instance and add to store
    var instance = new WorkflowInstance
    {
        Id = Guid.NewGuid().ToString(),
        WorkflowDefinitionId = id,
        CurrentState = initialState.Id,
    };
    store.Instances[instance.Id] = instance;
    return Results.Created($"/instances/{instance.Id}", instance);
});

// Get current state and history of an instance (nice for debugging)
app.MapGet("/instances/{id}", (string id, WorkflowStore store) =>
    store.Instances.TryGetValue(id, out var inst)
        ? Results.Ok(inst)
        : Results.NotFound("No instance found with that ID.")
);

// List all workflow instances (useful for admins/testing)
app.MapGet("/instances", (WorkflowStore store) =>
    Results.Ok(store.Instances.Values)
);

// Transition instance to a new state (if allowed by action)
// This is where most bugs can happen if not careful, so I added step-by-step checks.
app.MapPost("/instances/{instanceId}/actions/{actionId}", (string instanceId, string actionId, WorkflowStore store) =>
{
    // Step 1: Check instance exists
    if (!store.Instances.TryGetValue(instanceId, out var inst))
        return Results.NotFound("No instance with that ID.");

    // Step 2: Find the workflow definition for this instance
    if (!store.Definitions.TryGetValue(inst.WorkflowDefinitionId, out var def))
        return Results.NotFound("No workflow definition found for this instance.");

    // Step 3: Get current state object (not just the ID)
    var currState = def.States.FirstOrDefault(s => s.Id == inst.CurrentState);
    if (currState == null || !currState.Enabled)
        return Results.BadRequest("Instance is in an invalid or disabled state.");

    // Step 4: Don't allow actions from a final state
    if (currState.IsFinal)
        return Results.BadRequest("This instance is already in a final state. No more actions allowed.");

    // Step 5: Check the action exists and is enabled
    var action = def.Actions.FirstOrDefault(a => a.Id == actionId);
    if (action == null)
        return Results.BadRequest("Action not found in this workflow.");
    if (!action.Enabled)
        return Results.BadRequest("This action is currently disabled.");
    if (!action.FromStates.Contains(inst.CurrentState))
        return Results.BadRequest("This action can't be performed from the current state.");

    // Step 6: Target state must exist and be enabled
    var toState = def.States.FirstOrDefault(s => s.Id == action.ToState && s.Enabled);
    if (toState == null)
        return Results.BadRequest("The target state for this action is invalid or disabled.");

    // Step 7: Perform the transition and record it in history
    var history = new ActionHistoryItem
    {
        ActionId = action.Id,
        Timestamp = DateTime.UtcNow,
        FromState = inst.CurrentState,
        ToState = toState.Id
    };
    inst.CurrentState = toState.Id;
    inst.History.Add(history);

    // Return updated instance (showing new state)
    return Results.Ok(inst);
});

// Actually start the API app
app.Run();