using UnityEngine;
using UnityEngine.UI;

public class HotbarManager : MonoBehaviour
{
    public PlayerShooting playerShooting;
    public GameObject weaponIcon;

    void Start()
    {
        if (playerShooting == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null) playerShooting = player.GetComponent<PlayerShooting>();
        }
    }

    void Update()
    {
        if (playerShooting != null && weaponIcon != null)
        {
            // 무기를 획득했으면 핫바에 아이콘을 보여줌
            weaponIcon.SetActive(playerShooting.canShoot);
        }
    }
}
