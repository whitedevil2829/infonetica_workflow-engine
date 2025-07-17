// WorkflowStore.cs
// Author: Shivam Rai   
// ROll NO. 23MF10038
// For: Infonetica Workflow Engine Assignment
// Description: Simple in-memory store for workflow definitions and running instances.

using System.Collections.Generic;
using WorkflowEngine.Models;

namespace WorkflowEngine
{
    /// <summary>
    /// This class just holds everything in memory—no database or files.
    /// Keeps things simple for the assignment!
    /// </summary>
    public class WorkflowStore
    {
        // All workflow definitions go here (keyed by workflow ID)
        public Dictionary<string, WorkflowDefinition> Definitions { get; } = new();

        // All running workflow instances go here (keyed by instance ID)
        public Dictionary<string, WorkflowInstance> Instances { get; } = new();
    }
}
