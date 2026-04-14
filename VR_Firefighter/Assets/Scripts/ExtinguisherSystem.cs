using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

// =============================================================================
// All extinguisher classes in ONE file to avoid Unity cross-file compile issues.
// =============================================================================

// =============================================================================
// CLASS 1: ExtinguisherEquipper - Y button cycles DCP/CO2/Water
// =============================================================================
public class ExtinguisherEquipper : MonoBehaviour
{
    [Header("Extinguisher Models")]
    public GameObject dcpExtinguisher;
    public GameObject co2Extinguisher;
    public GameObject waterExtinguisher;

    [Header("Weapon Hold Position")]
    public Transform holdPosition;

    [Header("UI Popup")]
    public TMP_Text equippedLabel;

    private GameObject[] extinguishers;
    private int currentIndex = 0;
    private bool yWasPressed = false;

    private string[] names = { "DCP - Dry Chemical Powder", "CO2 - Carbon Dioxide", "Water" };
    private string[] descriptions = {
        "Effective: Class C Gas Fires",
        "Effective: Class E Electrical Fires",
        "Class A Solid Fires ONLY - NEVER on electrical"
    };

    void Start()
    {
        extinguishers = new GameObject[] { dcpExtinguisher, co2Extinguisher, waterExtinguisher };
        foreach (var e in extinguishers)
            if (e != null) e.SetActive(false);
        EquipExtinguisher(0);
        Debug.Log("ExtinguisherEquipper ready");
    }

    void Update()
    {
        var gp = Gamepad.current;
        if (gp == null) return;
        bool yNow = gp.buttonNorth.isPressed;
        if (yNow && !yWasPressed)
        {
            Debug.Log("Y pressed - cycling");
            CycleExtinguisher();
        }
        yWasPressed = yNow;
    }

    void CycleExtinguisher()
    {
        if (extinguishers[currentIndex] != null)
            extinguishers[currentIndex].SetActive(false);
        currentIndex = (currentIndex + 1) % extinguishers.Length;
        EquipExtinguisher(currentIndex);
    }

    void EquipExtinguisher(int index)
    {
        GameObject equipped = extinguishers[index];
        if (equipped == null) return;
        equipped.SetActive(true);
        if (holdPosition != null)
        {
            equipped.transform.SetParent(holdPosition);
            equipped.transform.localPosition = Vector3.zero;
            equipped.transform.localRotation = Quaternion.identity;
            equipped.transform.localScale = Vector3.one * 0.4f; // VR scale — smaller for close stereo view
        }
        if (GameManager.Instance != null)
            GameManager.Instance.currentExtinguisher = (GameManager.ExtType)index;
        ShowPopup(index);
        Debug.Log($"Equipped: {names[index]} (scale=0.4, parent={holdPosition?.name})");
    }

    void ShowPopup(int index)
    {
        if (equippedLabel == null) return;
        equippedLabel.text = names[index] + "\n" + descriptions[index];
        equippedLabel.gameObject.SetActive(true);
        CancelInvoke(nameof(HidePopup));
        Invoke(nameof(HidePopup), 3f);
    }

    void HidePopup()
    {
        if (equippedLabel != null)
            equippedLabel.gameObject.SetActive(false);
    }

    public int GetCurrentIndex() { return currentIndex; }
}

// =============================================================================
// CLASS 2: ExtinguisherAimer - LT raises extinguisher to aimed position
// =============================================================================
public class ExtinguisherAimer : MonoBehaviour
{
    [Header("Hold Position Transform")]
    public Transform holdPosition;

    [Header("Idle Position (hip carry)")]
    public Vector3 idleLocalPosition = new Vector3(0.15f, -0.35f, 0.45f);
    public Vector3 idleLocalRotation = new Vector3(0f, -10f, 0f);

    [Header("Aimed Position (center, raised)")]
    public Vector3 aimedLocalPosition = new Vector3(0.0f, -0.20f, 0.45f);
    public Vector3 aimedLocalRotation = new Vector3(-5f, 0f, 0f);

    [Header("Blend Speed")]
    public float aimSpeed = 8f;

    private bool isAiming = false;

    void Start()
    {
        // ── FORCE VR-corrected positions ──
        // Override any serialized (stale) values from the Inspector.
        // These are tuned for stereo split-screen (half-width viewports).
        idleLocalPosition = new Vector3(0.15f, -0.35f, 0.45f);
        idleLocalRotation = new Vector3(0f, -10f, 0f);
        aimedLocalPosition = new Vector3(0.0f, -0.20f, 0.45f);
        aimedLocalRotation = new Vector3(-5f, 0f, 0f);

        if (holdPosition != null)
        {
            holdPosition.localPosition = idleLocalPosition;
            holdPosition.localEulerAngles = idleLocalRotation;
        }
        Debug.Log("ExtinguisherAimer ready (VR-corrected positions forced)");
    }

    void Update()
    {
        var gp = Gamepad.current;
        if (gp == null) return;
        if (holdPosition == null) return;

        float lt = gp.leftTrigger.ReadValue();
        isAiming = lt > 0.1f;

        Vector3 targetPos = isAiming ? aimedLocalPosition : idleLocalPosition;
        Quaternion targetRot = isAiming
            ? Quaternion.Euler(aimedLocalRotation)
            : Quaternion.Euler(idleLocalRotation);

        holdPosition.localPosition = Vector3.Lerp(
            holdPosition.localPosition, targetPos, Time.deltaTime * aimSpeed);
        holdPosition.localRotation = Quaternion.Slerp(
            holdPosition.localRotation, targetRot, Time.deltaTime * aimSpeed);
    }

    public bool IsAiming() { return isAiming; }
}


// =============================================================================
// CLASS 3: ExtinguisherShooter - RT fires spray + recoil + fire suppression
// =============================================================================
public class ExtinguisherShooter : MonoBehaviour
{
    [Header("References")]
    public ExtinguisherAimer aimer;
    public ExtinguisherEquipper equipper;

    [Header("Spray Particles (DCP, CO2, Water)")]
    public ParticleSystem dcpSpray;
    public ParticleSystem co2Spray;
    public ParticleSystem waterSpray;

    [Header("Recoil")]
    public Transform holdPosition;
    public float recoilDistance = 0.04f;
    public float recoilSpeed = 15f;

    private ParticleSystem[] sprayParticles;
    private bool isFiring = false;
    private bool alreadyTriggeredInstantFail = false;

    void Start()
    {
        sprayParticles = new ParticleSystem[] { dcpSpray, co2Spray, waterSpray };
        foreach (var ps in sprayParticles)
            if (ps != null) ps.Stop();
        Debug.Log("ExtinguisherShooter ready");
    }

    void Update()
    {
        var gp = Gamepad.current;
        if (gp == null) return;
        if (aimer == null || equipper == null) return;

        float rt = gp.rightTrigger.ReadValue();
        bool wantFire = rt > 0.1f && aimer.IsAiming();

        if (wantFire && !isFiring)
            StartFire();
        else if (!wantFire && isFiring)
            StopFire();

        // Recoil while firing
        if (isFiring && holdPosition != null && aimer.IsAiming())
        {
            Vector3 recoilTarget = aimer.aimedLocalPosition + new Vector3(0f, 0f, -recoilDistance);
            holdPosition.localPosition = Vector3.Lerp(
                holdPosition.localPosition, recoilTarget, Time.deltaTime * recoilSpeed);
        }

        // Fire suppression
        if (isFiring)
            ApplyFireSuppression();
    }

    void StartFire()
    {
        if (!aimer.IsAiming()) return;
        isFiring = true;
        int idx = equipper.GetCurrentIndex();
        foreach (var ps in sprayParticles)
            if (ps != null) ps.Stop();
        if (sprayParticles[idx] != null)
        {
            sprayParticles[idx].Play();
            Debug.Log("Spraying: " + (GameManager.ExtType)idx);
        }
    }

    void StopFire()
    {
        isFiring = false;
        foreach (var ps in sprayParticles)
            if (ps != null && ps.isPlaying) ps.Stop();
    }

    void ApplyFireSuppression()
    {
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.gameActive) return;

        var fire = GameManager.Instance.currentFireController;
        if (fire == null)
        {
            fire = Object.FindFirstObjectByType<FireController>();
            if (fire != null) GameManager.Instance.currentFireController = fire;
        }
        if (fire == null) return;

        var ext = GameManager.Instance.currentExtinguisher;
        var sc = GameManager.Instance.currentScenario;

        // Water on electrical = instant fail
        if (ext == GameManager.ExtType.Water &&
            sc == GameManager.Scenario.ServerRoom &&
            !alreadyTriggeredInstantFail)
        {
            alreadyTriggeredInstantFail = true;
            StopFire();
            GameManager.Instance.InstantFail("DANGER - Electrocution hazard!\nNever use water on electrical fires.");
            return;
        }

        float suppressRate = 0f;
        if (sc == GameManager.Scenario.Kitchen)
        {
            if (ext == GameManager.ExtType.DCP) suppressRate = 0.25f;
            else if (ext == GameManager.ExtType.CO2) suppressRate = 0.03f;
            else if (ext == GameManager.ExtType.Water) suppressRate = 0.01f;
        }
        else if (sc == GameManager.Scenario.ServerRoom)
        {
            if (ext == GameManager.ExtType.CO2) suppressRate = 0.25f;
            else if (ext == GameManager.ExtType.DCP) suppressRate = 0.04f;
        }
        fire.ReduceFire(suppressRate);
    }
}
