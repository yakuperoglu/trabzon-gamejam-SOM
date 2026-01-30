using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Yetersiz Enerji Bildirimi - Sağ alt köşede gösterilir
/// </summary>
public class NotificationUI : MonoBehaviour
{
    [Header("Referanslar")]
    [Tooltip("Player objesini sürükle")]
    public PlayerMovement playerMovement;
    
    [Tooltip("Bildirim Text'i (TextMeshPro)")]
    public TextMeshProUGUI notificationText;
    
    [Tooltip("Bildirim Text'i (Normal Text - TMP yoksa)")]
    public Text notificationTextLegacy;

    [Header("Ayarlar")]
    [Tooltip("Bildirim görünme süresi")]
    public float displayDuration = 2f;
    
    [Tooltip("Fade out süresi")]
    public float fadeOutDuration = 0.5f;
    
    [Tooltip("Bildirim rengi")]
    public Color notificationColor = new Color(1f, 0.3f, 0.3f, 1f); // Kırmızımsı

    // Private
    private float hideTime;
    private bool isShowing;
    private CanvasGroup canvasGroup;

    void Start()
    {
        // Player bul
        if (playerMovement == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerMovement = player.GetComponent<PlayerMovement>();
            }
        }

        if (playerMovement != null)
        {
            playerMovement.OnNotEnoughStamina.AddListener(ShowNotification);
        }

        // CanvasGroup al veya ekle
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Başlangıçta gizle
        canvasGroup.alpha = 0f;
    }

    void Update()
    {
        if (isShowing && Time.time >= hideTime)
        {
            // Fade out
            canvasGroup.alpha -= Time.deltaTime / fadeOutDuration;
            
            if (canvasGroup.alpha <= 0)
            {
                isShowing = false;
                canvasGroup.alpha = 0f;
            }
        }
    }

    public void ShowNotification(string message)
    {
        // Text'i güncelle
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationText.color = notificationColor;
        }
        else if (notificationTextLegacy != null)
        {
            notificationTextLegacy.text = message;
            notificationTextLegacy.color = notificationColor;
        }

        // Göster
        canvasGroup.alpha = 1f;
        isShowing = true;
        hideTime = Time.time + displayDuration;
    }

    void OnDestroy()
    {
        if (playerMovement != null)
        {
            playerMovement.OnNotEnoughStamina.RemoveListener(ShowNotification);
        }
    }
}
