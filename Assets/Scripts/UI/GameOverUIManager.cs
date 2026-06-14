using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverUIManager : MonoBehaviour
{
    public static GameOverUIManager instance;
    public GameObject gameOverPanel;

    void Awake()
    {
        instance = this;
    }

    public void ShowGameOver()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void RetryGame()
    {
        Time.timeScale = 1f;
        GameManager.isRestarting = true;
        
        // 생존 시간/점수 기록 갱신 (재도전 시에도 기록)
        if (SaveManager.Instance != null && GameManager.Instance != null)
        {
            SaveManager.Instance.RecordSurvivalTime(GameManager.Instance.selectedMissionName, GameManager.Instance.currentMissionTime, GameManager.Instance.currentScore);
        }
        
        // 씬 즉시 재시작 (로비 스킵)
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void ReturnToLobby()
    {
        Time.timeScale = 1f;
        
        // 로비로 돌아갈 때 생존 시간/점수 기록 저장
        if (SaveManager.Instance != null && GameManager.Instance != null)
        {
            SaveManager.Instance.RecordSurvivalTime(GameManager.Instance.selectedMissionName, GameManager.Instance.currentMissionTime, GameManager.Instance.currentScore);
        }
        
        // 씬을 다시 로드하여 초기 상태(로비 표시)로 돌아감
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
