using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPos;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.1f;

    public void Shake(float duration, float magnitude)
    {
        var camFollow = GetComponent<CameraFollow>();
        if (camFollow != null)
        {
            camFollow.Shake(duration, magnitude);
        }
    }
}
