using UnityEngine;

/// <summary>
/// 메탈 하트비트: 플레이어 핵심 컨트롤러
/// 이동, 점프, 엎드리기 및 8방향 사격을 처리합니다.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("이동 설정 (Movement)")]
    [Tooltip("기본 이동 속도")]
    public float moveSpeed = 5f;
    [Tooltip("점프력")]
    public float jumpForce = 12f;
    
    [Header("물리 및 판정 (Physics)")]
    [Tooltip("바닥 판정을 위한 레이캐스트 거리")]
    public float groundCheckDistance = 0.1f;
    [Tooltip("바닥으로 인식할 레이어")]
    public LayerMask groundLayer;

    // 컴포넌트 참조
    private Rigidbody2D rb;
    private BoxCollider2D col;

    // 상태 변수
    private bool isGrounded;
    private bool isDucking;
    private Vector2 inputVector;
    private Vector2 aimDirection = Vector2.right; // 기본 사격 방향

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        
        // 회전 고정 및 중력 설정 최적화
        rb.freezeRotation = true;
    }

    private void Update()
    {
        HandleInput();
        UpdateAimDirection();
        HandleJump();
        HandleShooting();
    }

    private void FixedUpdate()
    {
        CheckGrounded();
        HandleMovement();
    }

    /// <summary>
    /// 사용자 입력(WASD)을 처리합니다.
    /// </summary>
    private void HandleInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        inputVector = new Vector2(moveX, moveY);

        // 아래 방향키(S)를 누르고 있고 바닥에 있다면 엎드리기 상태
        isDucking = (inputVector.y < -0.5f && isGrounded);
    }

    /// <summary>
    /// 물리 기반의 좌우 이동을 처리합니다.
    /// </summary>
    private void HandleMovement()
    {
        // 엎드려 있을 때는 이동 불가
        if (isDucking)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        // 좌우 이동 (Rigidbody 속도 직접 제어)
        rb.linearVelocity = new Vector2(inputVector.x * moveSpeed, rb.linearVelocity.y);

        // 캐릭터 좌우 반전 (Flip)
        if (inputVector.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(inputVector.x), 1, 1);
        }
    }

    /// <summary>
    /// 점프 로직을 처리합니다.
    /// </summary>
    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded && !isDucking)
        {
            // 수직 방향으로 점프력 가함
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    /// <summary>
    /// 바닥 충돌 여부를 Raycast로 정밀하게 검사합니다.
    /// </summary>
    private void CheckGrounded()
    {
        // 콜라이더의 하단 중앙 위치 계산
        Vector2 boundsCenter = col.bounds.center;
        Vector2 boundsBottom = new Vector2(boundsCenter.x, col.bounds.min.y);

        // 아래쪽으로 레이저를 쏴서 바닥 레이어와 충돌하는지 확인
        RaycastHit2D hit = Physics2D.Raycast(boundsBottom, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;

        // 디버그용 선 그리기 (에디터 씬 뷰에서 확인 가능)
        Debug.DrawRay(boundsBottom, Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    /// <summary>
    /// WASD 입력 조합을 통해 8방향 사격 각도를 결정합니다.
    /// </summary>
    private void UpdateAimDirection()
    {
        if (inputVector != Vector2.zero)
        {
            // 입력 벡터를 정규화하여 8방향 벡터로 변환
            aimDirection = inputVector.normalized;
        }
        else
        {
            // 입력이 없을 때는 캐릭터가 바라보는 기본 앞 방향
            aimDirection = new Vector2(Mathf.Sign(transform.localScale.x), 0);
        }
    }

    /// <summary>
    /// 사격 입력을 처리하고 총알을 발사합니다.
    /// </summary>
    private void HandleShooting()
    {
        // Fire1은 주로 마우스 좌클릭이나 J/Z 키에 매핑됩니다.
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    /// <summary>
    /// 실제 총알을 발사하는 로직입니다.
    /// </summary>
    private void Shoot()
    {
        // 추후 무기 시스템 매니저나 오브젝트 풀에서 총알을 가져와 발사하도록 변경
        Debug.Log($"[{gameObject.name}] 발사! 방향: {aimDirection}");
        
        // 예: 무기 인벤토리에서 현재 무기를 가져와 Fire(aimDirection) 호출
    }
}
