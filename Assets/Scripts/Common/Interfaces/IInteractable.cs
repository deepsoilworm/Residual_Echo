namespace ResidualEcho.Common.Interfaces
{
    /// <summary>
    /// 플레이어가 상호작용할 수 있는 오브젝트 인터페이스.
    /// 다른 파트(아이템, 레벨)에서 이 인터페이스를 구현하여 상호작용 연동.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// 상호작용 UI에 표시할 텍스트 (예: "문 열기", "아이템 줍기")
        /// </summary>
        string InteractionPrompt { get; }

        /// <summary>
        /// 상호작용 실행
        /// </summary>
        void Interact();
    }
}
