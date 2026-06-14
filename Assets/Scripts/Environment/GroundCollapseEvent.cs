using UnityEngine;
using System.Collections;

public class GroundCollapseEvent : MonoBehaviour
{
    [Header("Collapse Settings")]
    public GameObject[] targetGrounds;
    public GameObject missilePrefab;
    public GameObject explosionPrefab;
    
    [Header("Fall Speed")]
    public float missileSpeed = 25f;

    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return;

        if (collision.gameObject.name == "Player")
        {
            isTriggered = true;
            StartCoroutine(CollapseRoutine(collision.transform));
        }
    }

    private IEnumerator CollapseRoutine(Transform playerTransform)
    {
        // 1. 트리거 지점 기준으로 낙하할 미사일 위치 결정 (트리거의 X 위치, 하늘 높이)
        Vector3 targetPos = transform.position;
        // 바닥 충돌 판정을 위해 Y=0 근처로 잡음
        targetPos.y = -1f; 

        Vector3 startPos = new Vector3(targetPos.x, 20f, 0f);

        GameObject missileObj = null;
        if (missilePrefab != null)
        {
            missileObj = Instantiate(missilePrefab, startPos, Quaternion.Euler(0, 0, -90f)); // 아래를 향하도록 회전
        }
        else
        {
            // 3D 캡슐을 쓰면 렌더파이프라인에 따라 하얗게 보일 수 있으므로 2D Sprite로 생성합니다.
            missileObj = new GameObject("FallingMissile");
            missileObj.transform.position = startPos;
            missileObj.transform.localScale = new Vector3(0.5f, 3f, 1f); // 얇고 길쭉하게
            
            SpriteRenderer sr = missileObj.AddComponent<SpriteRenderer>();
            
            // 코드로 1x1 픽셀짜리 하얀색 텍스처를 만들어 스프라이트로 활용
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            Sprite redSprite = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            
            sr.sprite = redSprite;
            sr.color = Color.red; // 명확한 붉은색
            sr.sortingOrder = 15; // 다른 배경보다 확실히 앞에 오도록 설정
        }

        // 미사일 낙하음
        AudioClip fallSound = Resources.Load<AudioClip>("Sounds/MissileFall");
        if (fallSound != null)
        {
            AudioSource.PlayClipAtPoint(fallSound, transform.position);
        }

        // 2. 미사일 하강
        while (missileObj != null && missileObj.transform.position.y > targetPos.y)
        {
            missileObj.transform.position += Vector3.down * missileSpeed * Time.deltaTime;
            yield return null;
        }

        // 3. 충돌 및 폭발
        if (missileObj != null) Destroy(missileObj);

        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, targetPos, Quaternion.identity);
            Destroy(explosion, 3f);
        }

        // 폭발음 재생
        AudioClip expSound = Resources.Load<AudioClip>("Sounds/Explosion");
        if (expSound != null)
        {
            AudioSource.PlayClipAtPoint(expSound, targetPos);
        }

        // 카메라 진동
        CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.Shake(1f, 0.5f);
        }

        // 4. 타겟 바닥들 붕괴 (사라짐)
        if (targetGrounds != null)
        {
            foreach (GameObject ground in targetGrounds)
            {
                if (ground != null)
                {
                    ground.SetActive(false);
                }
            }
        }

        // 5. 지하로 카메라 추적 전환
        if (camFollow != null)
        {
            // 추락하는 동안 부드럽게 따라가도록 즉시 풀어줍니다
            camFollow.EnableFreeFollow();
        }

        // 보이지 않는 데스존 등 추가 처리가 필요하다면 여기서 수행
        
        // 스테이지 2 진입 알림 표시
        StageIndicator.Show("STAGE 2");
    }
}
