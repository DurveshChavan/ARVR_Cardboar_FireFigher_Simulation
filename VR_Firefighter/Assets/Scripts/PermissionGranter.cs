using UnityEngine;
using UnityEngine.Android;

public class PermissionGranter : MonoBehaviour
{
    void Awake()
    {
        // Lock orientation to LandscapeLeft FIRST — before any other script runs.
        // Cardboard VR requires LandscapeLeft (home button to the right / volume up).
        // Without this lock, Android auto-rotates to LandscapeRight and renders
        // the stereo view upside-down.
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        #if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Camera))
        {
            Permission.RequestUserPermission(Permission.Camera);
        }
        #endif
    }
}
