using UnityEngine;
using UnityEngine.UI;

public class UIMissionRecordCard : MonoBehaviour
{
    public string missionId;
    
    [Header("UI References")]
    public Text titleText;
    public Image lockIcon;
    public Image[] starIcons;
    public Text highScoreText;
    public Text bestTimeText;
    public Text deathsText;
    public Image progressBarFill;
    
    [Header("Colors")]
    public Color lockedColor = Color.gray;
    public Color unlockedColor = Color.white;
    public Color normalStarColor = Color.white;
    public Color hardStarColor = Color.yellow;
    public Color nightmareStarColor = Color.red;
    public Color lockedStarColor = new Color(0.2f, 0.2f, 0.2f, 1f);

    public void Setup(MissionRecord record)
    {
        missionId = record.missionId;
        
        // "MISSION 1" -> "Player 1" 형태로 변환해서 표시
        string displayTitle = missionId.Replace("MISSION", "Player").Replace(" ", " ");
        if (titleText != null) titleText.text = displayTitle;
        
        if (!record.isUnlocked)
        {
            // 잠금 상태 (이제 모든 플레이어 슬롯이 열려있도록 LobbyManager에서 처리할 수도 있지만, 일단 기본 로직 유지)
            SetCardAlpha(0.5f);
            if (lockIcon != null) lockIcon.gameObject.SetActive(true);
            if (highScoreText != null) highScoreText.text = ""; // 숨김
            if (bestTimeText != null) bestTimeText.text = "PLAY TIME: --:--";
            if (deathsText != null) deathsText.text = ""; // 숨김
            if (progressBarFill != null) progressBarFill.fillAmount = 0f;
            
            if (starIcons != null) foreach (var star in starIcons) if (star != null) star.color = lockedStarColor;
        }
        else
        {
            SetCardAlpha(1f);
            if (lockIcon != null) lockIcon.gameObject.SetActive(false);
            
            if (highScoreText != null) highScoreText.text = ""; // 숨김
            if (deathsText != null) deathsText.text = ""; // 숨김

            if (!record.isCleared)
            {
                // 해제됨, 미클리어 (가장 최근 생존 시간 표시)
                int mins = Mathf.FloorToInt(record.lastPlayTime / 60f);
                int secs = Mathf.FloorToInt(record.lastPlayTime % 60f);
                if (bestTimeText != null) bestTimeText.text = string.Format("PLAY TIME: {0:00}:{1:00}", mins, secs);
                
                if (progressBarFill != null) progressBarFill.fillAmount = 0f;
                if (starIcons != null) foreach (var star in starIcons) if (star != null) star.color = lockedStarColor;
            }
            else
            {
                // 클리어 (가장 최근 플레이 시간 표시)
                int mins = Mathf.FloorToInt(record.lastPlayTime / 60f);
                int secs = Mathf.FloorToInt(record.lastPlayTime % 60f);
                if (bestTimeText != null) bestTimeText.text = string.Format("PLAY TIME: {0:00}:{1:00}", mins, secs);
                
                if (progressBarFill != null) progressBarFill.fillAmount = 1f;
                
                // 난이도 별 색상 처리
                Color diffColor = normalStarColor;
                if (record.highestDifficulty == Difficulty.Hard) diffColor = hardStarColor;
                else if (record.highestDifficulty == Difficulty.Nightmare) diffColor = nightmareStarColor;
                
                if (progressBarFill != null) progressBarFill.color = diffColor;
                
                int starCount = (int)record.highestDifficulty + 1;
                if (starIcons != null)
                {
                    for (int i = 0; i < starIcons.Length; i++)
                    {
                        if (starIcons[i] != null)
                        {
                            if (i < starCount) starIcons[i].color = diffColor;
                            else starIcons[i].color = lockedStarColor;
                        }
                    }
                }
            }
        }
    }

    private void SetCardAlpha(float alpha)
    {
        CanvasGroup cg = GetComponent<CanvasGroup>();
        if (cg != null) cg.alpha = alpha;
    }
}
