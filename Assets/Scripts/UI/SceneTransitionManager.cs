using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NeonSerpent.UI
{
    /// <summary>
    /// Full-screen fade transition manager. Singleton, persists across scenes.
    /// Used for smooth transitions between menu and gameplay states.
    /// </summary>
    public class SceneTransitionManager : MonoBehaviour
    {
        public static SceneTransitionManager Instance { get; private set; }

        [Header("Transition Settings")]
        [SerializeField] private float defaultDuration = 0.5f;
        [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("UI")]
        [SerializeField] private Image fadeOverlay;

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;
        private bool _isTransitioning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            EnsureOverlayExists();
        }

        /// <summary>
        /// Perform a fade-out → execute callback → fade-in transition.
        /// </summary>
        public void TransitionTo(Action onMidTransition, float duration = -1f)
        {
            if (_isTransitioning) return;
            float dur = duration > 0f ? duration : defaultDuration;
            StartCoroutine(TransitionCoroutine(onMidTransition, dur));
        }

        /// <summary>
        /// Simple fade to black and stay.
        /// </summary>
        public void FadeOut(float duration = -1f)
        {
            float dur = duration > 0f ? duration : defaultDuration;
            StartCoroutine(FadeCoroutine(0f, 1f, dur, null));
        }

        /// <summary>
        /// Fade from black to clear.
        /// </summary>
        public void FadeIn(float duration = -1f, Action onComplete = null)
        {
            float dur = duration > 0f ? duration : defaultDuration;
            StartCoroutine(FadeCoroutine(1f, 0f, dur, onComplete));
        }

        private IEnumerator TransitionCoroutine(Action onMidTransition, float duration)
        {
            _isTransitioning = true;

            // Fade out
            yield return StartCoroutine(FadeCoroutine(0f, 1f, duration * 0.5f, null));

            // Execute callback
            onMidTransition?.Invoke();

            // Fade in
            yield return StartCoroutine(FadeCoroutine(1f, 0f, duration * 0.5f, null));

            _isTransitioning = false;
        }

        private IEnumerator FadeCoroutine(float fromAlpha, float toAlpha, float duration, Action onComplete)
        {
            if (fadeOverlay == null) yield break;

            float elapsed = 0f;
            Color color = fadeOverlay.color;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = fadeCurve.Evaluate(Mathf.Clamp01(elapsed / duration));
                color.a = Mathf.Lerp(fromAlpha, toAlpha, t);
                fadeOverlay.color = color;
                yield return null;
            }

            color.a = toAlpha;
            fadeOverlay.color = color;

            onComplete?.Invoke();
        }

        private void EnsureOverlayExists()
        {
            if (fadeOverlay != null) return;

            // Create overlay canvas
            var canvasGO = new GameObject("TransitionCanvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 9999; // On top of everything

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            canvasGO.AddComponent<GraphicRaycaster>();

            // Create black overlay image
            var overlayGO = new GameObject("FadeOverlay");
            overlayGO.transform.SetParent(canvasGO.transform);
            var rectTransform = overlayGO.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            fadeOverlay = overlayGO.AddComponent<Image>();
            fadeOverlay.color = new Color(0f, 0f, 0f, 0f);
            fadeOverlay.raycastTarget = false;
        }
    }
}
