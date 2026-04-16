# Phase 05: Game Logic Scripts - Context

**Gathered:** 2026-04-13
**Status:** Ready for planning

<domain>
## Phase Boundary

Implement the core logic scripts tracking game state, timer, extinction interactions, extinguisher swapping, and spraying mechanics using the Unity New Input System. Attach and configure these scripts in the main scene.
</domain>

<decisions>
## Implementation Decisions

### Scripts & Logic
- **GameManager.cs**: Tracks the timer (60s), selected scenario, and active extinguisher type. Handles win/loss conditions.
- **ExtinguisherController.cs**: Uses `Gamepad.current.buttonNorth` (Y button equivalent) to cycle the active extinguisher (DCP -> CO2 -> Water). It triggers a UI popup specifying the properties.
- **SprayController.cs**: Aiming (LT > 0.3) + Spraying (RT > 0.3) triggers suppression logic. Cross-references extinguisher type with fire scenario. Implements instant-fail logic for using Water on an electrical fire.
- **FireController.cs**: Exposes `ReduceFire(rate)`, scales visual model down as health depletes, triggering `MissionComplete` at <= 0.05f scale.

### Input Approach
- Must rely directly on `Gamepad.current` mapped properties (`leftTrigger`, `rightTrigger`, `buttonNorth`).
- No Old Input API (`Input.GetButton` forbidden).

### Scene Wiring
- `GameManager`: Attached to an empty "GameManager" GameObject in the root hierarchy.
- `PlayerRig`: Gets `ExtinguisherController` and `SprayController`.
- `FireParticles` (Kitchen & ServerRoom): Both get `FireController`.

</decisions>

<canonical_refs>
## Canonical References
No external references — requirements provided directly in the phase prompt.
</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- Scripts rely entirely on Unity + `TMPro` + `UnityEngine.InputSystem`.
</code_context>

<specifics>
## Specific Ideas
- The user provided exact C# scripts that need to be generated precisely as requested.
</specifics>

<deferred>
## Deferred Ideas
None.
</deferred>
