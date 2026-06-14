using UnityEngine;
using System.Collections;

public enum EnemyType
{
    Melee,
    Ranged
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2.5f;
    public float attackDamage = 15f;
    
    // 공격 쿨타임
    public float attackCooldown = 2f;
    private float lastAttackTime;

    private Rigidbody2D rb;
    private Transform playerTransform;
    private PlayerHealth playerHealth;
    
    private bool isLanded = false;
    private float baseScale;

    [Header("Effects")]
    public GameObject landParticlePrefab; // 착지 먼지 이펙트

    public EnemyType type;
    
    // 원거리 레이저용
    private LineRenderer laserLine;
    public Transform gunPoint; // 레이저 발사 위치 (임시로 자기 중심 사용)

    // 애니메이션 관련
    private SpriteRenderer sr;
    private Sprite[] moveSprites;
    private float animTimer;
    private int animIndex;
    private int[] walkSeq = { 1, 0, 2, 0 }; // 자연스러운 걷기 사이클
    
    private Sprite attackSprite;
    private Sprite meleeAttackSprite;
    private Sprite laserSprite;
    private bool isAttacking = false;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            attackDamage *= GameManager.Instance.GetDifficultyMultiplier();
        }
        
        sr = GetComponent<SpriteRenderer>();
        moveSprites = new Sprite[3];
        
        var baseSprites = Resources.LoadAll<Sprite>("Sprites/Enemies/enemy");
        if(baseSprites.Length > 0) moveSprites[0] = baseSprites[0];
        
        var move1Sprites = Resources.LoadAll<Sprite>("Sprites/Enemies/enemy_move1");
        if(move1Sprites.Length > 0) moveSprites[1] = move1Sprites[0];
        
        var move2Sprites = Resources.LoadAll<Sprite>("Sprites/Enemies/enemy_move2");
        if(move2Sprites.Length > 0) moveSprites[2] = move2Sprites[0];
        
        var attackSprites = Resources.LoadAll<Sprite>("Sprites/Enemies/enemy_attack");
        if(attackSprites.Length > 0) attackSprite = attackSprites[0];
        
        var meleeAttackSprites = Resources.LoadAll<Sprite>("Sprites/Enemies/enemy_melee_attack");
        if(meleeAttackSprites.Length > 0) meleeAttackSprite = meleeAttackSprites[0];
        
        var laserSprites = Resources.LoadAll<Sprite>("Sprites/Enemies/laser_beam");
        if(laserSprites.Length > 0) laserSprite = laserSprites[0];

        rb = GetComponent<Rigidbody2D>();
        // 원본 이미지 크기 저장 (0.25 등)
        baseScale = Mathf.Abs(transform.localScale.y);
        
        // 50% 확률로 근거리/원거리 결정
        type = (Random.value > 0.5f) ? EnemyType.Melee : EnemyType.Ranged;
        
        // 레이저 렌더러 가져오기
        laserLine = GetComponent<LineRenderer>();
        if (laserLine != null)
        {
            laserLine.enabled = false;
        }

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        // 색상으로 타입 구분을 원할 경우 (테스트용)
        // GetComponent<SpriteRenderer>().color = (type == EnemyType.Melee) ? Color.white : new Color(0.8f, 0.8f, 1f);
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // 거리가 50 이상이면 인공지능이 작동하지 않고 제자리에 대기 (런앤건 최적화)
        // 화면 크기(약 35)보다 넉넉하게 잡아서 화면에 보일 때는 무조건 움직이도록 수정
        if (distanceToPlayer > 50f)
        {
            if (moveSprites != null && moveSprites.Length > 0 && sr != null) sr.sprite = moveSprites[0];
            if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float direction = Mathf.Sign(playerTransform.position.x - transform.position.x);

        // 시선 방향 전환 (스프라이트가 기본적으로 오른쪽을 봄)
        if (direction > 0)
            transform.localScale = new Vector3(baseScale, baseScale, 1f);
        else
            transform.localScale = new Vector3(-baseScale, baseScale, 1f);

        // AI 행동 패턴
        if (type == EnemyType.Melee)
        {
            // 근접형: 무조건 돌진
            if (distanceToPlayer > 1.0f)
            {
                rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                TryMeleeAttack();
            }
        }
        else if (type == EnemyType.Ranged)
        {
            // 원거리형: 거리가 멀면 다가가고, 가까워지면 사격 (완전히 멈추지 않고 천천히 이동)
            if (distanceToPlayer > 3f)
            {
                rb.linearVelocity = new Vector2(direction * (moveSpeed * 0.6f), rb.linearVelocity.y);
                if (distanceToPlayer <= 10f) TryRangedAttack();
            }
            else
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                TryRangedAttack();
            }
        }

        // 애니메이션 재생
        if (isAttacking)
        {
            // 공격 중에는 상태 유지
        }
        else if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            animTimer += Time.deltaTime;
            // 프레임 전환 속도를 약간 더 빠르게 조정
            if (animTimer > 0.1f)
            {
                animTimer = 0f;
                if (moveSprites != null && moveSprites.Length > 2 && sr != null)
                {
                    animIndex = (animIndex + 1) % walkSeq.Length;
                    int targetSprite = walkSeq[animIndex];
                    if (moveSprites[targetSprite] != null) sr.sprite = moveSprites[targetSprite];
                }
            }
        }
        else
        {
            if (moveSprites != null && moveSprites.Length > 0 && moveSprites[0] != null && sr != null)
            {
                sr.sprite = moveSprites[0]; // 대기 상태
                animIndex = 0; // 초기화
            }
        }
    }

    private void TryMeleeAttack()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            StartCoroutine(MeleeAttackCoroutine());
            lastAttackTime = Time.time;
        }
    }

    IEnumerator MeleeAttackCoroutine()
    {
        if (playerTransform == null) yield break;

        isAttacking = true;
        if (meleeAttackSprite != null && sr != null)
        {
            sr.sprite = meleeAttackSprite;
        }

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log("근접 공격 쾅!");
        }

        // 0.3초간 모션 유지
        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }

    private void TryRangedAttack()
    {
        if (Time.time > lastAttackTime + attackCooldown)
        {
            StartCoroutine(FireLaser());
            lastAttackTime = Time.time;
        }
    }

    IEnumerator FireLaser()
    {
        if (playerTransform == null) yield break;

        isAttacking = true;
        if (attackSprite != null && sr != null)
        {
            sr.sprite = attackSprite;
        }

        // 발사 위치 계산 (총구 느낌을 위해 앞으로 살짝 이동)
        float aimDirection = Mathf.Sign(playerTransform.position.x - transform.position.x);
        Vector3 startPos = transform.position + new Vector3(aimDirection * 1.5f, 0.5f, 0f);
        Vector3 endPos = playerTransform.position + new Vector3(0, 0.5f, 0); // 플레이어 가슴팍

        // 시각적 레이저 생성 (기존 LineRenderer 대신 Sprite 사용)
        GameObject laserObj = new GameObject("LaserBeam");
        laserObj.transform.position = (startPos + endPos) / 2f; // 중간 지점
        
        // 회전 계산
        Vector3 dir = endPos - startPos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        laserObj.transform.rotation = Quaternion.Euler(0, 0, angle);
        
        SpriteRenderer laserSr = laserObj.AddComponent<SpriteRenderer>();
        if (laserSprite != null)
        {
            laserSr.sprite = laserSprite;
            laserSr.sortingOrder = 10;
            // 레이저 길이 및 두께 맞춤 조절 (두께를 1.2f로 대폭 키움)
            float distance = dir.magnitude;
            float spriteWidth = laserSprite.bounds.size.x;
            if (spriteWidth > 0)
                laserObj.transform.localScale = new Vector3(distance / spriteWidth, 1.2f, 1f);
        }
        
        Destroy(laserObj, 0.2f);

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log("원거리 레이저 찌직!");
        }

        // 0.2초 뒤 레이저 끄기 및 공격 모션 종료
        yield return new WaitForSeconds(0.2f);
        if (laserObj != null) Destroy(laserObj);
        isAttacking = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLanded && collision.gameObject.name == "Ground")
        {
            isLanded = true;
            
            // 바닥에 처음 닿았을 때 흙먼지 파티클 생성
            if (landParticlePrefab != null)
            {
                // 발 밑부분(충돌 위치 부근)에 생성
                Vector3 spawnPos = transform.position - new Vector3(0, GetComponent<CapsuleCollider2D>().size.y / 2f, 0);
                GameObject particle = Instantiate(landParticlePrefab, spawnPos, Quaternion.identity);
                Destroy(particle, 1.5f); // 1.5초 뒤 자동 삭제
            }
        }
    }
}
