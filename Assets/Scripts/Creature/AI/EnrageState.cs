using UnityEngine;

namespace ResidualEcho.Creature
{
    /// <summary>
    /// 격앙 상태: 격앙 레벨 상승 시 짧은 연출 후 이전 상태로 복귀한다.
    /// 연출 동안 크리처는 정지하며, 완료 후 격앙 전 상태로 돌아간다.
    /// </summary>
    public class EnrageState : CreatureState
    {
        private float timer;

        public EnrageState(CreatureStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            stateMachine.Agent.isStopped = true;
            timer = stateMachine.Settings.EnrageDuration;
        }

        public override void Update()
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                stateMachine.ReturnFromEnrage();
            }
        }

        public override void Exit()
        {
            stateMachine.Agent.isStopped = false;
        }
    }
}
