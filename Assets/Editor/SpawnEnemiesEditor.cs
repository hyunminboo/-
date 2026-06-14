using UnityEditor;
using UnityEngine;

public class SpawnEnemiesEditor : MonoBehaviour {
    [MenuItem("Tools/Spawn Enemies")]
    public static void Spawn() {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
        if (prefab == null) {
            Debug.LogError("Enemy prefab not found!");
            return;
        }
        for (int i = 0; i < 15; i++) {
            float xPos = 5f + (i * 4f);
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.position = new Vector3(xPos, 2f, 0f);
        }
        Debug.Log("Spawned 15 enemies on the field.");
    }
}
