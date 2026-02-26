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

        /// <summary>
        /// 현재 바라보고 있는 상호작용 가능 대상. UI 표시 등에 활용.
        /// </summary>
        public IInteractable CurrentTarget { get; private set; }

        private void Update()
        {
            DetectInteractable();
        }

        /// <summary>
        /// 카메라 정면 Raycast로 IInteractable 대상 감지
        /// </summary>
        private void DetectInteractable()
        {
            Ray ray = new Ray(cameraTransform.position, cameraTransform.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, settings.InteractionDistance))
            {
                CurrentTarget = hit.collider.GetComponent<IInteractable>();
            }
            else
            {
                CurrentTarget = null;
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
