using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Gizli Düğme - Bastığında geri gider ve kapıyı açar
/// </summary>
public class SecretButton : MonoBehaviour
{
    [Header("Düğme Ayarları")]
    [Tooltip("Düğmenin geri gitme mesafesi")]
    public float pressDistance = 0.1f;
    
    [Tooltip("Düğmenin hareket hızı")]
    public float pressSpeed = 3f;
    
    [Tooltip("Etkileşim mesafesi")]
    public float interactRange = 3f;

    [Header("Kapı Referansı")]
    [Tooltip("Bu düğmenin açtığı kapı")]
    public DoorController door;

    [Header("UI İpucu")]
    public GameObject interactPromptUI;

    // Private
    private Vector3 startPosition;
    private Vector3 pressedPosition;
    private bool isPressed = false;
    private bool isLooking = false;
    private Camera playerCamera;

    void Start()
    {
        startPosition = transform.localPosition;
        pressedPosition = startPosition - transform.forward * pressDistance;
        
        playerCamera = Camera.main;
        
        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(false);
        }
    }

    void Update()
    {
        CheckPlayerLooking();
        
        if (isLooking && !isPressed)
        {
            HandleInteraction();
        }
        
        AnimateButton();
    }

    void CheckPlayerLooking()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) return;
        }

        bool wasLooking = isLooking;
        isLooking = false;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                isLooking = true;
            }
        }

        // UI kontrolü - sadece bakılan buton aktif etsin
        if (interactPromptUI != null)
        {
            if (isLooking && !isPressed)
            {
                currentActiveButton = this;
                interactPromptUI.SetActive(true);
            }
            else if (wasLooking && currentActiveButton == this)
            {
                currentActiveButton = null;
                interactPromptUI.SetActive(false);
            }
        }
    }

    // Hangi buton şu an UI'ı kontrol ediyor
    private static SecretButton currentActiveButton;

    void HandleInteraction()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PressButton();
        }
    }

    void PressButton()
    {
        if (isPressed) return;
        
        isPressed = true;
        
        // Kapıyı aç
        if (door != null)
        {
            door.OpenDoor();
        }
        
        // UI'ı gizle
        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(false);
        }
    }

    void AnimateButton()
    {
        Vector3 targetPosition = isPressed ? pressedPosition : startPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * pressSpeed);
    }

    // Düğmeyi resetle (opsiyonel)
    public void ResetButton()
    {
        isPressed = false;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
