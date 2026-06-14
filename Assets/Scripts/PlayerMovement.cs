using UnityEngine;
using UnityEngine.InputSystem; // 새로운 Input System 사용

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f; // 점프력을 기존 10에서 7로 낮춤
    public int maxJumps = 2; // 최대 점프 횟수 (더블 점프)

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded = false;
    private int jumpCount = 0;
    
    [Header("Respawn System")]
    private Vector3 lastSafePosition;
    
    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing = false;
    private float lastDashTime = -100f;
    
    [Header("Effects")]
    public ParticleSystem boosterParticle;
    
    [Header("Crouch")]
    public Sprite crouchSprite;
    private Sprite normalSprite;
    private bool isCrouching = false;
    private BoxCollider2D playerCollider;
    private Vector2 normalColliderSize;
    private Vector2 normalColliderOffset;
    
    private float baseScale; // 원본 크기 저장용

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerCollider = GetComponent<BoxCollider2D>();
        
        if (spriteRenderer != null) normalSprite = spriteRenderer.sprite;
        if (playerCollider != null)
        {
            normalColliderSize = playerCollider.size;
            normalColliderOffset = playerCollider.offset;
        }
        
        // 현재 캐릭터의 스케일을 기준값으로 저장
        baseScale = Mathf.Abs(transform.localScale.y);

        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null) dashCooldown = stats.GetDashCooldown();
        
        lastSafePosition = transform.position;
    }

    public void UpdateDashCooldown(float newCooldown)
    {
        dashCooldown = newCooldown;
    }

    [Header("Skill Dash")]
    public bool isSkillDashing = false; // PlayerMelee 등의 스킬로 대시 중일 때

    private void LateUpdate()
    {
        ClampToScreen();
    }

    // 카메라 화면 밖으로 플레이어가 나가는 것을 방지 (특히 웨이브 중 카메라 잠겼을 때 무시하고 지나가는 버그 방지)
    private void ClampToScreen()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 viewportPos = cam.WorldToViewportPoint(transform.position);
            
            bool clamped = false;
            // 오른쪽 화면 밖으로 나가는 것 방지
            if (viewportPos.x > 0.95f)
            {
                viewportPos.x = 0.95f;
                clamped = true;
            }
            
            // 메탈슬러그 방식 뒤로가기 잠금 시, 왼쪽 화면 밖으로 나가는 것 방지
            CameraFollow camFollow = cam.GetComponent<CameraFollow>();
            if (camFollow != null && camFollow.lockBackwardMovement)
            {
                if (viewportPos.x < 0.05f)
                {
                    viewportPos.x = 0.05f;
                    clamped = true;
                }
            }

            if (clamped)
            {
                Vector3 worldPos = cam.ViewportToWorldPoint(viewportPos);
                transform.position = new Vector3(worldPos.x, transform.position.y, transform.position.z);
            }
        }
    }

    private Vector3 _lastFramePos;
    void Update()
    {
        if (Vector3.Distance(transform.position, _lastFramePos) > 5f && Time.time > 1f) {
            Debug.Log("[TELEPORT DEBUG] Player teleported from " + _lastFramePos + " to " + transform.position + " \nTrace: " + StackTraceUtility.ExtractStackTrace());
        }
        _lastFramePos = transform.position;
        
        if (Time.timeScale == 0) return; // 게임 일시정지 시 입력을 무시합니다.
        
        if (isDashing || isSkillDashing) return; // 일반 대시나 스킬 대시 중에는 다른 입력을 받지 않습니다.

        // 1. 새로운 Input System을 활용한 좌우 이동 (A, D 키 또는 좌우 방향키)
        float moveInput = 0f;
        
        if (Keyboard.current != null)
        {
            // 쪼그려 앉기 (Z키)
            if (Keyboard.current.zKey.isPressed)
            {
                if (!isCrouching) Crouch();
            }
            else
            {
                if (isCrouching) StandUp();
            }

            if (!isCrouching) // 쪼그려 앉은 상태에서는 이동 불가
            {
                if (Keyboard.current.leftArrowKey.isPressed || Keyboard.current.aKey.isPressed)
                    moveInput = -1f;
                else if (Keyboard.current.rightArrowKey.isPressed || Keyboard.current.dKey.isPressed)
                    moveInput = 1f;
            }
        }

        Vector2 targetVelocity;
        if (isGrounded && Mathf.Abs(moveInput) > 0.1f && _groundNormal.y > 0.1f && _groundNormal.y < 0.99f)
        {
            // 경사면의 기울기에 맞춰서 속도 벡터 회전
            Vector2 slopeDirection = new Vector2(_groundNormal.y, -_groundNormal.x);
            if (moveInput < 0) slopeDirection = -slopeDirection;
            targetVelocity = slopeDirection * moveSpeed;
        }
        else
        {
            targetVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
        }
        rb.linearVelocity = targetVelocity;

        // 부스터 파티클 제어 (움직일 때만 활성화)
        if (boosterParticle != null)
        {
            var emission = boosterParticle.emission;
            if (Mathf.Abs(moveInput) > 0.1f)
            {
                emission.enabled = true;
            }
            else
            {
                emission.enabled = false;
            }
        }

        // 2. 캐릭터 방향 반전 (마우스 커서 기준)
        if (Mouse.current != null)
        {
            Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));
            
            if (mouseWorldPos.x > transform.position.x)
                transform.localScale = new Vector3(-baseScale, baseScale, 1f);  // 마우스가 오른쪽에 있으면 오른쪽 보기
            else
                transform.localScale = new Vector3(baseScale, baseScale, 1f);   // 마우스가 왼쪽에 있으면 왼쪽 보기
        }

        // 3. 점프 (Space 키)
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // 바닥에 있거나 남은 점프 횟수가 있을 때 점프 가능
            if (isGrounded || jumpCount < maxJumps)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpCount++; // 점프 횟수 증가
            }
        }

        // 4. 대시 (Shift 키)
        if (Keyboard.current != null && Keyboard.current.shiftKey.wasPressedThisFrame)
        {
            if (Time.time >= lastDashTime + dashCooldown)
            {
                // 입력 방향이 없으면 캐릭터가 바라보는 방향으로 대시
                float dashDir = moveInput;
                if (dashDir == 0)
                {
                    // scale.x가 음수면 오른쪽, 양수면 왼쪽을 바라보는 상태
                    dashDir = transform.localScale.x < 0 ? 1f : -1f;
                }
                StartCoroutine(DashRoutine(dashDir));
            }
        }

        // 5. 안전한 지상 위치 저장 (낙사/물에 빠졌을 때 복귀용)
        if (isGrounded)
        {
            lastSafePosition = transform.position;
        }
    }

    private System.Collections.IEnumerator DashRoutine(float direction)
    {
        isDashing = true;
        lastDashTime = Time.time;

        // 대시 중에는 중력을 끄고 직진하게 만듭니다.
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(direction * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        // 대시 종료 후 원상복구
        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    // 바닥에 닿았는지 체크 (이름 기반에서 물리 방향 기반으로 변경하여 모든 맵 호환)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 만약 물 웅덩이가 트리거가 아니라 일반 콜라이더라면 여기서 처리
        if (collision.gameObject.name.ToLower().Contains("water"))
        {
            HandleWaterHazard();
            return;
        }
        
        CheckGrounded(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.name.ToLower().Contains("water")) return;
        CheckGrounded(collision);
    }

    private Vector2 _groundNormal = Vector2.up;

    private void CheckGrounded(Collision2D collision)
    {
        // 충돌한 면의 방향이 위쪽(바닥)인지 확인
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
            jumpCount = 0; // 땅에 닿으면 점프 횟수 초기화
            _groundNormal = collision.contacts[0].normal;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
        _groundNormal = Vector2.up;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Player triggered with: " + collision.gameObject.name);
        
        // 물에 빠졌을 때 처리
        if (collision.gameObject.name.ToLower().Contains("water"))
        {
            HandleWaterHazard();
        }
    }

    private void HandleWaterHazard()
    {
        Debug.Log("Water detected! Taking damage and teleporting...");
        // 체력 감소
        PlayerHealth health = GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(15); // 물에 빠지면 15 데미지
        }

        // 떨어지기 전 안전했던 위치보다 살짝 위쪽, 뒤쪽으로 복귀
        // (뒤로 복귀할 때 벽에 끼지 않도록 살짝만 위로)
        Vector3 respawnPos = lastSafePosition + new Vector3(0f, 3f, 0);
        transform.position = respawnPos;
        
        // 추락하던 가속도 초기화
        if (rb != null) rb.linearVelocity = Vector2.zero;
        isGrounded = false;
    }

    [Header("Crouch Float Fix")]
    public float crouchFloatFixOffset = 0.6f; // 쪼그려 앉을 때 공중에 뜨는 것을 방지하기 위해 추가로 콜라이더를 올리는 수치

    private void Crouch()
    {
        isCrouching = true;
        if (spriteRenderer != null && crouchSprite != null)
        {
            spriteRenderer.sprite = crouchSprite;
        }
        
        float actualOffset = crouchFloatFixOffset == 0f ? 0.6f : crouchFloatFixOffset;

        Collider2D[] cols = GetComponents<Collider2D>();
        foreach(var col in cols)
        {
            if (col is BoxCollider2D box)
            {
                box.size = new Vector2(normalColliderSize.x, normalColliderSize.y * 0.5f);
                box.offset = new Vector2(normalColliderOffset.x, normalColliderOffset.y - (normalColliderSize.y * 0.25f) + actualOffset);
            }
            else if (col is CapsuleCollider2D cap)
            {
                cap.size = new Vector2(cap.size.x, normalColliderSize.y * 0.5f);
                cap.offset = new Vector2(cap.offset.x, normalColliderOffset.y - (normalColliderSize.y * 0.25f) + actualOffset);
            }
        }
    }

    private void StandUp()
    {
        isCrouching = false;
        if (spriteRenderer != null && normalSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }
        
        Collider2D[] cols = GetComponents<Collider2D>();
        foreach(var col in cols)
        {
            if (col is BoxCollider2D box)
            {
                box.size = normalColliderSize;
                box.offset = normalColliderOffset;
            }
            else if (col is CapsuleCollider2D cap)
            {
                cap.size = new Vector2(cap.size.x, normalColliderSize.y);
                cap.offset = new Vector2(cap.offset.x, normalColliderOffset.y);
            }
        }
    }

    public void UpdateNormalSprite(Sprite newSprite)
    {
        normalSprite = newSprite;
        if (!isCrouching && spriteRenderer != null)
        {
            spriteRenderer.sprite = newSprite;
        }
    }


    public float GetDashCooldownPercent()
    {
        // 쿨타임이 얼마나 찼는지 (0 = 준비 완료, 1 = 방금 씀)
        if (Time.time >= lastDashTime + dashCooldown) return 0f;
        
        float timePassed = Time.time - lastDashTime;
        return 1f - (timePassed / dashCooldown);
    }
}
