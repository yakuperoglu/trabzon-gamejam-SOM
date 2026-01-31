using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Toplanabilir Eşya - Oyuncu objeye BAKTIĞINDA toplanabilir
/// </summary>
public class CollectibleItem : MonoBehaviour
{
    public enum ItemType
    {
        Mask1,
        Mask2,
        Mask3,
        Key
    }

    [Header("Eşya Ayarları")]
    [Tooltip("Bu eşyanın tipi")]
    public ItemType itemType = ItemType.Mask1;
    
    [Tooltip("Bakış mesafesi - bu mesafeden uzakta bakınca algılanmaz")]
    public float lookRange = 5f;

    [Header("Görsel Efektler")]
    [Tooltip("Dönen animasyon hızı")]
    public float rotationSpeed = 50f;
    
    [Tooltip("Yukarı-aşağı hareket genliği")]
    public float bobAmplitude = 0.2f;
    
    [Tooltip("Yukarı-aşağı hareket hızı")]
    public float bobSpeed = 2f;

    [Header("UI İpucu")]
    [Tooltip("Objeye bakınca gösterilecek UI (opsiyonel)")]
    public GameObject pickupPromptUI;

    // Private
    private Camera playerCamera;
    private bool isLookingAtItem = false;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        
        // Kamerayı bul
        playerCamera = Camera.main;
        
        // UI'ı başlangıçta gizle
        if (pickupPromptUI != null)
        {
            pickupPromptUI.SetActive(false);
        }
    }

    void Update()
    {
        // Görsel efektler
        AnimateItem();
        
        // Bakış kontrolü (Raycast)
        CheckPlayerLooking();
        
        // Toplama kontrolü
        if (isLookingAtItem)
        {
            HandlePickup();
        }
    }

    void AnimateItem()
    {
        // Döndür
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        
        // Yukarı-aşağı hareket
        float newY = startPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    void CheckPlayerLooking()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null) return;
        }

        bool wasLooking = isLookingAtItem;
        isLookingAtItem = false;

        // Kameradan ileriye doğru ray at
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        // Raycast yap
        if (Physics.Raycast(ray, out hit, lookRange))
        {
            // Bu objeye mi bakıyor?
            if (hit.collider.gameObject == gameObject || hit.collider.transform.IsChildOf(transform))
            {
                isLookingAtItem = true;
            }
        }

        // UI göster/gizle
        if (pickupPromptUI != null)
        {
            if (isLookingAtItem)
            {
                // Bu obje şu an aktif - UI'ı göster ve sahipliği al
                currentActiveCollectible = this;
                pickupPromptUI.SetActive(true);
            }
            else if (wasLooking && currentActiveCollectible == this)
            {
                // Artık bakmıyoruz ve bu obje sahipti - UI'ı kapat
                currentActiveCollectible = null;
                pickupPromptUI.SetActive(false);
            }
        }
    }

    // Hangi collectible şu an UI'ı kontrol ediyor
    private static CollectibleItem currentActiveCollectible;

    void OnDestroy()
    {
        if (currentActiveCollectible == this && pickupPromptUI != null)
        {
            pickupPromptUI.SetActive(false);
            currentActiveCollectible = null;
        }
    }

    void OnDisable()
    {
        if (currentActiveCollectible == this && pickupPromptUI != null)
        {
            pickupPromptUI.SetActive(false);
            currentActiveCollectible = null;
        }
    }

    void HandlePickup()
    {
        // E tuşu kontrolü - Input System
        bool ePressed = false;

        if (Keyboard.current != null)
        {
            ePressed = Keyboard.current.eKey.wasPressedThisFrame;
        }

        if (ePressed)
        {
            Collect();
        }
    }

    void Collect()
    {
        // InventorySystem'i bul
        InventorySystem inventory = InventorySystem.Instance;
        
        if (inventory == null)
        {
            inventory = FindAnyObjectByType<InventorySystem>();
        }
        
        if (inventory == null) return;

        switch (itemType)
        {
            case ItemType.Mask1:
                inventory.CollectMask(0);
                break;
            case ItemType.Mask2:
                inventory.CollectMask(1);
                break;
            case ItemType.Mask3:
                inventory.CollectMask(2);
                break;
            case ItemType.Key:
                inventory.CollectKey();
                break;
        }
        
        // UI'ı gizle
        if (pickupPromptUI != null)
        {
            pickupPromptUI.SetActive(false);
        }
        
        // Objeyi yok et
        Destroy(gameObject);
    }

    // Editor'da menzili göster
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lookRange);
    }
}

