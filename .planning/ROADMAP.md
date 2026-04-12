# Roadmap: VR Firefighter Training Game

**Milestone:** v1.0 — Full training game, Android Cardboard build
**Granularity:** Coarse | **Parallelization:** Enabled | **Mode:** YOLO

## Phase Summary

| # | Phase | Goal | Requirements | Plans |
|---|-------|------|--------------|-------|
| 1 | PlayerRig & Controller Input | 3/3 | Complete   | 2026-04-12 |
| 2 | Scenario Selection Screen | World-space canvas, A/B controller select environment | SEL-01–06 | 3 |
| 3 | Kitchen Environment | Kitchen geometry, LPG fire, wall sign, rack | KIT-01–05 | 3 |
| 4 | Server Room Environment | Server rack geometry, spark fire, wall sign, rack | SRV-01–04 | 3 |
| 5 | Game Logic Scripts | ExtinguisherController, SprayController, GameManager | EXT-01–03, SUP-01–06, GM-01–05 | 3 |
| 6 | HUD Canvas | TMP_Text timer, extinguisher popup, result display | HUD-01–04 | 2 |
| 7 | Android Build | Android Player Settings, export APK, device install | AND-01–03 | 3 |

## Phase Details

### Phase 1: PlayerRig & Controller Input

**Goal:** PlayerRig with CharacterController movement and verified Xbox controller input via New Input System

**Requirements:** RIG-01, RIG-02, RIG-03, RIG-04, RIG-05, RIG-06, RIG-07

**Plans:**
3/3 plans complete
2. PlayerMovement.cs — left stick X/Y movement, gravity, Gamepad.current null check, New Input System only
3. Input verification — confirm Gamepad.current detects Xbox controller, left stick moves rig correctly

**Success Criteria:**
1. PlayerRig exists with CharacterController and camera at eye height (1.6m)
2. Left stick moves PlayerRig forward/back/strafe at speed 3f with gravity applied
3. `if (Gamepad.current == null) return;` present in all input Update() loops
4. No calls to Input.GetAxis, Input.GetButton, or Input.GetButtonDown anywhere in codebase
5. Cardboard XR head tracking works — phone gyroscope rotates view without any camera script

**Depends on:** —

**UI hint:** no

---

### Phase 2: Scenario Selection Screen

**Goal:** World-space selection canvas with A/B controller buttons activating correct environments

**Requirements:** SEL-01, SEL-02, SEL-03, SEL-04, SEL-05, SEL-06

**Plans:**
1. SelectionScreen canvas — World Space Canvas 3m in front of start, KitchenCard and ServerCard quads with text
2. SelectionManager.cs — buttonSouth (A) activates Kitchen, buttonEast (B) activates ServerRoom, deactivates SelectionScreen
3. Scene state verification — Kitchen and ServerRoom disabled at start, only activate on correct button press

**Success Criteria:**
1. SelectionScreen World Space Canvas visible at game start, centered 3m ahead
2. Pressing A enables Kitchen, disables SelectionScreen
3. Pressing B enables ServerRoom, disables SelectionScreen
4. Kitchen and ServerRoom roots are disabled on Awake

**Depends on:** Phase 1

**UI hint:** no

---

### Phase 3: Kitchen Environment

**Goal:** Kitchen geometry, LPG cylinder fire particles, wall sign, and extinguisher rack — all built in scene

**Requirements:** KIT-01, KIT-02, KIT-03, KIT-04, KIT-05

**Plans:**
1. Kitchen geometry — floor, walls, counter, gas stove, LPG cylinder using Unity primitives
2. Fire particle system — orange/yellow Particle System on LPG cylinder, looping, auto-play
3. Extinguisher rack and wall sign — rack with extinguisher objects, TextMesh sign reading "GAS LEAK — Class C fire. Select the correct extinguisher."

**Success Criteria:**
1. Kitchen environment visible when activated by Phase 2 selection
2. Orange/yellow fire particles play on LPG cylinder
3. Wall sign is legible in VR (readable at 2–3m distance)
4. Extinguisher rack present with at least one extinguisher prop

**Depends on:** Phase 2

**UI hint:** no

---

### Phase 4: Server Room Environment

**Goal:** Server room geometry, blue-white spark fire particles, wall sign, and extinguisher rack — all built in scene

**Requirements:** SRV-01, SRV-02, SRV-03, SRV-04

**Plans:**
1. Server room geometry — floor, walls, server racks, cable trays, UPS unit using Unity primitives
2. Fire particle system — blue-white spark Particle System on middle server rack, looping, auto-play
3. Extinguisher rack and wall sign — rack with extinguisher objects, TextMesh sign reading "ELECTRICAL FIRE — DO NOT use water."

**Success Criteria:**
1. Server room environment visible when activated by Phase 2 selection
2. Blue-white spark fire particles play on middle server rack
3. Wall sign is legible in VR
4. Extinguisher rack present with at least one extinguisher prop

**Depends on:** Phase 2

**UI hint:** no

---

### Phase 5: Game Logic Scripts

**Goal:** ExtinguisherController, SprayController, and GameManager fully wired — fire suppression, instant fail, mission complete, and timer all functional

**Requirements:** EXT-01, EXT-02, EXT-03, SUP-01, SUP-02, SUP-03, SUP-04, SUP-05, SUP-06, GM-01, GM-02, GM-03, GM-04, GM-05

**Plans:**
1. ExtinguisherController.cs — Y button (buttonNorth) cycles DCP→CO2→Water→DCP, updates ExtinguisherText for 2s using Gamepad.current
2. SprayController.cs — LT+RT held simultaneously: correct extinguisher shrinks fire scale 1→0 over 4s; wrong extinguisher scale -= 0.01f*deltaTime; Water on electrical = InstantFail(); scale<=0.05 = MissionComplete()
3. GameManager.cs — orchestrates scenario state, 60s countdown timer, TimerText update, MissionComplete/MissionFailed/InstantFail methods, ResultText display

**Success Criteria:**
1. Y button cycles extinguisher and shows popup text for 2 seconds
2. Holding LT+RT with DCP on kitchen fire shrinks fire to zero over ~4 seconds → MissionComplete
3. Holding LT+RT with CO2 on server fire shrinks fire to zero over ~4 seconds → MissionComplete
4. Wrong extinguisher causes fire to barely shrink (scale -= 0.01f * Time.deltaTime)
5. Water on server room fire triggers InstantFail immediately with "DANGER — Electrocution hazard! Never use water on electrical fires."
6. Timer counts down from 60 and triggers MissionFailed("Time expired!") at 0
7. No legacy Input API used anywhere in project

**Depends on:** Phase 3, Phase 4

**UI hint:** no

---

### Phase 6: HUD Canvas

**Goal:** TMP_Text HUD elements (Timer, Extinguisher popup, Result) wired and rendering correctly in VR

**Requirements:** HUD-01, HUD-02, HUD-03, HUD-04

**Plans:**
1. HUD_Canvas setup — World Space Canvas as child of Main Camera, scale/distance tuned for Cardboard stereo
2. TMP_Text wiring — TimerText always visible, ExtinguisherText and ResultText hidden by default; all references assigned in GameManager and ExtinguisherController

**Success Criteria:**
1. HUD_Canvas is child of Main Camera and renders in both Cardboard eyes
2. TimerText shows countdown in real-time
3. ExtinguisherText appears on Y press for ~2 seconds then disappears
4. ResultText displays mission pass/fail message and is hidden otherwise

**Depends on:** Phase 5

**UI hint:** no

---

### Phase 7: Android Build

**Goal:** Android APK exported with correct Player Settings for Cardboard XR, installable on device

**Requirements:** AND-01, AND-02, AND-03

**Plans:**
1. Player Settings — Android: Minimum API 26, XR Plugin Management with Cardboard, IL2CPP scripting backend, ARM64, disable Auto Graphics API, set package name (com.arsce.vrfirefighter)
2. Build APK — File > Build Settings > Android > Build, resolve any errors, output APK to Builds/ folder
3. Device install — adb install APK on Android device, verify Cardboard stereo and Xbox controller input function

**Success Criteria:**
1. Android Player Settings correct: API 26+, Cardboard XR enabled, IL2CPP, ARM64
2. APK builds without errors
3. APK installs on Android device via ADB
4. Cardboard stereo rendering works on device
5. Xbox controller connects via Bluetooth and input is recognized in-game

**Depends on:** Phase 6

**UI hint:** no

---

## Requirement Coverage

| Requirement Group | Phase | Count |
|-------------------|-------|-------|
| RIG-01 to RIG-07 | 1 | 7 |
| SEL-01 to SEL-06 | 2 | 6 |
| KIT-01 to KIT-05 | 3 | 5 |
| SRV-01 to SRV-04 | 4 | 4 |
| EXT-01–03, SUP-01–06, GM-01–05 | 5 | 14 |
| HUD-01 to HUD-04 | 6 | 4 |
| AND-01 to AND-03 | 7 | 3 |

**Total v1 requirements:** 43 | **Mapped:** 43 | **Unmapped:** 0 ✓

---
*Created: 2026-04-13*
