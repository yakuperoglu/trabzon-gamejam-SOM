using UnityEngine;

/// <summary>
/// Global Audio Manager - Sahneler arası kalıcı ses yönetimi
/// Obje yok edilse bile ses çalmaya devam eder
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource audioSource;

    void Awake()
    {
        Debug.Log("AudioManager: Awake çağrıldı");
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;
        
        Debug.Log("AudioManager: Başarıyla başlatıldı!");
    }

    /// <summary>
    /// Ses çal - önceki sesi durdurur
    /// </summary>
    public void PlaySound(AudioClip clip, float volume = 1f, bool loop = false)
    {
        if (clip == null) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.loop = loop;
        audioSource.Play();
    }

    /// <summary>
    /// Ses çal - diğer sesleri durdurmadan (üst üste)
    /// </summary>
    public void PlaySoundOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        audioSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// Şu anki sesi durdur
    /// </summary>
    public void StopSound()
    {
        audioSource.Stop();
    }

    /// <summary>
    /// Ses çalıyor mu?
    /// </summary>
    public bool IsPlaying => audioSource != null && audioSource.isPlaying;
}
