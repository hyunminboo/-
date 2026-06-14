using UnityEngine;

public class SafeGround : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerRespawn respawn = collision.gameObject.GetComponent<PlayerRespawn>();
            if (respawn != null)
            {
                // 약간 위쪽에 리스폰되도록 오프셋 추가
                Vector3 safePos = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
                respawn.UpdateSafePosition(safePos);
            }
        }
    }
}
