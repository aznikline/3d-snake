using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeonSerpent.Core;
using NeonSerpent.Player;
using NeonSerpent.Gameplay;
using NeonSerpent.Audio;
using NeonSerpent.Progression;

namespace NeonSerpent.Level
{
    /// <summary>
    /// Manages level lifecycle: loading, initialization, completion detection,
    /// and transition to next level. Works with LevelData ScriptableObjects.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        [Header("Level Data")]
        [SerializeField] private List<LevelData> allLevels = new List<LevelData>();
        [SerializeField] private LevelData currentLevel;

        [Header("Player References")]
        public SnakeHeadController snakeController;
        public VerletSnakeBody snakeBody;
        public ComboSystem comboSystem;

        [Header("Systems")]
        public FoodSpawner foodSpawner;
        public DeathManager deathManager;
        public MusicManager musicManager;

        [Header("Events")]
        public System.Action<LevelData> OnLevelStarted;
        public System.Action<LevelData, Rank, int> OnLevelCompleted; // level, rank, score
        public System.Action OnLevelFailed;
        public System.Action OnCampaignComplete;

        public LevelData CurrentLevel => currentLevel;
        public bool IsLevelActive => _isLevelActive;

        private bool _isLevelActive;
        private float _levelStartTime;
        private int _deathCount;
        private int _foodCollected;
        private int _totalFoodSpawned;
        private bool _hasCompleted;

        private void OnEnable()
        {
        }

        private void OnDisable()
        {
            if (deathManager != null)
                deathManager.OnDeath -= OnPlayerDeath;
        }

        private void SubscribeDeathManager()
        {
            if (deathManager == null) return;

            deathManager.OnDeath -= OnPlayerDeath;
            deathManager.OnDeath += OnPlayerDeath;
        }

        private void Update()
        {
            if (!_isLevelActive || _hasCompleted) return;

            CheckLevelCompletion();
        }

        /// <summary>
        /// Load and start a specific level.
        /// </summary>
        public void LoadLevel(LevelData level)
        {
            if (level == null)
            {
                Debug.LogError("[LevelManager] Cannot load null level.");
                return;
            }

            currentLevel = level;
            _isLevelActive = true;
            _hasCompleted = false;
            _levelStartTime = Time.time;
            _deathCount = 0;
            _foodCollected = 0;
            _totalFoodSpawned = 0;

            ConfigureLevelGameplay(level);

            SubscribeDeathManager();
            deathManager?.ResetGracePeriod();
            Debug.Log($"[LevelManager] Reset grace period before changing state to Playing");

            ResetPlayer(level);

            GameStateManager.Instance.ChangeState(GameState.Playing);
            Debug.Log($"[LevelManager] State changed to Playing");
            OnLevelStarted?.Invoke(level);

            foodSpawner?.ResetSpawner();
            foodSpawner?.Initialize();

            Debug.Log($"[LevelManager] Started: {level.FullLevelName}");
        }

        /// <summary>
        /// Load level by ID string.
        /// </summary>
        public void LoadLevel(string levelId)
        {
            var level = allLevels.Find(l => l.levelId == levelId);
            if (level != null)
            {
                LoadLevel(level);
            }
            else
            {
                Debug.LogError($"[LevelManager] Level not found: {levelId}");
            }
        }

        /// <summary>
        /// Restart the current level.
        /// </summary>
        public void RestartLevel()
        {
            if (currentLevel != null)
            {
                LoadLevel(currentLevel);
            }
        }

        /// <summary>
        /// Load the next level in sequence.
        /// </summary>
        public void LoadNextLevel()
        {
            if (currentLevel == null) return;

            int currentIndex = allLevels.IndexOf(currentLevel);
            if (currentIndex >= 0 && currentIndex < allLevels.Count - 1)
            {
                LoadLevel(allLevels[currentIndex + 1]);
            }
            else
            {
                Debug.Log("[LevelManager] All levels completed. Campaign finished!");
                GameStateManager.Instance.ChangeState(GameState.MainMenu);
                OnCampaignComplete?.Invoke();
            }
        }

        /// <summary>
        /// Called when food is collected.
        /// </summary>
        public void OnFoodCollected()
        {
            _foodCollected++;
        }

        /// <summary>
        /// Called when food is spawned.
        /// </summary>
        public void OnFoodSpawned()
        {
            _totalFoodSpawned++;
        }

        private void ConfigureLevelGameplay(LevelData level)
        {
            // Enable/disable abilities based on level
            var wallRun = snakeController.GetComponent<WallRunAbility>();
            if (wallRun != null) wallRun.enabled = level.enableWallRun;

            var slide = snakeController.GetComponent<SlideAbility>();
            if (slide != null) slide.enabled = level.enableSlide;

            var grapple = snakeController.GetComponent<GrappleAbility>();
            if (grapple != null) grapple.enabled = level.enableGrapple;

            // Configure combo system
            if (comboSystem != null)
            {
                comboSystem.enabled = level.enableDash;
            }

            // Set music theme
            if (musicManager != null)
                musicManager.SetIntensity(level.musicTheme == MusicTheme.Ambient ? 0f : 0.5f);
        }

        private void ResetPlayer(LevelData level)
        {
            // Find player by component or use default position
            Vector3 startPos = Vector3.zero;
            var existingPlayer = FindObjectOfType<NeonSerpent.Player.SnakeHeadController>();
            if (existingPlayer != null)
                startPos = existingPlayer.transform.position;

            snakeController.transform.position = startPos;
            snakeController.transform.rotation = Quaternion.identity;
            snakeBody.ResetBody(startPos);
            comboSystem?.Reset();
        }

        private void LoadLevelGeometry(LevelData level)
        {
            if (level.geometryJson == null) return;

            // Parse JSON and instantiate geometry
            // This is a placeholder - actual implementation depends on geometry format
            Debug.Log($"[LevelManager] Loading geometry for {level.levelId}");
        }

        private void CheckLevelCompletion()
        {
            if (currentLevel == null) return;
            if (snakeBody == null) return;

            float elapsed = Time.time - _levelStartTime;
            if (elapsed < 2f) return;

            if (_foodCollected > 0 && snakeBody.NodeCount >= currentLevel.targetLength)
            {
                CompleteLevel();
            }
        }

        private void CompleteLevel()
        {
            _hasCompleted = true;
            _isLevelActive = false;

            float completionTime = Time.time - _levelStartTime;
            float collectRate = _totalFoodSpawned > 0 ? (float)_foodCollected / _totalFoodSpawned : 0f;

            Rank rank = LevelScoring.CalculateRank(currentLevel, completionTime, _deathCount, collectRate);
            int score = LevelScoring.CalculateScore(currentLevel, rank, completionTime, _deathCount, collectRate);

            // Save completion record
            SaveSystem.Instance?.RecordLevelCompletion(currentLevel.levelId, rank, score, completionTime);

            GameStateManager.Instance.ChangeState(GameState.LevelComplete);

            OnLevelCompleted?.Invoke(currentLevel, rank, score);

            Debug.Log($"[LevelManager] Completed: {currentLevel.FullLevelName} | Rank: {rank} | Score: {score} | Time: {completionTime:F1}s | Deaths: {_deathCount}");
        }

        private void OnPlayerDeath()
        {
            _deathCount++;

            // Check if max deaths exceeded
            if (_deathCount > currentLevel?.maxDeaths)
            {
                FailLevel();
            }
        }

        private void FailLevel()
        {
            _isLevelActive = false;
            GameStateManager.Instance.ChangeState(GameState.GameOver);
            OnLevelFailed?.Invoke();
        }

        /// <summary>
        /// Get all levels in a specific zone.
        /// </summary>
        public List<LevelData> GetLevelsInZone(ZoneType zone)
        {
            return allLevels.FindAll(l => l.zone == zone);
        }

        /// <summary>
        /// Get total level count.
        /// </summary>
        public int TotalLevelCount => allLevels.Count;
    }
}
