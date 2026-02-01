using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Loading Ekranı - Ses çalarken bekler, bitince sonraki sahneyi yükler
/// Time.timeScale = 0 iken de çalışır (WaitForSecondsRealtime kullanır)
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [Header("UI")]
    [Tooltip("Loading ekranı paneli")]
    public GameObject loadingPanel;

    [Header("Timing")]
    [Tooltip("Minimum loading süresi (saniye)")]
    public float minimumLoadingTime = 2f;

    // Private
    private AudioSource audioSource;
    private AudioClip currentAudio; // ExitDoor'dan gelen ses
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
        audioSource.playOnAwake = false;
        
        // Time.timeScale = 0 iken de çalması için
        audioSource.ignoreListenerPause = true;

        // Panel başta gizli
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
        
        // Sahne yüklendiğinde callback
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Loading ekranını göster ve sonraki sahneye geç
    /// </summary>
    public void LoadSceneWithLoading(string sceneName)
    {
        LoadSceneWithLoading(sceneName, null);
    }

    /// <summary>
    /// Loading ekranını göster ve sonraki sahneye geç (özel ses ile)
    /// </summary>
    public void LoadSceneWithLoading(string sceneName, AudioClip customAudio)
    {
        if (isLoading) return;
        
        targetSceneName = sceneName;
        targetSceneIndex = -1;
        currentAudio = customAudio; // Sadece ExitDoor'dan gelen ses
        
        StartLoading();
    }

    /// <summary>
    /// Loading ekranını göster ve sonraki sahneye geç (index ile)
    /// </summary>
    public void LoadSceneWithLoading(int sceneIndex)
    {
        LoadSceneWithLoading(sceneIndex, null);
    }

    /// <summary>
    /// Loading ekranını göster ve sonraki sahneye geç (index ve özel ses ile)
    /// </summary>
    public void LoadSceneWithLoading(int sceneIndex, AudioClip customAudio)
    {
        if (isLoading) return;
        
        targetSceneIndex = sceneIndex;
        targetSceneName = null;
        currentAudio = customAudio; // Sadece ExitDoor'dan gelen ses
        
        StartLoading();
    }

    void StartLoading()
    {
        isLoading = true;

        // Envanter UI'ını HEMEN gizle (ilk iş)
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.Hide();
        }

        // Panel göster
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(true);
        }

        // Mouse'u göster
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // NOT: TimeScale ExitDoor tarafından 0 yapılmış olabilir
        // Biz değiştirmiyoruz - WaitForSecondsRealtime kullanacağız

        // Ses çal (sadece ExitDoor'dan ses geldiyse)
        if (currentAudio != null && audioSource != null)
        {
            audioSource.clip = currentAudio;
            audioSource.Play();
        }
        
        StartCoroutine(LoadSceneAsync());
    }

    IEnumerator LoadSceneAsync()
    {
        float startTime = Time.realtimeSinceStartup;
        
        // Sahneyi async yükle
        AsyncOperation asyncLoad;
        if (targetSceneIndex >= 0)
        {
            asyncLoad = SceneManager.LoadSceneAsync(targetSceneIndex);
        }
        else if (!string.IsNullOrEmpty(targetSceneName))
        {
            asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        }
        else
        {
            int currentIndex = SceneManager.GetActiveScene().buildIndex;
            asyncLoad = SceneManager.LoadSceneAsync(currentIndex + 1);
        }

        if (asyncLoad == null)
        {
            Debug.LogError("LoadingScreen: Sahne yüklenemedi!");
            isLoading = false;
            yield break;
        }

        // Sahne hazır olsa bile aktivasyon beklemeye al
        asyncLoad.allowSceneActivation = false;

        // Sahne yüklenirken bekle (progress 0.9'a kadar gider, sonra aktivasyon bekler)
        while (asyncLoad.progress < 0.9f)
        {
            yield return null;
        }

        // Sesin bitmesini bekle (varsa)
        if (audioSource != null && audioSource.isPlaying)
        {
            while (audioSource.isPlaying)
            {
                yield return null;
            }
        }

        // Minimum süre geçmesini bekle (realtime - TimeScale'den bağımsız)
        float elapsed = Time.realtimeSinceStartup - startTime;
        if (elapsed < minimumLoadingTime)
        {
            yield return new WaitForSecondsRealtime(minimumLoadingTime - elapsed);
        }

        // Ek gecikme - önceki sahnenin görünmemesi için
        yield return new WaitForSecondsRealtime(0.3f);

        // Sahneyi aktif et - bu noktada panel hala görünür
        asyncLoad.allowSceneActivation = true;
        
        // OnSceneLoaded callback'i çağrılacak
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isLoading) return;
        
        isLoading = false;
        currentAudio = null; // Sesi temizle - bir sonraki geçişte eski ses çalmasın

        // Panel gizle - yeni sahne tamamen yüklendikten sonra
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }

        // Time scale'i geri getir
        Time.timeScale = 1f;

        // Mouse'u gizle (gameplay için)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // Envanter UI'ını göster ve güncelle
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.Show();
        }
        
        Debug.Log($"LoadingScreen: {scene.name} sahnesi yüklendi");
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
        
        // Time scale'i geri getir
        Time.timeScale = 1f;
        
        // Panel gizle
        if (loadingPanel != null)
        {
            loadingPanel.SetActive(false);
        }
        
        isLoading = false;
        
        // Direkt sahne yükle
        if (targetSceneIndex >= 0)
        {
            SceneManager.LoadScene(targetSceneIndex);
        }
        else if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
