using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class ShortenStage1 : MonoBehaviour
{
    [MenuItem("Tools/Shorten Stage 1")]
    public static void Execute()
    {
        int deletedEnemies = 0;
        var enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var e in enemies)
        {
            if (e.transform.position.x > 85f)
            {
                DestroyImmediate(e.gameObject);
                deletedEnemies++;
            }
        }
        
        var invisWall = GameObject.Find("InvisibleWall_Right");
        if (invisWall) 
        {
            invisWall.transform.position = new Vector3(90f, invisWall.transform.position.y, invisWall.transform.position.z);
        }

        var wave2 = GameObject.Find("Wave2_Trigger");
        if (wave2) 
        {
            wave2.transform.position = new Vector3(40f, wave2.transform.position.y, wave2.transform.position.z);
        }

        var stage2 = GameObject.Find("Stage2_Trigger");
        if (stage2) 
        {
            stage2.transform.position = new Vector3(95f, stage2.transform.position.y, stage2.transform.position.z);
        }

        // Delete unnecessary backgrounds to match the new length (optional but keeps it clean)
        var backgrounds = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int deletedBg = 0;
        foreach (var bg in backgrounds)
        {
            if (bg.name.StartsWith("Background_") && bg.transform.position.x > 110f)
            {
                DestroyImmediate(bg.gameObject);
                deletedBg++;
            }
        }

        Debug.Log($"Stage 1 Shortened! Deleted {deletedEnemies} enemies and {deletedBg} backgrounds.");
        
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
    }
}
