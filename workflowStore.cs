using System.Collections.Generic;
using WorkflowEngine.Models;
namespace WorkflowEngine
{
    public class WorkflowStore
    {
        public Dictionary<string, WorkflowDefinition>
        Definitions { get; } = new();
        public Dictionary<string, WorkflowInstance> 
        Instances { get; } = new();
    }
}

