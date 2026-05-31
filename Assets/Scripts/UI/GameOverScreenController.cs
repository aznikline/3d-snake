using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonSerpent.Core;
using NeonSerpent.Level;
using NeonSerpent.Progression;

namespace NeonSerpent.UI
{
    public class GameOverScreenController : MonoBehaviour
    {
        [Header("Panels")]
        public GameObject campaignPanel;

        [Header("Campaign Mode UI")]
        [SerializeField] private TextMeshProUGUI campaignLevelText;
        [SerializeField] private TextMeshProUGUI campaignDeathsText;
        [SerializeField] private TextMeshProUGUI campaignResultText;

        [Header("Buttons")]
        public Button retryButton;
        public Button backToMenuButton;

        [Header("Animation")]
        [SerializeField] private float revealDuration = 0.5f;
        [SerializeField] private float textRevealStagger = 0.1f;

        private CanvasGroup _canvasGroup;

        private void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            HideAllPanels();
            WireButtons();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void WireButtons()
        {
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetry);
            if (backToMenuButton != null)
                backToMenuButton.onClick.AddListener(OnBackToMenu);
        }

        private void SubscribeToEvents()
        {
            var levelManager = FindObjectOfType<LevelManager>();
            if (levelManager != null)
                levelManager.OnLevelFailed += ShowCampaignGameOver;
        }

        private void UnsubscribeFromEvents()
        {
            var levelManager = FindObjectOfType<LevelManager>();
            if (levelManager != null)
                levelManager.OnLevelFailed -= ShowCampaignGameOver;
        }

        public void ShowCampaignGameOver()
        {
            if (GameStateManager.Instance.CurrentMode != GameMode.Campaign) return;

            var levelManager = FindObjectOfType<LevelManager>();
            if (levelManager == null) return;

            if (campaignPanel != null) campaignPanel.SetActive(true);

            if (campaignLevelText != null)
            {
                campaignLevelText.text = levelManager.CurrentLevel?.displayName ?? "Unknown";
                campaignLevelText.alpha = 0f;
            }

            if (campaignDeathsText != null)
            {
                campaignDeathsText.text = "Lives Exhausted";
                campaignDeathsText.alpha = 0f;
            }

            if (campaignResultText != null)
            {
                campaignResultText.text = "MISSION FAILED";
                campaignResultText.color = new Color(1f, 0.3f, 0.2f);
                campaignResultText.alpha = 0f;
            }

            gameObject.SetActive(true);
            StartCoroutine(RevealSequence());
        }

        private IEnumerator RevealSequence()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                float elapsed = 0f;
                while (elapsed < revealDuration * 0.5f)
                {
                    elapsed += Time.unscaledDeltaTime;
                    _canvasGroup.alpha = elapsed / (revealDuration * 0.5f);
                    yield return null;
                }
                _canvasGroup.alpha = 1f;
            }

            var activePanel = campaignPanel;
            if (activePanel == null) yield break;

            var allTexts = activePanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in allTexts)
            {
                StartCoroutine(FadeInText(text, textRevealStagger));
            }
        }

        private IEnumerator FadeInText(TextMeshProUGUI text, float duration)
        {
            text.alpha = 0f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                text.alpha = elapsed / duration;
                yield return null;
            }
            text.alpha = 1f;
        }

        private void OnRetry()
        {
            HideAllPanels();

            var levelManager = FindObjectOfType<LevelManager>();
            levelManager?.RestartLevel();
        }

        private void OnBackToMenu()
        {
            HideAllPanels();

            var bootstrap = FindObjectOfType<GameBootstrap>();
            if (bootstrap != null)
            {
                bootstrap.ReturnToMenu();
            }

            GameStateManager.Instance.ChangeState(GameState.MainMenu);
        }

        private void HideAllPanels()
        {
            if (campaignPanel != null) campaignPanel.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
