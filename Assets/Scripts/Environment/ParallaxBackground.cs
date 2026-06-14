using UnityEngine;

/// <summary>
/// 메탈 하트비트: 패럴랙스 배경 스크롤
/// 카메라의 이동에 따라 배경이 다른 속도로 움직여 원근감을 주는 스크립트입니다.
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    [Tooltip("카메라 이동에 대한 배경의 이동 비율 (0: 카메라와 고정, 1: 일반 오브젝트와 동일)")]
    public Vector2 parallaxEffectMultiplier;

    private Transform cameraTransform;
    private Vector3 lastCameraPosition;
    private float textureUnitSizeX;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        lastCameraPosition = cameraTransform.position;

        // 배경 스프라이트의 가로 길이 계산 (무한 스크롤을 위해)
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            Sprite sprite = sr.sprite;
            Texture2D texture = sprite.texture;
            textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
        }
        else
        {
            textureUnitSizeX = 0f;
        }
    }

    private void LateUpdate()
    {
        // 카메라의 이동량 계산
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // 패럴랙스 효과 적용
        transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier.x, deltaMovement.y * parallaxEffectMultiplier.y, 0);
        lastCameraPosition = cameraTransform.position;

        // 무한 스크롤 처리 (스프라이트가 할당되어 기준 길이가 0보다 클 때만)
        if (textureUnitSizeX > 0f && Mathf.Abs(cameraTransform.position.x - transform.position.x) >= textureUnitSizeX)
        {
            float offsetPositionX = (cameraTransform.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(cameraTransform.position.x + offsetPositionX, transform.position.y, transform.position.z);
        }
    }
}
