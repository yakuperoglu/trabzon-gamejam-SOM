using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Zafer Ekranı - Oyuncu oyunu bitirdiğinde gösterilir
/// </summary>
public class VictoryScreen : MonoBehaviour
{
    public static VictoryScreen Instance { get; private set; }

    [Header("UI Panel")]
    [Tooltip("Zafer ekranı paneli")]
    public GameObject victoryPanel;

    [Header("Yazılar (Opsiyonel)")]
    [Tooltip("Tebrik mesajı")]
    public TextMeshProUGUI congratsText;
    
    [Tooltip("Alt mesaj")]
    public TextMeshProUGUI subtitleText;

    [Header("Butonlar")]
    [Tooltip("Ana menüye dön butonu")]
    public Button mainMenuButton;
    
    [Tooltip("Tekrar oyna butonu")]
    public Button playAgainButton;
    
    [Tooltip("Oyundan çık butonu")]
    public Button quitButton;

    [Header("Ayarlar")]
    [Tooltip("Ana menü sahne adı veya index'i (boş bırakılırsa index 0 kullanılır)")]
    public string mainMenuSceneName = "";
    
    [Tooltip("Başlangıç sahne index'i (tekrar oyna için)")]
    public int startSceneIndex = 1;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Panel'i gizle
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        // Buton event'leri
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }

        if (playAgainButton != null)
        {
            playAgainButton.onClick.AddListener(PlayAgain);
        }

        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }
    }

    /// <summary>
    /// Zafer ekranını göster
    /// </summary>
    public void ShowVictoryScreen()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        // Varsayılan mesajlar (eğer atanmamışsa)
        if (congratsText != null && string.IsNullOrEmpty(congratsText.text))
        {
            congratsText.text = "TEBRİKLER!";
        }

        if (subtitleText != null && string.IsNullOrEmpty(subtitleText.text))
        {
            subtitleText.text = "Oyunu Başarıyla Tamamladınız!";
        }

        Debug.Log("VictoryScreen: Oyun tamamlandı!");
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(startSceneIndex);
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
}
