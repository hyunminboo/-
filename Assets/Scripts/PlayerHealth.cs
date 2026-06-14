using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("UI Reference")]
    public Image healthBarFill; 
    public Image damageVignette; // 체력 낮을 때 화면 붉어지는 연출용

    [Header("Effects")]
    public GameObject hitParticlePrefab;
    private SpriteRenderer spriteRenderer;

    // ── 위험 연출 관련 변수 ──
    private float hitFlashTimer = 0f;           // 피격 순간 플래시 타이머
    private const float HIT_FLASH_DURATION = 0.25f;
    private float currentVignetteAlpha = 0f;    // 현재 비네트 알파 (부드러운 보간용)

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) originalColor = spriteRenderer.color;
        
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null) maxHealth = stats.GetMaxHP();

        currentHealth = maxHealth;
        UpdateHealthBar();

        // 붉은색 비네트(피격/빈사 연출) UI 자동 생성 시도
        TryCreateVignette();
    }

    /// <summary>
    /// DamageVignette UI를 생성합니다. HUDCanvas가 비활성이면 활성화될 때까지 재시도됩니다.
    /// </summary>
    private bool TryCreateVignette()
    {
        if (damageVignette != null) return true;

        // 1. 먼저 활성화된 HUDCanvas를 찾음
        Canvas targetCanvas = null;
        GameObject hud = GameObject.Find("HUDCanvas");
        if (hud != null)
        {
            targetCanvas = hud.GetComponent<Canvas>();
        }

        // 2. 못 찾으면 비활성 포함 모든 Canvas에서 HUDCanvas 검색
        if (targetCanvas == null)
        {
            Canvas[] allCanvases = Resources.FindObjectsOfTypeAll<Canvas>();
            foreach (Canvas c in allCanvases)
            {
                if (c.gameObject.name == "HUDCanvas" && c.gameObject.scene.isLoaded)
                {
                    targetCanvas = c;
                    // 비활성 Canvas면 아직 생성하지 않음 (나중에 활성화되면 재시도)
                    if (!c.gameObject.activeInHierarchy)
                    {
                        return false; // 아직 비활성이므로 나중에 재시도
                    }
                    break;
                }
            }
        }

        // 3. HUDCanvas가 없으면 아무 활성 Canvas에 생성
        if (targetCanvas == null)
        {
            targetCanvas = FindObjectOfType<Canvas>();
        }

        if (targetCanvas == null) return false;

        GameObject vigObj = new GameObject("DamageVignette");
        vigObj.transform.SetParent(targetCanvas.transform, false);
        vigObj.transform.SetAsLastSibling(); // UI 가장 앞쪽에 배치

        damageVignette = vigObj.AddComponent<Image>();
        damageVignette.raycastTarget = false;

        // ── 런타임 비네트 텍스처 생성 (가장자리→중앙 그라데이션) ──
        Texture2D vignetteTex = CreateVignetteTexture(256, 256);
        damageVignette.sprite = Sprite.Create(
            vignetteTex,
            new Rect(0, 0, vignetteTex.width, vignetteTex.height),
            new Vector2(0.5f, 0.5f)
        );
        damageVignette.type = Image.Type.Simple;
        damageVignette.preserveAspect = false;
        damageVignette.color = new Color(1f, 0f, 0f, 0f); // 투명한 빨간색

        RectTransform rt = vigObj.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;

        Debug.Log("[PlayerHealth] DamageVignette 생성 완료! 부모: " + targetCanvas.gameObject.name);
        return true;
    }

    /// <summary>
    /// 런타임에 비네트(가장자리 어두운) 텍스처를 생성합니다.
    /// 중앙은 완전 투명, 가장자리는 불투명한 원형 그라데이션
    /// </summary>
    private Texture2D CreateVignetteTexture(int width, int height)
    {
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[width * height];
        
        float cx = width * 0.5f;
        float cy = height * 0.5f;
        float maxDist = Mathf.Sqrt(cx * cx + cy * cy);
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = (x - cx) / cx;
                float dy = (y - cy) / cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy); // 0 ~ √2
                
                // 중앙(dist=0)은 투명, 가장자리(dist≥0.7)부터 점점 불투명
                // 부드러운 커브로 자연스러운 비네트 생성
                float alpha = Mathf.Clamp01((dist - 0.3f) / 0.9f);
                alpha = alpha * alpha; // 제곱으로 더 부드러운 전환
                
                pixels[y * width + x] = new Color(1f, 1f, 1f, alpha);
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        return tex;
    }

    public void UpdateMaxHealth(float newMax)
    {
        float diff = newMax - maxHealth;
        maxHealth = newMax;
        currentHealth += diff;
        UpdateHealthBar();
    }

    void Update()
    {
        // 최신 Input System 패키지 대응 코드로 변경
        if (Keyboard.current != null)
        {
            if (Keyboard.current.hKey.wasPressedThisFrame)
            {
                TakeDamage(15f);
            }
            if (Keyboard.current.jKey.wasPressedThisFrame)
            {
                Heal(15f);
            }
        }

        // ── 비네트가 아직 없으면 HUDCanvas 활성화 대기 후 생성 시도 ──
        if (damageVignette == null)
        {
            TryCreateVignette();
        }

        // ── 체력이 낮을수록 화면이 점점 붉어지는 위험 연출 ──
        UpdateDangerVignette();
    }

    /// <summary>
    /// 체력 비율에 따라 화면 가장자리가 붉어지는 비네트 연출을 업데이트합니다.
    /// - 70% 이하: 약한 붉은 비네트 시작
    /// - 50% 이하: 맥박 효과 추가
    /// - 25% 이하: 강한 맥박 + 진한 붉은색
    /// - 피격 순간: 짧은 플래시
    /// - 사망: 화면 전체 붉어짐
    /// </summary>
    private void UpdateDangerVignette()
    {
        if (damageVignette == null) return;

        if (!isDead)
        {
            float healthRatio = currentHealth / maxHealth;
            float targetAlpha = 0f;

            // 체력 70% 이하부터 비네트 시작 (기존 50%보다 일찍 경고)
            if (healthRatio < 0.7f)
            {
                // 70%→0%에 걸쳐 0→0.6 까지 증가
                float danger = 1f - (healthRatio / 0.7f); // 0 ~ 1
                float baseAlpha = danger * 0.6f;

                // 체력 50% 이하: 맥박(pulse) 효과 추가
                if (healthRatio < 0.5f)
                {
                    // 체력이 낮을수록 맥박이 빠르고 강해짐
                    float urgency = 1f - (healthRatio / 0.5f); // 0 ~ 1
                    float pulseSpeed = Mathf.Lerp(3f, 10f, urgency);   // 느림→빠름
                    float pulseStrength = Mathf.Lerp(0.05f, 0.25f, urgency); // 약→강
                    float pulse = Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)) * pulseStrength;
                    baseAlpha += pulse;
                }

                targetAlpha = baseAlpha;
            }

            // 피격 순간 플래시 효과 (순간적으로 더 밝게)
            if (hitFlashTimer > 0f)
            {
                hitFlashTimer -= Time.deltaTime;
                float flashIntensity = Mathf.Clamp01(hitFlashTimer / HIT_FLASH_DURATION);
                targetAlpha = Mathf.Max(targetAlpha, flashIntensity * 0.5f);
            }

            // 부드러운 보간
            currentVignetteAlpha = Mathf.Lerp(currentVignetteAlpha, targetAlpha, Time.deltaTime * 8f);

            // 색상도 체력에 따라 변화: 주황빛(경고) → 진한 빨강(위험)
            float healthForColor = Mathf.Clamp01(healthRatio);
            float r = 1f;
            float g = Mathf.Lerp(0f, 0.15f, healthForColor); // 체력 낮을수록 순수 빨강
            float b = 0f;
            
            damageVignette.color = new Color(r, g, b, currentVignetteAlpha);
        }
        else
        {
            // 사망 시: 화면이 서서히 진한 빨강으로 물듦
            currentVignetteAlpha = Mathf.Lerp(currentVignetteAlpha, 0.85f, Time.deltaTime * 2f);
            damageVignette.color = new Color(0.8f, 0f, 0f, currentVignetteAlpha);
        }
    }

    private bool isDead = false;

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        if (currentHealth <= 0) 
        {
            currentHealth = 0;
            Die();
        }
        else
        {
            Debug.Log("으악! 대미지 " + amount + " 감소! 현재 체력: " + currentHealth);
        }
        
        UpdateHealthBar();

        // 피격 시 비네트 플래시 트리거
        hitFlashTimer = HIT_FLASH_DURATION;

        // 피격 파티클 스폰
        if (hitParticlePrefab != null)
        {
            GameObject particle = Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
            Destroy(particle, 1f); // 1초 뒤 삭제
        }

        // 붉은색 점멸 효과
        if (spriteRenderer != null)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(FlashRed());
        }
    }

    private Color originalColor = Color.white;
    private Coroutine flashCoroutine;

    private System.Collections.IEnumerator FlashRed()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
        flashCoroutine = null;
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        Debug.Log("구급상자 사용! 체력 15 회복! 현재 체력: " + currentHealth);
        UpdateHealthBar();
    }

    public Text healthText;

    private void UpdateHealthBar()
    {
        // UI 연결이 끊겨있다면 이름으로 강제 탐색해서 연결
        if (healthBarFill == null)
        {
            GameObject fillObj = GameObject.Find("HealthBar_Fill");
            if (fillObj != null)
            {
                healthBarFill = fillObj.GetComponent<Image>();
            }
        }

        if (healthText == null)
        {
            GameObject bgObj = GameObject.Find("HealthBar_BG");
            if (bgObj != null)
            {
                Transform textTrans = bgObj.transform.Find("HealthText");
                if (textTrans != null)
                {
                    healthText = textTrans.GetComponent<Text>();
                }
                else
                {
                    GameObject tObj = new GameObject("HealthText");
                    tObj.transform.SetParent(bgObj.transform, false);
                    healthText = tObj.AddComponent<Text>();
                    healthText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    healthText.fontSize = 24;
                    healthText.fontStyle = FontStyle.Bold;
                    healthText.color = Color.white;
                    healthText.alignment = TextAnchor.MiddleCenter;
                    
                    RectTransform rt = tObj.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0, 0.5f);
                    rt.anchorMax = new Vector2(0, 0.5f);
                    rt.sizeDelta = new Vector2(120, 50);
                    rt.anchoredPosition = new Vector2(65, 0); // 금색 원통 중앙에 배치
                    
                    Outline outline = tObj.AddComponent<Outline>();
                    outline.effectColor = Color.black;
                    outline.effectDistance = new Vector2(1, -1);
                }
            }
        }

        if (healthBarFill != null)
        {
            float ratio = currentHealth / maxHealth;
            healthBarFill.fillAmount = ratio;
        }

        if (healthText != null)
        {
            healthText.text = $"{Mathf.CeilToInt(currentHealth)} / {Mathf.CeilToInt(maxHealth)}";
        }
    }



    private void Die()
    {
        isDead = true;
        
        // 사망 사운드 재생
        AudioClip deathSound = Resources.Load<AudioClip>("Sounds/PlayerDeath");
        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }

        if (SaveManager.Instance != null && GameManager.Instance != null)
        {
            SaveManager.Instance.AddDeath(GameManager.Instance.selectedMissionName);
        }

        // Stop movement and shooting
        PlayerMovement pm = GetComponent<PlayerMovement>();
        if (pm != null) pm.enabled = false;
        
        PlayerShooting ps = GetComponent<PlayerShooting>();
        if (ps != null) ps.enabled = false;

        // Death motion: 애니메이터가 켜져 있으면 "Die" 트리거 실행, 없거나 꺼져있으면 기존처럼 뒤로 넘어짐
        Animator anim = GetComponent<Animator>();
        if (anim != null && anim.enabled) 
        {
            anim.SetTrigger("Die");
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, transform.localScale.x > 0 ? 90f : -90f);
        }

        // Show Game Over UI
        if (GameOverUIManager.instance != null)
        {
            GameOverUIManager.instance.ShowGameOver();
        }
    }
}
