using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    
    [Header("State")]
    public bool canShoot = false; // 기본적으로 꺼둠 (아이템 획득 후 켜짐)
    
    [Header("Auto Shoot Skill")]
    public float autoShootDuration = 5f;
    public float autoShootCooldown = 15f;
    public float autoFireRate = 0.1f;
    
    [Header("Manual Shoot")]
    public float fireRate = 0.15f;
    private float lastFireTime = 0f;
    private float lastAutoShootTime = -100f;
    private bool isAutoShooting = false;
    private float nextAutoShootTime = 0f;
    private float autoShootEndTime = 0f;

    private SpriteRenderer spriteRenderer;
    public float currentDamage = 35f;
    
    [Header("Audio")]
    private AudioSource audioSource;
    private AudioClip gunSound;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            autoShootCooldown = stats.GetAutoShootCooldown();
            currentDamage = stats.GetAttackPower();
        }

        // 시작할 때 총 오브젝트를 시각적으로도 숨겨둡니다.
        Transform gun = transform.Find("Gun");
        if (gun != null)
        {
            gun.gameObject.SetActive(canShoot);
            // 총이 무조건 보이도록 런타임 강제 보정 (스케일, 소팅 오더, 위치)
            gun.localScale = new Vector3(1f, 1f, 1f);
            gun.localPosition = new Vector3(-1.5f, -0.2f, 0f);
            SpriteRenderer gunSr = gun.GetComponent<SpriteRenderer>();
            if (gunSr != null)
            {
                gunSr.sortingOrder = 15;
            }
        }

        // 사운드 세팅
        audioSource = gameObject.AddComponent<AudioSource>();
        gunSound = Resources.Load<AudioClip>("Sounds/Gunshot");
    }

    public void UpdateAutoShootCooldown(float newCooldown)
    {
        autoShootCooldown = newCooldown;
    }

    public void UpdateAttackPower(float newDamage)
    {
        currentDamage = newDamage;
    }

    [Header("Drop Settings")]
    public float dropHoldTime = 3f;
    private float currentDropHoldTime = 0f;
    public GameObject weaponPickupPrefab;

    void Update()
    {
        if (Time.timeScale == 0) return; // 게임 일시정지 시 입력을 무시합니다.

        // 무기 아이템을 먹기 전까지는 총이 돌아가거나 발사되지 않음
        if (!canShoot) return;

        // 마우스 입력 장치 확인
        if (Mouse.current == null || Camera.main == null) return;

        // 무기 버리기 기능(F키) 삭제 (버그 원인이 되며 사용되지 않음)

        // 1. 마우스 위치 계산
        Vector2 mouseScreenPos = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));

        // 2. 총(Gun)이 마우스 포인터 방향을 향하도록 설정
        Transform gun = transform.Find("Gun");
        if (gun != null)
        {
            if (!gun.gameObject.activeSelf) gun.gameObject.SetActive(true);

            // 마우스 위치를 플레이어의 로컬 좌표계로 변환
            Vector3 targetLocalPos = transform.InverseTransformPoint(mouseWorldPos);
            
            // 방향 계산 (목표 - 현재위치)
            Vector3 localDir = targetLocalPos - gun.localPosition;
            
            // 만약 총구와 마우스가 너무 가까우면 회전을 무시하여 뒤집힘 현상 방지
            if (localDir.sqrMagnitude > 0.001f)
            {
                // 각도 계산
                float angle = Mathf.Atan2(localDir.y, localDir.x) * Mathf.Rad2Deg;
                
                // 총의 로컬 회전값 적용 (마우스 방향)
                gun.localRotation = Quaternion.Euler(0, 0, angle);
                
                // 총과 하위 오브젝트(firePoint 등)가 모두 올바르게 뒤집히도록 localScale.y 조절
                // (총 사이즈를 줄이기 위해 기본 스케일을 0.5로 적용합니다)
                float flipScaleY = Mathf.Abs(angle) > 90f ? -0.5f : 0.5f;
                gun.localScale = new Vector3(0.5f, flipScaleY, 1f);
            }
        }

        // Q 스킬: 오토 샷 발동
        if (Keyboard.current != null && Keyboard.current.qKey.wasPressedThisFrame)
        {
            if (Time.time >= lastAutoShootTime + autoShootCooldown)
            {
                isAutoShooting = true;
                lastAutoShootTime = Time.time;
                autoShootEndTime = Time.time + autoShootDuration;
                Debug.Log("오토 샷(Q) 발동! 5초간 자동 연사!");
            }
        }

        // 오토 샷 유지시간 체크
        if (isAutoShooting)
        {
            if (Time.time > autoShootEndTime)
            {
                isAutoShooting = false;
            }
            else
            {
                // 오토 샷 중에는 지정된 속도로 자동 발사
                if (Time.time >= nextAutoShootTime)
                {
                    Shoot(mouseWorldPos);
                    nextAutoShootTime = Time.time + autoFireRate;
                }
            }
        }

        // 3. 발사 (수동 사격) - 오토 샷 중에도 수동 클릭 가능 (고장났다고 느끼지 않도록)
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Shoot(mouseWorldPos);
            lastFireTime = Time.time;
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            if (Time.time >= lastFireTime + fireRate)
            {
                Shoot(mouseWorldPos);
                lastFireTime = Time.time;
            }
        }
    }

    void Shoot(Vector3 mouseWorldPos)
    {
        if (bulletPrefab == null) 
        {
            bulletPrefab = Resources.Load<GameObject>("Prefabs/BulletPrefab");
            if (bulletPrefab == null) return;
        }

        Transform activeFirePoint = firePoint;
        if (activeFirePoint == null)
        {
            Transform gun = transform.Find("Gun");
            if (gun != null) activeFirePoint = gun.Find("firePoint");
        }

        Vector3 spawnPos = activeFirePoint != null ? activeFirePoint.position : transform.position;
        Quaternion spawnRot = activeFirePoint != null ? activeFirePoint.rotation : transform.rotation;

        if (audioSource == null) 
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
        if (gunSound == null) gunSound = Resources.Load<AudioClip>("Sounds/Gunshot");

        if (audioSource != null && gunSound != null)
        {
            audioSource.PlayOneShot(gunSound);
        }

        // 총알 생성
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, spawnRot);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        
        if (rb != null)
        {
            // 총구에서 마우스를 향하는 방향 벡터 계산
            Vector2 direction = mouseWorldPos - spawnPos;
            
            // 마우스가 총구에 너무 가까우면 벡터가 0이 되어 속도가 줄어드는 버그 방지
            if (direction.sqrMagnitude < 0.01f)
            {
                // 플레이어가 바라보는 방향(스케일 반전)을 고려하여 총구 정면 방향 강제 지정
                direction = activeFirePoint != null ? activeFirePoint.right * Mathf.Sign(transform.lossyScale.x) : transform.right * Mathf.Sign(transform.lossyScale.x);
            }
            
            // 방향 벡터의 길이를 1로 정규화
            direction.Normalize();
            
            // 해당 방향으로 총알 발사
            rb.linearVelocity = direction * bulletSpeed;
            
            // 총알이 날아가는 방향을 바라보도록 이미지 회전
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bullet.transform.rotation = Quaternion.Euler(0, 0, angle);
            
            SpriteRenderer bulletSr = bullet.GetComponent<SpriteRenderer>();
            if (bulletSr != null)
            {
                bulletSr.flipX = false;
            }
            
            Bullet bScript = bullet.GetComponent<Bullet>();
            if (bScript != null) bScript.damage = currentDamage;
        }
    }

    public void EnableShooting()
    {
        this.enabled = true; // 스크립트 강제 활성화 보장
        canShoot = true;
        isAutoShooting = false;
        
        // 아이템을 먹으면 총을 다시 화면에 보여줍니다.
        Transform gun = transform.Find("Gun");
        if (gun != null)
        {
            gun.gameObject.SetActive(true);
            gun.localScale = new Vector3(0.5f, 0.5f, 1f); // 총 크기 0.5로 축소
            gun.localPosition = new Vector3(-1.5f, -0.2f, 0f);
            SpriteRenderer gunSr = gun.GetComponent<SpriteRenderer>();
            if (gunSr != null)
            {
                gunSr.sortingOrder = 15;
            }
            
            Transform firePointObj = gun.Find("firePoint");
            if (firePointObj != null)
            {
                // 스케일 유지
                firePointObj.localScale = new Vector3(1f, 1f, 1f);
            }
        }
        
        Debug.Log("사격 기능이 활성화되었습니다!");
    }

    public void HideShooting()
    {
        canShoot = false;
        isAutoShooting = false;
        Transform gun = transform.Find("Gun");
        if (gun != null)
        {
            gun.gameObject.SetActive(false);
        }
    }

    public float GetAutoShootCooldownPercent()
    {
        if (Time.time >= lastAutoShootTime + autoShootCooldown) return 0f;
        
        float timePassed = Time.time - lastAutoShootTime;
        return 1f - (timePassed / autoShootCooldown);
    }

    // DropWeapon 메서드 삭제 (버그 원인)
}
