using System;
using UnityEngine;

namespace ResidualEcho.Common.Events
{
    /// <summary>
    /// 제네릭 페이로드를 전달하는 ScriptableObject 이벤트 채널 베이스 클래스.
    /// 구체 타입(FloatEventChannel 등)을 만들어 사용한다.
    /// </summary>
    public abstract class GameEventChannel<T> : ScriptableObject
    {
        private event Action<T> onEventRaised;

        /// <summary>
        /// 페이로드와 함께 이벤트를 발행한다.
        /// </summary>
        public void Raise(T value)
        {
            onEventRaised?.Invoke(value);
        }

        /// <summary>
        /// 이벤트를 구독한다.
        /// </summary>
        public void Subscribe(Action<T> listener)
        {
            onEventRaised += listener;
        }

        /// <summary>
        /// 이벤트 구독을 해제한다.
        /// </summary>
        public void Unsubscribe(Action<T> listener)
        {
            onEventRaised -= listener;
        }
    }

    /// <summary>
    /// float 값을 전달하는 이벤트 채널.
    /// </summary>
    [CreateAssetMenu(fileName = "NewFloatEventChannel", menuName = "ResidualEcho/Float Event Channel")]
    public class FloatEventChannel : GameEventChannel<float> { }
}
