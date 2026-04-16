using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum Scenario { Kitchen, ServerRoom }
    public enum ExtType { DCP, CO2, Water }

    [Header("State")]
    public Scenario currentScenario;
    public ExtType currentExtinguisher = ExtType.DCP;
    public bool gameActive = false;

    [Header("Timer")]
    public float timeLimit = 60f;
    private float timer;

    [Header("UI References")]
    public TMP_Text timerText;
    public TMP_Text extText;
    public TMP_Text resultText;

    [Header("Scene References")]
    public GameObject kitchenRoot;
    public GameObject serverRoomRoot;

    // Single fire (Kitchen scenario)
    public FireController currentFireController;

    // Multi-fire support (Server Room scenario with 3 racks)
    [HideInInspector] public FireController[] allFireControllers;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartGame(Scenario scenario)
    {
        currentScenario = scenario;
        timer = timeLimit;
        gameActive = true;

        if (resultText != null) resultText.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(true);

        // Find ALL FireControllers in the now-active scene
        allFireControllers = Object.FindObjectsByType<FireController>(FindObjectsSortMode.None);

        // Also keep backward-compatible single reference
        currentFireController = allFireControllers != null && allFireControllers.Length > 0
            ? allFireControllers[0]
            : null;

        Debug.Log($"[GameManager] StartGame({scenario}) — found {allFireControllers?.Length ?? 0} fire(s)");
    }

    void Update()
    {
        if (!gameActive) return;

        timer -= Time.deltaTime;
        int seconds = Mathf.CeilToInt(timer);
        if (timerText != null) timerText.text = $"Time: {seconds}s";

        if (timer <= 0f)
            MissionFailed("Time expired!");
    }

    /// <summary>Called by FireController when a single fire is extinguished.</summary>
    public void NotifyFireExtinguished(FireController fire)
    {
        if (!gameActive) return;

        // Check whether ALL fires in the scene are now out
        if (allFireControllers == null || allFireControllers.Length == 0)
        {
            MissionComplete(); // fallback
            return;
        }

        foreach (FireController fc in allFireControllers)
        {
            if (fc != null && fc.fireScale > 0.05f)
                return; // at least one still burning — keep going
        }

        // All fires out → win!
        MissionComplete();
    }

    public void MissionComplete()
    {
        if (!gameActive) return;
        gameActive = false;
        ShowResult("MISSION COMPLETE!\nFire suppressed.", Color.green);
    }

    public void MissionFailed(string reason)
    {
        if (!gameActive) return;
        gameActive = false;
        ShowResult($"MISSION FAILED\n{reason}", Color.red);
    }

    public void InstantFail(string reason)
    {
        MissionFailed(reason);
    }

    void ShowResult(string msg, Color col)
    {
        // Gracefully handle missing UI — log clearly instead of silent return
        if (resultText == null)
        {
            Debug.LogWarning($"[GameManager] resultText is null — cannot show: {msg}. " +
                             "Run VR Firefighter → Wire Entire Scene to fix UI references.");
        }
        else
        {
            resultText.text = msg;
            resultText.color = col;
            resultText.gameObject.SetActive(true);
        }

        if (timerText != null)
            timerText.gameObject.SetActive(false);

        Debug.Log($"[GameManager] Result: {msg}");
    }

    public void ShowExtinguisherPopup(string msg)
    {
        if (extText == null) return;
        StopAllCoroutines();
        StartCoroutine(ShowPopupCoroutine(msg));
    }

    System.Collections.IEnumerator ShowPopupCoroutine(string msg)
    {
        extText.text = msg;
        extText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        extText.gameObject.SetActive(false);
    }
}
