using UnityEngine;
using UnityEditor;

public class FixEnemyFloatingEditor : EditorWindow
{
    [MenuItem("Tools/Fix Enemy Floating")]
    public static void FixEnemies()
    {
        string[] prefabPaths = {
            "Assets/Prefabs/Enemy_Scout.prefab",
            "Assets/Prefabs/Enemy_Rifleman.prefab",
            "Assets/Prefabs/Enemy_Heavy.prefab",
            "Assets/Prefabs/Enemy_Grenadier.prefab",
            "Assets/Prefabs/Enemy.prefab"
        };

        foreach (string path in prefabPaths)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null) continue;

            SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) continue;

            // Get the tight bounds of the sprite
            Bounds bounds = sr.sprite.bounds;

            BoxCollider2D box = prefab.GetComponent<BoxCollider2D>();
            if (box != null)
            {
                // Set the box collider exactly to the visual bounds
                box.size = bounds.size;
                box.offset = bounds.center;
                EditorUtility.SetDirty(prefab);
            }

            CapsuleCollider2D cap = prefab.GetComponent<CapsuleCollider2D>();
            if (cap != null)
            {
                cap.size = bounds.size;
                cap.offset = bounds.center;
                EditorUtility.SetDirty(prefab);
            }
            
            Debug.Log($"Fixed collider for {prefab.name}. Bounds size: {bounds.size}, offset: {bounds.center}");
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Fixed Enemy Colliders!");
    }
}
