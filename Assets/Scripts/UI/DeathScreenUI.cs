using System.Collections;
using UnityEngine;

namespace ResidualEcho.Core
{
    /// <summary>
    /// 사망 화면 UI. CanvasGroup 알파를 조절하여 페이드 인/아웃을 처리한다.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class DeathScreenUI : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private Coroutine fadeCoroutine;

        private void Awake()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }

        /// <summary>
        /// 페이드 인 (화면이 어두워짐). duration 동안 알파 0 → 1.
        /// </summary>
        public void FadeIn(float duration)
        {
            StopCurrentFade();
            fadeCoroutine = StartCoroutine(FadeCoroutine(0f, 1f, duration));
        }

        /// <summary>
        /// 페이드 아웃 (화면이 밝아짐). duration 동안 알파 1 → 0.
        /// </summary>
        public void FadeOut(float duration)
        {
            StopCurrentFade();
            fadeCoroutine = StartCoroutine(FadeCoroutine(1f, 0f, duration));
        }

        private IEnumerator FadeCoroutine(float from, float to, float duration)
        {
            canvasGroup.blocksRaycasts = to > 0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = to;
            fadeCoroutine = null;
        }

        private void StopCurrentFade()
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
                fadeCoroutine = null;
            }
        }
    }
}
