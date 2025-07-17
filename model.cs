// Models.cs
// Author: Shivam RAi
// ROll NO. 23MF10038
// For: Infonetica Workflow Engine Assignment
// Description: Core model classes for representing workflow state machines,
//              actions, definitions, and instances.

using System;
using System.Collections.Generic;

namespace WorkflowEngine.Models
{
    /// <summary>
    /// Represents a single state in the workflow.
    /// E.g., "Pending", "Approved", "Rejected"
    /// </summary>
    public class State
    {
        public string Id { get; set; }      // Unique string, used for lookups (e.g., "pending")
        public string Name { get; set; }    // Human-readable label
        public bool IsInitial { get; set; } // Should this be the starting state?
        public bool IsFinal { get; set; }   // Is this an end-state? (no further transitions)
        public bool Enabled { get; set; } = true; // Can you use this state?
        public string Description { get; set; }   // Optional, for display or notes
    }

    /// <summary>
    /// Represents a transition/action you can take in a workflow.
    /// E.g., "approve", "reject"
    /// </summary>
    public class ActionDef
    {
        public string Id { get; set; }      // Unique string (e.g., "approve")
        public string Name { get; set; }    // Human-readable label
        public bool Enabled { get; set; } = true; // Is this action allowed right now?
        public List<string> FromStates { get; set; } = new(); // State IDs you can trigger from
        public string ToState { get; set; } // Target state ID after this action
        public string Description { get; set; } // Optional notes/explanation
    }

    /// <summary>
    /// A workflow definition bundles all states and actions for one process.
    /// Example: Leave approval flow (with all possible states and transitions)
    /// </summary>
    public class WorkflowDefinition
    {
        public string Id { get; set; }     // Unique ID for this workflow (e.g., "leave-approval")
        public string Name { get; set; }   // Human-readable name
        public List<State> States { get; set; } = new();      // All states this workflow uses
        public List<ActionDef> Actions { get; set; } = new(); // All possible transitions/actions
    }

    /// <summary>
    /// Keeps track of what happened to a workflow instance over time.
    /// Good for debugging/audit/history purposes.
    /// </summary>
    public class ActionHistoryItem
    {
        public string ActionId { get; set; }      // What action was executed
        public DateTime Timestamp { get; set; }   // When it was done
        public string FromState { get; set; }     // Where did it move from
        public string ToState { get; set; }       // Where did it move to
    }

    /// <summary>
    /// Represents one running instance of a workflow definition.
    /// Keeps current state and action history.
    /// </summary>
    public class WorkflowInstance
    {
        public string Id { get; set; }                   // Unique ID for this instance (Guid)
        public string WorkflowDefinitionId { get; set; } // Which workflow def is this instance using
        public string CurrentState { get; set; }         // Current state ID
        public List<ActionHistoryItem> History { get; set; } = new(); // What actions happened to it
    }
}
