using UnityEngine;
using UnityEditor;
using TMPro;
using System.Collections.Generic;

public class MasterSceneWirer
{
    [MenuItem("VR Firefighter/Wire Entire Scene")]
    public static void WireEntireScene()
    {
        List<string> log = new List<string>();
        log.Add("=== MasterSceneWirer — START ===");

        // ── STEP 1: Ensure GameManager object exists with correct scripts ──
        GameObject gmObj = FindAny("GameManager");
        if (gmObj == null)
        {
            gmObj = new GameObject("GameManager");
            gmObj.transform.position = Vector3.zero;
            log.Add("[CREATED] GameManager GameObject");
        }
        else log.Add("[FOUND] GameManager GameObject");

        EnsureComponent<GameManager>(gmObj, log);
        EnsureComponent<ScenarioSelector>(gmObj, log);

        // ── STEP 2: Wire ScenarioSelector references ──
        ScenarioSelector selector = gmObj.GetComponent<ScenarioSelector>();
        GameManager gm = gmObj.GetComponent<GameManager>();

        SerializedObject selectorSO = new SerializedObject(selector);

        GameObject selScreen = FindAny("SelectionScreen");
        WireRef(selectorSO, "selectionScreen", selScreen, "SelectionScreen", log);

        GameObject kitchen = FindAny("Kitchen");
        WireRef(selectorSO, "kitchenRoot", kitchen, "Kitchen", log);

        GameObject serverRoom = FindAny("ServerRoom");
        WireRef(selectorSO, "serverRoomRoot", serverRoom, "ServerRoom", log);

        SerializedProperty gmProp = selectorSO.FindProperty("gameManager");
        if (gmProp != null)
        {
            gmProp.objectReferenceValue = gm;
            log.Add("[WIRED] ScenarioSelector.gameManager → GameManager");
        }
        selectorSO.ApplyModifiedProperties();

        // ── STEP 3: Wire GameManager UI references ──
        SerializedObject gmSO = new SerializedObject(gm);

        WireTextRef(gmSO, "timerText", "TimerText", log);
        WireTextRef(gmSO, "extText", "ExtinguisherText", log);
        WireTextRef(gmSO, "resultText", "ResultText", log);

        // Wire scene references on GameManager
        if (kitchen != null)
        {
            SerializedProperty p = gmSO.FindProperty("kitchenRoot");
            if (p != null) p.objectReferenceValue = kitchen;
            log.Add("[WIRED] GameManager.kitchenRoot → Kitchen");
        }
        if (serverRoom != null)
        {
            SerializedProperty p = gmSO.FindProperty("serverRoomRoot");
            if (p != null) p.objectReferenceValue = serverRoom;
            log.Add("[WIRED] GameManager.serverRoomRoot → ServerRoom");
        }
        gmSO.ApplyModifiedProperties();

        // ── STEP 4: Wire PlayerRig with NEW scripts ──
        GameObject playerRig = FindAny("PlayerRig");
        if (playerRig != null)
        {
            Camera mainCam = Camera.main;

            // Ensure CardboardPlayerController (replaces old PlayerMovement)
            CardboardPlayerController cpc = EnsureComponent<CardboardPlayerController>(playerRig, log);
            if (mainCam != null)
            {
                SerializedObject cpcSO = new SerializedObject(cpc);
                SerializedProperty camProp = cpcSO.FindProperty("cameraTransform");
                if (camProp != null) camProp.objectReferenceValue = mainCam.transform;
                cpcSO.ApplyModifiedProperties();
            }

            // Ensure ExtinguisherEquipper
            ExtinguisherEquipper equipper = EnsureComponent<ExtinguisherEquipper>(playerRig, log);

            // Ensure ExtinguisherAimer
            ExtinguisherAimer aimer = EnsureComponent<ExtinguisherAimer>(playerRig, log);

            // Ensure ExtinguisherShooter
            ExtinguisherShooter shooter = EnsureComponent<ExtinguisherShooter>(playerRig, log);

            log.Add("[OK] PlayerRig scripts verified");

            // ── STEP 4.5: Wire extinguisher cross-references + sprays ──
            // Wire ExtinguisherShooter.aimer and .equipper
            SerializedObject shooterSO = new SerializedObject(shooter);
            SerializedProperty aimerRef = shooterSO.FindProperty("aimer");
            if (aimerRef != null) aimerRef.objectReferenceValue = aimer;
            SerializedProperty equipperRef = shooterSO.FindProperty("equipper");
            if (equipperRef != null) equipperRef.objectReferenceValue = equipper;

            // Find WeaponHoldPoint and wire holdPosition on all 3 scripts
            GameObject weaponHold = FindAny("WeaponHoldPoint");
            if (weaponHold != null)
            {
                // Aimer holdPosition
                SerializedObject aimerSO = new SerializedObject(aimer);
                SerializedProperty aimerHold = aimerSO.FindProperty("holdPosition");
                if (aimerHold != null) { aimerHold.objectReferenceValue = weaponHold.transform; aimerSO.ApplyModifiedProperties(); }

                // Shooter holdPosition
                SerializedProperty shooterHold = shooterSO.FindProperty("holdPosition");
                if (shooterHold != null) shooterHold.objectReferenceValue = weaponHold.transform;

                // Equipper holdPosition
                SerializedObject equipperSO = new SerializedObject(equipper);
                SerializedProperty equipperHold = equipperSO.FindProperty("holdPosition");
                if (equipperHold != null) { equipperHold.objectReferenceValue = weaponHold.transform; equipperSO.ApplyModifiedProperties(); }

                log.Add("[WIRED] holdPosition → WeaponHoldPoint on Aimer/Shooter/Equipper");
            }
            else log.Add("[MISSING] WeaponHoldPoint — extinguisher hold position not wired");

            // Wire spray particle references on ExtinguisherShooter
            WireSprayParticle(shooterSO, "dcpSpray", "DCP_Spray", log);
            WireSprayParticle(shooterSO, "co2Spray", "CO2_Spray", log);
            WireSprayParticle(shooterSO, "waterSpray", "Water_Spray", log);

            shooterSO.ApplyModifiedProperties();

            // Wire ExtinguisherEquipper model references
            WireExtinguisherModel(playerRig, equipper, log);
        }
        else log.Add("[MISSING] PlayerRig — create empty GameObject named PlayerRig");

        // ── STEP 5: Wire FireController on Kitchen fire ──
        GameObject lpg = FindAny("LPGCylinder");
        if (lpg != null)
        {
            ParticleSystem childPS = lpg.GetComponentInChildren<ParticleSystem>(true);
            if (childPS != null)
            {
                EnsureComponent<FireController>(childPS.gameObject, log);
                log.Add("[OK] FireController on Kitchen fire");
            }
            else
            {
                ParticleSystem lpgPS = lpg.GetComponent<ParticleSystem>();
                if (lpgPS != null)
                {
                    EnsureComponent<FireController>(lpg, log);
                    log.Add("[OK] FireController on LPGCylinder");
                }
                else log.Add("[MISSING] No ParticleSystem on LPGCylinder");
            }
        }
        else log.Add("[MISSING] LPGCylinder — run VR Firefighter > Build Kitchen Environment");

        // ── STEP 6: Wire FireController on ServerRoom fire ──
        GameObject fireSource = FindAny("FireSource");
        if (fireSource != null)
        {
            EnsureComponent<FireController>(fireSource, log);
            log.Add("[OK] FireController on ServerRoom fire");
        }
        else
        {
            GameObject rackMiddle = FindAny("Rack_Middle");
            if (rackMiddle != null)
            {
                ParticleSystem rackPS = rackMiddle.GetComponentInChildren<ParticleSystem>(true);
                if (rackPS != null)
                {
                    EnsureComponent<FireController>(rackPS.gameObject, log);
                    log.Add("[OK] FireController on ServerRoom fire (child of Rack_Middle)");
                }
                else log.Add("[MISSING] No ParticleSystem on Rack_Middle");
            }
            else log.Add("[MISSING] FireSource — run VR Firefighter > Build Server Room Environment");
        }

        // ── STEP 7: Set correct active/inactive states ──
        if (kitchen != null) { kitchen.SetActive(false); log.Add("[SET] Kitchen → inactive"); }
        if (serverRoom != null) { serverRoom.SetActive(false); log.Add("[SET] ServerRoom → inactive"); }
        if (selScreen != null) { selScreen.SetActive(true); log.Add("[SET] SelectionScreen → active"); }

        // ── STEP 8: Wire HUD to Main Camera ──
        GameObject hudCanvas = FindAny("HUD_Canvas");
        Camera mainCamera = Camera.main;
        if (hudCanvas != null && mainCamera != null)
        {
            if (hudCanvas.transform.parent != mainCamera.transform)
            {
                hudCanvas.transform.SetParent(mainCamera.transform, false);
                hudCanvas.transform.localPosition = new Vector3(0, 0, 1.5f);
                hudCanvas.transform.localRotation = Quaternion.identity;
                hudCanvas.transform.localScale = new Vector3(0.0005f, 0.0005f, 0.0005f);
                log.Add("[REPARENTED] HUD_Canvas → Main Camera child");
            }
            else log.Add("[OK] HUD_Canvas already child of Main Camera");
        }
        else
        {
            if (hudCanvas == null) log.Add("[MISSING] HUD_Canvas — run VR Firefighter > Build HUD Canvas");
        }

        // ── STEP 9: Invoke builders if objects missing ──
        if (kitchen == null)
        {
            try { EditorApplication.ExecuteMenuItem("VR Firefighter/Build Kitchen Environment"); log.Add("[RAN] Build Kitchen Environment"); }
            catch { log.Add("[SKIP] Kitchen builder not found"); }
        }
        if (serverRoom == null)
        {
            try { EditorApplication.ExecuteMenuItem("VR Firefighter/Build Server Room Environment"); log.Add("[RAN] Build Server Room Environment"); }
            catch { log.Add("[SKIP] Server Room builder not found"); }
        }
        if (hudCanvas == null)
        {
            try { EditorApplication.ExecuteMenuItem("VR Firefighter/Build HUD Canvas"); log.Add("[RAN] Build HUD Canvas"); }
            catch { log.Add("[SKIP] HUD builder not found"); }
        }

        // ── STEP 10: Delete rogue XR objects ──
        GameObject xrSim = FindAny("XR Device Simulator");
        if (xrSim != null) { Object.DestroyImmediate(xrSim); log.Add("[DELETED] XR Device Simulator"); }

        GameObject xrMgr = FindAny("XR Interaction Manager");
        if (xrMgr != null) { Object.DestroyImmediate(xrMgr); log.Add("[DELETED] XR Interaction Manager"); }

        // ── STEP 11: Summary ──
        log.Add("=== MasterSceneWirer — COMPLETE ===");
        log.Add("Scene wiring complete. Check Console for any MISSING items.");
        log.Add("NEXT: Run 'VR Firefighter > Build Extinguisher Models + Wire Scripts' to build the 3D guns.");

        foreach (string entry in log)
        {
            if (entry.Contains("[MISSING]"))
                Debug.LogWarning(entry);
            else
                Debug.Log(entry);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());
    }

    // ── Helpers ──
    static GameObject FindAny(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null) return obj;
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.name == name && go.scene.isLoaded) return go;
        return null;
    }

    static T EnsureComponent<T>(GameObject obj, List<string> log) where T : Component
    {
        T comp = obj.GetComponent<T>();
        if (comp == null)
        {
            comp = obj.AddComponent<T>();
            log.Add("[ADDED] " + typeof(T).Name + " to " + obj.name);
        }
        else log.Add("[EXISTS] " + typeof(T).Name + " on " + obj.name);
        return comp;
    }

    static void WireRef(SerializedObject so, string propName, GameObject target, string label, List<string> log)
    {
        SerializedProperty prop = so.FindProperty(propName);
        if (prop != null && target != null)
        {
            prop.objectReferenceValue = target;
            log.Add("[WIRED] " + propName + " → " + label);
        }
        else if (target == null)
            log.Add("[MISSING] " + label + " not found in scene");
    }

    static void WireTextRef(SerializedObject so, string propName, string objName, List<string> log)
    {
        GameObject obj = FindAny(objName);
        if (obj != null)
        {
            TMP_Text txt = obj.GetComponent<TMP_Text>();
            if (txt != null)
            {
                SerializedProperty p = so.FindProperty(propName);
                if (p != null) p.objectReferenceValue = txt;
                log.Add("[WIRED] " + propName + " → " + objName);
            }
        }
        else log.Add("[MISSING] " + objName + " — run VR Firefighter > Build HUD Canvas");
    }

    static void WireSprayParticle(SerializedObject so, string propName, string objName, List<string> log)
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
                        log.Add("[WIRED] " + propName + " → " + objName);
                    }
                }
                else log.Add("[MISSING] ParticleSystem on " + objName);
                return;
            }
        }
        log.Add("[MISSING] " + objName + " — spray will not work for this type");
    }

    static void WireExtinguisherModel(GameObject playerRig, ExtinguisherEquipper equipper, List<string> log)
    {
        SerializedObject eqSO = new SerializedObject(equipper);

        // Find extinguisher model GameObjects and wire them
        GameObject dcp = FindAny("DCP_Extinguisher");
        if (dcp == null) dcp = FindAny("Ext_DCP");
        if (dcp != null)
        {
            SerializedProperty p = eqSO.FindProperty("dcpExtinguisher");
            if (p != null) p.objectReferenceValue = dcp;
            log.Add("[WIRED] ExtinguisherEquipper.dcpExtinguisher → " + dcp.name);
        }

        GameObject co2 = FindAny("CO2_Extinguisher");
        if (co2 == null) co2 = FindAny("Ext_CO2");
        if (co2 != null)
        {
            SerializedProperty p = eqSO.FindProperty("co2Extinguisher");
            if (p != null) p.objectReferenceValue = co2;
            log.Add("[WIRED] ExtinguisherEquipper.co2Extinguisher → " + co2.name);
        }

        GameObject water = FindAny("Water_Extinguisher");
        if (water == null) water = FindAny("Ext_Water");
        if (water != null)
        {
            SerializedProperty p = eqSO.FindProperty("waterExtinguisher");
            if (p != null) p.objectReferenceValue = water;
            log.Add("[WIRED] ExtinguisherEquipper.waterExtinguisher → " + water.name);
        }

        // Wire equippedLabel (TMP text for popup)
        GameObject extLabel = FindAny("ExtinguisherText");
        if (extLabel != null)
        {
            TMPro.TMP_Text txt = extLabel.GetComponent<TMPro.TMP_Text>();
            if (txt != null)
            {
                SerializedProperty p = eqSO.FindProperty("equippedLabel");
                if (p != null) p.objectReferenceValue = txt;
                log.Add("[WIRED] ExtinguisherEquipper.equippedLabel → ExtinguisherText");
            }
        }

        eqSO.ApplyModifiedProperties();
    }
}
