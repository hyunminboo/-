using UnityEditor;
using UnityEngine;

public class SpawnManyEnemiesEditor : MonoBehaviour {
    [MenuItem("Tools/Spawn Many Enemies")]
    public static void Spawn() {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemy.prefab");
        if (prefab == null) {
            Debug.LogError("Enemy prefab not found!");
            return;
        }
        for (int i = 0; i < 30; i++) {
            float xPos = 40f + (i * 6f); // 40 to 220
            GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.transform.position = new Vector3(xPos, 2f, 0f);
        }
        Debug.Log("Spawned 30 enemies on the field.");
    }
}
