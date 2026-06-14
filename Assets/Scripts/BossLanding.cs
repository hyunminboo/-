using UnityEngine;
using System.Collections;

public class BossLanding : MonoBehaviour
{
    private bool hasLanded = false;
    private EnemyAI ai;

    private Collider2D myCol;

    void Start()
    {
        ai = GetComponent<EnemyAI>();
        myCol = GetComponent<Collider2D>();
        
        if (ai != null)
        {
            // 떨어지는 동안에는 AI가 작동하지 않게 끕니다.
            ai.enabled = false;
        }

        // 떨어지는 동안 플레이어를 짓눌러 바닥을 뚫지 않도록 충돌을 무시합니다.
        GameObject player = GameObject.Find("Player");
        if (player != null && myCol != null)
        {
            foreach (var pc in player.GetComponentsInChildren<Collider2D>())
            {
                Physics2D.IgnoreCollision(myCol, pc, true);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasLanded) return;
        
        // 바닥이나 다른 오브젝트에 닿으면 착지 판정
        hasLanded = true;
        
        // 1. 카메라 흔들림 (0.8초 동안 강하게)
        CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
        if (cam != null)
        {
            cam.Shake(0.8f, 1.0f);
        }

        // 2. 바닥 착지 파티클 (먼지 폭발 효과)
        GameObject tempExp = new GameObject("BossLandingDust");
        tempExp.transform.position = transform.position + new Vector3(0, -2f, 0); // 발 밑 위치
        tempExp.transform.localScale = new Vector3(5f, 5f, 1f);
        SpriteRenderer sr = tempExp.AddComponent<SpriteRenderer>();
        
        // 임시 텍스처 (하얀색 먼지구름 느낌)
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, new Color(0.8f, 0.8f, 0.8f, 0.7f));
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
        sr.sortingOrder = 15;
        
        // 커지면서 투명해지는 스크립트 붙이기
        tempExp.AddComponent<MissileExplosionEffect>();

        // 3. 쿵 소리 재생
        AudioClip thumpSound = Resources.Load<AudioClip>("Sounds/Explosion"); // 임시로 폭발음 사용
        if (thumpSound != null) AudioSource.PlayClipAtPoint(thumpSound, transform.position, 1.0f);

        // 4. 짧은 대기 후 AI 가동
        StartCoroutine(ActivateAIAfterDelay());
    }

    private IEnumerator ActivateAIAfterDelay()
    {
        // 착지 후 1초 동안 폼 잡기 (멈춤)
        yield return new WaitForSeconds(1.0f);
        if (ai != null)
        {
            ai.enabled = true;
            Debug.Log("[BossLanding] 보스 AI 가동 시작!");
        }

        // 플레이어와의 충돌 무시 해제
        GameObject player = GameObject.Find("Player");
        if (player != null && myCol != null)
        {
            foreach (var pc in player.GetComponentsInChildren<Collider2D>())
            {
                Physics2D.IgnoreCollision(myCol, pc, false);
            }
        }
        
        // 할 일을 다 한 랜딩 스크립트는 삭제
        Destroy(this);
    }
}
