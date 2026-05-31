using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonSerpent.Core;
using NeonSerpent.Level;
using NeonSerpent.Progression;

namespace NeonSerpent.UI
{
    /// <summary>
    /// Level complete screen showing rank, score, time, and deaths.
    /// Allows progression to next level, retry, or return to menu.
    /// </summary>
    public class LevelCompleteScreenController : MonoBehaviour
    {
        [Header("Display")]
        [SerializeField] private TextMeshProUGUI levelNameText;
        public TextMeshProUGUI rankText;
        public TextMeshProUGUI timeText;
        [SerializeField] private TextMeshProUGUI deathsText;
        public TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI newRecordText;
        [SerializeField] private Image rankBackground;

        [Header("Rank Colors")]
        [SerializeField] private Color sRankColor = new Color(1f, 0.84f, 0f);
        [SerializeField] private Color aRankColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private Color bRankColor = new Color(0.8f, 0.5f, 0.2f);
        [SerializeField] private Color cRankColor = Color.green;

        [Header("Buttons")]
        public Button nextLevelButton;
        [SerializeField] private Button retryButton;
        public Button backToMenuButton;

        [Header("Unlock Display")]
        [SerializeField] private GameObject unlockPanel;
        [SerializeField] private TextMeshProUGUI unlockText;

        private LevelData _completedLevel;
        private Rank _achievedRank;
        private int _score;

        private void Start()
        {
            HideScreen();
            WireButtons();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void WireButtons()
        {
            if (nextLevelButton != null)
                nextLevelButton.onClick.AddListener(OnNextLevel);
            if (retryButton != null)
                retryButton.onClick.AddListener(OnRetry);
            if (backToMenuButton != null)
                backToMenuButton.onClick.AddListener(OnBackToMenu);
        }

        private void SubscribeToEvents()
        {
            var levelManager = FindObjectOfType<LevelManager>();
            if (levelManager != null)
                levelManager.OnLevelCompleted += OnLevelCompleted;
        }

        private void UnsubscribeFromEvents()
        {
            var levelManager = FindObjectOfType<LevelManager>();
            if (levelManager != null)
                levelManager.OnLevelCompleted -= OnLevelCompleted;
        }

        private void OnLevelCompleted(LevelData level, Rank rank, int score)
        {
            _completedLevel = level;
            _achievedRank = rank;
            _score = score;

            ShowScreen(level, rank, score);
            CheckUnlocks(level, rank);
        }

        /// <summary>
        /// Display the level complete screen with all stats.
        /// </summary>
        public void ShowScreen(LevelData level, Rank rank, int score)
        {
            gameObject.SetActive(true);

            if (levelNameText != null)
                levelNameText.text = level?.displayName ?? "Level Complete";

            if (rankText != null)
            {
                rankText.text = rank.ToString();
                rankText.color = GetRankColor(rank);
            }

            if (rankBackground != null)
                rankBackground.color = GetRankColor(rank);

            // Calculate stats from save data
            var saveSystem = SaveSystem.Instance;
            var record = saveSystem?.GetLevelRecord(level?.levelId);

            if (timeText != null)
            {
                float bestTime = record?.bestTime ?? 0f;
                if (bestTime > 0f && bestTime < float.MaxValue)
                    timeText.text = $"Best Time: {bestTime:F1}s";
                else
                    timeText.text = "Time: --";
            }

            if (deathsText != null)
                deathsText.text = "Deaths: --"; // Death count not tracked per-completion in current system

            if (scoreText != null)
                scoreText.text = $"Score: {score:N0}";

            if (newRecordText != null)
            {
                bool isNewRecord = record != null && score >= record.bestScore;
                newRecordText.gameObject.SetActive(isNewRecord);
            }

            // Disable next level button if this was the last level
            if (nextLevelButton != null)
            {
                var levelManager = FindObjectOfType<LevelManager>();
                bool hasNext = levelManager != null && level != null &&
                    levelManager.GetLevelsInZone(level.zone).FindIndex(l => l == level) <
                    levelManager.GetLevelsInZone(level.zone).Count - 1;
                nextLevelButton.interactable = hasNext;
            }
        }

        private void CheckUnlocks(LevelData level, Rank rank)
        {
            // Check if any unlocks were triggered
            var unlockSystem = FindObjectOfType<UnlockSystem>();
            if (unlockSystem == null) return;

            // UnlockSystem fires events that UnlockNotificationUI handles
            // This screen just shows a summary if something was unlocked
            // Actual unlock check is done by UnlockSystem on level complete
        }

        private void OnNextLevel()
        {
            HideScreen();

            var levelManager = FindObjectOfType<LevelManager>();
            levelManager?.LoadNextLevel();
        }

        private void OnRetry()
        {
            HideScreen();

            var levelManager = FindObjectOfType<LevelManager>();
            levelManager?.RestartLevel();
        }

        private void OnBackToMenu()
        {
            HideScreen();

            var bootstrap = FindObjectOfType<GameBootstrap>();
            if (bootstrap != null)
            {
                bootstrap.ReturnToMenu();
            }

            GameStateManager.Instance.ChangeState(GameState.MainMenu);
        }

        private void HideScreen()
        {
            gameObject.SetActive(false);
            if (unlockPanel != null) unlockPanel.SetActive(false);
        }

        private Color GetRankColor(Rank rank)
        {
            return rank switch
            {
                Rank.S => sRankColor,
                Rank.A => aRankColor,
                Rank.B => bRankColor,
                Rank.C => cRankColor,
                _ => Color.white
            };
        }
    }
}
