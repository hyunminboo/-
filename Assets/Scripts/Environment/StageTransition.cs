using UnityEngine;

public class StageTransition : MonoBehaviour
{
    [Header("Transition Settings")]
    public Transform targetPosition;
    public string targetStageName = "Stage 3";
    
    [Header("Camera Settings")]
    public bool updateCameraBounds = true;
    public float newCameraMinX = 230f;
    public float newCameraMaxX = 500f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"[StageTransition] Player entered transition to {targetStageName}!");
            
            // Teleport the player
            if (targetPosition != null)
            {
                Vector3 spawnPos = targetPosition.position;
                
                if (targetStageName == "Stage 3" || targetPosition.gameObject.name == "Stage3StartPoint") 
                {
                    // 스테이지 3 진입 시 사용자가 지정한 이미지 위치(-5.6)보다 살짝 위로 스폰
                    spawnPos = new Vector3(198.4f, -5.1f, 0f);
                }
                else 
                {
                    // 실제 바닥을 찾아서 그 위로 스폰 위치 보정 (공중에서 떨어지는 현상 방지)
                    RaycastHit2D hit = Physics2D.Raycast(spawnPos, Vector2.down, 20f);
                    if (hit.collider != null && !hit.collider.isTrigger && hit.collider.gameObject.name != "Player")
                    {
                        spawnPos.y = hit.point.y + 0.5f; // 바닥보다 살짝 위
                    }
                }
                
                collision.transform.position = spawnPos;
            }
            else
            {
                // Fallback if target is not set
                collision.transform.position = transform.position + new Vector3(10f, 0, 0);
            }
            
            // Update camera if needed
            if (updateCameraBounds)
            {
                CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
                if (camFollow != null)
                {
                    camFollow.minX = newCameraMinX;
                    camFollow.maxX = newCameraMaxX;
                    camFollow.UnlockCamera();
                    camFollow.SnapToTarget(); // 플레이어와 함께 카메라도 즉시 이동
                }
            }
            
            // 스테이지 진입 알림 표시 (예: "Stage 3" -> "STAGE 3")
            if (!string.IsNullOrEmpty(targetStageName))
            {
                StageIndicator.Show(targetStageName.ToUpper());
            }
        }
    }
}
