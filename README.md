# 🔥 ARVR Cardboard Firefighter Simulation

> **A mobile VR fire extinguisher training game built for Google Cardboard + Android with Xbox controller input.**

Players are presented with a selection of two fire scenarios — **kitchen gas fire** or **server room electrical fire** — navigate the environment using a Bluetooth Xbox controller, select the correct extinguisher type, and suppress the fire before a **60-second timer** expires.

**Core Value:** Players correctly identify and apply the right fire extinguisher class to suppress a simulated fire before time runs out — teaching real-world fire safety decision-making in an immersive VR context.

---

## 📱 Platform

| | |
|---|---|
| **Device** | Android 8.0+ (Samsung A54 tested) |
| **VR Viewer** | Google Cardboard |
| **Controller** | Bluetooth Xbox Controller |
| **Engine** | Unity 6.3 LTS |
| **Input** | New Input System (`UnityEngine.InputSystem`) |

---

## 🎮 Scenarios

| Scenario | Fire Type | Correct Extinguisher | Wrong Choice |
|---|---|---|---|
| 🔥 Kitchen | Class B — gas fire | **DCP** (Dry Chemical Powder) | Water = ineffective |
| 🖥️ Server Room | Class C — electrical fire (3 racks) | **CO2** | Water = instant fail (electrocution) |

---

## 🕹️ Controls

| Button | Action |
|---|---|
| Left Stick | Move |
| Right Stick | Look |
| Y | Cycle extinguisher (DCP → CO2 → Water) |
| LT | Aim |
| RT | Spray |
| A (lobby) | Start Kitchen |
| B (lobby) | Start Server Room |
| Hold A (result) | Return to lobby |

---

## 📁 Repository Structure

```
AR_SCE/
├── VR_Firefighter/          ← Unity project root
│   ├── Assets/
│   │   ├── Scripts/         ← Runtime C# scripts
│   │   ├── Editor/          ← Unity Editor tools
│   │   ├── Scenes/          ← NewRealSCE.unity
│   │   ├── Materials/
│   │   └── Plugins/Android/
│   ├── ProjectSettings/
│   ├── Packages/
│   ├── README.md            ← Detailed project README
│   └── docs/                ← Full documentation
│       ├── ARCHITECTURE.md
│       ├── DESIGN_PATTERNS.md
│       ├── BUILD_GUIDE.md
│       ├── API_REFERENCE.md
│       ├── USER_GUIDE.md
│       └── TROUBLESHOOTING.md
└── .planning/               ← GSD workflow planning files
```

---

## 🚀 Quick Start

1. Open `VR_Firefighter/` in **Unity 6.3 LTS**
2. Open scene `Assets/Scenes/NewRealSCE.unity`
3. Run `VR Firefighter → Remove Missing Scripts`
4. Run `VR Firefighter → Build Extinguisher Models + Wire Scripts`
5. `Ctrl+S` → **Build Settings → Android → Build & Run**

See [`VR_Firefighter/docs/BUILD_GUIDE.md`](VR_Firefighter/docs/BUILD_GUIDE.md) for full setup instructions.

---

## 📚 Documentation

| Doc | Link |
|---|---|
| Architecture & Diagrams | [`docs/ARCHITECTURE.md`](VR_Firefighter/docs/ARCHITECTURE.md) |
| Design Patterns | [`docs/DESIGN_PATTERNS.md`](VR_Firefighter/docs/DESIGN_PATTERNS.md) |
| Build & Deploy | [`docs/BUILD_GUIDE.md`](VR_Firefighter/docs/BUILD_GUIDE.md) |
| Script API Reference | [`docs/API_REFERENCE.md`](VR_Firefighter/docs/API_REFERENCE.md) |
| User / Player Guide | [`docs/USER_GUIDE.md`](VR_Firefighter/docs/USER_GUIDE.md) |
| Troubleshooting | [`docs/TROUBLESHOOTING.md`](VR_Firefighter/docs/TROUBLESHOOTING.md) |

---

## 🛠️ Tech Stack

- **Unity 6.3 LTS** — Game engine
- **URP** — Universal Render Pipeline
- **Google Cardboard** — VR viewer (no XR Plugin — custom `ManualVRRig`)
- **New Input System** — Xbox controller + `AttitudeSensor` gyroscope
- **IL2CPP** — Android scripting backend
- **TextMeshPro** — HUD text rendering

---

## ⚠️ Constraints

- Input API: `UnityEngine.InputSystem` only — Old Input Manager disabled
- Every input script checks `if (Gamepad.current == null) return;`
- Gyroscope uses `AttitudeSensor.current` (not legacy `Input.gyro`)
- Screen locked to `LandscapeLeft` for Cardboard viewer compatibility

---

*Built in Unity 6.3 LTS — Google Cardboard XR Plugin NOT used (custom stereo rendering via ManualVRRig.cs)*
