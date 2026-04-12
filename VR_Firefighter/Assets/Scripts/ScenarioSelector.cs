using UnityEngine;
using UnityEngine.InputSystem;

public class ScenarioSelector : MonoBehaviour
{
    public GameObject selectionScreen;
    public GameObject kitchenRoot;
    public GameObject serverRoomRoot;
    public GameManager gameManager;

    void Update()
    {
        if (selectionScreen == null || !selectionScreen.activeSelf) return;

        var gp = Gamepad.current;
        if (gp == null) return;

        if (gp.buttonSouth.wasPressedThisFrame)
            StartScenario(kitchenRoot, GameManager.Scenario.Kitchen);

        if (gp.buttonEast.wasPressedThisFrame)
            StartScenario(serverRoomRoot, GameManager.Scenario.ServerRoom);
    }

    void StartScenario(GameObject scenarioRoot, GameManager.Scenario scenario)
    {
        selectionScreen.SetActive(false);
        scenarioRoot.SetActive(true);
        gameManager.StartGame(scenario);
    }
}
