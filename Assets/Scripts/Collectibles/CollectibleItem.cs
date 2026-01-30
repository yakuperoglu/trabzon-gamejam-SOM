using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Toplanabilir Eşya - Maskeler ve Anahtar için
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
    
    [Tooltip("Toplama mesafesi")]
    public float collectRange = 2f;
    
    [Tooltip("Toplama için E tuşuna basılı tutma süresi (saniye)")]
    public float holdDuration = 0.5f;

    [Header("Görsel Efektler")]
    [Tooltip("Dönen animasyon hızı")]
    public float rotationSpeed = 50f;
    
    [Tooltip("Yukarı-aşağı hareket genliği")]
    public float bobAmplitude = 0.2f;
    
    [Tooltip("Yukarı-aşağı hareket hızı")]
    public float bobSpeed = 2f;

    [Header("UI İpucu")]
    [Tooltip("Yakındayken gösterilecek ipucu metni (opsiyonel)")]
    public GameObject pickupPromptUI;

    // Private
    private Transform playerTransform;
    private PlayerInput playerInput;
    private InputAction interactAction;
    private bool playerInRange = false;
    private float holdTimer = 0f;
    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
        
        // Player'ı bul
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerInput = player.GetComponent<PlayerInput>();
            
            if (playerInput != null && playerInput.actions != null)
            {
                interactAction = playerInput.actions["Interact"];
            }
        }

        // UI'ı başlangıçta gizle
        if (pickupPromptUI != null)
        {
            pickupPromptUI.SetActive(false);
        }
    }

    void Update()
    {
        // Görsel efektler - dönen ve yukarı-aşağı hareket
        AnimateItem();
        
        // Mesafe kontrolü
        CheckPlayerDistance();
        
        // Toplama kontrolü
        if (playerInRange)
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

    void CheckPlayerDistance()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        bool wasInRange = playerInRange;
        playerInRange = distance <= collectRange;

        // UI ipucu göster/gizle
        if (pickupPromptUI != null)
        {
            pickupPromptUI.SetActive(playerInRange);
        }

        // Menzilden çıktıysa timer'ı sıfırla
        if (wasInRange && !playerInRange)
        {
            holdTimer = 0f;
        }
    }

    void HandlePickup()
    {
        bool isHolding = false;

        // Input System ile kontrol
        if (interactAction != null)
        {
            isHolding = interactAction.IsPressed();
        }
        // Fallback - Keyboard
        else if (Keyboard.current != null)
        {
            isHolding = Keyboard.current.eKey.isPressed;
        }

        if (isHolding)
        {
            holdTimer += Time.deltaTime;
            
            if (holdTimer >= holdDuration)
            {
                Collect();
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    void Collect()
    {
        if (InventorySystem.Instance == null)
        {
            Debug.LogError("CollectibleItem: InventorySystem bulunamadı!");
            return;
        }

        switch (itemType)
        {
            case ItemType.Mask1:
                InventorySystem.Instance.CollectMask(0);
                break;
            case ItemType.Mask2:
                InventorySystem.Instance.CollectMask(1);
                break;
            case ItemType.Mask3:
                InventorySystem.Instance.CollectMask(2);
                break;
            case ItemType.Key:
                InventorySystem.Instance.CollectKey();
                break;
        }

        // Objeyi yok et
        Debug.Log($"{itemType} toplandı ve envantere eklendi!");
        Destroy(gameObject);
    }

    // Trigger ile de çalışabilir (alternatif)
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (pickupPromptUI != null)
            {
                pickupPromptUI.SetActive(true);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            holdTimer = 0f;
            if (pickupPromptUI != null)
            {
                pickupPromptUI.SetActive(false);
            }
        }
    }

    // Editor'da menzili göster
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectRange);
    }
}
