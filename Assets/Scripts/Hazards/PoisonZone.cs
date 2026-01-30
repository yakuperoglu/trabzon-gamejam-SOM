using UnityEngine;

/// <summary>
/// Zehirli Sis Alanı - İçine giren oyuncu hasar alır
/// </summary>
[RequireComponent(typeof(Collider))]
public class PoisonZone : MonoBehaviour
{
    [Header("Görsel")]
    [Tooltip("Sis particle sistemi (opsiyonel)")]
    public ParticleSystem fogParticles;

    void Start()
    {
        // Collider'ın trigger olduğundan emin ol
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Player mı kontrol et
        if (other.CompareTag("Player") || other.GetComponent<PlayerHealth>() != null)
        {
            PlayerHealth health = PlayerHealth.Instance;
            if (health == null)
            {
                health = other.GetComponent<PlayerHealth>();
            }

            if (health != null)
            {
                health.EnterPoisonZone();
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerHealth>() != null)
        {
            PlayerHealth health = PlayerHealth.Instance;
            if (health == null)
            {
                health = other.GetComponent<PlayerHealth>();
            }

            if (health != null)
            {
                health.ExitPoisonZone();
            }
        }
    }
}
