namespace ResidualEcho.Creature
{
    /// <summary>
    /// 크리처 상태 베이스 클래스. 모든 구체 상태는 이 클래스를 상속한다.
    /// </summary>
    public abstract class CreatureState
    {
        protected CreatureStateMachine stateMachine;

        public CreatureState(CreatureStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
        }

        /// <summary>
        /// 상태 진입 시 호출
        /// </summary>
        public virtual void Enter() { }

        /// <summary>
        /// 매 프레임 호출
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// 상태 이탈 시 호출
        /// </summary>
        public virtual void Exit() { }
    }
}
