namespace ResidualEcho.Creature
{
    /// <summary>
    /// 출현 상태: 레벨 내 특정 스폰포인트에서 크리처가 나타나는 연출.
    /// TODO: 레벨 디자인 완성 후 구현 예정 (스폰포인트, 연출 타이밍 등)
    /// </summary>
    public class ManifestState : CreatureState
    {
        public ManifestState(CreatureStateMachine stateMachine) : base(stateMachine) { }

        public override void Enter()
        {
            // TODO: 출현 연출 시작 (레벨 스폰포인트에서 등장)
        }

        public override void Update()
        {
            // TODO: 출현 연출 완료 시 접근 상태로 전환
            stateMachine.TransitionTo(stateMachine.ApproachStateInstance);
        }

        public override void Exit()
        {
            // TODO: 출현 연출 정리
        }
    }
}
