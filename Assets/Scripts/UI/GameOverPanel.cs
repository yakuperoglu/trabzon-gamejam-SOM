using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Game Over Panel - Butonları olan basit panel scripti
/// Panel objesine ekle ve butonları Inspector'dan bağla
/// </summary>
public class GameOverPanel : MonoBehaviour
{
    [Header("Butonlar")]
    [Tooltip("Ana menüye dön butonu")]
    public Button mainMenuButton;
    
    [Tooltip("Tekrar oyna butonu")]
    public Button playAgainButton;
    
    [Tooltip("Oyundan çık butonu")]
    public Button quitButton;

    [Header("Ayarlar")]
    [Tooltip("Ana menü sahne index'i")]
    public int mainMenuSceneIndex = 0;
    
    [Tooltip("Oyuna başlangıç sahne index'i (tekrar oyna için)")]
    public int startSceneIndex = 1;

    void Start()
    {
        // Buton event'lerini bağla
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

    public void GoToMainMenu()
    {
        Debug.Log("GameOverPanel: Ana menüye gidiliyor (Scene Index: " + mainMenuSceneIndex + ")");
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneIndex);
    }

    public void PlayAgain()
    {
        Debug.Log("GameOverPanel: Tekrar oynanıyor (Scene Index: " + startSceneIndex + ")");
        Time.timeScale = 1f;
        SceneManager.LoadScene(startSceneIndex);
    }

    public void QuitGame()
    {
        Debug.Log("GameOverPanel: Oyundan çıkılıyor");
        Time.timeScale = 1f;
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
