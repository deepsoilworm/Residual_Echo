using UnityEngine;
using ResidualEcho.Common.Constants;
using ResidualEcho.Common.Events;

namespace ResidualEcho.Player
{
    /// <summary>
    /// 플레이어 사망 처리. 크리처 트리거 충돌 시 OnPlayerDied 이벤트를 발행한다.
    /// </summary>
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private GameEventChannel onPlayerDied;
        [SerializeField] private GameEventChannel onPlayerRespawned;

        private bool isDead;

        /// <summary>
        /// 현재 사망 상태인지 여부
        /// </summary>
        public bool IsDead => isDead;

        private void OnEnable()
        {
            isDead = false;

            if (onPlayerRespawned != null)
            {
                onPlayerRespawned.Subscribe(ResetHealth);
            }
        }

        private void OnDisable()
        {
            if (onPlayerRespawned != null)
            {
                onPlayerRespawned.Unsubscribe(ResetHealth);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isDead) return;

            if (other.CompareTag(GameTags.CREATURE))
            {
                Die();
            }
        }

        /// <summary>
        /// 사망 처리. OnPlayerDied 이벤트를 발행한다.
        /// </summary>
        private void Die()
        {
            isDead = true;

            if (onPlayerDied != null)
            {
                onPlayerDied.Raise();
            }
        }

        /// <summary>
        /// 리스폰 시 사망 상태를 초기화한다.
        /// </summary>
        public void ResetHealth()
        {
            isDead = false;
        }
    }
}
