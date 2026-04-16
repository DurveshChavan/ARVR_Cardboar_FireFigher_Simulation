# 🎞️ Presentation Outline — VR Firefighter Training Simulation
### Course: Multimedia AR/VR | Group Project

> **10 Slides:** 1 Title · 4 Members × 2 Slides · 1 Conclusion  
> *(Replace A, B, C, D with real names before presenting)*

---

---

## 🟦 SLIDE 1 — Title & Team

**Title:**
> # VR Firefighter Training Simulation
> ### An Immersive Google Cardboard VR Experience for Fire Safety Education

**Subtitle:** Multimedia AR/VR — Group Project

**Group Members:**

| Member | Name |
|---|---|
| A | *(Student A)* |
| B | *(Student B)* |
| C | *(Student C)* |
| D | *(Student D)* |

**Visual Suggestion:** Screenshot of the app running on phone inside Cardboard viewer

---

---

## 🟨 SLIDE 2 — Project Overview & Motivation *(Member A)*

**Title:** What We Built & Why

**Key Points:**
- Real-world fire safety training is often text-based or passive
- VR creates an active, memorable learning experience
- Players must *choose* the right extinguisher — reinforcing classification knowledge
- Two distinct fire scenarios covering different fire classes:
  - **Kitchen** → Class B (gas) → DCP extinguisher
  - **Server Room** → Class C (electrical) → CO2 extinguisher

**Core Learning Objective:**
> *"Apply the correct fire extinguisher before time runs out"*

**Visual Suggestion:** Side-by-side of Kitchen fire and Server Room fire scenes

---

## 🟨 SLIDE 3 — Technology Stack & Platform *(Member A)*

**Title:** Tools & Technologies

| Layer | Technology |
|---|---|
| Engine | Unity 6.3 LTS |
| Render Pipeline | URP (Universal Render Pipeline) |
| VR Platform | Google Cardboard (no XR Plugin — custom stereo renderer) |
| Input System | New Input System (`UnityEngine.InputSystem`) |
| Controller | Bluetooth Xbox Controller |
| Head Tracking | Android AttitudeSensor (gyroscope) |
| Platform | Android 8.0+ (IL2CPP, ARM64) |
| Language | C# (MonoBehaviour, Editor scripting) |

**Responsible for:** Project setup, Unity configuration, Android build pipeline, gyroscope integration

**Visual Suggestion:** Tech stack diagram / Unity logo + Android logo + Cardboard logo

---

---

## 🟩 SLIDE 4 — VR Rendering System *(Member B)*

**Title:** Custom Stereoscopic VR — No XR Plugin Needed

**Key Technical Points:**
- Built a custom `ManualVRRig.cs` — zero dependency on Google Cardboard SDK or XR Plugin
- Splits the screen into Left Eye (0–50%) and Right Eye (50–100%) viewports
- Eye separation: **64mm IPD** (standard Cardboard)
- Gyroscope pipeline:

```
AttitudeSensor → flip handedness → Euler(90,0,0) correction
              → recenter offset → Slerp smooth → Head rotation
```

- Auto-recenters 1 second after launch
- Right joystick controls neck rotation (independent of gyro)

**Responsible for:** ManualVRRig.cs, head tracking, coordinate conversion, stereo camera setup

**Visual Suggestion:** Diagram of PlayerRig → Neck → Head → LeftEye / RightEye hierarchy

---

## 🟩 SLIDE 5 — Player Movement & Controller System *(Member B)*

**Title:** Xbox Controller Integration (New Input System)

**Key Points:**
- Uses `UnityEngine.InputSystem.Gamepad.current` exclusively (no legacy Input Manager)
- Every input script guards: `if (Gamepad.current == null) return;`
- Left stick → `CharacterController` movement along head-forward projected horizontal plane
- Right stick → Neck yaw + pitch (independent of gyro head tilt)
- Bluetooth pairing: standard Android → Xbox Wireless Controller

**Controller Map:**

```
Left Stick  → Move       Right Stick → Look
Y           → Cycle Ext  LT          → Aim
RT          → Spray      A/B         → Select Scenario
Hold A      → Return to Lobby
```

**Responsible for:** CardboardPlayerController.cs, input system setup, controller mapping

**Visual Suggestion:** Xbox controller diagram with button annotations

---

---

## 🟥 SLIDE 6 — Game Logic & Scenario System *(Member C)*

**Title:** Game Manager, Timer & Mission Flow

**Key Points:**
- `GameManager` singleton controls all game state
- 60-second countdown timer — displayed on HUD
- Two scenarios activated/deactivated dynamically in a **single scene** (no scene loading)
- State machine:

```
Lobby → Scenario Selected → Playing → Win/Fail → Return to Lobby
```

- `ScenarioSelector` handles A/B button input with phantom-press guard (1s delay + 10 frames)
- Post-mission: hold A for 1.5s to return — progress shown as `[Hold A — X%]`

**Responsible for:** GameManager.cs, ScenarioSelector.cs, timer, win/fail logic

**Visual Suggestion:** State machine diagram (Lobby → Playing → Win/Fail → Lobby)

---

## 🟥 SLIDE 7 — Fire Suppression System *(Member C)*

**Title:** Multi-Fire System & Extinguisher Logic

**Key Points:**
- `FireController` per fire source — controls `fireScale` (1.0 → 0)
- `fireScale` drives: particle emission rate, color (orange→dark red), size
- Server Room has **3 independent FireSources** — ALL must be suppressed to win
- `ExtinguisherShooter` reduces ALL fires simultaneously when spraying
- Suppression rates teach correct extinguisher choice:

| Extinguisher | Kitchen (gas) | Server Room (electrical) |
|---|---|---|
| DCP | **0.25/s ✅** | 0.04/s ❌ |
| CO2 | 0.03/s ❌ | **0.25/s ✅** |
| Water | 0.01/s ❌ | ⚡ INSTANT FAIL |

**Responsible for:** FireController.cs, ExtinguisherShooter suppression logic, multi-fire win detection

**Visual Suggestion:** Before/after of server rack fire going from full → extinguished

---

---

## 🟪 SLIDE 8 — 3D Environments *(Member D)*

**Title:** Kitchen & Server Room Scene Design

**Kitchen Fire Scenario:**
- Commercial kitchen environment
- Gas hob with open Class B flame
- Fire positioned at counter level for natural aiming
- Props: counters, cabinets, appliances

**Server Room Scenario:**
- Data center server rack environment
- 3 server racks (`Rack_Left`, `Rack_Middle`, `Rack_Right`)
- Each rack has a `FireSource` with independent `FireController`
- Atmosphere: dark room, LED indicators, cable trays

**Materials Used:** URP Lit materials — `RackGrey`, `GreenLED`, `RedLED`, `LPGRed`, `CounterGrey`

**Responsible for:** Kitchen and Server Room GameObject hierarchy, materials, lighting

**Visual Suggestion:** Split screenshot — Kitchen fire (left) / Server Room (right)

---

## 🟪 SLIDE 9 — UI / HUD & Extinguisher System *(Member D)*

**Title:** HUD Design & Extinguisher Interaction

**HUD Elements (Canvas on LeftEye):**
- `TimerText` — countdown in seconds (green → urgent)
- `ExtinguisherText` — popup showing selected type (2s fade)
- `ResultText` — MISSION COMPLETE (green) / MISSION FAILED (red)

**Extinguisher System:**
- 3 models parented to `WeaponHoldPoint` under `Head`
  - `Ext_DCP` (red), `Ext_CO2` (grey), `Ext_Water` (blue)
- `ExtinguisherEquipper` — Y button cycles active model
- `ExtinguisherAimer` — LT animates idle ↔ aimed position (hip → center)
- `ExtinguisherShooter` — RT fires spray particles, checks scenario logic

**Responsible for:** HUD canvas, ExtinguisherSystem.cs (Equipper/Aimer/Shooter), particle effects

**Visual Suggestion:** In-game HUD screenshot showing timer + extinguisher in hand

---

---

## ⬛ SLIDE 10 — Conclusion & Learnings

**Title:** What We Achieved & What We Learned

**✅ Delivered Features:**
- Full stereoscopic VR rendering without SDK dependency
- Two complete fire training scenarios
- Gyroscope head tracking (Android AttitudeSensor)
- Bluetooth Xbox controller all inputs
- Multi-fire win condition (Server Room: 3 fires)
- Lobby → Game → Result → Lobby full loop
- Wrong extinguisher consequences (instant fail for water on electrical)

**💡 Key Learnings:**

| Area | Learning |
|---|---|
| VR | Custom stereo rendering without SDK — full control, more complexity |
| Input | New Input System vs legacy — AttitudeSensor vs Input.gyro |
| Unity | Idempotent Editor tools prevent scene corruption |
| Game Design | Player feedback (suppression rate) teaches correct choice |
| Android | IL2CPP, permissions, screen orientation locking |

**🔮 Future Improvements:**
- Additional fire scenarios (car fire, chemical lab)
- Smoke density + visibility effects
- Score/leaderboard system
- Spatial audio (fire crackling, CO2 hiss)
- QR-code Cardboard profile calibration

**Thank You!**
> *Questions?*

---

*Generated from: [`VR_Firefighter/docs/`](VR_Firefighter/docs/) — Full technical docs available in repo*
