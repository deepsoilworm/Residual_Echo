using UnityEngine;

namespace ResidualEcho.Player
{
    /// <summary>
    /// 플레이어 관련 설정값을 담는 ScriptableObject.
    /// Inspector에서 수치 조정 가능.
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerSettings", menuName = "ResidualEcho/Player Settings")]
    public class PlayerSettings : ScriptableObject
    {
        [Header("이동")]
        [SerializeField] private float walkSpeed = 3f;
        [SerializeField] private float sprintSpeed = 5.5f;
        [SerializeField] private float crouchSpeed = 1.5f;

        [Header("시점")]
        [SerializeField] private float mouseSensitivity = 0.15f;
        [SerializeField] private float verticalLookLimit = 80f;

        [Header("웅크리기")]
        [SerializeField] private float standingHeight = 1.8f;
        [SerializeField] private float crouchHeight = 1.0f;
        [SerializeField] private float crouchTransitionSpeed = 8f;

        [Header("중력")]
        [SerializeField] private float gravity = -15f;

        [Header("상호작용")]
        [SerializeField] private float interactionDistance = 2f;

        [Header("손전등")]
        [SerializeField] private float flashlightMaxBattery = 100f;
        [SerializeField] private float flashlightDrainRate = 1f;

        public float WalkSpeed => walkSpeed;
        public float SprintSpeed => sprintSpeed;
        public float CrouchSpeed => crouchSpeed;
        public float MouseSensitivity => mouseSensitivity;
        public float VerticalLookLimit => verticalLookLimit;
        public float StandingHeight => standingHeight;
        public float CrouchHeight => crouchHeight;
        public float CrouchTransitionSpeed => crouchTransitionSpeed;
        public float Gravity => gravity;
        public float InteractionDistance => interactionDistance;
        public float FlashlightMaxBattery => flashlightMaxBattery;
        public float FlashlightDrainRate => flashlightDrainRate;
    }
}
