using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    public Difficulty currentDifficulty = Difficulty.Normal;
    public string selectedMissionName = "MISSION 1";
    
    // 즉시 재시작을 위한 플래그
    public static bool isRestarting = false;
    
    // In-game tracking
    public int currentScore = 0;
    public float currentMissionTime = 0f;
    private bool isTimerRunning = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            if (SceneManager.GetActiveScene().name != "LobbyScene")
            {
                isTimerRunning = true;
            }
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "LobbyScene")
        {
            ResetMissionStats();
            isTimerRunning = true;
        }
        else
        {
            isTimerRunning = false;
        }
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            currentMissionTime += Time.deltaTime;
        }
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
    }

    public void StopTimer()
    {
        isTimerRunning = false;
    }

    public void ResetMissionStats()
    {
        currentScore = 0;
        currentMissionTime = 0f;
    }

    public float GetDifficultyMultiplier()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Hard: return 1.5f;
            case Difficulty.Nightmare: return 2.0f;
            case Difficulty.Normal:
            default: return 0.8f; // 노멀 난이도를 기존 1.0에서 0.8로 하향 조정
        }
    }
}
