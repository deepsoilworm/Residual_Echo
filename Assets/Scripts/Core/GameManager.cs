using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using ResidualEcho.Common.Constants;
using ResidualEcho.Common.Events;
using ResidualEcho.UI;

namespace ResidualEcho.Core
{
    /// <summary>
    /// 게임 전체 상태(Playing/GameOver)를 관리한다.
    /// 사망 → 페이드 → 게임 오버 화면 → 재시작/메인메뉴 시퀀스를 제어하며,
    /// 이벤트 채널을 통해 각 시스템과 통신한다.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("설정")]
        [SerializeField] private GameManagerSettings settings;

        [Header("이벤트 채널")]
        [SerializeField] private GameEventChannel onPlayerDied;
        [SerializeField] private GameEventChannel onPlayerRespawned;

        [Header("씬 참조")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private PlayerInput playerInput;
        [SerializeField] private CharacterController playerCharacterController;
        [SerializeField] private DeathScreenUI deathScreenUI;
        [SerializeField] private GameOverUI gameOverUI;

        private bool isGameOver;

        /// <summary>
        /// 현재 게임이 종료(사망) 상태인지 여부
        /// </summary>
        public bool IsGameOver => isGameOver;

        private void OnEnable()
        {
            if (onPlayerDied != null)
            {
                onPlayerDied.Subscribe(HandlePlayerDied);
            }
            if (gameOverUI != null)
            {
                gameOverUI.OnRestartClicked += HandleRestart;
                gameOverUI.OnMainMenuClicked += HandleMainMenu;
            }
        }

        private void OnDisable()
        {
            if (onPlayerDied != null)
            {
                onPlayerDied.Unsubscribe(HandlePlayerDied);
            }
            if (gameOverUI != null)
            {
                gameOverUI.OnRestartClicked -= HandleRestart;
                gameOverUI.OnMainMenuClicked -= HandleMainMenu;
            }
        }

        /// <summary>
        /// 플레이어 사망 이벤트 처리. 사망 → 페이드 → 게임 오버 화면 시퀀스를 시작한다.
        /// </summary>
        private void HandlePlayerDied()
        {
            if (isGameOver) return;
            isGameOver = true;

            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            // 1. 입력 차단
            if (playerInput != null)
            {
                playerInput.DeactivateInput();
            }

            // 2. 페이드 투 블랙
            if (deathScreenUI != null)
            {
                deathScreenUI.FadeIn(settings.FadeDuration);
            }

            yield return new WaitForSeconds(settings.FadeDuration);

            // 3. 대기 후 게임 오버 화면 표시
            yield return new WaitForSeconds(settings.RespawnDelay);

            if (gameOverUI != null)
            {
                gameOverUI.Show();
            }
        }

        /// <summary>
        /// 재시작 버튼 처리. 게임 오버 화면을 숨기고 리스폰한다.
        /// </summary>
        private void HandleRestart()
        {
            StartCoroutine(RestartSequence());
        }

        private IEnumerator RestartSequence()
        {
            // 1. 게임 오버 화면 숨기기
            if (gameOverUI != null)
            {
                gameOverUI.Hide();
            }

            // 2. 플레이어를 스폰 위치로 이동
            TeleportPlayer();

            // 3. 페이드 아웃 (밝아짐)
            if (deathScreenUI != null)
            {
                deathScreenUI.FadeOut(settings.FadeDuration);
            }

            yield return new WaitForSeconds(settings.FadeDuration);

            // 4. 입력 복원
            if (playerInput != null)
            {
                playerInput.ActivateInput();
            }

            isGameOver = false;

            // 5. 리스폰 이벤트 발행
            if (onPlayerRespawned != null)
            {
                onPlayerRespawned.Raise();
            }
        }

        /// <summary>
        /// 메인 메뉴 버튼 처리. SceneLoader를 통해 타이틀 씬으로 전환한다.
        /// </summary>
        private void HandleMainMenu()
        {
            if (gameOverUI != null)
            {
                gameOverUI.Hide();
            }

            isGameOver = false;

            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadScene(SceneNames.TITLE);
            }
        }

        /// <summary>
        /// CharacterController를 비활성화한 뒤 위치를 이동하고 다시 활성화한다.
        /// </summary>
        private void TeleportPlayer()
        {
            if (playerTransform == null || spawnPoint == null) return;

            if (playerCharacterController != null)
            {
                playerCharacterController.enabled = false;
            }

            playerTransform.position = spawnPoint.position;
            playerTransform.rotation = spawnPoint.rotation;

            if (playerCharacterController != null)
            {
                playerCharacterController.enabled = true;
            }
        }
    }
}
