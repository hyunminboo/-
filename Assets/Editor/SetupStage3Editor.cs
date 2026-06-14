using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class SetupStage3Editor : MonoBehaviour
{
    [MenuItem("Tools/Setup Stage 3 Elements")]
    public static void SetupStage3()
    {
        // 1. 스테이지 1 적들 복사 후 스테이지 3 (x + 155) 위치에 배치
        EnemyHealth[] allEnemies = Object.FindObjectsByType<EnemyHealth>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        List<GameObject> enemiesToDuplicate = new List<GameObject>();

        float shiftX = 155f; // 스테이지 1 적들을 스테이지 3로 옮길 x 오프셋 (40 -> 195)

        foreach (var eh in allEnemies)
        {
            float px = eh.transform.position.x;
            float py = eh.transform.position.y;
            
            // 스테이지 1 적 조건 (0 < x < 100, y > -5)
            if (px > 0 && px < 100 && py > -5f)
            {
                enemiesToDuplicate.Add(eh.gameObject);
            }
        }

        foreach (var obj in enemiesToDuplicate)
        {
            GameObject clone = Instantiate(obj);
            clone.transform.position = new Vector3(obj.transform.position.x + shiftX, obj.transform.position.y, obj.transform.position.z);
            clone.transform.SetParent(obj.transform.parent);
            clone.name = obj.name.Replace("(Clone)", "") + "_Stage3";
            Undo.RegisterCreatedObjectUndo(clone, "Duplicate Enemy for Stage 3");
        }
        
        Debug.Log($"스테이지 1에서 {enemiesToDuplicate.Count}명의 적을 복사하여 스테이지 3 구역에 배치했습니다.");

        // 2. WaveManager에 Boss 프리팹 할당
        WaveManager waveManager = Object.FindFirstObjectByType<WaveManager>();
        if (waveManager != null)
        {
            GameObject bossPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemies/Stage3_FinalBoss.prefab");
            if (bossPrefab != null)
            {
                // WaveManager 컴포넌트 강제 접근(직렬화 객체 사용)
                SerializedObject so = new SerializedObject(waveManager);
                SerializedProperty prop = so.FindProperty("stage3BossPrefab");
                if (prop != null)
                {
                    prop.objectReferenceValue = bossPrefab;
                    so.ApplyModifiedProperties();
                    Debug.Log("WaveManager에 Stage3_FinalBoss 프리팹이 성공적으로 할당되었습니다.");
                }
                else
                {
                    Debug.LogWarning("WaveManager에 stage3BossPrefab 필드가 아직 추가되지 않은 것 같습니다.");
                }
            }
            else
            {
                Debug.LogError("Assets/Prefabs/Enemies/Stage3_FinalBoss.prefab 경로에서 프리팹을 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("씬에 WaveManager가 존재하지 않습니다.");
        }

        // 씬 저장
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();
        Debug.Log("스테이지 3 설정 및 씬 저장이 완료되었습니다.");
    }
}
