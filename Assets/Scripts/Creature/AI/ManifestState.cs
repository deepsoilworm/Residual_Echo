using UnityEngine;

namespace ResidualEcho.Creature
{
    /// <summary>
    /// 출현 상태: 크리처가 스폰포인트로 워프한 뒤 일정 시간 후 모습을 드러낸다.
    /// Enter에서 비가시 + 워프, 대기 시간 경과 후 가시화 + Approach 전환.
    /// </summary>
    public class ManifestState : CreatureState
    {
        private float timer;

        public ManifestState(CreatureStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            stateMachine.Agent.isStopped = true;
            stateMachine.SetVisible(false);

            WarpToSpawnPoint();

            timer = stateMachine.Settings.ManifestDelay;
        }

        public override void Update()
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
            {
                stateMachine.SetVisible(true);
                stateMachine.TransitionTo(stateMachine.ApproachStateInstance);
            }
        }

        public override void Exit()
        {
            stateMachine.Agent.isStopped = false;
        }

        /// <summary>
        /// 스폰포인트가 설정되어 있으면 해당 위치로 워프한다.
        /// </summary>
        private void WarpToSpawnPoint()
        {
            Transform spawnPoint = stateMachine.CreatureSpawnPoint;
            if (spawnPoint == null) return;

            stateMachine.Agent.enabled = false;
            stateMachine.transform.position = spawnPoint.position;
            stateMachine.transform.rotation = spawnPoint.rotation;
            stateMachine.Agent.enabled = true;
        }
    }
}
