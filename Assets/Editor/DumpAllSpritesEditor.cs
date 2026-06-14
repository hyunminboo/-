using UnityEditor;
using UnityEngine;
using System.IO;

public class DumpAllSpritesEditor : MonoBehaviour {
    [MenuItem("Tools/Dump All Sprites")]
    public static void Dump() {
        SpriteRenderer[] srs = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
        string result = "";
        foreach (var sr in srs) {
            string spriteName = sr.sprite != null ? sr.sprite.name : "null";
            float rightEdge = sr.bounds.max.x;
            result += $"Object: {sr.gameObject.name}, Sprite: {spriteName}, Right Edge X: {rightEdge}\n";
        }
        File.WriteAllText("all_sprites.txt", result);
        Debug.Log("Wrote all sprites to file.");
    }
}
