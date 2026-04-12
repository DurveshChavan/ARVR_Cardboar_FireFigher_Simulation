# Requirements: VR Firefighter Training Game

**Defined:** 2026-04-13
**Core Value:** Players correctly identify and apply the right fire extinguisher to suppress a simulated fire before time runs out — teaching real-world fire safety decision-making in VR.

## v1 Requirements

Requirements for the initial (and only) release. Each maps to roadmap phases.

### Player Rig & Input

- [ ] **RIG-01**: PlayerRig (empty GameObject) has a CharacterController component
- [ ] **RIG-02**: Main Camera is child of PlayerRig at local position (0, 1.6, 0)
- [ ] **RIG-03**: Left stick X/Y moves PlayerRig forward/back/strafe at speed 3f
- [ ] **RIG-04**: Gravity of -9.81f is applied to PlayerRig every Update via CharacterController
- [ ] **RIG-05**: All input uses `UnityEngine.InputSystem` and `Gamepad.current` — no legacy Input API
- [ ] **RIG-06**: Every input script checks `if (Gamepad.current == null) return;` at start of Update()
- [ ] **RIG-07**: Cardboard XR handles head tracking automatically — no MouseLook or FreeLook scripts

### Scenario Selection

- [ ] **SEL-01**: SelectionScreen World Space Canvas is active at game start, positioned 3m in front of PlayerRig
- [ ] **SEL-02**: KitchenCard displays "Kitchen Fire — Press A"
- [ ] **SEL-03**: ServerCard displays "Server Room — Press B"
- [ ] **SEL-04**: Pressing A (buttonSouth) activates Kitchen environment and deactivates SelectionScreen
- [ ] **SEL-05**: Pressing B (buttonEast) activates ServerRoom environment and deactivates SelectionScreen
- [ ] **SEL-06**: Kitchen and ServerRoom GameObjects are disabled at game start

### Kitchen Environment

- [ ] **KIT-01**: Kitchen environment has floor, walls, counter, and gas stove geometry
- [ ] **KIT-02**: LPG cylinder is present in kitchen
- [ ] **KIT-03**: Orange/yellow Particle System fire effect is on the LPG cylinder
- [ ] **KIT-04**: Extinguisher rack is present in kitchen with extinguisher objects
- [ ] **KIT-05**: Wall sign reads "GAS LEAK — Class C fire. Select the correct extinguisher."

### Server Room Environment

- [ ] **SRV-01**: Server room has floor, walls, server racks, cable trays, and UPS unit geometry
- [ ] **SRV-02**: Blue-white spark Particle System fire effect is on the middle server rack
- [ ] **SRV-03**: Extinguisher rack is present in server room with extinguisher objects
- [ ] **SRV-04**: Wall sign reads "ELECTRICAL FIRE — DO NOT use water."

### Extinguisher System

- [ ] **EXT-01**: Y button (buttonNorth) cycles extinguisher: DCP (Red) → CO2 (Black) → Water (Blue) → DCP → ...
- [ ] **EXT-02**: ExtinguisherText HUD element shows current extinguisher name for 2 seconds on Y press, then hides
- [ ] **EXT-03**: Default/starting extinguisher is DCP (Red)

### Fire Suppression Logic

- [ ] **SUP-01**: Holding LT (leftTrigger) + RT (rightTrigger) simultaneously with correct extinguisher shrinks fire localScale from 1→0 over ~4 seconds
- [ ] **SUP-02**: Kitchen correct extinguisher is DCP; Server Room correct extinguisher is CO2
- [ ] **SUP-03**: Wrong extinguisher (non-water, non-correct) causes fire to barely shrink: `scale -= 0.01f * Time.deltaTime`
- [ ] **SUP-04**: Water extinguisher on electrical fire triggers InstantFail() immediately on first spray
- [ ] **SUP-05**: InstantFail() displays "DANGER — Electrocution hazard! Never use water on electrical fires."
- [ ] **SUP-06**: When fire localScale <= 0.05, MissionComplete() is called

### Game Manager & Timer

- [ ] **GM-01**: GameManager script on empty GameManager GameObject orchestrates all game state
- [ ] **GM-02**: 60-second countdown timer starts when scenario is selected
- [ ] **GM-03**: TimerText TMP_Text shows remaining seconds every frame
- [ ] **GM-04**: When timer reaches 0, MissionFailed("Time expired!") is called
- [ ] **GM-05**: ResultText TMP_Text displays pass/fail result and is hidden by default

### HUD

- [ ] **HUD-01**: HUD_Canvas is a World Space Canvas, child of Main Camera
- [ ] **HUD-02**: TimerText (TMP_Text) is visible during gameplay
- [ ] **HUD-03**: ExtinguisherText (TMP_Text) is hidden by default, shown on Y press for 2 seconds
- [ ] **HUD-04**: ResultText (TMP_Text) is hidden by default, shown on MissionComplete or MissionFailed

### Android Build

- [ ] **AND-01**: Android Player Settings configured for Cardboard VR (XR Plugin, target SDK, etc.)
- [ ] **AND-02**: APK can be built from Unity Build Settings targeting Android
- [ ] **AND-03**: Build is installable on an Android device via ADB

## v2 Requirements

Deferred to future releases. Not in current roadmap.

### Audio

- **AUD-01**: Spray sound effect plays while RT is held
- **AUD-02**: Fire crackle ambient sound plays in each scenario
- **AUD-03**: Victory/failure audio cue on mission complete/fail

### Accessibility

- **ACC-01**: Timer pause menu accessible via Start button
- **ACC-02**: Colorblind-friendly extinguisher indicators (shape + label)

### Extended Content

- **EXT-CONT-01**: Additional scenario — electrical panel (Class E variant)
- **EXT-CONT-02**: Score/grade screen after mission complete
- **EXT-CONT-03**: Tutorial mode with guided prompts

## Out of Scope

Explicitly excluded to prevent scope creep within the 12-hour deadline.

| Feature | Reason |
|---------|--------|
| Mouse/keyboard input | Xbox controller + gyro only; adding KB/M increases test surface |
| Multiple simultaneous scenarios | Sequential selection covers the training flow |
| Multiplayer | Single-player training simulation |
| Online leaderboards / score persistence | No backend; out of time budget |
| Voice narration | Text HUD covers feedback; audio engineering is out of scope |
| Fire physics (real physics simulation) | Visual scale shrink is sufficient for training |
| Additional fire classes beyond C and E | v1 covers exactly two scenarios as specified |
| PC VR / Quest / SteamVR | Android + Cardboard only |
| Async scene loading | Single-scene, toggle active/inactive |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| RIG-01 to RIG-07 | Phase 1 | Pending |
| SEL-01 to SEL-06 | Phase 2 | Pending |
| KIT-01 to KIT-05 | Phase 3 | Pending |
| SRV-01 to SRV-04 | Phase 4 | Pending |
| EXT-01 to EXT-03, SUP-01 to SUP-06, GM-01 to GM-05 | Phase 5 | Pending |
| HUD-01 to HUD-04 | Phase 6 | Pending |
| AND-01 to AND-03 | Phase 7 | Pending |

**Coverage:**
- v1 requirements: 40 total
- Mapped to phases: 40
- Unmapped: 0 ✓

---
*Requirements defined: 2026-04-13*
*Last updated: 2026-04-13 after initial definition*
