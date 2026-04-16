using UnityEngine;
using UnityEditor;

public class MissingScriptCleaner
{
    [MenuItem("VR Firefighter/Remove Missing Scripts")]
    public static void RemoveMissingScripts()
    {
        int totalRemoved = 0;
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();

        foreach (GameObject go in allObjects)
        {
            if (!go.scene.isLoaded) continue;

            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (count > 0)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                Debug.Log("[CLEANED] Removed " + count + " missing script(s) from: " + go.name);
                totalRemoved += count;
            }
        }

        if (totalRemoved > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log("=== Removed " + totalRemoved + " missing scripts total ===");
        }
        else
        {
            Debug.Log("No missing scripts found.");
        }
    }
}
