using UnityEngine;

namespace ResidualEcho.Creature
{
    /// <summary>
    /// 소실 상태: 크리처가 사라진 후 일정 시간 뒤 다시 접근 상태로 전환한다.
    /// </summary>
    public class VanishState : CreatureState
    {
        private float timer;

        public VanishState(CreatureStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            timer = stateMachine.Settings.VanishDuration;
            stateMachine.Agent.isStopped = true;
            stateMachine.SetVisible(false);
        }

        public override void Update()
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                stateMachine.TransitionTo(stateMachine.ApproachStateInstance);
            }
        }

        public override void Exit()
        {
            stateMachine.Agent.isStopped = false;
            stateMachine.SetVisible(true);
        }
    }
}
