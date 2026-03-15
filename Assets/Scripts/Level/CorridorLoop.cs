using UnityEngine;

namespace ResidualEcho.Level
{
    /// <summary>
    /// 8번출구 스타일 심리스 복도 루프 매니저.
    /// 동일한 복도 세그먼트 3개를 일직선으로 배치하고,
    /// 플레이어가 가운데 세그먼트를 벗어나면 복도 1개 길이만큼 오프셋 텔레포트한다.
    /// 세그먼트가 동일하므로 텔레포트가 시각적으로 보이지 않는다.
    ///
    /// [세그먼트 0 (뒤)] [세그먼트 1 (가운데)] [세그먼트 2 (앞)]
    ///                    ↑ 플레이어는 항상 여기
    /// </summary>
    public class CorridorLoop : MonoBehaviour
    {
        [Header("세그먼트")]
        [Tooltip("동일한 복도 세그먼트 3개. 순서대로 뒤-가운데-앞 배치")]
        [SerializeField] private Transform[] segments = new Transform[3];

        [Header("트리거")]
        [Tooltip("가운데 세그먼트 끝(앞쪽)에 배치. 플레이어가 앞으로 진행할 때 감지")]
        [SerializeField] private CorridorTrigger triggerForward;
        [Tooltip("가운데 세그먼트 시작(뒤쪽)에 배치. 플레이어가 뒤로 돌아갈 때 감지")]
        [SerializeField] private CorridorTrigger triggerBackward;

        [Header("진행도")]
        [SerializeField] private int exitTarget = 8;

        /// <summary>
        /// 루프 방향을 따라 세그먼트 1개 길이만큼의 오프셋 벡터.
        /// segments[1].position - segments[0].position 으로 자동 계산된다.
        /// </summary>
        private Vector3 segmentOffset;

        private int currentCount;
        private bool isTeleporting;
        private bool hasAnomalyThisLoop;

        /// <summary>
        /// 현재 진행 카운터 (0 ~ exitTarget)
        /// </summary>
        public int CurrentCount => currentCount;

        /// <summary>
        /// 목표 카운터
        /// </summary>
        public int ExitTarget => exitTarget;

        private void Awake()
        {
            if (segments.Length < 3 || segments[0] == null || segments[1] == null || segments[2] == null)
            {
                Debug.LogError("[CorridorLoop] 세그먼트 3개를 모두 할당해야 합니다.");
                enabled = false;
                return;
            }

            segmentOffset = segments[1].position - segments[0].position;
        }

        private void OnEnable()
        {
            if (triggerForward != null) triggerForward.OnPlayerEntered += HandleForward;
            if (triggerBackward != null) triggerBackward.OnPlayerEntered += HandleBackward;
        }

        private void OnDisable()
        {
            if (triggerForward != null) triggerForward.OnPlayerEntered -= HandleForward;
            if (triggerBackward != null) triggerBackward.OnPlayerEntered -= HandleBackward;
        }

        /// <summary>
        /// 앞으로 진행 (가운데→앞 세그먼트 경계 통과).
        /// 세그먼트 1개 길이만큼 뒤로 텔레포트하여 다시 가운데 세그먼트에 위치시킨다.
        /// </summary>
        private void HandleForward(CharacterController player)
        {
            if (!hasAnomalyThisLoop)
            {
                currentCount++;
                Debug.Log($"[CorridorLoop] Forward OK: {currentCount}/{exitTarget}");
            }
            else
            {
                currentCount = 0;
                Debug.Log("[CorridorLoop] Forward FAIL (anomaly missed): reset to 0");
            }

            CheckClear();
            StartNewLoop();
            SeamlessTeleport(player, -segmentOffset);
        }

        /// <summary>
        /// 뒤로 돌아감 (가운데→뒤 세그먼트 경계 통과).
        /// 세그먼트 1개 길이만큼 앞으로 텔레포트하여 다시 가운데 세그먼트에 위치시킨다.
        /// </summary>
        private void HandleBackward(CharacterController player)
        {
            if (hasAnomalyThisLoop)
            {
                currentCount++;
                Debug.Log($"[CorridorLoop] Backward OK: {currentCount}/{exitTarget}");
            }
            else
            {
                currentCount = 0;
                Debug.Log("[CorridorLoop] Backward FAIL (no anomaly): reset to 0");
            }

            CheckClear();
            StartNewLoop();
            SeamlessTeleport(player, segmentOffset);
        }

        /// <summary>
        /// 새 루프를 시작한다. 이상현상 여부를 결정.
        /// </summary>
        private void StartNewLoop()
        {
            // TODO: 이상현상 시스템 연동. 지금은 50% 확률.
            hasAnomalyThisLoop = Random.value > 0.5f;
        }

        /// <summary>
        /// 클리어 조건 확인
        /// </summary>
        private void CheckClear()
        {
            if (currentCount >= exitTarget)
            {
                Debug.Log("[CorridorLoop] CLEAR!");
                // TODO: 클리어 이벤트 발생
            }
        }

        /// <summary>
        /// 플레이어를 오프셋만큼 이동시킨다 (심리스 텔레포트).
        /// 주변 환경이 동일하므로 시각적 끊김이 없다.
        /// </summary>
        private void SeamlessTeleport(CharacterController player, Vector3 offset)
        {
            if (isTeleporting) return;
            isTeleporting = true;

            player.enabled = false;
            player.transform.position += offset;
            player.enabled = true;

            Invoke(nameof(ResetTeleport), 0.1f);
        }

        private void ResetTeleport()
        {
            isTeleporting = false;
        }
    }
}
