using UnityEngine;
using System.Collections;

public class IntroCutscene : MonoBehaviour
{
    [Header("References")]
    public GameObject playerCharacter;
    public GameObject vehicleObj;
    public GameObject missileObj;
    public ParticleSystem explosionEffect;
    public GameObject weaponPickupPrefab; // 새로 추가: 무기 아이템 프리팹
    public WaveManager waveManager;

    [Header("Settings")]
    public Transform vehicleStartPoint;
    public Transform centerPoint;
    public float vehicleSpeed = 20f;
    public float missileSpeed = 30f;
    
    // 미사일 시작 위치
    private Vector3 missileStartPos;

    public void StartCutscene()
    {
        // 씬 시작 시 플레이어를 끄고 차량을 켭니다.
        if (playerCharacter != null) playerCharacter.SetActive(false);
        if (vehicleObj != null)
        {
            vehicleObj.SetActive(true);
            
            // 왼쪽 끝에서 시작
            vehicleObj.transform.position = new Vector3(-30f, 0f, 0f);
            
            // 바닥에 딱 붙여서 시작하도록 레이캐스트
            RaycastHit2D startHit = Physics2D.Raycast(new Vector2(-30f, 10f), Vector2.down, 30f);
            if (startHit.collider != null)
            {
                string objName = startHit.collider.gameObject.name.ToLower();
                if (objName.Contains("ground") || objName.Contains("platform") || objName.Contains("road") || objName.Contains("street"))
                {
                    float bottomOffset = 0f;
                    BoxCollider2D bc = vehicleObj.GetComponent<BoxCollider2D>();
                    if (bc != null) {
                        bottomOffset = (bc.size.y / 2f) - bc.offset.y;
                        bottomOffset *= vehicleObj.transform.localScale.y;
                    } else {
                        SpriteRenderer sr = vehicleObj.GetComponent<SpriteRenderer>();
                        if (sr != null) bottomOffset = sr.bounds.extents.y;
                    }
                    vehicleObj.transform.position = new Vector3(-30f, startHit.point.y + bottomOffset, 0f);
                }
            }
            
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null)
            {
                cam.lockBackwardMovement = false; 
                cam.minX = -35f; 
                cam.target = vehicleObj.transform; 
                Camera.main.transform.position = new Vector3(-30f, Camera.main.transform.position.y, Camera.main.transform.position.z);
            }

            // 엔진 사운드 추가
            AudioSource audioSource = vehicleObj.AddComponent<AudioSource>();
            AudioClip engineSound = Resources.Load<AudioClip>("Sounds/CarEngine");
            if (engineSound != null)
            {
                audioSource.clip = engineSound;
                audioSource.loop = true;
                audioSource.playOnAwake = true;
                audioSource.spatialBlend = 0f; // 2D 사운드로 설정하여 잘 들리게 함
                audioSource.volume = 0.8f;
                audioSource.Play();
            }
        }
        if (missileObj != null) missileObj.SetActive(false);

        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        float targetX = -20f; // 한 블럭 정도만 이동하고 멈출 목표 위치 (플레이어가 스폰될 안전 구역)

        if (vehicleObj != null)
        {
            bool missileHit = false;
            bool missileSpawned = false;

            while (!missileHit)
            {
                // 지형을 타고(바닥에 붙어서) 이동
                if (vehicleObj.transform.position.x < targetX)
                {
                    float nextX = vehicleObj.transform.position.x + (vehicleSpeed * Time.deltaTime);
                    float nextY = vehicleObj.transform.position.y;
                    
                    // 지형에 맞게 움직이도록 아래로 레이캐스트
                    RaycastHit2D hit = Physics2D.Raycast(new Vector2(nextX, 10f), Vector2.down, 30f);
                    if (hit.collider != null)
                    {
                        string objName = hit.collider.gameObject.name.ToLower();
                        if (objName.Contains("ground") || objName.Contains("platform") || objName.Contains("road") || objName.Contains("street"))
                        {
                            float bottomOffset = 0f;
                            BoxCollider2D bc = vehicleObj.GetComponent<BoxCollider2D>();
                            if (bc != null) {
                                bottomOffset = (bc.size.y / 2f) - bc.offset.y;
                                bottomOffset *= vehicleObj.transform.localScale.y;
                            } else {
                                SpriteRenderer vsr = vehicleObj.GetComponent<SpriteRenderer>();
                                if (vsr != null) bottomOffset = vsr.bounds.extents.y;
                            }
                            
                            nextY = hit.point.y + bottomOffset;
                        }
                    }
                    
                    vehicleObj.transform.position = new Vector3(nextX, nextY, vehicleObj.transform.position.z);
                }

                // 목표 위치에 가까워지면 미사일 스폰
                if (!missileSpawned && vehicleObj.transform.position.x > targetX - 3f)
                {
                    missileSpawned = true;
                    if (missileObj != null)
                    {
                        missileObj.SetActive(true);
                        // 수직으로 떨어지도록 차량 바로 위쪽(X 오프셋 0) 허공에 스폰
                        missileStartPos = vehicleObj.transform.position + new Vector3(0f, 15f, 0f);
                        missileObj.transform.position = missileStartPos;
                    }
                }

                // 미사일 추적 이동
                if (missileSpawned && missileObj != null)
                {
                    missileObj.transform.position = Vector3.MoveTowards(
                        missileObj.transform.position, 
                        vehicleObj.transform.position, 
                        missileSpeed * Time.deltaTime
                    );

                    // 미사일이 차량에 거의 닿으면 폭발! (루프 탈출)
                    if (Vector3.Distance(missileObj.transform.position, vehicleObj.transform.position) < 0.5f)
                    {
                        missileHit = true;
                    }
                }

                yield return null;
            }
        }

        // 3. 콰쾅! 폭발 및 차량 파괴
        Vector3 explosionPos = vehicleObj != null ? vehicleObj.transform.position : new Vector3(targetX, -1.5f, 0f);
        if (explosionEffect != null)
        {
            explosionEffect.transform.position = explosionPos;
            explosionEffect.Play();
        }

        // 폭발 사운드(CarCrash) 재생
        AudioClip crashSound = Resources.Load<AudioClip>("Sounds/CarCrash");
        if (crashSound != null)
        {
            GameObject sfxObj = new GameObject("CrashSFX");
            sfxObj.transform.position = explosionPos;
            AudioSource src = sfxObj.AddComponent<AudioSource>();
            src.clip = crashSound;
            src.spatialBlend = 0f; // 잘 들리게 2D
            src.volume = 1f;
            src.Play();
            Destroy(sfxObj, crashSound.length);
        }

        if (missileObj != null) Destroy(missileObj);
        
        // 엔딩 재사용을 위해 차량을 분리하고 삭제하지 않습니다.
        if (vehicleObj != null) 
        {
            vehicleObj.transform.SetParent(null); // 부모에서 분리하여 같이 파괴되지 않게 함
            vehicleObj.name = "IntroVehicle"; // 이름 지정
            vehicleObj.SetActive(false);
            // 자식 이펙트 끄기
            foreach(ParticleSystem ps in vehicleObj.GetComponentsInChildren<ParticleSystem>())
            {
                ps.Stop();
            }
        }

        // 먼지가 가라앉는 시간(연출)
        yield return new WaitForSeconds(0.3f);

        // 4. 플레이어 등장! (폭발한 차량 위치에서 등장)
        if (playerCharacter != null)
        {
            playerCharacter.transform.position = explosionPos;
            playerCharacter.SetActive(true);
            
            // 스테이지 1 UI 팝업 연출 추가
            StageIndicator.Show("STAGE 1");
            
            // 카메라 타겟을 다시 플레이어로 원상복구
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null)
            {
                cam.target = playerCharacter.transform;
                cam.lockBackwardMovement = true; 
            }
        }

        // 5. 플레이어 옆에 진짜 총 아이템 투하
        yield return new WaitForSeconds(1f);
        if (weaponPickupPrefab != null)
        {
            Vector3 dropPos = explosionPos + new Vector3(1.5f, 0f, 0f);
            
            // 땅에 닿게 보정
            RaycastHit2D dropHit = Physics2D.Raycast(new Vector2(dropPos.x, 10f), Vector2.down, 30f);
            if (dropHit.collider != null && dropHit.collider.gameObject.name.Contains("Ground"))
            {
                dropPos.y = dropHit.point.y + 0.5f;
            }
            
            GameObject pickup = Instantiate(weaponPickupPrefab, dropPos, Quaternion.identity);
            
            WeaponPickup wp = pickup.GetComponent<WeaponPickup>();
            if (wp != null) {
                wp.waveManager = this.waveManager;
                wp.weaponType = WeaponPickup.WeaponType.Gun; // 강제로 총으로 설정
            }
            
            Debug.Log("무기 아이템 투하 완료! 주워야 전투가 시작됩니다.");
        }
        else
        {
            if (waveManager != null) waveManager.StartWave1();
        }
        
        Destroy(gameObject);
    }
}
