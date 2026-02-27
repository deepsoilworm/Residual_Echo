using UnityEngine;
using UnityEngine.AI;

namespace ResidualEcho.Creature
{
    /// <summary>
    /// 접근 상태: 플레이어의 대략적 위치 근처로 천천히 이동한다.
    /// 시야 내에 플레이어를 포착하면 추격 상태로 전환.
    /// </summary>
    public class ApproachState : CreatureState
    {
        private Vector3 targetPosition;

        public ApproachState(CreatureStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            NavMeshAgent agent = stateMachine.Agent;
            agent.speed = stateMachine.Settings.ApproachSpeed * stateMachine.RageSpeedMultiplier;

            UpdateApproachTarget();
        }

        public override void Update()
        {
            // 시야 내 플레이어 감지 시 추격 전환
            if (stateMachine.Detection.CanSeePlayer)
            {
                stateMachine.TransitionTo(stateMachine.ChaseState);
                return;
            }

            // 목적지 도달 시 새로운 대략적 위치로 재설정
            NavMeshAgent agent = stateMachine.Agent;
            if (!agent.pathPending && agent.remainingDistance < 1f)
            {
                UpdateApproachTarget();
            }
        }

        /// <summary>
        /// 플레이어 위치 근처에 랜덤 오프셋을 더한 대략적 위치로 이동 목표를 설정한다.
        /// </summary>
        private void UpdateApproachTarget()
        {
            Transform player = stateMachine.PlayerTransform;
            if (player == null) return;

            float radius = stateMachine.Settings.ApproachRandomRadius
                - (stateMachine.RageLevel * stateMachine.Settings.RageApproachRadiusShrink);
            if (radius < 1f) radius = 1f;
            Vector2 randomOffset = Random.insideUnitCircle * radius;
            targetPosition = player.position + new Vector3(randomOffset.x, 0f, randomOffset.y);

            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, radius, NavMesh.AllAreas))
            {
                stateMachine.Agent.SetDestination(hit.position);
            }
        }
    }
}
