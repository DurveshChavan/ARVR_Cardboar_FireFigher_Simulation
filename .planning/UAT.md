# User Acceptance Testing (UAT)

**Status:** In Progress
**Scope:** Phases 1–6 (Core Gameplay Loop)

We are running conversational verification to ensure the game functions perfectly in the Unity Editor before you build and install the APK (Phase 7).

## Test Cases

### Test 1: Navigation & Scenario Selection
- **Status:** Pending
- **Goal:** Verify that the player can look/move, and that the Controller correctly triggers scenario activation.
- **Criteria:** Headset/VR emulation isn't required in Editor for the basic logic, but the left stick should move the rig. Pressing 'A' should disable the Selection Screen and spawn the Kitchen. Pressing 'B' should spawn the Server Room instead.

### Test 2: Input Logic (Extinguisher Swapping)
- **Status:** Pending
- **Goal:** Verify the `ExtinguisherController` works.
- **Criteria:** Press 'Y' (Gamepad North). The ExtinguisherText UI element should appear for 2 seconds declaring DCP -> CO2 -> Water.

### Test 3: Suppression & HUD (Kitchen Scenario)
- **Status:** Pending
- **Goal:** Validate basic fire logic and HUD scaling.
- **Criteria:** After selecting Kitchen, TimerText should count down from 60s. Aiming (LT) and Spraying (RT) with the correct extinguisher (DCP) should shrink the fire to 0 relative size.

### Test 4: Extinguisher Interactions (Server Room Scenario)
- **Status:** Pending
- **Goal:** Validate specific hazard consequences.
- **Criteria:** Start Server Room. Cycle to the "Water" extinguisher. Aim (LT) and Spray (RT). The game should immediately trigger an "Instant Fail" with an electrocution warning on the screen.
