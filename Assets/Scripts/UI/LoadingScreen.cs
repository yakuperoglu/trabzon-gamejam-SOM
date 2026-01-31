using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Loading Ekranı - Ses çalarken bekler, bitince sonraki sahneyi yükler
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Loading ekranı paneli")]
    public GameObject loadingPanel;

    [Header("Ses")]
    [Tooltip("Loading sırasında çalacak ses")]
    public AudioClip loadingAudio;
    
    [Tooltip("Ses kaynağı (yoksa otomatik oluşturulur)")]
    public AudioSource audioSource;

    // Private
    private string targetSceneName;
    private int targetSceneIndex = -1;
    private bool isLoading = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Audio source yoksa oluştur
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Panel başta gizli
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
    }

    /// <summary>
    /// Loading ekranını göster ve sonraki sahneye geç
    /// </summary>
    public void LoadSceneWithLoading(string sceneName)
    {
        if (isLoading) return;
        
        targetSceneName = sceneName;
        targetSceneIndex = -1;
        StartLoading();
    }

    /// <summary>
    /// Loading ekranını göster ve sonraki sahneye geç (index ile)
    /// </summary>
    public void LoadSceneWithLoading(int sceneIndex)
    {
        if (isLoading) return;
        
        targetSceneIndex = sceneIndex;
        targetSceneName = null;
        StartLoading();
    }

    void StartLoading()
    {
        isLoading = true;

        // Panel göster
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        // Mouse'u göster
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Time scale normal
        Time.timeScale = 1f;

        // Ses çal ve bitince sahneyi yükle
        if (loadingAudio != null && audioSource != null)
        {
            audioSource.clip = loadingAudio;
            audioSource.Play();
            StartCoroutine(WaitForAudioAndLoad());
        }
        else
        {
            // Ses yoksa direkt yükle
            LoadTargetScene();
        }
    }

    IEnumerator WaitForAudioAndLoad()
    {
        // Ses bitene kadar bekle
        while (audioSource.isPlaying)
        {
            yield return null;
        }

        // Küçük bir gecikme
        yield return new WaitForSeconds(0.5f);

        // Sahneyi yükle
        LoadTargetScene();
    }

    void LoadTargetScene()
    {
        isLoading = false;

        // Panel gizle
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }

        // Sahneyi yükle
        if (targetSceneIndex >= 0)
        {
            SceneManager.LoadScene(targetSceneIndex);
        }
        else if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }

        // Mouse'u gizle (yeni sahne için)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Loading'i atla (test için)
    /// </summary>
    public void SkipLoading()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        StopAllCoroutines();
        LoadTargetScene();
    }
}
