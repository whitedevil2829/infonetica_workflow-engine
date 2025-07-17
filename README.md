# Infonetica Configurable Workflow Engine – Assignment Submission
*By Shivam Rai*
---
## About this Project

This is my submission for the Infonetica Software Engineer Intern assignment.  
The goal was to build a backend API service that lets you define and run state-machine based workflows (like approval flows) using simple HTTP endpoints.  
Everything is kept in-memory for simplicity, and you can easily try it out via Swagger UI.

---

## Getting Started (How to Run)

**Requirements:**  
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

**Steps:**  
1. Clone this repo:
    ```sh
    git clone https://github.com/whitedevil2829/infonetica_workflow-engine.git
    cd infonetica_workflow-engine
    ```
2. Run the API:
    ```sh
    dotnet run
    ```
3. Open your browser at  
   [http://localhost:5085/swagger](http://localhost:5085/swagger)  
   (the port might be different; check your terminal for the actual port)

Here you’ll find an interactive UI to test all features.

---

## What This API Does

- Lets you **define workflows** as state machines (add your own states and actions)
- Lets you **start workflow instances** from your definitions
- Lets you **move those instances between states** by executing actions
- Lets you **see all your definitions, states, actions, and current/running instances**

---

## API Endpoints

| Endpoint                                        | Method | What it Does                                       |
|-------------------------------------------------|--------|----------------------------------------------------|
| `/workflows`                                    | POST   | Add a new workflow definition                      |
| `/workflows/{id}`                               | GET    | Get a specific workflow definition                 |
| `/workflows`                                    | GET    | List all workflow definitions                      |
| `/workflows/{id}/instances`                     | POST   | Start a new workflow instance                      |
| `/instances/{id}`                               | GET    | See state and action history for an instance       |
| `/instances`                                    | GET    | List all running instances                         |
| `/instances/{instanceId}/actions/{actionId}`    | POST   | Execute an action (move instance to a new state)   |

---

## Example: How to Use (Step by Step)

1. **Create a Workflow**
    - Use `POST /workflows` with a body like:
      ```json
      {
        "id": "leave-approval",
        "name": "Leave Approval Workflow",
        "states": [
          { "id": "pending", "name": "Pending", "isInitial": true, "isFinal": false, "enabled": true },
          { "id": "approved", "name": "Approved", "isInitial": false, "isFinal": true, "enabled": true },
          { "id": "rejected", "name": "Rejected", "isInitial": false, "isFinal": true, "enabled": true }
        ],
        "actions": [
          { "id": "approve", "name": "Approve", "enabled": true, "fromStates": ["pending"], "toState": "approved" },
          { "id": "reject", "name": "Reject", "enabled": true, "fromStates": ["pending"], "toState": "rejected" }
        ]
      }
      ```

2. **Start an Instance**
    - Use `POST /workflows/leave-approval/instances` (no body needed).
    - Copy the `id` of the new instance from the response.

3. **Execute an Action**
    - Use `POST /instances/{instanceId}/actions/approve` to approve, or `/actions/reject` to reject.
    - The instance will move to the next state, if allowed.

4. **Check Instance State**
    - Use `GET /instances/{instanceId}` to see its current state and full action history.

---

## Validation Rules (What the API checks for you)

- **Each workflow must have exactly one initial state** (`isInitial: true`)
- **State and action IDs must be unique** within a workflow
- **All transitions are checked:**  
  - Actions can only be executed if enabled and valid from the current state
  - Target states for actions must exist and be enabled
  - You can’t move from a final state
  - Duplicate workflow definitions are blocked
- **Helpful error messages** are returned if you try something invalid

---
## How Everything is Stored
- **Everything is kept in memory** (RAM) – if you restart the server, all data is lost.  
- There’s no database or file storage (as required by the assignment), but the code is ready for it if needed in the future.
---
## Assumptions & Shortcuts
- Only backend, no UI except Swagger (for quick testing)
- No login/authentication needed for this assignment
- API is stateless, easy to reset—just restart the server
- Sample payloads and all main flows tested in Swagger UI
---

## Limitations
- Data is **not** persistent (lost on restart)
- No batch operations, only single actions per API call
- No async/database features (simple, as per requirements)
- No automated tests (but code is ready for them)
---

## Why This Meets the Assignment
- Follows all instructions from the PDF (define, start, move, list, validate)
- Exposes all required endpoints
- In-memory storage, no database
- Error handling, validation, and clear structure
- Well-commented code and clear documentation
---
