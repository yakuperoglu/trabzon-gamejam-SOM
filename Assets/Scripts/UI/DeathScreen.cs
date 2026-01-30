using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Ölüm Ekranı - Oyuncu öldüğünde gösterilir
/// </summary>
public class DeathScreen : MonoBehaviour
{
    [Header("UI Panel")]
    [Tooltip("Ölüm ekranı paneli")]
    public GameObject deathPanel;

    [Header("Butonlar")]
    [Tooltip("Tekrar oyna butonu")]
    public Button retryButton;
    
    [Tooltip("Oyundan çık butonu")]
    public Button quitButton;

    private PlayerHealth playerHealth;

    void Start()
    {
        // Panel'i gizle
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        // PlayerHealth'i bul
        playerHealth = PlayerHealth.Instance;
        if (playerHealth == null)
        {
            playerHealth = FindAnyObjectByType<PlayerHealth>();
        }

        // Ölüm event'ine bağlan
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath.AddListener(ShowDeathScreen);
        }

        // Buton event'leri
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(Retry);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    void ShowDeathScreen()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }
    }

    public void Retry()
    {
        // Panel'i gizle
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        // Sahneyi yeniden yükle
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void OnDestroy()
    {
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDeath.RemoveListener(ShowDeathScreen);
        }
    }
}
