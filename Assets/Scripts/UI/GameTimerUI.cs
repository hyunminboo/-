using UnityEngine;
using UnityEngine.UI;

public class GameTimerUI : MonoBehaviour
{
    private Text timerText;

    private void Awake()
    {
        timerText = GetComponent<Text>();
    }

    private void Start()
    {
        // 강제로 크기를 키우고 우측 상단 모서리로 정확히 이동시킵니다
        if (timerText != null)
        {
            timerText.fontSize = 72; // 글자 크기를 훨씬 크게 (기존 48 -> 72)
            timerText.alignment = TextAnchor.UpperRight; // 우측 상단 정렬
            
            RectTransform rt = GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = new Vector2(1, 1);
                rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                rt.anchoredPosition = new Vector2(-50, -50); // 여백을 약간 주고 우측 상단에 딱 붙임
                rt.sizeDelta = new Vector2(300, 100); // 텍스트 영역을 넉넉하게 확장
            }
        }
    }

    private void Update()
    {
        if (GameManager.Instance != null && timerText != null)
        {
            float time = GameManager.Instance.currentMissionTime;
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            
            // MM:SS 포맷으로 시간 표시
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}
