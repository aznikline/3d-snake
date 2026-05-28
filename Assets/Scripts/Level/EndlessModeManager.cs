using UnityEngine;
using NeonSerpent.Core;
using NeonSerpent.Player;
using NeonSerpent.Gameplay;
using NeonSerpent.Progression;

namespace NeonSerpent.Level
{
    /// <summary>
    /// Manages Endless mode gameplay. Tracks high scores, handles
    /// procedural generation, and submits scores to leaderboards.
    /// </summary>
    public class EndlessModeManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ProceduralGenerator generator;
        [SerializeField] private SnakeHeadController snakeController;
        [SerializeField] private VerletSnakeBody snakeBody;
        [SerializeField] private ComboSystem comboSystem;
        [SerializeField] private DeathManager deathManager;
        [SerializeField] private FoodSpawner foodSpawner;

        [Header("UI")]
        [SerializeField] private GameObject endlessHUD;

        [Header("Events")]
        public System.Action<int, int> OnScoreUpdated; // currentScore, highScore
        public System.Action<int> OnDistanceUpdated;   // distance in meters
        public System.Action OnEndlessRunEnded;

        public bool IsRunning => _isRunning;
        public int CurrentScore => _currentScore;
        public int HighScore => _highScore;
        public float CurrentDistance => _currentDistance;

        private bool _isRunning;
        private int _currentScore;
        private int _highScore;
        private float _currentDistance;
        private float _startZ;
        private int _foodEaten;
        private int _maxLength;

        private void Awake()
        {
            LoadHighScore();
        }

        private void Update()
        {
            if (!_isRunning) return;

            UpdateDistance();
            UpdateScore();
            UpdateDifficulty();
        }

        /// <summary>
        /// Start a new endless run.
        /// </summary>
        public void StartRun()
        {
            _isRunning = true;
            _currentScore = 0;
            _currentDistance = 0f;
            _foodEaten = 0;
            _maxLength = GameConstants.InitialSegmentCount;

            // Reset player
            Vector3 startPos = Vector3.zero;
            snakeController.transform.position = startPos;
            snakeBody.ResetBody(startPos);
            comboSystem?.Reset();

            // Reset generator
            generator?.ResetGenerator();

            // Configure food spawner for endless
            if (foodSpawner != null)
            {
                // foodSpawner.SetEndlessMode(true);
            }

            _startZ = startPos.z;

            GameStateManager.Instance.SetGameMode(GameMode.Endless);
            GameStateManager.Instance.ChangeState(GameState.Playing);

            if (endlessHUD != null)
                endlessHUD.SetActive(true);

            Debug.Log("[EndlessModeManager] Endless run started.");
        }

        /// <summary>
        /// End the current run (called on death).
        /// </summary>
        public void EndRun()
        {
            if (!_isRunning) return;

            _isRunning = false;

            // Update high score
            if (_currentScore > _highScore)
            {
                _highScore = _currentScore;
                SaveHighScore();
            }

            // Submit to leaderboard
            SubmitScore();

            // Save stats
            SaveRunStats();

            GameStateManager.Instance.ChangeState(GameState.GameOver);

            if (endlessHUD != null)
                endlessHUD.SetActive(false);

            OnEndlessRunEnded?.Invoke();

            Debug.Log($"[EndlessModeManager] Run ended. Score: {_currentScore} | Distance: {_currentDistance:F0}m | Max Length: {_maxLength}");
        }

        /// <summary>
        /// Called when food is eaten.
        /// </summary>
        public void OnFoodEaten()
        {
            _foodEaten++;
            _currentScore += GameConstants.ScorePerFood;

            // Bonus for length
            int lengthBonus = snakeBody.NodeCount * 10;
            _currentScore += lengthBonus;

            // Update max length
            _maxLength = Mathf.Max(_maxLength, snakeBody.NodeCount);
        }

        /// <summary>
        /// Called when combo milestone is reached.
        /// </summary>
        public void OnComboMilestone(int comboCount)
        {
            _currentScore += comboCount * GameConstants.ScorePerFoodCombo;
        }

        private void UpdateDistance()
        {
            _currentDistance = snakeController.transform.position.z - _startZ;
            OnDistanceUpdated?.Invoke(Mathf.RoundToInt(_currentDistance));
        }

        private void UpdateScore()
        {
            OnScoreUpdated?.Invoke(_currentScore, _highScore);
        }

        private void UpdateDifficulty()
        {
            // Difficulty already handled by ProceduralGenerator based on snake length
            // Additional endless-specific difficulty can be added here
        }

        private void SubmitScore()
        {
            // TODO: Submit to Steam leaderboard
            // LeaderboardManager.Instance?.SubmitScore(_currentScore);
            Debug.Log($"[EndlessModeManager] Submitting score: {_currentScore}");
        }

        private void SaveHighScore()
        {
            PlayerPrefs.SetInt("EndlessHighScore", _highScore);
            PlayerPrefs.Save();
        }

        private void LoadHighScore()
        {
            _highScore = PlayerPrefs.GetInt("EndlessHighScore", 0);
        }

        private void SaveRunStats()
        {
            var saveData = SaveSystem.Instance?.GetProgressData();
            if (saveData != null)
            {
                saveData.totalPlayTime += Time.time;
                SaveSystem.Instance.SaveProgress();
            }
        }

        private void OnEnable()
        {
            if (deathManager != null)
                deathManager.OnDeath += OnPlayerDeath;
        }

        private void OnDisable()
        {
            if (deathManager != null)
                deathManager.OnDeath -= OnPlayerDeath;
        }

        private void OnPlayerDeath()
        {
            // In endless mode, death ends the run immediately
            if (GameStateManager.Instance.CurrentMode == GameMode.Endless)
            {
                EndRun();
            }
        }
    }
}
