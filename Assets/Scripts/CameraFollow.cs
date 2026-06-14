using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 2f, -10f);

    [Header("Boundaries")]
    public float minX = -10f; // 맵 왼쪽 끝
    public float maxX = 100f; // 맵 오른쪽 끝 (진행에 따라 늘어나거나 잠김)
    
    // 카메라는 보통 뒤로 돌아가지 못하게 하는 옵션(메탈슬러그 방식)
    public bool lockBackwardMovement = true; 
    
    [Header("Y Axis Follow")]
    public bool followY = false;
    
    [Header("Shake")]
    public float shakeDuration = 0f;
    public float shakeMagnitude = 0f;
    
    private float initialY;
    private Vector3 logicalPosition;

    void Start()
    {
        initialY = transform.position.y;
        logicalPosition = transform.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 목표 위치 계산
        Vector3 targetPosition = target.position + offset;

        // X축 스크롤 진행
        float desiredX = targetPosition.x;

        // 뒤로 돌아가지 못하게 막기
        if (lockBackwardMovement && desiredX < logicalPosition.x)
        {
            desiredX = logicalPosition.x;
        }

        // 바운더리 제한
        desiredX = Mathf.Clamp(desiredX, minX, maxX);

        // 지하 맵(스테이지 2)에 직접 배치하여 테스트할 때를 위해, 
        // 플레이어가 일정 깊이 이하로 내려가면 자동으로 Y축 추적을 활성화합니다.
        if (!followY && target.position.y < -10f)
        {
            EnableFreeFollow();
        }

        // Y축 고정 여부
        float desiredY = followY ? targetPosition.y : initialY;
        
        Vector3 clampedTargetPos = new Vector3(desiredX, desiredY, targetPosition.z);

        // 부드럽게 이동 (논리적 위치만 업데이트)
        logicalPosition = Vector3.Lerp(logicalPosition, clampedTargetPos, smoothSpeed * Time.deltaTime);

        // 실제 카메라 위치 적용 (흔들림 포함)
        if (shakeDuration > 0)
        {
            Vector3 shakeOffset = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0) * shakeMagnitude;
            transform.position = logicalPosition + shakeOffset;
            shakeDuration -= Time.deltaTime;
        }
        else
        {
            transform.position = logicalPosition;
        }
    }
    
    public void Shake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }

    // 웨이브 발생 시 화면을 가두는 함수
    public void LockCamera(float currentMaxX)
    {
        maxX = currentMaxX;
    }

    // 웨이브 클리어 시 화면 제한을 풀어주는 함수
    public void UnlockCamera(float newMaxX = 1000f)
    {
        maxX = newMaxX;
    }

    // Y축 추적 및 뒤로가기 제한 해제 (지하 맵 추락 시 사용)
    public void EnableFreeFollow()
    {
        followY = true;
        lockBackwardMovement = false;
        maxX = 99999f; // 지하 맵에서는 카메라가 더 멀리 갈 수 있도록 허용
    }

    // 대규모 공간 이동(텔레포트) 시 카메라를 즉시 이동시키는 함수
    public void SnapToTarget()
    {
        if (target != null)
        {
            Vector3 targetPosition = target.position + offset;
            float desiredX = Mathf.Clamp(targetPosition.x, minX, maxX);
            float desiredY = followY ? targetPosition.y : initialY;
            logicalPosition = new Vector3(desiredX, desiredY, targetPosition.z);
            transform.position = logicalPosition;
        }
    }
}
