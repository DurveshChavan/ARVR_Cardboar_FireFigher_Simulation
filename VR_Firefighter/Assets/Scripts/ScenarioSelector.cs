using UnityEngine;
using UnityEngine.InputSystem;

public class ScenarioSelector : MonoBehaviour
{
    [SerializeField] public GameObject selectionScreen;
    [SerializeField] public GameObject kitchenRoot;
    [SerializeField] public GameObject serverRoomRoot;
    [SerializeField] public GameManager gameManager;

    // Startup guards — prevent phantom button press from Xbox BT controller
    private float startupDelay = 1.0f;
    private int frameCount = 0;

    void Start()
    {
        // Fallback: find by name if not assigned in Inspector
        if (selectionScreen == null) selectionScreen = GameObject.Find("SelectionScreen");
        if (kitchenRoot == null)     kitchenRoot = FindInactive("Kitchen");
        if (serverRoomRoot == null)  serverRoomRoot = FindInactive("ServerRoom");
        if (gameManager == null)     gameManager = Object.FindFirstObjectByType<GameManager>();

        // Ensure correct start state
        if (kitchenRoot != null)     kitchenRoot.SetActive(false);
        if (serverRoomRoot != null)  serverRoomRoot.SetActive(false);
        if (selectionScreen != null) selectionScreen.SetActive(true);

        Debug.Log("ScenarioSelector ready. Kitchen=" + (kitchenRoot != null) +
                  " ServerRoom=" + (serverRoomRoot != null) +
                  " SelectionScreen=" + (selectionScreen != null));
    }

    void Update()
    {
        if (selectionScreen == null || !selectionScreen.activeSelf) return;

        // Guard 1: ignore input for the first 1 second (phantom BT press window)
        if (startupDelay > 0f) { startupDelay -= Time.deltaTime; return; }

        // Guard 2: ignore input for the first 10 frames
        if (frameCount < 10) { frameCount++; return; }

        var gp = Gamepad.current;
        if (gp == null) return;

        Debug.Log("ScenarioSelector Update - accepting input, frame=" + Time.frameCount);

        if (gp.buttonSouth.wasPressedThisFrame)
        {
            Debug.Log("A pressed at frame " + Time.frameCount + " - loading Kitchen");
            StartScenario(kitchenRoot, GameManager.Scenario.Kitchen);
        }

        if (gp.buttonEast.wasPressedThisFrame)
        {
            Debug.Log("B pressed at frame " + Time.frameCount + " - loading ServerRoom");
            StartScenario(serverRoomRoot, GameManager.Scenario.ServerRoom);
        }
    }

    void StartScenario(GameObject scenarioRoot, GameManager.Scenario scenario)
    {
        if (scenarioRoot == null) { Debug.LogError("ScenarioSelector: scenarioRoot is null!"); return; }
        if (gameManager == null) { Debug.LogError("ScenarioSelector: gameManager is null!"); return; }

        selectionScreen.SetActive(false);
        scenarioRoot.SetActive(true);
        gameManager.StartGame(scenario);
    }

    // Helper to find inactive GameObjects by name
    GameObject FindInactive(string name)
    {
        // Try active first
        GameObject obj = GameObject.Find(name);
        if (obj != null) return obj;

        var all = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (var go in all)
            if (go.name == name && go.scene.isLoaded) return go;
        return null;
    }
}
