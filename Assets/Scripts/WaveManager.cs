using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    public GameObject dropshipPrefab;
    public GameObject enemyPrefab;
    public GameObject stage3BossPrefab;

    private int currentWave = 1;
    private List<EnemyHealth> activeEnemies = new List<EnemyHealth>();
    private bool waveInProgress = false;

    void Start()
    {
        // 씬 시작 시 자동 시작 로직 제거 (이제 인트로 컷신 매니저가 호출함)
    }

    public void StartWave1()
    {
        Debug.Log("==== 웨이브 1 시작! 미리 배치된 적들을 등록합니다! ====");
        currentWave = 1;
        waveInProgress = true;
        
        EnemyHealth[] allEnemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
        int count = 0;
        
        float camX = Camera.main.transform.position.x;
        float camHalfWidth = Camera.main.orthographicSize * Camera.main.aspect;
        float maxXAllowed = camX + camHalfWidth + 10f; // 화면 우측 약간 여유분까지
        
        foreach(var enemy in allEnemies)
        {
            if (enemy.gameObject.activeInHierarchy && enemy.GetCurrentHealth() > 0)
            {
                // 화면에 닿을 수 있는 거리(X) 이면서 지하(스테이지2, Y < -5)가 아닌 적만 웨이브 1에 등록
                if (enemy.transform.position.x <= maxXAllowed && enemy.transform.position.y > -5f)
                {
                    RegisterEnemy(enemy);
                    count++;
                }
            }
        }
        
        Debug.Log("총 " + count + "명의 씬에 배치된 적을 웨이브 1에 등록했습니다.");
        
        if (count == 0)
        {
            waveInProgress = false;
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null) camFollow.UnlockCamera();
        }
    }

    public void StartWave2()
    {
        Debug.Log("==== 웨이브 2 시작! 좌측에서 수송기 등장! ====");
        currentWave = 2;
        waveInProgress = true;
        
        // 현재 카메라 위치 기준으로 좌측 화면 밖에서 우측으로 이동
        float camX = Camera.main.transform.position.x;
        Vector3 startPos = new Vector3(camX - 15f, 2.0f, 0f);
        Vector3 endPos = new Vector3(camX + 15f, 2.0f, 0f);

        SpawnDropship(startPos, endPos, 5);
    }

    public void StartWave3()
    {
        Debug.Log("==== 웨이브 3 시작! 스테이지 3 구역의 적들을 등록합니다! ====");
        currentWave = 3;
        waveInProgress = true;
        
        EnemyHealth[] allEnemies = FindObjectsByType<EnemyHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int count = 0;
        
        foreach(var enemy in allEnemies)
        {
            if (enemy.gameObject.activeInHierarchy && enemy.GetCurrentHealth() > 0)
            {
                // 스테이지 3 구역(X >= 180)의 적만 등록
                if (enemy.transform.position.x >= 180f)
                {
                    RegisterEnemy(enemy);
                    count++;
                }
            }
        }
        
        Debug.Log("총 " + count + "명의 씬에 배치된 적을 웨이브 3에 등록했습니다.");
        
        if (count == 0)
        {
            waveInProgress = false;
            StartCoroutine(WaitAndStartNextWave());
        }
    }

    public void StartWave4()
    {
        Debug.Log("==== 웨이브 4 시작! 스테이지 3의 마지막 비행기(Dropship) 등장! ====");
        currentWave = 4;
        waveInProgress = true;
        
        // 현재 카메라 위치 기준으로 우측 상단에서 등장
        float camX = Camera.main != null ? Camera.main.transform.position.x : 200f;
        // 비행기는 오른쪽 끝에서 나타나 약간 중앙으로 옵니다.
        Vector3 startPos = new Vector3(camX + 15f, 5f, 0f);
        Vector3 endPos = new Vector3(camX + 5f, 5f, 0f);

        SpawnDropship(startPos, endPos, 5);
    }

    public void StartBossPhase()
    {
        Debug.Log("⚠️⚠️⚠️ 최종 보스 등장!! ⚠️⚠️⚠️");

        // 보스 BGM으로 교체
        GameObject bgmManager = GameObject.Find("BGM_Manager");
        if (bgmManager != null)
        {
            AudioSource audio = bgmManager.GetComponent<AudioSource>();
            if (audio != null)
            {
                AudioClip bossBGM = Resources.Load<AudioClip>("Sounds/BossBGM");
                if (bossBGM != null)
                {
                    audio.clip = bossBGM;
                    audio.Play();
                }
                else
                {
                    Debug.LogWarning("BossBGM.mp3를 Resources/Sounds 경로에서 찾을 수 없습니다.");
                }
            }
        }

        StartCoroutine(BossSpawnSequence());
    }

    private IEnumerator BossSpawnSequence()
    {
        if (stage3BossPrefab == null)
        {
            Debug.LogError("stage3BossPrefab이 WaveManager에 할당되지 않았습니다!");
            yield break;
        }

        // 1. 화면에 X 표시와 BOSS 경고 띄우기 (BossWarningUI)
        GameObject warningObj = new GameObject("BossWarningTrigger");
        warningObj.AddComponent<BossWarningUI>();

        // 2. 1.5초 대기 (경고 연출 시간)
        yield return new WaitForSeconds(1.5f);

        // 3. 현재 화면 중앙보다 살짝 앞쪽 하늘(Y=+15)에서 보스 스폰
        float camX = Camera.main != null ? Camera.main.transform.position.x : 0f;
        Vector3 skySpawnPos = new Vector3(camX + 5f, 15f, 0f);
        Instantiate(stage3BossPrefab, skySpawnPos, Quaternion.identity);
        
        Debug.Log("최종 보스가 하늘에서 강하합니다!");
    }

    void SpawnDropship(Vector3 start, Vector3 end, int enemyCount)
    {
        if (dropshipPrefab == null || enemyPrefab == null)
        {
            Debug.LogError("수송기 또는 적 프리팹이 할당되지 않았습니다!");
            return;
        }

        GameObject dropshipObj = Instantiate(dropshipPrefab);
        Dropship dropship = dropshipObj.GetComponent<Dropship>();
        
        dropship.Initialize(start, end, enemyPrefab, enemyCount, this);
    }

    IEnumerator SpawnGroundWave(int enemyCount)
    {
        if (enemyPrefab == null) yield break;

        for (int i = 0; i < enemyCount; i++)
        {
            float camX = Camera.main.transform.position.x;
            float spawnX = Mathf.Min(camX + 15f, 95f); // 낙사 구역(X > 100) 스폰 방지
            Vector3 spawnPos = new Vector3(spawnX, 2f, 0f); // 우측 화면 밖 약간 위
            
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            EnemyHealth enemyHealth = enemyObj.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                RegisterEnemy(enemyHealth);
            }
            
            yield return new WaitForSeconds(1.5f); // 1.5초 간격으로 스폰
        }
    }

    public void RegisterEnemy(EnemyHealth enemyHealth)
    {
        activeEnemies.Add(enemyHealth);
        enemyHealth.OnDeath += () => OnEnemyDied(enemyHealth);
    }

    private void OnEnemyDied(EnemyHealth enemyHealth)
    {
        activeEnemies.Remove(enemyHealth);
        Debug.Log("적 처치! 남은 적 수: " + activeEnemies.Count);
        
        // 적이 다 죽었는지 체크
        if (activeEnemies.Count == 0 && waveInProgress)
        {
            waveInProgress = false;
            
            // 카메라 화면 잠금 해제 (메탈슬러그 방식)
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.UnlockCamera();
                Debug.Log("✅ 웨이브 클리어! 앞으로 전진하세요! GO!");
                
                // 화면 우측에 GO -> 화살표 띄우기
                if (GoIndicator.instance != null)
                {
                    GoIndicator.instance.ShowGo();
                }
            }
            
            // 웨이브 2나 보스로 넘어가는 로직은 트리거 방식으로 바꿀 예정이므로 일단 자동 대기는 유지하거나 주석 처리
            // StartCoroutine(WaitAndStartNextWave());
        }
    }

    IEnumerator WaitAndStartNextWave()
    {
        Debug.Log("웨이브 클리어! 3초 후 다음 페이즈 진입...");
        // 3초 대기 후 다음 웨이브 시작
        yield return new WaitForSeconds(3f);

        currentWave++;

        if (currentWave == 2)
        {
            StartWave2();
        }
        else if (currentWave == 3)
        {
            StartWave3();
        }
        else if (currentWave == 4)
        {
            StartWave4();
        }
        else if (currentWave >= 5)
        {
            StartBossPhase();
        }
    }
}
