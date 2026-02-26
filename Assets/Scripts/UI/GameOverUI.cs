using System;
using UnityEngine;
using UnityEngine.UI;

namespace ResidualEcho.UI
{
    /// <summary>
    /// 게임 오버 화면 UI. "사망" 텍스트와 재시작/메인메뉴 버튼을 표시한다.
    /// CanvasGroup으로 표시/숨김을 제어하며, 버튼 클릭 시 이벤트를 발행한다.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class GameOverUI : MonoBehaviour
    {
        [Header("버튼")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        /// <summary>
        /// 재시작 버튼 클릭 시 발행
        /// </summary>
        public event Action OnRestartClicked;

        /// <summary>
        /// 메인 메뉴 버튼 클릭 시 발행
        /// </summary>
        public event Action OnMainMenuClicked;

        private CanvasGroup canvasGroup;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            Hide();
        }

        private void OnEnable()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleRestartClicked);
            }
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(HandleMainMenuClicked);
            }
        }

        private void OnDisable()
        {
            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(HandleRestartClicked);
            }
            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveListener(HandleMainMenuClicked);
            }
        }

        /// <summary>
        /// 게임 오버 화면을 표시한다. 커서를 언락하고 UI를 활성화한다.
        /// </summary>
        public void Show()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        /// <summary>
        /// 게임 오버 화면을 숨긴다. 커서를 잠그고 UI를 비활성화한다.
        /// </summary>
        public void Hide()
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void HandleRestartClicked()
        {
            OnRestartClicked?.Invoke();
        }

        private void HandleMainMenuClicked()
        {
            OnMainMenuClicked?.Invoke();
        }
    }
}
