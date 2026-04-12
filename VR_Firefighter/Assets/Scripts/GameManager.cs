using UnityEngine;

public class GameManager : MonoBehaviour
{
    public enum Scenario { Kitchen, ServerRoom }

    // Called by ScenarioSelector when player picks a scenario
    public void StartGame(Scenario scenario)
    {
        // Phase 5 will implement full game logic here:
        // - Start 60s countdown timer
        // - Set active scenario for suppression logic
        // - Enable HUD elements
        Debug.Log("StartGame called with scenario: " + scenario);
    }
}
