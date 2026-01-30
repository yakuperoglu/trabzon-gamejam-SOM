using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Oyun Ayarları - Sensitivity ve ses ayarlarını yönetir
/// Singleton olarak tüm oyundan erişilebilir
/// </summary>
public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    [Header("Varsayılan Değerler")]
    [Range(0.1f, 10f)]
    public float defaultSensitivity = 2f;
    
    [Range(0f, 1f)]
    public float defaultVolume = 1f;

    // Mevcut değerler
    public float Sensitivity { get; private set; }
    public float Volume { get; private set; }

    // Referanslar
    private CameraController cameraController;

    void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Kayıtlı ayarları yükle
        LoadSettings();
    }

    void Start()
    {
        // CameraController'ı bul
        cameraController = FindAnyObjectByType<CameraController>();
        
        // Ayarları uygula
        ApplySettings();
    }

    public void SetSensitivity(float value)
    {
        Sensitivity = Mathf.Clamp(value, 0.1f, 10f);
        
        // CameraController'a uygula
        if (cameraController == null)
        {
            cameraController = FindAnyObjectByType<CameraController>();
        }
        
        if (cameraController != null)
        {
            cameraController.mouseSensitivity = Sensitivity;
        }

        SaveSettings();
    }

    public void SetVolume(float value)
    {
        Volume = Mathf.Clamp01(value);
        AudioListener.volume = Volume;
        SaveSettings();
    }

    void ApplySettings()
    {
        SetSensitivity(Sensitivity);
        SetVolume(Volume);
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat("Sensitivity", Sensitivity);
        PlayerPrefs.SetFloat("Volume", Volume);
        PlayerPrefs.Save();
    }

    void LoadSettings()
    {
        Sensitivity = PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity);
        Volume = PlayerPrefs.GetFloat("Volume", defaultVolume);
    }

    public void ResetToDefaults()
    {
        Sensitivity = defaultSensitivity;
        Volume = defaultVolume;
        ApplySettings();
    }
}
