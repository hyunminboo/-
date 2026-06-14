using UnityEngine;
using System.Collections;

public class OutroCutscene : MonoBehaviour
{
    public float vehicleSpeed = 10f;
    
    public void StartOutro()
    {
        StartCoroutine(OutroRoutine());
    }

    private IEnumerator OutroRoutine()
    {
        // 1. 플레이어 찾기 (사격만 정지, 이동은 가능하게 함)
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            PlayerShooting ps = player.GetComponent<PlayerShooting>();
            if (ps != null) ps.enabled = false;
            // 이동은 가능해야 차량에 탈 수 있으므로 PlayerMovement는 끄지 않습니다.
        }

        // 2. 인트로 차량 재활용 (없으면 경고 후 바로 UI 출력)
        GameObject vehicle = GameObject.Find("IntroVehicle");
        
        // Find로 못 찾을 경우 비활성 오브젝트 검색
        if (vehicle == null)
        {
            foreach (GameObject go in Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[])
            {
                if (go.name == "IntroVehicle" && go.scene.isLoaded)
                {
                    vehicle = go;
                    break;
                }
            }
        }

        if (vehicle == null)
        {
            GameObject prefab = Resources.Load<GameObject>("Prefabs/OutroVehicle");
            if (prefab != null)
            {
                vehicle = Instantiate(prefab);
                vehicle.name = "OutroVehicle";
            }
            else
            {
                Debug.LogWarning("[OutroCutscene] 인트로 차량을 찾을 수 없어 즉시 클리어 UI를 띄웁니다.");
                yield return new WaitForSeconds(2f);
                GameClearUIManager.Instance.ShowGameClear();
                yield break;
            }
        }

        // 3. 차량 준비 (화면 우측 밖에서 스폰)
        vehicle.SetActive(true);
        foreach (ParticleSystem ps in vehicle.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Play();
        }
        AudioSource vehicleAudio = vehicle.GetComponent<AudioSource>();
        if (vehicleAudio != null) vehicleAudio.Play();

        float startX = Camera.main.transform.position.x + 20f;
        vehicle.transform.position = new Vector3(startX, 0f, 0f);
        
        // 방향 전환 (인트로에서는 우측을 향했으므로 좌측을 향하도록 x 스케일 반전)
        Vector3 vScale = vehicle.transform.localScale;
        vehicle.transform.localScale = new Vector3(-Mathf.Abs(vScale.x), vScale.y, vScale.z);

        // 높이를 맞추기 위한 레이캐스트
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(startX, 10f), Vector2.down, 30f);
        if (hit.collider != null)
        {
            float bottomOffset = 0f;
            BoxCollider2D bc = vehicle.GetComponent<BoxCollider2D>();
            if (bc != null) {
                bottomOffset = (bc.size.y / 2f) - bc.offset.y;
                bottomOffset *= Mathf.Abs(vehicle.transform.localScale.y);
            } else {
                SpriteRenderer vsr = vehicle.GetComponent<SpriteRenderer>();
                if (vsr != null) bottomOffset = vsr.bounds.extents.y;
            }
            vehicle.transform.position = new Vector3(startX, hit.point.y + bottomOffset, vehicle.transform.position.z);
        }

        // 4. 차량이 플레이어 우측 근처까지만 이동 후 정차
        float targetX = player != null ? player.transform.position.x + 8f : Camera.main.transform.position.x + 8f;
        while (vehicle.transform.position.x > targetX)
        {
            float nextX = vehicle.transform.position.x - (vehicleSpeed * Time.deltaTime);
            float nextY = vehicle.transform.position.y;
            
            RaycastHit2D moveHit = Physics2D.Raycast(new Vector2(nextX, 10f), Vector2.down, 30f);
            if (moveHit.collider != null)
            {
                float bottomOffset = 0f;
                BoxCollider2D bc = vehicle.GetComponent<BoxCollider2D>();
                if (bc != null) {
                    bottomOffset = (bc.size.y / 2f) - bc.offset.y;
                    bottomOffset *= Mathf.Abs(vehicle.transform.localScale.y);
                } else {
                    SpriteRenderer vsr = vehicle.GetComponent<SpriteRenderer>();
                    if (vsr != null) bottomOffset = vsr.bounds.extents.y;
                }
                nextY = moveHit.point.y + bottomOffset;
            }
            
            vehicle.transform.position = new Vector3(nextX, nextY, vehicle.transform.position.z);
            yield return null;
        }

        // 5. 플레이어가 다가와 탑승할 때까지 대기
        if (vehicleAudio != null) vehicleAudio.Pause();
        
        bool isBoarded = false;
        while (!isBoarded)
        {
            // 플레이어가 차량 근처(거리 3f 이내)로 오면 탑승으로 간주
            if (player != null && Vector2.Distance(player.transform.position, vehicle.transform.position) < 3.5f)
            {
                isBoarded = true;
            }
            yield return null;
        }

        // 플레이어 캐릭터 숨김 (탑승 연출)
        if (player != null)
        {
            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null) pm.enabled = false;
            player.SetActive(false);
        }
        
        yield return new WaitForSeconds(0.5f);

        // 6. 차량 우측으로 다시 방향 전환 후 출발
        if (vehicleAudio != null) vehicleAudio.Play();
        vehicle.transform.localScale = new Vector3(Mathf.Abs(vScale.x), vScale.y, vScale.z);
        
        float endX = Camera.main.transform.position.x + 25f;
        while (vehicle.transform.position.x < endX)
        {
            float nextX = vehicle.transform.position.x + (vehicleSpeed * Time.deltaTime);
            float nextY = vehicle.transform.position.y;
            
            RaycastHit2D moveHit = Physics2D.Raycast(new Vector2(nextX, 10f), Vector2.down, 30f);
            if (moveHit.collider != null)
            {
                float bottomOffset = 0f;
                BoxCollider2D bc = vehicle.GetComponent<BoxCollider2D>();
                if (bc != null) {
                    bottomOffset = (bc.size.y / 2f) - bc.offset.y;
                    bottomOffset *= Mathf.Abs(vehicle.transform.localScale.y);
                } else {
                    SpriteRenderer vsr = vehicle.GetComponent<SpriteRenderer>();
                    if (vsr != null) bottomOffset = vsr.bounds.extents.y;
                }
                nextY = moveHit.point.y + bottomOffset;
            }
            
            vehicle.transform.position = new Vector3(nextX, nextY, vehicle.transform.position.z);
            yield return null;
        }

        // 7. 화면 밖으로 퇴장 완료, 클리어 UI 팝업
        vehicle.SetActive(false);
        GameClearUIManager.Instance.ShowGameClear();
    }
}
