using UnityEngine;

/// <summary>
/// Maske 3 - Işık Maskesi (Flashlight)
/// Aktifken baktığımız yönü aydınlatan bir spot ışık oluşturur
/// </summary>
public class Mask3 : MaskBase
{
    public override string MaskName => "Işık Maskesi";
    public override int MaskIndex => 2;

    [Header("Işık Ayarları")]
    [Tooltip("Işık rengi")]
    public Color lightColor = Color.white;
    
    [Tooltip("Işık şiddeti")]
    [Range(0.1f, 500f)]
    public float lightIntensity = 200f;
    
    [Tooltip("Işık menzili")]
    [Range(1f, 150f)]
    public float lightRange = 100f;
    
    [Tooltip("Spot açısı")]
    [Range(1f, 179f)]
    public float spotAngle = 75f;
    
    [Tooltip("İç spot açısı (yumuşak kenar için)")]
    [Range(0f, 100f)]
    public float innerSpotAngle = 45f;

    [Header("Gölge Ayarları")]
    [Tooltip("Gölge oluştursun mu?")]
    public bool castShadows = true;
    
    [Tooltip("Gölge kalitesi")]
    public LightShadows shadowType = LightShadows.Soft;

    [Header("Pozisyon")]
    [Tooltip("Işık kameradan ne kadar ileride olsun")]
    public float forwardOffset = 0.2f;

    // Private
    private Light flashlight;
    private Camera playerCamera;

    void Start()
    {
        playerCamera = Camera.main;
    }

    protected override void OnActivate()
    {
        // Kamerayı bul
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        if (playerCamera == null) return;

        // Işık oluştur
        GameObject lightObj = new GameObject("Mask3_Flashlight");
        lightObj.transform.SetParent(playerCamera.transform);
        lightObj.transform.localPosition = Vector3.forward * forwardOffset;
        lightObj.transform.localRotation = Quaternion.identity;

        flashlight = lightObj.AddComponent<Light>();
        flashlight.type = LightType.Spot;
        
        // Ayarları uygula
        ApplyLightSettings();
    }

    void ApplyLightSettings()
    {
        if (flashlight == null) return;

        flashlight.color = lightColor;
        flashlight.intensity = lightIntensity;
        flashlight.range = lightRange;
        flashlight.spotAngle = spotAngle;
        flashlight.innerSpotAngle = innerSpotAngle;
        
        if (castShadows)
        {
            flashlight.shadows = shadowType;
        }
        else
        {
            flashlight.shadows = LightShadows.None;
        }
    }

    protected override void OnDeactivate()
    {
        // Işığı yok et
        if (flashlight != null)
        {
            Destroy(flashlight.gameObject);
            flashlight = null;
        }
    }

    void Update()
    {
        // Aktifken ayarları canlı güncelle (Inspector'dan değiştirince)
        if (IsActive && flashlight != null)
        {
            ApplyLightSettings();
        }
    }

    void OnDestroy()
    {
        // Temizlik
        if (flashlight != null)
        {
            Destroy(flashlight.gameObject);
        }
    }
}

