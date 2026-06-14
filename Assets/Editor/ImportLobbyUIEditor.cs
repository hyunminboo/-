using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

public class ImportLobbyUIEditor : EditorWindow
{
    [MenuItem("Tools/Import Lobby To Current Scene")]
    public static void ImportLobby()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        
        // 로비 씬을 추가로 엽니다.
        string lobbyScenePath = "Assets/Scenes/LobbyScene.unity";
        Scene lobbyScene = EditorSceneManager.OpenScene(lobbyScenePath, OpenSceneMode.Additive);
        
        if (!lobbyScene.IsValid())
        {
            Debug.LogError("LobbyScene.unity 를 찾을 수 없습니다.");
            return;
        }

        // 로비 씬의 모든 최상위 오브젝트를 찾습니다.
        GameObject[] rootObjects = lobbyScene.GetRootGameObjects();
        GameObject lobbyCanvas = null;
        
        foreach (var obj in rootObjects)
        {
            // LobbyManager가 붙어있는 캔버스나 오브젝트를 찾습니다.
            if (obj.GetComponentInChildren<LobbyManager>(true) != null)
            {
                lobbyCanvas = obj;
                break;
            }
        }

        if (lobbyCanvas != null)
        {
            // 찾은 로비 캔버스를 현재 씬으로 이동시킵니다.
            SceneManager.MoveGameObjectToScene(lobbyCanvas, currentScene);
            Debug.Log("로비 UI를 현재 씬으로 성공적으로 가져왔습니다!");
            Selection.activeGameObject = lobbyCanvas;
        }
        else
        {
            Debug.LogError("로비 씬에서 LobbyManager가 포함된 UI를 찾지 못했습니다.");
        }

        // 로비 씬을 다시 닫습니다.
        EditorSceneManager.CloseScene(lobbyScene, true);
        
        // 현재 씬의 변경사항을 저장하도록 마크합니다.
        EditorSceneManager.MarkSceneDirty(currentScene);
    }
}
