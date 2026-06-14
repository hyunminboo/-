using UnityEngine;
using System.Collections;

public class Dropship : MonoBehaviour
{
    public float speed = 5f;
    public Vector3 targetPosition;
    
    public GameObject enemyPrefab;
    public int dropCount = 5;
    
    private WaveManager waveManager;
    private EnemyHealth health;
    private Transform playerTransform;

    public void Initialize(Vector3 start, Vector3 end, GameObject prefab, int count, WaveManager manager)
    {
        transform.position = start;
        // 보스는 맵 중앙에 멈춰서 싸웁니다
        targetPosition = new Vector3((start.x + end.x) / 2f, start.y, 0f);
        enemyPrefab = prefab;
        dropCount = count;
        waveManager = manager;

        GameObject player = GameObject.Find("Player");
        if (player != null) playerTransform = player.transform;

        // 방향에 맞춰 수송기 좌우 반전
        if (start.x < end.x)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
        }
        else
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1f);
        }

        // 중간 보스를 위한 컴포넌트 동적 추가
        health = gameObject.GetComponent<EnemyHealth>();
        if (health == null) health = gameObject.AddComponent<EnemyHealth>();
        health.maxHealth = 800f; // 보스 체력 세팅

        BoxCollider2D box = gameObject.GetComponent<BoxCollider2D>();
        if (box == null) box = gameObject.AddComponent<BoxCollider2D>();
        box.size = new Vector2(6f, 3f); // 수송기 충돌체 크기 조정

        Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic; // 공중에 떠 있도록

        // WaveManager에 등록 (이 녀석이 죽어야 웨이브 클리어)
        waveManager.RegisterEnemy(health);

        // 보스가 죽으면 무조건 카메라 잠금을 해제하고 GO 인디케이터 표시
        health.OnDeath += () => {
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.UnlockCamera();
                if (GoIndicator.instance != null) GoIndicator.instance.ShowGo();
            }
        };

        // 엔진 사운드
        AudioSource audioSource = gameObject.GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        AudioClip engineSound = Resources.Load<AudioClip>("Sounds/Engine");
        if (engineSound != null)
        {
            audioSource.clip = engineSound;
            audioSource.loop = true;
            audioSource.playOnAwake = true;
            audioSource.spatialBlend = 1f;
            audioSource.minDistance = 5f;
            audioSource.maxDistance = 30f;
            audioSource.volume = 0.6f;
            audioSource.Play();
        }

        StartCoroutine(BossBehaviorRoutine());
    }

    IEnumerator BossBehaviorRoutine()
    {
        // 1. Enter (중앙으로 이동)
        float dist = Vector3.Distance(transform.position, targetPosition);
        float timeToReach = dist / (speed * 1.5f);
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < timeToReach)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, targetPosition, elapsed / timeToReach);
            yield return null;
        }

        // 2. Hover & Attack Phase
        float attackTimer = 0f;
        int spawnedCount = 0;
        
        while (true)
        {
            attackTimer += Time.deltaTime;
            
            // 공중에 둥둥 떠있는 느낌 (Bobbing) - hoverTimer 대신 Time.time 사용
            transform.position = targetPosition + new Vector3(Mathf.Sin(Time.time * 1.5f) * 4f, Mathf.Sin(Time.time * 2.5f) * 0.8f, 0f);

            // 공격 쿨타임을 4초로 늘림 (기존 3초)
            if (attackTimer > 4f)
            {
                attackTimer = 0f;
                // 스폰 횟수를 dropCount(기본 5)로 제한. 초과하면 무조건 레이저 공격.
                int attackPattern = (spawnedCount >= dropCount) ? 1 : Random.Range(0, 2); 
                
                if (attackPattern == 0)
                {
                    yield return StartCoroutine(DropEnemyAttack());
                    spawnedCount++;
                }
                else
                {
                    yield return StartCoroutine(LaserAttack());
                }
            }

            yield return null;
        }
    }

    IEnumerator DropEnemyAttack()
    {
        if (enemyPrefab != null && waveManager != null)
        {
            Vector3 spawnPos = transform.position + new Vector3(0, -1.5f, 0);
            GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            EnemyHealth eh = enemyObj.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                waveManager.RegisterEnemy(eh);
            }
        }
        // 공격 후 짧은 딜레이
        yield return new WaitForSeconds(1f);
    }

    IEnumerator LaserAttack()
    {
        if (playerTransform == null) yield break;

        // 레이저 충전 시각 효과
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color origColor = Color.white;
        if (sr != null)
        {
            origColor = sr.color;
            sr.color = Color.red;
        }

        Vector3 startPos = transform.position + new Vector3(0, -1f, 0);
        // 플레이어의 현재 위치를 타겟으로 고정 (회피 가능하도록)
        Vector3 targetPos = playerTransform.position;

        // 예측선 (경고 레이저) 그리기
        GameObject warningLaserObj = new GameObject("WarningLaserBeam");
        LineRenderer wlr = warningLaserObj.AddComponent<LineRenderer>();
        wlr.SetPosition(0, startPos);
        wlr.SetPosition(1, targetPos);
        wlr.startWidth = 0.05f;
        wlr.endWidth = 0.05f;
        wlr.material = new Material(Shader.Find("Sprites/Default"));
        wlr.startColor = new Color(1f, 0f, 0f, 0.5f);
        wlr.endColor = new Color(1f, 0f, 0f, 0.5f);
        wlr.sortingOrder = 9;

        // 1.5초 동안 예측선 표시 (플레이어 피할 시간 부여)
        float chargeTime = 1.5f;
        float elapsed = 0f;
        while (elapsed < chargeTime)
        {
            elapsed += Time.deltaTime;
            // 경고선 점멸 효과
            float alpha = Mathf.PingPong(Time.time * 5f, 0.5f) + 0.1f;
            wlr.startColor = new Color(1f, 0f, 0f, alpha);
            wlr.endColor = wlr.startColor;
            
            // 보스도 약간 진동
            transform.position += new Vector3(Random.Range(-0.05f, 0.05f), Random.Range(-0.05f, 0.05f), 0);
            yield return null;
        }

        Destroy(warningLaserObj);
        if (sr != null) sr.color = origColor;

        // 실제 레이저 발사
        AudioSource audioSource = GetComponent<AudioSource>();
        AudioClip laserSound = Resources.Load<AudioClip>("Sounds/Laser");
        if (audioSource != null && laserSound != null)
        {
            audioSource.PlayOneShot(laserSound, 1f);
        }

        GameObject laserObj = new GameObject("BossLaserBeam");
        LineRenderer lr = laserObj.AddComponent<LineRenderer>();
        lr.SetPosition(0, startPos);
        lr.SetPosition(1, targetPos); // 고정된 타겟 위치로 발사
        lr.startWidth = 0.8f;
        lr.endWidth = 0.8f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = Color.red;
        lr.endColor = Color.yellow;
        lr.sortingOrder = 10;
        
        Destroy(laserObj, 0.3f);

        // 데미지 판정: 레이저 선(startPos -> targetPos)과 플레이어 사이의 거리를 계산
        if (playerTransform != null)
        {
            Vector2 lineDir = (targetPos - startPos).normalized;
            Vector2 pointToStart = (Vector2)playerTransform.position - (Vector2)startPos;
            float dot = Vector2.Dot(pointToStart, lineDir);
            
            float distanceToLine = 0f;
            if (dot <= 0) 
                distanceToLine = Vector2.Distance(playerTransform.position, startPos);
            else if (dot >= Vector2.Distance(targetPos, startPos)) 
                distanceToLine = Vector2.Distance(playerTransform.position, targetPos);
            else 
            {
                Vector2 closestPoint = (Vector2)startPos + lineDir * dot;
                distanceToLine = Vector2.Distance(playerTransform.position, closestPoint);
            }

            // 플레이어가 레이저 근처(폭 1.5 이내)에 있으면 데미지 적용
            if (distanceToLine < 1.5f)
            {
                PlayerHealth ph = playerTransform.GetComponent<PlayerHealth>();
                if (ph != null) ph.TakeDamage(30f);
            }
        }

        yield return new WaitForSeconds(1f);
    }
}
