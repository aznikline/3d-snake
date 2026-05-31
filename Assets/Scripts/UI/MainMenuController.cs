using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonSerpent.Core;
using NeonSerpent.Level;
using NeonSerpent.Progression;

namespace NeonSerpent.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Panels")]
        public GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;

        [Header("Buttons")]
        public Button campaignButton;
        public Button settingsButton;
        public Button quitButton;

        [Header("Title")]
        [SerializeField] private TextMeshProUGUI titleText;

        [Header("Info Text")]
        public TextMeshProUGUI infoText;

        [Header("Settings")]
        public GameBootstrap gameBootstrap;

        [Header("Transition")]
        [SerializeField] private float panelFadeDuration = 0.3f;

        private Canvas _canvas;
        private CanvasGroup _canvasGroup;

        private void Awake()
        {
            _canvas = GetComponentInParent<Canvas>();
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            GameStateManager.Instance.ChangeState(GameState.MainMenu);

            WireButtons();
            ShowMainPanel();
        }

        private void WireButtons()
        {
            if (campaignButton != null)
                campaignButton.onClick.AddListener(StartCampaign);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettings);
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
        }

        private void ShowMainPanel()
        {
            SetActivePanel(mainPanel);
        }

        private void SetActivePanel(GameObject panel)
        {
            StartCoroutine(SwitchPanel(panel));
        }

        private IEnumerator SwitchPanel(GameObject targetPanel)
        {
            if (_canvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < panelFadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _canvasGroup.alpha = 1f - elapsed / panelFadeDuration;
                    yield return null;
                }
            }

            if (mainPanel != null) mainPanel.SetActive(targetPanel == mainPanel);
            if (settingsPanel != null) settingsPanel.SetActive(targetPanel == settingsPanel);

            if (_canvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < panelFadeDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _canvasGroup.alpha = elapsed / panelFadeDuration;
                    yield return null;
                }
                _canvasGroup.alpha = 1f;
            }
        }

        public void StartCampaign()
        {
            StartCoroutine(LaunchMode(GameMode.Campaign));
        }

        private IEnumerator LaunchMode(GameMode mode)
        {
            if (_canvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < 0.5f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _canvasGroup.alpha = 1f - elapsed / 0.5f;
                    yield return null;
                }
            }

            if (gameBootstrap != null)
            {
                gameBootstrap.StartCampaignMode();
            }
            else
            {
                GameStateManager.Instance.SetGameMode(GameMode.Campaign);
                GameStateManager.Instance.ChangeState(GameState.Playing);
            }

            if (_canvas != null)
                _canvas.gameObject.SetActive(false);
            else
                gameObject.SetActive(false);
        }

        public void OpenSettings()
        {
            SetActivePanel(settingsPanel);
        }

        public void BackToMain()
        {
            ShowMainPanel();
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void SetInfo(string text)
        {
            if (infoText != null)
                infoText.text = text;
        }
    }
}
