using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public enum Difficulty { Normal, Hard, Nightmare }

[Serializable]
public class MissionRecord
{
    public string missionId;
    public bool isUnlocked;
    public bool isCleared;
    public int highScore;
    public float bestClearTime;
    public float longestSurvivalTime; // 최고 생존 기록
    public float lastPlayTime;        // 최근 플레이 기록
    public int totalDeaths;
    public Difficulty highestDifficulty;
    public bool isNew;

    public MissionRecord(string id)
    {
        missionId = id;
        isUnlocked = false;
        isCleared = false;
        highScore = 0;
        bestClearTime = float.MaxValue;
        longestSurvivalTime = 0f;
        lastPlayTime = 0f;
        totalDeaths = 0;
        highestDifficulty = Difficulty.Normal;
        isNew = true;
    }
}

[Serializable]
public class SaveData
{
    public List<MissionRecord> missionRecords = new List<MissionRecord>();
}

public class SaveManager : MonoBehaviour
{
    private static SaveManager _instance;
    public static SaveManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SaveManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("SaveManager");
                    _instance = go.AddComponent<SaveManager>();
                }
            }
            return _instance;
        }
    }
    
    private SaveData currentSaveData;
    private string saveFilePath;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            saveFilePath = Path.Combine(Application.persistentDataPath, "save.json");
            LoadGame();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            currentSaveData = JsonUtility.FromJson<SaveData>(json);
            
            // 기존 세이브 파일에서도 모든 슬롯 강제 해제
            if (currentSaveData != null)
            {
                foreach (var rec in currentSaveData.missionRecords)
                {
                    rec.isUnlocked = true;
                }
            }
        }
        
        if (currentSaveData == null)
        {
            currentSaveData = new SaveData();
            // 플레이어 슬롯 3개 기본 추가 (모두 해제 상태)
            currentSaveData.missionRecords.Add(new MissionRecord("MISSION 1") { isUnlocked = true });
            currentSaveData.missionRecords.Add(new MissionRecord("MISSION 2") { isUnlocked = true });
            currentSaveData.missionRecords.Add(new MissionRecord("MISSION 3") { isUnlocked = true });
            SaveGame();
        }
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(currentSaveData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log("Game Saved to: " + saveFilePath);
    }

    public MissionRecord GetRecord(string missionId)
    {
        return currentSaveData.missionRecords.Find(r => r.missionId == missionId);
    }
    
    public List<MissionRecord> GetAllRecords()
    {
        return currentSaveData.missionRecords;
    }

    public void UpdateMissionRecord(string missionId, int score, float time, Difficulty diff)
    {
        MissionRecord record = GetRecord(missionId);
        if (record != null)
        {
            record.isCleared = true;
            record.isNew = false;
            record.lastPlayTime = time; // 최근 클리어 시간 저장
            
            if (score > record.highScore) record.highScore = score;
            if (time < record.bestClearTime) record.bestClearTime = time;
            
            if ((int)diff > (int)record.highestDifficulty || (record.highestDifficulty == Difficulty.Normal && diff == Difficulty.Normal))
            {
                record.highestDifficulty = diff;
            }
            
            // 다음 미션 잠금 해제
            if (missionId == "MISSION 1")
            {
                var m2 = GetRecord("MISSION 2");
                if (m2 != null) m2.isUnlocked = true;
            }
            else if (missionId == "MISSION 2")
            {
                var m3 = GetRecord("MISSION 3");
                if (m3 != null) m3.isUnlocked = true;
            }
            
            SaveGame();
        }
    }

    public void AddDeath(string missionId)
    {
        MissionRecord record = GetRecord(missionId);
        if (record != null)
        {
            record.totalDeaths++;
            SaveGame();
        }
    }

    public void RecordSurvivalTime(string missionId, float time, int score)
    {
        MissionRecord record = GetRecord(missionId);
        if (record != null)
        {
            record.lastPlayTime = time; // 무조건 최근 플레이 시간 기록
            if (time > record.longestSurvivalTime) record.longestSurvivalTime = time;
            if (score > record.highScore) record.highScore = score;
            SaveGame();
        }
    }
}
