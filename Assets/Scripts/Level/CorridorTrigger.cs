using System;
using UnityEngine;

namespace ResidualEcho.Level
{
    /// <summary>
    /// 복도 경계에 배치되는 방향성 트리거.
    /// 플레이어가 지정된 방향으로 통과할 때만 이벤트를 발생시킨다.
    /// BoxCollider(IsTrigger)와 함께 사용한다.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class CorridorTrigger : MonoBehaviour
    {
        [Tooltip("이 방향으로 이동하는 플레이어만 감지한다 (로컬 기준)")]
        [SerializeField] private Vector3 passDirection = Vector3.forward;

        /// <summary>
        /// 플레이어가 트리거를 올바른 방향으로 통과했을 때 발생하는 이벤트
        /// </summary>
        public event Action<CharacterController> OnPlayerEntered;

        private void OnTriggerEnter(Collider other)
        {
            var player = other.GetComponent<CharacterController>();
            if (player == null) return;

            // 플레이어 이동 방향과 기대 방향의 내적 확인
            Vector3 worldPassDir = transform.TransformDirection(passDirection).normalized;
            Vector3 playerVelocity = player.velocity;

            // velocity가 0이면 진입 방향으로 판단
            if (playerVelocity.sqrMagnitude < 0.01f)
            {
                playerVelocity = player.transform.forward;
            }

            if (Vector3.Dot(playerVelocity.normalized, worldPassDir) > 0f)
            {
                OnPlayerEntered?.Invoke(player);
            }
        }
    }
}
