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
        [SerializeField] private float approachSpeed = 2f;
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

        public float ApproachSpeed => approachSpeed;
        public float ChaseSpeed => chaseSpeed;
        public float DetectionRange => detectionRange;
        public float FieldOfView => fieldOfView;
        public float ChaseActivationRange => chaseActivationRange;
        public float VanishDistance => vanishDistance;
        public float VanishDuration => vanishDuration;
        public float ApproachRandomRadius => approachRandomRadius;
    }
}
