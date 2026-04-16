# Troubleshooting Guide — VR Firefighter

---

## App crashes immediately on launch

**Symptom:** App shows splash screen then crashes to home screen.  
**Logcat error:** `CachedReader::OutOfBoundsError` or `SIGTRAP`

**Cause:** Corrupted scene serialization — missing script GUIDs or dangling XR loader references.

**Fix:**
1. In Unity Editor, open `NewRealSCE.unity`
2. Run `VR Firefighter → Remove Missing Scripts`
3. Run `VR Firefighter → Build Extinguisher Models + Wire Scripts`
4. `Ctrl+S` → rebuild APK

---

## View is upside down / floor appears at top

**Symptom:** In the headset, everything is flipped — sky at bottom, floor at top.

**Cause:** Phone auto-rotated to `LandscapeRight` instead of `LandscapeLeft`.

**Fix:** Already handled in code — `PermissionGranter.Awake()` locks `Screen.orientation = LandscapeLeft`. If still happening:
1. Ensure `PermissionGranter.cs` is attached to a scene GameObject
2. Check `ProjectSettings → Player → Orientation` is set to `Landscape Left`

---

## Gyroscope not working / head tracking is dead

**Symptom:** Moving the phone/head doesn't change the view direction.

**Cause 1:** `AttitudeSensor` not enabled.  
**Check:** Logcat for `[ManualVRRig] AttitudeSensor enabled` or `No AttitudeSensor found`.

**Cause 2:** Legacy `Input.gyro` being called — returns zeros when New Input System is active.  
**Already fixed:** `ManualVRRig` uses `AttitudeSensor.current` (New Input System).

**Cause 3:** Sensor not yet enumerated at `Start()`.  
**Already handled:** `RetryEnableAttitudeSensor()` coroutine checks again after 1 frame.

**Fix if still broken:**
```csharp
// Add to ManualVRRig.Start() as debug:
Debug.Log($"AttitudeSensor available: {AttitudeSensor.current != null}");
Debug.Log($"Supported: {SystemInfo.supportsGyroscope}");
```

---

## World is tilted / player starts looking at ceiling

**Symptom:** When app launches, view is stuck looking up or sideways.

**Cause:** Gyro wasn't recentered — initial phone orientation was captured as base.

**Fix:** The app auto-recenters after 1 second. If missing:
- Ensure `ManualVRRig.cs` calls `StartCoroutine(AutoRecenterAfterDelay(1.0f))` in `InitGyroscope()`

**Manual recenter:** If you add a button binding, call `ManualVRRig.RecenterGyro()`.

---

## Controller buttons not responding (selection screen)

**Symptom:** Pressing A or B does nothing on the lobby screen.

**Cause 1:** Startup delay guard (1 second + 10 frames) still active.  
**Fix:** Wait 2 seconds after app launches, then press A or B.

**Cause 2:** Bluetooth controller disconnected mid-session.  
**Fix:** Hold Xbox button to reconnect, wait for LED to stop flashing.

**Cause 3:** `Gamepad.current == null` — controller not recognized by New Input System.  
**Check:** Logcat for `ScenarioSelector` tag — if no output, Gamepad not found.

---

## Fire doesn't reduce when spraying

**Symptom:** Holding RT sprays particles but fire doesn't shrink.

**Cause 1:** Wrong extinguisher selected — suppression rate may be near-zero.  
**Fix:** Press Y to cycle to correct type. DCP for kitchen, CO2 for server room.

**Cause 2:** `FireController` not found by GameManager.  
**Check:** Logcat for `[GameManager] StartGame(x) — found 0 fire(s)` → fire GameObjects weren't active when `StartGame()` ran.

**Cause 3:** `allFireControllers` array is null (StartGame not called).  
**Fix:** Ensure `ScenarioSelector.StartScenario()` calls `gameManager.StartGame(scenario)`.

---

## Only one fire shrinks in Server Room (other two don't)

**Already fixed:** `ExtinguisherShooter.ApplyFireSuppression()` now iterates `GameManager.allFireControllers[]` and calls `ReduceFire()` on ALL of them.

If still happening, ensure you built with the latest `ExtinguisherSystem.cs`.

---

## "Return to Lobby" doesn't work after mission

**Symptom:** After win/fail, holding A does nothing.

**Cause 1:** `gameActive` is still `true` (mission outcome didn't clear it).  
**Check:** `GameManager.MissionComplete()` / `MissionFailed()` each set `gameActive = false`.

**Cause 2:** `scenarioSelector` reference is null in GameManager.  
**Fix:** `GameManager.Start()` calls `FindFirstObjectByType<ScenarioSelector>()`. Ensure `ScenarioSelector` is in the scene.

---

## Missing Scripts warning in Inspector

**Symptom:** Inspector shows `Missing Script (Script)` with a blank Script field.

**Cause:** Script GUID mismatch after files were moved or merged.

**Fix (in order):**
1. `VR Firefighter → Remove Missing Scripts` (removes null-GUID components)
2. `VR Firefighter → Build Extinguisher Models + Wire Scripts` (adds fresh components)

---

## Build fails — Gradle error

**Symptom:** `Build and Run` fails with Gradle/NDK error.

**Fix steps:**
1. Unity Hub → Installs → your Unity version → Add Modules → Android Build Support (tick all sub-options)
2. `Edit → Preferences → External Tools` — let Unity use internal NDK/JDK
3. Delete `VR_Firefighter/Library/` and rebuild

---

## Performance — frame rate drops on device

**Symptoms:** Stuttering, low FPS in headset.

**Fixes:**
- `Project Settings → Quality → set to Low or Medium on Android`
- Reduce `maxEmissionRate` on FireController (default 40 → try 20)
- Disable `Multithreaded Rendering` if there are shader issues (usually helps)
- Ensure `Vulkan` is NOT in the Graphics API list (use OpenGLES3 only)
