using UnityEngine;
using UnityEngine.AI;
using ResidualEcho.Common.Constants;
using ResidualEcho.Common.Events;

namespace ResidualEcho.Creature
{
    /// <summary>
    /// 크리처 상태 머신. NavMeshAgent를 제어하며 상태 전환을 관리한다.
    /// 접근 → 추격 → 소실 → 접근 사이클로 동작.
    /// 격앙 수정자와 경직 상태를 지원한다.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CreatureDetection))]
    public class CreatureStateMachine : MonoBehaviour
    {
        [SerializeField] private CreatureSettings settings;
        [SerializeField] private Renderer creatureRenderer;
        [SerializeField] private Transform creatureSpawnPoint;

        [Header("이벤트 채널")]
        [SerializeField] private GameEventChannel onHayunItemCollected;
        [SerializeField] private GameEventChannel onSongStarted;
        [SerializeField] private GameEventChannel onSongEnded;
        [SerializeField] private GameEventChannel onPlayerDied;
        [SerializeField] private GameEventChannel onPlayerRespawned;
        [SerializeField] private GameEventChannel onCreatureSpawnRequested;

        private CreatureState currentState;
        private CreatureState stateBeforeParalysis;
        private CreatureState stateBeforeEnrage;
        private Animator animator;
        private int rageLevel;
        private Vector3 initialPosition;

        /// <summary>
        /// NavMeshAgent 참조
        /// </summary>
        public NavMeshAgent Agent { get; private set; }

        /// <summary>
        /// 감지 시스템 참조
        /// </summary>
        public CreatureDetection Detection { get; private set; }

        /// <summary>
        /// 크리처 설정값
        /// </summary>
        public CreatureSettings Settings => settings;

        /// <summary>
        /// 플레이어 Transform 참조
        /// </summary>
        public Transform PlayerTransform => Detection.PlayerTransform;

        /// <summary>
        /// 크리처 스폰포인트 Transform. ManifestState에서 사용.
        /// 외부에서 동적으로 변경 가능 (이벤트 트리거 등).
        /// </summary>
        public Transform CreatureSpawnPoint
        {
            get => creatureSpawnPoint;
            set => creatureSpawnPoint = value;
        }

        /// <summary>
        /// 현재 격앙 레벨 (0 ~ MaxRageLevel)
        /// </summary>
        public int RageLevel => rageLevel;

        /// <summary>
        /// 격앙에 따른 속도 배율. RageLevel × RageSpeedBonus 만큼 증가.
        /// </summary>
        public float RageSpeedMultiplier => 1f + (rageLevel * settings.RageSpeedBonus);

        // 상태 인스턴스
        public ApproachState ApproachStateInstance { get; private set; }
        public ChaseState ChaseState { get; private set; }
        public VanishState VanishState { get; private set; }
        public ParalysisState ParalysisState { get; private set; }
        public ManifestState ManifestState { get; private set; }
        public EnrageState EnrageState { get; private set; }

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            Detection = GetComponent<CreatureDetection>();
            animator = GetComponentInChildren<Animator>();

            ApproachStateInstance = new ApproachState(this);
            ChaseState = new ChaseState(this);
            VanishState = new VanishState(this);
            ParalysisState = new ParalysisState(this);
            ManifestState = new ManifestState(this);
            EnrageState = new EnrageState(this);

            initialPosition = transform.position;
        }

        private void Start()
        {
            // 스폰포인트가 설정되어 있으면 바로 출현 (프리팹 인스턴스화 시)
            if (creatureSpawnPoint != null)
            {
                TransitionTo(ManifestState);
            }
            else
            {
                // 스폰 요청 전까지 숨어있는다
                SetVisible(false);
                Agent.enabled = false;
            }
        }

        private void OnEnable()
        {
            if (onHayunItemCollected != null) onHayunItemCollected.Subscribe(HandleHayunItemCollected);
            if (onSongStarted != null) onSongStarted.Subscribe(HandleSongStarted);
            if (onSongEnded != null) onSongEnded.Subscribe(HandleSongEnded);
            if (onPlayerDied != null) onPlayerDied.Subscribe(HandlePlayerDied);
            if (onPlayerRespawned != null) onPlayerRespawned.Subscribe(HandlePlayerRespawned);
            if (onCreatureSpawnRequested != null) onCreatureSpawnRequested.Subscribe(HandleCreatureSpawnRequested);
        }

        private void OnDisable()
        {
            if (onHayunItemCollected != null) onHayunItemCollected.Unsubscribe(HandleHayunItemCollected);
            if (onSongStarted != null) onSongStarted.Unsubscribe(HandleSongStarted);
            if (onSongEnded != null) onSongEnded.Unsubscribe(HandleSongEnded);
            if (onPlayerDied != null) onPlayerDied.Unsubscribe(HandlePlayerDied);
            if (onPlayerRespawned != null) onPlayerRespawned.Unsubscribe(HandlePlayerRespawned);
            if (onCreatureSpawnRequested != null) onCreatureSpawnRequested.Unsubscribe(HandleCreatureSpawnRequested);
        }

        private void Update()
        {
            currentState?.Update();
            UpdateAnimator();
        }

        /// <summary>
        /// Animator 파라미터를 NavMeshAgent 속도와 현재 상태에 맞게 갱신한다.
        /// </summary>
        private void UpdateAnimator()
        {
            if (animator == null) return;
            animator.SetFloat("Speed", Agent.velocity.magnitude);
            animator.SetBool("IsChasing", currentState is ChaseState);
        }

        /// <summary>
        /// 상태를 전환한다. 이전 상태의 Exit → 새 상태의 Enter 순서로 호출.
        /// </summary>
        public void TransitionTo(CreatureState newState)
        {
            currentState?.Exit();
            currentState = newState;
            currentState.Enter();
        }

        /// <summary>
        /// 크리처의 가시성을 설정한다 (소실 상태에서 사용).
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (creatureRenderer != null)
            {
                creatureRenderer.enabled = visible;
            }
        }

        /// <summary>
        /// 경직 상태에서 복귀한다. 이전 상태로 돌아간다.
        /// </summary>
        public void ReturnFromParalysis()
        {
            CreatureState returnState = stateBeforeParalysis ?? ApproachStateInstance;
            stateBeforeParalysis = null;
            TransitionTo(returnState);
        }

        /// <summary>
        /// 격앙 상태에서 복귀한다. 격앙 전 상태로 돌아간다.
        /// </summary>
        public void ReturnFromEnrage()
        {
            CreatureState returnState = stateBeforeEnrage ?? ApproachStateInstance;
            stateBeforeEnrage = null;
            TransitionTo(returnState);
        }

        /// <summary>
        /// 크리처를 초기 위치로 리셋하고 접근 상태로 전환한다.
        /// </summary>
        public void ResetToInitialPosition()
        {
            Agent.enabled = false;
            transform.position = initialPosition;
            Agent.enabled = true;

            SetVisible(true);
            stateBeforeParalysis = null;
            stateBeforeEnrage = null;
            TransitionTo(ManifestState);
        }

        // --- 이벤트 핸들러 ---

        private void HandleHayunItemCollected()
        {
            if (rageLevel >= settings.MaxRageLevel) return;

            rageLevel++;

            if (currentState is EnrageState || currentState is ParalysisState) return;

            stateBeforeEnrage = currentState;
            TransitionTo(EnrageState);
        }

        private void HandleSongStarted()
        {
            if (currentState is ParalysisState) return;

            stateBeforeParalysis = currentState;
            TransitionTo(ParalysisState);
        }

        private void HandleSongEnded()
        {
            if (currentState is ParalysisState paralysis)
            {
                paralysis.OnSongEnded();
            }
        }

        private void HandlePlayerDied()
        {
            Agent.isStopped = true;
        }

        private void HandlePlayerRespawned()
        {
            Agent.isStopped = false;
            ResetToInitialPosition();
        }

        /// <summary>
        /// 크리처 스폰 요청 처리. ManifestState로 전환하여 스폰포인트에 출현한다.
        /// CreatureSpawnPoint가 외부에서 설정된 후 호출되어야 한다.
        /// </summary>
        /// <summary>
        /// 크리처 스폰 요청 처리. ManifestState로 전환하여 스폰포인트에 출현한다.
        /// CreatureSpawnPoint가 외부에서 설정된 후 호출되어야 한다.
        /// </summary>
        private void HandleCreatureSpawnRequested()
        {
            if (creatureSpawnPoint == null) return;

            Agent.enabled = true;
            TransitionTo(ManifestState);
        }
    }
}
