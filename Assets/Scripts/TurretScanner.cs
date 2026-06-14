using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TurretScanner : MonoBehaviour
{
    [Header("FOV Settings")]
    public float viewRadius = 15f;
    [Range(0, 360)]
    public float viewAngle = 45f;
    public LayerMask obstacleMask;
    
    [Header("Sweep Settings")]
    public float sweepSpeed = 45f; // Degrees per second
    public float sweepAngleLimit = 90f; // Sweeps from -limit to +limit
    public float baseDirectionAngle = 270f; // 270 = straight down. So it sweeps lower hemisphere.

    [Header("Visuals")]
    public Material fovMaterial;
    public Color fovColor = new Color(1f, 0f, 0f, 0.3f); // 반투명 빨간색

    private Mesh mesh;
    private float currentSweepAngle;
    private int sweepDirection = 1;
    
    private bool isWarning = false;
    
    private bool playerDetected = false;
    private Transform playerTransform;

    void Start()
    {
        mesh = new Mesh();
        mesh.name = "FOV_Mesh";
        GetComponent<MeshFilter>().mesh = mesh;
        
        var renderer = GetComponent<MeshRenderer>();
        if (fovMaterial != null) 
        {
            renderer.material = fovMaterial;
        }
        else
        {
            // 동적으로 매터리얼 생성
            Material mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = fovColor;
            renderer.material = mat;
        }
        renderer.sortingOrder = 15;
        
        GameObject player = GameObject.Find("Player");
        if (player != null) playerTransform = player.transform;
        
        currentSweepAngle = -sweepAngleLimit;
        
        // 지형 충돌 마스크 (Default 레이어 등에 Ground가 있다고 가정)
        obstacleMask = LayerMask.GetMask("Default", "Ground", "Environment");
        // 마스크가 0이면 모든 것을 감지하므로 그냥 놔두거나 적절히 세팅해야 함
        if (obstacleMask == 0) obstacleMask = Physics2D.DefaultRaycastLayers;
    }

    void Update()
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.Find("Player");
            if (player != null) playerTransform = player.transform;
        }

        if (!isWarning)
        {
            // 왕복 스캔
            currentSweepAngle += sweepSpeed * sweepDirection * Time.deltaTime;
            if (currentSweepAngle >= sweepAngleLimit)
            {
                currentSweepAngle = sweepAngleLimit;
                sweepDirection = -1;
            }
            else if (currentSweepAngle <= -sweepAngleLimit)
            {
                currentSweepAngle = -sweepAngleLimit;
                sweepDirection = 1;
            }
        }

        DrawFieldOfView();
        CheckPlayerDetection();
        UpdateSpriteFacing();
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle);
        float stepAngleSize = viewAngle / stepCount;
        
        float startAngle = baseDirectionAngle + currentSweepAngle - viewAngle / 2f;
        
        Vector3[] vertices = new Vector3[stepCount + 2];
        int[] triangles = new int[stepCount * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = startAngle + stepAngleSize * i;
            Vector3 dir = DirFromAngle(angle, false);
            
            // 벽에 닿으면 레이더가 거기서 멈추도록 레이캐스트
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, viewRadius, obstacleMask);
            bool hitWall = false;
            foreach (var h in hits)
            {
                if (h.collider != null && h.collider.transform.root != transform.root && !h.collider.CompareTag("Player") && !h.collider.CompareTag("Enemy"))
                {
                    vertices[i + 1] = transform.InverseTransformPoint(h.point);
                    hitWall = true;
                    break;
                }
            }
            if (!hitWall)
            {
                vertices[i + 1] = transform.InverseTransformPoint(transform.position + dir * viewRadius);
            }

            if (i < stepCount)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    void CheckPlayerDetection()
    {
        playerDetected = false;
        if (playerTransform == null) return;

        Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
        float currentFacingAngle = baseDirectionAngle + currentSweepAngle;
        
        if (Vector3.Angle(DirFromAngle(currentFacingAngle, false), dirToPlayer) < viewAngle / 2f)
        {
            float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distToPlayer <= viewRadius)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dirToPlayer, distToPlayer, obstacleMask);
                bool blockedByWall = false;
                foreach (var h in hits)
                {
                    if (h.collider != null && h.collider.transform.root != transform.root && !h.collider.CompareTag("Player") && !h.collider.CompareTag("Enemy"))
                    {
                        blockedByWall = true;
                        break;
                    }
                }
                
                if (!blockedByWall)
                {
                    playerDetected = true;
                }
            }
        }
    }
    
    void UpdateSpriteFacing()
    {
        // 레이더가 향하는 x 방향에 맞춰 스프라이트 좌우 반전
        float currentFacingAngle = baseDirectionAngle + currentSweepAngle;
        Vector3 dir = DirFromAngle(currentFacingAngle, false);
        var sr = transform.parent != null ? transform.parent.GetComponent<SpriteRenderer>() : GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            // 원본 이미지가 오른쪽을 본다고 가정
            if (dir.x < 0) sr.flipX = true;
            else sr.flipX = false;
        }
    }

    public bool IsPlayerDetected()
    {
        return playerDetected;
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.z;
        }
        return new Vector3(Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0);
    }
    
    public void SetWarningMode(bool warning)
    {
        isWarning = warning;
        var mr = GetComponent<MeshRenderer>();
        if (mr != null && mr.material != null)
        {
            if (warning)
            {
                mr.material.color = new Color(1f, 1f, 0f, 0.6f); // 노란색 경고
            }
            else
            {
                mr.material.color = fovColor;
            }
        }
    }
}
