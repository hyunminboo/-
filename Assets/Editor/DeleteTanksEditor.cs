using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class DeleteTanksEditor : MonoBehaviour
{
    [MenuItem("Tools/Delete Tanks")]
    public static void DeleteTanks()
    {
        EnemyAI[] enemies = Object.FindObjectsByType<EnemyAI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<GameObject> tanksToDelete = new List<GameObject>();

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy.gameObject.name.Contains("Tank") || enemy.role == EnemyRole.MissileTank)
            {
                tanksToDelete.Add(enemy.gameObject);
            }
        }

        int count = tanksToDelete.Count;
        foreach (GameObject tank in tanksToDelete)
        {
            Undo.DestroyObjectImmediate(tank);
        }

        if (count > 0)
        {
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveOpenScenes();
            Debug.Log($"성공적으로 {count}개의 탱크 오브젝트를 씬에서 삭제했습니다.");
        }
        else
        {
            Debug.Log("삭제할 탱크가 씬에 존재하지 않습니다.");
        }
    }
}
