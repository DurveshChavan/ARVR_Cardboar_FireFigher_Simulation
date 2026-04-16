using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections;

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

    [Header("Lobby / Return")]
    [Tooltip("Reference to the ScenarioSelector — wired automatically at runtime.")]
    public ScenarioSelector scenarioSelector;

    // Single fire (Kitchen scenario)
    public FireController currentFireController;

    // Multi-fire support (Server Room scenario with 3 racks)
    [HideInInspector] public FireController[] allFireControllers;

    // Return-to-lobby state
    private bool _awaitingLobbyReturn = false;
    private float _returnHoldTimer = 0f;
    private const float ReturnHoldSeconds = 1.5f; // hold A for 1.5s to return

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (scenarioSelector == null)
            scenarioSelector = Object.FindFirstObjectByType<ScenarioSelector>();
    }

    public void StartGame(Scenario scenario)
    {
        currentScenario = scenario;
        timer = timeLimit;
        gameActive = true;
        _awaitingLobbyReturn = false;
        _returnHoldTimer = 0f;

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
        // ── In-game timer ─────────────────────────────────────────────────────
        if (gameActive)
        {
            timer -= Time.deltaTime;
            int seconds = Mathf.CeilToInt(timer);
            if (timerText != null) timerText.text = $"Time: {seconds}s";

            if (timer <= 0f)
                MissionFailed("Time expired!");

            return; // don't check lobby return while game is running
        }

        // ── Post-game: hold A to return to lobby ─────────────────────────────
        if (_awaitingLobbyReturn)
        {
            var gp = Gamepad.current;
            if (gp != null && gp.buttonSouth.isPressed)
            {
                _returnHoldTimer += Time.deltaTime;

                // Update prompt to show hold progress
                if (resultText != null)
                {
                    int pct = Mathf.RoundToInt((_returnHoldTimer / ReturnHoldSeconds) * 100f);
                    resultText.text = _returnHoldBaseMsg +
                        $"\n\n[Hold A — {pct}%]";
                }

                if (_returnHoldTimer >= ReturnHoldSeconds)
                {
                    _awaitingLobbyReturn = false;
                    ReturnToLobby();
                }
            }
            else
            {
                // Released — reset hold timer but keep prompt visible
                _returnHoldTimer = 0f;
                if (resultText != null)
                    resultText.text = _returnHoldBaseMsg + "\n\n[Hold A to return to lobby]";
            }
        }
    }

    // ── Mission outcomes ─────────────────────────────────────────────────────

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
        BeginLobbyReturn();
    }

    public void MissionFailed(string reason)
    {
        if (!gameActive) return;
        gameActive = false;
        ShowResult($"MISSION FAILED\n{reason}", Color.red);
        BeginLobbyReturn();
    }

    public void InstantFail(string reason)
    {
        MissionFailed(reason);
    }

    // ── Return-to-lobby ──────────────────────────────────────────────────────

    private string _returnHoldBaseMsg = "";

    void BeginLobbyReturn()
    {
        _awaitingLobbyReturn = true;
        _returnHoldTimer = 0f;
        // The prompt gets appended to whatever resultText shows
        if (resultText != null)
            _returnHoldBaseMsg = resultText.text;
        else
            _returnHoldBaseMsg = "";

        if (resultText != null)
            resultText.text = _returnHoldBaseMsg + "\n\n[Hold A to return to lobby]";
    }

    void ReturnToLobby()
    {
        // Reset extinguisher type for next round
        currentExtinguisher = ExtType.DCP;

        // Reset all fire controllers so they're fresh for replay
        if (allFireControllers != null)
        {
            foreach (FireController fc in allFireControllers)
            {
                if (fc != null)
                {
                    fc.fireScale = 1f;
                    fc.gameObject.SetActive(true);
                    // Re-enable particle system
                    var ps = fc.GetComponent<ParticleSystem>();
                    if (ps == null) ps = fc.GetComponentInChildren<ParticleSystem>();
                    if (ps != null) ps.Play();
                }
            }
        }

        allFireControllers = null;
        currentFireController = null;

        // Hide result UI, show timer (will be hidden by ScenarioSelector)
        if (resultText != null) resultText.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);

        // Deactivate scenario roots
        if (kitchenRoot != null) kitchenRoot.SetActive(false);
        if (serverRoomRoot != null) serverRoomRoot.SetActive(false);

        // Tell ScenarioSelector to show the lobby screen
        if (scenarioSelector != null)
            scenarioSelector.ShowLobby();
        else
            Debug.LogWarning("[GameManager] scenarioSelector is null — cannot return to lobby.");

        Debug.Log("[GameManager] Returned to lobby.");
    }

    // ── UI helpers ───────────────────────────────────────────────────────────

    void ShowResult(string msg, Color col)
    {
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

    IEnumerator ShowPopupCoroutine(string msg)
    {
        extText.text = msg;
        extText.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        extText.gameObject.SetActive(false);
    }
}
