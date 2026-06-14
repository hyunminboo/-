using UnityEngine;

public class BossTriggerOnDeath : MonoBehaviour
{
    private EnemyHealth enemyHealth;
    private bool hasTriggered = false;

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath += TriggerBoss;
        }
        else
        {
            Debug.LogError("BossTriggerOnDeath requires an EnemyHealth component!");
        }
    }

    void TriggerBoss()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        WaveManager waveManager = Object.FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
        {
            Debug.Log("[BossTriggerOnDeath] 이 적이 죽어서 보스를 소환합니다!");
            waveManager.StartBossPhase();
        }
        else
        {
            Debug.LogError("WaveManager not found in scene!");
        }
    }

    void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.OnDeath -= TriggerBoss;
        }
    }
}
