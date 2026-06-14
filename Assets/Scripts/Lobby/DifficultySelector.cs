using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class DifficultySelector : MonoBehaviour
{
    [Header("Difficulty Buttons")]
    public Button btnNormal;
    public Button btnHard;
    public Button btnNightmare;

    private Difficulty currentSelected = Difficulty.Normal;
    private Outline outNormal;
    private Outline outHard;
    private Outline outNightmare;

    public UnityAction<Difficulty> OnDifficultySelected;

    private void Awake()
    {
        if (btnNormal != null) outNormal = btnNormal.GetComponent<Outline>();
        if (btnHard != null) outHard = btnHard.GetComponent<Outline>();
        if (btnNightmare != null) outNightmare = btnNightmare.GetComponent<Outline>();

        if (btnNormal != null) btnNormal.onClick.AddListener(() => SelectDifficulty(Difficulty.Normal));
        if (btnHard != null) btnHard.onClick.AddListener(() => SelectDifficulty(Difficulty.Hard));
        if (btnNightmare != null) btnNightmare.onClick.AddListener(() => SelectDifficulty(Difficulty.Nightmare));
    }

    public void SelectDifficulty(Difficulty diff)
    {
        currentSelected = diff;
        
        UpdateButtonVisual(btnNormal, outNormal, diff == Difficulty.Normal, new Color(0f, 1f, 0f, 1f)); // Green for Normal
        UpdateButtonVisual(btnHard, outHard, diff == Difficulty.Hard, new Color(1f, 0.5f, 0f, 1f));   // Orange for Hard
        UpdateButtonVisual(btnNightmare, outNightmare, diff == Difficulty.Nightmare, new Color(1f, 0f, 0f, 1f)); // Red for Nightmare

        OnDifficultySelected?.Invoke(diff);
    }

    private void UpdateButtonVisual(Button btn, Outline outline, bool isSelected, Color highlightColor)
    {
        if (btn == null) return;

        Image img = btn.GetComponent<Image>();
        if (img != null)
        {
            if (isSelected)
            {
                // 선택된 난이도의 색상을 띠면서 약간 밝은 유리 느낌
                Color selectedColor = new Color(highlightColor.r * 0.5f + 0.2f, highlightColor.g * 0.5f + 0.2f, highlightColor.b * 0.5f + 0.2f, 0.9f);
                img.color = selectedColor;
            }
            else
            {
                // 선택 안 된 기본 어두운 유리 느낌
                img.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
            }
        }
        
        // 기존에 달려있던 아웃라인 끄기 (버그 원인)
        if (outline != null)
        {
            outline.enabled = false;
        }
    }
}
