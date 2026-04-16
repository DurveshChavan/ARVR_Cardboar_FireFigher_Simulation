# Build & Deploy Guide — VR Firefighter

## Prerequisites

| Tool | Version | Notes |
|---|---|---|
| Unity | **6.3 LTS** | Must match exactly — other versions unsupported |
| Android Build Support | Included in Unity Hub | Install via Unity Hub → Installs → Add Modules |
| Android SDK | API 26+ (Android 8.0) | Auto-installed with Unity Android module |
| NDK | r23+ | Auto-installed with Unity Android module |
| JDK | 11 | Bundled with Unity |
| ADB | Any | Required only for `adb logcat` debugging |

---

## Step 1 — Open Project

1. Launch **Unity Hub**
2. Click **Open → Add project from disk**
3. Navigate to `VR_Firefighter/` folder (contains `Assets/`, `ProjectSettings/`)
4. Select the folder and open

> ⚠️ Open `VR_Firefighter/` — **not** the root `AR_SCE/` folder.

---

## Step 2 — Scene Setup (first time only)

Open `Assets/Scenes/NewRealSCE.unity`.

Run these Editor tools from the **`VR Firefighter`** menu bar (top of Unity):

```
1. VR Firefighter → Remove Missing Scripts
2. VR Firefighter → Build Extinguisher Models + Wire Scripts
3. File → Save   (Ctrl+S)
```

These tools are **idempotent** — safe to run multiple times.

---

## Step 3 — Build Settings

`File → Build Settings`:

| Setting | Value |
|---|---|
| Platform | Android |
| Architecture | ARM64 |
| Scripting Backend | IL2CPP |
| API Compatibility Level | .NET Standard 2.1 |
| Minimum API Level | Android 8.0 (API 26) |
| Target API Level | Automatic |
| Active Input Handling | Input System Package (New) |

> The scene `NewRealSCE` must be in the build list (index 0).

---

## Step 4 — Player Settings

`Edit → Project Settings → Player → Android tab`:

| Setting | Value |
|---|---|
| Company Name | your company |
| Product Name | VR Firefighter |
| Package Name | `com.yourcompany.vrfirefighter` |
| Orientation | Landscape Left (locked) |
| Multithreaded Rendering | ✅ Enabled |
| Graphics API | OpenGLES3 (Vulkan off) |

---

## Step 5 — Connect Device & Build

1. Enable **Developer Options** on phone:  
   Settings → About Phone → tap Build Number 7 times
2. Enable **USB Debugging** in Developer Options
3. Connect phone via USB cable
4. **Build Settings → Build and Run**

Unity will compile, sign with a debug key, and deploy to connected device.

---

## Step 6 — Wireless ADB (optional)

After initial USB connection:
```powershell
adb tcpip 5555
adb connect <phone-ip>:5555
```

Now unplug USB — builds deploy wirelessly.

---

## Debugging on Device

### View logs in real-time
```powershell
adb logcat -s Unity:V
```

### Filter for game logs only
```powershell
adb logcat -s Unity:V | Select-String "GameManager|ManualVRRig|FireController|Extinguisher"
```

### Check for crashes
```powershell
adb logcat -s AndroidRuntime:E | head -50
```

---

## Clean Build (when things break)

```
1. Close Unity
2. Delete VR_Firefighter/Library/ folder (Unity will regenerate)
3. Delete VR_Firefighter/Temp/ folder
4. Re-open Unity — it will reimport all assets (~5 min)
5. Repeat Step 2 (Run Editor tools)
6. Build & Run
```

---

## Common Build Errors

| Error | Cause | Fix |
|---|---|---|
| `CachedReader::OutOfBoundsError` | Corrupted scene serialization | Run `Remove Missing Scripts` + `Build Extinguisher Models` |
| `SIGTRAP` on Android launch | Missing XR loader reference | Check `EditorBuildSettings.asset` has no XR config objects |
| `Gradle build failed` | NDK/SDK mismatch | Unity Hub → Installs → Add Android Build Support |
| `Input.gyro returns zeros` | Old Input System disabled | ✅ Already fixed — uses `AttitudeSensor` (New Input System) |
| `Screen upside down` | Auto-rotation picked wrong orientation | ✅ Already fixed — locked to `LandscapeLeft` |
| Missing scripts in Inspector | GUID mismatch after file move | Run `Remove Missing Scripts` then `Build Extinguisher Models` |
