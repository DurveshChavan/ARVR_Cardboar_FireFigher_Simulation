using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor window to automatically build the ManualVRRig hierarchy.
/// Menu: Tools → Setup ManualVR Rig
///
/// What it does:
///   1. Finds or creates PlayerRig with CharacterController
///   2. Creates Neck (child of PlayerRig) at (0, 1.7, 0)
///   3. Creates Head (child of Neck) at (0, 0, 0)
///   4. Creates LeftEye Camera (child of Head)
///   5. Creates RightEye Camera (child of Head), removes AudioListener
///   6. Reparents existing Main Camera children (HUD, WeaponHoldPoint, etc.) to Head
///   7. Disables the old Main Camera (or deletes it)
///   8. Adds ManualVRRig to PlayerRig and auto-wires all 4 references
///   9. Tags LeftEye as MainCamera
/// </summary>
public class SetupManualVRRig : EditorWindow
{
    [MenuItem("Tools/Setup ManualVR Rig")]
    public static void ShowWindow()
    {
        Execute();
    }

    static void Execute()
    {
        Undo.SetCurrentGroupName("Setup ManualVR Rig");
        int undoGroup = Undo.GetCurrentGroup();

        // --- Find or create PlayerRig ---
        GameObject playerRig = GameObject.Find("PlayerRig");
        if (playerRig == null)
        {
            playerRig = new GameObject("PlayerRig");
            Undo.RegisterCreatedObjectUndo(playerRig, "Create PlayerRig");
            playerRig.AddComponent<CharacterController>();
            Debug.Log("[SetupManualVR] Created new PlayerRig with CharacterController.");
        }
        else
        {
            Debug.Log("[SetupManualVR] Found existing PlayerRig.");
        }

        // --- Find existing Main Camera and its children ---
        Transform oldMainCam = playerRig.transform.Find("Main Camera");
        Transform[] childrenToReparent = null;

        if (oldMainCam != null)
        {
            // Collect children BEFORE we reparent them
            childrenToReparent = new Transform[oldMainCam.childCount];
            for (int i = 0; i < oldMainCam.childCount; i++)
            {
                childrenToReparent[i] = oldMainCam.GetChild(i);
            }
            Debug.Log($"[SetupManualVR] Found old Main Camera with {childrenToReparent.Length} children to migrate.");
        }

        // --- Create Neck ---
        Transform existingNeck = playerRig.transform.Find("Neck");
        GameObject neckObj;
        if (existingNeck != null)
        {
            neckObj = existingNeck.gameObject;
            Debug.Log("[SetupManualVR] Neck already exists, reusing.");
        }
        else
        {
            neckObj = new GameObject("Neck");
            Undo.RegisterCreatedObjectUndo(neckObj, "Create Neck");
            neckObj.transform.SetParent(playerRig.transform, false);
            neckObj.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            Debug.Log("[SetupManualVR] Created Neck at (0, 1.7, 0).");
        }

        // --- Create Head ---
        Transform existingHead = neckObj.transform.Find("Head");
        GameObject headObj;
        if (existingHead != null)
        {
            headObj = existingHead.gameObject;
            Debug.Log("[SetupManualVR] Head already exists, reusing.");
        }
        else
        {
            headObj = new GameObject("Head");
            Undo.RegisterCreatedObjectUndo(headObj, "Create Head");
            headObj.transform.SetParent(neckObj.transform, false);
            headObj.transform.localPosition = Vector3.zero;
            Debug.Log("[SetupManualVR] Created Head at (0, 0, 0).");
        }

        // --- Create LeftEye Camera ---
        Transform existingLeft = headObj.transform.Find("LeftEye");
        GameObject leftEyeObj;
        Camera leftCam;
        if (existingLeft != null)
        {
            leftEyeObj = existingLeft.gameObject;
            leftCam = leftEyeObj.GetComponent<Camera>();
            Debug.Log("[SetupManualVR] LeftEye already exists, reusing.");
        }
        else
        {
            leftEyeObj = new GameObject("LeftEye");
            Undo.RegisterCreatedObjectUndo(leftEyeObj, "Create LeftEye");
            leftEyeObj.transform.SetParent(headObj.transform, false);
            leftEyeObj.transform.localPosition = new Vector3(-0.032f, 0f, 0f);

            leftCam = leftEyeObj.AddComponent<Camera>();
            leftCam.nearClipPlane = 0.03f;
            leftCam.farClipPlane = 1000f;
            leftCam.fieldOfView = 60f;
            leftCam.rect = new Rect(0f, 0f, 0.5f, 1f);
            leftCam.depth = 0;

            // Add AudioListener to LeftEye (the "primary" camera)
            leftEyeObj.AddComponent<AudioListener>();

            leftEyeObj.tag = "MainCamera";
            Debug.Log("[SetupManualVR] Created LeftEye Camera (tagged MainCamera).");
        }

        // --- Create RightEye Camera ---
        Transform existingRight = headObj.transform.Find("RightEye");
        GameObject rightEyeObj;
        Camera rightCam;
        if (existingRight != null)
        {
            rightEyeObj = existingRight.gameObject;
            rightCam = rightEyeObj.GetComponent<Camera>();
            Debug.Log("[SetupManualVR] RightEye already exists, reusing.");
        }
        else
        {
            rightEyeObj = new GameObject("RightEye");
            Undo.RegisterCreatedObjectUndo(rightEyeObj, "Create RightEye");
            rightEyeObj.transform.SetParent(headObj.transform, false);
            rightEyeObj.transform.localPosition = new Vector3(0.032f, 0f, 0f);

            rightCam = rightEyeObj.AddComponent<Camera>();
            rightCam.nearClipPlane = 0.03f;
            rightCam.farClipPlane = 1000f;
            rightCam.fieldOfView = 60f;
            rightCam.rect = new Rect(0.5f, 0f, 0.5f, 1f);
            rightCam.depth = 0;

            // NO AudioListener on RightEye
            Debug.Log("[SetupManualVR] Created RightEye Camera (no AudioListener).");
        }

        // --- Reparent old Main Camera's children to Head ---
        if (childrenToReparent != null)
        {
            foreach (Transform child in childrenToReparent)
            {
                if (child == null) continue;
                Undo.SetTransformParent(child, headObj.transform, "Reparent " + child.name);
                Debug.Log($"[SetupManualVR] Migrated '{child.name}' from old Main Camera to Head.");
            }
        }

        // --- Disable old Main Camera ---
        if (oldMainCam != null)
        {
            Camera oldCamComponent = oldMainCam.GetComponent<Camera>();
            if (oldCamComponent != null)
            {
                Undo.RecordObject(oldCamComponent, "Disable old camera");
                oldCamComponent.enabled = false;
            }

            // Remove MainCamera tag from old camera
            Undo.RecordObject(oldMainCam.gameObject, "Untag old camera");
            oldMainCam.gameObject.tag = "Untagged";

            // Disable AudioListener on old camera
            AudioListener oldListener = oldMainCam.GetComponent<AudioListener>();
            if (oldListener != null)
            {
                Undo.RecordObject(oldListener, "Disable old AudioListener");
                oldListener.enabled = false;
            }

            Debug.Log("[SetupManualVR] Disabled old Main Camera (camera, tag, audio listener).");
        }

        // --- Add ManualVRRig if not present ---
        ManualVRRig vrRig = playerRig.GetComponent<ManualVRRig>();
        if (vrRig == null)
        {
            vrRig = Undo.AddComponent<ManualVRRig>(playerRig);
            Debug.Log("[SetupManualVR] Added ManualVRRig component to PlayerRig.");
        }

        // --- Wire the references ---
        Undo.RecordObject(vrRig, "Wire ManualVRRig references");
        vrRig.leftEye = leftCam;
        vrRig.rightEye = rightCam;
        vrRig.neck = neckObj.transform;
        vrRig.head = headObj.transform;
        Debug.Log("[SetupManualVR] Wired all 4 references on ManualVRRig.");

        // --- Update PlayerRig position (remove the 1.6 since Neck handles eye height) ---
        if (playerRig.transform.localPosition.y > 1.0f)
        {
            Undo.RecordObject(playerRig.transform, "Fix PlayerRig height");
            Vector3 pos = playerRig.transform.localPosition;
            pos.y = 0f;  // Ground level — Neck provides the 1.7 eye height
            playerRig.transform.localPosition = pos;
            Debug.Log("[SetupManualVR] Reset PlayerRig Y to 0 (Neck provides eye height).");
        }

        Undo.CollapseUndoOperations(undoGroup);

        EditorUtility.SetDirty(playerRig);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("═══════════════════════════════════════════════════════");
        Debug.Log("[SetupManualVR] ✓ COMPLETE! Hierarchy built and wired.");
        Debug.Log("  PlayerRig → Neck → Head → LeftEye + RightEye");
        Debug.Log("  ManualVRRig component added and auto-configured.");
        Debug.Log("  Old Main Camera disabled. LeftEye tagged MainCamera.");
        Debug.Log("═══════════════════════════════════════════════════════");

        EditorUtility.DisplayDialog(
            "ManualVR Rig Setup Complete",
            "Hierarchy created:\n\n" +
            "PlayerRig\n" +
            "  └─ Neck (gamepad look)\n" +
            "     └─ Head (gyro tracking)\n" +
            "        ├─ LeftEye [Camera, MainCamera]\n" +
            "        └─ RightEye [Camera]\n\n" +
            "ManualVRRig component wired with all 4 references.\n" +
            "Old Main Camera disabled.\n\n" +
            "Build your APK and test!",
            "OK"
        );
    }
}
