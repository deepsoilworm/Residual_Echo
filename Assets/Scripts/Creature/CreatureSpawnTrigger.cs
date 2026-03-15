using UnityEngine;
using ResidualEcho.Common.Interfaces;

namespace ResidualEcho.Creature
{
    /// <summary>
    /// 상호작용 시 크리처 프리팹을 스폰포인트에 인스턴스화하는 트리거.
    /// 아오오니 스타일: 특정 이벤트(상호작용, 아이템 획득 등)마다
    /// 크리처가 문이나 지정 위치에 나타난다.
    /// 프리팹 기반이므로 여러 마리 동시 스폰 가능.
    /// </summary>
    public class CreatureSpawnTrigger : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject creaturePrefab;
        [SerializeField] private Transform[] spawnPoints;

        [Tooltip("상호작용 후 비활성화하여 1회만 작동")]
        [SerializeField] private bool oneShot = true;

        /// <summary>
        /// 상호작용 UI에 표시할 텍스트
        /// </summary>
        public string InteractionPrompt => "Investigate";

        /// <summary>
        /// 상호작용 실행: 랜덤 스폰포인트에 크리처 프리팹을 인스턴스화한다.
        /// </summary>
        public void Interact()
        {
            if (creaturePrefab == null || spawnPoints == null || spawnPoints.Length == 0) return;

            Transform chosen = spawnPoints[Random.Range(0, spawnPoints.Length)];

            var instance = Instantiate(creaturePrefab, chosen.position, chosen.rotation);
            var sm = instance.GetComponent<CreatureStateMachine>();
            if (sm != null)
            {
                sm.CreatureSpawnPoint = chosen;
            }

            if (oneShot)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
