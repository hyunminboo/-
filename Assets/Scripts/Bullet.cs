using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;
    public float damage = 35f;

    public GameObject hitEffectPrefab; // 충돌 시 터지는 파티클 프리팹

    void Start()
    {
        // 파티클 프리팹이 없다면 Resources 폴더에서 자동 로드 시도
        if (hitEffectPrefab == null)
        {
            hitEffectPrefab = Resources.Load<GameObject>("BulletHitEffect");
        }
        
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어나 아이템 습득 트리거 등은 무시
        if (collision.GetComponent<PlayerMovement>() != null || collision.CompareTag("Player")) return;
        
        // 적에게 닿았을 때
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage, transform.right); 
            SpawnHitEffect();
            Destroy(gameObject);
            return;
        }

        // 지형(그라운드 등)에 닿았을 때 (트리거가 아닌 단단한 콜라이더)
        if (!collision.isTrigger || collision.gameObject.name.ToLower().Contains("ground"))
        {
            SpawnHitEffect();
            Destroy(gameObject);
        }
    }

    private void SpawnHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            // 총알 위치에 파티클 생성
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
    }
}
