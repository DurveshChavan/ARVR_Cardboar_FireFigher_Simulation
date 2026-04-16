using UnityEngine;

/// <summary>
/// Controls fire behavior: scales particle emission with fireScale,
/// changes color as fire shrinks, triggers Mission Complete at 0.05.
/// Attach to the GameObject that has (or whose child has) a ParticleSystem.
/// </summary>
public class FireController : MonoBehaviour
{
    [Header("Fire State")]
    public float fireScale = 1f;

    [Header("Visuals")]
    [Tooltip("Starting emission rate at full fire.")]
    public float maxEmissionRate = 40f;
    [Tooltip("Min emission rate just before extinguished.")]
    public float minEmissionRate = 3f;

    private ParticleSystem ps;
    private ParticleSystem.EmissionModule emission;
    private ParticleSystem.MainModule mainModule;
    private bool hasParticles = false;

    // Original colors
    private Color fullFireStartColor = new Color(1f, 0.6f, 0f, 1f);     // Orange flame
    private Color dyingFireStartColor = new Color(0.5f, 0.1f, 0f, 0.6f); // Dark red, translucent

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null) ps = GetComponentInChildren<ParticleSystem>();

        if (ps != null)
        {
            hasParticles = true;
            emission = ps.emission;
            mainModule = ps.main;

            // Set initial emission rate
            emission.rateOverTime = maxEmissionRate;
            Debug.Log($"[FireController] Ready on '{gameObject.name}' — emission={maxEmissionRate}");
        }
        else
        {
            Debug.LogWarning($"[FireController] No ParticleSystem found on '{gameObject.name}'");
        }
    }

    void Update()
    {
        // Scale the fire visually
        transform.localScale = Vector3.one * Mathf.Max(fireScale, 0.01f);

        // Update particle emission rate to match fire scale
        if (hasParticles)
        {
            float rate = Mathf.Lerp(minEmissionRate, maxEmissionRate, fireScale);
            emission.rateOverTime = rate;

            // Interpolate color: orange at full → dark red when dying
            Color currentColor = Color.Lerp(dyingFireStartColor, fullFireStartColor, fireScale);
            mainModule.startColor = currentColor;

            // Scale particle size with fire
            mainModule.startSizeMultiplier = Mathf.Lerp(0.1f, 1f, fireScale);

            // Stop particles when fully extinguished
            if (fireScale <= 0.05f && ps.isPlaying)
            {
                ps.Stop();
            }
        }

        // Notify GameManager — it will check if ALL fires are out before declaring win
        if (fireScale <= 0.05f && GameManager.Instance != null && GameManager.Instance.gameActive)
        {
            GameManager.Instance.NotifyFireExtinguished(this);
        }
    }

    /// <summary>
    /// Reduces fire by a rate (called by ExtinguisherShooter every frame while spraying).
    /// </summary>
    public void ReduceFire(float rate)
    {
        fireScale -= rate * Time.deltaTime;
        fireScale = Mathf.Max(0f, fireScale);
    }
}
