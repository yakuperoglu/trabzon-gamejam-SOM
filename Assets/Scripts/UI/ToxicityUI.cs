using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Toksisite Bar UI - Ekranın üstünde toksisite gösterimi
/// Slider kullan, Image değil
/// </summary>
public class ToxicityUI : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Player Health referansı (otomatik bulunur)")]
    public PlayerHealth playerHealth;

    [Header("UI Elemanları")]
    [Tooltip("Toksisite bar Slider")]
    public Slider toxicitySlider;

    [Header("Renk Ayarları")]
    [Tooltip("Slider Fill Image (renk değişimi için)")]
    public Image sliderFillImage;
    
    public Color healthyColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;
    
    [Range(0f, 1f)]
    public float warningThreshold = 0.5f;
    [Range(0f, 1f)]
    public float dangerThreshold = 0.25f;

    void Start()
    {
        // PlayerHealth'i bul
        if (playerHealth == null)
        {
            playerHealth = PlayerHealth.Instance;
            if (playerHealth == null)
            {
                playerHealth = FindAnyObjectByType<PlayerHealth>();
            }
        }

        // Slider ayarla
        if (toxicitySlider != null)
        {
            toxicitySlider.minValue = 0f;
            toxicitySlider.maxValue = 1f;
            toxicitySlider.value = 1f; // Başlangıçta full
        }

        // Event'e bağlan
        if (playerHealth != null)
        {
            playerHealth.OnToxicityChanged.AddListener(UpdateUI);
        }
    }

    void UpdateUI(float toxicityPercent)
    {
        // Slider güncelle
        if (toxicitySlider != null)
        {
            toxicitySlider.value = toxicityPercent;
        }

        // Renk değiştir - yumuşak geçiş
        if (sliderFillImage != null)
        {
            Color targetColor;
            
            if (toxicityPercent <= dangerThreshold)
            {
                // Tehlikeli bölge - kırmızı
                targetColor = dangerColor;
            }
            else if (toxicityPercent <= warningThreshold)
            {
                // Warning bölgesi - kırmızı ile sarı arası
                float t = (toxicityPercent - dangerThreshold) / (warningThreshold - dangerThreshold);
                targetColor = Color.Lerp(dangerColor, warningColor, t);
            }
            else
            {
                // Sağlıklı bölge - sarı ile yeşil arası
                float t = (toxicityPercent - warningThreshold) / (1f - warningThreshold);
                targetColor = Color.Lerp(warningColor, healthyColor, t);
            }
            
            sliderFillImage.color = targetColor;
        }
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnToxicityChanged.RemoveListener(UpdateUI);
        }
    }
}
