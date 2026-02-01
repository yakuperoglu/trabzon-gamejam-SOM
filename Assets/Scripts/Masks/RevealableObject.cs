using UnityEngine;

/// <summary>
/// Gizli Obje - Sadece Maske 1 aktifken görünür
/// Point Light ve Scale Pulse ile garantili parlama efekti
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
    
    [Tooltip("Parlama hızı")]
    public float pulseSpeed = 2f;

    [Header("Işık Efekti (Kesin Çalışır)")]
    [Tooltip("Point Light ile parlama")]
    public bool usePointLight = true;
    
    [Tooltip("Işık şiddeti minimum")]
    [Range(0f, 5f)]
    public float minLightIntensity = 0.5f;
    
    [Tooltip("Işık şiddeti maximum")]
    [Range(0f, 10f)]
    public float maxLightIntensity = 3f;
    
    [Tooltip("Işık menzili")]
    [Range(1f, 20f)]
    public float lightRange = 5f;

    [Header("Scale Pulse Efekti")]
    [Tooltip("Scale pulse kullan")]
    public bool useScalePulse = true;
    
    [Tooltip("Minimum scale çarpanı")]
    [Range(0.8f, 1f)]
    public float minScale = 0.95f;
    
    [Tooltip("Maximum scale çarpanı")]
    [Range(1f, 1.5f)]
    public float maxScale = 1.1f;

    // Bileşenler
    private Renderer[] renderers;
    private Collider[] colliders;
    private bool isRevealed = false;
    
    // Efekt için
    private Light glowLight;
    private Vector3 originalScale;

    void Awake()
    {
        // Renderer ve Collider'ları topla
        renderers = GetComponentsInChildren<Renderer>();
        colliders = GetComponentsInChildren<Collider>();
        
        // Orijinal scale'i kaydet
        originalScale = transform.localScale;
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

        // Point Light efekti
        if (usePointLight && glowLight != null)
        {
            glowLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, pulse);
        }

        // Scale Pulse efekti
        if (useScalePulse)
        {
            float scaleFactor = Mathf.Lerp(minScale, maxScale, pulse);
            transform.localScale = originalScale * scaleFactor;
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

        // Renderer'ları göster
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                renderer.enabled = true;
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

        // Point Light oluştur (yoksa)
        if (usePointLight && useGlowEffect)
        {
            CreateGlowLight();
        }
    }

    void CreateGlowLight()
    {
        if (glowLight != null) return;

        // Yeni ışık objesi oluştur
        GameObject lightObj = new GameObject("RevealGlowLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.zero;

        // Point Light ekle
        glowLight = lightObj.AddComponent<Light>();
        glowLight.type = LightType.Point;
        glowLight.color = glowColor;
        glowLight.intensity = maxLightIntensity;
        glowLight.range = lightRange;
        glowLight.shadows = LightShadows.None; // Performans için
    }

    void DestroyGlowLight()
    {
        if (glowLight != null)
        {
            Destroy(glowLight.gameObject);
            glowLight = null;
        }
    }

    public void Hide()
    {
        isRevealed = false;

        // Scale'i sıfırla
        transform.localScale = originalScale;

        // Işığı kaldır
        DestroyGlowLight();

        // Renderer'ları gizle
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
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
        DestroyGlowLight();
    }
}
