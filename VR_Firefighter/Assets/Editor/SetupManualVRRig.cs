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
///  10. Wires CardboardPlayerController.cameraTransform to Head
///  11. Wires ExtinguisherAimer + ExtinguisherShooter holdPosition to WeaponHoldPoint
///  12. Wires ExtinguisherShooter spray particle references
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
        // This moves WeaponHoldPoint, HUD_Canvas, etc. to be children of Head
        // so they are visible to both LeftEye and RightEye cameras
        if (childrenToReparent != null)
        {
            foreach (Transform child in childrenToReparent)
            {
                if (child == null) continue;
                Undo.SetTransformParent(child, headObj.transform, "Reparent " + child.name);
                Debug.Log($"[SetupManualVR] Migrated '{child.name}' from old Main Camera to Head.");
            }
        }

        // --- Also search for WeaponHoldPoint if it wasn't a child of Main Camera ---
        Transform weaponHold = headObj.transform.Find("WeaponHoldPoint");
        if (weaponHold == null)
        {
            // Search globally for WeaponHoldPoint
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "WeaponHoldPoint" && go.scene.isLoaded)
                {
                    Undo.SetTransformParent(go.transform, headObj.transform, "Reparent WeaponHoldPoint to Head");
                    weaponHold = go.transform;
                    Debug.Log("[SetupManualVR] Found WeaponHoldPoint elsewhere — reparented to Head.");
                    break;
                }
            }
        }

        // --- Also search for HUD_Canvas ---
        Transform hudCanvas = headObj.transform.Find("HUD_Canvas");
        if (hudCanvas == null)
        {
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (go.name == "HUD_Canvas" && go.scene.isLoaded)
                {
                    Undo.SetTransformParent(go.transform, headObj.transform, "Reparent HUD_Canvas to Head");
                    go.transform.localPosition = new Vector3(0f, 0f, 1.5f);
                    go.transform.localRotation = Quaternion.identity;
                    go.transform.localScale = new Vector3(0.0005f, 0.0005f, 0.0005f);
                    Debug.Log("[SetupManualVR] Found HUD_Canvas elsewhere — reparented to Head.");
                    break;
                }
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

            // Disable TrackedPoseDriver if present (no longer needed — ManualVRRig replaces it)
            var tpd = oldMainCam.GetComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
            if (tpd != null)
            {
                Undo.RecordObject(tpd, "Disable TrackedPoseDriver");
                tpd.enabled = false;
                Debug.Log("[SetupManualVR] Disabled TrackedPoseDriver on old Main Camera.");
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

        // --- Wire ManualVRRig references ---
        Undo.RecordObject(vrRig, "Wire ManualVRRig references");
        vrRig.leftEye = leftCam;
        vrRig.rightEye = rightCam;
        vrRig.neck = neckObj.transform;
        vrRig.head = headObj.transform;
        Debug.Log("[SetupManualVR] Wired all 4 references on ManualVRRig.");

        // --- Wire CardboardPlayerController.cameraTransform to Head ---
        CardboardPlayerController cpc = playerRig.GetComponent<CardboardPlayerController>();
        if (cpc != null)
        {
            SerializedObject cpcSO = new SerializedObject(cpc);
            SerializedProperty camProp = cpcSO.FindProperty("cameraTransform");
            if (camProp != null)
            {
                camProp.objectReferenceValue = headObj.transform;
                cpcSO.ApplyModifiedProperties();
                Debug.Log("[SetupManualVR] Wired CardboardPlayerController.cameraTransform → Head.");
            }
        }

        // --- Wire ExtinguisherAimer + ExtinguisherShooter holdPosition ---
        if (weaponHold != null)
        {
            ExtinguisherAimer aimer = playerRig.GetComponent<ExtinguisherAimer>();
            if (aimer != null)
            {
                SerializedObject aimerSO = new SerializedObject(aimer);
                SerializedProperty holdProp = aimerSO.FindProperty("holdPosition");
                if (holdProp != null)
                {
                    holdProp.objectReferenceValue = weaponHold;
                    aimerSO.ApplyModifiedProperties();
                    Debug.Log("[SetupManualVR] Wired ExtinguisherAimer.holdPosition → WeaponHoldPoint.");
                }
            }

            ExtinguisherShooter shooter = playerRig.GetComponent<ExtinguisherShooter>();
            if (shooter != null)
            {
                SerializedObject shooterSO = new SerializedObject(shooter);

                SerializedProperty shootHoldProp = shooterSO.FindProperty("holdPosition");
                if (shootHoldProp != null)
                {
                    shootHoldProp.objectReferenceValue = weaponHold;
                }

                // Wire aimer and equipper references
                ExtinguisherEquipper equipper = playerRig.GetComponent<ExtinguisherEquipper>();
                SerializedProperty aimerRef = shooterSO.FindProperty("aimer");
                if (aimerRef != null && aimer != null) aimerRef.objectReferenceValue = aimer;
                SerializedProperty equipperRef = shooterSO.FindProperty("equipper");
                if (equipperRef != null && equipper != null) equipperRef.objectReferenceValue = equipper;

                // Wire spray particle references — find DCP_Spray, CO2_Spray, Water_Spray in scene
                WireSprayParticle(shooterSO, "dcpSpray", "DCP_Spray");
                WireSprayParticle(shooterSO, "co2Spray", "CO2_Spray");
                WireSprayParticle(shooterSO, "waterSpray", "Water_Spray");

                shooterSO.ApplyModifiedProperties();
                Debug.Log("[SetupManualVR] Wired ExtinguisherShooter (holdPosition, aimer, equipper, sprays).");
            }

            // Wire ExtinguisherEquipper holdPosition
            ExtinguisherEquipper eq = playerRig.GetComponent<ExtinguisherEquipper>();
            if (eq != null)
            {
                SerializedObject eqSO = new SerializedObject(eq);
                SerializedProperty eqHoldProp = eqSO.FindProperty("holdPosition");
                if (eqHoldProp != null)
                {
                    eqHoldProp.objectReferenceValue = weaponHold;
                    eqSO.ApplyModifiedProperties();
                    Debug.Log("[SetupManualVR] Wired ExtinguisherEquipper.holdPosition → WeaponHoldPoint.");
                }
            }
        }

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
        Debug.Log("  PlayerRig (Y=0) → Neck (Y=1.7) → Head → LeftEye + RightEye");
        Debug.Log("  ManualVRRig component added and auto-configured.");
        Debug.Log("  WeaponHoldPoint + HUD_Canvas reparented to Head.");
        Debug.Log("  Old Main Camera disabled. LeftEye tagged MainCamera.");
        Debug.Log("  CardboardPlayerController.cameraTransform → Head.");
        Debug.Log("  ExtinguisherAimer/Shooter/Equipper → WeaponHoldPoint.");
        Debug.Log("  Spray particles wired (DCP_Spray, CO2_Spray, Water_Spray).");
        Debug.Log("═══════════════════════════════════════════════════════");

        EditorUtility.DisplayDialog(
            "ManualVR Rig Setup Complete",
            "Hierarchy created:\n\n" +
            "PlayerRig (Y=0)\n" +
            "  └─ Neck (Y=1.7 — gamepad look)\n" +
            "     └─ Head (gyro tracking)\n" +
            "        ├─ LeftEye [Camera, MainCamera]\n" +
            "        ├─ RightEye [Camera]\n" +
            "        ├─ WeaponHoldPoint (extinguishers)\n" +
            "        └─ HUD_Canvas\n\n" +
            "All references wired. Build APK and test!",
            "OK"
        );
    }

    static void WireSprayParticle(SerializedObject so, string propName, string objName)
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == objName && go.scene.isLoaded)
            {
                ParticleSystem ps = go.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    SerializedProperty prop = so.FindProperty(propName);
                    if (prop != null)
                    {
                        prop.objectReferenceValue = ps;
                        Debug.Log($"[SetupManualVR] Wired {propName} → {objName}");
                    }
                }
                return;
            }
        }
        Debug.LogWarning($"[SetupManualVR] {objName} not found in scene — spray will not work for this type.");
    }
}
