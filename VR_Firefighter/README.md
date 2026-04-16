# VR Firefighter Training Game 🔥

> **A mobile VR fire extinguisher training simulation built for Google Cardboard + Android.**

Players choose between two fire scenarios, select the correct extinguisher type, and suppress the fire before a 60-second timer expires — teaching real-world fire safety decision-making in an immersive VR environment.

---

## 📱 Platform

| Target | Spec |
|---|---|
| **Device** | Android 8.0+ (API 26+) |
| **VR Viewer** | Google Cardboard |
| **Controller** | Bluetooth Xbox Controller |
| **Engine** | Unity 6.3 LTS |
| **Render Pipeline** | URP (Universal Render Pipeline) |

---

## 🎮 Gameplay

```
Launch App
    │
    ▼
[Selection Screen]
  ┌────────────────┬────────────────┐
  │  Kitchen Fire  │  Server Room   │
  │   Press A      │    Press B     │
  └────────────────┴────────────────┘
    │                    │
    ▼                    ▼
[Kitchen Scene]     [Server Room Scene]
 Gas fire (Class B)  Electrical fire (Class C)
 Correct: DCP        Correct: CO2
    │                    │
    └────────┬───────────┘
             ▼
      [Spray fire with RT]
      [60 second timer]
             │
      All fires out?
        Yes → MISSION COMPLETE
        No  → MISSION FAILED (time up or wrong extinguisher)
             │
             ▼
      [Hold A 1.5s → Return to Lobby]
```

### Controls (Xbox Bluetooth Controller)

| Button | Action |
|---|---|
| **Left Stick** | Move player |
| **Right Stick** | Look / rotate view |
| **Y** | Cycle extinguisher type (DCP → CO2 → Water) |
| **LT (Left Trigger)** | Aim extinguisher |
| **RT (Right Trigger)** | Fire / spray |
| **A** (selection screen) | Start Kitchen scenario |
| **B** (selection screen) | Start Server Room scenario |
| **Hold A** (result screen) | Return to lobby |

### Fire Safety Training Goals

| Scenario | Fire Class | Correct Extinguisher | Wrong Choice |
|---|---|---|---|
| Kitchen gas fire | Class B (flammable gas) | **DCP** (Dry Chemical Powder) | Water = ineffective |
| Server room electrical fire | Class C (electrical) | **CO2** | Water = instant fail (electrocution hazard) |

---

## 🏗️ Architecture

See [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) for full system diagrams.

### Scene Hierarchy (VR Rig)

```
PlayerRig (GameObject)
├── CharacterController
├── ManualVRRig            ← gyro head tracking + stereo rendering
├── CardboardPlayerController ← left-stick locomotion
├── ExtinguisherEquipper   ← Y button cycling
├── ExtinguisherAimer      ← LT aiming
└── ExtinguisherShooter    ← RT firing
    │
    └── Neck
        └── Head           ← gyroscope rotates this
            ├── LeftEye (Camera, Viewport 0–0.5) ← stereo left
            ├── RightEye (Camera, Viewport 0.5–1) ← stereo right
            └── WeaponHoldPoint
                ├── Ext_DCP   (active when DCP selected)
                ├── Ext_CO2   (active when CO2 selected)
                └── Ext_Water (active when Water selected)
```

---

## 🛠️ Setup & Build

See [`docs/BUILD_GUIDE.md`](docs/BUILD_GUIDE.md) for full instructions.

### Quick Start

1. Open `VR_Firefighter/` in **Unity 6.3 LTS**
2. Accept package imports (URP, Input System, TextMeshPro)
3. Open `Assets/Scenes/NewRealSCE.unity`
4. Run **`VR Firefighter → Build Extinguisher Models + Wire Scripts`**
5. Run **`VR Firefighter → Remove Missing Scripts`**
6. **File → Build Settings → Android → Build & Run**

### Requirements

- Unity 6.3 LTS with Android Build Support module
- Android SDK API 26+, NDK r23+
- JDK 11+
- Google Cardboard viewer
- Bluetooth Xbox controller

---

## 📁 Project Structure

```
VR_Firefighter/
├── Assets/
│   ├── Editor/           ← Unity Editor tools (build, wire, fix)
│   │   ├── ExtinguisherModelBuilder.cs
│   │   ├── SetupManualVRRig.cs
│   │   ├── MissingScriptCleaner.cs
│   │   └── ...
│   ├── Scripts/          ← Runtime game scripts
│   │   ├── ManualVRRig.cs
│   │   ├── GameManager.cs
│   │   ├── ExtinguisherSystem.cs
│   │   ├── FireController.cs
│   │   ├── ScenarioSelector.cs
│   │   └── CardboardPlayerController.cs
│   ├── Materials/        ← URP materials
│   ├── Scenes/
│   │   └── NewRealSCE.unity  ← main scene
│   └── Plugins/Android/  ← AndroidManifest, Gradle templates
├── ProjectSettings/
├── Packages/
└── docs/                 ← all documentation
```

---

## 📚 Documentation

| Doc | Description |
|---|---|
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | System architecture, component diagrams |
| [`docs/DESIGN_PATTERNS.md`](docs/DESIGN_PATTERNS.md) | Design patterns used in the codebase |
| [`docs/BUILD_GUIDE.md`](docs/BUILD_GUIDE.md) | Step-by-step build and deploy guide |
| [`docs/USER_GUIDE.md`](docs/USER_GUIDE.md) | Player instructions and gameplay guide |
| [`docs/API_REFERENCE.md`](docs/API_REFERENCE.md) | Script API reference |
| [`docs/TROUBLESHOOTING.md`](docs/TROUBLESHOOTING.md) | Common issues and fixes |

---

## 🔧 Editor Tools (`VR Firefighter` menu)

| Tool | Purpose |
|---|---|
| **Build Extinguisher Models + Wire Scripts** | Rebuilds weapon hierarchy, wires all script references |
| **Remove Missing Scripts** | Strips null/ghost MonoBehaviour components |
| **Setup Manual VR Rig** | Builds PlayerRig → Neck → Head stereo camera hierarchy |
| **Wire Entire Scene** | Wires all GameManager/HUD/Selector references |

---

## 👥 Team

Built as a 12-hour rapid prototype for fire safety VR training demonstration.

---

## 📄 License

MIT License — see `LICENSE` for details.
