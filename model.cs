// Models.cs
using System;
using System.Collections.Generic;

namespace WorkflowEngine.Models
{
    public class State
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsInitial { get; set; }
        public bool IsFinal { get; set; }
        public bool Enabled { get; set; } = true;
        public string Description { get; set; }
    }

    public class ActionDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public List<string> FromStates { get; set; } = new();
        public string ToState { get; set; }
        public string Description { get; set; }
    }

    public class WorkflowDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<State> States { get; set; } = new();
        public List<ActionDef> Actions { get; set; } = new();
    }

    public class ActionHistoryItem
    {
        public string ActionId { get; set; }
        public DateTime Timestamp { get; set; }
        public string FromState { get; set; }
        public string ToState { get; set; }
    }

    public class WorkflowInstance
    {
        public string Id { get; set; }
        public string WorkflowDefinitionId { get; set; }
        public string CurrentState { get; set; }
        public List<ActionHistoryItem> History { get; set; } = new();
    }
}

