using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

/// <summary>
/// Envanter Sistemi - Singleton pattern ile maske ve anahtar yönetimi
/// </summary>
public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [Header("Maske Durumları")]
    [Tooltip("Hangi maskeler alındı?")]
    public bool[] hasMask = new bool[3]; // Mask1, Mask2, Mask3
    
    [Tooltip("Anahtar alındı mı?")]
    public bool hasKey = false;
    
    [Tooltip("Aktif maske index (-1 = yok)")]
    public int activeMaskIndex = -1;

    [Header("Maske Referansları")]
    [Tooltip("Maske scriptlerini buraya sürükle (opsiyonel)")]
    public MaskBase[] masks = new MaskBase[3];

    [Header("Events")]
    public UnityEvent<int> OnMaskCollected;      // Toplanan maske index'i
    public UnityEvent OnKeyCollected;             // Anahtar toplandığında
    public UnityEvent<int> OnMaskActivated;       // Aktif maske değiştiğinde
    public UnityEvent<int> OnMaskDeactivated;     // Maske deaktif olduğunda

    // Input Actions
    private PlayerInput playerInput;
    private InputAction mask1Action;
    private InputAction mask2Action;
    private InputAction mask3Action;

    void Awake()
    {
        // Singleton kurulumu
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Sahneler arası kalıcı ol
        DontDestroyOnLoad(gameObject);

        // Input setup
        playerInput = GetComponent<PlayerInput>();
        if (playerInput == null)
        {
            playerInput = FindAnyObjectByType<PlayerInput>();
        }

        SetupInputActions();
    }

    void SetupInputActions()
    {
        if (playerInput != null && playerInput.actions != null)
        {
            mask1Action = playerInput.actions["Mask1"];
            mask2Action = playerInput.actions["Mask2"];
            mask3Action = playerInput.actions["Mask3"];
        }
    }

    void Update()
    {
        HandleMaskInput();
    }

    void HandleMaskInput()
    {
        // Input System ile kontrol
        if (mask1Action != null && mask1Action.WasPressedThisFrame())
        {
            ActivateMask(0);
        }
        else if (mask2Action != null && mask2Action.WasPressedThisFrame())
        {
            ActivateMask(1);
        }
        else if (mask3Action != null && mask3Action.WasPressedThisFrame())
        {
            ActivateMask(2);
        }
        // Fallback - Input System yoksa Keyboard ile
        else if (Keyboard.current != null)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                ActivateMask(0);
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                ActivateMask(1);
            }
            else if (Keyboard.current.digit3Key.wasPressedThisFrame)
            {
                ActivateMask(2);
            }
        }
    }

    /// <summary>
    /// Maske topla
    /// </summary>
    public void CollectMask(int maskIndex)
    {
        if (maskIndex < 0 || maskIndex >= 3) return;
        
        if (!hasMask[maskIndex])
        {
            hasMask[maskIndex] = true;
            OnMaskCollected?.Invoke(maskIndex);
        }
    }

    /// <summary>
    /// Anahtar topla
    /// </summary>
    public void CollectKey()
    {
        if (!hasKey)
        {
            hasKey = true;
            OnKeyCollected?.Invoke();
        }
    }

    /// <summary>
    /// Anahtarı kullan (kapıda harcandığında)
    /// </summary>
    public void UseKey()
    {
        hasKey = false;
    }

    /// <summary>
    /// Yeni seviye için envanter hazırla
    /// Maskeler korunur, anahtar zaten kullanılmış olmalı
    /// </summary>
    public void PrepareForNewLevel()
    {
        // Maskeler korunur (hasMask[] değişmez)
        // Aktif maske korunur
        // Anahtar zaten UseKey() ile kullanılmış olmalı
        hasKey = false; // Güvenlik için
    }

    /// <summary>
    /// Maskeyi aktifleştir (sürekli aktif kalır)
    /// </summary>
    public void ActivateMask(int maskIndex)
    {
        if (maskIndex < 0 || maskIndex >= 3) return;
        
        // Maske alınmış mı kontrol et
        if (!hasMask[maskIndex]) return;

        // Zaten bu maske aktifse, deaktif et
        if (activeMaskIndex == maskIndex)
        {
            DeactivateMask(maskIndex);
            return;
        }

        // Önceki maskeyi deaktif et
        if (activeMaskIndex >= 0 && activeMaskIndex < 3)
        {
            DeactivateMask(activeMaskIndex);
        }

        // Yeni maskeyi aktif et
        activeMaskIndex = maskIndex;
        
        // Maske script varsa aktifleştir
        if (masks != null && masks.Length > maskIndex && masks[maskIndex] != null)
        {
            masks[maskIndex].Activate();
        }

        OnMaskActivated?.Invoke(maskIndex);
    }

    /// <summary>
    /// Maskeyi deaktif et
    /// </summary>
    public void DeactivateMask(int maskIndex)
    {
        if (maskIndex < 0 || maskIndex >= 3) return;

        // Maske script varsa deaktifleştir
        if (masks != null && masks.Length > maskIndex && masks[maskIndex] != null)
        {
            masks[maskIndex].Deactivate();
        }

        if (activeMaskIndex == maskIndex)
        {
            activeMaskIndex = -1;
        }
        OnMaskDeactivated?.Invoke(maskIndex);
    }

    /// <summary>
    /// Belirli bir maskeye sahip mi?
    /// </summary>
    public bool HasMask(int index)
    {
        if (index < 0 || index >= 3) return false;
        return hasMask[index];
    }

    /// <summary>
    /// Belirli bir maske aktif mi?
    /// </summary>
    public bool IsMaskActive(int index)
    {
        return activeMaskIndex == index && hasMask[index];
    }

    /// <summary>
    /// Settings menüsünden oyundan çıkış
    /// </summary>
    public void ExitForSettings()
    {
        Debug.Log("Oyundan çıkılıyor...");
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
