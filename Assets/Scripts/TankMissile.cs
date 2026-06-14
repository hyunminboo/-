using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class TankMissile : MonoBehaviour
{
    public float speed = 12f;
    public float damage = 20f;
    public float lifeTime = 5f;
    
    public GameObject explosionPrefab;
    
    private Rigidbody2D rb;
    private bool isExploded = false;

    public void Initialize(Vector2 direction)
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * speed;
        
        if (GameManager.Instance != null)
        {
            damage *= GameManager.Instance.GetDifficultyMultiplier();
        }
        
        // 방향에 맞게 회전
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // n초 후 자동 소멸
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isExploded) return;
        
        // 탱크 자신이나 다른 적, 투사체, 트리거 무시
        if (collision.gameObject.GetComponent<EnemyHealth>() != null) return;
        if (collision.isTrigger && collision.gameObject.name != "Player") return;
        if (collision.gameObject.name.Contains("Laser") || collision.gameObject.name.Contains("Zone")) return;

        isExploded = true;
        
        if (collision.gameObject.name == "Player")
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage);
            
            Rigidbody2D prb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (prb != null)
            {
                // 뒤로만 살짝 넉백, 위로는 넉백 안함
                float dirSign = Mathf.Sign(rb.linearVelocity.x);
                prb.linearVelocity = new Vector2(0f, prb.linearVelocity.y);
                prb.AddForce(new Vector2(dirSign * 5f, 0f), ForceMode2D.Impulse);
            }
        }

        Explode();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isExploded) return;
        if (collision.gameObject.GetComponent<EnemyHealth>() != null) return;

        isExploded = true;
        
        if (collision.gameObject.name == "Player")
        {
            PlayerHealth ph = collision.gameObject.GetComponent<PlayerHealth>();
            if (ph != null) ph.TakeDamage(damage);
            
            Rigidbody2D prb = collision.gameObject.GetComponent<Rigidbody2D>();
            if (prb != null)
            {
                float dirSign = Mathf.Sign(rb.linearVelocity.x);
                prb.linearVelocity = new Vector2(0f, prb.linearVelocity.y);
                prb.AddForce(new Vector2(dirSign * 5f, 0f), ForceMode2D.Impulse);
            }
        }

        Explode();
    }

    private void Explode()
    {
        // 폭발 이펙트 (GroundCollapseEvent 폭발 이펙트 재사용하거나 빨간색 파티클 등)
        if (explosionPrefab != null)
        {
            GameObject exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(exp, 2f);
        }
        else
        {
            // 임시 폭발 이펙트
            GameObject tempExp = new GameObject("MissileExplosion");
            tempExp.transform.position = transform.position;
            tempExp.transform.localScale = new Vector3(3f, 3f, 1f);
            SpriteRenderer sr = tempExp.AddComponent<SpriteRenderer>();
            
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.red);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            sr.sortingOrder = 15;
            
            // 간단하게 크기가 줄어들면서 투명해지는 애니메이션 컴포넌트
            tempExp.AddComponent<MissileExplosionEffect>();
        }

        // 폭발 사운드
        AudioClip expSound = Resources.Load<AudioClip>("Sounds/Explosion");
        if (expSound != null)
        {
            AudioSource.PlayClipAtPoint(expSound, transform.position, 0.7f);
        }

        Destroy(gameObject);
    }
}

public class MissileExplosionEffect : MonoBehaviour
{
    private SpriteRenderer sr;
    private float life = 0.5f;
    void Start() { sr = GetComponent<SpriteRenderer>(); Destroy(gameObject, life); }
    void Update()
    {
        if (sr != null)
        {
            transform.localScale += Vector3.one * Time.deltaTime * 5f;
            Color c = sr.color;
            c.a -= Time.deltaTime * 2f;
            sr.color = c;
        }
    }
}
