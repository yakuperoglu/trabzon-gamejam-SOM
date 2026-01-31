using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Envanter UI - Image objelerini SetActive ile gösterir
/// Eşya toplandığında ilgili Image aktif olur
/// Seçili slot Outline efekti ile gösterilir
/// </summary>
public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("UI Container")]
    [Tooltip("Ana envanter paneli - loading sırasında gizlenecek (opsiyonel)")]
    public GameObject inventoryPanel;

    [Header("Maske Image Objeleri")]
    [Tooltip("Mask1 Image objesi - Başta inactive")]
    public GameObject mask1Image;
    
    [Tooltip("Mask2 Image objesi - Başta inactive")]
    public GameObject mask2Image;
    
    [Tooltip("Mask3 Image objesi - Başta inactive")]
    public GameObject mask3Image;
    
    [Header("Anahtar Image Objesi")]
    [Tooltip("Key Image objesi - Başta inactive")]
    public GameObject keyImage;

    [Header("Seçim Outline Ayarları")]
    [Tooltip("Outline rengi")]
    public Color outlineColor = new Color(1f, 0.84f, 0f, 1f); // Altın sarısı
    
    [Tooltip("Outline kalınlığı")]
    [Range(1f, 10f)]
    public float outlineThickness = 3f;

    [Header("Seçim Çerçeveleri (Opsiyonel - Outline kullanılmazsa)")]
    [Tooltip("Aktif maske için çerçeve objeleri")]
    public GameObject[] selectionFrames = new GameObject[3];

    // Private
    private InventorySystem inventory;
    private Outline[] maskOutlines = new Outline[3];

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

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

        // Outline'ları kur
        SetupOutlines();

        // Event'lere bağlan
        inventory.OnMaskCollected.AddListener(OnMaskCollected);
        inventory.OnKeyCollected.AddListener(OnKeyCollected);
        inventory.OnMaskActivated.AddListener(OnMaskActivated);
        inventory.OnMaskDeactivated.AddListener(OnMaskDeactivated);

        // İlk durumu ayarla (mevcut envantere göre)
        RefreshUI();
    }

    /// <summary>
    /// Maske Image'larına Outline component ekle
    /// </summary>
    void SetupOutlines()
    {
        GameObject[] maskImages = { mask1Image, mask2Image, mask3Image };
        
        for (int i = 0; i < maskImages.Length; i++)
        {
            if (maskImages[i] != null)
            {
                // Outline component var mı kontrol et, yoksa ekle
                Outline outline = maskImages[i].GetComponent<Outline>();
                if (outline == null)
                {
                    outline = maskImages[i].AddComponent<Outline>();
                }
                
                // Outline ayarlarını yap
                outline.effectColor = outlineColor;
                outline.effectDistance = new Vector2(outlineThickness, outlineThickness);
                outline.enabled = false; // Başlangıçta kapalı
                
                maskOutlines[i] = outline;
            }
        }
    }

    void RefreshUI()
    {
        if (inventory == null) return;

        // Maske görünürlüklerini güncelle
        if (mask1Image != null) mask1Image.SetActive(inventory.hasMask[0]);
        if (mask2Image != null) mask2Image.SetActive(inventory.hasMask[1]);
        if (mask3Image != null) mask3Image.SetActive(inventory.hasMask[2]);
        
        // Anahtar görünürlüğü
        if (keyImage != null) keyImage.SetActive(inventory.hasKey);

        // Seçim efektlerini güncelle
        UpdateSelection();
    }

    /// <summary>
    /// Envanter UI'ını gizle (loading sırasında)
    /// </summary>
    public void Hide()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
        else
        {
            // Panel yoksa direkt objeyi gizle
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Envanter UI'ını göster (yeni seviye başlayınca)
    /// </summary>
    public void Show()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }
        else
        {
            gameObject.SetActive(true);
        }
        
        // Inventory referansını yeniden al (yeni sahnedeki için)
        inventory = InventorySystem.Instance;
        RefreshUI();
    }

    /// <summary>
    /// Seçili maskeyi outline veya frame ile göster
    /// </summary>
    void UpdateSelection()
    {
        if (inventory == null) return;

        // Outline'ları güncelle
        for (int i = 0; i < maskOutlines.Length; i++)
        {
            if (maskOutlines[i] != null)
            {
                maskOutlines[i].enabled = (inventory.activeMaskIndex == i);
            }
        }

        // Opsiyonel: Frame'leri de güncelle (eğer kullanılıyorsa)
        UpdateSelectionFrames();
    }

    void UpdateSelectionFrames()
    {
        if (inventory == null) return;

        for (int i = 0; i < selectionFrames.Length; i++)
        {
            if (selectionFrames[i] != null)
            {
                selectionFrames[i].SetActive(inventory.activeMaskIndex == i);
            }
        }
    }

    // Event Handlers
    void OnMaskCollected(int maskIndex)
    {
        switch (maskIndex)
        {
            case 0:
                if (mask1Image != null) mask1Image.SetActive(true);
                Debug.Log("UI: Mask1 gösteriliyor");
                break;
            case 1:
                if (mask2Image != null) mask2Image.SetActive(true);
                Debug.Log("UI: Mask2 gösteriliyor");
                break;
            case 2:
                if (mask3Image != null) mask3Image.SetActive(true);
                Debug.Log("UI: Mask3 gösteriliyor");
                break;
        }
    }

    void OnKeyCollected()
    {
        if (keyImage != null) keyImage.SetActive(true);
        Debug.Log("UI: Key gösteriliyor");
    }

    void OnMaskActivated(int maskIndex)
    {
        UpdateSelection();
        Debug.Log($"UI: Mask{maskIndex + 1} seçildi - Outline aktif");
    }

    void OnMaskDeactivated(int maskIndex)
    {
        UpdateSelection();
        Debug.Log($"UI: Mask{maskIndex + 1} seçimi kaldırıldı");
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

