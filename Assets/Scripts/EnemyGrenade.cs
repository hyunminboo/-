using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyGrenade : MonoBehaviour
{
    public float explosionRadius = 3f;
    public float damage = 20f;
    public float explosionDelay = 2f;
    
    public GameObject explosionParticlePrefab;
    
    private bool hasExploded = false;

    void Start()
    {
        Invoke("Explode", explosionDelay);
    }

    void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        AudioClip explosionSound = Resources.Load<AudioClip>("Sounds/Explosion");
        if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, 1.0f);
        }

        // 폭발 파티클
        if (explosionParticlePrefab != null)
        {
            GameObject fx = Instantiate(explosionParticlePrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1f);
        }
        else
        {
            // 파티클이 없으면 시각적 피드백을 위해 임시 원 생성
            GameObject tempFx = new GameObject("Explosion");
            tempFx.transform.position = transform.position;
            SpriteRenderer sr = tempFx.AddComponent<SpriteRenderer>();
            // 기본 원형 스프라이트 생성 또는 아무거나..
            tempFx.transform.localScale = new Vector3(explosionRadius, explosionRadius, 1);
            Destroy(tempFx, 0.2f);
        }

        // 플레이어 데미지 판정
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player") || hit.name == "Player")
            {
                PlayerHealth ph = hit.GetComponent<PlayerHealth>();
                if (ph != null)
                {
                    ph.TakeDamage(damage);
                    Debug.Log("[Grenadier] 수류탄 폭발! 플레이어 피격");
                }
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
