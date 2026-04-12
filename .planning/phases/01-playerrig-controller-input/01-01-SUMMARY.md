---
phase: 01-playerrig-controller-input
plan: 01
subsystem: player-input
tags: [input-system, character-controller, movement]
key-files:
  created:
    - VR_Firefighter/Assets/Scripts/PlayerMovement.cs
  modified: []
metrics:
  tasks: 2
  commits: 1
  files-changed: 1
---

# Plan 01 Summary: PlayerMovement.cs Script

## What Was Built

Created `PlayerMovement.cs` — the core movement script for the PlayerRig. Uses Unity's New Input System (`Gamepad.current`) to read Xbox controller left stick input and move the PlayerRig via CharacterController with gravity.

## Commits

| # | Hash | Description |
|---|------|-------------|
| 1 | cbd1a04 | feat(01-01): create PlayerMovement.cs with New Input System |

## Task Results

| Task | Status | Notes |
|------|--------|-------|
| 1. Create PlayerMovement.cs | ✓ Complete | Exact user-provided code, verbatim |
| 2. Document scene setup | ✓ Complete | Scene hierarchy instructions in Plan 02 |

## Key Verifications

- ✓ `Gamepad.current` usage confirmed (1 occurrence)
- ✓ No legacy Input API (`Input.GetAxis`, `Input.GetButton`) — 0 occurrences
- ✓ Null check `if (gp == null) return;` present
- ✓ `speed = 3f` default set
- ✓ Uses `UnityEngine.InputSystem` namespace

## Deviations

None — script matches user-provided code exactly.

## Self-Check: PASSED

All acceptance criteria met. Script ready for attachment to PlayerRig in Unity Editor.
