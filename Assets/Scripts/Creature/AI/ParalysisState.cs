using UnityEngine;

namespace ResidualEcho.Creature
{
    /// <summary>
    /// 경직 상태: 하윤의 노래 재생 중 크리처가 움직이지 못한다.
    /// 노래가 끝나면 recoveryDelay 후 이전 상태로 복귀한다.
    /// </summary>
    public class ParalysisState : CreatureState
    {
        private bool songEnded;
        private float recoveryTimer;

        public ParalysisState(CreatureStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            songEnded = false;
            recoveryTimer = 0f;
            stateMachine.Agent.isStopped = true;
        }

        public override void Update()
        {
            if (!songEnded) return;

            recoveryTimer -= Time.deltaTime;
            if (recoveryTimer <= 0f)
            {
                stateMachine.ReturnFromParalysis();
            }
        }

        public override void Exit()
        {
            stateMachine.Agent.isStopped = false;
        }

        /// <summary>
        /// 노래가 끝났음을 알린다. recoveryDelay 후 이전 상태로 복귀.
        /// </summary>
        public void OnSongEnded()
        {
            songEnded = true;
            recoveryTimer = stateMachine.Settings.ParalysisRecoveryDelay;
        }
    }
}
