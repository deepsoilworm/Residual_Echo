using UnityEngine;

namespace ResidualEcho.Creature
{
    /// <summary>
    /// 추격 상태: 시야 내 포착된 플레이어를 빠른 속도로 쫓는다.
    /// 플레이어와 충분히 거리가 벌어지면 소실 상태로 전환.
    /// </summary>
    public class ChaseState : CreatureState
    {
        public ChaseState(CreatureStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            stateMachine.Agent.speed = stateMachine.Settings.ChaseSpeed * stateMachine.RageSpeedMultiplier;
        }

        public override void Update()
        {
            Transform player = stateMachine.PlayerTransform;
            if (player == null) return;

            // 플레이어 위치 직접 추적
            stateMachine.Agent.SetDestination(player.position);

            // 거리가 소실 거리를 초과하면 소실 전환
            float distance = Vector3.Distance(stateMachine.transform.position, player.position);
            if (distance > stateMachine.Settings.VanishDistance)
            {
                stateMachine.TransitionTo(stateMachine.VanishState);
            }
        }
    }
}
