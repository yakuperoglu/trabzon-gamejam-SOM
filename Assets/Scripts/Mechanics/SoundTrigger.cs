using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Ses Tetikleyici - Objeye temas veya bakıp E basınca ses çalar
/// Toggle modunda: tekrar tetiklenince durur
/// </summary>
public class SoundTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        OnTriggerEnter,  // Collider'a temas
        OnInteract,      // Bakıp E tuşu
        Both             // Her ikisi de
    }

    [Header("Tetikleme Ayarları")]
    public TriggerType triggerType = TriggerType.OnTriggerEnter;
    
    [Tooltip("Toggle modu - tekrar tetikleyince durur")]
    public bool toggleMode = false;

    [Header("Ses Ayarları")]
    public AudioClip soundClip;
    
    [Range(0f, 1f)]
    public float volume = 1f;
    
    [Tooltip("Ses döngü mü?")]
    public bool loop = false;

    [Header("Etkileşim (OnInteract için)")]
    public float interactRange = 3f;
    public GameObject interactPromptUI;

    [Header("Events")]
    public UnityEvent OnSoundStarted;
    public UnityEvent OnSoundStopped;

    // Private
    private AudioSource audioSource;
    private Camera playerCamera;
    private bool isLooking = false;
    private bool isPlaying = false;
    private static SoundTrigger currentActiveTrigger;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.playOnAwake = false;
        audioSource.loop = loop;
        audioSource.volume = volume;
        
        if (soundClip != null)
        {
            audioSource.clip = soundClip;
        }

        playerCamera = Camera.main;
        
        if (interactPromptUI != null)
        {
            interactPromptUI.SetActive(false);
        }
    }

    void Update()
    {
        if (triggerType == TriggerType.OnInteract || triggerType == TriggerType.Both)
        {
            CheckPlayerLooking();
            
            if (isLooking)
            {
                HandleInteraction();
            }
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

        if (interactPromptUI != null)
        {
            if (isLooking)
            {
                currentActiveTrigger = this;
                interactPromptUI.SetActive(true);
            }
            else if (wasLooking && currentActiveTrigger == this)
            {
                currentActiveTrigger = null;
                interactPromptUI.SetActive(false);
            }
        }
    }

    void HandleInteraction()
    {
        if (UnityEngine.InputSystem.Keyboard.current != null && 
            UnityEngine.InputSystem.Keyboard.current.eKey.wasPressedThisFrame)
        {
            ToggleSound();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (triggerType == TriggerType.OnInteract) return;
        
        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
        {
            ToggleSound();
        }
    }

    public void ToggleSound()
    {
        if (toggleMode && isPlaying)
        {
            StopSound();
        }
        else
        {
            PlaySound();
        }
    }

    public void PlaySound()
    {
        if (soundClip == null)
        {
            Debug.LogWarning("SoundTrigger: soundClip atanmamış!");
            return;
        }
        
        // AudioManager üzerinden çal (obje yok edilse bile ses devam eder)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySound(soundClip, volume, loop);
            isPlaying = true;
            currentlyPlaying = this;
            OnSoundStarted?.Invoke();
            Debug.Log($"SoundTrigger: {gameObject.name} sesi AudioManager ile çalıyor");
        }
        else
        {
            // AudioManager yoksa eski yöntem
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            
            StopAllOtherAudio();
            audioSource.Stop();
            audioSource.clip = soundClip;
            audioSource.volume = volume;
            audioSource.loop = loop;
            audioSource.Play();
            
            isPlaying = true;
            currentlyPlaying = this;
            OnSoundStarted?.Invoke();
            Debug.Log($"SoundTrigger: {gameObject.name} sesi lokal AudioSource ile çalıyor");
        }
    }

    void StopAllOtherAudio()
    {
        // Tüm AudioSource'ları bul ve durdur
        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in allAudioSources)
        {
            if (source != audioSource && source.isPlaying)
            {
                source.Stop();
            }
        }
        
        // Diğer SoundTrigger'ları da durdur
        if (currentlyPlaying != null && currentlyPlaying != this)
        {
            currentlyPlaying.isPlaying = false;
            currentlyPlaying.OnSoundStopped?.Invoke();
        }
    }

    // Şu an çalan SoundTrigger
    private static SoundTrigger currentlyPlaying;

    public void StopSound()
    {
        audioSource.Stop();
        isPlaying = false;
        OnSoundStopped?.Invoke();
    }

    void OnDrawGizmosSelected()
    {
        if (triggerType != TriggerType.OnTriggerEnter)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, interactRange);
        }
    }
}
