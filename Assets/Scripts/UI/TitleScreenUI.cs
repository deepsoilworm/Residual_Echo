using UnityEngine;
using UnityEngine.UI;
using ResidualEcho.Common.Constants;
using ResidualEcho.Core;

namespace ResidualEcho.UI
{
    /// <summary>
    /// 타이틀 화면 UI. 게임 시작, 설정, 종료 버튼을 처리한다.
    /// </summary>
    public class TitleScreenUI : MonoBehaviour
    {
        [Header("버튼")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("설정 패널")]
        [SerializeField] private GameObject settingsPanel;

        [Header("설정 패널 내부")]
        [SerializeField] private Button settingsCloseButton;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(HandleStart);
            }
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(HandleSettings);
            }
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(HandleQuit);
            }
            if (settingsCloseButton != null)
            {
                settingsCloseButton.onClick.AddListener(HandleCloseSettings);
            }
        }

        private void OnDisable()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(HandleStart);
            }
            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(HandleSettings);
            }
            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(HandleQuit);
            }
            if (settingsCloseButton != null)
            {
                settingsCloseButton.onClick.RemoveListener(HandleCloseSettings);
            }
        }

        private void HandleStart()
        {
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(SceneNames.PROTOTYPE_TEST);
            }
        }

        private void HandleSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(true);
            }
        }

        private void HandleCloseSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }
        }

        private void HandleQuit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
