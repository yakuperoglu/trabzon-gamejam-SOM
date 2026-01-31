using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Pause Menü - ESC ile açılır, Time.timeScale durdurur
/// </summary>
public class PauseMenu : MonoBehaviour
{
    [Header("UI Panel")]
    [Tooltip("Pause menü paneli")]
    public GameObject pausePanel;

    [Header("Sliderlar")]
    [Tooltip("Sensitivity slider")]
    public Slider sensitivitySlider;
    
    [Tooltip("Ses slider")]
    public Slider volumeSlider;

    [Header("Slider Değer Textleri (Opsiyonel)")]
    public TextMeshProUGUI sensitivityValueText;
    public TextMeshProUGUI volumeValueText;

    [Header("Butonlar")]
    [Tooltip("Oyundan çık butonu")]
    public Button quitButton;

    // Durum
    public bool IsPaused { get; private set; }
    private GameSettings settings;

    void Start()
    {
        // GameSettings'i bul veya oluştur
        settings = GameSettings.Instance;
        if (settings == null)
        {
            settings = FindAnyObjectByType<GameSettings>();
        }

        // Başlangıçta gizle
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Slider ayarları
        SetupSliders();

        // Buton eventleri
        if (quitButton != null)
        {
            quitButton.onClick.AddListener(QuitGame);
        }

        IsPaused = false;
    }

    void SetupSliders()
    {
        // Sensitivity slider
        if (sensitivitySlider != null)
        {
            sensitivitySlider.minValue = 0.1f;
            sensitivitySlider.maxValue = 10f;
            
            if (settings != null)
            {
                sensitivitySlider.value = settings.Sensitivity;
            }
            
            sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
            UpdateSensitivityText(sensitivitySlider.value);
        }

        // Volume slider
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            
            if (settings != null)
            {
                volumeSlider.value = settings.Volume;
            }
            
            volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            UpdateVolumeText(volumeSlider.value);
        }
    }

    void Update()
    {
        // ESC tuşu kontrolü
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Pause()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        // Envanter UI'ını gizle
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.Hide();
        }

        // Slider değerlerini güncelle
        if (settings != null)
        {
            if (sensitivitySlider != null) sensitivitySlider.value = settings.Sensitivity;
            if (volumeSlider != null) volumeSlider.value = settings.Volume;
        }

        // Mouse'u göster
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Resume()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Envanter UI'ını göster
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.Show();
        }

        // Mouse'u gizle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnSensitivityChanged(float value)
    {
        if (settings != null)
        {
            settings.SetSensitivity(value);
        }
        UpdateSensitivityText(value);
    }

    void OnVolumeChanged(float value)
    {
        if (settings != null)
        {
            settings.SetVolume(value);
        }
        UpdateVolumeText(value);
    }

    void UpdateSensitivityText(float value)
    {
        if (sensitivityValueText != null)
        {
            sensitivityValueText.text = value.ToString("F1");
        }
    }

    void UpdateVolumeText(float value)
    {
        if (volumeValueText != null)
        {
            volumeValueText.text = Mathf.RoundToInt(value * 100) + "%";
        }
    }

    public void QuitGame()
    {
        // Editor'da çalışırken
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void OnDestroy()
    {
        // Event temizliği
        if (sensitivitySlider != null)
        {
            sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
        }
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveListener(OnVolumeChanged);
        }
    }
}
