using UnityEngine;

public class DeathZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth ph = collision.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(9999f); // 즉사 데미지
                Debug.Log("플레이어 낙사!");
            }
        }
        else if (collision.CompareTag("Enemy"))
        {
            EnemyHealth eh = collision.GetComponent<EnemyHealth>();
            if (eh != null)
            {
                eh.TakeDamage(9999f, Vector2.zero);
            }
            else
            {
                Destroy(collision.gameObject);
            }
        }
    }
}
