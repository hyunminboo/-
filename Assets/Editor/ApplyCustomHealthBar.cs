using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

public class ApplyCustomHealthBar
{
    [MenuItem("Custom Tools/Apply Custom Health Bar")]
    public static void ApplyUI()
    {
        if (EditorApplication.isPlaying) return;

        // Ensure the directory exists
        if (!System.IO.Directory.Exists("Assets/Sprites/UI"))
        {
            System.IO.Directory.CreateDirectory("Assets/Sprites/UI");
            AssetDatabase.Refresh();
        }

        GameObject oldHUD = GameObject.Find("HUDCanvas");
        if (oldHUD != null) Object.DestroyImmediate(oldHUD);

        GameObject canvasObj = new GameObject("HUDCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50; 
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject textObj = new GameObject("HP_Text");
        textObj.transform.SetParent(canvasObj.transform, false);
        Text hpText = textObj.AddComponent<Text>();
        hpText.text = "HP";
        hpText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hpText.fontSize = 35;
        hpText.fontStyle = FontStyle.Bold;
        hpText.color = Color.white;
        RectTransform tRect = textObj.GetComponent<RectTransform>();
        tRect.anchorMin = new Vector2(0, 1);
        tRect.anchorMax = new Vector2(0, 1);
        tRect.pivot = new Vector2(0, 1);
        tRect.sizeDelta = new Vector2(100, 50);
        tRect.anchoredPosition = new Vector2(50, -40);

        // 체력 바 배경 (금색 프레임)
        GameObject bgObj = new GameObject("HealthBar_BG");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        
        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/hp_bg.png");
        if (bgSprite != null) {
            bgImg.sprite = bgSprite;
            bgImg.color = Color.white; // 본연의 색상 유지
        } else {
            bgImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // 이미지 없으면 임시 배경
        }
        
        RectTransform bgRect = bgImg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0f, 1f);
        bgRect.anchorMax = new Vector2(0f, 1f);
        bgRect.pivot = new Vector2(0f, 1f);
        // 비율에 맞게 넓게 설정 (예: 400x50)
        bgRect.sizeDelta = new Vector2(400, 50); 
        bgRect.anchoredPosition = new Vector2(120, -40); 

        // 체력 바 채우기 (푸른색 알맹이)
        GameObject fillObj = new GameObject("HealthBar_Fill");
        fillObj.transform.SetParent(bgObj.transform, false);
        Image fillImg = fillObj.AddComponent<Image>();
        fillImg.color = Color.white; // 색상 강제 오버라이드 끔 (이미지 색상 그대로)
        
        Sprite fillSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/hp_fill.png");
        if (fillSprite != null) {
            fillImg.sprite = fillSprite;
        } else {
            // 이미지 없을 시 기본 하얀색
            fillImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            fillImg.color = new Color(0.2f, 0.8f, 0.2f);
        }
        
        fillImg.type = Image.Type.Filled;
        fillImg.fillMethod = Image.FillMethod.Horizontal;
        fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImg.fillAmount = 1f;

        RectTransform fillRect = fillImg.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        // 여백 살짝 줘서 금빛 프레임 안에 들어가게 (상하좌우 약간의 여백)
        fillRect.offsetMin = new Vector2(15, 10);
        fillRect.offsetMax = new Vector2(-15, -10);

        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerHealth hpScript = player.GetComponent<PlayerHealth>();
            if (hpScript == null) hpScript = player.AddComponent<PlayerHealth>();
            
            hpScript.healthBarFill = fillImg;
            UnityEditor.EditorUtility.SetDirty(player);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(canvasObj.scene);
        SceneView.RepaintAll();
        Debug.Log("커스텀 이미지 체력 바 적용 완료!");
    }
}
