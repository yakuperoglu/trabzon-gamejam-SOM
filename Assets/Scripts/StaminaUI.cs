using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Stamina UI - Slider veya Image ile stamina gösterimi
/// 3 saniye dolu kalırsa fade out, enerji harcanınca fade in
/// </summary>
public class StaminaUI : MonoBehaviour
{
    [Header("Player Referansı")]
    [Tooltip("Player objesini buraya sürükle")]
    public PlayerMovement playerMovement;

    [Header("UI Elemanları (birini seç)")]
    [Tooltip("Slider kullanıyorsan buraya sürükle")]
    public Slider staminaSlider;
    
    [Tooltip("Image kullanıyorsan buraya sürükle (Image Type = Filled olmalı)")]
    public Image staminaFillImage;

    [Header("Opsiyonel - Renkler")]
    [Tooltip("Renk değişimi istiyorsan işaretle")]
    public bool useColorChange = true;
    
    public Color fullColor = Color.green;
    public Color emptyColor = Color.red;

    [Header("Auto-Hide Ayarları")]
    [Tooltip("Stamina dolu kaldığında kaybolma süresi (saniye)")]
    public float hideDelay = 3f;
    
    [Tooltip("Fade süresi")]
    public float fadeDuration = 0.5f;

    // Private
    private CanvasGroup canvasGroup;
    private float fullStaminaTime;
    private bool isHidden;
    private float targetAlpha = 1f;

    void Start()
    {
        // Player'ı otomatik bul
        if (playerMovement == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMovement = player.GetComponent<PlayerMovement>();
            }
        }

        if (playerMovement == null)
        {
            Debug.LogError("StaminaUI: PlayerMovement bulunamadı! Player objesini sürükle.");
            return;
        }

        // CanvasGroup ekle (fade için)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Slider ayarları - soldan sağa dolu, enerji azaldıkça sağdan sola küçülür
        if (staminaSlider != null)
        {
            staminaSlider.direction = Slider.Direction.LeftToRight;
            staminaSlider.minValue = 0f;
            staminaSlider.maxValue = 1f;
            staminaSlider.value = 1f; // Başlangıçta dolu
        }

        // Event'e bağlan
        playerMovement.OnStaminaChanged.AddListener(UpdateUI);
        
        // Başlangıç
        fullStaminaTime = Time.time;
        UpdateUI(playerMovement.CurrentStamina, playerMovement.MaxStaminaValue);
    }

    void Update()
    {
        // Fade animasyonu
        if (canvasGroup != null)
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, Time.deltaTime / fadeDuration);
        }

        // Stamina tam doluysa ve bekleme süresi geçtiyse gizle
        if (playerMovement != null && playerMovement.CurrentStamina >= playerMovement.MaxStaminaValue)
        {
            if (Time.time - fullStaminaTime >= hideDelay && !isHidden)
            {
                targetAlpha = 0f;
                isHidden = true;
            }
        }
    }

    void UpdateUI(float current, float max)
    {
        float percent = current / max;

        // Stamina tam dolu değilse göster
        if (percent < 1f)
        {
            targetAlpha = 1f;
            isHidden = false;
            fullStaminaTime = Time.time; // Timer'ı sıfırla
        }
        else
        {
            // Tam dolu, timer başlasın
            if (!isHidden)
            {
                fullStaminaTime = Time.time;
            }
        }

        // Slider varsa güncelle
        if (staminaSlider != null)
        {
            staminaSlider.value = percent;
            
            if (useColorChange && staminaSlider.fillRect != null)
            {
                Image fill = staminaSlider.fillRect.GetComponent<Image>();
                if (fill != null)
                {
                    fill.color = Color.Lerp(emptyColor, fullColor, percent);
                }
            }
        }

        // Image varsa güncelle
        if (staminaFillImage != null)
        {
            staminaFillImage.fillAmount = percent;
            
            if (useColorChange)
            {
                staminaFillImage.color = Color.Lerp(emptyColor, fullColor, percent);
            }
        }
    }

    void OnDestroy()
    {
        if (playerMovement != null)
        {
            playerMovement.OnStaminaChanged.RemoveListener(UpdateUI);
        }
    }
}
