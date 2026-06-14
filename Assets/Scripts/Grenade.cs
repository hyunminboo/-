using UnityEngine;

public class Grenade : MonoBehaviour
{
    public float explosionRadius = 3f;
    public float explosionDamage = 50f;
    public GameObject explosionEffectPrefab;
    
    private bool hasExploded = false;

    void Start()
    {
        // 3초 뒤 자동 폭발 (땅에 닿지 않았을 경우 방어 코드)
        Invoke("Explode", 3f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;
        
        // 적이나 바닥에 닿으면 즉시 폭발
        if (collision.gameObject.CompareTag("Enemy") || collision.gameObject.name.StartsWith("Ground") || collision.gameObject.name.StartsWith("Platform"))
        {
            Explode();
        }
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

        // 카메라 흔들림 호출 (효과 대폭 축소)
        CameraShake shake = Camera.main.GetComponent<CameraShake>();
        if (shake != null) shake.Shake(0.15f, 0.15f);

        // 폭발 이펙트 생성
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1.0f); // 1초 뒤 삭제
        }

        // 폭발 범위 내의 모든 적에게 데미지 (광역 공격)
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hitEnemies)
        {
            EnemyHealth health = hit.GetComponent<EnemyHealth>();
            if (health != null)
            {
                Vector2 hitDir = (hit.transform.position - transform.position).normalized;
                health.TakeDamage(explosionDamage, hitDir);
            }
        }

        Debug.Log("💣 수류탄 폭발!");
        Destroy(gameObject);
    }

    // 인스펙터 창에서 폭발 범위 확인용
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
