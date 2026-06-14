using UnityEditor;
using UnityEngine;
using System.IO;

public class FindBgBoundsEditor : MonoBehaviour {
    [MenuItem("Tools/Find Bg Bounds")]
    public static void FindBounds() {
        SpriteRenderer[] srs = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        string result = "";
        foreach (var sr in srs) {
            if (sr.gameObject.name.ToLower().Contains("background") && !sr.gameObject.name.ToLower().Contains("ground")) {
                string spriteName = sr.sprite != null ? sr.sprite.name : "null";
                float rightEdge = sr.bounds.max.x;
                result += $"Object: {sr.gameObject.name}, Sprite: {spriteName}, Right Edge X: {rightEdge}\n";
            }
        }
        File.WriteAllText("bg_bounds.txt", result);
        Debug.Log("Wrote bg bounds to file.");
    }
}
