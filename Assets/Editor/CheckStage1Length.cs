using UnityEngine;
using UnityEditor;

public class CheckStage1Length : MonoBehaviour
{
    [MenuItem("Tools/Check Stage 1 Layout")]
    public static void CheckLayout()
    {
        var enemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        int count = 0;
        foreach (var e in enemies)
        {
            Debug.Log($"Enemy at: {e.transform.position.x}");
            count++;
        }
        Debug.Log($"Total Enemies: {count}");

        var stage2 = GameObject.Find("Stage2_Trigger");
        if (stage2) Debug.Log($"Stage2_Trigger: {stage2.transform.position.x}");

        var wave2 = GameObject.Find("Wave2_Trigger");
        if (wave2) Debug.Log($"Wave2_Trigger: {wave2.transform.position.x}");

        var invisWall = GameObject.Find("InvisibleWall_Right");
        if (invisWall) Debug.Log($"InvisibleWall_Right: {invisWall.transform.position.x}");
        
        var backgrounds = Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var bg in backgrounds)
        {
            if (bg.name.StartsWith("Background"))
            {
                Debug.Log($"{bg.name} at {bg.transform.position.x}");
            }
        }
    }
}
