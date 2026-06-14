using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    [Header("UI References")]
    public UIMissionRecordCard[] missionCards; // 3개
    public Text summaryText;
    public Image summaryProgressBar;
    
    public Text enemyInfoText;
    public DifficultySelector diffSelector;
    public Button btnStartOperation;

    [Header("Mission Data")]
    public MissionData[] missions; // 3개

    private MissionData currentSelectedMission;
    private Difficulty currentSelectedDifficulty = Difficulty.Normal;
    private bool hasSelectedDifficulty = false;

    public CharacterSelector charSelector;
    public Sprite femaleSprite;
    public Sprite maleSprite;

    private void Start()
    {
        if (GameManager.isRestarting)
        {
            GameManager.isRestarting = false;
            
            // 이전 선택된 미션/난이도 복원
            int mIndex = 0;
            if (GameManager.Instance != null && missions != null)
            {
                for (int i = 0; i < missions.Length; i++)
                {
                    if (missions[i].missionId == GameManager.Instance.selectedMissionName)
                    {
                        mIndex = i;
                        break;
                    }
                }
            }
            
            if (missions != null && mIndex >= 0 && mIndex < missions.Length)
            {
                currentSelectedMission = missions[mIndex];
            }
            
            if (GameManager.Instance != null)
            {
                currentSelectedDifficulty = GameManager.Instance.currentDifficulty;
            }
            hasSelectedDifficulty = true;
            
            StartOperation();
            return;
        }

        // 로비 띄워져 있는 동안 뒷 배경(게임) 일시정지
        Time.timeScale = 0f;

        // 로비 화면일 때는 게임 UI(HUD, 인벤토리 등)를 숨깁니다.
        var hud = GameObject.Find("HUDCanvas");
        if (hud != null) hud.SetActive(false);
        var inv = GameObject.Find("InventoryCanvas");
        if (inv != null) inv.SetActive(false);
        
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.LoadGame();
            RefreshAllRecords();
        }

        if (diffSelector != null)
        {
            diffSelector.OnDifficultySelected += OnDifficultySelected;
        }

        if (btnStartOperation != null)
        {
            btnStartOperation.interactable = false;
            btnStartOperation.onClick.AddListener(StartOperation);
        }

        // 초기 선택 (MISSION 1)
        if (missions != null && missions.Length > 0)
        {
            SelectMission(0);
        }
    }

    private void RefreshAllRecords()
    {
        if (SaveManager.Instance == null) return;
        
        List<MissionRecord> records = SaveManager.Instance.GetAllRecords();
        int clearedCount = 0;
        float totalProgress = 0f;

        for (int i = 0; i < missionCards.Length; i++)
        {
            if (i < records.Count)
            {
                MissionRecord record = records[i];
                missionCards[i].Setup(record);
                
                if (record.isCleared) clearedCount++;
                
                if (record.isUnlocked)
                {
                    Button btn = missionCards[i].GetComponent<Button>();
                    if (btn != null)
                    {
                        int index = i;
                        btn.onClick.RemoveAllListeners();
                        btn.onClick.AddListener(() => SelectMission(index));
                    }
                }
            }
        }

        summaryText.text = $"CLEARED {clearedCount} / {missionCards.Length}";
        if (missionCards.Length > 0)
            summaryProgressBar.fillAmount = (float)clearedCount / missionCards.Length;
    }

    public void SelectMission(int index)
    {
        if (index < 0 || index >= missions.Length) return;
        
        if (SaveManager.Instance != null)
        {
            MissionRecord record = SaveManager.Instance.GetRecord(missions[index].missionId);
            if (record == null || !record.isUnlocked) return; // 잠긴 미션 선택 불가
        }
        
        currentSelectedMission = missions[index];
        if (enemyInfoText != null) 
        {
            enemyInfoText.text = currentSelectedMission.enemyDescription;
        }
        
        // 다시 난이도를 선택해야 시작 활성화
        hasSelectedDifficulty = false;
        if (btnStartOperation != null)
        {
            btnStartOperation.interactable = false;
        }
        
        // 시각적으로 강조 (테두리 등은 추가 개발)
    }

    private void OnDifficultySelected(Difficulty diff)
    {
        currentSelectedDifficulty = diff;
        hasSelectedDifficulty = true;
        
        if (currentSelectedMission != null)
        {
            btnStartOperation.interactable = true;
        }
    }

    private void StartOperation()
    {
        if (currentSelectedMission == null || !hasSelectedDifficulty) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.selectedMissionName = currentSelectedMission.missionId;
            GameManager.Instance.currentDifficulty = currentSelectedDifficulty;
        }

        // Apply selected character sprite
        if (charSelector != null)
        {
            GameObject playerObj = GameObject.Find("Player");
            
            // 만약 플레이어가 비활성화 상태라서 Find로 못 찾았다면, IntroCutscene에 연결된 참조를 통해 가져옵니다.
            if (playerObj == null)
            {
                IntroCutscene intro = Object.FindObjectOfType<IntroCutscene>(true);
                if (intro != null)
                {
                    playerObj = intro.playerCharacter;
                }
            }

            if (playerObj != null)
            {
                SpriteRenderer sr = playerObj.GetComponent<SpriteRenderer>();
                PlayerMovement pm = playerObj.GetComponent<PlayerMovement>();
                Animator anim = playerObj.GetComponent<Animator>();
                if (sr != null)
                {
                    if (charSelector.currentSelected == PlayerCharacterType.Female && femaleSprite != null)
                    {
                        sr.sprite = femaleSprite;
                        if (pm != null) pm.UpdateNormalSprite(femaleSprite);
                        if (anim != null) anim.enabled = true;
                        Debug.Log("[LobbyManager] 여캐 스프라이트가 적용되었습니다.");
                    }
                    else if (charSelector.currentSelected == PlayerCharacterType.Male && maleSprite != null)
                    {
                        sr.sprite = maleSprite;
                        if (pm != null) pm.UpdateNormalSprite(maleSprite);
                        if (anim != null) anim.enabled = false;
                        Debug.Log("[LobbyManager] 남캐 스프라이트가 적용되었습니다.");
                    }
                    
                    // Update PolygonCollider2D to match the new sprite
                    PolygonCollider2D polyCollider = playerObj.GetComponent<PolygonCollider2D>();
                    if (polyCollider != null)
                    {
                        Destroy(polyCollider);
                        playerObj.AddComponent<PolygonCollider2D>();
                    }
                }
            }
        }

        // 한 씬 안에서 로비와 게임을 같이 쓸 때는 씬 로드가 아니라 UI만 숨기면 됩니다.
        gameObject.SetActive(false);
        
        // 숨겨뒀던 게임 UI 다시 켜기
        var canvases = Resources.FindObjectsOfTypeAll<Canvas>();
        foreach (var canvas in canvases)
        {
            if (canvas.name == "HUDCanvas" || canvas.name == "InventoryCanvas")
            {
                canvas.gameObject.SetActive(true);
            }
        }
        
        // 로비가 꺼지면 게임이 시작되도록 시간 흐름 정상화
        Time.timeScale = 1f;

        // 인트로 컷신 시작
        IntroCutscene cutscene = Object.FindObjectOfType<IntroCutscene>();
        if (cutscene != null)
        {
            cutscene.StartCutscene();
        }

        // 게임 시작 시 인게임 BGM(gamebgm)으로 교체하여 재생
        GameObject bgmManager = GameObject.Find("BGM_Manager");
        if (bgmManager != null)
        {
            AudioSource audio = bgmManager.GetComponent<AudioSource>();
            if (audio != null)
            {
                AudioClip gameBGM = Resources.Load<AudioClip>("Sounds/gamebgm");
                if (gameBGM != null)
                {
                    audio.clip = gameBGM;
                    audio.Play();
                }
                else
                {
                    audio.Stop();
                }
            }
        }
    }
}
