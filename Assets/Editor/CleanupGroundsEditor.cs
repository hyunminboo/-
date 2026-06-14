using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class CleanupGroundsEditor : MonoBehaviour
{
    [MenuItem("Tools/Cleanup Grounds")]
    public static void Execute()
    {
        int deleted = 0;
        var transforms = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var t in transforms)
        {
            if (t != null && t.position.x > 110f)
            {
                if (t.name.StartsWith("Ground_") || t.name.StartsWith("Ruin") || t.name.StartsWith("Prop"))
                {
                    DestroyImmediate(t.gameObject);
                    deleted++;
                }
            }
        }
        
        Debug.Log($"Deleted {deleted} ground/prop/ruin objects past X=110.");
        
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }
}
