#if UNITY_STANDALONE
using System;
using System.Collections.Generic;
using UnityEngine;
using NeonSerpent.Core;
using NeonSerpent.Level;
using NeonSerpent.Progression;

namespace NeonSerpent.Steam
{
    /// <summary>
    /// Manages Steam achievements. Defines achievement conditions and
    /// triggers unlocks based on gameplay events.
    /// </summary>
    public class AchievementManager : MonoBehaviour
    {
        public static AchievementManager Instance { get; private set; }

        [Header("Achievements")]
        [SerializeField] private List<AchievementDefinition> achievements = new List<AchievementDefinition>();

        [Header("Events")]
        public System.Action<AchievementDefinition> OnAchievementUnlocked;

        public bool IsInitialized => _isInitialized;

        private bool _isInitialized;
        private HashSet<string> _unlockedAchievements = new HashSet<string>();
        private Dictionary<string, int> _achievementProgress = new Dictionary<string, int>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeAchievements();
        }

        /// <summary>
        /// Initialize achievements from Steam and local cache.
        /// </summary>
        public void InitializeAchievements()
        {
            if (_isInitialized) return;

            // TODO: Load achievement progress from Steam
            // SteamUserStats.RequestCurrentStats();

            // Load from local save
            var saveData = SaveSystem.Instance?.GetProgressData();
            if (saveData != null)
            {
                // _unlockedAchievements = new HashSet<string>(saveData.unlockedAchievements);
            }

            _isInitialized = true;
            Debug.Log($"[AchievementManager] Initialized with {achievements.Count} achievements.");
        }

        /// <summary>
        /// Unlock an achievement by ID.
        /// </summary>
        public void UnlockAchievement(string achievementId)
        {
            if (!_isInitialized) return;
            if (_unlockedAchievements.Contains(achievementId)) return;

            var achievement = achievements.Find(a => a.achievementId == achievementId);
            if (achievement == null)
            {
                Debug.LogWarning($"[AchievementManager] Achievement not found: {achievementId}");
                return;
            }

            _unlockedAchievements.Add(achievementId);

            // TODO: SteamUserStats.SetAchievement(achievementId);
            // TODO: SteamUserStats.StoreStats();

            // Save locally
            var saveData = SaveSystem.Instance?.GetProgressData();
            if (saveData != null)
            {
                // saveData.unlockedAchievements.Add(achievementId);
                SaveSystem.Instance.SaveProgress();
            }

            OnAchievementUnlocked?.Invoke(achievement);
            Debug.Log($"[AchievementManager] Achievement unlocked: {achievement.displayName}");
        }

        /// <summary>
        /// Update progress for a progressive achievement.
        /// </summary>
        public void UpdateProgress(string achievementId, int currentProgress)
        {
            if (!_isInitialized) return;

            var achievement = achievements.Find(a => a.achievementId == achievementId);
            if (achievement == null || !achievement.isProgressive) return;

            _achievementProgress[achievementId] = currentProgress;

            if (currentProgress >= achievement.targetProgress)
            {
                UnlockAchievement(achievementId);
            }
            else
            {
                // TODO: SteamUserStats.SetStat(achievement.statName, currentProgress);
            }
        }

        /// <summary>
        /// Check if an achievement is unlocked.
        /// </summary>
        public bool IsUnlocked(string achievementId)
        {
            return _unlockedAchievements.Contains(achievementId);
        }

        /// <summary>
        /// Get progress for a progressive achievement.
        /// </summary>
        public int GetProgress(string achievementId)
        {
            return _achievementProgress.GetValueOrDefault(achievementId, 0);
        }

        // ── Event Handlers ──

        /// <summary>
        /// Call when a level is completed.
        /// </summary>
        public void OnLevelCompleted(LevelData level, Rank rank)
        {
            // First completion achievements
            if (level.zone == ZoneType.NeonCity && level.zoneOrder == 1)
            {
                UnlockAchievement("ACH_FIRST_LEVEL");
            }

            // Zone completion achievements
            if (level.zoneOrder == level.zone.LevelCount())
            {
                UnlockAchievement($"ACH_ZONE_{level.zone}");
            }

            // Rank achievements
            if (rank == Rank.S)
            {
                UnlockAchievement("ACH_FIRST_S_RANK");
                UpdateProgress("ACH_S_RANK_MASTER", CountSRanks());
            }

            // Campaign completion
            if (level.zone == ZoneType.VoidEdge && level.zoneOrder == 15)
            {
                UnlockAchievement("ACH_CAMPAIGN_COMPLETE");
            }
        }

        /// <summary>
        /// Call when player dies.
        /// </summary>
        public void OnPlayerDeath()
        {
            UpdateProgress("ACH_DEATH_COUNT", GetProgress("ACH_DEATH_COUNT") + 1);
        }

        /// <summary>
        /// Call when food is eaten.
        /// </summary>
        public void OnFoodEaten(int totalFoodEaten)
        {
            UpdateProgress("ACH_FOOD_COLLECTOR", totalFoodEaten);
        }

        /// <summary>
        /// Call when a combo milestone is reached.
        /// </summary>
        public void OnComboMilestone(int comboCount)
        {
            if (comboCount >= 10)
                UnlockAchievement("ACH_COMBO_10");
            if (comboCount >= 50)
                UnlockAchievement("ACH_COMBO_50");

            UpdateProgress("ACH_MAX_COMBO", comboCount);
        }

        /// <summary>
        /// Call when endless mode score is updated.
        /// </summary>
        public void OnEndlessScore(int score)
        {
            if (score >= 10000)
                UnlockAchievement("ACH_ENDLESS_10K");
            if (score >= 50000)
                UnlockAchievement("ACH_ENDLESS_50K");
            if (score >= 100000)
                UnlockAchievement("ACH_ENDLESS_100K");
        }

        /// <summary>
        /// Call when a challenge is completed.
        /// </summary>
        public void OnChallengeCompleted(string ruleId)
        {
            UpdateProgress("ACH_CHALLENGE_MASTER", GetProgress("ACH_CHALLENGE_MASTER") + 1);
        }

        private int CountSRanks()
        {
            // Count total S ranks from save data
            int count = 0;
            var saveData = SaveSystem.Instance?.GetProgressData();
            if (saveData != null)
            {
                foreach (var record in saveData.levelRecords)
                {
                    if (record.bestRank == Rank.S)
                        count++;
                }
            }
            return count;
        }
    }

    // ── Data Types ──

    [Serializable]
    public class AchievementDefinition
    {
        public string achievementId;      // Steam API name
        public string displayName;
        public string description;
        public Sprite icon;
        public bool isProgressive;
        public int targetProgress;        // For progressive achievements
        public string statName;           // Steam stat name for progress
        public bool isHidden;             // Hidden until unlocked
    }
}
#endif
