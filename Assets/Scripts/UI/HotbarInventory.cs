using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[System.Serializable]
public class HotbarSlot
{
    public ItemType itemType;
    public int count = 0;
    public Image iconImage;
    public Text countText;
    public Outline selectionOutline; // 에디터에서 수동 할당 안해도 코드에서 추가하도록 변경
}

public class HotbarInventory : MonoBehaviour
{
    public static HotbarInventory instance;

    public HotbarSlot[] slots = new HotbarSlot[5];
    public Sprite medkitSprite;
    public Sprite grenadeSprite;
    public Sprite swordSprite;
    public Sprite gunSprite;

    public int selectedIndex = -1;

    private void Awake()
    {
        instance = this;
        ResetInventory();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 로비씬이 아닌 게임씬이 로드될 때 인벤토리를 비움 (재시작 시 초기화)
        if (scene.name != "LobbyScene")
        {
            ResetInventory();
            
            // 다시 총이 있다면 장착하게끔 호출
            GameObject player = GameObject.Find("Player");
            if (player != null)
            {
                PlayerShooting shooting = player.GetComponent<PlayerShooting>();
                if (shooting != null && shooting.canShoot)
                {
                    AddItem(ItemType.Gun, 1);
                    SelectSlot(0);
                }
            }
        }
    }

    public void ResetInventory()
    {
        selectedIndex = -1;
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].itemType = (ItemType)999; // None
                slots[i].count = 0;
                UpdateSlotUI(i);
            }
        }
        SelectSlot(-1);
    }

    private void Start()
    {
        // 런타임에 Outline(장착 하이라이트) 컴포넌트 자동 추가 및 설정
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].iconImage != null)
            {
                Outline outline = slots[i].iconImage.gameObject.GetComponent<Outline>();
                if (outline == null) outline = slots[i].iconImage.gameObject.AddComponent<Outline>();
                
                outline.effectColor = Color.yellow;
                outline.effectDistance = new Vector2(4, -4);
                outline.enabled = false;
                slots[i].selectionOutline = outline;
            }
            
            // UI를 빈 상태(count = 0)로 확실히 갱신합니다.
            UpdateSlotUI(i);
        }
        
        // 시작할 때 플레이어가 총을 들고 있다면 인벤토리에 추가
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerShooting shooting = player.GetComponent<PlayerShooting>();
            if (shooting != null && shooting.canShoot)
            {
                AddItem(ItemType.Gun, 1);
                SelectSlot(0);
            }
        }
    }

    public bool AddItem(ItemType type, int amount)
    {
        // 1. 기존 스택 찾기 (수류탄, 구급상자 등 쌓이는 아이템)
        if (type == ItemType.Medkit || type == ItemType.GrenadeAmmo)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].count > 0 && slots[i].itemType == type)
                {
                    slots[i].count += amount;
                    UpdateSlotUI(i);
                    return true;
                }
            }
        }

        // 2. 빈 슬롯 찾기
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].count == 0)
            {
                slots[i].itemType = type;
                slots[i].count = amount;
                UpdateSlotUI(i);
                
                // 처음 먹은거면 자동 장착 (소모품은 자동장착 방지하여 무기가 풀리는 버그 수정)
                if (selectedIndex == -1 && (type == ItemType.Gun || type == ItemType.Sword))
                {
                    SelectSlot(i);
                }
                return true;
            }
        }

        return false; // 인벤토리 꽉 참
    }

    public void ConsumeSelectedItem()
    {
        if (selectedIndex < 0 || selectedIndex >= slots.Length) return;
        if (slots[selectedIndex].count <= 0) return;

        slots[selectedIndex].count--;
        UpdateSlotUI(selectedIndex);

        if (slots[selectedIndex].count <= 0)
        {
            // 아이템 다 쓰면 자동으로 무기(총이나 칼)를 찾아 재장착합니다.
            bool weaponFound = false;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].count > 0 && (slots[i].itemType == ItemType.Gun || slots[i].itemType == ItemType.Sword))
                {
                    SelectSlot(i);
                    weaponFound = true;
                    break;
                }
            }
            if (!weaponFound)
            {
                SelectSlot(-1);
            }
        }
    }

    private void UpdateSlotUI(int index)
    {
        if (slots[index].count > 0)
        {
            if (slots[index].iconImage != null)
            {
                slots[index].iconImage.enabled = true;
                if (slots[index].itemType == ItemType.Medkit) slots[index].iconImage.sprite = medkitSprite;
                else if (slots[index].itemType == ItemType.GrenadeAmmo) slots[index].iconImage.sprite = grenadeSprite;
                else if (slots[index].itemType == ItemType.Sword) slots[index].iconImage.sprite = swordSprite;
                else if (slots[index].itemType == ItemType.Gun) slots[index].iconImage.sprite = gunSprite;
            }
            if (slots[index].countText != null)
            {
                // 장비류는 개수 표시 안함
                if (slots[index].itemType == ItemType.Sword || slots[index].itemType == ItemType.Gun)
                    slots[index].countText.text = "";
                else
                    slots[index].countText.text = slots[index].count.ToString();
            }
        }
        else
        {
            if (slots[index].iconImage != null) slots[index].iconImage.enabled = false;
            if (slots[index].countText != null) slots[index].countText.text = "";
        }
    }

    void Update()
    {
        if (Keyboard.current == null) return;
        
        if (Keyboard.current.digit1Key.wasPressedThisFrame) HandleSlotInput(0);
        if (Keyboard.current.digit2Key.wasPressedThisFrame) HandleSlotInput(1);
        if (Keyboard.current.digit3Key.wasPressedThisFrame) HandleSlotInput(2);
        if (Keyboard.current.digit4Key.wasPressedThisFrame) HandleSlotInput(3);
        if (Keyboard.current.digit5Key.wasPressedThisFrame) HandleSlotInput(4);
    }

    private void HandleSlotInput(int index)
    {
        if (index < 0 || index >= slots.Length) return;
        if (slots[index].count <= 0) return; // 빈 슬롯

        if (selectedIndex == index)
        {
            // 이미 선택된 상태에서 다시 누르면 (구급상자 등 즉시 사용)
            if (slots[index].itemType == ItemType.Medkit)
            {
                GameObject player = GameObject.Find("Player");
                if (player != null)
                {
                    PlayerHealth ph = player.GetComponent<PlayerHealth>();
                    if (ph != null) 
                    {
                        ph.Heal(50f);
                        ConsumeSelectedItem(); // 1개 소모
                    }
                }
            }
        }
        else
        {
            // 새 슬롯 장착
            SelectSlot(index);
        }
    }

    private void SelectSlot(int index)
    {
        selectedIndex = index;

        // UI 하이라이트 갱신
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].selectionOutline != null)
            {
                slots[i].selectionOutline.enabled = (i == selectedIndex);
            }
            
            // "장착중" 텍스트 대신 개수 텍스트 색상을 노란색으로 강조
            if (slots[i].countText != null)
            {
                slots[i].countText.color = (i == selectedIndex) ? Color.yellow : Color.white;
            }
        }

        // 실제 플레이어 장비 연동
        UpdatePlayerEquipment();
    }

    private void UpdatePlayerEquipment()
    {
        GameObject player = GameObject.Find("Player");
        if (player == null) return;

        PlayerMelee melee = player.GetComponent<PlayerMelee>();
        PlayerShooting shooting = player.GetComponent<PlayerShooting>();

        ItemType activeType = (selectedIndex >= 0 && slots[selectedIndex].count > 0) ? slots[selectedIndex].itemType : (ItemType)999;

        if (activeType == ItemType.Sword)
        {
            if (melee != null) melee.EnableMelee();
            if (shooting != null) shooting.HideShooting();
        }
        else if (activeType == ItemType.Gun)
        {
            if (shooting != null) shooting.EnableShooting();
            if (melee != null) melee.HideMelee();
        }
        else
        {
            // 무기가 아닌 아이템(구급상자, 수류탄 등)을 선택했거나, 아무것도 없는 경우
            // 인벤토리에 무기가 아예 없다면 무기를 숨깁니다.
            bool hasWeapon = false;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].count > 0 && (slots[i].itemType == ItemType.Gun || slots[i].itemType == ItemType.Sword))
                {
                    hasWeapon = true;
                    break;
                }
            }

            if (!hasWeapon)
            {
                if (shooting != null) shooting.HideShooting();
                if (melee != null) melee.HideMelee();
            }
        }
    }
}
