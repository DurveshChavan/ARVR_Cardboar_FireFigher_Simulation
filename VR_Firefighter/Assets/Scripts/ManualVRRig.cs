using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// ManualVRRig — Pure C# stereoscopic VR without any XR Plugin.
///
/// HIERARCHY REQUIRED:
///   PlayerRig          (CharacterController + movement scripts)
///   └─ Neck            (assign to 'neck' field — receives gamepad yaw/pitch)
///      └─ Head         (assign to 'head' field — receives gyroscope rotation)
///         ├─ LeftEye   (Camera — assign to 'leftEye' field)
///         └─ RightEye  (Camera — assign to 'rightEye' field)
///
/// COMPONENT SETUP:
///   1. Attach this script to PlayerRig.
///   2. Drag Neck, Head, LeftEye Camera, RightEye Camera into the inspector fields.
///   3. Set the LeftEye and RightEye cameras to have no AudioListener (only one allowed).
///   4. Disable any other cameras in the scene, or ensure they have lower depth.
/// </summary>
public class ManualVRRig : MonoBehaviour
{
    [Header("Camera References")]
    [Tooltip("The left eye camera (will cover left half of screen).")]
    public Camera leftEye;

    [Tooltip("The right eye camera (will cover right half of screen).")]
    public Camera rightEye;

    [Header("Hierarchy References")]
    [Tooltip("The Neck object — gamepad yaw and pitch rotate this.")]
    public Transform neck;

    [Tooltip("The Head object (child of Neck) — gyroscope rotates this.")]
    public Transform head;

    [Header("IPD (Inter-Pupillary Distance)")]
    [Tooltip("Distance between eyes in meters. Default: 0.064 (64mm).")]
    public float ipd = 0.064f;

    [Header("Gyroscope Settings")]
    [Tooltip("Smoothing factor for gyroscope (0=no smoothing, 1=frozen). Recommended: 0.1–0.2.")]
    [Range(0f, 0.95f)]
    public float gyroSmoothFactor = 0.1f;

    [Header("Gamepad Look Settings")]
    [Tooltip("Horizontal look speed when using right joystick (degrees/sec).")]
    public float yawSpeed = 90f;

    [Tooltip("Vertical look speed when using right joystick (degrees/sec).")]
    public float pitchSpeed = 60f;

    [Tooltip("Maximum up/down look angle in degrees.")]
    [Range(10f, 85f)]
    public float maxPitch = 70f;

    // Internal state
    private bool _gyroAvailable;
    private Quaternion _gyroOffset = Quaternion.identity;   // Set on Recenter
    private Quaternion _smoothedGyroRot = Quaternion.identity;
    private float _neckYaw;
    private float _neckPitch;

    // -----------------------------------------------------------------------
    // Unity Lifecycle
    // -----------------------------------------------------------------------

    void Start()
    {
        SetupStereoRects();
        SetupIPD();
        InitGyroscope();
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        Screen.brightness = 1f;
    }

    void Update()
    {
        ReadGamepad();
        ReadGyroscope();
    }

    // -----------------------------------------------------------------------
    // Step 1: Stereoscopic Split-Screen
    // -----------------------------------------------------------------------

    void SetupStereoRects()
    {
        if (leftEye == null || rightEye == null)
        {
            Debug.LogError("[ManualVRRig] LeftEye or RightEye camera not assigned!");
            return;
        }

        // Left eye: left half of screen
        leftEye.rect = new Rect(0f, 0f, 0.5f, 1f);

        // Right eye: right half of screen
        rightEye.rect = new Rect(0.5f, 0f, 0.5f, 1f);

        // Match FOV and clipping planes
        rightEye.fieldOfView = leftEye.fieldOfView;
        rightEye.nearClipPlane = leftEye.nearClipPlane;
        rightEye.farClipPlane = leftEye.farClipPlane;

        // Match depth so both render
        rightEye.depth = leftEye.depth;

        Debug.Log("[ManualVRRig] Stereo rects set. Left=(0,0,0.5,1) Right=(0.5,0,0.5,1)");
    }

    // -----------------------------------------------------------------------
    // Step 2: IPD Offset
    // -----------------------------------------------------------------------

    void SetupIPD()
    {
        if (leftEye == null || rightEye == null) return;

        float halfIPD = ipd * 0.5f;

        // Position relative to Head object
        leftEye.transform.localPosition = new Vector3(-halfIPD, 0f, 0f);
        rightEye.transform.localPosition = new Vector3(halfIPD, 0f, 0f);

        Debug.Log($"[ManualVRRig] IPD set to {ipd * 1000f:F0}mm. L:{-halfIPD:F4}m R:+{halfIPD:F4}m");
    }

    // -----------------------------------------------------------------------
    // Step 3: Gyroscope Head Tracking
    // -----------------------------------------------------------------------

    void InitGyroscope()
    {
        _gyroAvailable = SystemInfo.supportsGyroscope;

        if (_gyroAvailable)
        {
            Input.gyro.enabled = true;
            _gyroOffset = Quaternion.identity;
            Debug.Log("[ManualVRRig] Gyroscope enabled.");
        }
        else
        {
            Debug.LogWarning("[ManualVRRig] No gyroscope detected — gamepad look only.");
        }
    }

    void ReadGyroscope()
    {
        if (!_gyroAvailable || head == null) return;

        // ─── CRITICAL COORDINATE CONVERSION ────────────────────────────────
        // Unity uses a left-handed coordinate system.
        // Android gyroscope reports attitude in a right-handed coordinate system.
        //
        // Raw:        (x,  y,  z,  w)
        // Converted:  (x,  y, -z, -w)   → flips handedness
        // Then rotate -90° on X         → maps "phone held upright facing forward"
        //                                  to "camera looking along Unity's +Z axis"
        // ───────────────────────────────────────────────────────────────────
        Quaternion rawAttitude = Input.gyro.attitude;
        Quaternion deviceRot = new Quaternion(rawAttitude.x, rawAttitude.y, -rawAttitude.z, -rawAttitude.w);
        Quaternion unityRot = Quaternion.Euler(-90f, 0f, 0f) * deviceRot;

        // Apply the recenter offset to zero out the starting rotation
        Quaternion recenteredRot = _gyroOffset * unityRot;

        // Smooth to reduce jitter
        _smoothedGyroRot = Quaternion.Slerp(_smoothedGyroRot, recenteredRot, 1f - gyroSmoothFactor);

        head.localRotation = _smoothedGyroRot;
    }

    /// <summary>Call this to recenter the gyroscope to the current phone orientation.</summary>
    public void RecenterGyro()
    {
        if (!_gyroAvailable) return;

        Quaternion rawAttitude = Input.gyro.attitude;
        Quaternion deviceRot = new Quaternion(rawAttitude.x, rawAttitude.y, -rawAttitude.z, -rawAttitude.w);
        Quaternion unityRot = Quaternion.Euler(-90f, 0f, 0f) * deviceRot;

        // Offset = inverse of current, so applying it zeroes out the base orientation
        _gyroOffset = Quaternion.Inverse(unityRot);
        Debug.Log("[ManualVRRig] Gyro recentered.");
    }

    // -----------------------------------------------------------------------
    // Step 4: Gamepad Right-Joystick Look (Neck rotation)
    // -----------------------------------------------------------------------

    void ReadGamepad()
    {
        if (neck == null) return;

        float h = 0f;
        float v = 0f;

#if ENABLE_INPUT_SYSTEM
        // ── New Input System path ──────────────────────────────────────────
        // Reads the right stick from the first connected Gamepad.
        // Note: In Input System, the Y axis is already inverted relative to
        //       "looking up" — positive Y = stick pushed up = look up (positive pitch).
        var gp = Gamepad.current;
        if (gp != null)
        {
            Vector2 stick = gp.rightStick.ReadValue();
            h = stick.x;
            v = stick.y;
        }
#else
        // ── Legacy Input Manager path ─────────────────────────────────────
        // Add these axes in Edit → Project Settings → Input Manager:
        //   Name: "RightStickHorizontal"  → Joystick Axis 4  (or 3 depending on controller)
        //   Name: "RightStickVertical"    → Joystick Axis 5  (or 4)
        // Xbox controller axis mapping on Android:
        //   Axis 3 = Right Stick X
        //   Axis 4 = Right Stick Y
        h = Input.GetAxis("RightStickHorizontal");
        v = Input.GetAxis("RightStickVertical");
#endif
        // Apply dead zone
        if (Mathf.Abs(h) < 0.12f) h = 0f;
        if (Mathf.Abs(v) < 0.12f) v = 0f;

        // Accumulate yaw and pitch on the Neck
        _neckYaw += h * yawSpeed * Time.deltaTime;
        _neckPitch -= v * pitchSpeed * Time.deltaTime;   // Subtract: stick up → look up → negative pitch in Unity
        _neckPitch = Mathf.Clamp(_neckPitch, -maxPitch, maxPitch);

        neck.localRotation = Quaternion.Euler(_neckPitch, _neckYaw, 0f);
    }

    // -----------------------------------------------------------------------
    // Editor validation
    // -----------------------------------------------------------------------

#if UNITY_EDITOR
    void OnValidate()
    {
        if (leftEye != null && rightEye != null)
        {
            // Keep IPD visually correct in editor
            float halfIPD = ipd * 0.5f;
            leftEye.transform.localPosition = new Vector3(-halfIPD, 0f, 0f);
            rightEye.transform.localPosition = new Vector3(halfIPD, 0f, 0f);
        }
    }
#endif
}
