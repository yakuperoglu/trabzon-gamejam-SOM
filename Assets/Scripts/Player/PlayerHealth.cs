using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Oyuncu Sağlık/Toksisite Sistemi
/// Zehirli alanlarda toksisite artar, 0'a ulaşınca ölüm
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Toksisite Ayarları")]
    [Tooltip("Maksimum toksisite barı")]
    public float maxToxicity = 100f;
    
    [Tooltip("Zehirli alanda saniyede alınan hasar")]
    public float toxicityDamageRate = 75f;
    
    [Tooltip("Zehirli alan dışında toksisite iyileşme hızı")]
    public float toxicityRecoveryRate = 1f;

    [Header("Durum")]
    [SerializeField] private float currentToxicity;
    private bool isInPoisonZone = false;
    private bool isDead = false;

    // Events
    public UnityEvent<float> OnToxicityChanged;  // 0-1 arası
    public UnityEvent OnPlayerDeath;

    public float CurrentToxicity => currentToxicity;
    public float ToxicityPercent => currentToxicity / maxToxicity;
    public bool IsDead => isDead;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        currentToxicity = maxToxicity;
    }

    void Update()
    {
        if (isDead) return;

        if (isInPoisonZone)
        {
            // Maske 2 aktifse hasar alma
            if (Mask2.IsPoisonImmune)
            {
                // Korumalı - toksisite değişmez
            }
            else
            {
                // Hasar al
                currentToxicity -= toxicityDamageRate * Time.deltaTime;
                currentToxicity = Mathf.Max(0, currentToxicity);
                OnToxicityChanged?.Invoke(ToxicityPercent);

                if (currentToxicity <= 0)
                {
                    Die();
                }
            }
        }
        else
        {
            // Zehirli alan dışında iyileş (Maske 2 aktif değilse)
            if (!Mask2.IsPoisonImmune && currentToxicity < maxToxicity)
            {
                currentToxicity += toxicityRecoveryRate * Time.deltaTime;
                currentToxicity = Mathf.Min(maxToxicity, currentToxicity);
                OnToxicityChanged?.Invoke(ToxicityPercent);
            }
        }
    }

    public void EnterPoisonZone()
    {
        isInPoisonZone = true;
    }

    public void ExitPoisonZone()
    {
        isInPoisonZone = false;
    }

    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        Time.timeScale = 0f;
        
        // Mouse'u göster
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnPlayerDeath?.Invoke();
    }

    public void Respawn()
    {
        isDead = false;
        currentToxicity = maxToxicity;
        isInPoisonZone = false;
        Time.timeScale = 1f;
        
        // Mouse'u gizle
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        OnToxicityChanged?.Invoke(ToxicityPercent);
    }
}
