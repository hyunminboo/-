using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMelee : MonoBehaviour
{
    [Header("Weapon Settings")]
    public Transform swordTransform; // The sword object attached to the player
    public float attackRange = 3f;
    public float attackRadius = 2.5f;
    public LayerMask enemyLayer; // Set this to the layer enemies are on

    [Header("State")]
    public bool canAttack = false; // Enabled after pickup
    public float currentDamage = 50f;
    public float attackCooldown = 0.5f;
    private float lastAttackTime = -1f;
    private bool isAttacking = false;

    [Header("Blade Dash Skill (Q)")]
    public float dashSkillCooldown = 10f;
    public float dashSpeed = 30f;
    public float dashDuration = 0.2f;
    public float dashAoeRadius = 3.5f;
    public float dashAoeDamage = 120f;
    private float lastDashSkillTime = -100f;
    private bool isDashSlashing = false;

    [Header("Drop Settings")]
    public float dropHoldTime = 3f;
    private float currentDropHoldTime = 0f;
    public GameObject weaponPickupPrefab;

    private SpriteRenderer playerRenderer;

    [Header("Audio")]
    private AudioSource audioSource;
    private AudioClip swordSound;
    void Start()
    {
        playerRenderer = GetComponent<SpriteRenderer>();
        
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            dashSkillCooldown = stats.GetBladeDashCooldown();
            currentDamage = stats.GetAttackPower();
        }

        if (swordTransform != null)
        {
            swordTransform.gameObject.SetActive(canAttack);
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        swordSound = Resources.Load<AudioClip>("Sounds/Sword");
    }

    public void UpdateDashSkillCooldown(float newCooldown)
    {
        dashSkillCooldown = newCooldown;
    }

    public void UpdateAttackPower(float newDamage)
    {
        currentDamage = newDamage;
    }

    void Update()
    {
        if (Time.timeScale == 0) return;

        // 무기 버리기 (F 키 3초 유지)
        if (canAttack && Keyboard.current != null)
        {
            if (Keyboard.current.fKey.isPressed)
            {
                currentDropHoldTime += Time.deltaTime;
                if (currentDropHoldTime >= dropHoldTime)
                {
                    DropWeapon();
                }
            }
            else
            {
                currentDropHoldTime = 0f;
            }
        }

        if (!canAttack) return;

        // 마우스 위치로 플레이어가 바라보게 함 (원래 PlayerMovement에서 처리하지만, 무기 방향도 필요할 수 있음)
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));
        
        if (swordTransform != null && !isAttacking)
        {
            // 대기 중에는 살짝 위쪽으로 들고 있는 느낌
            swordTransform.localRotation = Quaternion.Euler(0, 0, -15f);
        }

        // Q 스킬: 돌진 베기 발동
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (Time.time >= lastDashSkillTime + dashSkillCooldown && !isDashSlashing && !isAttacking)
            {
                StartCoroutine(BladeDash(mouseWorldPos));
            }
        }

        // 수동 공격 (대시 중 아닐 때만)
        if (!isDashSlashing && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (Time.time >= lastAttackTime + attackCooldown && !isAttacking)
            {
                StartCoroutine(PerformAttack(mouseWorldPos));
            }
        }
        
        // 상태 고착(Stuck) 방지 워치독: 코루틴이 알 수 없는 이유로 정지되었을 경우 1초 뒤 강제 해제
        if (isAttacking && Time.time > lastAttackTime + attackCooldown + 1.0f)
        {
            isAttacking = false;
            if (swordTransform != null) swordTransform.localRotation = Quaternion.Euler(0, 0, 0);
        }
        if (isDashSlashing && Time.time > lastDashSkillTime + dashDuration + 1.0f)
        {
            isDashSlashing = false;
        }
    }

    [Header("Effects")]
    public GameObject hitEffectPrefab;

    IEnumerator PerformAttack(Vector3 mouseWorldPos)
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        try
        {
            if (audioSource != null && swordSound != null)
            {
                audioSource.PlayOneShot(swordSound);
            }

            // 방향 결정 (스케일 x가 음수일 때 오른쪽을 바라봄)
            float dirSign = transform.localScale.x < 0 ? 1f : -1f;

            // 애니메이션 효과: 검을 위에서 아래로 베기 (로컬 로테이션 변경)
            if (swordTransform != null)
            {
                float startAngle = 90f;  // 더 크게 휘두르도록 변경
                float endAngle = -90f;
                float duration = 0.15f;
                float elapsed = 0f;
                
                TrailRenderer tr = swordTransform.GetComponent<TrailRenderer>();
                if (tr != null) {
                    tr.Clear();
                    tr.emitting = true;
                }

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float currentAngle = Mathf.Lerp(startAngle, endAngle, elapsed / duration);
                    swordTransform.localRotation = Quaternion.Euler(0, 0, currentAngle);
                    yield return null;
                }
                
                if (tr != null) tr.emitting = false;
                
                // 원래 각도로 복귀
                swordTransform.localRotation = Quaternion.Euler(0, 0, 0);
            }
            else
            {
                // 검 오브젝트가 없어도 데미지 처리를 위해 약간 대기
                yield return new WaitForSeconds(0.15f);
            }

            // 데미지 처리 - Enemy 레이어로 필터링하여 정확한 판정
            int enemyLayerMask = LayerMask.GetMask("Enemy");
            Vector2 attackCenter = (Vector2)transform.position + new Vector2(dirSign * attackRange, 0f);
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackCenter, attackRadius, enemyLayerMask);

            foreach (Collider2D enemy in hitEnemies)
            {
                EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
                if (eh != null)
                {
                    Vector2 hitDir = (enemy.transform.position - transform.position).normalized;
                    eh.TakeDamage(currentDamage, hitDir);
                    
                    // 히트 파티클 이펙트 생성
                    if (hitEffectPrefab != null)
                    {
                        Instantiate(hitEffectPrefab, enemy.transform.position, Quaternion.identity);
                    }
                }
            }
        }
        finally
        {
            isAttacking = false;
        }
    }

    // ===== 돌진 베기 스킬 =====
    IEnumerator BladeDash(Vector3 targetWorldPos)
    {
        isDashSlashing = true;
        lastDashSkillTime = Time.time;

        try
        {
            if (audioSource != null && swordSound != null)
            {
                audioSource.PlayOneShot(swordSound);
            }

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            float originalGravity = rb != null ? rb.gravityScale : 1f;

            PlayerMovement pm = GetComponent<PlayerMovement>();
            if (pm != null) pm.isSkillDashing = true;

            // 방향 계산 (마우스 위치로)
            Vector2 dashDir = ((Vector2)targetWorldPos - (Vector2)transform.position).normalized;

            // TrailRenderer 켜기
            TrailRenderer tr = swordTransform != null ? swordTransform.GetComponent<TrailRenderer>() : null;
            if (tr != null) { tr.Clear(); tr.emitting = true; }

            // 중력 끄고 고속 이동
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearVelocity = dashDir * dashSpeed;
            }

            // 검 회전 효과 (빙글빙글)
            float elapsed = 0f;
            while (elapsed < dashDuration)
            {
                elapsed += Time.deltaTime;
                if (swordTransform != null)
                    swordTransform.Rotate(0, 0, -720f * Time.deltaTime);
                yield return null;
            }

            // 멈추고 AOE 폭발
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = originalGravity;
            }

            if (pm != null) pm.isSkillDashing = false;

            if (tr != null) tr.emitting = false;
            if (swordTransform != null) swordTransform.localRotation = Quaternion.Euler(0, 0, -15f);

            // AOE 피해
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, dashAoeRadius, LayerMask.GetMask("Enemy"));
            foreach (Collider2D enemy in hitEnemies)
            {
                EnemyHealth eh = enemy.GetComponent<EnemyHealth>();
                if (eh != null)
                {
                    Vector2 hitDir = (enemy.transform.position - transform.position).normalized;
                    eh.TakeDamage(dashAoeDamage, hitDir);
                    
                    // 히트 파티클 이펙트 생성 (Q 스킬에서는 여러 적에게 동시 타격)
                    if (hitEffectPrefab != null)
                    {
                        Instantiate(hitEffectPrefab, enemy.transform.position, Quaternion.identity);
                    }
                }
            }
        }
        finally
        {
            isDashSlashing = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        float dirSign = transform.localScale.x < 0 ? 1f : -1f;
        Vector2 attackCenter = (Vector2)transform.position + new Vector2(dirSign * attackRange, 0f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackCenter, attackRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dashAoeRadius);
    }

    public void EnableMelee()
    {
        canAttack = true;
        if (swordTransform != null)
        {
            swordTransform.gameObject.SetActive(true);
        }
        Debug.Log("근접 공격(검) 기능이 활성화되었습니다!");
    }

    public void HideMelee()
    {
        canAttack = false;
        isDashSlashing = false;
        isAttacking = false;
        if (swordTransform != null)
        {
            swordTransform.gameObject.SetActive(false);
        }
    }

    public float GetDashSkillCooldownPercent()
    {
        if (Time.time >= lastDashSkillTime + dashSkillCooldown) return 0f;
        float timePassed = Time.time - lastDashSkillTime;
        return 1f - (timePassed / dashSkillCooldown);
    }

    public void DropWeapon()
    {
        canAttack = false;
        currentDropHoldTime = 0f;
        isDashSlashing = false;
        
        if (swordTransform != null)
        {
            swordTransform.gameObject.SetActive(false);
        }
        
        if (weaponPickupPrefab != null)
        {
            float dropDir = transform.localScale.x < 0 ? 1f : -1f;
            Vector3 dropPos = transform.position + new Vector3(dropDir * 1.5f, 0f, 0f);
            GameObject drop = Instantiate(weaponPickupPrefab, dropPos, Quaternion.identity);
            
            Rigidbody2D rb = drop.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.gravityScale = 0f;
            }
        }
        
        // 핫바에서 아이템 제거
        if (HotbarInventory.instance != null && HotbarInventory.instance.selectedIndex != -1)
        {
            var slot = HotbarInventory.instance.slots[HotbarInventory.instance.selectedIndex];
            if (slot.itemType == ItemType.Sword)
            {
                HotbarInventory.instance.ConsumeSelectedItem();
            }
        }

        Debug.Log("무기(검)를 버렸습니다!");
    }
}
