using UnityEngine;
using System.Collections;

public class NeonBlinker : MonoBehaviour
{
    private SpriteRenderer sr;
    public float minFlickerSpeed = 0.05f;
    public float maxFlickerSpeed = 0.3f;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            StartCoroutine(FlickerRoutine());
        }
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            // 가끔 깜빡거림 (찌지직)
            if (Random.value > 0.8f)
            {
                int flickers = Random.Range(2, 5);
                for (int i = 0; i < flickers; i++)
                {
                    sr.color = new Color(1f, 1f, 1f, Random.Range(0.2f, 0.5f));
                    yield return new WaitForSeconds(Random.Range(minFlickerSpeed, maxFlickerSpeed));
                    sr.color = new Color(1f, 1f, 1f, 1f);
                    yield return new WaitForSeconds(Random.Range(minFlickerSpeed, maxFlickerSpeed));
                }
            }
            
            // 정상 상태 유지
            yield return new WaitForSeconds(Random.Range(0.5f, 3f));
        }
    }
}
