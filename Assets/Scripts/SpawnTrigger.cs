using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SpawnTrigger : MonoBehaviour
{
    public EnemySpawner spawner;
    private bool isTriggered = false;

    void Start()
    {
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isTriggered) return;

        if (collision.gameObject.name == "Player")
        {
            isTriggered = true;
            Debug.Log($"[SpawnTrigger] {gameObject.name} 발동!");
            
            // 카메라 화면 잠금 (메탈슬러그 방식)
            CameraFollow camFollow = Camera.main.GetComponent<CameraFollow>();
            if (camFollow != null)
            {
                camFollow.LockCamera(Camera.main.transform.position.x);
            }

            if (spawner != null)
            {
                spawner.StartSpawn();
            }
            else
            {
                Debug.LogWarning("[SpawnTrigger] 연결된 EnemySpawner가 없습니다!");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(col.offset, col.size);
        }
    }
}
