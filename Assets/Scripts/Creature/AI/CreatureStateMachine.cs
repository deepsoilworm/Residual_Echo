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

        [Header("이벤트 채널")]
        [SerializeField] private GameEventChannel onHayunItemCollected;
        [SerializeField] private GameEventChannel onSongStarted;
        [SerializeField] private GameEventChannel onSongEnded;
        [SerializeField] private GameEventChannel onPlayerDied;
        [SerializeField] private GameEventChannel onPlayerRespawned;

        private CreatureState currentState;
        private CreatureState stateBeforeParalysis;
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

            initialPosition = transform.position;
        }

        private void Start()
        {
            TransitionTo(ApproachStateInstance);
        }

        private void OnEnable()
        {
            if (onHayunItemCollected != null) onHayunItemCollected.Subscribe(HandleHayunItemCollected);
            if (onSongStarted != null) onSongStarted.Subscribe(HandleSongStarted);
            if (onSongEnded != null) onSongEnded.Subscribe(HandleSongEnded);
            if (onPlayerDied != null) onPlayerDied.Subscribe(HandlePlayerDied);
            if (onPlayerRespawned != null) onPlayerRespawned.Subscribe(HandlePlayerRespawned);
        }

        private void OnDisable()
        {
            if (onHayunItemCollected != null) onHayunItemCollected.Unsubscribe(HandleHayunItemCollected);
            if (onSongStarted != null) onSongStarted.Unsubscribe(HandleSongStarted);
            if (onSongEnded != null) onSongEnded.Unsubscribe(HandleSongEnded);
            if (onPlayerDied != null) onPlayerDied.Unsubscribe(HandlePlayerDied);
            if (onPlayerRespawned != null) onPlayerRespawned.Unsubscribe(HandlePlayerRespawned);
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
        /// 크리처를 초기 위치로 리셋하고 접근 상태로 전환한다.
        /// </summary>
        public void ResetToInitialPosition()
        {
            Agent.enabled = false;
            transform.position = initialPosition;
            Agent.enabled = true;

            SetVisible(true);
            stateBeforeParalysis = null;
            TransitionTo(ApproachStateInstance);
        }

        // --- 이벤트 핸들러 ---

        private void HandleHayunItemCollected()
        {
            if (rageLevel < settings.MaxRageLevel)
            {
                rageLevel++;
            }
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
    }
}
