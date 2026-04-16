# Architecture — VR Firefighter Training Game

## Overview

The game is a single-scene Unity application with no scene loading between states. All game objects for both scenarios exist in the one scene (`NewRealSCE.unity`) and are toggled active/inactive via `ScenarioSelector` and `GameManager`.

---

## System Architecture Diagram

```mermaid
graph TD
    A[Android Device] -->|Gyroscope| B[AttitudeSensor]
    A -->|Bluetooth| C[Xbox Controller / Gamepad]

    B --> D[ManualVRRig]
    C --> D
    C --> E[CardboardPlayerController]
    C --> F[ExtinguisherSystem]

    D -->|rotates| G[Head Transform]
    D -->|renders| H[LeftEye Camera]
    D -->|renders| I[RightEye Camera]

    E -->|moves| J[PlayerRig / CharacterController]

    F -->|cycles| K[ExtinguisherEquipper]
    F -->|aims| L[ExtinguisherAimer]
    F -->|fires| M[ExtinguisherShooter]

    M -->|ReduceFire| N[FireController x1..3]
    N -->|NotifyFireExtinguished| O[GameManager]

    O -->|StartGame / Result| P[HUD Canvas]
    O -->|ShowLobby| Q[ScenarioSelector]

    Q -->|SetActive| R[Kitchen Root]
    Q -->|SetActive| S[Server Room Root]
    Q -->|SetActive| T[Selection Screen]
```

---

## VR Rendering Architecture

The app does **not** use any XR Plugin or Google Cardboard SDK. It uses a custom `ManualVRRig` to implement stereoscopic rendering.

```mermaid
graph LR
    Phone["📱 Phone Screen\n(Split horizontal)"]
    LE["LeftEye Camera\nViewport: 0,0 → 0.5,1"]
    RE["RightEye Camera\nViewport: 0.5,0 → 1,1"]
    Head["Head Transform\n(Gyro-driven rotation)"]
    Neck["Neck Transform\n(Gamepad-driven yaw/pitch)"]

    Phone --> LE
    Phone --> RE
    Head --> LE
    Head --> RE
    Neck --> Head
```

**Eye offset:** ±0.032m (64mm IPD) on the X axis, applied to each camera's local position.

**Gyroscope pipeline:**
```
AttitudeSensor.current.attitude.ReadValue()
    → flip handedness: (x, y, -z, -w)
    → apply Euler(90, 0, 0) [LandscapeLeft correction]
    → multiply by _gyroOffset [recenter]
    → Slerp smooth
    → Head.localRotation
```

---

## Game State Machine

```mermaid
stateDiagram-v2
    [*] --> Lobby : App Launch
    Lobby --> Kitchen : A button pressed
    Lobby --> ServerRoom : B button pressed
    Kitchen --> Playing : StartGame(Kitchen)
    ServerRoom --> Playing : StartGame(ServerRoom)
    Playing --> MissionComplete : All fires extinguished
    Playing --> MissionFailed : Timer expired OR wrong extinguisher
    MissionComplete --> Lobby : Hold A 1.5s
    MissionFailed --> Lobby : Hold A 1.5s
```

---

## Fire Suppression System

```mermaid
sequenceDiagram
    participant Player
    participant ExtinguisherShooter
    participant GameManager
    participant FireController
    participant HUD

    Player->>ExtinguisherShooter: Hold RT (Right Trigger)
    ExtinguisherShooter->>ExtinguisherShooter: CheckCorrectExtinguisher()
    ExtinguisherShooter->>GameManager: Check scenario + extinguisher type
    GameManager-->>ExtinguisherShooter: suppressRate (0.04–0.25 per second)

    loop Every frame while RT held
        ExtinguisherShooter->>FireController: ReduceFire(suppressRate) [ALL fires]
        FireController->>FireController: fireScale -= rate * deltaTime
        FireController->>FireController: Update particle emission + color
    end

    FireController->>GameManager: NotifyFireExtinguished(this) [when fireScale ≤ 0.05]
    GameManager->>GameManager: Check if ALL fires are out
    GameManager->>HUD: ShowResult("MISSION COMPLETE", green)
    GameManager->>Player: BeginLobbyReturn()
```

---

## Component Dependency Map

```mermaid
graph LR
    GM[GameManager<br/>Singleton] --> SS[ScenarioSelector]
    GM --> FC1[FireController 1]
    GM --> FC2[FireController 2]
    GM --> FC3[FireController 3]

    SS --> KR[Kitchen Root]
    SS --> SR[Server Room Root]
    SS --> SEL[Selection Screen]

    EQ[ExtinguisherEquipper] --> EXT[Ext Models<br/>DCP / CO2 / Water]
    EA[ExtinguisherAimer] --> WHP[WeaponHoldPoint]
    ESH[ExtinguisherShooter] --> EA
    ESH --> EQ
    ESH --> SPR[Spray Particles<br/>DCP / CO2 / Water]
    ESH --> GM

    MVR[ManualVRRig] --> HEAD[Head Transform]
    MVR --> LEFT[LeftEye Camera]
    MVR --> RIGHT[RightEye Camera]
    MVR --> NECK[Neck Transform]
```

---

## Directory Structure

```
Assets/
├── Editor/                         ← Editor-only tools (not in build)
│   ├── ExtinguisherModelBuilder.cs ← Rebuild weapon hierarchy + wire scripts
│   ├── SetupManualVRRig.cs         ← Build stereo VR camera rig
│   ├── MissingScriptCleaner.cs     ← Remove null MonoBehaviour refs
│   ├── GameLogicWirer.cs           ← Wire HUD / GameManager refs
│   ├── KitchenBuilder.cs           ← Procedural kitchen environment
│   ├── ServerRoomBuilder.cs        ← Procedural server room environment
│   ├── HUDBuilder.cs               ← Build HUD canvas hierarchy
│   ├── MaterialFixer.cs            ← Fix URP material assignments
│   └── SceneBinaryIsolator.cs      ← Binary search corruption tool
│
├── Scripts/                        ← Runtime MonoBehaviours
│   ├── ManualVRRig.cs              ← Stereo render + gyro + gamepad look
│   ├── CardboardPlayerController.cs← Left-stick movement
│   ├── GameManager.cs              ← Singleton game state manager
│   ├── ExtinguisherSystem.cs       ← ExtinguisherEquipper + Aimer + Shooter
│   ├── FireController.cs           ← Per-fire particle + scale controller
│   ├── ScenarioSelector.cs         ← Lobby input + scenario activation
│   ├── PermissionGranter.cs        ← Android camera permission + orientation lock
│   ├── Billboard.cs                ← Always-face-camera utility
│   └── CardboardInitializer.cs     ← No-op stub (legacy, safe to keep)
│
├── Materials/                      ← URP Lit materials
├── Scenes/
│   └── NewRealSCE.unity            ← Single master scene
└── Plugins/Android/                ← AndroidManifest + Gradle overrides
```
