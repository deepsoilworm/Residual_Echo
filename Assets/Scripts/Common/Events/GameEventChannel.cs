using System;
using UnityEngine;

namespace ResidualEcho.Common.Events
{
    /// <summary>
    /// 파라미터 없는 ScriptableObject 이벤트 채널.
    /// Inspector에서 드래그앤드롭으로 이벤트 발행/구독을 연결할 수 있다.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEventChannel", menuName = "ResidualEcho/Event Channel")]
    public class GameEventChannel : ScriptableObject
    {
        private event Action onEventRaised;

        /// <summary>
        /// 이벤트를 발행한다. 모든 구독자에게 알림.
        /// </summary>
        public void Raise()
        {
            onEventRaised?.Invoke();
        }

        /// <summary>
        /// 이벤트를 구독한다.
        /// </summary>
        public void Subscribe(Action listener)
        {
            onEventRaised += listener;
        }

        /// <summary>
        /// 이벤트 구독을 해제한다.
        /// </summary>
        public void Unsubscribe(Action listener)
        {
            onEventRaised -= listener;
        }
    }
}
