using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

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
        // New Input System path — AttitudeSensor replaces the legacy Input.gyro API.
        // activeInputHandler=1 (New Input System Only) means Input.gyro is dead on Android.
        _gyroAvailable = AttitudeSensor.current != null;

        if (_gyroAvailable)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
            _gyroOffset = Quaternion.identity;
            _smoothedGyroRot = Quaternion.identity;
            Debug.Log("[ManualVRRig] AttitudeSensor enabled.");
            // Auto-recenter after 1 second so the initial phone position = forward/level
            StartCoroutine(AutoRecenterAfterDelay(1.0f));
        }
        else
        {
            // Fallback — try enabling and check again after one frame
            StartCoroutine(RetryEnableAttitudeSensor());
            Debug.LogWarning("[ManualVRRig] AttitudeSensor not available at Start — will retry.");
        }
    }

    System.Collections.IEnumerator RetryEnableAttitudeSensor()
    {
        yield return null; // wait one frame for devices to enumerate
        if (AttitudeSensor.current != null)
        {
            InputSystem.EnableDevice(AttitudeSensor.current);
            _gyroAvailable = true;
            _gyroOffset = Quaternion.identity;
            Debug.Log("[ManualVRRig] AttitudeSensor enabled (retry success).");
            // Auto-recenter after sensor has settled
            StartCoroutine(AutoRecenterAfterDelay(1.0f));
        }
        else
        {
            Debug.LogWarning("[ManualVRRig] No AttitudeSensor found — gyro head tracking unavailable. Gamepad look only.");
        }
    }

    System.Collections.IEnumerator AutoRecenterAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RecenterGyro();
        Debug.Log("[ManualVRRig] Auto-recentered gyro after " + delay + "s");
    }

    void ReadGyroscope()
    {
        if (!_gyroAvailable || head == null) return;
        if (AttitudeSensor.current == null) return;

        // ─── COORDINATE CONVERSION (New Input System AttitudeSensor) ──────────
        // AttitudeSensor.attitude is in Android sensor space (right-handed).
        // Unity is left-handed. Same conversion as legacy Input.gyro.attitude:
        //   Flip Z and W to convert handedness, then rotate -90° on X to
        //   map "phone upright facing forward" → "Unity camera looking +Z".
        // ─────────────────────────────────────────────────────────────────────
        Quaternion raw = AttitudeSensor.current.attitude.ReadValue();
        Quaternion deviceRot  = new Quaternion(raw.x, raw.y, -raw.z, -raw.w);
        // +90° on X: maps LandscapeLeft AttitudeSensor space to Unity camera forward (+Z).
        // (Original code used -90° for legacy Input.gyro in portrait — this is the corrected
        // value for New Input System AttitudeSensor in LandscapeLeft orientation.)
        Quaternion unityRot   = Quaternion.Euler(90f, 0f, 0f) * deviceRot;

        // Apply the recenter offset to zero out the starting orientation
        Quaternion recentered = _gyroOffset * unityRot;

        // Smooth to reduce jitter
        _smoothedGyroRot = Quaternion.Slerp(_smoothedGyroRot, recentered, 1f - gyroSmoothFactor);

        head.localRotation = _smoothedGyroRot;
    }

    /// <summary>Call this to recenter the gyroscope to the current phone orientation.</summary>
    public void RecenterGyro()
    {
        if (!_gyroAvailable || AttitudeSensor.current == null) return;

        Quaternion raw      = AttitudeSensor.current.attitude.ReadValue();
        Quaternion deviceRot = new Quaternion(raw.x, raw.y, -raw.z, -raw.w);
        Quaternion unityRot  = Quaternion.Euler(90f, 0f, 0f) * deviceRot;

        // Offset = inverse of current → applying it zeroes out the base orientation
        _gyroOffset = Quaternion.Inverse(unityRot);
        Debug.Log("[ManualVRRig] Gyro recentered (AttitudeSensor).");
    }

    // -----------------------------------------------------------------------
    // Step 4: Gamepad Right-Joystick Look (Neck rotation)
    // -----------------------------------------------------------------------

    void ReadGamepad()
    {
        if (neck == null) return;

        float h = 0f;
        float v = 0f;

        // New Input System only (activeInputHandler=1)
        var gp = Gamepad.current;
        if (gp != null)
        {
            Vector2 stick = gp.rightStick.ReadValue();
            h = stick.x;
            v = stick.y;
        }
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
