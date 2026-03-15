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
            SetupCreatureSpawnSystem();
        }

        /// <summary>
        /// 씬의 Creature를 프리팹으로 저장하고, 모든 CreatureSpawnTrigger에 연결한다.
        /// </summary>
        [MenuItem("ResidualEcho/Setup Creature Spawn System")]
        public static void SetupCreatureSpawnSystem()
        {
            // 1) Creature 프리팹 생성/갱신 (비활성 오브젝트 포함 검색)
            GameObject creature = null;
            foreach (var sm2 in Object.FindObjectsByType<CreatureStateMachine>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                creature = sm2.gameObject;
                break;
            }
            if (creature == null)
            {
                Debug.LogWarning("[ResidualEcho] Creature not found in scene!");
                return;
            }
            creature.SetActive(true); // 프리팹 저장을 위해 임시 활성화

            // 스폰포인트 참조 초기화 (프리팹에는 씬 참조 불가)
            var sm = creature.GetComponent<CreatureStateMachine>();
            if (sm != null)
            {
                sm.CreatureSpawnPoint = null;
            }

            string prefabFolder = "Assets/Prefabs/Creature";
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Creature"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                    AssetDatabase.CreateFolder("Assets", "Prefabs");
                AssetDatabase.CreateFolder("Assets/Prefabs", "Creature");
            }

            string prefabPath = $"{prefabFolder}/Creature.prefab";
            PrefabUtility.SaveAsPrefabAsset(creature, prefabPath);
            Debug.Log($"[ResidualEcho] Creature prefab saved: {prefabPath}");

            // 2) CorridorSegment 프리팹 YAML에서 creaturePrefab 참조 직접 설정
            string corridorPrefabPath = "Assets/Prefabs/Level/CorridorSegment.prefab";
            string creatureGuid = AssetDatabase.AssetPathToGUID(prefabPath);
            // Creature 프리팹의 root GameObject fileID 조회
            var creatureAsset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (creatureAsset != null && !string.IsNullOrEmpty(creatureGuid))
            {
                string fullPath = System.IO.Path.GetFullPath(corridorPrefabPath);
                string yaml = System.IO.File.ReadAllText(fullPath);
                string replacement = $"creaturePrefab: {{fileID: {GlobalObjectId.GetGlobalObjectIdSlow(creatureAsset).targetObjectId}, guid: {creatureGuid}, type: 3}}";
                yaml = yaml.Replace("creaturePrefab: {fileID: 0}", replacement);
                System.IO.File.WriteAllText(fullPath, yaml);
                AssetDatabase.ImportAsset(corridorPrefabPath);
                Debug.Log($"[ResidualEcho] Creature prefab linked via YAML: {replacement}");
            }

            // 3) 씬 인스턴스 갱신을 위해 레벨 재생성 필요 → 사용자에게 알림

            // 3) 씬의 Creature 비활성화 (프리팹에서 스폰하므로)
            creature.SetActive(false);

            Debug.Log("[ResidualEcho] Creature spawn system setup complete!");
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

        [MenuItem("ResidualEcho/Generate Greybox Level")]
        public static void GenerateGreyboxLevel()
        {
            // 기존 레벨 제거
            var existing = GameObject.Find("GreyboxLevel");
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            // --- Environment --- 아래에 생성
            var envParent = GameObject.Find("--- Environment ---");
            var root = new GameObject("GreyboxLevel");
            root.isStatic = true;
            if (envParent != null)
            {
                root.transform.SetParent(envParent.transform, false);
            }

            // NavMeshSurface 추가
            if (root.GetComponent<NavMeshSurface>() == null)
            {
                root.AddComponent<NavMeshSurface>();
            }

            // --- 치수 ---
            float cw = 3f;     // corridor width
            float ch = 2.9f;   // corridor height
            float wt = 0.3f;   // wall thickness
            float hw = cw / 2f;

            // 8번출구 스타일 심리스 루프: Z자 복도 × 3 세그먼트
            //
            // [세그먼트 0 (뒤)] ─연결─ [세그먼트 1 (가운데)] ─연결─ [세그먼트 2 (앞)]
            //                           ↑ 플레이어는 항상 여기
            //
            // 각 세그먼트: 직선A → 코너1 → 직선B → 코너2 → 직선C
            // 세그먼트 간 연결: C 끝 → 다음 A 시작 (벽 없이 이어짐)
            //
            float lenA = 5f;    // 직선A — 짧은 꺾임 구간
            float lenB = 20f;   // 직선B — 긴 메인 복도 (교실+지하실)
            float lenC = 5f;    // 직선C — 짧은 꺾임 구간

            var lv = root.transform;
            var floorMat = CreateGreyboxMaterial("Floor", new Color(0.25f, 0.25f, 0.25f));
            var wallMat = CreateGreyboxMaterial("Wall", new Color(0.35f, 0.35f, 0.32f));
            var ceilMat = CreateGreyboxMaterial("Ceiling", new Color(0.2f, 0.2f, 0.2f));
            var stairMat = CreateGreyboxMaterial("Stair", new Color(0.3f, 0.3f, 0.28f));
            // 프리팹 스폰 방식 — 이벤트 채널 불필요

            // 세그먼트 오프셋 계산 (Z자 1개의 시작→끝 벡터)
            float bStartX = cw;
            float bCenterZ = lenA + hw;
            float c2x = bStartX + lenB + hw;
            float cStartZ = bCenterZ + hw;
            float cEndZ = cStartZ + lenC;

            // C→A 연결 계단 (올라가기)
            int transStairCount = 6;
            float transStepHeight = 0.3f;   // 한 계단 높이
            float transStepDepth = 0.5f;    // 한 계단 깊이 (z)
            float transRise = transStairCount * transStepHeight; // 총 상승 1.8m
            float transLength = transStairCount * transStepDepth; // 총 길이 3.0m

            Vector3 segmentOffset = new Vector3(c2x, transRise, cEndZ + transLength);

            // ====== 세그먼트 프리팹 생성 ======
            string prefabFolder = "Assets/Prefabs/Level";
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/Level"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
                    AssetDatabase.CreateFolder("Assets", "Prefabs");
                AssetDatabase.CreateFolder("Assets/Prefabs", "Level");
            }

            // 임시 세그먼트 생성
            var tempSeg = new GameObject("CorridorSegment");
            var sr = tempSeg.transform;

            CreateCorridorSegment(sr, cw, ch, wt, hw, lenA, lenB, lenC,
                bStartX, bCenterZ, c2x, cStartZ, cEndZ,
                transStairCount, transStepHeight, transStepDepth, transRise, transLength,
                floorMat, wallMat, ceilMat, stairMat);

            // Static 설정
            foreach (Transform child in tempSeg.GetComponentsInChildren<Transform>())
            {
                child.gameObject.isStatic = true;
            }

            // 프리팹 저장
            string prefabPath = $"{prefabFolder}/CorridorSegment.prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(tempSeg, prefabPath);
            Object.DestroyImmediate(tempSeg);
            Debug.Log($"[ResidualEcho] Corridor segment prefab saved: {prefabPath}");

            // 프리팹 인스턴스 3개 배치
            Transform[] segmentRoots = new Transform[3];
            for (int seg = 0; seg < 3; seg++)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = $"Segment_{seg}";
                instance.transform.SetParent(lv, false);
                instance.transform.position = segmentOffset * seg;
                segmentRoots[seg] = instance.transform;
            }

            // ====== 심리스 루프 시스템 ======
            var loopSys = CreateChild(lv, "LoopSystem");

            // TriggerForward: 세그먼트1(가운데)→세그먼트2(앞) 경계
            // 위치: 세그먼트1의 계단 꼭대기 (C 끝 + 계단 길이)
            var triggerFwd = new GameObject("TriggerForward");
            triggerFwd.transform.SetParent(loopSys, false);
            triggerFwd.transform.position = segmentOffset + new Vector3(c2x, transRise + ch / 2f, cEndZ + transLength - 0.5f);
            var trigFwdCol = triggerFwd.AddComponent<BoxCollider>();
            trigFwdCol.isTrigger = true;
            trigFwdCol.size = new Vector3(cw, ch, 1f);

            // TriggerBackward: 세그먼트0(뒤)→세그먼트1(가운데) 경계
            // 위치: 세그먼트1의 A 시작 = segmentOffset * 1 + (0, ch/2, 0.5)
            var triggerBwd = new GameObject("TriggerBackward");
            triggerBwd.transform.SetParent(loopSys, false);
            triggerBwd.transform.position = segmentOffset + new Vector3(0f, ch / 2f, 0.5f);
            var trigBwdCol = triggerBwd.AddComponent<BoxCollider>();
            trigBwdCol.isTrigger = true;
            trigBwdCol.size = new Vector3(cw, ch, 1f);

            // 출구 표지판 위치 (가운데 세그먼트 기준)
            var signPos = new GameObject("ExitSignPosition");
            signPos.transform.SetParent(loopSys, false);
            signPos.transform.position = segmentOffset + new Vector3(-hw - wt, ch * 0.7f, 2f);
            signPos.transform.rotation = Quaternion.LookRotation(Vector3.right);

            // 컴포넌트 부착 + 참조 연결
            var trigFwdComp = triggerFwd.AddComponent<ResidualEcho.Level.CorridorTrigger>();
            var trigBwdComp = triggerBwd.AddComponent<ResidualEcho.Level.CorridorTrigger>();
            var loopComp = loopSys.gameObject.AddComponent<ResidualEcho.Level.CorridorLoop>();

            // 트리거 방향 설정: Forward는 +Z, Backward는 -Z (C/A 복도 진행 방향)
            var trigFwdSO = new SerializedObject(trigFwdComp);
            trigFwdSO.FindProperty("passDirection").vector3Value = Vector3.forward;
            trigFwdSO.ApplyModifiedProperties();

            var trigBwdSO = new SerializedObject(trigBwdComp);
            trigBwdSO.FindProperty("passDirection").vector3Value = Vector3.back;
            trigBwdSO.ApplyModifiedProperties();

            var loopSO = new SerializedObject(loopComp);
            // segments 배열 할당
            var segmentsProp = loopSO.FindProperty("segments");
            segmentsProp.arraySize = 3;
            for (int i = 0; i < 3; i++)
            {
                segmentsProp.GetArrayElementAtIndex(i).objectReferenceValue = segmentRoots[i];
            }
            loopSO.FindProperty("triggerForward").objectReferenceValue = trigFwdComp;
            loopSO.FindProperty("triggerBackward").objectReferenceValue = trigBwdComp;
            loopSO.ApplyModifiedProperties();

            // Static 설정 (LoopSystem 제외)
            foreach (Transform child in root.GetComponentsInChildren<Transform>())
            {
                if (child == loopSys || child.IsChildOf(loopSys))
                    continue;
                child.gameObject.isStatic = true;
            }

            Undo.RegisterCreatedObjectUndo(root, "Generate Greybox Level");
            EditorUtility.SetDirty(root);
            Debug.Log("[ResidualEcho] Greybox level generated! (Seamless Z-corridor × 3 segments + Loop System)");
        }

        /// <summary>
        /// Z자 복도 세그먼트 1개의 내용물을 생성한다.
        /// Corridor A → Corner1 → Corridor B → Corner2 → Corridor C → TransitionStairs
        /// + Room + Underground
        /// </summary>
        private static void CreateCorridorSegment(
            Transform sr,
            float cw, float ch, float wt, float hw,
            float lenA, float lenB, float lenC,
            float bStartX, float bCenterZ, float c2x, float cStartZ, float cEndZ,
            int transStairCount, float transStepHeight, float transStepDepth, float transRise, float transLength,
            Material floorMat, Material wallMat, Material ceilMat, Material stairMat)
        {
            // ====== 직선 A (+Z방향) ======
            var corridorA = CreateChild(sr, "Corridor_A");
            CreateBox(corridorA, "Floor", new Vector3(0f, -wt / 2f, lenA / 2f),
                new Vector3(cw, wt, lenA), floorMat);
            CreateBox(corridorA, "Ceiling", new Vector3(0f, ch + wt / 2f, lenA / 2f),
                new Vector3(cw, wt, lenA), ceilMat);
            CreateBox(corridorA, "Wall_Left", new Vector3(-hw - wt / 2f, ch / 2f, lenA / 2f),
                new Vector3(wt, ch, lenA), wallMat);
            CreateBox(corridorA, "Wall_Right", new Vector3(hw + wt / 2f, ch / 2f, lenA / 2f),
                new Vector3(wt, ch, lenA), wallMat);

            // ====== 코너1 (A→B, 우회전 +X) ======
            var corner1 = CreateChild(sr, "Corner1");
            float c1x = hw;
            float c1z = lenA + hw;
            CreateBox(corner1, "Floor", new Vector3(c1x, -wt / 2f, c1z),
                new Vector3(cw * 2, wt, cw), floorMat);
            CreateBox(corner1, "Ceiling", new Vector3(c1x, ch + wt / 2f, c1z),
                new Vector3(cw * 2, wt, cw), ceilMat);
            CreateBox(corner1, "Wall_Outer", new Vector3(c1x, ch / 2f, lenA + cw + wt / 2f),
                new Vector3(cw * 2 + wt * 2, ch, wt), wallMat);
            CreateBox(corner1, "Wall_Inner", new Vector3(-hw - wt / 2f, ch / 2f, c1z),
                new Vector3(wt, ch, cw), wallMat);

            // ====== 직선 B (+X방향) ======
            var corridorB = CreateChild(sr, "Corridor_B");
            CreateBox(corridorB, "Floor", new Vector3(bStartX + lenB / 2f, -wt / 2f, bCenterZ),
                new Vector3(lenB, wt, cw), floorMat);
            CreateBox(corridorB, "Ceiling", new Vector3(bStartX + lenB / 2f, ch + wt / 2f, bCenterZ),
                new Vector3(lenB, wt, cw), ceilMat);
            // 상벽 (z+) — 지하 입구 구멍 포함
            float stairDoorWidth = 1.5f;
            float stairDoorHeight = 2.4f;
            float ugDoorX = bStartX + lenB * 0.3f;
            float wallTopZ = bCenterZ + hw + wt / 2f;
            float wallTopL = ugDoorX - bStartX - stairDoorWidth / 2f;
            float wallTopR = bStartX + lenB - (ugDoorX + stairDoorWidth / 2f);
            CreateBox(corridorB, "Wall_Top_L",
                new Vector3(bStartX + wallTopL / 2f, ch / 2f, wallTopZ),
                new Vector3(wallTopL, ch, wt), wallMat);
            CreateBox(corridorB, "Wall_Top_R",
                new Vector3(ugDoorX + stairDoorWidth / 2f + wallTopR / 2f, ch / 2f, wallTopZ),
                new Vector3(wallTopR, ch, wt), wallMat);
            CreateBox(corridorB, "Wall_Top_DoorTop",
                new Vector3(ugDoorX, stairDoorHeight + (ch - stairDoorHeight) / 2f, wallTopZ),
                new Vector3(stairDoorWidth, ch - stairDoorHeight, wt), wallMat);
            // 하벽 (z-) — 교실 앞문/뒷문 2개 (한국 교실 스타일)
            float doorWidth = 1.2f;
            float doorHeight = 2.4f;
            float doorX = bStartX + lenB / 2f;       // 교실 중심
            float doorSpacing = 3.0f;                 // 중심에서 각 문까지 거리
            float door1X = doorX - doorSpacing;       // 앞문
            float door2X = doorX + doorSpacing;       // 뒷문
            float wallBotZ = bCenterZ - hw - wt / 2f;
            // 벽 3조각: 왼쪽 | 문1 | 중간 | 문2 | 오른쪽
            float wallL = door1X - doorWidth / 2f - bStartX;
            float wallM = door2X - doorWidth / 2f - (door1X + doorWidth / 2f);
            float wallR = bStartX + lenB - (door2X + doorWidth / 2f);
            CreateBox(corridorB, "Wall_Bot_L",
                new Vector3(bStartX + wallL / 2f, ch / 2f, wallBotZ),
                new Vector3(wallL, ch, wt), wallMat);
            CreateBox(corridorB, "Wall_Bot_M",
                new Vector3(doorX, ch / 2f, wallBotZ),
                new Vector3(wallM, ch, wt), wallMat);
            CreateBox(corridorB, "Wall_Bot_R",
                new Vector3(bStartX + lenB - wallR / 2f, ch / 2f, wallBotZ),
                new Vector3(wallR, ch, wt), wallMat);
            CreateBox(corridorB, "Wall_Bot_DoorTop1",
                new Vector3(door1X, doorHeight + (ch - doorHeight) / 2f, wallBotZ),
                new Vector3(doorWidth, ch - doorHeight, wt), wallMat);
            CreateBox(corridorB, "Wall_Bot_DoorTop2",
                new Vector3(door2X, doorHeight + (ch - doorHeight) / 2f, wallBotZ),
                new Vector3(doorWidth, ch - doorHeight, wt), wallMat);

            // ====== 코너2 (B→C, 좌회전 +Z) ======
            var corner2 = CreateChild(sr, "Corner2");
            CreateBox(corner2, "Floor", new Vector3(c2x, -wt / 2f, bCenterZ),
                new Vector3(cw, wt, cw), floorMat);
            CreateBox(corner2, "Ceiling", new Vector3(c2x, ch + wt / 2f, bCenterZ),
                new Vector3(cw, wt, cw), ceilMat);
            CreateBox(corner2, "Wall_Outer", new Vector3(c2x + hw + wt / 2f, ch / 2f, bCenterZ),
                new Vector3(wt, ch, cw + wt * 2), wallMat);
            CreateBox(corner2, "Wall_Inner", new Vector3(c2x, ch / 2f, bCenterZ - hw - wt / 2f),
                new Vector3(cw, ch, wt), wallMat);

            // ====== 직선 C (+Z방향) ======
            var corridorC = CreateChild(sr, "Corridor_C");
            CreateBox(corridorC, "Floor", new Vector3(c2x, -wt / 2f, cStartZ + lenC / 2f),
                new Vector3(cw, wt, lenC), floorMat);
            CreateBox(corridorC, "Ceiling", new Vector3(c2x, ch + wt / 2f, cStartZ + lenC / 2f),
                new Vector3(cw, wt, lenC), ceilMat);
            CreateBox(corridorC, "Wall_Right", new Vector3(c2x + hw + wt / 2f, ch / 2f, cStartZ + lenC / 2f),
                new Vector3(wt, ch, lenC), wallMat);
            CreateBox(corridorC, "Wall_Left", new Vector3(c2x - hw - wt / 2f, ch / 2f, cStartZ + lenC / 2f),
                new Vector3(wt, ch, lenC), wallMat);

            // ====== C→A 연결 계단 (올라가기) ======
            var transStairs = CreateChild(sr, "TransitionStairs");
            for (int i = 0; i < transStairCount; i++)
            {
                float stepY = transStepHeight * (i + 0.5f);
                float stepZ = cEndZ + (i + 0.5f) * transStepDepth;
                CreateBox(transStairs, $"Step_{i}",
                    new Vector3(c2x, stepY - transStepHeight / 2f, stepZ),
                    new Vector3(cw, transStepHeight, transStepDepth), stairMat);
            }
            float transHalfLen = transLength / 2f;
            float transCenterZ = cEndZ + transHalfLen;
            float transCenterY = transRise / 2f;
            CreateBox(transStairs, "Wall_Right",
                new Vector3(c2x + hw + wt / 2f, transCenterY + ch / 2f, transCenterZ),
                new Vector3(wt, ch + transRise, transLength), wallMat);
            CreateBox(transStairs, "Wall_Left",
                new Vector3(c2x - hw - wt / 2f, transCenterY + ch / 2f, transCenterZ),
                new Vector3(wt, ch + transRise, transLength), wallMat);
            CreateBox(transStairs, "Ceiling",
                new Vector3(c2x, transRise + ch + wt / 2f, transCenterZ),
                new Vector3(cw, wt, transLength), ceilMat);

            // ====== 방/교실 (직선B 하단으로 돌출) ======
            var room = CreateChild(sr, "Room");
            float roomWidth = 10.8f;  // 교실 가로 (X)
            float roomDepth = 9.5f;   // 교실 세로 (Z)
            float roomCenterX = doorX;
            float roomCenterZ = bCenterZ - hw - wt - roomDepth / 2f;
            CreateBox(room, "Floor", new Vector3(roomCenterX, -wt / 2f, roomCenterZ),
                new Vector3(roomWidth, wt, roomDepth), floorMat);
            CreateBox(room, "Ceiling", new Vector3(roomCenterX, ch + wt / 2f, roomCenterZ),
                new Vector3(roomWidth, wt, roomDepth), ceilMat);
            CreateBox(room, "Wall_Back", new Vector3(roomCenterX, ch / 2f, roomCenterZ - roomDepth / 2f - wt / 2f),
                new Vector3(roomWidth, ch, wt), wallMat);
            CreateBox(room, "Wall_Left", new Vector3(roomCenterX - roomWidth / 2f - wt / 2f, ch / 2f, roomCenterZ),
                new Vector3(wt, ch, roomDepth), wallMat);
            CreateBox(room, "Wall_Right", new Vector3(roomCenterX + roomWidth / 2f + wt / 2f, ch / 2f, roomCenterZ),
                new Vector3(wt, ch, roomDepth), wallMat);

            // ====== 교실 스폰포인트 (문 위치) + 상호작용 박스 ======
            // 교실 문 안쪽 스폰포인트 (교실 안에서 문을 보면 크리처가 서 있는 위치)
            float spawnZ = bCenterZ - hw - wt; // 문 교실쪽 안쪽 면
            // 앞문 스폰포인트 (교실 안을 바라보도록 = -Z)
            var spawnPoint1 = new GameObject("SpawnPoint_Door1");
            spawnPoint1.transform.SetParent(room, false);
            spawnPoint1.transform.localPosition = new Vector3(door1X - roomCenterX, 0f, spawnZ - roomCenterZ);
            spawnPoint1.transform.localRotation = Quaternion.LookRotation(Vector3.back);
            // 뒷문 스폰포인트
            var spawnPoint2 = new GameObject("SpawnPoint_Door2");
            spawnPoint2.transform.SetParent(room, false);
            spawnPoint2.transform.localPosition = new Vector3(door2X - roomCenterX, 0f, spawnZ - roomCenterZ);
            spawnPoint2.transform.localRotation = Quaternion.LookRotation(Vector3.back);

            // 상호작용 박스 (교실 중앙에 배치)
            var triggerBox = CreateBox(room, "InvestigateBox",
                new Vector3(roomCenterX, 0.5f, roomCenterZ),
                new Vector3(0.6f, 1f, 0.6f), stairMat);
            triggerBox.isStatic = false;
            triggerBox.tag = "Interactable";
            var boxCollider = triggerBox.GetComponent<BoxCollider>();
            if (boxCollider == null) boxCollider = triggerBox.AddComponent<BoxCollider>();
            var spawnTrigger = triggerBox.AddComponent<CreatureSpawnTrigger>();

            // CreatureSpawnTrigger에 스폰포인트 할당
            var triggerSO = new SerializedObject(spawnTrigger);
            var spawnPointsProp = triggerSO.FindProperty("spawnPoints");
            spawnPointsProp.arraySize = 2;
            spawnPointsProp.GetArrayElementAtIndex(0).objectReferenceValue = spawnPoint1.transform;
            spawnPointsProp.GetArrayElementAtIndex(1).objectReferenceValue = spawnPoint2.transform;
            triggerSO.ApplyModifiedProperties();

            // ====== 지하 (직선B 중간, 상벽 쪽에서 계단 하강) ======
            var ug = CreateChild(sr, "Underground");
            float ugZ = bCenterZ + hw + wt;
            float ugDepth = 3.5f;
            float ugRoomSize = 4f;
            float ugX = ugDoorX;
            int stairCount = 7;
            float stairStep = ugDepth / stairCount;
            float stairDepthZ = 0.6f;
            for (int i = 0; i < stairCount; i++)
            {
                CreateBox(ug, $"Stair_{i}",
                    new Vector3(ugX, -stairStep * (i + 0.5f), ugZ + (i + 0.5f) * stairDepthZ),
                    new Vector3(stairDoorWidth, 0.15f, stairDepthZ), stairMat);
                CreateBox(ug, $"StairWall_L_{i}",
                    new Vector3(ugX - stairDoorWidth / 2f - wt / 2f, -stairStep * i - stairStep / 2f, ugZ + (i + 0.5f) * stairDepthZ),
                    new Vector3(wt, stairStep + 0.15f, stairDepthZ), wallMat);
                CreateBox(ug, $"StairWall_R_{i}",
                    new Vector3(ugX + stairDoorWidth / 2f + wt / 2f, -stairStep * i - stairStep / 2f, ugZ + (i + 0.5f) * stairDepthZ),
                    new Vector3(wt, stairStep + 0.15f, stairDepthZ), wallMat);
            }
            float ugFloorY = -ugDepth;
            float stairEndZ = ugZ + stairCount * stairDepthZ;
            float ugCZ = stairEndZ + ugRoomSize / 2f;
            CreateBox(ug, "UG_Floor", new Vector3(ugX, ugFloorY - wt / 2f, ugCZ),
                new Vector3(ugRoomSize, wt, ugRoomSize), floorMat);
            CreateBox(ug, "UG_Ceiling", new Vector3(ugX, ugFloorY + ch + wt / 2f, ugCZ),
                new Vector3(ugRoomSize, wt, ugRoomSize), ceilMat);
            CreateBox(ug, "UG_Wall_Back", new Vector3(ugX, ugFloorY + ch / 2f, ugCZ + ugRoomSize / 2f + wt / 2f),
                new Vector3(ugRoomSize, ch, wt), wallMat);
            CreateBox(ug, "UG_Wall_Left", new Vector3(ugX - ugRoomSize / 2f - wt / 2f, ugFloorY + ch / 2f, ugCZ),
                new Vector3(wt, ch, ugRoomSize), wallMat);
            CreateBox(ug, "UG_Wall_Right", new Vector3(ugX + ugRoomSize / 2f + wt / 2f, ugFloorY + ch / 2f, ugCZ),
                new Vector3(wt, ch, ugRoomSize), wallMat);
        }

        /// <summary>
        /// GameEventChannel ScriptableObject 에셋을 생성/로드한다.
        /// </summary>
        private static GameEventChannel GetOrCreateEventChannel(string channelName)
        {
            string folder = "Assets/Data/Events";
            if (!AssetDatabase.IsValidFolder("Assets/Data"))
                AssetDatabase.CreateFolder("Assets", "Data");
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/Data", "Events");

            string path = $"{folder}/{channelName}.asset";
            var channel = AssetDatabase.LoadAssetAtPath<GameEventChannel>(path);
            if (channel == null)
            {
                channel = ScriptableObject.CreateInstance<GameEventChannel>();
                AssetDatabase.CreateAsset(channel, path);
                Debug.Log($"[ResidualEcho] Event channel created: {path}");
            }
            return channel;
        }

        /// <summary>
        /// 그레이박스용 머티리얼을 생성/로드한다.
        /// </summary>
        private static Material CreateGreyboxMaterial(string name, Color color)
        {
            string folder = "Assets/Materials/Greybox";
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
                AssetDatabase.CreateFolder("Assets", "Materials");
            if (!AssetDatabase.IsValidFolder(folder))
                AssetDatabase.CreateFolder("Assets/Materials", "Greybox");

            string path = $"{folder}/Greybox_{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = color;
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        /// <summary>
        /// 빈 자식 오브젝트를 생성한다.
        /// </summary>
        private static Transform CreateChild(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        /// <summary>
        /// 큐브 프리미티브를 생성한다. 위치/크기/머티리얼 지정.
        /// </summary>
        private static GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 scale, Material mat)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.isStatic = true;

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null && mat != null)
            {
                renderer.sharedMaterial = mat;
            }

            return go;
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
