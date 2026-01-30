using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Stamina UI - Slider veya Image ile stamina gösterimi
/// Inspector'dan UI elemanlarını sürükle bırak
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
        
        // Başlangıç değerini göster
        UpdateUI(playerMovement.CurrentStamina, playerMovement.MaxStaminaValue);
    }

    void UpdateUI(float current, float max)
    {
        float percent = current / max;

        // Slider varsa güncelle
        if (staminaSlider != null)
        {
            // Unity slider direction'ı kendisi halleder
            // Sadece 0-1 arası değer veriyoruz (0 = boş, 1 = dolu)
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
                staminaFillImage.fillAmount = percent;
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
