using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryManager : MonoBehaviour
{
    public GameObject inventoryUI;
    public GameObject hudCanvas;
    private bool isInventoryOpen = false;

    void Start()
    {
        if (hudCanvas == null)
        {
            hudCanvas = GameObject.Find("HUDCanvas");
        }
    }

    void Update()
    {
        // Tab 키를 누르면 인벤토리 토글
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        isInventoryOpen = !isInventoryOpen;
        
        if (inventoryUI != null)
        {
            inventoryUI.SetActive(isInventoryOpen);
            
            // 인벤토리가 열릴 때마다 UI 정보(아이템, 스탯 등) 갱신
            if (isInventoryOpen)
            {
                InventoryUIManager uiMgr = GetComponent<InventoryUIManager>();
                if (uiMgr != null)
                {
                    uiMgr.UpdateUI();
                }
            }
        }

        if (hudCanvas != null)
        {
            hudCanvas.SetActive(!isInventoryOpen);
        }

        // 인벤토리가 열리면 게임 시간을 멈춤 (0), 닫히면 원래대로 (1)
        Time.timeScale = isInventoryOpen ? 0f : 1f;
    }
}
