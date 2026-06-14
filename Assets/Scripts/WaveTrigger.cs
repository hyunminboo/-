using UnityEngine;

public class WaveTrigger : MonoBehaviour
{
    public int waveToTrigger = 2; // 1차는 인트로에서 하므로 기본 2차
    public WaveManager waveManager;
    
    private bool isTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return;

        if (collision.gameObject.name == "Player")
        {
            isTriggered = true;
            Debug.Log("==== 구역 진입! 화면 잠금 및 웨이브 " + waveToTrigger + " 시작! ====");

            // 기존에 떠있던 GO 표시 숨기기
            if (GoIndicator.instance != null)
            {
                GoIndicator.instance.HideGo();
            }

            // 1. 카메라 화면 잠금 (오른쪽으로 더 이상 못 가게 막음)
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                // 현재 카메라의 X 위치로 최대치를 고정
                camFollow.LockCamera(Camera.main.transform.position.x);
            }

            // 2. 웨이브 매니저에게 수송기 투하 요청
            if (waveManager == null)
            {
                waveManager = GameObject.Find("WaveManager").GetComponent<WaveManager>();
            }

            if (waveManager != null)
            {
                if (waveToTrigger == 2) waveManager.StartWave2();
                // 추후 3이나 보스가 추가되면 여기서 분기 처리
            }

            // 트리거 역할이 끝났으므로 삭제
            Destroy(gameObject);
        }
    }
}
