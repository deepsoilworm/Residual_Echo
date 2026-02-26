#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.AI.Navigation;
using ResidualEcho.Player;
using ResidualEcho.Creature;

namespace ResidualEcho.Editor
{
    /// <summary>
    /// 에디터 유틸리티: 씬 셋업, NavMesh 베이크
    /// </summary>
    public static class ResidualEchoEditorTools
    {
        [MenuItem("ResidualEcho/Bake NavMesh")]
        public static void BakeNavMesh()
        {
            var surface = Object.FindAnyObjectByType<NavMeshSurface>();
            if (surface != null)
            {
                surface.BuildNavMesh();
                EditorUtility.SetDirty(surface);
                Debug.Log("[ResidualEcho] NavMesh baked!");
            }
            else
            {
                Debug.LogWarning("[ResidualEcho] NavMeshSurface not found!");
            }
        }

        [MenuItem("ResidualEcho/Wire Scene References")]
        public static void WireSceneReferences()
        {
            // --- PlayerSettings SO ---
            var playerSettings = AssetDatabase.LoadAssetAtPath<ResidualEcho.Player.PlayerSettings>(
                "Assets/ScriptableObjects/Player/PlayerSettings.asset");
            var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/InputSystem_Actions.inputactions");

            // --- CreatureSettings SO ---
            var creatureSettings = AssetDatabase.LoadAssetAtPath<CreatureSettings>(
                "Assets/ScriptableObjects/Creature/CreatureSettings.asset");

            // --- Player ---
            var playerGO = GameObject.Find("Player");
            if (playerGO == null) { Debug.LogError("Player not found!"); return; }

            var cameraHolder = playerGO.transform.Find("CameraHolder");
            var mainCamera = cameraHolder?.Find("Main Camera");
            var flashlightGO = cameraHolder?.Find("Flashlight");

            // PlayerController
            var pc = playerGO.GetComponent<PlayerController>();
            if (pc != null)
            {
                var so = new SerializedObject(pc);
                so.FindProperty("settings").objectReferenceValue = playerSettings;
                so.FindProperty("cameraHolder").objectReferenceValue = cameraHolder;
                so.ApplyModifiedProperties();
            }

            // PlayerInteraction
            var pi = playerGO.GetComponent<PlayerInteraction>();
            if (pi != null)
            {
                var so = new SerializedObject(pi);
                so.FindProperty("settings").objectReferenceValue = playerSettings;
                so.FindProperty("cameraTransform").objectReferenceValue = mainCamera;
                so.ApplyModifiedProperties();
            }

            // PlayerFlashlight
            var pf = playerGO.GetComponent<PlayerFlashlight>();
            if (pf != null && flashlightGO != null)
            {
                var so = new SerializedObject(pf);
                so.FindProperty("settings").objectReferenceValue = playerSettings;
                so.FindProperty("spotLight").objectReferenceValue = flashlightGO.GetComponent<Light>();
                so.ApplyModifiedProperties();
            }

            // PlayerInput
            var playerInput = playerGO.GetComponent<PlayerInput>();
            if (playerInput != null && inputActions != null)
            {
                var so = new SerializedObject(playerInput);
                so.FindProperty("m_Actions").objectReferenceValue = inputActions;
                so.FindProperty("m_DefaultActionMap").stringValue = "Player";
                so.ApplyModifiedProperties();
            }

            // --- Creature ---
            var creatureGO = GameObject.Find("Creature");
            if (creatureGO == null) { Debug.LogError("Creature not found!"); return; }

            var creatureMesh = creatureGO.transform.Find("CreatureMesh");

            // CreatureDetection
            var cd = creatureGO.GetComponent<CreatureDetection>();
            if (cd != null)
            {
                var so = new SerializedObject(cd);
                so.FindProperty("settings").objectReferenceValue = creatureSettings;
                so.ApplyModifiedProperties();
            }

            // CreatureStateMachine
            var csm = creatureGO.GetComponent<CreatureStateMachine>();
            if (csm != null && creatureMesh != null)
            {
                var so = new SerializedObject(csm);
                so.FindProperty("settings").objectReferenceValue = creatureSettings;
                so.FindProperty("creatureRenderer").objectReferenceValue = creatureMesh.GetComponentInChildren<Renderer>();
                so.ApplyModifiedProperties();
            }

            // ResidualEcho.Creature.CreatureModelAnchor (애니메이션 루트 이동 고정)
            if (creatureMesh != null && creatureMesh.GetComponent<ResidualEcho.Creature.CreatureModelAnchor>() == null)
            {
                creatureMesh.gameObject.AddComponent<ResidualEcho.Creature.CreatureModelAnchor>();
                EditorUtility.SetDirty(creatureMesh.gameObject);
            }

            Debug.Log("[ResidualEcho] All scene references wired successfully!");
        }

        [MenuItem("ResidualEcho/Full Setup (Wire + Bake)")]
        public static void FullSetup()
        {
            WireSceneReferences();
            BakeNavMesh();
        }

        [MenuItem("ResidualEcho/Set Creature Animations Loop")]
        public static void SetCreatureAnimationsLoop()
        {
            string[] fbxPaths =
            {
                "Assets/Models/Creature/Meshy_AI_Animation_Axe_Breathe_and_Look_Around_withSkin.fbx",
                "Assets/Models/Creature/Meshy_AI_Animation_Walking_withSkin.fbx",
                "Assets/Models/Creature/Meshy_AI_Animation_Lean_Forward_Sprint_inplace_withSkin.fbx",
            };

            foreach (var path in fbxPaths)
            {
                var importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer == null) { Debug.LogWarning($"[ResidualEcho] Importer not found: {path}"); continue; }

                // 루트 본 위치/회전 이동 데이터 제거 (제자리 애니메이션)
                importer.motionNodeName = "<Root Transform>";

                var clips = importer.clipAnimations;
                if (clips.Length == 0)
                    clips = importer.defaultClipAnimations;

                for (int i = 0; i < clips.Length; i++)
                {
                    clips[i].loopTime = true;
                    clips[i].lockRootRotation = true;
                    clips[i].lockRootHeightY = true;
                    clips[i].lockRootPositionXZ = true;
                    clips[i].keepOriginalPositionXZ = false;
                    clips[i].keepOriginalPositionY = false;
                    clips[i].keepOriginalOrientation = false;
                }

                importer.clipAnimations = clips;
                importer.SaveAndReimport();
                Debug.Log($"[ResidualEcho] Loop + root motion bake: {path}");
            }

            Debug.Log("[ResidualEcho] All creature animations fixed!");
        }

        [MenuItem("ResidualEcho/Fix Creature Animator")]
        public static void FixCreatureAnimator()
        {
            var creatureGO = GameObject.Find("Creature");
            if (creatureGO == null) { Debug.LogError("Creature not found!"); return; }

            var creatureMesh = creatureGO.transform.Find("CreatureMesh");
            if (creatureMesh == null) { Debug.LogError("CreatureMesh not found!"); return; }

            // Creature 루트의 Animator 제거
            var rootAnimator = creatureGO.GetComponent<Animator>();
            if (rootAnimator != null)
            {
                Object.DestroyImmediate(rootAnimator);
                Debug.Log("[ResidualEcho] Removed Animator from Creature root");
            }

            // CreatureMesh에 Animator + Controller 연결
            var meshAnimator = creatureMesh.GetComponent<Animator>();
            if (meshAnimator == null)
                meshAnimator = creatureMesh.gameObject.AddComponent<Animator>();

            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(
                "Assets/Animations/Creature/CreatureAnimator.controller");

            var so = new SerializedObject(meshAnimator);
            so.FindProperty("m_Controller").objectReferenceValue = controller;
            so.FindProperty("m_ApplyRootMotion").boolValue = false;
            so.ApplyModifiedProperties();

            EditorUtility.SetDirty(creatureMesh.gameObject);
            Debug.Log("[ResidualEcho] Animator moved to CreatureMesh with controller assigned!");
        }

        [MenuItem("ResidualEcho/Assign Creature Animation Clips")]
        public static void AssignCreatureAnimationClips()
        {
            string controllerPath = "Assets/Animations/Creature/CreatureAnimator.controller";
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                Debug.LogError("[ResidualEcho] CreatureAnimator.controller not found!");
                return;
            }

            // FBX 경로 → 상태 이름 매핑
            var clipMap = new System.Collections.Generic.Dictionary<string, string>
            {
                { "Assets/Models/Creature/Meshy_AI_Animation_Axe_Breathe_and_Look_Around_withSkin.fbx", "Idle" },
                { "Assets/Models/Creature/Meshy_AI_Animation_Walking_withSkin.fbx", "Walk" },
                { "Assets/Models/Creature/Meshy_AI_Animation_Lean_Forward_Sprint_inplace_withSkin.fbx", "Run" },
            };

            var rootStateMachine = controller.layers[0].stateMachine;

            foreach (var kvp in clipMap)
            {
                // FBX에서 AnimationClip 추출
                var assets = AssetDatabase.LoadAllAssetsAtPath(kvp.Key);
                AnimationClip clip = null;
                foreach (var asset in assets)
                {
                    if (asset is AnimationClip c && !c.name.StartsWith("__preview__"))
                    {
                        clip = c;
                        break;
                    }
                }

                if (clip == null)
                {
                    Debug.LogWarning($"[ResidualEcho] No AnimationClip found in {kvp.Key}");
                    continue;
                }

                // 해당 이름의 상태 찾아서 클립 연결
                foreach (var state in rootStateMachine.states)
                {
                    if (state.state.name == kvp.Value)
                    {
                        state.state.motion = clip;
                        Debug.Log($"[ResidualEcho] {kvp.Value} ← {clip.name}");
                        break;
                    }
                }
            }

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log("[ResidualEcho] All animation clips assigned!");
        }

        [MenuItem("ResidualEcho/Create Creature Animator")]
        public static void CreateCreatureAnimator()
        {
            // 폴더 확인
            if (!AssetDatabase.IsValidFolder("Assets/Animations"))
                AssetDatabase.CreateFolder("Assets", "Animations");
            if (!AssetDatabase.IsValidFolder("Assets/Animations/Creature"))
                AssetDatabase.CreateFolder("Assets/Animations", "Creature");

            string path = "Assets/Animations/Creature/CreatureAnimator.controller";

            // Animator Controller 생성
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            // 파라미터 추가
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsChasing", AnimatorControllerParameterType.Bool);

            // 기본 레이어
            var rootStateMachine = controller.layers[0].stateMachine;

            // 상태 생성 (클립은 비워두고 나중에 연결)
            var idleState = rootStateMachine.AddState("Idle", new Vector3(300, 0, 0));
            var walkState = rootStateMachine.AddState("Walk", new Vector3(300, 100, 0));
            var runState = rootStateMachine.AddState("Run", new Vector3(300, 200, 0));

            // 기본 상태 = Idle
            rootStateMachine.defaultState = idleState;

            // Idle → Walk (Speed > 0.1)
            var idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToWalk.hasExitTime = false;
            idleToWalk.duration = 0.15f;

            // Walk → Idle (Speed < 0.1)
            var walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            walkToIdle.hasExitTime = false;
            walkToIdle.duration = 0.15f;

            // Walk → Run (IsChasing = true)
            var walkToRun = walkState.AddTransition(runState);
            walkToRun.AddCondition(AnimatorConditionMode.If, 0, "IsChasing");
            walkToRun.hasExitTime = false;
            walkToRun.duration = 0.1f;

            // Run → Walk (IsChasing = false)
            var runToWalk = runState.AddTransition(walkState);
            runToWalk.AddCondition(AnimatorConditionMode.IfNot, 0, "IsChasing");
            runToWalk.hasExitTime = false;
            runToWalk.duration = 0.15f;

            // Idle → Run (Speed > 0.1 && IsChasing)
            var idleToRun = idleState.AddTransition(runState);
            idleToRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            idleToRun.AddCondition(AnimatorConditionMode.If, 0, "IsChasing");
            idleToRun.hasExitTime = false;
            idleToRun.duration = 0.1f;

            // Run → Idle (Speed < 0.1)
            var runToIdle = runState.AddTransition(idleState);
            runToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            runToIdle.hasExitTime = false;
            runToIdle.duration = 0.15f;

            AssetDatabase.SaveAssets();
            Debug.Log($"[ResidualEcho] Creature Animator Controller created at {path}");

            // 씬의 Creature에 Animator 연결
            var creatureGO = GameObject.Find("Creature");
            if (creatureGO != null)
            {
                var animator = creatureGO.GetComponent<Animator>();
                if (animator == null)
                    animator = creatureGO.AddComponent<Animator>();

                var so = new SerializedObject(animator);
                so.FindProperty("m_Controller").objectReferenceValue = controller;
                so.ApplyModifiedProperties();

                EditorUtility.SetDirty(creatureGO);
                Debug.Log("[ResidualEcho] Animator assigned to Creature!");
            }
        }
    }
}
#endif
