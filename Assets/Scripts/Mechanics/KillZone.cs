using UnityEngine;

/// <summary>
/// Ölüm Bölgesi - Oyuncu bu trigger'a girince ölür
/// Cam köprünün altına veya herhangi bir tehlikeli bölgeye koyulabilir
/// </summary>
public class KillZone : MonoBehaviour
{
    [Header("Ayarlar")]
    [Tooltip("Ölmeden önce gecikme süresi")]
    public float deathDelay = 0f;

    void OnTriggerEnter(Collider other)
    {
        // Player mı kontrol et
        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            if (deathDelay > 0)
            {
                StartCoroutine(DelayedDeath(health, deathDelay));
            }
            else
            {
                health.Die();
            }
        }
    }

    System.Collections.IEnumerator DelayedDeath(PlayerHealth health, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (health != null)
        {
            health.Die();
        }
    }
}
