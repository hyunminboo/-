using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameClearUIManager : MonoBehaviour
{
    private static GameClearUIManager _instance;
    public static GameClearUIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("GameClearUIManager");
                _instance = go.AddComponent<GameClearUIManager>();
            }
            return _instance;
        }
    }

    public void ShowGameClear()
    {
        // 1. Canvas 생성
        GameObject canvasObj = new GameObject("GameClearCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // 최상단
        
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        // 2. 배경 패널 (반투명 검정)
        GameObject panelObj = new GameObject("BackgroundPanel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRT = panelObj.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.sizeDelta = Vector2.zero;

        // 3. GAME CLEAR 텍스트
        GameObject textObj = new GameObject("ClearText");
        textObj.transform.SetParent(panelObj.transform, false);
        Text clearText = textObj.AddComponent<Text>();
        clearText.text = "GAME CLEAR";
        clearText.font = Resources.Load<Font>("Fonts/NanumGothic");
        clearText.fontSize = 100;
        clearText.fontStyle = FontStyle.Bold;
        clearText.color = Color.yellow;
        clearText.alignment = TextAnchor.MiddleCenter;
        
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(3, -3);

        RectTransform textRT = textObj.GetComponent<RectTransform>();
        textRT.anchoredPosition = new Vector2(0, 100);
        textRT.sizeDelta = new Vector2(800, 200);

        // 4. 로비로 돌아가기 버튼
        CreateButton(panelObj.transform, "Return to Lobby", "로비로 돌아가기", new Vector2(0, -50), () => 
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name); // 게임이 단일 씬 구조이므로 재로드 시 로비 표시됨
        });

        // 5. 게임 종료 버튼
        CreateButton(panelObj.transform, "Quit Game", "게임 종료", new Vector2(0, -150), () => 
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
        });

        // 시간이 잠시 정지되도록 (옵션)
        // Time.timeScale = 0f;
    }

    private void CreateButton(Transform parent, string name, string label, Vector2 pos, UnityEngine.Events.UnityAction action)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        Image btnImage = buttonObj.AddComponent<Image>();
        btnImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        Button btn = buttonObj.AddComponent<Button>();
        btn.onClick.AddListener(action);

        RectTransform btnRT = buttonObj.GetComponent<RectTransform>();
        btnRT.anchoredPosition = pos;
        btnRT.sizeDelta = new Vector2(300, 80);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        Text txt = textObj.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.Load<Font>("Fonts/NanumGothic");
        txt.fontSize = 30;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        
        RectTransform txtRT = textObj.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.sizeDelta = Vector2.zero;
    }
}
