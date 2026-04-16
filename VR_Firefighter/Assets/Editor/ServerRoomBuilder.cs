using UnityEngine;
using UnityEditor;

public class ServerRoomBuilder
{
    [MenuItem("VR Firefighter/Build Server Room Environment")]
    public static void BuildServerRoom()
    {
        // 1. Create Root Object
        GameObject serverRoom = new GameObject("ServerRoom");
        serverRoom.transform.position = Vector3.zero;

        // 2. Setup Materials Programmatically if they don't exist
        Material darkGrey = CreateOrLoadMaterial("DarkGrey", ColorUtility.TryParseHtmlString("#2A2A2A", out Color dc) ? dc : Color.gray);
        Material veryDarkGrey = CreateOrLoadMaterial("VeryDarkGrey", ColorUtility.TryParseHtmlString("#1A1A1A", out Color vdc) ? vdc : Color.gray);
        Material rackGrey = CreateOrLoadMaterial("RackGrey", ColorUtility.TryParseHtmlString("#303030", out Color rc) ? rc : Color.gray);
        Material trayGrey = CreateOrLoadMaterial("TrayGrey", ColorUtility.TryParseHtmlString("#404040", out Color tc) ? tc : Color.gray);
        Material blackMatch = CreateOrLoadMaterial("UPSBlack", ColorUtility.TryParseHtmlString("#0A0A0A", out Color bm) ? bm : Color.black);
        Material greenLed = CreateOrLoadMaterial("GreenLED", ColorUtility.TryParseHtmlString("#00FF00", out Color gl) ? gl : Color.green);
        Material redLed = CreateOrLoadMaterial("RedLED", ColorUtility.TryParseHtmlString("#FF0000", out Color rl) ? rl : Color.red);
        Material signRed = CreateOrLoadMaterial("SignRed", ColorUtility.TryParseHtmlString("#CC0000", out Color sr) ? sr : Color.red);

        // Cylinders placeholders
        Material redExt = CreateOrLoadMaterial("Red", Color.red);
        Material blackExt = CreateOrLoadMaterial("Black", Color.black);
        Material blueExt = CreateOrLoadMaterial("Blue", Color.blue);

        // Floor
        CreatePrimitive(PrimitiveType.Plane, "Floor", new Vector3(0, 0, 0), new Vector3(1, 1, 1), darkGrey, serverRoom.transform);

        // Walls
        CreatePrimitive(PrimitiveType.Cube, "WallNorth", new Vector3(0, 1.5f, 4), new Vector3(8, 3, 0.2f), veryDarkGrey, serverRoom.transform);
        CreatePrimitive(PrimitiveType.Cube, "WallSouth", new Vector3(0, 1.5f, -4), new Vector3(8, 3, 0.2f), veryDarkGrey, serverRoom.transform);
        CreatePrimitive(PrimitiveType.Cube, "WallEast", new Vector3(4, 1.5f, 0), new Vector3(0.2f, 3, 8), veryDarkGrey, serverRoom.transform);
        CreatePrimitive(PrimitiveType.Cube, "WallWest", new Vector3(-4, 1.5f, 0), new Vector3(0.2f, 3, 8), veryDarkGrey, serverRoom.transform);

        // Server Racks
        GameObject rackLeft = CreatePrimitive(PrimitiveType.Cube, "Rack_Left", new Vector3(-1.5f, 1, 1), new Vector3(0.6f, 2, 0.8f), rackGrey, serverRoom.transform);
        GameObject rackMiddle = CreatePrimitive(PrimitiveType.Cube, "Rack_Middle", new Vector3(0, 1, 1), new Vector3(0.6f, 2, 0.8f), rackGrey, serverRoom.transform);
        GameObject rackRight = CreatePrimitive(PrimitiveType.Cube, "Rack_Right", new Vector3(1.5f, 1, 1), new Vector3(0.6f, 2, 0.8f), rackGrey, serverRoom.transform);

        // LEDs on racks
        AddRackLeds(rackLeft, greenLed, redLed);
        AddRackLeds(rackMiddle, greenLed, redLed);
        AddRackLeds(rackRight, greenLed, redLed);

        // Details
        CreatePrimitive(PrimitiveType.Cube, "CableTray", new Vector3(0, 2.1f, 1), new Vector3(4, 0.1f, 0.3f), trayGrey, serverRoom.transform);
        CreatePrimitive(PrimitiveType.Cube, "UPSUnit", new Vector3(3, 0.5f, 1), new Vector3(0.5f, 1, 0.4f), blackMatch, serverRoom.transform);
        CreatePrimitive(PrimitiveType.Cube, "OfficeChair_Seat", new Vector3(2.5f, 0.5f, -1), new Vector3(0.5f, 0.1f, 0.5f), rackGrey, serverRoom.transform);
        CreatePrimitive(PrimitiveType.Cube, "OfficeChair_Back", new Vector3(2.5f, 0.8f, -1.2f), new Vector3(0.5f, 0.6f, 0.05f), rackGrey, serverRoom.transform);

        // Fire Source on Rack Middle
        GameObject fireSource = new GameObject("FireSource");
        fireSource.transform.parent = rackMiddle.transform;
        fireSource.transform.localPosition = new Vector3(0, 1.05f, 0); // 2.1 height on top of 2-height rack
        fireSource.tag = "FireZone";

        // Particle System
        ParticleSystem ps = fireSource.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.15f;
        
        Gradient grad = new Gradient();
        grad.SetKeys(new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(new Color(0, 0.26f, 1f), 1.0f) }, new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(0.0f, 1.0f) });
        main.startColor = new ParticleSystem.MinMaxGradient(grad);

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(0.4f, 0.1f, 0.2f);
        
        var emission = ps.emission;
        emission.rateOverTime = 50f;

        ParticleSystemRenderer renderer = fireSource.GetComponent<ParticleSystemRenderer>();
        // Just use unity default particle material if we can't find specific ones since it's an editor script
        Material defaultParticleMat = AssetDatabase.GetBuiltinExtraResource<Material>("Default-Particle.mat");
        if(defaultParticleMat != null) renderer.material = defaultParticleMat;

        // Trigger Collider
        BoxCollider bc = fireSource.AddComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(0.6f, 0.5f, 0.8f);

        // Attempt to add FireController if it exists in namespace
        System.Type t = System.Type.GetType("FireController, Assembly-CSharp");
        if(t != null)
        {
            fireSource.AddComponent(t);
        }
        else
        {
            Debug.LogWarning("FireController script not found. Make sure an implementation exists.");
        }

        // Extinguishers
        GameObject extRack = new GameObject("ExtinguisherRack");
        extRack.transform.parent = serverRoom.transform;
        extRack.transform.position = new Vector3(-3.5f, 0.5f, 2f);
        
        CreatePrimitive(PrimitiveType.Cylinder, "DCP_Extinguisher", new Vector3(-3.5f, 0.4f, 1.5f), new Vector3(0.3f, 0.4f, 0.3f), redExt, extRack.transform);
        CreatePrimitive(PrimitiveType.Cylinder, "CO2_Extinguisher", new Vector3(-3.5f, 0.4f, 2.0f), new Vector3(0.3f, 0.4f, 0.3f), blackExt, extRack.transform);
        CreatePrimitive(PrimitiveType.Cylinder, "Water_Extinguisher", new Vector3(-3.5f, 0.4f, 2.5f), new Vector3(0.3f, 0.4f, 0.3f), blueExt, extRack.transform);


        // Warning Sign
        GameObject sign = CreatePrimitive(PrimitiveType.Cube, "WarningSign", new Vector3(0, 2, -3.9f), new Vector3(2.2f, 0.9f, 0.05f), signRed, serverRoom.transform);
        
        // Disable scene hierarchy root
        serverRoom.SetActive(false);

        Debug.Log("Server Room successfully built.");
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

    private static void AddRackLeds(GameObject rack, Material defaultGreen, Material defaultRed)
    {
        // Simple generation of 3 little LEDs on front
        float startY = 0.5f;
        for(int i = 0; i < 3; i++)
        {
            GameObject led = GameObject.CreatePrimitive(PrimitiveType.Cube);
            led.name = "LED_" + i;
            led.transform.parent = rack.transform;
            // Place on front face (Z direction is usually face). Scale is small.
            led.transform.localScale = new Vector3(0.05f / rack.transform.localScale.x, 0.05f / rack.transform.localScale.y, 0.05f / rack.transform.localScale.z);
            led.transform.localPosition = new Vector3(0, startY - (i * 0.1f), -0.5f);
            
            Material chosen = (i % 2 == 0) ? defaultGreen : defaultRed;
            if(led.TryGetComponent<Renderer>(out Renderer ren))
            {
                ren.sharedMaterial = chosen;
            }
        }
    }

    private static Material CreateOrLoadMaterial(string name, Color color)
    {
        string path = "Assets/Materials/" + name + ".mat";
        Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            // Just use Standard or URP lit depending on what's available
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
