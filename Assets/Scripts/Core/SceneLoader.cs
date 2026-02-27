using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ResidualEcho.Core
{
    /// <summary>
    /// 씬 전환을 관리하는 DontDestroyOnLoad 싱글턴.
    /// 페이드 투 블랙 → 씬 로드 → 페이드 인 시퀀스를 처리한다.
    /// SceneLoaderCanvas 프리팹에 부착하여 사용한다.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        /// <summary>
        /// 싱글턴 인스턴스
        /// </summary>
        public static SceneLoader Instance { get; private set; }

        [Header("페이드 설정")]
        [SerializeField] private CanvasGroup fadeCanvasGroup;
        [SerializeField] private float fadeDuration = 0.5f;

        private bool isTransitioning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = 0f;
                fadeCanvasGroup.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// 페이드 투 블랙 → 씬 로드 → 페이드 인 시퀀스로 씬을 전환한다.
        /// </summary>
        /// <param name="sceneName">로드할 씬 이름</param>
        public void LoadScene(string sceneName)
        {
            if (isTransitioning) return;
            StartCoroutine(LoadSceneCoroutine(sceneName));
        }

        private IEnumerator LoadSceneCoroutine(string sceneName)
        {
            isTransitioning = true;

            // 페이드 투 블랙
            yield return FadeCoroutine(0f, 1f);

            // 씬 로드
            yield return SceneManager.LoadSceneAsync(sceneName);

            // 페이드 인
            yield return FadeCoroutine(1f, 0f);

            isTransitioning = false;
        }

        /// <summary>
        /// 페이드 인 (화면이 어두워짐). 외부에서 페이드만 단독으로 사용할 때 호출한다.
        /// </summary>
        public void FadeIn()
        {
            if (fadeCanvasGroup != null)
            {
                StopAllCoroutines();
                isTransitioning = false;
                StartCoroutine(FadeCoroutine(0f, 1f));
            }
        }

        /// <summary>
        /// 페이드 아웃 (화면이 밝아짐). 외부에서 페이드만 단독으로 사용할 때 호출한다.
        /// </summary>
        public void FadeOut()
        {
            if (fadeCanvasGroup != null)
            {
                StopAllCoroutines();
                isTransitioning = false;
                StartCoroutine(FadeCoroutine(1f, 0f));
            }
        }

        private IEnumerator FadeCoroutine(float from, float to)
        {
            if (fadeCanvasGroup == null) yield break;

            fadeCanvasGroup.blocksRaycasts = true;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
                yield return null;
            }

            fadeCanvasGroup.alpha = to;
            fadeCanvasGroup.blocksRaycasts = to > 0f;
        }
    }
}
