using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;
using System.Collections.Generic;

[InitializeOnLoad]
public class AutoSetupAnimation : AssetPostprocessor
{
    static string spritePath = "Assets/Sprites/PlayerAnim/walk_sheet.png";

    static AutoSetupAnimation()
    {
        EditorApplication.delayCall += CheckAndSetup;
    }

    static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
    {
        foreach (string asset in imported)
        {
            if (asset.Replace("\\", "/") == spritePath)
            {
                EditorApplication.delayCall += CheckAndSetup;
                break;
            }
        }
    }

    static void CheckAndSetup()
    {
        if (File.Exists(spritePath))
        {
            DoSetup();
        }
    }

    static void DoSetup()
    {
        // 1. 스프라이트 시트 자르기 (3프레임 기준)
        TextureImporter importer = AssetImporter.GetAtPath(spritePath) as TextureImporter;
        if (importer != null && importer.spriteImportMode != SpriteImportMode.Multiple)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.filterMode = FilterMode.Bilinear;

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePath);
            if (tex != null)
            {
                int cols = 3;
                int frameWidth = tex.width / cols;
                int frameHeight = tex.height;

                SpriteMetaData[] metaData = new SpriteMetaData[cols];
                for (int i = 0; i < cols; i++)
                {
                    metaData[i] = new SpriteMetaData
                    {
                        name = "walk_frame_" + i,
                        rect = new Rect(i * frameWidth, 0, frameWidth, frameHeight),
                        alignment = 0,
                        pivot = new Vector2(0.5f, 0.5f)
                    };
                }
                importer.spritesheet = metaData;
                importer.SaveAndReimport();
            }
        }

        // 2. 잘라낸 스프라이트 로드
        Object[] assets = AssetDatabase.LoadAllAssetsAtPath(spritePath);
        List<Sprite> walkFrames = new List<Sprite>();
        foreach (Object obj in assets)
        {
            if (obj is Sprite) walkFrames.Add(obj as Sprite);
        }

        if (walkFrames.Count < 3) return;

        // 3. 애니메이션 클립 생성
        string idlePath = "Assets/Animations/Idle.anim";
        string walkPath = "Assets/Animations/Walk.anim";
        
        AnimationClip idleClip = new AnimationClip { frameRate = 12 };
        EditorCurveBinding spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };
        ObjectReferenceKeyframe[] idleKeys = new ObjectReferenceKeyframe[1];
        idleKeys[0] = new ObjectReferenceKeyframe { time = 0, value = walkFrames[0] };
        AnimationUtility.SetObjectReferenceCurve(idleClip, spriteBinding, idleKeys);
        
        AnimationClip walkClip = new AnimationClip { frameRate = 6 };
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(walkClip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(walkClip, settings);
        
        ObjectReferenceKeyframe[] walkKeys = new ObjectReferenceKeyframe[4];
        walkKeys[0] = new ObjectReferenceKeyframe { time = 0f, value = walkFrames[0] };
        walkKeys[1] = new ObjectReferenceKeyframe { time = 1f/6f, value = walkFrames[1] };
        walkKeys[2] = new ObjectReferenceKeyframe { time = 2f/6f, value = walkFrames[2] };
        walkKeys[3] = new ObjectReferenceKeyframe { time = 3f/6f, value = walkFrames[0] };
        AnimationUtility.SetObjectReferenceCurve(walkClip, spriteBinding, walkKeys);

        AssetDatabase.CreateAsset(idleClip, idlePath);
        AssetDatabase.CreateAsset(walkClip, walkPath);

        // 4. 애니메이터 컨트롤러 세팅
        string controllerPath = "Assets/Animations/PlayerController.controller";
        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        controller.AddParameter("isWalking", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
        AnimatorState idleState = rootStateMachine.AddState("Idle");
        idleState.motion = idleClip;
        AnimatorState walkState = rootStateMachine.AddState("Walk");
        walkState.motion = walkClip;

        rootStateMachine.defaultState = idleState;

        AnimatorStateTransition toWalk = idleState.AddTransition(walkState);
        toWalk.AddCondition(AnimatorConditionMode.If, 0, "isWalking");
        toWalk.duration = 0;

        AnimatorStateTransition toIdle = walkState.AddTransition(idleState);
        toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "isWalking");
        toIdle.duration = 0;

        // 5. 플레이어에 부착
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            Animator animator = player.GetComponent<Animator>();
            if (animator == null) animator = player.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            
            Selection.activeGameObject = player;
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(player.scene);
            SceneView.RepaintAll();
            Debug.Log("걷기 애니메이션 세팅 완료!");
        }

        string scriptPath = "Assets/Editor/AutoSetupAnimation.cs";
        if (File.Exists(scriptPath))
        {
            AssetDatabase.DeleteAsset(scriptPath);
        }
    }
}
