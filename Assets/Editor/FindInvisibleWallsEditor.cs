using UnityEngine;
using UnityEditor;

public class FindInvisibleWallsEditor : MonoBehaviour
{
    [MenuItem("Tools/Find Invisible Walls")]
    public static void FindWalls()
    {
        BoxCollider2D[] colliders = Object.FindObjectsByType<BoxCollider2D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var col in colliders)
        {
            SpriteRenderer sr = col.GetComponent<SpriteRenderer>();
            if (sr == null || sr.color.a == 0 || !sr.enabled)
            {
                Debug.Log($"Invisible Wall found: {col.gameObject.name} at X: {col.transform.position.x}");
            }
        }
    }
}
