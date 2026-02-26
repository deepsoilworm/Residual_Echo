using UnityEngine;

namespace ResidualEcho.Core
{
    /// <summary>
    /// GameManager 관련 설정값을 담는 ScriptableObject.
    /// </summary>
    [CreateAssetMenu(fileName = "GameManagerSettings", menuName = "ResidualEcho/Game Manager Settings")]
    public class GameManagerSettings : ScriptableObject
    {
        [Header("사망/리스폰")]
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private float respawnDelay = 2f;

        /// <summary>
        /// 화면 페이드 인/아웃에 걸리는 시간 (초)
        /// </summary>
        public float FadeDuration => fadeDuration;

        /// <summary>
        /// 사망 후 리스폰까지의 대기 시간 (초)
        /// </summary>
        public float RespawnDelay => respawnDelay;
    }
}
