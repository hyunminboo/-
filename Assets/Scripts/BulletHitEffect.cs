using UnityEngine;

public class BulletHitEffect : MonoBehaviour
{
    void Start()
    {
        // 파티클 시스템 재생이 끝나면 자동으로 삭제
        ParticleSystem ps = GetComponent<ParticleSystem>();
        if (ps != null)
        {
            Destroy(gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
        }
        else
        {
            Destroy(gameObject, 1f);
        }
    }
}
