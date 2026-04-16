using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SceneBinaryIsolator : EditorWindow
{
    private List<GameObject> activeRoots = new List<GameObject>();
    private Vector2 scrollPos;

    [MenuItem("Tools/Binary Search Isolator")]
    public static void ShowWindow()
    {
        GetWindow<SceneBinaryIsolator>("Binary Isolator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Scene Binary Search Isolator", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Use this tool to rapidly halve the objects in your scene. " +
            "If a build crashes, the corrupted asset is in the currently remaining objects.\n\n" +
            "1. Gather Root Objects.\n" +
            "2. Build App. Does it crash?\n" +
            "   - YES: The error is in the REMAINING objects. Keep dividing.\n" +
            "   - NO: The error was in the DESTROYED objects. Undo, then destroy the other half.\n\n" +
            "NOTE: This tool now physically DELETES the objects (with Undo support) because " +
            "disabled objects are still serialized into the APK and will still cause a crash!", 
            MessageType.Warning);

        if (GUILayout.Button("1. Gather ENABLED Root Objects", GUILayout.Height(30)))
        {
            GatherActiveRoots();
        }

        GUILayout.Label($"Currently Tracking: {activeRoots.Count} Enabled Roots");

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Destroy FIRST Half", GUILayout.Height(40)))
        {
            DisableHalf(true);
        }
        if (GUILayout.Button("Destroy SECOND Half", GUILayout.Height(40)))
        {
            DisableHalf(false);
        }
        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (GUILayout.Button("Undo Last Action (Ctrl+Z)", GUILayout.Height(30)))
        {
            Undo.PerformUndo();
            GatherActiveRoots();
        }
        


        EditorGUILayout.Space();
        GUILayout.Label("Tracked Objects:", EditorStyles.boldLabel);

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        foreach (var go in activeRoots)
        {
            if (go != null)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(go, typeof(GameObject), true);
                }
            }
        }
        EditorGUILayout.EndScrollView();
    }

    private void GatherActiveRoots()
    {
        activeRoots.Clear();
        Scene activeScene = SceneManager.GetActiveScene();
        GameObject[] rootObjects = activeScene.GetRootGameObjects();

        foreach (GameObject go in rootObjects)
        {
            if (go.activeSelf && go.hideFlags != HideFlags.HideAndDontSave)
            {
                activeRoots.Add(go);
            }
        }
    }

    private void DisableHalf(bool firstHalf)
    {
        if (activeRoots.Count == 0)
        {
            Debug.LogWarning("No active roots to disable! Please gather ENABLED root objects first.");
            return;
        }

        if (activeRoots.Count <= 1)
        {
            Debug.LogWarning("Cannot divide further! You are down to the last object: " + activeRoots[0].name);
            EditorGUIUtility.PingObject(activeRoots[0]);
            return;
        }

        int halfIndex = activeRoots.Count / 2;
        int startIndex = firstHalf ? 0 : halfIndex;
        int endIndex = firstHalf ? halfIndex : activeRoots.Count;

        Undo.SetCurrentGroupName("Binary Isolator Destroy Half");
        int group = Undo.GetCurrentGroup();

        for (int i = startIndex; i < endIndex; i++)
        {
            if (activeRoots[i] != null)
            {
                Undo.DestroyObjectImmediate(activeRoots[i]);
            }
        }
        Undo.CollapseUndoOperations(group);

        GatherActiveRoots();
        Debug.Log($"Halved scene. Now tracking {activeRoots.Count} active root objects.");
    }
}
