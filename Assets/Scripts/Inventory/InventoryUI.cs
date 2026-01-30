using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Envanter UI - Image objelerini SetActive ile gösterir
/// Eşya toplandığında ilgili Image aktif olur
/// </summary>
public class InventoryUI : MonoBehaviour
{
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

    [Header("Seçim Çerçeveleri (Opsiyonel)")]
    [Tooltip("Aktif maske için çerçeve objeleri")]
    public GameObject[] selectionFrames = new GameObject[3];

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

        // İlk durumu ayarla (mevcut envantere göre)
        RefreshUI();
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

        // Seçim çerçevelerini güncelle
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
        UpdateSelectionFrames();
    }

    void OnMaskDeactivated(int maskIndex)
    {
        UpdateSelectionFrames();
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

