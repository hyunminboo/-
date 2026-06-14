using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class CreateTimerUIEditor : EditorWindow
{
    [MenuItem("Tools/Create Timer UI")]
    public static void CreateTimerUI()
    {
        // 1. 기존 캔버스 찾기 (없으면 새로 생성)
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // 2. EventSystem 없으면 생성
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 3. 우측 상단 타이머 텍스트 생성
        GameObject timerObj = new GameObject("GameTimerText");
        timerObj.transform.SetParent(canvas.transform, false);

        Text timerText = timerObj.AddComponent<Text>();
        timerText.text = "00:00";
        timerText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        timerText.fontSize = 48;
        timerText.color = Color.white;
        timerText.alignment = TextAnchor.MiddleRight;

        // 약간의 외곽선 추가 (가독성 향상)
        Outline outline = timerObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(2f, -2f);

        // RectTransform 설정 (우측 상단 고정)
        RectTransform rt = timerObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(1, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(1, 1);
        rt.anchoredPosition = new Vector2(-30, -30); // 우측, 상단에서 30픽셀 여백
        rt.sizeDelta = new Vector2(200, 60);

        // 스크립트 부착
        timerObj.AddComponent<GameTimerUI>();

        Selection.activeGameObject = timerObj;
        Debug.Log("Timer UI created successfully in the Canvas!");
    }
}
