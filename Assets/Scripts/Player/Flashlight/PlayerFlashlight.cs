using UnityEngine;
using UnityEngine.InputSystem;

namespace ResidualEcho.Player
{
    /// <summary>
    /// 손전등 ON/OFF 토글 및 배터리 관리.
    /// 자식 오브젝트의 Spot Light를 제어한다.
    /// </summary>
    public class PlayerFlashlight : MonoBehaviour
    {
        [SerializeField] private PlayerSettings settings;
        [SerializeField] private Light spotLight;

        private float currentBattery;
        private bool isOn;

        /// <summary>
        /// 현재 배터리 잔량 (0~최대치)
        /// </summary>
        public float CurrentBattery => currentBattery;

        /// <summary>
        /// 손전등 켜짐 여부
        /// </summary>
        public bool IsOn => isOn;

        private void Awake()
        {
            currentBattery = settings.FlashlightMaxBattery;
            SetFlashlight(false);
        }

        private void Update()
        {
            if (isOn)
            {
                DrainBattery();
            }
        }

        /// <summary>
        /// 배터리 소모 처리. 배터리가 0이 되면 자동으로 꺼진다.
        /// </summary>
        private void DrainBattery()
        {
            currentBattery -= settings.FlashlightDrainRate * Time.deltaTime;

            if (currentBattery <= 0f)
            {
                currentBattery = 0f;
                SetFlashlight(false);
            }
        }

        private void SetFlashlight(bool on)
        {
            isOn = on;
            spotLight.enabled = on;
        }

        /// <summary>
        /// Flashlight 입력 콜백 (PlayerInput 컴포넌트가 호출)
        /// </summary>
        public void OnFlashlight(InputValue value)
        {
            if (value.isPressed && currentBattery > 0f)
            {
                SetFlashlight(!isOn);
            }
        }
    }
}
