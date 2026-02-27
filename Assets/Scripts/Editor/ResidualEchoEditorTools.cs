#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Unity.AI.Navigation;
using TMPro;
using ResidualEcho.Player;
using ResidualEcho.Creature;
using ResidualEcho.Core;
using ResidualEcho.Common.Events;
using ResidualEcho.Common.Constants;
using ResidualEcho.UI;

namespace ResidualEcho.Editor
{
    /// <summary>
    /// 에디터 유틸리티: 씬 셋업, NavMesh 베이크
    /// </summary>
    public static class ResidualEchoEditorTools
    {
        [MenuItem("ResidualEcho/Register Tags and Layers")]
        public static void RegisterTagsAndLayers()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));

            // --- Tags ---
            var tagsProp = tagManager.FindProperty("tags");
            string[] requiredTags = { GameTags.CREATURE, GameTags.INTERACTABLE };

            foreach (var tag in requiredTags)
            {
                bool found = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                    tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
                    Debug.Log($"[ResidualEcho] Tag registered: {tag}");
                }
            }

            // --- Layers ---
            var layersProp = tagManager.FindProperty("layers");
            var layerMap = new System.Collections.Generic.Dictionary<int, string>
            {
                { GameLayers.PLAYER, "Player" },
                { GameLayers.CREATURE, "Creature" },
                { GameLayers.INTERACTABLE, "Interactable" },
            };

            foreach (var kvp in layerMap)
            {
                var layerProp = layersProp.GetArrayElementAtIndex(kvp.Key);
                if (string.IsNullOrEmpty(layerProp.stringValue))
                {
                    layerProp.stringValue = kvp.Value;
                    Debug.Log($"[ResidualEcho] Layer {kvp.Key} registered: {kvp.Value}");
                }
            }

            tagManager.ApplyModifiedProperties();
            Debug.Log("[ResidualEcho] Tags and layers registered!");
        }

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

            // --- GameManagerSettings SO ---
            var gameManagerSettings = AssetDatabase.LoadAssetAtPath<GameManagerSettings>(
                "Assets/ScriptableObjects/Core/GameManagerSettings.asset");

            // --- Event Channels ---
            var onPlayerDied = AssetDatabase.LoadAssetAtPath<GameEventChannel>(
                "Assets/ScriptableObjects/Events/OnPlayerDied.asset");
            var onPlayerRespawned = AssetDatabase.LoadAssetAtPath<GameEventChannel>(
                "Assets/ScriptableObjects/Events/OnPlayerRespawned.asset");
            var onHayunItemCollected = AssetDatabase.LoadAssetAtPath<GameEventChannel>(
                "Assets/ScriptableObjects/Events/OnHayunItemCollected.asset");
            var onSongStarted = AssetDatabase.LoadAssetAtPath<GameEventChannel>(
                "Assets/ScriptableObjects/Events/OnSongStarted.asset");
            var onSongEnded = AssetDatabase.LoadAssetAtPath<GameEventChannel>(
                "Assets/ScriptableObjects/Events/OnSongEnded.asset");

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

            // PlayerHealth
            var ph = playerGO.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                var so = new SerializedObject(ph);
                so.FindProperty("onPlayerDied").objectReferenceValue = onPlayerDied;
                so.FindProperty("onPlayerRespawned").objectReferenceValue = onPlayerRespawned;
                so.ApplyModifiedProperties();
            }

            // --- GameManager ---
            var gameManagerGO = GameObject.Find("GameManager");
            if (gameManagerGO != null)
            {
                var gm = gameManagerGO.GetComponent<GameManager>();
                if (gm != null)
                {
                    var so = new SerializedObject(gm);
                    so.FindProperty("settings").objectReferenceValue = gameManagerSettings;
                    so.FindProperty("onPlayerDied").objectReferenceValue = onPlayerDied;
                    so.FindProperty("onPlayerRespawned").objectReferenceValue = onPlayerRespawned;
                    so.FindProperty("playerTransform").objectReferenceValue = playerGO.transform;
                    so.FindProperty("playerInput").objectReferenceValue = playerGO.GetComponent<PlayerInput>();
                    so.FindProperty("playerCharacterController").objectReferenceValue = playerGO.GetComponent<CharacterController>();

                    var spawnPointGO = GameObject.Find("SpawnPoint");
                    if (spawnPointGO != null)
                    {
                        so.FindProperty("spawnPoint").objectReferenceValue = spawnPointGO.transform;
                    }

                    var deathScreenGO = GameObject.Find("DeathScreen");
                    if (deathScreenGO != null)
                    {
                        so.FindProperty("deathScreenUI").objectReferenceValue = deathScreenGO.GetComponent<DeathScreenUI>();
                    }

                    var gameOverScreenGO = GameObject.Find("GameOverScreen");
                    if (gameOverScreenGO != null)
                    {
                        so.FindProperty("gameOverUI").objectReferenceValue = gameOverScreenGO.GetComponent<GameOverUI>();
                    }

                    so.ApplyModifiedProperties();
                }
            }

            // --- GameOverUI 버튼 연결 ---
            {
                var goScreenGO = GameObject.Find("GameOverScreen");
                if (goScreenGO != null)
                {
                    var goUI = goScreenGO.GetComponent<GameOverUI>();
                    if (goUI != null)
                    {
                        var goTransform = goScreenGO.transform;
                        var restartBtn = goTransform.Find("RestartButton");
                        var mainMenuBtn = goTransform.Find("MainMenuButton");

                        var goSO = new SerializedObject(goUI);
                        if (restartBtn != null)
                            goSO.FindProperty("restartButton").objectReferenceValue = restartBtn.GetComponent<Button>();
                        if (mainMenuBtn != null)
                            goSO.FindProperty("mainMenuButton").objectReferenceValue = mainMenuBtn.GetComponent<Button>();
                        goSO.ApplyModifiedProperties();
                    }
                }
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
                so.FindProperty("onHayunItemCollected").objectReferenceValue = onHayunItemCollected;
                so.FindProperty("onSongStarted").objectReferenceValue = onSongStarted;
                so.FindProperty("onSongEnded").objectReferenceValue = onSongEnded;
                so.FindProperty("onPlayerDied").objectReferenceValue = onPlayerDied;
                so.FindProperty("onPlayerRespawned").objectReferenceValue = onPlayerRespawned;
                so.ApplyModifiedProperties();
            }

            // --- SceneLoaderCanvas ---
            var sceneLoaderCanvasGO = GameObject.Find("SceneLoaderCanvas");
            if (sceneLoaderCanvasGO != null)
            {
                var sceneLoader = sceneLoaderCanvasGO.GetComponent<SceneLoader>();
                if (sceneLoader != null)
                {
                    var fadeOverlay = sceneLoaderCanvasGO.transform.Find("FadeOverlay");
                    if (fadeOverlay != null)
                    {
                        var slSO = new SerializedObject(sceneLoader);
                        slSO.FindProperty("fadeCanvasGroup").objectReferenceValue = fadeOverlay.GetComponent<CanvasGroup>();
                        slSO.ApplyModifiedProperties();
                    }
                }
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

        [MenuItem("ResidualEcho/Create Event Assets")]
        public static void CreateEventAssets()
        {
            string eventsFolder = "Assets/ScriptableObjects/Events";
            if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects/Events"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/ScriptableObjects"))
                    AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Events");
            }

            string coreFolder = "Assets/ScriptableObjects/Core";
            if (!AssetDatabase.IsValidFolder(coreFolder))
            {
                AssetDatabase.CreateFolder("Assets/ScriptableObjects", "Core");
            }

            // Event Channels
            string[] eventNames = { "OnPlayerDied", "OnPlayerRespawned", "OnHayunItemCollected", "OnSongStarted", "OnSongEnded" };
            foreach (var name in eventNames)
            {
                string path = $"{eventsFolder}/{name}.asset";
                if (AssetDatabase.LoadAssetAtPath<GameEventChannel>(path) == null)
                {
                    var asset = ScriptableObject.CreateInstance<GameEventChannel>();
                    AssetDatabase.CreateAsset(asset, path);
                    Debug.Log($"[ResidualEcho] Created: {path}");
                }
            }

            // GameManagerSettings
            string gmSettingsPath = $"{coreFolder}/GameManagerSettings.asset";
            if (AssetDatabase.LoadAssetAtPath<GameManagerSettings>(gmSettingsPath) == null)
            {
                var asset = ScriptableObject.CreateInstance<GameManagerSettings>();
                AssetDatabase.CreateAsset(asset, gmSettingsPath);
                Debug.Log($"[ResidualEcho] Created: {gmSettingsPath}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ResidualEcho] All event assets created!");
        }

        [MenuItem("ResidualEcho/Setup Game Loop Scene")]
        public static void SetupGameLoopScene()
        {
            // --- GameManager ---
            var gameManagerGO = GameObject.Find("GameManager");
            if (gameManagerGO == null)
            {
                gameManagerGO = new GameObject("GameManager");
                Debug.Log("[ResidualEcho] Created GameManager GameObject");
            }
            if (gameManagerGO.GetComponent<GameManager>() == null)
            {
                gameManagerGO.AddComponent<GameManager>();
            }

            // --- SpawnPoint ---
            var spawnPointGO = GameObject.Find("SpawnPoint");
            if (spawnPointGO == null)
            {
                spawnPointGO = new GameObject("SpawnPoint");
                Debug.Log("[ResidualEcho] Created SpawnPoint GameObject");
            }
            // SpawnPoint를 Player 위치에 배치
            var playerGO = GameObject.Find("Player");
            if (playerGO != null)
            {
                spawnPointGO.transform.position = playerGO.transform.position;
                spawnPointGO.transform.rotation = playerGO.transform.rotation;
            }

            // --- Player: PlayerHealth 추가 ---
            if (playerGO != null && playerGO.GetComponent<PlayerHealth>() == null)
            {
                playerGO.AddComponent<PlayerHealth>();
                Debug.Log("[ResidualEcho] Added PlayerHealth to Player");
            }

            // --- Canvas + DeathScreen ---
            var canvasGO = GameObject.Find("Canvas");
            if (canvasGO == null)
            {
                canvasGO = new GameObject("Canvas");
                var canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
                Debug.Log("[ResidualEcho] Created Canvas");
            }
            // 기존 Canvas에 GraphicRaycaster가 없으면 추가
            if (canvasGO.GetComponent<GraphicRaycaster>() == null)
            {
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            var deathScreenGO = canvasGO.transform.Find("DeathScreen");
            if (deathScreenGO == null)
            {
                var dsGO = new GameObject("DeathScreen");
                dsGO.transform.SetParent(canvasGO.transform, false);

                // RectTransform을 전체 화면으로 설정
                var rt = dsGO.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                // 검정 이미지 추가
                var image = dsGO.AddComponent<UnityEngine.UI.Image>();
                image.color = Color.black;

                // CanvasGroup + DeathScreenUI 추가
                dsGO.AddComponent<CanvasGroup>();
                dsGO.AddComponent<DeathScreenUI>();

                Debug.Log("[ResidualEcho] Created DeathScreen UI");
            }

            // --- GameOverScreen (Canvas 하위) ---
            var canvasTransform = canvasGO.transform;
            var gameOverScreenGO = canvasTransform.Find("GameOverScreen");
            if (gameOverScreenGO == null)
            {
                var goGO = new GameObject("GameOverScreen");
                goGO.transform.SetParent(canvasTransform, false);

                var rt = goGO.AddComponent<RectTransform>();
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;

                goGO.AddComponent<CanvasGroup>();
                goGO.AddComponent<GameOverUI>();

                // "사망" 텍스트
                var titleGO = new GameObject("DeathText");
                titleGO.transform.SetParent(goGO.transform, false);
                var titleRT = titleGO.AddComponent<RectTransform>();
                titleRT.anchorMin = new Vector2(0.5f, 0.7f);
                titleRT.anchorMax = new Vector2(0.5f, 0.7f);
                titleRT.sizeDelta = new Vector2(400f, 80f);
                var titleTMP = titleGO.AddComponent<TextMeshProUGUI>();
                titleTMP.text = "YOU DIED";
                titleTMP.fontSize = 64f;
                titleTMP.alignment = TextAlignmentOptions.Center;
                titleTMP.color = Color.white;

                // 재시작 버튼
                var restartGO = CreateUIButton(goGO.transform, "RestartButton", "Restart",
                    new Vector2(0.5f, 0.45f), new Vector2(200f, 50f));

                // 메인 메뉴 버튼
                var mainMenuGO = CreateUIButton(goGO.transform, "MainMenuButton", "Main Menu",
                    new Vector2(0.5f, 0.35f), new Vector2(200f, 50f));

                // GameOverUI에 버튼 연결
                var gameOverUI = goGO.GetComponent<GameOverUI>();
                var goSO = new SerializedObject(gameOverUI);
                goSO.FindProperty("restartButton").objectReferenceValue = restartGO.GetComponent<Button>();
                goSO.FindProperty("mainMenuButton").objectReferenceValue = mainMenuGO.GetComponent<Button>();
                goSO.ApplyModifiedProperties();

                Debug.Log("[ResidualEcho] Created GameOverScreen UI");
            }

            // --- EventSystem ---
            EnsureEventSystem();

            // --- SceneLoaderCanvas (DontDestroyOnLoad) ---
            CreateSceneLoaderCanvas();

            // --- Creature: 트리거 콜라이더 확인 ---
            var creatureGO = GameObject.Find("Creature");
            if (creatureGO != null)
            {
                // 기존 콜라이더가 없으면 트리거 용 CapsuleCollider 추가
                var existingCollider = creatureGO.GetComponent<Collider>();
                if (existingCollider == null)
                {
                    var capsule = creatureGO.AddComponent<CapsuleCollider>();
                    capsule.isTrigger = true;
                    capsule.height = 2f;
                    capsule.center = new Vector3(0f, 1f, 0f);
                    capsule.radius = 0.8f;
                    Debug.Log("[ResidualEcho] Added trigger CapsuleCollider to Creature");
                }
                else if (!existingCollider.isTrigger)
                {
                    // 기존 콜라이더가 있지만 트리거가 아닌 경우, 별도 트리거 추가
                    var capsule = creatureGO.AddComponent<CapsuleCollider>();
                    capsule.isTrigger = true;
                    capsule.height = 2f;
                    capsule.center = new Vector3(0f, 1f, 0f);
                    capsule.radius = 0.8f;
                    Debug.Log("[ResidualEcho] Added trigger CapsuleCollider to Creature (alongside existing collider)");
                }
            }

            EditorUtility.SetDirty(gameManagerGO);
            if (playerGO != null) EditorUtility.SetDirty(playerGO);
            if (creatureGO != null) EditorUtility.SetDirty(creatureGO);

            Debug.Log("[ResidualEcho] Game loop scene setup complete! Run 'Wire Scene References' next.");
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
        [MenuItem("ResidualEcho/Setup Title Scene")]
        public static void SetupTitleScene()
        {
            // --- Main Camera ---
            var cam = Camera.main;
            if (cam != null)
            {
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Color.black;
                EditorUtility.SetDirty(cam.gameObject);
            }

            // --- TitleCanvas ---
            var titleCanvasGO = GameObject.Find("TitleCanvas");
            if (titleCanvasGO == null)
            {
                titleCanvasGO = new GameObject("TitleCanvas");
                var canvas = titleCanvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
                var scaler = titleCanvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                titleCanvasGO.AddComponent<GraphicRaycaster>();
            }

            var titleTransform = titleCanvasGO.transform;

            // TitleScreenUI 컴포넌트
            var titleUI = titleCanvasGO.GetComponent<TitleScreenUI>();
            if (titleUI == null)
            {
                titleUI = titleCanvasGO.AddComponent<TitleScreenUI>();
            }

            // "잔향" 타이틀 텍스트
            if (titleTransform.Find("TitleText") == null)
            {
                var titleTextGO = new GameObject("TitleText");
                titleTextGO.transform.SetParent(titleTransform, false);
                var rt = titleTextGO.AddComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.75f);
                rt.anchorMax = new Vector2(0.5f, 0.75f);
                rt.sizeDelta = new Vector2(600f, 120f);
                var tmp = titleTextGO.AddComponent<TextMeshProUGUI>();
                tmp.text = "Residual Echo";
                tmp.fontSize = 96f;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;
            }

            // 버튼들
            var startBtn = titleTransform.Find("StartButton")?.gameObject
                ?? CreateUIButton(titleTransform, "StartButton", "Start",
                    new Vector2(0.5f, 0.45f), new Vector2(250f, 55f));
            var settingsBtn = titleTransform.Find("SettingsButton")?.gameObject
                ?? CreateUIButton(titleTransform, "SettingsButton", "Settings",
                    new Vector2(0.5f, 0.35f), new Vector2(250f, 55f));
            var quitBtn = titleTransform.Find("QuitButton")?.gameObject
                ?? CreateUIButton(titleTransform, "QuitButton", "Quit",
                    new Vector2(0.5f, 0.25f), new Vector2(250f, 55f));

            // SettingsPanel (비활성)
            var settingsPanel = titleTransform.Find("SettingsPanel")?.gameObject;
            GameObject closeBtn = null;
            if (settingsPanel == null)
            {
                settingsPanel = new GameObject("SettingsPanel");
                settingsPanel.transform.SetParent(titleTransform, false);
                var spRT = settingsPanel.AddComponent<RectTransform>();
                spRT.anchorMin = new Vector2(0.25f, 0.2f);
                spRT.anchorMax = new Vector2(0.75f, 0.8f);
                spRT.offsetMin = Vector2.zero;
                spRT.offsetMax = Vector2.zero;

                var bg = settingsPanel.AddComponent<Image>();
                bg.color = new Color(0.15f, 0.15f, 0.15f, 0.95f);

                // "준비 중" 텍스트
                var placeholderGO = new GameObject("PlaceholderText");
                placeholderGO.transform.SetParent(settingsPanel.transform, false);
                var phRT = placeholderGO.AddComponent<RectTransform>();
                phRT.anchorMin = new Vector2(0.5f, 0.6f);
                phRT.anchorMax = new Vector2(0.5f, 0.6f);
                phRT.sizeDelta = new Vector2(300f, 60f);
                var phTMP = placeholderGO.AddComponent<TextMeshProUGUI>();
                phTMP.text = "Coming Soon";
                phTMP.fontSize = 36f;
                phTMP.alignment = TextAlignmentOptions.Center;
                phTMP.color = Color.white;

                // 닫기 버튼
                closeBtn = CreateUIButton(settingsPanel.transform, "CloseButton", "Close",
                    new Vector2(0.5f, 0.2f), new Vector2(150f, 45f));

                settingsPanel.SetActive(false);
            }
            else
            {
                closeBtn = settingsPanel.transform.Find("CloseButton")?.gameObject;
            }

            // TitleScreenUI에 참조 연결
            var tuiSO = new SerializedObject(titleUI);
            tuiSO.FindProperty("startButton").objectReferenceValue = startBtn.GetComponent<Button>();
            tuiSO.FindProperty("settingsButton").objectReferenceValue = settingsBtn.GetComponent<Button>();
            tuiSO.FindProperty("quitButton").objectReferenceValue = quitBtn.GetComponent<Button>();
            tuiSO.FindProperty("settingsPanel").objectReferenceValue = settingsPanel;
            if (closeBtn != null)
            {
                tuiSO.FindProperty("settingsCloseButton").objectReferenceValue = closeBtn.GetComponent<Button>();
            }
            tuiSO.ApplyModifiedProperties();

            // --- EventSystem ---
            EnsureEventSystem();

            // --- SceneLoaderCanvas ---
            CreateSceneLoaderCanvas();

            EditorUtility.SetDirty(titleCanvasGO);
            Debug.Log("[ResidualEcho] Title scene setup complete!");
        }

        /// <summary>
        /// UI 버튼 오브젝트를 생성한다. 검정 배경 + 흰 텍스트 스타일.
        /// </summary>
        private static GameObject CreateUIButton(Transform parent, string name, string label,
            Vector2 anchorPos, Vector2 size)
        {
            var btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);

            var rt = btnGO.AddComponent<RectTransform>();
            rt.anchorMin = anchorPos;
            rt.anchorMax = anchorPos;
            rt.sizeDelta = size;

            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);

            btnGO.AddComponent<Button>();

            // 텍스트 자식
            var textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            var textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = Vector2.zero;
            textRT.offsetMax = Vector2.zero;
            var tmp = textGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 24f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;

            return btnGO;
        }

        /// <summary>
        /// SceneLoaderCanvas를 씬에 생성한다. 이미 존재하면 스킵.
        /// </summary>
        private static void CreateSceneLoaderCanvas()
        {
            if (GameObject.Find("SceneLoaderCanvas") != null) return;

            var slcGO = new GameObject("SceneLoaderCanvas");
            var canvas = slcGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            slcGO.AddComponent<CanvasScaler>();

            // FadeOverlay
            var fadeGO = new GameObject("FadeOverlay");
            fadeGO.transform.SetParent(slcGO.transform, false);

            var fadeRT = fadeGO.AddComponent<RectTransform>();
            fadeRT.anchorMin = Vector2.zero;
            fadeRT.anchorMax = Vector2.one;
            fadeRT.offsetMin = Vector2.zero;
            fadeRT.offsetMax = Vector2.zero;

            var fadeImage = fadeGO.AddComponent<Image>();
            fadeImage.color = Color.black;

            var fadeCG = fadeGO.AddComponent<CanvasGroup>();
            fadeCG.alpha = 0f;
            fadeCG.blocksRaycasts = false;

            // SceneLoader 컴포넌트
            var sceneLoader = slcGO.AddComponent<SceneLoader>();
            var slSO = new SerializedObject(sceneLoader);
            slSO.FindProperty("fadeCanvasGroup").objectReferenceValue = fadeCG;
            slSO.ApplyModifiedProperties();

            EditorUtility.SetDirty(slcGO);
            Debug.Log("[ResidualEcho] Created SceneLoaderCanvas");
        }

        /// <summary>
        /// EventSystem이 씬에 없으면 생성한다.
        /// </summary>
        private static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() != null) return;

            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            EditorUtility.SetDirty(esGO);
            Debug.Log("[ResidualEcho] Created EventSystem");
        }
    }
}
#endif
