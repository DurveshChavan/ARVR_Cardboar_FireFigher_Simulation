# API Reference — VR Firefighter Scripts

Complete reference for all runtime MonoBehaviour scripts.

---

## GameManager

**File:** `Assets/Scripts/GameManager.cs`  
**Type:** Singleton MonoBehaviour  
**Attach to:** `GameManager` GameObject in scene root

### Public Fields

| Field | Type | Description |
|---|---|---|
| `Instance` | `GameManager` (static) | Global singleton reference |
| `currentScenario` | `Scenario` | Active scenario (Kitchen / ServerRoom) |
| `currentExtinguisher` | `ExtType` | Currently selected extinguisher type |
| `gameActive` | `bool` | True while timer is running |
| `timeLimit` | `float` | Seconds per round (default: 60) |
| `timerText` | `TMP_Text` | Timer HUD label |
| `extText` | `TMP_Text` | Extinguisher popup label |
| `resultText` | `TMP_Text` | Result screen label |
| `kitchenRoot` | `GameObject` | Root of kitchen scene objects |
| `serverRoomRoot` | `GameObject` | Root of server room objects |
| `scenarioSelector` | `ScenarioSelector` | Auto-found at Start() |
| `currentFireController` | `FireController` | First fire (single-fire fallback) |
| `allFireControllers` | `FireController[]` | All fires found at StartGame() |

### Enums

```csharp
public enum Scenario  { Kitchen, ServerRoom }
public enum ExtType   { DCP, CO2, Water }
```

### Public Methods

```csharp
// Start a scenario — finds all FireControllers, starts timer
void StartGame(Scenario scenario)

// Called by FireController when its fireScale ≤ 0.05
// Triggers MissionComplete() only when ALL fires are out
void NotifyFireExtinguished(FireController fire)

// Declare mission success — shows result, starts lobby return prompt
void MissionComplete()

// Declare mission failure with reason string
void MissionFailed(string reason)

// Shorthand for MissionFailed (e.g., wrong extinguisher)
void InstantFail(string reason)

// Show a 2-second popup over the extinguisher HUD slot
void ShowExtinguisherPopup(string msg)
```

---

## ManualVRRig

**File:** `Assets/Scripts/ManualVRRig.cs`  
**Type:** MonoBehaviour  
**Attach to:** `PlayerRig`

Implements stereoscopic VR rendering and gyroscope head tracking **without** any XR Plugin.

### Inspector Fields

| Field | Type | Description |
|---|---|---|
| `head` | `Transform` | Head transform (gyro drives this) |
| `neck` | `Transform` | Neck transform (gamepad yaw/pitch) |
| `leftEye` | `Camera` | Left eye camera (viewport 0–0.5) |
| `rightEye` | `Camera` | Right eye camera (viewport 0.5–1) |
| `eyeSeparation` | `float` | IPD in meters (default: 0.064) |
| `gyroSmoothFactor` | `float` | Slerp smoothing (0 = instant, 1 = never) |
| `lookSensitivity` | `float` | Right stick look speed |

### Public Methods

```csharp
// Resets the gyro offset to treat current phone orientation as "forward"
// Called automatically after 1 second at startup
void RecenterGyro()
```

### Gyroscope Coordinate Conversion

```
AttitudeSensor.attitude  →  flip Z and W (right→left handed)
                         →  Euler(90, 0, 0) [LandscapeLeft correction]
                         →  multiply by _gyroOffset [recenter]
                         →  Slerp smooth
                         →  Head.localRotation
```

---

## CardboardPlayerController

**File:** `Assets/Scripts/CardboardPlayerController.cs`  
**Type:** MonoBehaviour  
**Attach to:** `PlayerRig`

> Name is legacy — contains **no** Cardboard SDK dependencies.

### Inspector Fields

| Field | Type | Description |
|---|---|---|
| `cameraTransform` | `Transform` | Head transform for movement direction |
| `moveSpeed` | `float` | Walk speed m/s (default: 3) |

### Behaviour

Reads `Gamepad.current.leftStick` every frame. Moves `CharacterController` along the camera's forward/right axes projected onto the horizontal plane.

---

## ExtinguisherSystem (3 classes in one file)

**File:** `Assets/Scripts/ExtinguisherSystem.cs`

### ExtinguisherEquipper

Cycles between extinguisher models on **Y button** press.

| Field | Type | Description |
|---|---|---|
| `extinguisherModels` | `GameObject[]` | [0]=DCP, [1]=CO2, [2]=Water |
| `currentIndex` | `int` | Active extinguisher index |

```csharp
void CycleExtinguisher()  // called on Y press, activates model[currentIndex]
```

### ExtinguisherAimer

Animates the weapon between idle (hip) and aimed (center) position on **LT**.

| Field | Type | Description |
|---|---|---|
| `holdPosition` | `Transform` | WeaponHoldPoint parent |
| `idleLocalPosition` | `Vector3` | Hip carry offset |
| `idleLocalRotation` | `Vector3` | Hip carry rotation |
| `aimedLocalPosition` | `Vector3` | Aimed offset |
| `aimedLocalRotation` | `Vector3` | Aimed rotation |
| `aimSpeed` | `float` | Lerp speed (default: 8) |

### ExtinguisherShooter

Fires the extinguisher on **RT**. Handles wrong-extinguisher logic.

| Field | Type | Description |
|---|---|---|
| `aimer` | `ExtinguisherAimer` | Reference for must-be-aimed check |
| `equipper` | `ExtinguisherEquipper` | Reference for current type |
| `dcpSpray` | `ParticleSystem` | DCP spray particles |
| `co2Spray` | `ParticleSystem` | CO2 spray particles |
| `waterSpray` | `ParticleSystem` | Water spray particles |
| `holdPosition` | `Transform` | WeaponHoldPoint for recoil |
| `recoilDistance` | `float` | Recoil kickback distance |
| `recoilSpeed` | `float` | Recoil animation speed |

**Suppression rates** (fireScale per second):

| Scenario | DCP | CO2 | Water |
|---|---|---|---|
| Kitchen (gas) | **0.25** | 0.03 | 0.01 |
| Server Room (electrical) | 0.04 | **0.25** | ❌ InstantFail |

---

## FireController

**File:** `Assets/Scripts/FireController.cs`  
**Type:** MonoBehaviour  
**Attach to:** Each FireSource GameObject

### Fields

| Field | Type | Description |
|---|---|---|
| `fireScale` | `float` | Current fire intensity (1.0 = full, 0 = out) |
| `maxEmissionRate` | `float` | Particles/sec at full fire (default: 40) |
| `minEmissionRate` | `float` | Particles/sec near-extinguished (default: 3) |

### Public Methods

```csharp
// Called every frame by ExtinguisherShooter while spraying
void ReduceFire(float rate)  // rate = suppressRate * Time.deltaTime
```

### Internal Behaviour

Every frame:
1. `transform.localScale = Vector3.one * fireScale`
2. `emission.rateOverTime` lerped between min and max
3. `startColor` lerped orange → dark red
4. `startSizeMultiplier` lerped 1 → 0.1
5. When `fireScale ≤ 0.05`: particles stopped, `GameManager.NotifyFireExtinguished(this)` called

---

## ScenarioSelector

**File:** `Assets/Scripts/ScenarioSelector.cs`  
**Type:** MonoBehaviour  
**Attach to:** Any persistent scene object

### Fields

| Field | Type | Description |
|---|---|---|
| `selectionScreen` | `GameObject` | Selection UI root |
| `kitchenRoot` | `GameObject` | Kitchen scene root |
| `serverRoomRoot` | `GameObject` | Server room scene root |
| `gameManager` | `GameManager` | Auto-found at Start() |

### Public Methods

```csharp
// Called by GameManager.ReturnToLobby() — reactivates selection screen
void ShowLobby()
```

### Input Guards

- 1.0 second startup delay (prevents phantom Bluetooth button press at connect)
- 10 frame guard (prevents input on first frames)
- Both guards are reset to "already passed" when `ShowLobby()` is called

---

## PermissionGranter

**File:** `Assets/Scripts/PermissionGranter.cs`  
**Type:** MonoBehaviour  
**Attach to:** `PlayerRig` or any GameObject with Script Execution Order = earliest

Runs in `Awake()` before all `Start()` calls:
1. Locks `Screen.orientation = LandscapeLeft`
2. Requests Android camera permission (required for Cardboard viewer distortion)

---

## Billboard

**File:** `Assets/Scripts/Billboard.cs`  
**Type:** MonoBehaviour  
**Attach to:** Any GameObject that should always face the camera

Rotates the transform to face `Camera.main` every `LateUpdate()`. Used for fire warning signs and UI elements in world space.
