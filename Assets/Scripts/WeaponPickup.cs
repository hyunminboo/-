using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public WaveManager waveManager;

    private bool isPickedUp = false;
    private float spawnTime;

    private void Awake()
    {
        spawnTime = Time.time;
    }

    private void Start()
    {

        // 플레이어와 부딪혀서 투명벽처럼 막히는 현상 방지
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Collider2D[] playerCols = player.GetComponents<Collider2D>();
            Collider2D[] myCols = GetComponents<Collider2D>();
            foreach(var mc in myCols)
            {
                if (!mc.isTrigger)
                {
                    foreach(var pc in playerCols)
                    {
                        Physics2D.IgnoreCollision(mc, pc);
                    }
                }
            }
        }

        if (waveManager == null)
            waveManager = UnityEngine.Object.FindObjectOfType<WaveManager>();

        // 아이템이 공중에 뜨거나 땅을 뚫는 현상 방지: 바닥으로 스냅
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 20f);
        if (hit.collider != null && !hit.collider.isTrigger)
        {
            transform.position = new Vector3(transform.position.x, hit.point.y + 0.5f, transform.position.z);
        }
    }

    public enum WeaponType { Gun, Sword }
    public WeaponType weaponType = WeaponType.Gun; // 기본값은 Gun (현재 인트로 컷신 드랍)

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isPickedUp) return;
        if (Time.time < spawnTime + 1.5f) return; // 드롭 후 1.5초간 다시 줍지 못함
        
        if (collision.gameObject.name == "Player")
        {
            if (HotbarInventory.instance != null)
            {
                // 이미 같은 종류의 무기를 가지고 있다면 줍지 않음
                ItemType targetType = (weaponType == WeaponType.Sword) ? ItemType.Sword : ItemType.Gun;
                for (int i = 0; i < HotbarInventory.instance.slots.Length; i++)
                {
                    if (HotbarInventory.instance.slots[i].count > 0 && HotbarInventory.instance.slots[i].itemType == targetType)
                    {
                        return; // 이미 가지고 있음
                    }
                }

                // 인벤토리에 추가 (성공 시에만 주움)
                bool added = HotbarInventory.instance.AddItem(targetType, 1);
                if (!added) return; 
            }

            isPickedUp = true;

            // 2. 웨이브 시작
            if (waveManager != null)
            {
                waveManager.StartWave1();
            }
            else
            {
                Debug.LogWarning("WeaponPickup: WaveManager not found!");
            }

            // 3. 습득 이펙트
            Debug.Log("==== 🗡️ 검/총 습득! 전투 개시! ====");
            
            AudioClip pickupSound = Resources.Load<AudioClip>("Sounds/ItemPickup");
            if (pickupSound != null)
            {
                GameObject sfxObj = new GameObject("PickupSFX");
                sfxObj.transform.position = transform.position;
                AudioSource src = sfxObj.AddComponent<AudioSource>();
                src.clip = pickupSound;
                src.spatialBlend = 0f;
                src.volume = 0.8f;
                src.Play();
                Destroy(sfxObj, pickupSound.length);
            }
            
            // 4. 아이템 삭제
            Destroy(gameObject);
        }
    }
}
