using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonSerpent.Core;
using NeonSerpent.Level;

namespace NeonSerpent.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject pausePanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Buttons")]
        public Button resumeButton;
        public Button restartButton;
        public Button settingsButton;
        public Button quitToMenuButton;
        public Button quitGameButton;
        [SerializeField] private Button settingsBackButton;

        [Header("Info")]
        [SerializeField] private TextMeshProUGUI modeText;
        [SerializeField] private TextMeshProUGUI levelText;

        [Header("Animation")]
        [SerializeField] private float slideDuration = 0.3f;
        [SerializeField] private float slideOffset = 50f;

        [Header("Background Overlay")]
        [SerializeField] private Image backgroundOverlay;

        [Header("Settings Reference")]
        [SerializeField] private SettingsMenuController settingsController;

        private bool _isPaused;
        private float _prePauseTimeScale = 1f;
        private CanvasGroup _canvasGroup;
        private RectTransform _panelRect;
        private Vector2 _panelOriginalPos;
        private Coroutine _animRoutine;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            if (pausePanel != null)
            {
                _panelRect = pausePanel.GetComponent<RectTransform>();
                _panelOriginalPos = _panelRect != null ? _panelRect.anchoredPosition : Vector2.zero;
            }

            // Create background overlay if not set
            if (backgroundOverlay == null && pausePanel != null)
            {
                var overlayGO = new GameObject("PauseOverlay");
                overlayGO.transform.SetParent(transform);
                overlayGO.transform.SetAsFirstSibling();
                var overlayRect = overlayGO.AddComponent<RectTransform>();
                overlayRect.anchorMin = Vector2.zero;
                overlayRect.anchorMax = Vector2.one;
                overlayRect.offsetMin = Vector2.zero;
                overlayRect.offsetMax = Vector2.zero;
                backgroundOverlay = overlayGO.AddComponent<Image>();
                backgroundOverlay.color = new Color(0f, 0f, 0f, 0.5f);
                backgroundOverlay.raycastTarget = true;
            }

            WireButtons();
            HideAllPanelsInstant();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
            {
                TogglePause();
            }
        }

        private void WireButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(ResumeGame);
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettings);
            if (quitToMenuButton != null)
                quitToMenuButton.onClick.AddListener(QuitToMenu);
            if (quitGameButton != null)
                quitGameButton.onClick.AddListener(QuitGame);
            if (settingsBackButton != null)
                settingsBackButton.onClick.AddListener(CloseSettings);
        }

        public void TogglePause()
        {
            if (_isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        public void PauseGame()
        {
            if (!GameStateManager.Instance.IsPlaying) return;

            _isPaused = true;
            _prePauseTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            GameStateManager.Instance.ChangeState(GameState.Paused);

            ShowPausePanel();
            UpdatePauseInfo();
            AnimateIn();
        }

        public void ResumeGame()
        {
            StartCoroutine(ResumeSequence());
        }

        private IEnumerator ResumeSequence()
        {
            yield return AnimateOut();

            _isPaused = false;
            Time.timeScale = _prePauseTimeScale;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            GameStateManager.Instance.ChangeState(GameState.Playing);
            HideAllPanelsInstant();
        }

        public void RestartGame()
        {
            ResumeGame();

            var levelManager = FindObjectOfType<LevelManager>();
            levelManager?.RestartLevel();
        }

        public void QuitToMenu()
        {
            ResumeGame();

            var mainMenu = FindObjectOfType<MainMenuController>();
            if (mainMenu != null)
                mainMenu.gameObject.SetActive(true);

            var bootstrap = FindObjectOfType<GameBootstrap>();
            if (bootstrap != null)
                bootstrap.ReturnToMenu();

            GameStateManager.Instance.ChangeState(GameState.MainMenu);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        // ── Animation ──

        private void AnimateIn()
        {
            if (_animRoutine != null) StopCoroutine(_animRoutine);
            _animRoutine = StartCoroutine(SlideIn());
        }

        private IEnumerator AnimateOut()
        {
            if (_animRoutine != null) StopCoroutine(_animRoutine);
            yield return _animRoutine = StartCoroutine(SlideOut());
        }

        private IEnumerator SlideIn()
        {
            if (_panelRect != null)
            {
                _panelRect.anchoredPosition = _panelOriginalPos + new Vector2(0f, slideOffset);
            }
            if (backgroundOverlay != null)
                backgroundOverlay.gameObject.SetActive(true);

            if (_canvasGroup != null) _canvasGroup.alpha = 0f;
            pausePanel?.SetActive(true);

            float elapsed = 0f;
            while (elapsed < slideDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = EaseOut(elapsed / slideDuration);

                if (_panelRect != null)
                    _panelRect.anchoredPosition = Vector2.Lerp(
                        _panelOriginalPos + new Vector2(0f, slideOffset), _panelOriginalPos, t);

                if (_canvasGroup != null)
                    _canvasGroup.alpha = t;

                yield return null;
            }

            if (_panelRect != null)
                _panelRect.anchoredPosition = _panelOriginalPos;
            if (_canvasGroup != null) _canvasGroup.alpha = 1f;
        }

        private IEnumerator SlideOut()
        {
            float elapsed = 0f;
            while (elapsed < slideDuration * 0.6f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = elapsed / (slideDuration * 0.6f);

                if (_panelRect != null)
                    _panelRect.anchoredPosition = Vector2.Lerp(
                        _panelOriginalPos, _panelOriginalPos + new Vector2(0f, slideOffset), t);

                if (_canvasGroup != null)
                    _canvasGroup.alpha = 1f - t;

                yield return null;
            }
        }

        private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);

        // ── Panel Management ──

        private void OpenSettings()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
            settingsController?.LoadCurrentSettings();
        }

        private void CloseSettings()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        private void ShowPausePanel()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
        }

        private void HideAllPanelsInstant()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (backgroundOverlay != null) backgroundOverlay.gameObject.SetActive(false);
        }

        private void UpdatePauseInfo()
        {
            var mode = GameStateManager.Instance.CurrentMode;
            if (modeText != null)
                modeText.text = mode.ToString();

            var levelManager = FindObjectOfType<LevelManager>();
            if (levelText != null && levelManager?.CurrentLevel != null)
                levelText.text = levelManager.CurrentLevel.displayName;
        }

        private void OnDestroy()
        {
            if (_isPaused)
                Time.timeScale = 1f;
        }
    }
}
