using UnityEngine;
using ResidualEcho.Common.Constants;

namespace ResidualEcho.Creature
{
    /// <summary>
    /// 크리처의 플레이어 감지 시스템.
    /// 시야각(FOV)과 거리 기반으로 플레이어를 감지한다.
    /// </summary>
    public class CreatureDetection : MonoBehaviour
    {
        [SerializeField] private CreatureSettings settings;

        private Transform playerTransform;

        /// <summary>
        /// 현재 플레이어가 시야 내에 있는지 여부
        /// </summary>
        public bool CanSeePlayer { get; private set; }

        /// <summary>
        /// 감지 대상 플레이어 Transform
        /// </summary>
        public Transform PlayerTransform => playerTransform;

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag(GameTags.PLAYER);
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        private void Update()
        {
            CanSeePlayer = CheckVisibility();
        }

        /// <summary>
        /// 거리 + 시야각 + 장애물 차단 검사로 플레이어 가시성을 판정한다.
        /// </summary>
        private bool CheckVisibility()
        {
            if (playerTransform == null) return false;

            Vector3 directionToPlayer = playerTransform.position - transform.position;
            float distance = directionToPlayer.magnitude;

            // 감지 범위 밖
            if (distance > settings.DetectionRange) return false;

            // 시야각 밖
            float angle = Vector3.Angle(transform.forward, directionToPlayer);
            if (angle > settings.FieldOfView / 2f) return false;

            // 장애물 차단 검사
            Vector3 eyePosition = transform.position + Vector3.up * 1.5f;
            Vector3 targetPosition = playerTransform.position + Vector3.up * 1f;
            Vector3 direction = targetPosition - eyePosition;

            if (Physics.Raycast(eyePosition, direction.normalized, out RaycastHit hit, distance))
            {
                if (hit.transform == playerTransform || hit.transform.IsChildOf(playerTransform))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
