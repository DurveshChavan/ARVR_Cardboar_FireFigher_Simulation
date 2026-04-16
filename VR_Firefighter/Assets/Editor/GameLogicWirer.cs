using UnityEngine;
using UnityEditor;
using TMPro;

public class GameLogicWirer
{
    [MenuItem("VR Firefighter/Wire Component Logic")]
    public static void WireLogic()
    {
        // 1. Create/Find GameManager
        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj == null)
        {
            gmObj = new GameObject("GameManager");
        }
        GameManager gm = gmObj.GetComponent<GameManager>();
        if (gm == null) gm = gmObj.AddComponent<GameManager>();

        // 2. Attach Controllers to PlayerRig
        GameObject playerRig = GameObject.Find("PlayerRig");
        if (playerRig != null)
        {
            if (playerRig.GetComponent<ExtinguisherEquipper>() == null)
                playerRig.AddComponent<ExtinguisherEquipper>();
            
            if (playerRig.GetComponent<ExtinguisherAimer>() == null)
                playerRig.AddComponent<ExtinguisherAimer>();

            if (playerRig.GetComponent<ExtinguisherShooter>() == null)
                playerRig.AddComponent<ExtinguisherShooter>();
        }
        else
        {
            Debug.LogWarning("PlayerRig not found in scene! Controllers could not be attached.");
        }

        // 3. Attach FireController to Kitchen and ServerRoom fire particles
        AttachFireControllerTo("Kitchen", "FireParticles", true);
        AttachFireControllerTo("ServerRoom", "FireSource", true);

        // 4. Wire Inspector References on GameManager
        // Try to locate environments
        GameObject kitchenRoot = GameObject.Find("Kitchen");
        if(kitchenRoot == null) kitchenRoot = FindInactiveObjectByName("Kitchen");
        if(kitchenRoot != null) gm.kitchenRoot = kitchenRoot;

        GameObject serverRoot = GameObject.Find("ServerRoom");
        if(serverRoot == null) serverRoot = FindInactiveObjectByName("ServerRoom");
        if(serverRoot != null) gm.serverRoomRoot = serverRoot;

        // Try to locate TMP Texts (assuming they exist in the HUD)
        // Note: The user prompt mentions attaching UI references.
        // We will do a generic search, but HUD Canvas is Phase 6, so they might not exist yet!
        TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();
        foreach(TMP_Text t in texts)
        {
            if (t.name.Contains("Timer")) gm.timerText = t;
            else if (t.name.Contains("Result")) gm.resultText = t;
            else if (t.name.Contains("Ext") || t.name.Contains("Popup")) gm.extText = t;
        }

        EditorUtility.SetDirty(gm);
        Debug.Log("Game Logic scripts attached and Inspector references wired (where available)!");
    }

    private static void AttachFireControllerTo(string rootName, string childName, bool includeInactive)
    {
        GameObject root = GameObject.Find(rootName);
        if(root == null) root = FindInactiveObjectByName(rootName);

        if (root != null)
        {
            Transform match = FindChildRecursive(root.transform, childName);
            if(match != null)
            {
                if(match.GetComponent<FireController>() == null)
                    match.gameObject.AddComponent<FireController>();
            }
            else
            {
                Debug.LogWarning("Fire source '" + childName + "' not found under " + rootName);
            }
        }
    }

    private static GameObject FindInactiveObjectByName(string name)
    {
        Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
        for (int i = 0; i < objs.Length; i++)
        {
            if (objs[i].hideFlags == HideFlags.None && objs[i].name == name)
            {
                return objs[i].gameObject;
            }
        }
        return null;
    }

    private static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform result = FindChildRecursive(child, name);
            if (result != null) return result;
        }
        return null;
    }
}
