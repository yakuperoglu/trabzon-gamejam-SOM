using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Çıkış Kapısı - Anahtar varsa sonraki sahneye geçiş yapar
/// </summary>
public class ExitDoor : MonoBehaviour
{
    [Header("Sahne Ayarları")]
    [Tooltip("Geçilecek sahne adı (Build Settings'e eklenmeli)")]
    public string nextSceneName;
    
    [Tooltip("Veya sahne index'i kullan (-1 ise isim kullanılır)")]
    public int nextSceneIndex = -1;

    [Header("Etkileşim")]
    [Tooltip("Etkileşim mesafesi")]
    public float interactRange = 3f;

    [Header("UI")]
    [Tooltip("E tuşu ipucu UI")]
    public GameObject interactPromptUI;
    
    [Tooltip("Anahtar gerekli uyarı UI")]
    public GameObject keyRequiredUI;

    // Private
    private Camera playerCamera;
    private bool isLooking = false;
    private static ExitDoor currentActiveExit;

    void Start()
    {
        playerCamera = Camera.main;
        
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (keyRequiredUI != null) keyRequiredUI.SetActive(false);
    }

    void Update()
    {
        CheckPlayerLooking();
        
        if (isLooking)
        {
            HandleInteraction();
        }
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

        // UI kontrolü
        if (interactPromptUI != null)
        {
            if (isLooking)
            {
                currentActiveExit = this;
                // Anahtar varsa interact prompt göster
                bool hasKey = InventorySystem.Instance != null && InventorySystem.Instance.hasKey;
                interactPromptUI.SetActive(hasKey);
                
                if (keyRequiredUI != null)
                {
                    keyRequiredUI.SetActive(!hasKey);
                }
            }
            else if (wasLooking && currentActiveExit == this)
            {
                currentActiveExit = null;
                interactPromptUI.SetActive(false);
                if (keyRequiredUI != null) keyRequiredUI.SetActive(false);
            }
        }
    }

    void HandleInteraction()
    {
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryExit();
        }
    }

    void TryExit()
    {
        // Anahtar kontrolü
        if (InventorySystem.Instance == null || !InventorySystem.Instance.hasKey)
        {
            // Anahtar yok - uyarı göster
            if (keyRequiredUI != null)
            {
                keyRequiredUI.SetActive(true);
            }
            return;
        }

        // Anahtarı kullan (envanterde kalmamalı)
        InventorySystem.Instance.UseKey();

        // Envanter UI'ını gizle
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.Hide();
        }

        // Oyunu durdur ama ses devam etsin (TimeScale = 0)
        Time.timeScale = 0f;

        // UI'ları kapat
        if (interactPromptUI != null) interactPromptUI.SetActive(false);
        if (keyRequiredUI != null) keyRequiredUI.SetActive(false);

        // Sahneyi yükle
        LoadNextScene();
    }

    void LoadNextScene()
    {
        // Loading screen varsa onu kullan
        if (LoadingScreen.Instance != null)
        {
            if (nextSceneIndex >= 0)
            {
                LoadingScreen.Instance.LoadSceneWithLoading(nextSceneIndex);
            }
            else if (!string.IsNullOrEmpty(nextSceneName))
            {
                LoadingScreen.Instance.LoadSceneWithLoading(nextSceneName);
            }
            else
            {
                // Bir sonraki sahne
                int currentIndex = SceneManager.GetActiveScene().buildIndex;
                LoadingScreen.Instance.LoadSceneWithLoading(currentIndex + 1);
            }
        }
        else
        {
            // Loading screen yoksa direkt yükle
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (nextSceneIndex >= 0)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                int currentIndex = SceneManager.GetActiveScene().buildIndex;
                int nextIndex = currentIndex + 1;
                
                if (nextIndex < SceneManager.sceneCountInBuildSettings)
                {
                    SceneManager.LoadScene(nextIndex);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
