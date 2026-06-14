using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StageIndicator : MonoBehaviour
{
    public static void Show(string stageName)
    {
        GameObject obj = new GameObject("StageIndicator");
        StageIndicator indicator = obj.AddComponent<StageIndicator>();
        indicator.StartCoroutine(indicator.ShowRoutine(stageName));
    }

    private IEnumerator ShowRoutine(string stageName)
    {
        // 1. 캔버스 생성
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        // 2. 텍스트 오브젝트 생성
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(transform, false);

        Text text = textObj.AddComponent<Text>();
        text.text = stageName;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf") ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 120;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = new Color(1f, 0.8f, 0.2f, 0f); // 약간 황금색
        text.fontStyle = FontStyle.BoldAndItalic;

        // 약간의 그림자/외곽선 효과
        Outline outline = textObj.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(4f, -4f);

        RectTransform rt = textObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0, 150f); // 화면 중앙에서 약간 위
        rt.sizeDelta = new Vector2(1000f, 300f);

        // 3. 애니메이션 연출 (페이드 인 & 스케일업)
        float duration = 1.0f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Easing (Out Cubic)
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            
            text.color = new Color(text.color.r, text.color.g, text.color.b, easeT);
            rt.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one * 1.0f, easeT);
            yield return null;
        }

        // 4. 유지 시간
        yield return new WaitForSeconds(2.0f);

        // 5. 페이드 아웃
        elapsed = 0f;
        duration = 0.5f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            text.color = new Color(text.color.r, text.color.g, text.color.b, 1f - t);
            rt.localScale = Vector3.Lerp(Vector3.one * 1.0f, Vector3.one * 1.2f, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
