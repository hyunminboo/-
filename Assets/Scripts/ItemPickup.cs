using UnityEngine;

public enum ItemType { Medkit, GrenadeAmmo, Sword, Gun, ExpModule }

[RequireComponent(typeof(BoxCollider2D))]
public class ItemPickup : MonoBehaviour
{
    public ItemType itemType = ItemType.Medkit;
    public float healAmount = 50f;
    public int grenadeAmount = 1;
    public int spAmount = 5; // 경험치 모듈이 주는 SP 양
    
    public GameObject pickupEffectPrefab;
    
    private void Start()
    {
        // 플레이어와 부딪혀서 투명벽처럼 막히는 현상 방지
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Collider2D[] playerCols = player.GetComponents<Collider2D>();
            Collider2D[] myCols = GetComponents<Collider2D>();
            foreach(var mc in myCols)
            {
                if (!mc.isTrigger)
                {
                    foreach(var pc in playerCols)
                    {
                        Physics2D.IgnoreCollision(mc, pc);
                    }
                }
            }
        }
        // 아이템이 공중에 뜨거나 땅을 뚫는 현상 방지: 바닥으로 스냅
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 20f);
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            // 땅 위치보다 살짝 위로 스냅 (아이템의 중심점을 고려해 0.5f 정도 올려줌)
            transform.position = new Vector3(transform.position.x, hit.point.y + 0.5f, transform.position.z);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.name == "Player")
        {
            // 경험치 모듈은 인벤토리에 넣지 않고 바로 SP로 변환
            if (itemType == ItemType.ExpModule)
            {
                PlayerStats stats = collision.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.AddSkillPoints(spAmount);
                }
            }
            else
            {
                if (HotbarInventory.instance != null)
                {
                    bool added = HotbarInventory.instance.AddItem(itemType, 1);
                    if (!added) return; // 인벤토리가 가득 차서 못 먹음
                }
            }

            if (pickupEffectPrefab != null)
            {
                GameObject fx = Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
                Destroy(fx, 1f);
            }
            
            AudioClip pickupSound = Resources.Load<AudioClip>("Sounds/ItemPickup");
            if (pickupSound != null)
            {
                GameObject sfxObj = new GameObject("PickupSFX");
                sfxObj.transform.position = transform.position;
                AudioSource src = sfxObj.AddComponent<AudioSource>();
                src.clip = pickupSound;
                src.spatialBlend = 0f;
                src.volume = 0.8f;
                src.Play();
                Destroy(sfxObj, pickupSound.length);
            }
            
            // SP 획득 텍스트 표시 (경험치 모듈인 경우)
            if (itemType == ItemType.ExpModule)
            {
                ShowSPPopup(transform.position, spAmount);
            }
            
            Destroy(gameObject);
        }
    }
    
    /// <summary>
    /// SP 획득 시 화면에 "+5 SP" 같은 팝업 텍스트를 띄웁니다
    /// </summary>
    private void ShowSPPopup(Vector3 worldPos, int amount)
    {
        GameObject popupObj = new GameObject("SPPopup");
        
        Canvas canvas = popupObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(popupObj.transform, false);
        
        UnityEngine.UI.Text txt = textObj.AddComponent<UnityEngine.UI.Text>();
        txt.text = "+" + amount + " SP";
        txt.fontSize = 28;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.color = new Color(0.30f, 0.90f, 0.95f, 1f); // 사이안
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontStyle = FontStyle.Bold;
        
        UnityEngine.UI.Outline outline = textObj.AddComponent<UnityEngine.UI.Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2, -2);

        RectTransform rt = textObj.GetComponent<RectTransform>();
        // 화면 좌표로 변환
        Vector3 screenPos = Camera.main != null ? Camera.main.WorldToScreenPoint(worldPos) : new Vector3(Screen.width / 2f, Screen.height / 2f);
        rt.position = screenPos + new Vector3(0, 40, 0);
        rt.sizeDelta = new Vector2(200, 50);
        
        // 위로 올라가면서 사라지는 애니메이션
        SPPopupAnim anim = popupObj.AddComponent<SPPopupAnim>();
        anim.textComponent = txt;
        anim.textRT = rt;
    }
}

/// <summary>
/// SP 획득 팝업 애니메이션 (위로 올라가면서 페이드아웃)
/// </summary>
public class SPPopupAnim : MonoBehaviour
{
    public UnityEngine.UI.Text textComponent;
    public RectTransform textRT;
    private float elapsed = 0f;
    private float duration = 1.0f;
    private Vector3 startPos;

    void Start()
    {
        if (textRT != null) startPos = textRT.position;
    }

    void Update()
    {
        elapsed += Time.unscaledDeltaTime; // timeScale=0이어도 동작
        float t = elapsed / duration;
        
        if (textRT != null)
            textRT.position = startPos + new Vector3(0, t * 60f, 0);
        
        if (textComponent != null)
        {
            Color c = textComponent.color;
            c.a = Mathf.Lerp(1f, 0f, t);
            textComponent.color = c;
        }
        
        if (t >= 1f) Destroy(gameObject);
    }
}
