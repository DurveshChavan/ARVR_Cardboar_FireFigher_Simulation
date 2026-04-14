---
gsd_state_version: 1.0
milestone: v1.0
milestone_name: milestone
status: Ready to plan
last_updated: "2026-04-12T21:14:21.470Z"
progress:
  total_phases: 7
  completed_phases: 3
  total_plans: 9
  completed_plans: 9
  percent: 100
---

# Project State: VR Firefighter Training Game

## Current Status

**Phase:** 4
**Milestone:** v1.0
**Last Action:** Roadmap created — 2026-04-13

## Project Reference

See: .planning/PROJECT.md (updated 2026-04-13)

**Core value:** Players correctly identify and apply the right fire extinguisher to suppress a simulated fire before time runs out.
**Current focus:** Phase 3 — Kitchen Environment

## Phase Tracker

| # | Phase | Status |
|---|-------|--------|
| 1 | PlayerRig & Controller Input | ○ Pending |
| 2 | Scenario Selection Screen | ○ Pending |
| 3 | Kitchen Environment | ○ Pending |
| 4 | Server Room Environment | ○ Pending |
| 5 | Game Logic Scripts | ○ Pending |
| 6 | HUD Canvas | ○ Pending |
| 7 | Android Build | ○ Pending |

## Critical Context for Every Agent

- **Unity project path:** `C:\AR_SCE\VR_Firefighter`
- **All C# scripts go in:** `Assets/Scripts/`
- **Engine:** Unity 6.3 LTS
- **Platform:** Android + Google Cardboard VIEWER, but using ManualVRRig (NO Cardboard XR Plugin — abandoned due to display subsystem crash on Unity 6)
- **VR Approach:** ManualVRRig.cs — stereo split-screen via Camera rects + Android gyroscope + gamepad fallback
- **Input system:** `UnityEngine.InputSystem` ONLY — `Gamepad.current` with null check
- **FORBIDDEN:** `Input.GetAxis()`, `Input.GetButton()`, `Input.GetButtonDown()`  *(exception: `Input.gyro` legacy API is allowed — only way to access gyroscope)*
- **Scene structure:** Single scene, objects toggled active/inactive
- **HUD:** World Space Canvas, child of Head (or LeftEye) camera object
- **Deadline:** 12-hour build — no scope creep

## Quick Tasks Completed

| # | Description | Date | Commit | Directory |
|---|-------------|------|--------|-----------|
| 260415-0m1 | Replace Cardboard XR Plugin with ManualVRRig | 2026-04-14 | 2bde5a3 | [260415-0m1](./quick/260415-0m1-replace-cardboard-xr-plugin-with-manualv/) |
| 260415-1is | Phase A: Fix camera height, weapon/HUD reparent, fire visuals, spray wiring | 2026-04-14 | e62b532 | [260415-1is](./quick/260415-1is-phase-a-fix-camera-height-reparent-weapo/) |

