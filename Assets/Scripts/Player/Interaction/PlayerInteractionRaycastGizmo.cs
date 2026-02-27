using UnityEngine;

namespace ResidualEcho.Player
{
    /// <summary>
    /// PlayerInteraction과 동일한 기준(카메라 forward, 상호작용 거리)으로
    /// 상호작용 Raycast 기즈모를 시각화한다.
    /// </summary>
    public class PlayerInteractionRaycastGizmo : MonoBehaviour
    {
        [SerializeField] private PlayerSettings settings;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private bool drawOnlyWhenSelected = true;

        private static readonly Color HitColor = new Color(0f, 1f, 0f, 0.9f);
        private static readonly Color MissColor = new Color(1f, 0.75f, 0f, 0.9f);

        /// <summary>
        /// 오브젝트가 선택되지 않아도 기즈모를 렌더링한다.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (drawOnlyWhenSelected)
            {
                return;
            }

            DrawInteractionRayGizmo();
        }

        /// <summary>
        /// 오브젝트 선택 상태에서 기즈모를 렌더링한다.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            DrawInteractionRayGizmo();
        }

        private void DrawInteractionRayGizmo()
        {
            if (cameraTransform == null || settings == null)
            {
                return;
            }

            Vector3 origin = cameraTransform.position;
            Vector3 direction = cameraTransform.forward;
            float maxDistance = settings.InteractionDistance;
            Ray ray = new Ray(origin, direction);

            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            {
                Gizmos.color = HitColor;
                Gizmos.DrawLine(origin, hit.point);
                Gizmos.DrawWireSphere(hit.point, 0.08f);
                return;
            }

            Gizmos.color = MissColor;
            Gizmos.DrawLine(origin, origin + direction * maxDistance);
        }
    }
}
