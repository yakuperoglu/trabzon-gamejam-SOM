using UnityEngine;

/// <summary>
/// Bitiş Bölgesi - Oyuncu bu trigger'a girince oyun biter ve game over ekranı gösterilir
/// Oyunun son noktasına koyulur
/// </summary>
public class EndZone : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("Temas edildiğinde açılacak Game Over paneli - Inspector'dan sürükle bırak yap")]
    public GameObject gameOverPanel;

    [Header("Ayarlar")]
    [Tooltip("Panel açılmadan önce gecikme süresi")]
    public float delay = 0f;
    
    [Tooltip("Bitiş anında çalacak ses")]
    public AudioClip endSound;
    
    [Tooltip("Ses seviyesi")]
    [Range(0f, 1f)]
    public float soundVolume = 1f;

    private bool hasTriggered = false;

    void Start()
    {
        // Panel başlangıçta gizli olsun
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Zaten tetiklendiyse çık
        if (hasTriggered) return;

        // Player mı kontrol et
        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
        {
            hasTriggered = true;
            
            if (delay > 0)
            {
                StartCoroutine(DelayedEnd(delay));
            }
            else
            {
                TriggerEnd();
            }
        }
    }

    System.Collections.IEnumerator DelayedEnd(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        TriggerEnd();
    }

    void TriggerEnd()
    {
        // Ses çal (varsa)
        if (endSound != null)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySound(endSound, soundVolume);
            }
            else
            {
                AudioSource.PlayClipAtPoint(endSound, transform.position, soundVolume);
            }
        }

        // Oyunu durdur
        Time.timeScale = 0f;

        // Mouse'u göster
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Envanter UI'ını gizle
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.Hide();
        }

        // Game Over panelini aç
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Debug.Log("EndZone: Game Over paneli açıldı!");
        }
        else
        {
            Debug.LogWarning("EndZone: Game Over Panel atanmamış! Inspector'dan paneli sürükle bırak yap.");
        }
    }

    void OnDrawGizmos()
    {
        // Editor'da görsel
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f); // Yeşil
        
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else
        {
            Gizmos.DrawSphere(transform.position, 1f);
        }
    }
}
