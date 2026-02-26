using UnityEngine;

namespace ResidualEcho.Creature
{
    /// <summary>
    /// 애니메이션에 포함된 루트 본 이동을 무시하고 로컬 위치를 고정한다.
    /// CreatureMesh(FBX 모델)에 부착하여 사용.
    /// </summary>
    public class CreatureModelAnchor : MonoBehaviour
    {
        private Vector3 fixedLocalPosition;
        private Quaternion fixedLocalRotation;

        private void Awake()
        {
            fixedLocalPosition = transform.localPosition;
            fixedLocalRotation = transform.localRotation;
        }

        private void LateUpdate()
        {
            transform.localPosition = fixedLocalPosition;
            transform.localRotation = fixedLocalRotation;
        }
    }
}
