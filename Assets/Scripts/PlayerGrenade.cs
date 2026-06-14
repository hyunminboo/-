using UnityEngine;
using UnityEngine.UI; 

public class PlayerGrenade : MonoBehaviour
{
    [Header("Settings")]
    public int grenadeCount = 3; // 기본 지급 3개
    public GameObject playerGrenadePrefab; 
    public float throwForceY = 5f;
    
    void Start()
    {
        // 핫바 인스턴스가 존재하면 기본 수류탄 지급 (삭제 - 이제 맵에서 직접 주워야 함)
        // Invoke("GiveInitialGrenades", 0.5f);
    }

    void GiveInitialGrenades()
    {
        if (HotbarInventory.instance != null)
        {
            // HotbarInventory.instance.AddItem(ItemType.GrenadeAmmo, 3);
        }
    }
    
    void Update()
    {
        if (UnityEngine.InputSystem.Keyboard.current == null || UnityEngine.InputSystem.Mouse.current == null) return;
        
        bool throwPressed = UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame || 
                            UnityEngine.InputSystem.Keyboard.current.gKey.wasPressedThisFrame;
                            
        if (throwPressed)
        {
            if (HotbarInventory.instance != null && HotbarInventory.instance.selectedIndex != -1)
            {
                var slot = HotbarInventory.instance.slots[HotbarInventory.instance.selectedIndex];
                if (slot.itemType == ItemType.GrenadeAmmo && slot.count > 0)
                {
                    ThrowGrenade();
                    HotbarInventory.instance.ConsumeSelectedItem();
                }
            }
        }
    }
    
    void ThrowGrenade()
    {
        if (playerGrenadePrefab == null) return;
        
        Debug.Log("수류탄 투척!");
        
        Vector2 mouseScreenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));
        float dirSign = Mathf.Sign(mousePos.x - transform.position.x);
        
        Vector3 spawnPos = transform.position + new Vector3(dirSign * 1f, 1f, 0f);
        GameObject grenadeObj = Instantiate(playerGrenadePrefab, spawnPos, Quaternion.identity);
        
        Rigidbody2D grb = grenadeObj.GetComponent<Rigidbody2D>();
        if (grb != null)
        {
            // 거리 비례 힘 조절 (너무 세게 안 날아가도록)
            float dist = Mathf.Abs(mousePos.x - transform.position.x);
            float currentThrowForceX = Mathf.Clamp(dist, 5f, 15f); 
            grb.AddForce(new Vector2(dirSign * currentThrowForceX, throwForceY), ForceMode2D.Impulse);
        }
    }
    
    public void AddGrenade(int amount)
    {
        // 더이상 사용 안함 (Hotbar가 관리)
    }
}
