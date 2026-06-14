using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// InventoryPanel > RightPanel을 코드로 재디자인합니다.
/// 단색(사이안/다크) 테마 기반 밀리터리 UI.
/// InventoryCanvas에 붙어 OnEnable 시 한 번만 빌드합니다.
/// </summary>
public class SkillUpgradeUIBuilder : MonoBehaviour
{
    // ---- 단색 팔레트 (사이안 + 다크 톤) ----
    static readonly Color BG_DARK      = new Color(0.05f, 0.06f, 0.08f, 0.95f);
    static readonly Color PANEL_BG     = new Color(0.08f, 0.09f, 0.12f, 0.95f);
    static readonly Color ACCENT       = new Color(0.25f, 0.85f, 0.90f, 1f);      // 메인 사이안
    static readonly Color ACCENT_DIM   = new Color(0.15f, 0.50f, 0.55f, 1f);      // 어두운 사이안
    static readonly Color BTN_NORMAL   = new Color(0.12f, 0.13f, 0.17f, 1f);
    static readonly Color BTN_HOVER    = new Color(0.16f, 0.18f, 0.24f, 1f);
    static readonly Color BTN_DISABLED = new Color(0.08f, 0.09f, 0.11f, 1f);
    static readonly Color BORDER       = new Color(0.25f, 0.85f, 0.90f, 0.25f);
    static readonly Color TEXT_BRIGHT   = new Color(0.90f, 0.92f, 0.95f, 1f);
    static readonly Color TEXT_DIM     = new Color(0.45f, 0.48f, 0.55f, 1f);
    static readonly Color LINE_COLOR   = new Color(0.25f, 0.85f, 0.90f, 0.3f);

    private bool isBuilt = false;
    private InventoryUIManager uiManager;

    // 런타임 레퍼런스
    private Text spText;
    private Image spIcon;
    private SkillRow[] skillRows;

    struct SkillRow
    {
        public RectTransform root;
        public Image accentBar;
        public Text nameText;
        public Text levelText;
        public Text descText;
        public Button upgradeBtn;
        public Text upgradeBtnText;
        public Image btnImage;
        public Image rowBgImage;
    }

    Font cachedFont;

    Font GetFont()
    {
        if (cachedFont != null) return cachedFont;
        var fonts = Resources.FindObjectsOfTypeAll<Font>();
        foreach (var f in fonts)
        {
            if (f.name == "Inter-SemiBold") { cachedFont = f; return f; }
        }
        foreach (var f in fonts)
        {
            if (f.name == "Inter-Regular") { cachedFont = f; return f; }
        }
        cachedFont = Resources.Load<Font>("Fonts/NanumGothic");
        return cachedFont;
    }

    void OnEnable()
    {
        if (!isBuilt) BuildUI();
        else RefreshAllUI();
    }

    void BuildUI()
    {
        isBuilt = true;
        uiManager = GetComponent<InventoryUIManager>();

        // --- InventoryPanel 찾기 ---
        Transform invPanel = null;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).name == "InventoryPanel")
            {
                invPanel = transform.GetChild(i);
                break;
            }
        }
        if (invPanel == null) return;

        Image panelBg = invPanel.GetComponent<Image>();
        if (panelBg != null) panelBg.color = BG_DARK;

        // LeftPanel 숨기기
        Transform leftPanel = null;
        Transform rightPanel = null;
        for (int i = 0; i < invPanel.childCount; i++)
        {
            string n = invPanel.GetChild(i).name;
            if (n == "LeftPanel") leftPanel = invPanel.GetChild(i);
            if (n == "RightPanel") rightPanel = invPanel.GetChild(i);
        }
        if (leftPanel != null) leftPanel.gameObject.SetActive(false);
        if (rightPanel == null) return;

        // --- 기존 RightPanel 자식 모두 삭제 ---
        for (int i = rightPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(rightPanel.GetChild(i).gameObject);
        }

        // --- RightPanel 스타일 ---
        Image rpImg = rightPanel.GetComponent<Image>();
        if (rpImg != null) rpImg.color = PANEL_BG;

        Outline rpOutline = rightPanel.gameObject.GetComponent<Outline>();
        if (rpOutline == null) rpOutline = rightPanel.gameObject.AddComponent<Outline>();
        rpOutline.effectColor = BORDER;
        rpOutline.effectDistance = new Vector2(1, -1);

        // ===== 헤더 영역 =====
        // 타이틀
        Text titleText = CreateText(rightPanel, "TitleText", "SKILL UPGRADE", 26, ACCENT, TextAnchor.MiddleLeft);
        SetAnchors(titleText.rectTransform, 0.05f, 0.91f, 0.55f, 0.98f);
        titleText.fontStyle = FontStyle.Bold;

        // 경험치 모듈 아이콘 + SP 텍스트 (우측 상단)
        // 아이콘
        Sprite expSprite = Resources.Load<Sprite>("Sprites/UI/exp_module");
        if (expSprite != null)
        {
            spIcon = CreateImage(rightPanel, "SPIcon", Color.white);
            spIcon.sprite = expSprite;
            spIcon.preserveAspect = true;
            SetAnchors(spIcon.rectTransform, 0.72f, 0.91f, 0.78f, 0.98f);
        }
        
        spText = CreateText(rightPanel, "SPText", "0", 22, ACCENT, TextAnchor.MiddleLeft);
        SetAnchors(spText.rectTransform, 0.79f, 0.91f, 0.96f, 0.98f);
        spText.fontStyle = FontStyle.Bold;

        // 구분선
        Image headerLine = CreateImage(rightPanel, "HeaderLine", LINE_COLOR);
        SetAnchors(headerLine.rectTransform, 0.04f, 0.90f, 0.96f, 0.905f);

        // 안내 텍스트
        Text hintText = CreateText(rightPanel, "HintText", "경험치 모듈을 소모하여 스킬을 강화합니다", 13, TEXT_DIM, TextAnchor.MiddleCenter);
        SetAnchors(hintText.rectTransform, 0.04f, 0.84f, 0.96f, 0.90f);

        // ===== 스킬 행 생성 =====
        string[] names = { "HP 강화", "ATK 강화", "DASH 강화", "스킬 강화" };
        string[] descs = { "최대 체력 +20", "공격력 +10", "대시 쿨타임 -0.15s", "스킬 쿨타임 -2s" };

        skillRows = new SkillRow[4];

        float rowTop = 0.82f;
        float rowHeight = 0.17f;
        float rowGap = 0.02f;

        for (int i = 0; i < 4; i++)
        {
            float top = rowTop - i * (rowHeight + rowGap);
            float bottom = top - rowHeight;
            skillRows[i] = CreateSkillRow(rightPanel, i, names[i], descs[i], bottom, top);
        }

        // --- 닫기 힌트 ---
        Text closeHint = CreateText(rightPanel, "CloseHint", "[TAB] 닫기", 13, TEXT_DIM, TextAnchor.MiddleCenter);
        SetAnchors(closeHint.rectTransform, 0.30f, 0.01f, 0.70f, 0.06f);

        // InventoryUIManager에 레퍼런스 연결
        if (uiManager != null)
        {
            uiManager.skillPointsText = spText;
            uiManager.hpUpgradeText = skillRows[0].levelText;
            uiManager.atkUpgradeText = skillRows[1].levelText;
            uiManager.dashUpgradeText = skillRows[2].levelText;
            uiManager.autoShootUpgradeText = skillRows[3].levelText;
        }

        StartCoroutine(DelayedRefresh());
    }

    IEnumerator DelayedRefresh()
    {
        yield return null;
        RefreshAllUI();
    }

    SkillRow CreateSkillRow(Transform parent, int index, string skillName, string desc, float yMin, float yMax)
    {
        SkillRow row = new SkillRow();

        // 행 배경
        Image rowBg = CreateImage(parent, "SkillRow_" + index, BTN_NORMAL);
        SetAnchors(rowBg.rectTransform, 0.04f, yMin, 0.96f, yMax);
        row.root = rowBg.rectTransform;
        row.rowBgImage = rowBg;

        // 왼쪽 악센트 바 (사이안 스트립)
        row.accentBar = CreateImage(rowBg.transform, "AccentBar", ACCENT);
        SetAnchors(row.accentBar.rectTransform, 0f, 0f, 0.006f, 1f);

        // 스킬 이름
        row.nameText = CreateText(rowBg.transform, "SkillName", skillName, 18, TEXT_BRIGHT, TextAnchor.MiddleLeft);
        SetAnchors(row.nameText.rectTransform, 0.03f, 0.52f, 0.45f, 0.95f);
        row.nameText.fontStyle = FontStyle.Bold;

        // 레벨 텍스트
        row.levelText = CreateText(rowBg.transform, "LevelText", "Lv.1", 14, ACCENT, TextAnchor.MiddleLeft);
        SetAnchors(row.levelText.rectTransform, 0.03f, 0.08f, 0.20f, 0.50f);

        // 설명 (스탯 미리보기)
        row.descText = CreateText(rowBg.transform, "Desc", desc, 13, TEXT_DIM, TextAnchor.MiddleLeft);
        SetAnchors(row.descText.rectTransform, 0.22f, 0.08f, 0.70f, 0.50f);

        // 업그레이드 버튼
        GameObject btnObj = new GameObject("UpgradeBtn_" + index);
        btnObj.transform.SetParent(rowBg.transform, false);

        row.btnImage = btnObj.AddComponent<Image>();
        row.btnImage.color = ACCENT_DIM;

        row.upgradeBtn = btnObj.AddComponent<Button>();

        ColorBlock cb = row.upgradeBtn.colors;
        cb.normalColor = new Color(ACCENT.r * 0.25f, ACCENT.g * 0.25f, ACCENT.b * 0.25f, 1f);
        cb.highlightedColor = new Color(ACCENT.r * 0.35f, ACCENT.g * 0.35f, ACCENT.b * 0.35f, 1f);
        cb.pressedColor = new Color(ACCENT.r * 0.50f, ACCENT.g * 0.50f, ACCENT.b * 0.50f, 1f);
        cb.disabledColor = BTN_DISABLED;
        cb.fadeDuration = 0.08f;
        row.upgradeBtn.colors = cb;

        RectTransform btnRT = btnObj.GetComponent<RectTransform>();
        SetAnchors(btnRT, 0.74f, 0.12f, 0.97f, 0.88f);

        // 버튼 텍스트
        row.upgradeBtnText = CreateText(btnObj.transform, "BtnText", "UPGRADE\n10 SP", 12, TEXT_BRIGHT, TextAnchor.MiddleCenter);
        SetAnchors(row.upgradeBtnText.rectTransform, 0f, 0f, 1f, 1f);
        row.upgradeBtnText.fontStyle = FontStyle.Bold;

        // 버튼 테두리
        Outline btnOutline = btnObj.AddComponent<Outline>();
        btnOutline.effectColor = new Color(ACCENT.r, ACCENT.g, ACCENT.b, 0.4f);
        btnOutline.effectDistance = new Vector2(1, -1);

        // 버튼 클릭 이벤트
        int idx = index;
        row.upgradeBtn.onClick.AddListener(() => OnUpgradeClicked(idx));

        return row;
    }

    void OnUpgradeClicked(int index)
    {
        if (uiManager == null) return;
        switch (index)
        {
            case 0: uiManager.OnUpgradeHP(); break;
            case 1: uiManager.OnUpgradeATK(); break;
            case 2: uiManager.OnUpgradeDash(); break;
            case 3: uiManager.OnUpgradeAutoShoot(); break;
        }
        RefreshAllUI();
    }

    public void RefreshAllUI()
    {
        if (uiManager == null || uiManager.playerStats == null) return;
        PlayerStats ps = uiManager.playerStats;

        // SP 표시
        if (spText != null)
            spText.text = ps.skillPoints.ToString();

        // 각 행 갱신
        int[] levels = { ps.hpLevel, ps.atkLevel, ps.dashLevel, ps.autoShootLevel };
        int[] costs = { ps.hpCost, ps.atkCost, ps.dashCost, ps.autoShootCost };
        string[] details = {
            $"HP {ps.GetMaxHP():F0} → {ps.GetMaxHP() + 20f:F0}",
            $"DMG {ps.GetAttackPower():F0} → {ps.GetAttackPower() + 10f:F0}",
            $"쿨타임 {ps.GetDashCooldown():F2}s → {Mathf.Max(0.2f, ps.GetDashCooldown() - 0.15f):F2}s",
            $"쿨타임 {ps.GetAutoShootCooldown():F1}s → {Mathf.Max(5f, ps.GetAutoShootCooldown() - 2f):F1}s"
        };

        for (int i = 0; i < 4 && i < skillRows.Length; i++)
        {
            if (skillRows[i].levelText != null)
                skillRows[i].levelText.text = $"Lv.{levels[i]}";

            if (skillRows[i].descText != null)
                skillRows[i].descText.text = details[i];

            bool canAfford = ps.skillPoints >= costs[i];

            if (skillRows[i].upgradeBtnText != null)
            {
                skillRows[i].upgradeBtnText.text = $"UPGRADE\n{costs[i]} SP";
                skillRows[i].upgradeBtnText.color = canAfford ? TEXT_BRIGHT : TEXT_DIM;
            }

            if (skillRows[i].upgradeBtn != null)
                skillRows[i].upgradeBtn.interactable = canAfford;

            // 악센트 바 색상 (SP 충분하면 밝은 사이안, 부족하면 어두운 색)
            if (skillRows[i].accentBar != null)
                skillRows[i].accentBar.color = canAfford ? ACCENT : ACCENT_DIM;
        }
    }

    // ---- 유틸리티 ----
    Text CreateText(Transform parent, string objName, string content, int fontSize, Color color, TextAnchor align)
    {
        GameObject obj = new GameObject(objName);
        obj.transform.SetParent(parent, false);
        Text txt = obj.AddComponent<Text>();
        txt.text = content;
        txt.font = GetFont();
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = align;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;
        txt.raycastTarget = false;
        return txt;
    }

    Image CreateImage(Transform parent, string objName, Color color)
    {
        GameObject obj = new GameObject(objName);
        obj.transform.SetParent(parent, false);
        Image img = obj.AddComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    void SetAnchors(RectTransform rt, float xMin, float yMin, float xMax, float yMax)
    {
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
