using UnityEngine;
using System.Collections;

public enum EnemyRole { Scout, Rifleman, Grenadier, Heavy, Turret, MissileTank, Boss }
public enum EnemyState { Idle, Alert, Chase, Attack, Dead }

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(EnemyHealth))]
public class EnemyAI : MonoBehaviour
{
    [Header("Core Settings")]
    public EnemyRole role = EnemyRole.Scout;
    public EnemyState currentState = EnemyState.Idle;
    
    [Header("Stats")]
    public float moveSpeed = 3f;
    public float attackDamage = 10f;
    public float attackCooldown = 2f;
    
    [Header("Ranges")]
    public float detectionRange = 15f;
    public float attackRange = 1.5f; 

    [Header("Visuals & Effects")]
    public GameObject landParticlePrefab;
    public SpriteRenderer spriteRenderer;
    public Sprite attackSprite;
    public Sprite laserSprite;
    
    [Header("Grenadier Only")]
    public GameObject grenadePrefab;

    private Rigidbody2D rb;
    private EnemyHealth health;
    private Transform playerTransform;
    private PlayerHealth playerHealth;
    
    private float lastAttackTime;
    private float baseScale;
    private bool isLanded = false;
    
    // For Heavy enrage
    private bool isEnraged = false;
    
    // For Turret Scanner
    private TurretScanner turretScanner;
    
    // For Move Dust
    private ParticleSystem moveDustParticle;
    
    // For Knockback Stun
    public bool isStunned = false;
    
    // For Animation
    private float animTime = 0f;

    [Header("Audio")]
    private AudioSource audioSource;
    private AudioClip footstepSound;
    private AudioClip laserSound;
    private float nextFootstepTime = 0f;
    
    [Header("Visual Fix")]
    public float colliderYOffset = 0.8f; // 적들이 공중에 뜨는 현상을 고치기 위해 콜라이더를 올리는 수치

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
        // 바닥에서 떠다니는 문제 해결: 콜라이더를 위로 올려서 몸체가 바닥으로 내려가게 함 (프리팹 0 초기화 방지)
        float actualOffset = colliderYOffset == 0f ? 0.8f : colliderYOffset;
        
        CapsuleCollider2D cap = GetComponent<CapsuleCollider2D>();
        if (cap != null) cap.offset = new Vector2(cap.offset.x, cap.offset.y + actualOffset);
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null) box.offset = new Vector2(box.offset.x, box.offset.y + actualOffset);

        if (GameManager.Instance != null)
        {
            attackDamage *= GameManager.Instance.GetDifficultyMultiplier();
        }
        
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<EnemyHealth>();
        
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = Mathf.Abs(transform.localScale.y);
        
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        turretScanner = GetComponentInChildren<TurretScanner>();

        health.OnDeath += OnDie;

        // Audio Setup
        EnsureAudioSetup();

        // 역할에 따른 초기값 세팅
        if (role == EnemyRole.Scout)
        {
            attackRange = 1.5f;
            moveSpeed = 4f; 
        }
        else if (role == EnemyRole.Rifleman)
        {
            attackRange = 8f;
            moveSpeed = 2f;
        }
        else if (role == EnemyRole.Grenadier)
        {
            attackRange = 10f; // 6~10 유지
            moveSpeed = 2f; // Scout의 절반
            attackCooldown = 3f; // 수류탄은 약간 쿨타임 김
        }
        else if (role == EnemyRole.Heavy)
        {
            attackRange = 8f;
            moveSpeed = 1f; // 매우 느림
            rb.mass = 1000f; // 넉백 저항
            attackCooldown = 0.5f; // 지속 사격을 위해 쿨타임 짧음
            // Inspector에 의해 프리팹에서 체력이 이미 설정되어 있을 수 있지만 안전장치
            // Heavy 프리팹은 EnemyHealth.maxHealth = 300 세팅 필요
        }
        else if (role == EnemyRole.Turret)
        {
            attackRange = 15f;
            moveSpeed = 0f;
            rb.mass = 1000f; // 넉백 면역
            attackCooldown = 1.5f;
        }
        else if (role == EnemyRole.Boss)
        {
            detectionRange = 50f;
            attackRange = 25f;
            moveSpeed = 2f;
            rb.mass = 5000f; // 완전 면역
            attackCooldown = 3.0f;
        }

        if (role == EnemyRole.Heavy || role == EnemyRole.Boss)
        {
            CreateDustParticleSystem();
        }

        StartCoroutine(StateMachineRoutine());
    }

    void CreateDustParticleSystem()
    {
        GameObject dustObj = new GameObject("MoveDust");
        dustObj.transform.SetParent(transform);
        
        // 콜라이더 하단을 기준으로 위치 설정
        float yOffset = -0.5f;
        var col = GetComponent<Collider2D>();
        if (col != null) yOffset = col.bounds.min.y - transform.position.y;
        
        dustObj.transform.localPosition = new Vector3(0, yOffset, 0); 
        
        moveDustParticle = dustObj.AddComponent<ParticleSystem>();
        var main = moveDustParticle.main;
        main.duration = 1f;
        main.loop = true;
        main.startLifetime = 0.6f;
        main.startSpeed = 1.5f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.5f, 1.2f);
        main.startColor = new Color(0.6f, 0.5f, 0.4f, 0.6f); // 갈색 흙먼지 느낌
        main.simulationSpace = ParticleSystemSimulationSpace.World; // 월드 기준으로 흩뿌려짐
        main.gravityModifier = -0.2f; // 먼지처럼 살짝 위로 떠오름
        
        var em = moveDustParticle.emission;
        em.rateOverTime = 30f;
        em.enabled = false;
        
        var shape = moveDustParticle.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(col != null ? col.bounds.size.x * 0.8f : 2f, 0.2f, 1f); 
        
        var renderer = dustObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Sprites/Default")); // 기본 사각형 텍스처
        renderer.sortingOrder = 5;
        
        movementDust = moveDustParticle;
    }

    private float facingDir = 1f;

    [Header("Movement Dust")]
    public ParticleSystem movementDust;

    void Update()
    {
        if (playerTransform == null || playerHealth == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                playerTransform = player.transform;
                playerHealth = player.GetComponent<PlayerHealth>();
            }
        }

        if (role == EnemyRole.Heavy && !isEnraged)
        {
            if (health != null && health.GetCurrentHealth() <= health.maxHealth * 0.5f)
            {
                isEnraged = true;
                moveSpeed *= 1.5f;
                // 빨간색 틴트로 분노 표시
                if (spriteRenderer != null) spriteRenderer.color = new Color(1f, 0.5f, 0.5f);
                Debug.Log("[Heavy] 분노 페이즈 돌입! 이속 증가");
            }
        }

        // --- 프로시저럴 애니메이션 (스쿼시 앤 스트레치) ---
        if (currentState != EnemyState.Dead && !isStunned)
        {
            bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;
            
            if (animator != null)
            {
                animator.SetBool("isMoving", isMoving);
            }

            if (movementDust != null)
            {
                var emission = movementDust.emission;
                emission.enabled = isMoving && isLanded; // 땅에 닿아있고 이동 중일 때만 먼지 발생
            }

            if (isMoving)
            {
                transform.localScale = new Vector3(facingDir * baseScale, baseScale, 1f);

                // 발소리 재생
                if (Time.time >= nextFootstepTime && isLanded)
                {
                    EnsureAudioSetup();
                    if (audioSource != null && footstepSound != null)
                    {
                        audioSource.PlayOneShot(footstepSound, 0.3f);
                    }
                    nextFootstepTime = Time.time + (0.5f / moveSpeed); // 속도에 비례한 간격
                }
            }
            else
            {
                transform.localScale = new Vector3(facingDir * baseScale, baseScale, 1f);
            }
        }
        else
        {
            if (movementDust != null)
            {
                var emission = movementDust.emission;
                emission.enabled = false;
            }
        }
    }

    IEnumerator StateMachineRoutine()
    {
        while (currentState != EnemyState.Dead)
        {
            if (!isStunned)
            {
                switch (currentState)
                {
                    case EnemyState.Idle:
                        UpdateIdle();
                        break;
                    case EnemyState.Alert:
                        UpdateAlert();
                        break;
                    case EnemyState.Chase:
                        UpdateChase();
                        break;
                    case EnemyState.Attack:
                        UpdateAttack();
                        break;
                }
            }
            yield return null;
        }
    }

    void UpdateIdle()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        if (playerTransform == null) return;

        if (role == EnemyRole.Turret && turretScanner != null)
        {
            if (turretScanner.IsPlayerDetected())
            {
                ChangeState(EnemyState.Attack);
            }
            return;
        }

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        if (dist <= detectionRange)
        {
            ChangeState(EnemyState.Alert);
        }
    }

    void UpdateAlert()
    {
        ChangeState(EnemyState.Chase);
    }

    void UpdateChase()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        float dirSign = Mathf.Sign(playerTransform.position.x - transform.position.x);
        
        FaceDirection(dirSign);

        if (role == EnemyRole.Grenadier)
        {
            // 거리 6~10 유지 및 엄폐
            if (dist < 6f)
            {
                // 너무 가까우면 뒤로 물러남
                rb.linearVelocity = new Vector2(-dirSign * moveSpeed, rb.linearVelocity.y);
            }
            else if (dist > 10f && dist <= detectionRange)
            {
                // 멀면 다가감
                rb.linearVelocity = new Vector2(dirSign * moveSpeed, rb.linearVelocity.y);
            }
            else
            {
                // 6~10 사이: 엄폐물 찾기
                GameObject cover = FindCover();
                if (cover != null)
                {
                    float coverDir = Mathf.Sign(cover.transform.position.x - transform.position.x);
                    rb.linearVelocity = new Vector2(coverDir * moveSpeed, rb.linearVelocity.y);
                    // 엄폐물 뒤에 대략 도착했으면 공격
                    if (Mathf.Abs(cover.transform.position.x - transform.position.x) < 1f)
                    {
                        ChangeState(EnemyState.Attack);
                    }
                }
                else
                {
                    ChangeState(EnemyState.Attack);
                }
            }
        }
        else if (role == EnemyRole.Heavy)
        {
            // Heavy는 사거리에 들어와도 멈추지 않고 계속 전진
            rb.linearVelocity = new Vector2(dirSign * moveSpeed, rb.linearVelocity.y);
            if (dist <= attackRange)
            {
                ChangeState(EnemyState.Attack);
            }
        }
        else if (role == EnemyRole.Turret)
        {
            if (turretScanner != null)
            {
                if (turretScanner.IsPlayerDetected())
                    ChangeState(EnemyState.Attack);
                else
                    ChangeState(EnemyState.Idle);
            }
            else
            {
                if (dist <= attackRange) ChangeState(EnemyState.Attack);
                else if (dist > detectionRange * 1.5f) ChangeState(EnemyState.Idle);
            }
        }
        else
        {
            // Scout, Rifleman
            if (dist <= attackRange)
            {
                ChangeState(EnemyState.Attack);
            }
            else if (dist > detectionRange * 1.5f)
            {
                ChangeState(EnemyState.Idle);
            }
            else
            {
                rb.linearVelocity = new Vector2(dirSign * moveSpeed, rb.linearVelocity.y);
            }
        }
    }

    GameObject FindCover()
    {
        GameObject[] covers = GameObject.FindGameObjectsWithTag("Cover");
        GameObject bestCover = null;
        float minDist = Mathf.Infinity;
        foreach (var c in covers)
        {
            float d = Vector2.Distance(transform.position, c.transform.position);
            // 플레이어와 엄폐물 사이의 거리도 고려할 수 있으나, 일단 가장 가까운 커버
            if (d < minDist && d < 10f)
            {
                minDist = d;
                bestCover = c;
            }
        }
        return bestCover;
    }

    void UpdateAttack()
    {
        if (playerTransform == null) return;

        float dist = Vector2.Distance(transform.position, playerTransform.position);
        float dirSign = Mathf.Sign(playerTransform.position.x - transform.position.x);
        FaceDirection(dirSign);

        if (role == EnemyRole.Heavy)
        {
            // Heavy는 계속 전진하며 사격
            rb.linearVelocity = new Vector2(dirSign * moveSpeed, rb.linearVelocity.y);
            if (dist > attackRange + 1f)
            {
                ChangeState(EnemyState.Chase);
                return;
            }
        }
        else if (role == EnemyRole.Grenadier)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (dist < 6f || dist > 10f)
            {
                ChangeState(EnemyState.Chase);
                return;
            }
        }
        else if (role == EnemyRole.Turret)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (turretScanner != null)
            {
                if (!turretScanner.IsPlayerDetected())
                {
                    ChangeState(EnemyState.Idle);
                    return;
                }
            }
            else if (dist > attackRange + 1f)
            {
                ChangeState(EnemyState.Idle);
                return;
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (dist > attackRange + 1f)
            {
                ChangeState(EnemyState.Chase);
                return;
            }
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            if (role == EnemyRole.Scout)
                StartCoroutine(PerformMeleeAttack());
            else if (role == EnemyRole.Rifleman)
                StartCoroutine(PerformRangedAttack());
            else if (role == EnemyRole.Turret)
                StartCoroutine(PerformTurretAttack());
            else if (role == EnemyRole.Grenadier)
                StartCoroutine(PerformGrenadeAttack());
            else if (role == EnemyRole.Heavy)
                StartCoroutine(PerformHeavyAttack());
            else if (role == EnemyRole.Boss)
                StartCoroutine(PerformBossAttack());
        }
    }

    void FaceDirection(float dirSign)
    {
        if (dirSign > 0)
            facingDir = 1f;
        else if (dirSign < 0)
            facingDir = -1f;
    }

    public void ChangeState(EnemyState newState)
    {
        if (currentState == EnemyState.Dead) return;
        currentState = newState;
    }

    IEnumerator PerformMeleeAttack()
    {
        if (attackSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = attackSprite;
            
        yield return new WaitForSeconds(0.1f);

        if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= attackRange + 0.5f)
        {
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator PerformRangedAttack()
    {
        if (attackSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = attackSprite;
            
        yield return new WaitForSeconds(0.1f);
        
        if (playerTransform == null) yield break;

        float aimDirection = Mathf.Sign(playerTransform.position.x - transform.position.x);
        Vector3 startPos = transform.position + new Vector3(aimDirection * 1.5f, 0.5f, 0f);
        Vector3 endPos = playerTransform.position + new Vector3(0, 0.5f, 0);

        CreateLaserBeam(startPos, endPos, 0.2f, false);

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator PerformTurretAttack()
    {
        if (playerTransform == null) yield break;

        // 1. 레이더 경고 모드 (노란색, 정지)
        if (turretScanner != null) turretScanner.SetWarningMode(true);
        
        float aimDirection = Mathf.Sign(playerTransform.position.x - transform.position.x);
        Vector3 startPos = transform.position + new Vector3(aimDirection * 1.5f, 0.5f, 0f);
        Vector3 endPos = playerTransform.position + new Vector3(0, 0.5f, 0);

        // 2. 가느다란 경고 레이저 선 표시
        CreateLaserBeam(startPos, endPos, 0.5f, true);

        // 3. 0.5초 경고 대기
        yield return new WaitForSeconds(0.5f);
        
        if (turretScanner != null) turretScanner.SetWarningMode(false);
        if (playerTransform == null) yield break;

        // 4. 실제 레이저 발사
        if (attackSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = attackSprite;
            
        endPos = playerTransform.position + new Vector3(0, 0.5f, 0); // 위치 재갱신
        CreateLaserBeam(startPos, endPos, 0.2f, false);

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
        yield return new WaitForSeconds(0.2f);
    }

    IEnumerator PerformHeavyAttack()
    {
        if (attackSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = attackSprite;
            
        yield return new WaitForSeconds(0.05f); // 딜레이 최소화 (지속 사격 느낌)
        
        if (playerTransform == null) yield break;

        float aimDirection = Mathf.Sign(playerTransform.position.x - transform.position.x);
        Vector3 startPos = transform.position + new Vector3(aimDirection * 1.5f, 0.5f, 0f);
        Vector3 endPos = playerTransform.position + new Vector3(0, 0.5f, 0);

        CreateLaserBeam(startPos, endPos, 0.1f, false); // 매우 짧은 유지

        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator PerformBossAttack()
    {
        if (playerTransform == null) yield break;

        // 패턴 랜덤 선택 (0: 레이저 난사, 1: 미사일 폭격, 2: 점프 내려찍기)
        int pattern = UnityEngine.Random.Range(0, 3);
        
        float aimDirection = Mathf.Sign(playerTransform.position.x - transform.position.x);
        FaceDirection(aimDirection);

        if (pattern == 0)
        {
            // 패턴 0: 레이저 난사 (3회 연속 발사)
            for (int i = 0; i < 3; i++)
            {
                if (playerTransform == null) break;
                aimDirection = Mathf.Sign(playerTransform.position.x - transform.position.x);
                FaceDirection(aimDirection);

                Vector3 startPos = transform.position + new Vector3(aimDirection * 2.0f, 0.5f, 0f);
                Vector3 endPos = playerTransform.position + new Vector3(0, 0.5f, 0);
                
                CreateLaserBeam(startPos, endPos, 0.2f, false);
                
                if (spriteRenderer != null) StartCoroutine(RecoilEffect(aimDirection));

                if (playerHealth != null && Vector2.Distance(transform.position, playerTransform.position) <= attackRange)
                {
                    playerHealth.TakeDamage(attackDamage * 0.7f); // 난사 패턴은 데미지 약간 감소
                }
                
                yield return new WaitForSeconds(0.4f);
            }
        }
        else if (pattern == 1)
        {
            // 패턴 1: 미사일 폭격 (3발 다발 사격)
            Vector3 cannonOffset = new Vector3(aimDirection * 2.5f, 2.0f, 0f);
            Vector3 startPos = transform.position + cannonOffset;
            Vector3 targetAimPos = playerTransform.position + new Vector3(0, 0.5f, 0);

            // 경고 레이저
            CreateLaserBeam(startPos, targetAimPos, 1.0f, true);
            yield return new WaitForSeconds(1.0f);

            if (playerTransform == null) yield break;

            if (spriteRenderer != null) StartCoroutine(RecoilEffect(aimDirection));

            // 미사일 3발 흩뿌려 발사
            GameObject missilePrefabObj = Resources.Load<GameObject>("Prefabs/TankMissilePrefab");
            AudioClip fireSound = Resources.Load<AudioClip>("Sounds/MissileLaunch");

            for (int i = -1; i <= 1; i++)
            {
                Vector3 finalAimPos = playerTransform.position + new Vector3(0, 0.5f, 0);
                // 각도를 약간씩 틀어서 발사
                Vector2 baseDir = (finalAimPos - startPos).normalized;
                float angle = Mathf.Atan2(baseDir.y, baseDir.x) + (i * 15f * Mathf.Deg2Rad);
                Vector2 fireDir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

                if (missilePrefabObj != null)
                {
                    GameObject missile = Instantiate(missilePrefabObj, startPos, Quaternion.identity);
                    TankMissile tm = missile.GetComponent<TankMissile>();
                    if (tm != null) tm.Initialize(fireDir);
                }
            }
            if (fireSound != null && audioSource != null) audioSource.PlayOneShot(fireSound, 0.9f);
        }
        else if (pattern == 2)
        {
            // 패턴 2: 점프 내려찍기 (광역 데미지 + 화면 흔들림)
            isStunned = true; // 점프 중 넉백 방지
            Vector3 startPos = transform.position;
            Vector3 targetPos = new Vector3(playerTransform.position.x, startPos.y, startPos.z);
            
            // 점프 중 플레이어를 밀어내서 바닥을 뚫는 현상 방지를 위해 충돌 임시 무시
            Collider2D myCol = GetComponent<Collider2D>();
            Collider2D[] playerCols = playerTransform != null ? playerTransform.GetComponentsInChildren<Collider2D>() : new Collider2D[0];
            if (myCol != null && playerCols.Length > 0)
            {
                foreach (var pc in playerCols) Physics2D.IgnoreCollision(myCol, pc, true);
            }

            // 위로 점프 (충돌 무시를 위해 isKinematic 임시 전환)
            rb.isKinematic = true;
            rb.linearVelocity = Vector2.zero;
            
            float jumpHeight = 10f;
            float jumpDuration = 0.8f;
            float elapsed = 0f;

            // 올라가기
            while (elapsed < jumpDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / jumpDuration;
                float currentY = Mathf.Lerp(startPos.y, startPos.y + jumpHeight, Mathf.Sin(t * Mathf.PI / 2f));
                float currentX = Mathf.Lerp(startPos.x, targetPos.x, t);
                transform.position = new Vector3(currentX, currentY, startPos.z);
                yield return null;
            }

            // 공중 체공 (잠시 타겟팅)
            yield return new WaitForSeconds(0.3f);
            
            if (playerTransform != null)
            {
                float targetX = playerTransform.position.x;
                float targetY = startPos.y;

                // 타겟 위치의 실제 바닥 찾기
                RaycastHit2D groundHit = Physics2D.Raycast(new Vector2(targetX, transform.position.y), Vector2.down, 30f, LayerMask.GetMask("Ground", "Default"));
                if (groundHit.collider != null && !groundHit.collider.isTrigger && groundHit.collider.gameObject.name != "Player")
                {
                    targetY = groundHit.point.y;
                    Collider2D col = GetComponent<Collider2D>();
                    if (col != null) targetY += col.bounds.extents.y;
                    else targetY += 1.5f; 
                }

                targetPos = new Vector3(targetX, targetY, startPos.z);
            }

            // 빠르게 떨어지기
            elapsed = 0f;
            float fallDuration = 0.2f;
            Vector3 fallStartPos = transform.position;

            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fallDuration;
                transform.position = Vector3.Lerp(fallStartPos, targetPos, t);
                yield return null;
            }

            transform.position = targetPos;
            rb.isKinematic = false;
            isStunned = false;

            // 착지 파티클
            if (landParticlePrefab != null)
            {
                Instantiate(landParticlePrefab, transform.position, Quaternion.identity);
            }

            // 화면 흔들림
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null) cam.Shake(0.5f, 0.5f);
            
            // 쿵 소리
            EnsureAudioSetup();
            if (audioSource != null && footstepSound != null)
            {
                audioSource.PlayOneShot(footstepSound, 1.0f);
            }

            // 광역 데미지 처리
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 5f);
            foreach (var hit in hits)
            {
                if (hit.CompareTag("Player"))
                {
                    // 파묻힘 방지를 위해 플레이어를 보스 바깥으로 살짝 밀어내기 (텔레포트 + 넉백)
                    float dir = (hit.transform.position.x > transform.position.x) ? 1f : -1f;
                    if (hit.transform.position.x == transform.position.x) dir = 1f;
                    hit.transform.position = new Vector3(transform.position.x + dir * 3f, hit.transform.position.y + 0.5f, hit.transform.position.z);
                    
                    Rigidbody2D prb = hit.GetComponent<Rigidbody2D>();
                    if (prb != null)
                    {
                        prb.linearVelocity = new Vector2(0f, prb.linearVelocity.y);
                        prb.AddForce(new Vector2(dir * 5f, 5f), ForceMode2D.Impulse);
                    }

                    PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                    float baseSmashDmg = 30f;
                    if (GameManager.Instance != null) baseSmashDmg *= GameManager.Instance.GetDifficultyMultiplier();
                    if (ph != null) ph.TakeDamage(baseSmashDmg); // 광역 데미지 난이도 비례
                }
            }

            // 충격파 시각 효과 (간단히 레이저 선을 바닥에 짧게 표시)
            CreateLaserBeam(transform.position + Vector3.left * 5f, transform.position + Vector3.right * 5f, 0.3f, false);
        }
        
        yield return new WaitForSeconds(1.0f); // 패턴 종료 후 추가 대기
    }

    IEnumerator RecoilEffect(float aimDirection)
    {
        Vector3 originalPos = spriteRenderer.transform.localPosition;
        Vector3 recoilPos = originalPos - new Vector3(aimDirection * 0.3f, 0, 0);
        
        float time = 0;
        while(time < 0.05f)
        {
            time += Time.deltaTime;
            spriteRenderer.transform.localPosition = Vector3.Lerp(originalPos, recoilPos, time / 0.05f);
            yield return null;
        }
        
        time = 0;
        while(time < 0.2f)
        {
            time += Time.deltaTime;
            spriteRenderer.transform.localPosition = Vector3.Lerp(recoilPos, originalPos, time / 0.2f);
            yield return null;
        }
        spriteRenderer.transform.localPosition = originalPos;
    }

    void CreateLaserBeam(Vector3 startPos, Vector3 endPos, float duration, bool isWarning = false)
    {
        EnsureAudioSetup();
        
        if (!isWarning)
        {
            if (audioSource != null && laserSound != null)
            {
                audioSource.PlayOneShot(laserSound, 0.8f);
            }
        }
        else
        {
            // 경고 사운드
            AudioClip warnClip = Resources.Load<AudioClip>("Sounds/WarningBeep");
            if (warnClip != null && audioSource != null)
            {
                audioSource.PlayOneShot(warnClip, 0.6f);
            }
        }

        GameObject laserObj = new GameObject("LaserBeam");
        laserObj.transform.position = (startPos + endPos) / 2f;
        
        Vector3 dir = endPos - startPos;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        laserObj.transform.rotation = Quaternion.Euler(0, 0, angle);
        
        SpriteRenderer laserSr = laserObj.AddComponent<SpriteRenderer>();
        if (laserSprite == null)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            laserSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        if (laserSprite != null)
        {
            laserSr.sprite = laserSprite;
            laserSr.sortingOrder = 10;
            float distance = dir.magnitude;
            float spriteWidth = laserSprite.bounds.size.x;
            if (spriteWidth > 0)
            {
                float thickness = isWarning ? 0.05f : 0.2f; // 경고선은 매우 얇게, 공격선도 얇은 레이저처럼
                laserObj.transform.localScale = new Vector3(distance / spriteWidth, thickness, 1f);
                if (isWarning) 
                {
                    laserSr.color = new Color(1f, 0.5f, 0f, 0.5f); // 경고 시 주황색 반투명
                }
                else
                {
                    laserSr.color = new Color(1f, 0.2f, 0.2f, 0.9f); // 일반 발사 시 붉은색
                }
            }
        }
        
        Destroy(laserObj, duration);
    }

    IEnumerator PerformGrenadeAttack()
    {
        if (attackSprite != null && spriteRenderer != null)
            spriteRenderer.sprite = attackSprite;
            
        yield return new WaitForSeconds(0.2f); // 던지기 전 모션 딜레이
        
        if (playerTransform == null || grenadePrefab == null) yield break;

        float aimDirection = Mathf.Sign(playerTransform.position.x - transform.position.x);
        Vector3 startPos = transform.position + new Vector3(aimDirection * 1.0f, 1.0f, 0f);

        GameObject grenade = Instantiate(grenadePrefab, startPos, Quaternion.identity);
        Rigidbody2D grb = grenade.GetComponent<Rigidbody2D>();
        if (grb != null)
        {
            // 플레이어 쪽으로 포물선 던지기
            float distToPlayer = playerTransform.position.x - transform.position.x;
            float forceX = distToPlayer * 0.8f; // 거리 비례 X 힘
            float forceY = 5f; // 고정된 위로 던지는 힘
            grb.AddForce(new Vector2(forceX, forceY), ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(0.2f);
    }

    void OnDie()
    {
        ChangeState(EnemyState.Dead);
        rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.enabled = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isLanded && (collision.gameObject.name.StartsWith("Ground") || collision.gameObject.name.StartsWith("Platform")))
        {
            isLanded = true;
            if (landParticlePrefab != null)
            {
                Vector3 spawnPos = transform.position - new Vector3(0, GetComponent<CapsuleCollider2D>().size.y / 2f, 0);
                GameObject particle = Instantiate(landParticlePrefab, spawnPos, Quaternion.identity);
                Destroy(particle, 1.5f);
            }
        }
    }

    private void EnsureAudioSetup()
    {
        if (audioSource == null) 
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) 
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.volume = 0.5f;
            }
        }
        if (footstepSound == null) footstepSound = Resources.Load<AudioClip>("Sounds/Footstep");
        if (laserSound == null) laserSound = Resources.Load<AudioClip>("Sounds/Laser");
    }
}
