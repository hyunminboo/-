using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossWarningUI : MonoBehaviour
{
    private Canvas canvas;
    private Text bossText;
    private Text xMarkText;
    
    private float flashTimer = 0f;
    private bool isVisible = true;

    void Start()
    {
        // 1. 임시 캔버스 생성
        GameObject canvasObj = new GameObject("BossWarningCanvas");
        canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // 최상단 배치
        
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        
        // 2. 배경 패널 (살짝 어둡게)
        GameObject bgObj = new GameObject("WarningBG");
        bgObj.transform.SetParent(canvasObj.transform, false);
        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0f, 0f, 0f, 0.4f);
        RectTransform bgRt = bgImg.GetComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.sizeDelta = Vector2.zero;

        // 3. 중앙에 커다란 X 마크
        GameObject xObj = new GameObject("X_Mark");
        xObj.transform.SetParent(canvasObj.transform, false);
        xMarkText = xObj.AddComponent<Text>();
        xMarkText.font = Resources.Load<Font>("Fonts/NanumGothic"); // 임시 폰트
        xMarkText.text = "X";
        xMarkText.fontSize = 300;
        xMarkText.color = new Color(1f, 0f, 0f, 0.7f); // 반투명 빨강
        xMarkText.alignment = TextAnchor.MiddleCenter;
        RectTransform xRt = xMarkText.GetComponent<RectTransform>();
        xRt.anchorMin = new Vector2(0.5f, 0.5f);
        xRt.anchorMax = new Vector2(0.5f, 0.5f);
        xRt.sizeDelta = new Vector2(500, 500);

        // 4. "BOSS" 텍스트
        GameObject textObj = new GameObject("Boss_Text");
        textObj.transform.SetParent(canvasObj.transform, false);
        bossText = textObj.AddComponent<Text>();
        bossText.font = Resources.Load<Font>("Fonts/NanumGothic");
        bossText.text = "WARNING\nBOSS";
        bossText.fontSize = 120;
        bossText.color = Color.white;
        bossText.alignment = TextAnchor.MiddleCenter;
        RectTransform tRt = bossText.GetComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0.5f, 0.5f);
        tRt.anchorMax = new Vector2(0.5f, 0.5f);
        tRt.sizeDelta = new Vector2(800, 300);

        // 경고 사운드가 있다면 재생 (선택)
        AudioClip warningSound = Resources.Load<AudioClip>("Sounds/Warning");
        if (warningSound != null) AudioSource.PlayClipAtPoint(warningSound, Camera.main.transform.position);

        // 1.5초 후 자동 삭제
        Destroy(canvasObj, 2.0f);
        Destroy(gameObject, 2.0f);
    }

    void Update()
    {
        // 텍스트 깜빡임 효과 (플래시)
        flashTimer += Time.deltaTime;
        if (flashTimer > 0.15f)
        {
            flashTimer = 0f;
            isVisible = !isVisible;
            if (xMarkText != null) xMarkText.enabled = isVisible;
            if (bossText != null) bossText.enabled = isVisible;
        }
    }
}
