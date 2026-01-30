using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Envanter UI - Ekranın alt ortasında 4 slotlu envanter gösterimi
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("Slot Referansları")]
    [Tooltip("Slot arka plan Image'ları (4 adet)")]
    public Image[] slotBackgrounds = new Image[4];
    
    [Tooltip("Slot ikon Image'ları (4 adet)")]
    public Image[] slotIcons = new Image[4];
    
    [Tooltip("Slot seçim çerçeveleri (3 adet - maskeler için)")]
    public Image[] slotSelectionFrames = new Image[3];
    
    [Tooltip("Slot numaraları (opsiyonel - TextMeshPro)")]
    public TextMeshProUGUI[] slotNumbers = new TextMeshProUGUI[4];

    [Header("Renkler")]
    [Tooltip("Boş slot rengi")]
    public Color emptySlotColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
    
    [Tooltip("Dolu slot rengi")]
    public Color filledSlotColor = new Color(1f, 1f, 1f, 1f);
    
    [Tooltip("Aktif maske çerçeve rengi")]
    public Color activeFrameColor = new Color(1f, 0.8f, 0f, 1f); // Altın sarısı
    
    [Tooltip("Pasif çerçeve rengi")]
    public Color inactiveFrameColor = new Color(0.5f, 0.5f, 0.5f, 0f); // Şeffaf

    [Header("Opsiyonel - Sprite'lar")]
    [Tooltip("Maske ikonları (3 adet)")]
    public Sprite[] maskIcons = new Sprite[3];
    
    [Tooltip("Anahtar ikonu")]
    public Sprite keyIcon;

    // Private
    private InventorySystem inventory;

    void Start()
    {
        // InventorySystem'i bul
        inventory = InventorySystem.Instance;
        if (inventory == null)
        {
            inventory = FindAnyObjectByType<InventorySystem>();
        }

        if (inventory == null)
        {
            Debug.LogError("InventoryUI: InventorySystem bulunamadı!");
            return;
        }

        // Event'lere bağlan
        inventory.OnMaskCollected.AddListener(OnMaskCollected);
        inventory.OnKeyCollected.AddListener(OnKeyCollected);
        inventory.OnMaskActivated.AddListener(OnMaskActivated);
        inventory.OnMaskDeactivated.AddListener(OnMaskDeactivated);

        // İlk UI güncellemesi
        RefreshUI();
    }

    void RefreshUI()
    {
        if (inventory == null) return;

        // Maske slotlarını güncelle (0, 1, 2)
        for (int i = 0; i < 3; i++)
        {
            UpdateSlot(i, inventory.hasMask[i]);
            UpdateSelectionFrame(i, inventory.activeMaskIndex == i);
        }

        // Anahtar slotunu güncelle (3)
        UpdateSlot(3, inventory.hasKey);
    }

    void UpdateSlot(int slotIndex, bool hasItem)
    {
        if (slotIndex < 0 || slotIndex >= 4) return;

        // Arka plan rengi
        if (slotBackgrounds != null && slotBackgrounds.Length > slotIndex && slotBackgrounds[slotIndex] != null)
        {
            slotBackgrounds[slotIndex].color = hasItem ? filledSlotColor : emptySlotColor;
        }

        // İkon görünürlüğü ve sprite
        if (slotIcons != null && slotIcons.Length > slotIndex && slotIcons[slotIndex] != null)
        {
            slotIcons[slotIndex].enabled = hasItem;
            
            // Sprite ata
            if (hasItem)
            {
                if (slotIndex < 3 && maskIcons != null && maskIcons.Length > slotIndex && maskIcons[slotIndex] != null)
                {
                    slotIcons[slotIndex].sprite = maskIcons[slotIndex];
                }
                else if (slotIndex == 3 && keyIcon != null)
                {
                    slotIcons[slotIndex].sprite = keyIcon;
                }
            }
        }
    }

    void UpdateSelectionFrame(int maskIndex, bool isActive)
    {
        if (maskIndex < 0 || maskIndex >= 3) return;
        
        if (slotSelectionFrames != null && slotSelectionFrames.Length > maskIndex && slotSelectionFrames[maskIndex] != null)
        {
            slotSelectionFrames[maskIndex].color = isActive ? activeFrameColor : inactiveFrameColor;
        }
    }

    // Event Handlers
    void OnMaskCollected(int maskIndex)
    {
        UpdateSlot(maskIndex, true);
    }

    void OnKeyCollected()
    {
        UpdateSlot(3, true);
    }

    void OnMaskActivated(int maskIndex)
    {
        // Tüm çerçeveleri güncelle
        for (int i = 0; i < 3; i++)
        {
            UpdateSelectionFrame(i, i == maskIndex);
        }
    }

    void OnMaskDeactivated(int maskIndex)
    {
        UpdateSelectionFrame(maskIndex, false);
    }

    void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnMaskCollected.RemoveListener(OnMaskCollected);
            inventory.OnKeyCollected.RemoveListener(OnKeyCollected);
            inventory.OnMaskActivated.RemoveListener(OnMaskActivated);
            inventory.OnMaskDeactivated.RemoveListener(OnMaskDeactivated);
        }
    }
}
