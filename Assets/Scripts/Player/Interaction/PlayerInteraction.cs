using UnityEngine;
using UnityEngine.InputSystem;
using ResidualEcho.Common.Interfaces;

namespace ResidualEcho.Player
{
    /// <summary>
    /// E키 상호작용 처리.
    /// 카메라 중앙에서 Raycast를 쏘아 IInteractable 대상을 감지하고 실행한다.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private PlayerSettings settings;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private LayerMask interactionLayerMask = ~0;

        private readonly RaycastHit[] raycastHits = new RaycastHit[10];

        /// <summary>
        /// 현재 바라보고 있는 상호작용 가능 대상. UI 표시 등에 활용.
        /// </summary>
        public IInteractable CurrentTarget { get; private set; }

        private void Update()
        {
            DetectInteractable();
        }

        /// <summary>
        /// 카메라 정면 Raycast로 IInteractable 대상 감지.
        /// RaycastNonAlloc으로 여러 히트를 받아 자기 자신(Player)을 건너뛰고
        /// 첫 번째 다른 오브젝트에서 IInteractable을 찾는다.
        /// </summary>
        private void DetectInteractable()
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);
            int hitCount = Physics.RaycastNonAlloc(ray, raycastHits, settings.InteractionDistance, interactionLayerMask);

            CurrentTarget = null;

            for (int i = 0; i < hitCount; i++)
            {
                // 자기 자신(Player) 콜라이더는 무시
                if (raycastHits[i].collider.gameObject == gameObject)
                {
                    continue;
                }

                CurrentTarget = raycastHits[i].collider.GetComponent<IInteractable>();
                break;
            }
        }

        /// <summary>
        /// Interact 입력 콜백 (PlayerInput 컴포넌트가 호출)
        /// </summary>
        public void OnInteract(InputValue value)
        {
            if (value.isPressed && CurrentTarget != null)
            {
                CurrentTarget.Interact();
            }
        }
    }
}
