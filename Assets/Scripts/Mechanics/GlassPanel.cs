using UnityEngine;

/// <summary>
/// Cam köprü paneli - Squid Game benzeri mekanik
/// Doğru panel güvenli, yanlış panel oyuncuyu düşürür
/// Mask1 aktifken doğru paneller parlar
/// </summary>
public class GlassPanel : MonoBehaviour
{
    [Header("Panel Ayarları")]
    [Tooltip("Bu panel güvenli mi?")]
    public bool isSafe = false;
    
    [Tooltip("Yanlış panele basınca düşme gecikmesi")]
    public float breakDelay = 0.1f;
    
    [Tooltip("Düştükten sonra ölme gecikmesi")]
    public float deathDelay = 1f;

    [Header("Parlama Efekti (Mask1)")]
    public Color safeGlowColor = new Color(0.2f, 1f, 0.3f, 1f);
    
    [Range(0f, 2f)]
    public float minGlowIntensity = 0.3f;
    
    [Range(0f, 5f)]
    public float maxGlowIntensity = 2f;
    
    public float pulseSpeed = 2f;

    // Private
    private Renderer panelRenderer;
    private Collider panelCollider;
    private bool isGlowing = false;
    private bool isBroken = false;
    private GlassBridgeManager bridgeManager;
    private PlayerHealth playerToKill;

    void Awake()
    {
        panelRenderer = GetComponent<Renderer>();
        panelCollider = GetComponent<Collider>();
    }

    void OnEnable()
    {
        Mask1.OnRevealStateChanged += OnRevealStateChanged;
        
        if (Mask1.IsRevealActive && isSafe)
        {
            StartGlow();
        }
    }

    void OnDisable()
    {
        Mask1.OnRevealStateChanged -= OnRevealStateChanged;
    }

    void Update()
    {
        if (isGlowing && isSafe && !isBroken)
        {
            UpdateGlowEffect();
        }
    }

    void OnRevealStateChanged(bool revealed)
    {
        if (revealed && isSafe)
        {
            StartGlow();
        }
        else
        {
            StopGlow();
        }
    }

    void StartGlow()
    {
        isGlowing = true;
    }

    void StopGlow()
    {
        isGlowing = false;
        
        if (panelRenderer != null)
        {
            foreach (var mat in panelRenderer.materials)
            {
                if (mat.HasProperty("_EmissionColor"))
                {
                    mat.SetColor("_EmissionColor", Color.black);
                }
            }
        }
    }

    void UpdateGlowEffect()
    {
        if (panelRenderer == null) return;

        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float intensity = Mathf.Lerp(minGlowIntensity, maxGlowIntensity, pulse);

        foreach (var mat in panelRenderer.materials)
        {
            if (mat.HasProperty("_EmissionColor"))
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", safeGlowColor * intensity);
            }
        }
    }

    public void SetBridgeManager(GlassBridgeManager manager)
    {
        bridgeManager = manager;
    }

    public void SetSafe(bool safe)
    {
        isSafe = safe;
        isBroken = false;
        playerToKill = null;
        
        if (panelCollider != null)
        {
            panelCollider.enabled = true;
            
            // Yanlış paneller trigger olsun - oyuncu direkt düşsün
            panelCollider.isTrigger = !isSafe;
        }
        
        // Mevcut mask durumuna göre glow
        if (Mask1.IsRevealActive && isSafe)
        {
            StartGlow();
        }
        else
        {
            StopGlow();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isBroken) return;
        
        // Player mı kontrol et
        PlayerHealth health = collision.gameObject.GetComponent<PlayerHealth>();
        if (health != null || collision.gameObject.CompareTag("Player"))
        {
            if (!isSafe)
            {
                playerToKill = health;
                BreakPanel();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isBroken) return;
        
        // Player mı kontrol et
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null || other.CompareTag("Player"))
        {
            if (!isSafe)
            {
                playerToKill = health;
                BreakPanel();
            }
        }
    }

    void BreakPanel()
    {
        if (isBroken) return;
        isBroken = true;

        // Collider'ı devre dışı bırak (oyuncu düşsün)
        Invoke(nameof(DisableCollider), breakDelay);
        
        // Manager'a haber ver
        if (bridgeManager != null)
        {
            bridgeManager.OnPanelBroken(this);
        }
    }

    void DisableCollider()
    {
        if (panelCollider != null)
        {
            panelCollider.enabled = false;
        }
    }

    public void ResetPanel()
    {
        isBroken = false;
        
        if (panelCollider != null)
        {
            panelCollider.enabled = true;
        }
        
        StopGlow();
    }
}
