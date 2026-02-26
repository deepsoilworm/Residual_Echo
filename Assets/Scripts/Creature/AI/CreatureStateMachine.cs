using UnityEngine;
using UnityEngine.AI;
using ResidualEcho.Common.Constants;

namespace ResidualEcho.Creature
{
    /// <summary>
    /// 크리처 상태 머신. NavMeshAgent를 제어하며 상태 전환을 관리한다.
    /// 접근 → 추격 → 소실 → 접근 사이클로 동작.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(CreatureDetection))]
    public class CreatureStateMachine : MonoBehaviour
    {
        [SerializeField] private CreatureSettings settings;
        [SerializeField] private Renderer creatureRenderer;

        private CreatureState currentState;
        private Animator animator;

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

        // 상태 인스턴스
        public ApproachState ApproachStateInstance { get; private set; }
        public ChaseState ChaseState { get; private set; }
        public VanishState VanishState { get; private set; }

        private void Awake()
        {
            Agent = GetComponent<NavMeshAgent>();
            Detection = GetComponent<CreatureDetection>();
            animator = GetComponentInChildren<Animator>();

            ApproachStateInstance = new ApproachState(this);
            ChaseState = new ChaseState(this);
            VanishState = new VanishState(this);
        }

        private void Start()
        {
            TransitionTo(ApproachStateInstance);
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
    }
}
