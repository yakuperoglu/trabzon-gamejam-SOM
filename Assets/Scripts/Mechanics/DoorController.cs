using UnityEngine;

/// <summary>
/// Kapı Kontrolcüsü - Menteşe noktasından smooth açılır
/// 
/// KURULUM:
/// 1. Boş bir GameObject oluştur (DoorPivot)
/// 2. DoorPivot'u kapının menteşe olacağı köşeye yerleştir
/// 3. Kapı modelini DoorPivot'un CHILD'ı yap
/// 4. Bu scripti DoorPivot'a ekle (kapıya değil!)
/// </summary>
public class DoorController : MonoBehaviour
{
    [Header("Kapı Ayarları")]
    [Tooltip("Açılma açısı (derece)")]
    public float openAngle = 90f;
    
    [Tooltip("Açılma hızı (düşük = yavaş, yüksek = hızlı)")]
    public float openSpeed = 3f;
    
    [Tooltip("Açılma yönü (1 = sola, -1 = sağa)")]
    public float openDirection = 1f;

    [Header("Ses (Opsiyonel)")]
    public AudioClip openSound;
    public AudioClip closeSound;

    // Private
    private float startYRotation;
    private float targetYRotation;
    private float currentYRotation;
    private bool isOpen = false;
    private bool isAnimating = false;
    private AudioSource audioSource;

    void Start()
    {
        // Başlangıç rotasyonunu kaydet
        startYRotation = transform.eulerAngles.y;
        currentYRotation = startYRotation;
        targetYRotation = startYRotation;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        if (isAnimating)
        {
            AnimateDoor();
        }
    }

    void AnimateDoor()
    {
        // Sabit hızda hareket
        currentYRotation = Mathf.MoveTowards(currentYRotation, targetYRotation, Time.deltaTime * openSpeed * 30f);
        
        // Rotasyonu uygula
        transform.eulerAngles = new Vector3(
            transform.eulerAngles.x,
            currentYRotation,
            transform.eulerAngles.z
        );

        // Hedefe ulaştıysa dur
        if (Mathf.Approximately(currentYRotation, targetYRotation))
        {
            isAnimating = false;
        }
    }

    public void OpenDoor()
    {
        if (isOpen) return;
        
        isOpen = true;
        isAnimating = true;
        targetYRotation = startYRotation + (openAngle * openDirection);
        
        PlaySound(openSound);
    }

    public void CloseDoor()
    {
        if (!isOpen) return;
        
        isOpen = false;
        isAnimating = true;
        targetYRotation = startYRotation;
        
        PlaySound(closeSound);
    }

    public void ToggleDoor()
    {
        if (isOpen)
            CloseDoor();
        else
            OpenDoor();
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
