using UnityEditor;
using UnityEngine;

public class CleanupEnemiesEditor : MonoBehaviour {
    [MenuItem("Tools/Cleanup Enemies")]
    public static void Cleanup() {
        // Find all objects with name starting with "Enemy(Clone)" or "Enemy" but not the prefab
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int count = 0;
        foreach (GameObject obj in allObjects) {
            if (obj.scene.name == null) continue; // Skip prefabs
            if (obj.name.StartsWith("Enemy") && !obj.name.Contains("Spawner")) {
                DestroyImmediate(obj);
                count++;
            }
        }
        Debug.Log($"Cleaned up {count} enemies from the scene.");
    }
}
