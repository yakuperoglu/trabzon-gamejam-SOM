using UnityEngine;

/// <summary>
/// Gizli Obje - Sadece Maske 1 aktifken görünür
/// Parlayan animasyonlu efekt ile
/// </summary>
public class RevealableObject : MonoBehaviour
{
    [Header("Görünürlük Ayarları")]
    [Tooltip("Başlangıçta görünmez mi?")]
    public bool startHidden = true;

    [Header("Parlama Efekti")]
    [Tooltip("Parlama efekti kullan")]
    public bool useGlowEffect = true;
    
    [Tooltip("Parlama rengi")]
    public Color glowColor = new Color(0.3f, 0.7f, 1f, 1f);
    
    [Tooltip("Minimum parlama şiddeti")]
    [Range(0f, 2f)]
    public float minGlowIntensity = 0.3f;
    
    [Tooltip("Maximum parlama şiddeti")]
    [Range(0f, 5f)]
    public float maxGlowIntensity = 1.5f;
    
    [Tooltip("Parlama hızı")]
    public float pulseSpeed = 2f;

    // Bileşenler
    private Renderer[] renderers;
    private Collider[] colliders;
    private bool isRevealed = false;

    void Awake()
    {
        // Renderer ve Collider'ları topla
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
    }

    void Start()
    {
        // Başlangıçta gizle
        if (startHidden)
        {
            Hide();
        }
    }

    void OnEnable()
    {
        Mask1.OnRevealStateChanged += OnRevealStateChanged;
        
        // Mevcut duruma göre ayarla
        if (Mask1.IsRevealActive)
        {
            Reveal();
        }
        else if (startHidden)
        {
            Hide();
        }
    }

    void OnDisable()
    {
        Mask1.OnRevealStateChanged -= OnRevealStateChanged;
    }

    void Update()
    {
        // Revealed ise parlama animasyonu
        if (isRevealed && useGlowEffect)
        {
            UpdateGlowEffect();
        }
    }

    void UpdateGlowEffect()
    {
        // Sin wave ile nabız efekti (0-1 arası)
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float intensity = Mathf.Lerp(minGlowIntensity, maxGlowIntensity, pulse);

        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                // MaterialPropertyBlock kullan - build'de düzgün çalışır
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(propBlock);
                
                // Emission rengi ayarla (HDR için intensity ile çarp)
                Color emissionColor = glowColor * intensity;
                propBlock.SetColor("_EmissionColor", emissionColor);
                
                renderer.SetPropertyBlock(propBlock);
                
                // Global Illumination için emission'ı güncelle
                DynamicGI.SetEmissive(renderer, emissionColor);
            }
        }
    }

    void OnRevealStateChanged(bool revealed)
    {
        if (revealed)
        {
            Reveal();
        }
        else if (startHidden)
        {
            Hide();
        }
    }

    public void Reveal()
    {
        isRevealed = true;

        // Renderer'ları göster ve emission'ı aktifleştir
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
                
                // Emission keyword'ü aktifleştir (build için gerekli)
                foreach (var mat in renderer.materials)
                {
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        mat.EnableKeyword("_EMISSION");
                        mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    }
                }
            }
        }

        // Collider'ları aktifleştir
        foreach (var col in colliders)
        {
            if (col != null)
            {
                col.enabled = true;
            }
        }
    }

    public void Hide()
    {
        isRevealed = false;

        // Emission'ı kapat
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                foreach (var mat in renderer.materials)
                {
                    if (mat.HasProperty("_EmissionColor"))
                    {
                        mat.SetColor("_EmissionColor", Color.black);
                    }
                }
                renderer.enabled = false;
            }
        }

        // Collider'ları deaktifleştir
        foreach (var col in colliders)
        {
            if (col != null)
            {
                col.enabled = false;
            }
        }
    }

    void OnDestroy()
    {
        Mask1.OnRevealStateChanged -= OnRevealStateChanged;
    }
}
