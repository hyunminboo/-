using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class FixUIRemoval
{
    static FixUIRemoval()
    {
        EditorApplication.delayCall += () =>
        {
            GameObject canvas = GameObject.Find("MainMenuCanvas");
            if (canvas != null)
            {
                Object.DestroyImmediate(canvas);
                Debug.Log("✅ MainMenuCanvas 자동 삭제 완료!");
            }
            
            GameObject eventSys = GameObject.Find("EventSystem");
            if (eventSys != null && eventSys.GetComponent<UnityEngine.EventSystems.EventSystem>() != null)
            {
                // 게임 씬에 원래 EventSystem이 없었다고 가정하면 지웁니다.
                // 하지만 원래 있었다면 지우면 안되므로, 안전하게 둡니다.
            }
        };
    }
}
