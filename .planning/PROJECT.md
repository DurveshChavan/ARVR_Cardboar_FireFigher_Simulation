# VR Firefighter Training Game

## What This Is

A mobile VR fire extinguisher training game built for Google Cardboard + Android with Xbox controller input. Players are presented with a selection of two fire scenarios (kitchen gas fire or server room electrical fire), navigate the environment using a Bluetooth Xbox controller, select the correct extinguisher type, and suppress the fire before a 60-second timer expires. Built in Unity 6.3 LTS using the Google Cardboard XR Plugin and Unity's New Input System — phone gyroscope provides head-tracking automatically via Cardboard XR.

## Core Value

Players correctly identify and apply the right fire extinguisher class to suppress a simulated fire before time runs out — teaching real-world fire safety decision-making in an immersive VR context.

## Requirements

### Validated

(None yet — ship to validate)

### Active

- [ ] PlayerRig with CharacterController, left stick movement (speed 3f), gravity applied
- [ ] Xbox controller input via UnityEngine.InputSystem — no legacy Input API
- [ ] Cardboard XR head tracking (automatic — no camera rotation scripts)
- [ ] Scenario selection screen: floating world-space canvas, A=Kitchen, B=Server Room
- [ ] Kitchen environment: floor, walls, gas stove, LPG cylinder, particle fire, wall sign
- [ ] Server room environment: floor, walls, server racks, cable trays, UPS, spark fire, wall sign
- [ ] Extinguisher cycle via Y button: DCP (Red) → CO2 (Black) → Water (Blue) → loop
- [ ] Extinguisher popup display for 2 seconds on Y press
- [ ] Fire suppression: hold LT + RT with correct extinguisher — fire scale shrinks 1→0 over 4s
- [ ] Correct extinguisher: Kitchen=DCP, Server=CO2
- [ ] Wrong extinguisher: fire barely shrinks (scale -= 0.01 * deltaTime)
- [ ] Water on electrical fire: InstantFail() immediately — "DANGER — Electrocution hazard!"
- [ ] 60-second countdown timer shown in HUD TimerText
- [ ] MissionComplete() when fire scale <= 0.05
- [ ] MissionFailed("Time expired!") when timer hits 0
- [ ] HUD World Space Canvas (child of Main Camera): TimerText, ExtinguisherText, ResultText
- [ ] ResultText and ExtinguisherText hidden by default
- [ ] Android build: player settings, APK export

### Out of Scope

- Mouse/keyboard input — Xbox controller + gyroscope only
- Multiple simultaneous scenarios — sequential selection only
- Multiplayer — single-player training simulation
- Procedural environments — hand-crafted scene geometry
- Additional fire classes beyond C (gas) and E (electrical) — v1 covers exactly two scenarios
- Online leaderboards or score persistence — no backend
- Voice narration — text-based HUD feedback only

## Context

- **Engine**: Unity 6.3 LTS
- **Platform**: Android (Google Cardboard VR)
- **XR SDK**: Google Cardboard XR Plugin (handles head tracking via phone gyroscope automatically)
- **Input**: Unity New Input System (UnityEngine.InputSystem) — Xbox controller via Bluetooth
- **Input Rule**: NEVER use Input.GetAxis/GetButton/GetButtonDown — ALWAYS use `Gamepad.current` with null check
- **Scene**: Single scene, GameObjects toggled active/inactive (not async scene loading)
- **HUD**: World Space Canvas, child of Main Camera
- **Phases as specified**: 7 phases covering PlayerRig → scenario selection → environments → game logic → HUD → Android build
- **Deadline**: 12-hour build constraint — no scope creep, build exactly as specified

## Constraints

- **Tech Stack**: Unity 6.3 LTS, Google Cardboard XR Plugin, New Input System — non-negotiable
- **Input API**: Only `UnityEngine.InputSystem` and `Gamepad.current` — Old Input Manager disabled
- **Timeline**: 12-hour deadline — strict scope adherence required
- **Platform**: Android + Google Cardboard — no PC VR, no Quest
- **Dependencies**: Google Cardboard XR Plugin must be in Packages/manifest.json
- **Null Safety**: Every input script must check `if (Gamepad.current == null) return;` in Update()

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| New Input System (not legacy) | Unity 6 + Cardboard XR requires it; old API will not work | — Pending |
| Single scene, objects toggled | Avoids async loading complexity within 12hr deadline | — Pending |
| World Space HUD (child of camera) | Cardboard stereo split requires world-space canvas | — Pending |
| CharacterController for movement | Simple gravity + movement without Rigidbody complexity | — Pending |
| Fire scale shrink suppression | Visual, lightweight — no physics collision needed | — Pending |
| DCP=Kitchen, CO2=Server, Water=InstantFail | Matches real fire safety (Class C=gas, Class E=electrical) | — Pending |

## Evolution

This document evolves at phase transitions and milestone boundaries.

**After each phase transition** (via `/gsd-transition`):
1. Requirements invalidated? → Move to Out of Scope with reason
2. Requirements validated? → Move to Validated with phase reference
3. New requirements emerged? → Add to Active
4. Decisions to log? → Add to Key Decisions
5. "What This Is" still accurate? → Update if drifted

**After each milestone** (via `/gsd-complete-milestone`):
1. Full review of all sections
2. Core Value check — still the right priority?
3. Audit Out of Scope — reasons still valid?
4. Update Context with current state

---
*Last updated: 2026-04-13 after initialization*
