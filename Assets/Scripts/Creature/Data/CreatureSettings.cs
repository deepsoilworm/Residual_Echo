using UnityEngine;

namespace ResidualEcho.Creature
{
    /// <summary>
    /// 크리처 AI 관련 설정값을 담는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "CreatureSettings", menuName = "ResidualEcho/Creature Settings")]
    public class CreatureSettings : ScriptableObject
    {
        [Header("이동 속도")]
        [SerializeField] private float approachSpeed = 3f;
        [SerializeField] private float chaseSpeed = 6f;

        [Header("감지")]
        [SerializeField] private float detectionRange = 15f;
        [SerializeField] private float fieldOfView = 90f;
        [SerializeField] private float chaseActivationRange = 12f;

        [Header("소실")]
        [SerializeField] private float vanishDistance = 25f;
        [SerializeField] private float vanishDuration = 3f;

        [Header("접근")]
        [SerializeField] private float approachRandomRadius = 5f;

        [Header("격앙 (Rage)")]
        [SerializeField] private float rageSpeedBonus = 0.15f;
        [SerializeField] private int maxRageLevel = 5;
        [SerializeField] private float rageApproachRadiusShrink = 0.5f;

        [Header("출현 (Manifest)")]
        [SerializeField] private float manifestDelay = 1.5f;

        [Header("격앙 (Enrage)")]
        [SerializeField] private float enrageDuration = 1f;

        [Header("경직 (Paralysis)")]
        [SerializeField] private float paralysisRecoveryDelay = 1f;

        public float ApproachSpeed => approachSpeed;
        public float ChaseSpeed => chaseSpeed;
        public float DetectionRange => detectionRange;
        public float FieldOfView => fieldOfView;
        public float ChaseActivationRange => chaseActivationRange;
        public float VanishDistance => vanishDistance;
        public float VanishDuration => vanishDuration;
        public float ApproachRandomRadius => approachRandomRadius;

        /// <summary>
        /// 격앙 레벨당 속도 보너스 배율 (0.15 = 15%)
        /// </summary>
        public float RageSpeedBonus => rageSpeedBonus;

        /// <summary>
        /// 최대 격앙 레벨
        /// </summary>
        public int MaxRageLevel => maxRageLevel;

        /// <summary>
        /// 격앙 레벨당 접근 랜덤 반경 축소량
        /// </summary>
        public float RageApproachRadiusShrink => rageApproachRadiusShrink;

        /// <summary>
        /// 출현 연출 대기 시간 (초)
        /// </summary>
        public float ManifestDelay => manifestDelay;

        /// <summary>
        /// 격앙 연출 시간 (초)
        /// </summary>
        public float EnrageDuration => enrageDuration;

        /// <summary>
        /// 경직 상태에서 노래 종료 후 복귀까지의 지연 시간 (초)
        /// </summary>
        public float ParalysisRecoveryDelay => paralysisRecoveryDelay;
    }
}
