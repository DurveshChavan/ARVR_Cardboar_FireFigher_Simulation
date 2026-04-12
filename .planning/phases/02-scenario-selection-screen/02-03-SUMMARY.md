# Plan 02-03 Summary: Scene Wiring & State Verification

## Status: ⏳ Requires Unity Editor

## What was done:
This plan is **Unity Editor work** — it cannot be completed via file creation alone.

## What needs to be done in Unity Editor:

### 1. Create GameManager object
- Create empty root GameObject named "GameManager"
- Attach `GameManager.cs` component
- Attach `ScenarioSelector.cs` component

### 2. Wire ScenarioSelector references
In ScenarioSelector Inspector:
- `selectionScreen` → SelectionScreen canvas
- `kitchenRoot` → Kitchen object
- `serverRoomRoot` → ServerRoom object
- `gameManager` → GameManager component (same object)

### 3. Verify in Play mode
- SelectionScreen visible at start ✓
- Kitchen/ServerRoom inactive at start ✓
- A → Kitchen active, SelectionScreen hidden ✓
- B → ServerRoom active, SelectionScreen hidden ✓
- Console: "StartGame called with scenario: Kitchen/ServerRoom" ✓
- Zero errors, zero null references ✓

## Requirements addressed:
- SEL-01: SelectionScreen active at start, 3m in front
- SEL-04: A activates Kitchen, deactivates SelectionScreen
- SEL-05: B activates ServerRoom, deactivates SelectionScreen
- SEL-06: Kitchen and ServerRoom disabled at start
