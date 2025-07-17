# Infonetica – Configurable Workflow Engine (State-Machine API)

*Created by Shivam Rai for Infonetica Software Engineer Intern assignment, July 2025.*

This project is a minimal backend service for defining and executing configurable state-machine workflows, as described in the Infonetica assignment PDF.

---

## Quick Start Instructions

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

### How to Run

```sh
git clone https://github.com/whitedevil2829/infonetica_workflow-engine.git
cd infonetica_workflow-engine
dotnet run
After running, open http://localhost:5085/swagger
(Check your terminal for the actual port number.)

API Overview
This API lets you:
Define workflows as state machines (with states + actions).
Start workflow instances from definitions.
Move workflow instances between states using actions (with validation).
Inspect/list workflow definitions, states, actions, and running instances.

Endpoints
Endpoint	Method	Purpose
/workflows	POST	Create new workflow definition
/workflows/{id}	GET	Retrieve workflow definition
/workflows	GET	List all workflow definitions
/workflows/{id}/instances	POST	Start a new workflow instance
/instances/{id}	GET	Get state and history of an instance
/instances	GET	List all running instances
/instances/{instanceId}/actions/{actionId}	POST	Execute action (move instance)

Example Usage
1. Create Workflow Definition
Use POST /workflows (in Swagger UI).
body: json
2. Start Workflow Instance
Use POST /workflows/leave-approval/instances (no body needed).
Response will contain the new instance's ID.
3. Execute Action
Use POST /instances/{instanceId}/actions/approve
Replace {instanceId} with the actual instance ID you received above.
4. Inspect Instance State and History
Use GET /instances/{instanceId}

Assignment Requirements
Define: Create workflows with states and actions.
Start: Launch workflow instances from any definition.
Move: Execute actions to move an instance between states, with validation.
Inspect: List and retrieve states, actions, definitions, and running instances.
Validation: Blocks duplicates, missing initial state, wrong transitions, disabled states/actions, final states.
Persistence: Uses in-memory data store (no database, per assignment).
Documentation: Includes quick-start, API docs, payload examples, and design assumptions.

Assumptions & Shortcuts
All data is in-memory only (server restart resets everything).
Each workflow definition must have exactly one initial state.
All state and action IDs must be unique within a workflow.
Only enabled states and actions are used in transitions.
No authentication or user management is implemented.
Project can be extended to use persistent storage (database/file) if needed.
