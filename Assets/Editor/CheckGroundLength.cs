using UnityEngine;
using UnityEditor;

public class CheckGroundLength : MonoBehaviour
{
    [MenuItem("Tools/Check Ground Layout")]
    public static void CheckLayout()
    {
        var sprites = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var s in sprites)
        {
            if (s.transform.position.x > 110f && s.transform.position.x < 150f)
            {
                Debug.Log($"Sprite over 110: {s.name} at {s.transform.position.x}");
            }
        }
    }
}
