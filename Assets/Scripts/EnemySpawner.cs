using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class EnemyWave
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public float delayBeforeSpawn;
}

public class EnemySpawner : MonoBehaviour
{
    [Header("Wave Configuration")]
    public List<EnemyWave> waves;
    public bool isClear = false;

    [Header("Events")]
    public UnityEvent OnAllEnemiesCleared;

    private List<EnemyHealth> activeEnemies = new List<EnemyHealth>();
    private bool spawnInProgress = false;

    public void StartSpawn()
    {
        if (spawnInProgress || isClear) return;
        spawnInProgress = true;
        StartCoroutine(SpawnSequence());
    }

    IEnumerator SpawnSequence()
    {
        foreach (var wave in waves)
        {
            if (wave.delayBeforeSpawn > 0)
                yield return new WaitForSeconds(wave.delayBeforeSpawn);

            if (wave.enemyPrefab != null && wave.spawnPoint != null)
            {
                // 절대 플레이어 뒤에서 스폰되지 않도록 처리 (선택적)
                // 지금은 spawnPoint의 위치를 신뢰함
                GameObject enemyObj = Instantiate(wave.enemyPrefab, wave.spawnPoint.position, Quaternion.identity);
                EnemyHealth eh = enemyObj.GetComponent<EnemyHealth>();
                if (eh != null)
                {
                    activeEnemies.Add(eh);
                    eh.OnDeath += () => OnEnemyDied(eh);
                }
            }
        }
    }

    private void OnEnemyDied(EnemyHealth eh)
    {
        activeEnemies.Remove(eh);
        
        // 웨이브 스폰이 다 끝났고, 남은 적이 없다면 클리어!
        if (spawnInProgress && activeEnemies.Count == 0)
        {
            StartCoroutine(CheckClearRoutine());
        }
    }

    IEnumerator CheckClearRoutine()
    {
        // 0.5초 정도 대기 (동시에 죽었을 때 중복 호출 방지 및 자연스러운 연출)
        yield return new WaitForSeconds(0.5f);
        
        if (activeEnemies.Count == 0 && !isClear)
        {
            isClear = true;
            spawnInProgress = false;
            Debug.Log($"[EnemySpawner] {gameObject.name} 클리어!");
            
            OnAllEnemiesCleared?.Invoke();

            // 만약 메인 카메라의 잠금을 푸는 기능이 있다면 호출
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.UnlockCamera();
                if (GoIndicator.instance != null)
                {
                    GoIndicator.instance.ShowGo();
                }
            }
        }
    }
}
