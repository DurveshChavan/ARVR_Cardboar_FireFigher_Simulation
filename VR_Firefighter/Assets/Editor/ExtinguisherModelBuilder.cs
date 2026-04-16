using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// VR Firefighter → Build Extinguisher Models + Wire Scripts
///
/// What this tool does (in order):
///  1. Finds PlayerRig → Neck → Head hierarchy (built by Tools/Setup ManualVR Rig)
///  2. Removes any stale CardboardInitializer stub from PlayerRig
///  3. Deletes any duplicate WeaponHoldPoint found under LeftEye (wrong location)
///  4. Ensures ONE WeaponHoldPoint exists as a direct child of Head
///  5. Builds 3 extinguisher models (Ext_DCP, Ext_CO2, Ext_Water) under WeaponHoldPoint
///  6. Builds spray ParticleSystems on each extinguisher nozzle
///  7. Ensures ExtinguisherEquipper, ExtinguisherAimer, ExtinguisherShooter on PlayerRig
///  8. Wires all cross-references and saves the scene
/// </summary>
public class ExtinguisherModelBuilder
{
    [MenuItem("VR Firefighter/Build Extinguisher Models + Wire Scripts")]
    public static void Build()
    {
        Undo.SetCurrentGroupName("Build Extinguisher Models");
        int undoGroup = Undo.GetCurrentGroup();

        List<string> log = new List<string>();
        log.Add("=== ExtinguisherModelBuilder START ===");

        // ── STEP 1: Locate PlayerRig ──────────────────────────────────────────
        GameObject playerRig = GameObject.Find("PlayerRig");
        if (playerRig == null)
        {
            Debug.LogError("[ExtinguisherModelBuilder] PlayerRig not found! Run Tools → Setup ManualVR Rig first.");
            return;
        }

        // ── STEP 2: Remove CardboardInitializer stub (it does nothing, causes missing script warnings) ──
        CardboardInitializer ci = playerRig.GetComponent<CardboardInitializer>();
        if (ci != null)
        {
            Undo.DestroyObjectImmediate(ci);
            log.Add("[REMOVED] CardboardInitializer stub from PlayerRig");
        }
        else log.Add("[OK] No CardboardInitializer on PlayerRig");

        // ── STEP 3: Find Head (PlayerRig → Neck → Head) ───────────────────────
        Transform neck = playerRig.transform.Find("Neck");
        Transform head = neck != null ? neck.Find("Head") : null;

        if (head == null)
        {
            Debug.LogError("[ExtinguisherModelBuilder] Could not find PlayerRig/Neck/Head! Run Tools → Setup ManualVR Rig first.");
            return;
        }
        log.Add($"[FOUND] Head at path: {GetPath(head)}");

        // ── STEP 4: Clean up duplicate WeaponHoldPoints ───────────────────────
        // The bug: ExtinguisherModelBuilder previously parented WeaponHoldPoint to
        // Camera.main, which ends up being LeftEye. This creates a duplicate.
        // We need exactly ONE WeaponHoldPoint directly under Head.

        // Search all scene GameObjects for every WeaponHoldPoint
        List<GameObject> allWHP = new List<GameObject>();
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == "WeaponHoldPoint" && go.scene.isLoaded)
                allWHP.Add(go);
        }

        // Keep only the one that is a direct child of Head; destroy all others
        Transform correctWHP = head.Find("WeaponHoldPoint");
        foreach (GameObject whp in allWHP)
        {
            if (whp.transform == correctWHP) continue; // this is the good one
            if (correctWHP == null && whp.transform.parent == head)
            {
                // Treat it as the correct one
                correctWHP = whp.transform;
                continue;
            }
            log.Add($"[REMOVED] Duplicate WeaponHoldPoint at path: {GetPath(whp.transform)}");
            Undo.DestroyObjectImmediate(whp);
        }

        // If no WeaponHoldPoint exists yet, create one under Head
        if (correctWHP == null)
        {
            GameObject hp = new GameObject("WeaponHoldPoint");
            Undo.RegisterCreatedObjectUndo(hp, "Create WeaponHoldPoint");
            hp.transform.SetParent(head, false);
            hp.transform.localPosition = new Vector3(0.15f, -0.25f, 0.5f);
            hp.transform.localRotation = Quaternion.identity;
            hp.transform.localScale = Vector3.one;
            correctWHP = hp.transform;
            log.Add($"[CREATED] WeaponHoldPoint under Head at {hp.transform.localPosition}");
        }
        else
        {
            log.Add($"[FOUND] WeaponHoldPoint at correct location: {GetPath(correctWHP)}");
        }

        Transform holdPoint = correctWHP;

        // ── STEP 5: Build extinguisher models ─────────────────────────────────
        GameObject dcpModel   = BuildExtinguisherModel("Ext_DCP",   Color.red,           holdPoint);
        GameObject co2Model   = BuildExtinguisherModel("Ext_CO2",   Color.black,         holdPoint);

        Color blueColor;
        ColorUtility.TryParseHtmlString("#2244CC", out blueColor);
        GameObject waterModel = BuildExtinguisherModel("Ext_Water",  blueColor,           holdPoint);

        log.Add("[BUILT] Ext_DCP, Ext_CO2, Ext_Water under WeaponHoldPoint");

        // ── STEP 6: Build spray particle systems ──────────────────────────────
        ParticleSystem dcpSpray   = BuildSprayParticle(dcpModel,   "DCP_Spray",
            new Color(1f, 0.95f, 0.8f, 0.6f), new Color(1f, 1f, 0.9f, 0f));

        ParticleSystem co2Spray   = BuildSprayParticle(co2Model,   "CO2_Spray",
            new Color(0.85f, 0.9f, 1f, 0.5f), new Color(1f, 1f, 1f, 0f));

        ParticleSystem waterSpray = BuildSprayParticle(waterModel, "Water_Spray",
            new Color(0.3f, 0.5f, 1f, 0.7f), new Color(0.5f, 0.7f, 1f, 0f));

        log.Add("[BUILT] Spray particles: DCP_Spray, CO2_Spray, Water_Spray");

        // Default: only DCP visible
        co2Model.SetActive(false);
        waterModel.SetActive(false);

        // ── STEP 7: Ensure runtime scripts on PlayerRig ───────────────────────
        // CardboardPlayerController = left-joystick locomotion (nothing Cardboard-specific
        // at runtime — just Gamepad.current + CharacterController). It can get wiped by
        // "Remove Missing Scripts" if its GUID was broken, so we always re-ensure it here.
        CardboardPlayerController cpc = EnsureComponent<CardboardPlayerController>(playerRig);
        // Wire cameraTransform → Head so movement follows the look direction
        SerializedObject cpcSO = new SerializedObject(cpc);
        SetProp(cpcSO, "cameraTransform", head);
        cpcSO.ApplyModifiedProperties();
        log.Add("[ENSURED] CardboardPlayerController (left-stick movement) on PlayerRig, cameraTransform → Head");

        ExtinguisherEquipper equipper = EnsureComponent<ExtinguisherEquipper>(playerRig);
        ExtinguisherAimer    aimer    = EnsureComponent<ExtinguisherAimer>(playerRig);
        ExtinguisherShooter  shooter  = EnsureComponent<ExtinguisherShooter>(playerRig);

        log.Add("[OK] ExtinguisherEquipper / Aimer / Shooter ensured on PlayerRig");

        // ── STEP 8: Wire all references via SerializedObject (Undo-safe) ──────

        // Wire Equipper
        SerializedObject equipperSO = new SerializedObject(equipper);
        SetProp(equipperSO, "dcpExtinguisher",   dcpModel);
        SetProp(equipperSO, "co2Extinguisher",   co2Model);
        SetProp(equipperSO, "waterExtinguisher", waterModel);
        SetProp(equipperSO, "holdPosition",      holdPoint);
        // Wire ExtinguisherText popup label
        GameObject extTextObj = FindAny("ExtinguisherText");
        if (extTextObj != null)
        {
            TMP_Text lbl = extTextObj.GetComponent<TMP_Text>();
            if (lbl != null) SetProp(equipperSO, "equippedLabel", lbl);
        }
        equipperSO.ApplyModifiedProperties();
        log.Add("[WIRED] ExtinguisherEquipper references");

        // Wire Aimer
        SerializedObject aimerSO = new SerializedObject(aimer);
        SetProp(aimerSO, "holdPosition", holdPoint);
        aimerSO.ApplyModifiedProperties();
        log.Add("[WIRED] ExtinguisherAimer.holdPosition → WeaponHoldPoint");

        // Wire Shooter
        SerializedObject shooterSO = new SerializedObject(shooter);
        SetProp(shooterSO, "aimer",         aimer);
        SetProp(shooterSO, "equipper",      equipper);
        SetProp(shooterSO, "holdPosition",  holdPoint);
        SetProp(shooterSO, "dcpSpray",      dcpSpray);
        SetProp(shooterSO, "co2Spray",      co2Spray);
        SetProp(shooterSO, "waterSpray",    waterSpray);
        shooterSO.ApplyModifiedProperties();
        log.Add("[WIRED] ExtinguisherShooter references");


        // ── STEP 10: Finish up ────────────────────────────────────────────────
        Undo.CollapseUndoOperations(undoGroup);
        EditorUtility.SetDirty(playerRig);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

        // Print summary
        log.Add("=== ExtinguisherModelBuilder COMPLETE ===");
        foreach (string entry in log)
        {
            if (entry.Contains("[REMOVED]") || entry.Contains("[MISSING]"))
                Debug.LogWarning(entry);
            else
                Debug.Log(entry);
        }

        EditorUtility.DisplayDialog(
            "Extinguisher Models Built ✓",
            "Hierarchy (all under Head → WeaponHoldPoint):\n\n" +
            "WeaponHoldPoint\n" +
            "  ├─ Ext_DCP (Red)    + DCP_Spray\n" +
            "  ├─ Ext_CO2 (Black)  + CO2_Spray\n" +
            "  └─ Ext_Water (Blue) + Water_Spray\n\n" +
            "Scripts on PlayerRig:\n" +
            "  ExtinguisherEquipper / Aimer / Shooter\n\n" +
            "Save scene (Ctrl+S) then Build & Run!",
            "OK");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Extinguisher model builder (primitive cylindrical model)
    // ─────────────────────────────────────────────────────────────────────────
    static GameObject BuildExtinguisherModel(string modelName, Color bodyColor, Transform parent)
    {
        // Destroy existing so rebuild is idempotent
        Transform existing = parent.Find(modelName);
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        GameObject root = new GameObject(modelName);
        Undo.RegisterCreatedObjectUndo(root, "Create " + modelName);
        root.transform.SetParent(parent, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale    = Vector3.one;

        // Body — main cylinder
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        body.name = "Body";
        body.transform.SetParent(root.transform, false);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale    = new Vector3(0.06f, 0.15f, 0.06f);
        SetColor(body, bodyColor);
        Object.DestroyImmediate(body.GetComponent<Collider>());

        // Nozzle — small cylinder pointing forward
        GameObject nozzle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        nozzle.name = "Nozzle";
        nozzle.transform.SetParent(root.transform, false);
        nozzle.transform.localPosition = new Vector3(0f, 0.12f, 0.05f);
        nozzle.transform.localScale    = new Vector3(0.015f, 0.04f, 0.015f);
        nozzle.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        SetColor(nozzle, Color.grey);
        Object.DestroyImmediate(nozzle.GetComponent<Collider>());

        // Handle — small cube on top
        GameObject handle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        handle.name = "Handle";
        handle.transform.SetParent(root.transform, false);
        handle.transform.localPosition = new Vector3(0f, 0.17f, 0f);
        handle.transform.localScale    = new Vector3(0.04f, 0.02f, 0.025f);
        SetColor(handle, new Color(0.15f, 0.15f, 0.15f));
        Object.DestroyImmediate(handle.GetComponent<Collider>());

        // Pressure gauge — tiny sphere
        GameObject gauge = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        gauge.name = "Gauge";
        gauge.transform.SetParent(root.transform, false);
        gauge.transform.localPosition = new Vector3(0.035f, 0.08f, 0f);
        gauge.transform.localScale    = new Vector3(0.02f, 0.02f, 0.01f);
        SetColor(gauge, Color.white);
        Object.DestroyImmediate(gauge.GetComponent<Collider>());

        return root;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Spray particle system builder
    // ─────────────────────────────────────────────────────────────────────────
    static ParticleSystem BuildSprayParticle(GameObject extinguisher, string sprayName,
        Color startColor, Color endColor)
    {
        // Destroy existing spray if present (idempotent rebuild)
        Transform existingSpray = extinguisher.transform.Find(sprayName);
        if (existingSpray != null) Object.DestroyImmediate(existingSpray.gameObject);

        Transform nozzle = extinguisher.transform.Find("Nozzle");
        Vector3 sprayLocalPos = nozzle != null
            ? nozzle.localPosition + new Vector3(0f, 0f, 0.05f)
            : new Vector3(0f, 0.12f, 0.1f);

        GameObject sprayObj = new GameObject(sprayName);
        sprayObj.transform.SetParent(extinguisher.transform, false);
        sprayObj.transform.localPosition = sprayLocalPos;
        sprayObj.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        ParticleSystem ps = sprayObj.AddComponent<ParticleSystem>();

        var main          = ps.main;
        main.startLifetime   = 0.6f;
        main.startSpeed      = 3f;
        main.startSize       = 0.03f;
        main.maxParticles    = 200;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake     = false;

        Gradient grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]  { new GradientColorKey(startColor, 0f),   new GradientColorKey(endColor, 1f) },
            new GradientAlphaKey[]  { new GradientAlphaKey(startColor.a, 0f), new GradientAlphaKey(0f, 1f) }
        );
        main.startColor = new ParticleSystem.MinMaxGradient(grad);

        var emission = ps.emission;
        emission.rateOverTime = 80f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle  = 12f;
        shape.radius = 0.01f;

        ParticleSystemRenderer rdr = sprayObj.GetComponent<ParticleSystemRenderer>();
        Material mat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
        if (mat != null) rdr.material = mat;

        ps.Stop();
        return ps;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────
    static void SetColor(GameObject obj, Color color)
    {
        Renderer r = obj.GetComponent<Renderer>();
        if (r == null) return;
        Material m = new Material(Shader.Find("Standard"));
        m.color = color;
        r.sharedMaterial = m;
    }

    static T EnsureComponent<T>(GameObject obj) where T : Component
    {
        T c = obj.GetComponent<T>();
        if (c == null) c = Undo.AddComponent<T>(obj);
        return c;
    }

    /// <summary>Set an Object reference property on a SerializedObject.</summary>
    static void SetProp(SerializedObject so, string propName, Object value)
    {
        SerializedProperty p = so.FindProperty(propName);
        if (p != null) p.objectReferenceValue = value;
    }

    static GameObject FindAny(string name)
    {
        GameObject obj = GameObject.Find(name);
        if (obj != null) return obj;
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
            if (go.name == name && go.scene.isLoaded) return go;
        return null;
    }

    static string GetPath(Transform t)
    {
        if (t == null) return "null";
        string path = t.name;
        Transform p = t.parent;
        while (p != null) { path = p.name + "/" + path; p = p.parent; }
        return path;
    }
}
