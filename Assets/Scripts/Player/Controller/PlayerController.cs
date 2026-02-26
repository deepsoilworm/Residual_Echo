using UnityEngine;
using UnityEngine.InputSystem;

namespace ResidualEcho.Player
{
    /// <summary>
    /// 1인칭 플레이어 컨트롤러.
    /// CharacterController 기반 이동, 마우스 시점 회전, 달리기, 웅크리기를 처리한다.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerSettings settings;
        [SerializeField] private Transform cameraHolder;

        private CharacterController characterController;
        private Vector2 moveInput;
        private Vector2 lookInput;
        private float verticalRotation;
        private float verticalVelocity;
        private bool isSprinting;
        private bool isCrouching;
        private float currentHeight;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            currentHeight = settings.StandingHeight;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            HandleLook();
            HandleMovement();
            HandleCrouch();
        }

        /// <summary>
        /// 마우스 시점 회전 처리. X축(상하) 회전은 제한한다.
        /// </summary>
        private void HandleLook()
        {
            float mouseX = lookInput.x * settings.MouseSensitivity;
            float mouseY = lookInput.y * settings.MouseSensitivity;

            verticalRotation -= mouseY;
            verticalRotation = Mathf.Clamp(verticalRotation, -settings.VerticalLookLimit, settings.VerticalLookLimit);

            cameraHolder.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }

        /// <summary>
        /// WASD 이동 + 중력 처리. 달리기/웅크리기 상태에 따라 속도가 달라진다.
        /// </summary>
        private void HandleMovement()
        {
            float speed = GetCurrentSpeed();

            Vector3 moveDirection = transform.right * moveInput.x + transform.forward * moveInput.y;
            moveDirection *= speed;

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += settings.Gravity * Time.deltaTime;
            moveDirection.y = verticalVelocity;

            characterController.Move(moveDirection * Time.deltaTime);
        }

        /// <summary>
        /// 웅크리기 높이 전환을 부드럽게 처리한다.
        /// </summary>
        private void HandleCrouch()
        {
            float targetHeight = isCrouching ? settings.CrouchHeight : settings.StandingHeight;

            if (Mathf.Abs(currentHeight - targetHeight) > 0.01f)
            {
                currentHeight = Mathf.Lerp(currentHeight, targetHeight, settings.CrouchTransitionSpeed * Time.deltaTime);
                characterController.height = currentHeight;
                characterController.center = new Vector3(0f, currentHeight / 2f, 0f);

                // 카메라 위치도 높이에 맞게 조정
                Vector3 camPos = cameraHolder.localPosition;
                camPos.y = currentHeight - 0.1f;
                cameraHolder.localPosition = camPos;
            }
        }

        private float GetCurrentSpeed()
        {
            if (isCrouching) return settings.CrouchSpeed;
            if (isSprinting) return settings.SprintSpeed;
            return settings.WalkSpeed;
        }

        // --- Input System 콜백 (PlayerInput 컴포넌트가 호출) ---

        /// <summary>
        /// Move 입력 콜백
        /// </summary>
        public void OnMove(InputValue value)
        {
            moveInput = value.Get<Vector2>();
        }

        /// <summary>
        /// Look 입력 콜백
        /// </summary>
        public void OnLook(InputValue value)
        {
            lookInput = value.Get<Vector2>();
        }

        /// <summary>
        /// Sprint 입력 콜백
        /// </summary>
        public void OnSprint(InputValue value)
        {
            isSprinting = value.isPressed;
        }

        /// <summary>
        /// Crouch 입력 콜백
        /// </summary>
        public void OnCrouch(InputValue value)
        {
            isCrouching = !isCrouching;
        }
    }
}
