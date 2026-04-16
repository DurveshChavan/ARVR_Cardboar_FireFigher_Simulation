using UnityEngine;
using UnityEditor;

public class KitchenBuilder
{
    [MenuItem("VR Firefighter/Build Kitchen Environment")]
    public static void BuildKitchen()
    {
        // 1. Create Root Object
        GameObject kitchen = new GameObject("Kitchen");
        kitchen.transform.position = Vector3.zero;

        // 2. Setup Materials Programmatically
        Material grey = CreateOrLoadMaterial("Grey", ColorUtility.TryParseHtmlString("#808080", out Color gc) ? gc : Color.gray);
        Material white = CreateOrLoadMaterial("White", Color.white);
        Material counterGrey = CreateOrLoadMaterial("CounterGrey", ColorUtility.TryParseHtmlString("#A0A0A0", out Color cg) ? cg : Color.grey);
        Material stoveDarkGrey = CreateOrLoadMaterial("StoveDarkGrey", ColorUtility.TryParseHtmlString("#303030", out Color sdc) ? sdc : Color.gray);
        Material lpgRed = CreateOrLoadMaterial("LPGRed", ColorUtility.TryParseHtmlString("#CC2200", out Color lr) ? lr : Color.red);
        Material signYellow = CreateOrLoadMaterial("SignYellow", ColorUtility.TryParseHtmlString("#FFEE00", out Color sy) ? sy : Color.yellow);
        
        Material redExt = CreateOrLoadMaterial("Red", Color.red);
        Material blackExt = CreateOrLoadMaterial("BlackExt", ColorUtility.TryParseHtmlString("#111111", out Color bE) ? bE : Color.black);
        Material blueExt = CreateOrLoadMaterial("Blue", ColorUtility.TryParseHtmlString("#2244CC", out Color blE) ? blE : Color.blue);

        // Floor
        CreatePrimitive(PrimitiveType.Plane, "Floor", new Vector3(0, 0, 0), new Vector3(1, 1, 1), grey, kitchen.transform);

        // Walls
        CreatePrimitive(PrimitiveType.Cube, "WallNorth", new Vector3(0, 1.5f, 4), new Vector3(8, 3, 0.2f), white, kitchen.transform);
        CreatePrimitive(PrimitiveType.Cube, "WallSouth", new Vector3(0, 1.5f, -4), new Vector3(8, 3, 0.2f), white, kitchen.transform);
        CreatePrimitive(PrimitiveType.Cube, "WallEast", new Vector3(4, 1.5f, 0), new Vector3(0.2f, 3, 8), white, kitchen.transform);
        CreatePrimitive(PrimitiveType.Cube, "WallWest", new Vector3(-4, 1.5f, 0), new Vector3(0.2f, 3, 8), white, kitchen.transform);

        // Counter and Stove
        CreatePrimitive(PrimitiveType.Cube, "Counter", new Vector3(-2, 0.45f, -3), new Vector3(3, 0.9f, 1), counterGrey, kitchen.transform);
        CreatePrimitive(PrimitiveType.Cube, "StoveTop", new Vector3(-2, 0.92f, -3), new Vector3(1.2f, 0.05f, 0.9f), stoveDarkGrey, kitchen.transform);

        // LPG Cylinder
        GameObject lpgCylinder = CreatePrimitive(PrimitiveType.Cylinder, "LPGCylinder", new Vector3(-1, 0.4f, -3), new Vector3(0.3f, 0.4f, 0.3f), lpgRed, kitchen.transform);

        // Fire Source on LPGCylinder
        GameObject fireSource = new GameObject("FireParticles");
        fireSource.transform.parent = lpgCylinder.transform;
        fireSource.transform.localPosition = Vector3.zero; 
        fireSource.tag = "FireZone";

        // Particle System
        ParticleSystem ps = fireSource.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.8f;
        main.startSpeed = 1.5f;
        main.startSize = 0.3f;
        
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(new Color(1f, 0.4f, 0f), 0.0f), new GradientColorKey(new Color(1f, 0.8f, 0f), 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });
        main.startColor = new ParticleSystem.MinMaxGradient(grad);

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        
        var emission = ps.emission;
        emission.rateOverTime = 30f;

        ParticleSystemRenderer renderer = fireSource.GetComponent<ParticleSystemRenderer>();
        Material defaultParticleMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
        if(defaultParticleMat != null) renderer.material = defaultParticleMat;

        // Trigger Collider
        SphereCollider sc = fireSource.AddComponent<SphereCollider>();
        sc.isTrigger = true;
        sc.radius = 0.4f;

        // Attempt to add FireController
        System.Type t = System.Type.GetType("FireController, Assembly-CSharp");
        if(t != null)
        {
            fireSource.AddComponent(t);
        }
        else
        {
            Debug.LogWarning("FireController script not found.");
        }

        // Extinguisher Rack
        CreatePrimitive(PrimitiveType.Cylinder, "ExtRack_DCP", new Vector3(-3.5f, 0.5f, 2f), new Vector3(0.12f, 0.5f, 0.12f), redExt, kitchen.transform);
        CreatePrimitive(PrimitiveType.Cylinder, "ExtRack_CO2", new Vector3(-3.5f, 0.5f, 2.4f), new Vector3(0.12f, 0.5f, 0.12f), blackExt, kitchen.transform);
        CreatePrimitive(PrimitiveType.Cylinder, "ExtRack_Water", new Vector3(-3.5f, 0.5f, 2.8f), new Vector3(0.12f, 0.5f, 0.12f), blueExt, kitchen.transform);

        // Rack Label
        GameObject labelBox = CreatePrimitive(PrimitiveType.Cube, "RackLabel_Wall", new Vector3(-3.9f, 0.8f, 2.4f), new Vector3(0.05f, 0.3f, 0.8f), grey, kitchen.transform);

        // Warning Sign
        GameObject sign = CreatePrimitive(PrimitiveType.Cube, "WarningSign", new Vector3(0, 2, -3.9f), new Vector3(2f, 0.8f, 0.05f), signYellow, kitchen.transform);
        
        // Disable scene hierarchy root
        kitchen.SetActive(false);

        Debug.Log("Kitchen environment successfully built. Note: TMP_Text components need to be manually added to 'WarningSign' and 'RackLabel_Wall'.");
    }

    private static GameObject CreatePrimitive(PrimitiveType type, string name, Vector3 position, Vector3 scale, Material mat, Transform parent)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.parent = parent;
        obj.transform.localPosition = position;
        obj.transform.localScale = scale;

        if (mat != null)
        {
            obj.GetComponent<Renderer>().sharedMaterial = mat;
        }

        return obj;
    }

    private static Material CreateOrLoadMaterial(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if(shader == null) shader = Shader.Find("Standard");

            m = new Material(shader);
            m.SetColor("_BaseColor", color);
            m.color = color;
            
            if(!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }
            AssetDatabase.CreateAsset(m, path);
        }
        return m;
    }
}
