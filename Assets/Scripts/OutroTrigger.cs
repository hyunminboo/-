using UnityEngine;

public class OutroTrigger : MonoBehaviour
{
    private EnemyHealth health;
    private bool hasTriggered = false;

    void Start()
    {
        health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.OnDeath += TriggerOutro;
        }
    }

    void TriggerOutro()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        // 보스가 죽으면 폭발 파티클 연출 (1번만)
        GameObject explosionPrefab = Resources.Load<GameObject>("Prefabs/ExplosionEffect");
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // 보스가 죽으면 잠시 후 HeliOutroCutscene을 시작합니다.
        GameObject outroObj = new GameObject("HeliOutroCutsceneManager");
        HeliOutroCutscene outro = outroObj.AddComponent<HeliOutroCutscene>();
        outro.StartOutro();
        
        // Save records (점수, 클리어 기록 저장 등)
        if (SaveManager.Instance != null && GameManager.Instance != null)
        {
            GameManager.Instance.StopTimer();
            SaveManager.Instance.UpdateMissionRecord(
                GameManager.Instance.selectedMissionName, 
                GameManager.Instance.currentScore, 
                GameManager.Instance.currentMissionTime, 
                GameManager.Instance.currentDifficulty
            );
        }
    }
}
