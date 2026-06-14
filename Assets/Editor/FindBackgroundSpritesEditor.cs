using UnityEditor;
using UnityEngine;

public class FindBackgroundSpritesEditor : MonoBehaviour {
    [MenuItem("Tools/Find Background Sprites")]
    public static void FindSprites() {
        SpriteRenderer[] srs = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        foreach (var sr in srs) {
            if (sr.gameObject.name.ToLower().Contains("bg") || sr.gameObject.name.ToLower().Contains("background") || sr.gameObject.name.ToLower().Contains("floor") || sr.gameObject.name.ToLower().Contains("ground")) {
                string spriteName = sr.sprite != null ? sr.sprite.name : "null";
                float rightEdge = sr.bounds.max.x;
                Debug.Log($"Object: {sr.gameObject.name}, Sprite: {spriteName}, Right Edge X: {rightEdge}, Width: {sr.bounds.size.x}, Pos: {sr.transform.position}");
            }
        }
    }
}
