using UnityEditor;
using UnityEngine;

public class FindBackgroundEditor : MonoBehaviour {
    [MenuItem("Tools/Find Background")]
    public static void FindBg() {
        SpriteRenderer[] srs = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        foreach (var sr in srs) {
            if (sr.gameObject.name.ToLower().Contains("bg") || sr.gameObject.name.ToLower().Contains("background") || sr.sortingOrder < -5) {
                float rightEdge = sr.bounds.max.x;
                Debug.Log($"Background Object: {sr.gameObject.name}, Right Edge X: {rightEdge}, Width: {sr.bounds.size.x}, Pos: {sr.transform.position.x}");
            }
        }
    }
}
