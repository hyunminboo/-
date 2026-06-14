using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    public float GetCurrentHealth() { return currentHealth; }
    
    [Header("Effects")]
    public GameObject hitParticlePrefab;
    
    [Header("UI")]
    public Canvas healthCanvas;
    public Slider healthSlider;
    private Coroutine hideUiCoroutine;

    [Header("EXP Module Drop")]
    [Range(0f, 1f)]
    public float expModuleDropRate = 0.6f; // 60% 확률로 경험치 모듈 드랍
    public int expModuleSPAmount = 5; // 경험치 모듈 하나당 SP

    public event Action OnDeath;

    private SpriteRenderer sr;
    private EnemyAI ai;
    private Rigidbody2D rb;

    void Start()
    {
        // 레이어가 Enemy가 아니면 자동 수정 (검 공격 등 레이어 기반 감지가 되도록)
        int enemyLayer = LayerMask.NameToLayer("Enemy");
        if (enemyLayer >= 0 && gameObject.layer != enemyLayer)
        {
            gameObject.layer = enemyLayer;
        }

        if (GameManager.Instance != null)
        {
            maxHealth *= GameManager.Instance.GetDifficultyMultiplier();
        }
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        ai = GetComponent<EnemyAI>();
        rb = GetComponent<Rigidbody2D>();
        
        if (healthCanvas != null)
        {
            healthCanvas.gameObject.SetActive(false); // 초기엔 숨김
        }
        if (healthSlider != null)
        {
            healthSlider.maxValue = 1f;
            healthSlider.value = 1f;
        }
    }

    void Update()
    {
        if (healthSlider != null)
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, currentHealth / maxHealth, Time.deltaTime * 5f);
        }

        // 낙사 처리: 낭떠러지로 떨어져 맵 아래로 가면 즉사 처리하여 웨이브가 막히지 않도록 함
        if (transform.position.y < -20f)
        {
            Die();
        }
    }

    public void TakeDamage(float amount, Vector2 hitDirection)
    {
        if (currentHealth <= 0) return;

        currentHealth -= amount;
        
        // 데미지 텍스트 팝업 표시
        ShowDamagePopup(transform.position, amount);
        
        // 1. 타격 파티클 스폰
        if (hitParticlePrefab != null)
        {
            GameObject particle = Instantiate(hitParticlePrefab, transform.position, Quaternion.identity);
            Destroy(particle, 1f);
        }
        
        // 2. Hit Flash
        if (gameObject.activeInHierarchy)
            StartCoroutine(FlashRoutine());

        // 3. 체력바 UI 표시
        if (healthCanvas != null)
        {
            healthCanvas.gameObject.SetActive(true);
            if (hideUiCoroutine != null) StopCoroutine(hideUiCoroutine);
            if (gameObject.activeInHierarchy)
                hideUiCoroutine = StartCoroutine(HideHealthUI());
        }

        // 4. Knockback
        if (ai == null || (ai.role != EnemyRole.Heavy && ai.role != EnemyRole.Turret && ai.role != EnemyRole.Boss))
        {
            if (gameObject.activeInHierarchy)
                StartCoroutine(KnockbackRoutine(hitDirection));
        }

        // 5. Boss HP Threshold Item Drop
        if (ai != null && ai.role == EnemyRole.Boss)
        {
            CheckBossHpDrop();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private int bossItemsDropped = 0;
    private void CheckBossHpDrop()
    {
        // 25% 체력이 깎일 때마다 아이템 하나 드랍 (너무 많이 드랍되는 현상 수정)
        float hpPercentage = currentHealth / maxHealth;
        int expectedDrops = Mathf.FloorToInt((1f - hpPercentage) / 0.25f);
        
        while (bossItemsDropped < expectedDrops && bossItemsDropped < 3) // 최대 3번 (75%, 50%, 25%)
        {
            bossItemsDropped++;
            // 80% 구급상자, 20% 수류탄 (무한 무기는 제외)
            string itemPath = UnityEngine.Random.value < 0.8f ? "Items/Item_Medkit" : "Items/Item_GrenadeAmmo";
            GameObject itemPrefab = Resources.Load<GameObject>(itemPath);
            if (itemPrefab != null)
            {
                GameObject item = Instantiate(itemPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
                Rigidbody2D itemRb = item.GetComponent<Rigidbody2D>();
                if (itemRb != null)
                {
                    itemRb.AddForce(new Vector2(UnityEngine.Random.Range(-3f, 3f), 6f), ForceMode2D.Impulse);
                }
            }
        }
    }

    private IEnumerator FlashRoutine()
    {
        if (sr != null && sr.material != null)
        {
            Color originalColor = sr.material.color;
            sr.material.color = new Color(3f, 3f, 3f);
            yield return new WaitForSeconds(0.1f);
            sr.material.color = originalColor;
        }
    }

    private IEnumerator KnockbackRoutine(Vector2 hitDirection)
    {
        if (rb != null)
        {
            if (ai != null) ai.isStunned = true; // 스턴 추가
            
            // 넉백 방향과 힘 (y축 살짝 띄우기)
            Vector2 forceDir = hitDirection.normalized;
            forceDir.y += 0.2f;
            
            rb.linearVelocity = forceDir.normalized * 5f;
            
            yield return new WaitForSeconds(0.15f);
            
            if (ai != null) ai.isStunned = false;
        }
    }

    private IEnumerator HideHealthUI()
    {
        yield return new WaitForSeconds(3f);
        if (healthCanvas != null)
        {
            healthCanvas.gameObject.SetActive(false);
        }
    }

    private void Die()
    {
        OnDeath?.Invoke();
        
        // 기존 아이템 드랍 (수류탄, 구급상자 등)
        DropRandomItem();
        
        // 경험치 모듈 드랍
        DropExpModule();
        
        Destroy(gameObject);
    }
    
    private static bool hasSwordDropped = false;

    private void DropRandomItem()
    {
        float rand = UnityEngine.Random.value;
        string itemPath = "";
        
        // 검이 한 번도 드랍되지 않았을 때만 10% 확률로 검 드랍
        if (!hasSwordDropped && rand <= 0.1f) 
        {
            itemPath = "Items/Item_Sword";
            hasSwordDropped = true; // 이제 두 번 다시 드랍되지 않음
        }
        else if (rand <= 0.6f) // 50% 확률로 수류탄 (검 드랍 안될 시 60% 확률)
        {
            itemPath = "Items/Item_GrenadeAmmo";
        }
        else // 40% 확률로 구급상자
        {
            itemPath = "Items/Item_Medkit";
        }
        
        GameObject itemPrefab = Resources.Load<GameObject>(itemPath);
        
        if (itemPrefab != null)
        {
            GameObject item = Instantiate(itemPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
            Rigidbody2D itemRb = item.GetComponent<Rigidbody2D>();
            if (itemRb != null)
            {
                itemRb.AddForce(new Vector2(UnityEngine.Random.Range(-2f, 2f), 5f), ForceMode2D.Impulse);
            }
        }
    }

    /// <summary>
    /// 경험치 모듈을 확률에 따라 드랍합니다.
    /// </summary>
    private void DropExpModule()
    {
        if (UnityEngine.Random.value > expModuleDropRate) return;
        
        // 경험치 모듈 프리팹을 Resources에서 로드
        GameObject expPrefab = Resources.Load<GameObject>("Items/Item_ExpModule");
        
        if (expPrefab != null)
        {
            Vector3 spawnPos = transform.position + Vector3.up * 0.3f;
            GameObject expItem = Instantiate(expPrefab, spawnPos, Quaternion.identity);
            
            Rigidbody2D expRb = expItem.GetComponent<Rigidbody2D>();
            if (expRb != null)
            {
                expRb.AddForce(new Vector2(UnityEngine.Random.Range(-1.5f, 1.5f), 4f), ForceMode2D.Impulse);
            }
        }
    }

    /// <summary>
    /// 데미지 수치를 화면에 띄웁니다. (SP 팝업 애니메이션 클래스를 재사용)
    /// </summary>
    private void ShowDamagePopup(Vector3 worldPos, float amount)
    {
        GameObject popupObj = new GameObject("DamagePopup");
        
        Canvas canvas = popupObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(popupObj.transform, false);
        
        UnityEngine.UI.Text txt = textObj.AddComponent<UnityEngine.UI.Text>();
        txt.text = Mathf.RoundToInt(amount).ToString();
        
        // 크리티컬/높은 데미지일수록 글씨를 더 크고 빨갛게
        txt.fontSize = amount >= 30f ? 36 : 28;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = amount >= 30f ? new Color(1f, 0.3f, 0.1f, 1f) : new Color(1f, 0.8f, 0.2f, 1f); // 빨간주황 or 노랑
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontStyle = FontStyle.Bold;
        
        UnityEngine.UI.Outline outline = textObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        RectTransform rt = textObj.GetComponent<RectTransform>();
        
        // 위치를 약간 무작위로 분산시켜 겹치지 않게
        Vector3 randomOffset = new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(0f, 0.5f), 0);
        Vector3 screenPos = Camera.main != null ? Camera.main.WorldToScreenPoint(worldPos + randomOffset) : new Vector3(Screen.width / 2f, Screen.height / 2f);
        
        rt.position = screenPos + new Vector3(0, 30, 0);
        rt.sizeDelta = new Vector2(200, 50);
        
        // 기존 ItemPickup.cs에 있는 SPPopupAnim 재사용
        SPPopupAnim anim = popupObj.AddComponent<SPPopupAnim>();
        anim.textComponent = txt;
        anim.textRT = rt;
    }
}
