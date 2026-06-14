using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class LobbySceneBuilder
{
    [MenuItem("Tools/Build Lobby Scene")]
    public static void BuildUI()
    {
        // 씬 내 기존 LobbyCanvas 제거
        GameObject existingCanvas = GameObject.Find("LobbyCanvas");
        if (existingCanvas != null)
        {
            Object.DestroyImmediate(existingCanvas);
        }
        
        EnsureEventSystem();
        EnsureMainCamera();
        
        // 폰트 설정
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // 1. Canvas 생성
        GameObject canvasObj = new GameObject("LobbyCanvas", typeof(RectTransform));
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. 배경 생성
        GameObject bgObj = new GameObject("Background", typeof(RectTransform));
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.16f, 0.16f); // 어두운 군용 금속 느낌 (#1a2a2a)
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;

        // 3. 좌측 패널 (RECORD)
        GameObject leftPanel = new GameObject("LeftPanel_Record", typeof(RectTransform));
        leftPanel.transform.SetParent(bgObj.transform, false);
        Image leftImg = leftPanel.AddComponent<Image>();
        leftImg.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);
        Outline leftOutline = leftPanel.AddComponent<Outline>();
        leftOutline.effectColor = new Color(0.6f, 0.3f, 0.1f); // 녹슨 금속 테두리 스타일
        leftOutline.effectDistance = new Vector2(2, -2);
        
        RectTransform leftRect = leftPanel.GetComponent<RectTransform>();
        leftRect.anchorMin = new Vector2(0.05f, 0.05f);
        leftRect.anchorMax = new Vector2(0.45f, 0.95f);
        leftRect.offsetMin = Vector2.zero;
        leftRect.offsetMax = Vector2.zero;

        // "RECORD" 제목
        GameObject recordTitle = new GameObject("TitleText", typeof(RectTransform));
        recordTitle.transform.SetParent(leftPanel.transform, false);
        Text titleText = recordTitle.AddComponent<Text>();
        titleText.text = "RECORD";
        titleText.font = defaultFont;
        titleText.fontSize = 40;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.UpperCenter;
        titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
        titleText.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform titleRect = recordTitle.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.9f);
        titleRect.anchorMax = new Vector2(1, 1f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        // 미션 카드 컨테이너
        GameObject cardsContainer = new GameObject("CardsContainer", typeof(RectTransform));
        cardsContainer.transform.SetParent(leftPanel.transform, false);
        VerticalLayoutGroup vlg = cardsContainer.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 15f;
        vlg.childForceExpandHeight = false;
        vlg.childControlHeight = false;
        RectTransform cardsRect = cardsContainer.GetComponent<RectTransform>();
        cardsRect.anchorMin = new Vector2(0.05f, 0.2f);
        cardsRect.anchorMax = new Vector2(0.95f, 0.85f);
        cardsRect.offsetMin = Vector2.zero;
        cardsRect.offsetMax = Vector2.zero;

        UIMissionRecordCard[] missionCards = new UIMissionRecordCard[3];
        for (int i = 0; i < 3; i++)
        {
            missionCards[i] = CreateMissionCard(cardsContainer.transform, defaultFont, i);
        }

        // 전체 진행도 요약 (하단)
        GameObject summaryPanel = new GameObject("SummaryPanel", typeof(RectTransform));
        summaryPanel.transform.SetParent(leftPanel.transform, false);
        RectTransform summaryRect = summaryPanel.GetComponent<RectTransform>();
        summaryRect.anchorMin = new Vector2(0.05f, 0.05f);
        summaryRect.anchorMax = new Vector2(0.95f, 0.15f);
        summaryRect.offsetMin = Vector2.zero;
        summaryRect.offsetMax = Vector2.zero;

        GameObject summaryTextObj = new GameObject("SummaryText", typeof(RectTransform));
        summaryTextObj.transform.SetParent(summaryPanel.transform, false);
        Text summaryText = summaryTextObj.AddComponent<Text>();
        summaryText.text = "CLEARED 0 / 3";
        summaryText.font = defaultFont;
        summaryText.fontSize = 24;
        summaryText.color = Color.white;
        summaryText.alignment = TextAnchor.MiddleCenter;
        summaryText.horizontalOverflow = HorizontalWrapMode.Overflow;
        summaryText.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform summaryTextRect = summaryTextObj.GetComponent<RectTransform>();
        summaryTextRect.anchorMin = new Vector2(0, 0.5f);
        summaryTextRect.anchorMax = new Vector2(1, 1f);
        summaryTextRect.offsetMin = Vector2.zero;
        summaryTextRect.offsetMax = Vector2.zero;

        GameObject summaryBarObj = new GameObject("SummaryBar", typeof(RectTransform));
        summaryBarObj.transform.SetParent(summaryPanel.transform, false);
        Image summaryBarBg = summaryBarObj.AddComponent<Image>();
        summaryBarBg.color = Color.gray;
        RectTransform summaryBarRect = summaryBarObj.GetComponent<RectTransform>();
        summaryBarRect.anchorMin = new Vector2(0, 0.1f);
        summaryBarRect.anchorMax = new Vector2(1, 0.4f);
        summaryBarRect.offsetMin = Vector2.zero;
        summaryBarRect.offsetMax = Vector2.zero;

        GameObject summaryBarFillObj = new GameObject("Fill", typeof(RectTransform));
        summaryBarFillObj.transform.SetParent(summaryBarObj.transform, false);
        Image summaryBarFill = summaryBarFillObj.AddComponent<Image>();
        summaryBarFill.color = new Color(0.2f, 0.8f, 0.2f);
        summaryBarFill.type = Image.Type.Filled;
        summaryBarFill.fillMethod = Image.FillMethod.Horizontal;
        summaryBarFill.fillAmount = 0f;
        RectTransform summaryBarFillRect = summaryBarFillObj.GetComponent<RectTransform>();
        summaryBarFillRect.anchorMin = Vector2.zero;
        summaryBarFillRect.anchorMax = Vector2.one;
        summaryBarFillRect.offsetMin = Vector2.zero;
        summaryBarFillRect.offsetMax = Vector2.zero;


        // 4. 우측 패널 (ENEMY INFO + 난이도)
        GameObject rightPanel = new GameObject("RightPanel_Info", typeof(RectTransform));
        rightPanel.transform.SetParent(bgObj.transform, false);
        Image rightImg = rightPanel.AddComponent<Image>();
        rightImg.color = new Color(0.05f, 0.05f, 0.05f, 0.8f);
        Outline rightOutline = rightPanel.AddComponent<Outline>();
        rightOutline.effectColor = new Color(0.6f, 0.3f, 0.1f);
        rightOutline.effectDistance = new Vector2(2, -2);
        
        RectTransform rightRect = rightPanel.GetComponent<RectTransform>();
        rightRect.anchorMin = new Vector2(0.55f, 0.3f);
        rightRect.anchorMax = new Vector2(0.95f, 0.95f);
        rightRect.offsetMin = Vector2.zero;
        rightRect.offsetMax = Vector2.zero;

        GameObject infoTitle = new GameObject("InfoTitle", typeof(RectTransform));
        infoTitle.transform.SetParent(rightPanel.transform, false);
        Text infoTextTitle = infoTitle.AddComponent<Text>();
        infoTextTitle.text = "ENEMY INFO";
        infoTextTitle.font = defaultFont;
        infoTextTitle.fontSize = 40;
        infoTextTitle.fontStyle = FontStyle.Bold;
        infoTextTitle.color = Color.white;
        infoTextTitle.alignment = TextAnchor.UpperCenter;
        infoTextTitle.horizontalOverflow = HorizontalWrapMode.Overflow;
        infoTextTitle.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform infoTitleRect = infoTitle.GetComponent<RectTransform>();
        infoTitleRect.anchorMin = new Vector2(0, 0.85f);
        infoTitleRect.anchorMax = new Vector2(1, 1f);
        infoTitleRect.offsetMin = Vector2.zero;
        infoTitleRect.offsetMax = Vector2.zero;

        GameObject enemyDescObj = new GameObject("EnemyDescription", typeof(RectTransform));
        enemyDescObj.transform.SetParent(rightPanel.transform, false);
        Text enemyDescText = enemyDescObj.AddComponent<Text>();
        enemyDescText.text = "Select a mission to view enemy info.";
        enemyDescText.font = defaultFont;
        enemyDescText.fontSize = 24;
        enemyDescText.color = Color.gray;
        enemyDescText.alignment = TextAnchor.UpperLeft;
        enemyDescText.horizontalOverflow = HorizontalWrapMode.Overflow;
        enemyDescText.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform enemyDescRect = enemyDescObj.GetComponent<RectTransform>();
        enemyDescRect.anchorMin = new Vector2(0.05f, 0.4f);
        enemyDescRect.anchorMax = new Vector2(0.95f, 0.8f);
        enemyDescRect.offsetMin = Vector2.zero;
        enemyDescRect.offsetMax = Vector2.zero;

        // 난이도 버튼 컨테이너
        GameObject diffContainer = new GameObject("DifficultyContainer", typeof(RectTransform));
        diffContainer.transform.SetParent(rightPanel.transform, false);
        RectTransform diffRect = diffContainer.GetComponent<RectTransform>();
        diffRect.anchorMin = new Vector2(0.05f, 0.05f);
        diffRect.anchorMax = new Vector2(0.95f, 0.35f);
        diffRect.offsetMin = Vector2.zero;
        diffRect.offsetMax = Vector2.zero;
        HorizontalLayoutGroup hlg = diffContainer.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 20f;
        hlg.childControlWidth = true;
        hlg.childForceExpandWidth = true;

        Button btnNormal = CreateDifficultyButton(diffContainer.transform, "NORMAL\n[x1.0]", Color.white, defaultFont);
        Button btnHard = CreateDifficultyButton(diffContainer.transform, "HARD\n[x1.5]", Color.yellow, defaultFont);
        Button btnNightmare = CreateDifficultyButton(diffContainer.transform, "NIGHTMARE\n[x2.0]", Color.red, defaultFont);

        DifficultySelector diffSelector = rightPanel.AddComponent<DifficultySelector>();
        diffSelector.btnNormal = btnNormal;
        diffSelector.btnHard = btnHard;
        diffSelector.btnNightmare = btnNightmare;

        // 5. 작전 시작 버튼 (하단 우측)
        GameObject startBtnObj = new GameObject("StartButton", typeof(RectTransform));
        startBtnObj.transform.SetParent(bgObj.transform, false);
        Image startImg = startBtnObj.AddComponent<Image>();
        startImg.color = new Color(0.2f, 0.6f, 0.2f); // 군용 녹색
        Outline startOutline = startBtnObj.AddComponent<Outline>();
        startOutline.effectColor = Color.green;
        Button btnStart = startBtnObj.AddComponent<Button>();
        RectTransform startRect = startBtnObj.GetComponent<RectTransform>();
        startRect.anchorMin = new Vector2(0.55f, 0.05f);
        startRect.anchorMax = new Vector2(0.95f, 0.25f);
        startRect.offsetMin = Vector2.zero;
        startRect.offsetMax = Vector2.zero;

        GameObject startTextObj = new GameObject("Text", typeof(RectTransform));
        startTextObj.transform.SetParent(startBtnObj.transform, false);
        Text startText = startTextObj.AddComponent<Text>();
        startText.text = "작전 시작\nSTART OPERATION\n<size=18>Play to enter combat</size>";
        startText.font = defaultFont;
        startText.alignment = TextAnchor.MiddleCenter;
        startText.color = Color.white;
        startText.fontSize = 32;
        startText.fontStyle = FontStyle.Bold;
        startText.supportRichText = true;
        startText.horizontalOverflow = HorizontalWrapMode.Overflow;
        startText.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform startTextRect = startTextObj.GetComponent<RectTransform>();
        startTextRect.anchorMin = Vector2.zero;
        startTextRect.anchorMax = Vector2.one;
        startTextRect.offsetMin = Vector2.zero;
        startTextRect.offsetMax = Vector2.zero;

        // 6. 매니저 부착
        LobbyManager manager = canvasObj.AddComponent<LobbyManager>();
        manager.missionCards = missionCards;
        manager.summaryText = summaryText;
        manager.summaryProgressBar = summaryBarFill;
        manager.enemyInfoText = enemyDescText;
        manager.diffSelector = diffSelector;
        manager.btnStartOperation = btnStart;

        // 생성할 미션 데이터 스크립터블 오브젝트 (없으면 임시 생성)
        manager.missions = new MissionData[3];
        for (int i = 0; i < 3; i++)
        {
            string path = $"Assets/Resources/Missions/Mission_{i + 1}.asset";
            MissionData md = AssetDatabase.LoadAssetAtPath<MissionData>(path);
            if (md == null)
            {
                md = ScriptableObject.CreateInstance<MissionData>();
                md.missionId = $"MISSION {i + 1}";
                md.sceneName = "SampleScene"; // 모든 미션이 일단 SampleScene을 가리킴
                md.enemyDescription = $"ENEMY INFO\n- Mission {i+1} Hostiles\n- Danger Level: " + (i == 0 ? "LOW" : (i == 1 ? "HIGH" : "CRITICAL"));
                
                if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
                if (!AssetDatabase.IsValidFolder("Assets/Resources/Missions")) AssetDatabase.CreateFolder("Assets/Resources", "Missions");
                
                AssetDatabase.CreateAsset(md, path);
            }
            manager.missions[i] = md;
        }
        AssetDatabase.SaveAssets();

        Debug.Log("Lobby Scene UI 생성 완료!");
    }

    private static UIMissionRecordCard CreateMissionCard(Transform parent, Font font, int index)
    {
        GameObject cardObj = new GameObject("MissionCard_" + index, typeof(RectTransform));
        cardObj.transform.SetParent(parent, false);
        Image bg = cardObj.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f);
        Button btn = cardObj.AddComponent<Button>();
        CanvasGroup cg = cardObj.AddComponent<CanvasGroup>();
        
        RectTransform rect = cardObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.sizeDelta = new Vector2(0, 160f); // 고정 높이

        // Title
        GameObject titleObj = new GameObject("Title", typeof(RectTransform));
        titleObj.transform.SetParent(cardObj.transform, false);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = "MISSION " + (index + 1);
        titleText.font = font;
        titleText.fontSize = 24;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = Color.white;
        titleText.horizontalOverflow = HorizontalWrapMode.Overflow;
        titleText.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.05f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.95f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;

        // Stats
        GameObject statsObj = new GameObject("Stats", typeof(RectTransform));
        statsObj.transform.SetParent(cardObj.transform, false);
        Text statsText = statsObj.AddComponent<Text>();
        statsText.text = "HIGH SCORE: ------\nBEST TIME: --:--\nDEATHS: -";
        statsText.font = font;
        statsText.fontSize = 18;
        statsText.color = Color.lightGray;
        statsText.horizontalOverflow = HorizontalWrapMode.Overflow;
        statsText.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform statsRect = statsObj.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.05f, 0.2f);
        statsRect.anchorMax = new Vector2(0.6f, 0.7f);
        statsRect.offsetMin = Vector2.zero;
        statsRect.offsetMax = Vector2.zero;

        // Lock Icon (임시 텍스트로 대체)
        GameObject lockObj = new GameObject("LockIcon", typeof(RectTransform));
        lockObj.transform.SetParent(cardObj.transform, false);
        Text lockText = lockObj.AddComponent<Text>();
        lockText.text = "LOCKED";
        lockText.font = font;
        lockText.fontSize = 28;
        lockText.color = Color.red;
        lockText.alignment = TextAnchor.MiddleRight;
        RectTransform lockRect = lockObj.GetComponent<RectTransform>();
        lockRect.anchorMin = new Vector2(0.6f, 0.5f);
        lockRect.anchorMax = new Vector2(0.95f, 0.9f);
        lockRect.offsetMin = Vector2.zero;
        lockRect.offsetMax = Vector2.zero;
        GameObject dummyLockObj = new GameObject("LockImage", typeof(RectTransform));
        dummyLockObj.transform.SetParent(lockObj.transform, false);
        Image dummyLockImg = dummyLockObj.AddComponent<Image>();
        dummyLockImg.color = new Color(0,0,0,0); // 투명

        // Progress Bar
        GameObject pbObj = new GameObject("ProgressBar", typeof(RectTransform));
        pbObj.transform.SetParent(cardObj.transform, false);
        Image pbBg = pbObj.AddComponent<Image>();
        pbBg.color = Color.gray;
        RectTransform pbRect = pbObj.GetComponent<RectTransform>();
        pbRect.anchorMin = new Vector2(0.05f, 0.05f);
        pbRect.anchorMax = new Vector2(0.95f, 0.15f);
        pbRect.offsetMin = Vector2.zero;
        pbRect.offsetMax = Vector2.zero;

        GameObject pbFillObj = new GameObject("Fill", typeof(RectTransform));
        pbFillObj.transform.SetParent(pbObj.transform, false);
        Image pbFill = pbFillObj.AddComponent<Image>();
        pbFill.color = Color.white;
        pbFill.type = Image.Type.Filled;
        pbFill.fillMethod = Image.FillMethod.Horizontal;
        pbFill.fillAmount = 0f;
        RectTransform pbFillRect = pbFillObj.GetComponent<RectTransform>();
        pbFillRect.anchorMin = Vector2.zero;
        pbFillRect.anchorMax = Vector2.one;
        pbFillRect.offsetMin = Vector2.zero;
        pbFillRect.offsetMax = Vector2.zero;

        // Stars Container
        GameObject starsObj = new GameObject("Stars", typeof(RectTransform));
        starsObj.transform.SetParent(cardObj.transform, false);
        HorizontalLayoutGroup hlg = starsObj.AddComponent<HorizontalLayoutGroup>();
        RectTransform starsRect = starsObj.GetComponent<RectTransform>();
        starsRect.anchorMin = new Vector2(0.7f, 0.2f);
        starsRect.anchorMax = new Vector2(0.95f, 0.45f);
        starsRect.offsetMin = Vector2.zero;
        starsRect.offsetMax = Vector2.zero;
        hlg.spacing = 5f;
        hlg.childControlWidth = true;
        hlg.childForceExpandWidth = true;

        Image[] stars = new Image[3];
        for(int s=0; s<3; s++)
        {
            GameObject sObj = new GameObject("Star", typeof(RectTransform));
            sObj.transform.SetParent(starsObj.transform, false);
            stars[s] = sObj.AddComponent<Image>();
            stars[s].color = new Color(0.2f, 0.2f, 0.2f); // Locked star color
        }

        UIMissionRecordCard card = cardObj.AddComponent<UIMissionRecordCard>();
        card.titleText = titleText;
        card.lockIcon = dummyLockImg;
        card.starIcons = stars;
        card.highScoreText = statsText;
        card.bestTimeText = statsText; // 편의상 같은 텍스트 컴포넌트 사용 (Setup에서 문자열 전체 포맷팅)
        card.deathsText = statsText;
        card.progressBarFill = pbFill;

        return card;
    }

    private static Button CreateDifficultyButton(Transform parent, string title, Color textColor, Font font)
    {
        GameObject btnObj = new GameObject("Button_" + title.Replace("\n", ""), typeof(RectTransform));
        btnObj.transform.SetParent(parent, false);
        Image img = btnObj.AddComponent<Image>();
        img.color = new Color(0.2f, 0.2f, 0.2f); // 기본 어두운 배경
        Outline outline = btnObj.AddComponent<Outline>();
        outline.effectColor = Color.yellow;
        outline.effectDistance = new Vector2(3, -3);
        outline.enabled = false; // 기본 비활성화
        Button btn = btnObj.AddComponent<Button>();

        GameObject textObj = new GameObject("Text", typeof(RectTransform));
        textObj.transform.SetParent(btnObj.transform, false);
        Text text = textObj.AddComponent<Text>();
        text.text = title;
        text.font = font;
        text.color = textColor;
        text.fontSize = 24;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        RectTransform rect = textObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        return btn;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            // Use new Input System module if available
            System.Type inputSystemModule = System.Type.GetType("UnityEngine.InputSystem.UI.InputSystemUIInputModule, Unity.InputSystem");
            if (inputSystemModule != null)
            {
                eventSystem.AddComponent(inputSystemModule);
            }
            else
            {
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }
    }

    private static void EnsureMainCamera()
    {
        if (Camera.main == null && Object.FindObjectOfType<Camera>() == null)
        {
            GameObject camObj = new GameObject("Main Camera");
            Camera cam = camObj.AddComponent<Camera>();
            camObj.AddComponent<AudioListener>();
            camObj.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Color.black;
        }
    }
}
