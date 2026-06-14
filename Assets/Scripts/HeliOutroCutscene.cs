using UnityEngine;
using System.Collections;

public class HeliOutroCutscene : MonoBehaviour
{
    public float descendSpeed = 3f;
    public float ascendSpeed = 4f;
    public float forwardSpeed = 8f;

    public void StartOutro()
    {
        StartCoroutine(OutroRoutine());
    }

    private IEnumerator OutroRoutine()
    {
        // 1. 플레이어 찾기 및 제어 중지 (이동은 가능하게 유지, 사격만 금지)
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerShooting ps = player.GetComponent<PlayerShooting>();
            if (ps != null) ps.enabled = false;
        }

        // 2. 헬리콥터 생성
        GameObject heliPrefab = Resources.Load<GameObject>("Prefabs/OutroHelicopter");
        if (heliPrefab == null)
        {
            Debug.LogWarning("[HeliOutroCutscene] 헬기 프리팹을 찾을 수 없어 즉시 클리어 UI를 띄웁니다.");
            yield return new WaitForSeconds(2f);
            if (GameClearUIManager.Instance != null) GameClearUIManager.Instance.ShowGameClear();
            yield break;
        }

        GameObject heli = Instantiate(heliPrefab);
        
        // 헬기 하강 목표 X 좌표 (사용자가 지정한 위치: 326.7)
        float targetX = 326.7f;
        // 카메라 높이보다 위쪽에서 스폰
        float spawnY = Camera.main != null ? Camera.main.transform.position.y + 15f : 10f; 
        
        heli.transform.position = new Vector3(targetX, spawnY, 0f);
        
        // 스케일 및 방향 조절 (이미지가 너무 클 수 있으므로 0.5로 축소)
        heli.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

        // 사운드 재생
        AudioSource audio = heli.GetComponent<AudioSource>();
        if (audio != null) audio.Play();

        // 3. 하강 연출 (목표 Y = -4.5 정도, 바닥(-6.26)보다 약간 위)
        float targetY = -4.5f;
        
        while (heli.transform.position.y > targetY)
        {
            heli.transform.position += Vector3.down * descendSpeed * Time.deltaTime;
            yield return null;
        }
        
        heli.transform.position = new Vector3(targetX, targetY, 0f);

        // 4. 대기 및 플레이어 탑승 대기 (플레이어가 헬기에 다가올 때까지)
        bool isBoarded = false;
        while (!isBoarded)
        {
            if (player != null && Vector2.Distance(player.transform.position, heli.transform.position) < 3.5f)
            {
                isBoarded = true;
            }
            yield return null;
        }

        // 5. 탑승 연출
        if (player != null)
        {
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = false;

            Rigidbody2D prb = player.GetComponent<Rigidbody2D>();
            if (prb != null) prb.linearVelocity = Vector2.zero;

            // 플레이어를 숨김
            player.SetActive(false);
            
            AudioClip getInClip = Resources.Load<AudioClip>("Sounds/ItemPickup");
            if (getInClip != null)
            {
                AudioSource.PlayClipAtPoint(getInClip, heli.transform.position);
            }
        }

        yield return new WaitForSeconds(1f);

        // 6. 상승 연출 (다시 위로 올라감)
        float ascendTargetY = Camera.main.transform.position.y + 8f;
        while (heli.transform.position.y < ascendTargetY)
        {
            heli.transform.position += Vector3.up * ascendSpeed * Time.deltaTime;
            yield return null;
        }

        // 7. 우측으로 비행 연출 (화면 밖으로)
        float flyTargetX = Camera.main.transform.position.x + 25f;
        
        while (heli.transform.position.x < flyTargetX)
        {
            heli.transform.position += Vector3.right * forwardSpeed * Time.deltaTime;
            yield return null;
        }

        // 7. 종료 처리
        Destroy(heli);
        
        if (GameClearUIManager.Instance != null)
        {
            GameClearUIManager.Instance.ShowGameClear();
        }
    }
}
