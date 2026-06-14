using UnityEngine;
using UnityEngine.UI;

public class GoIndicator : MonoBehaviour
{
    public static GoIndicator instance;
    private Text goText;
    private RectTransform rectTransform;
    
    private bool isShowing = false;
    private Vector2 basePosition;

    public float hoverSpeed = 5f;
    public float hoverAmount = 20f;

    void Awake()
    {
        instance = this;
        goText = GetComponent<Text>();
        rectTransform = GetComponent<RectTransform>();
        
        if (goText != null)
        {
            goText.color = Color.white; // 하얀색으로 고정
            goText.enabled = false; // 기본적으로 무조건 숨김
        }
    }

    void Start()
    {
        if (rectTransform != null)
        {
            // 초기 위치 저장
            basePosition = rectTransform.anchoredPosition;
        }
    }

    void Update()
    {
        if (isShowing && rectTransform != null)
        {
            // 위아래로 둥둥 떠다니는(Hover) 효과
            float newY = basePosition.y + Mathf.Sin(Time.time * hoverSpeed) * hoverAmount;
            rectTransform.anchoredPosition = new Vector2(basePosition.x, newY);

            // 플레이어가 이동 키(좌/우, A/D)를 누르면 즉시 사라지게 함
            if (UnityEngine.InputSystem.Keyboard.current != null)
            {
                var kb = UnityEngine.InputSystem.Keyboard.current;
                if (kb.leftArrowKey.isPressed || kb.rightArrowKey.isPressed || kb.aKey.isPressed || kb.dKey.isPressed)
                {
                    HideGo();
                }
            }
        }
    }

    public void ShowGo()
    {
        if (goText != null)
        {
            goText.enabled = true;
            isShowing = true;
            
            // 보여질 때 현재 위치를 다시 기준점으로 잡음
            if (rectTransform != null)
            {
                basePosition = rectTransform.anchoredPosition;
            }
        }
    }

    public void HideGo()
    {
        if (goText != null)
        {
            goText.enabled = false;
            isShowing = false;
        }
    }
}
