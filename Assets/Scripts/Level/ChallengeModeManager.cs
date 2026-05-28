using System;
using System.Collections.Generic;
using UnityEngine;
using NeonSerpent.Core;
using NeonSerpent.Player;

namespace NeonSerpent.Level
{
    /// <summary>
    /// Manages Challenge mode with daily/weekly rotating special rules.
    /// Rules are defined as ScriptableObjects and applied at run start.
    /// </summary>
    public class ChallengeModeManager : MonoBehaviour
    {
        [Header("Challenge Rules")]
        [SerializeField] private List<ChallengeRule> availableRules = new List<ChallengeRule>();

        [Header("Rotation")]
        [SerializeField] private int dailyChallengeCount = 3;
        [SerializeField] private int weeklyChallengeCount = 5;

        [Header("References")]
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private SnakeHeadController snakeController;
        [SerializeField] private VerletSnakeBody snakeBody;
        [SerializeField] private ComboSystem comboSystem;

        public ChallengeRule CurrentRule => _currentRule;
        public bool IsChallengeActive => _isChallengeActive;

        private ChallengeRule _currentRule;
        private bool _isChallengeActive;
        private float _challengeStartTime;
        private int _challengeScore;

        private void Start()
        {
            GenerateDailyChallenges();
        }

        /// <summary>
        /// Start a challenge with a specific rule.
        /// </summary>
        public void StartChallenge(ChallengeRule rule)
        {
            if (rule == null)
            {
                Debug.LogError("[ChallengeModeManager] Cannot start null challenge.");
                return;
            }

            _currentRule = rule;
            _isChallengeActive = true;
            _challengeScore = 0;
            _challengeStartTime = Time.time;

            // Apply rule modifiers
            ApplyRuleModifiers(rule);

            // Start level
            if (levelManager != null && rule.targetLevel != null)
            {
                levelManager.LoadLevel(rule.targetLevel);
            }

            GameStateManager.Instance.SetGameMode(GameMode.Challenge);

            Debug.Log($"[ChallengeModeManager] Started challenge: {rule.displayName}");
        }

        /// <summary>
        /// Start a challenge by rule ID.
        /// </summary>
        public void StartChallenge(string ruleId)
        {
            var rule = availableRules.Find(r => r.ruleId == ruleId);
            if (rule != null)
            {
                StartChallenge(rule);
            }
            else
            {
                Debug.LogError($"[ChallengeModeManager] Rule not found: {ruleId}");
            }
        }

        /// <summary>
        /// End the current challenge.
        /// </summary>
        public void EndChallenge(bool success)
        {
            if (!_isChallengeActive) return;

            _isChallengeActive = false;

            float completionTime = Time.time - _challengeStartTime;

            // Remove rule modifiers
            RemoveRuleModifiers(_currentRule);

            // Save challenge result
            SaveChallengeResult(_currentRule, success, _challengeScore, completionTime);

            Debug.Log($"[ChallengeModeManager] Challenge ended: {_currentRule.displayName} | Success: {success} | Score: {_challengeScore}");

            _currentRule = null;
        }

        /// <summary>
        /// Get today's daily challenges.
        /// </summary>
        public List<ChallengeRule> GetDailyChallenges()
        {
            return GetChallengesForDate(DateTime.UtcNow.Date, dailyChallengeCount);
        }

        /// <summary>
        /// Get this week's weekly challenges.
        /// </summary>
        public List<ChallengeRule> GetWeeklyChallenges()
        {
            // Use the start of the week as seed
            DateTime weekStart = DateTime.UtcNow.Date.AddDays(-(int)DateTime.UtcNow.DayOfWeek);
            return GetChallengesForDate(weekStart, weeklyChallengeCount);
        }

        private void ApplyRuleModifiers(ChallengeRule rule)
        {
            if (snakeController == null) return;

            // Speed modifier
            if (rule.speedMultiplier != 1f)
            {
                // This would need to modify the controller's base speed
                // Implementation depends on how SnakeHeadController handles speed
            }

            // Disable abilities
            if (rule.disableDash && comboSystem != null)
                comboSystem.enabled = false;

            if (rule.disableWallRun)
            {
                var wallRun = snakeController.GetComponent<WallRunAbility>();
                if (wallRun != null) wallRun.enabled = false;
            }

            if (rule.disableSlide)
            {
                var slide = snakeController.GetComponent<SlideAbility>();
                if (slide != null) slide.enabled = false;
            }

            if (rule.disableGrapple)
            {
                var grapple = snakeController.GetComponent<GrappleAbility>();
                if (grapple != null) grapple.enabled = false;
            }

            // Mirror controls
            if (rule.mirrorControls)
            {
                // This would need to invert input in SnakeHeadController
            }

            // Time limit
            if (rule.timeLimit > 0)
            {
                Invoke(nameof(OnTimeLimitReached), rule.timeLimit);
            }
        }

        private void RemoveRuleModifiers(ChallengeRule rule)
        {
            if (snakeController == null) return;

            // Re-enable all abilities
            if (comboSystem != null)
                comboSystem.enabled = true;

            var wallRun = snakeController.GetComponent<WallRunAbility>();
            if (wallRun != null) wallRun.enabled = true;

            var slide = snakeController.GetComponent<SlideAbility>();
            if (slide != null) slide.enabled = true;

            var grapple = snakeController.GetComponent<GrappleAbility>();
            if (grapple != null) grapple.enabled = true;

            CancelInvoke(nameof(OnTimeLimitReached));
        }

        private void OnTimeLimitReached()
        {
            if (_isChallengeActive)
            {
                EndChallenge(false); // Time limit exceeded = failure
            }
        }

        private void SaveChallengeResult(ChallengeRule rule, bool success, int score, float time)
        {
            string key = $"challenge_{rule.ruleId}_{DateTime.UtcNow:yyyyMMdd}";
            PlayerPrefs.SetInt(key + "_completed", success ? 1 : 0);
            PlayerPrefs.SetInt(key + "_score", score);
            PlayerPrefs.SetFloat(key + "_time", time);
            PlayerPrefs.Save();
        }

        private List<ChallengeRule> GetChallengesForDate(DateTime date, int count)
        {
            // Use date as seed for deterministic selection
            System.Random rng = new System.Random(date.Year * 10000 + date.Month * 100 + date.Day);

            List<ChallengeRule> selected = new List<ChallengeRule>();
            List<ChallengeRule> pool = new List<ChallengeRule>(availableRules);

            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int index = rng.Next(pool.Count);
                selected.Add(pool[index]);
                pool.RemoveAt(index);
            }

            return selected;
        }

        private void GenerateDailyChallenges()
        {
            // This could be called at midnight to refresh challenges
            Debug.Log("[ChallengeModeManager] Daily challenges generated.");
        }
    }

    // ── Challenge Rule Definition ──

    [CreateAssetMenu(fileName = "NewChallengeRule", menuName = "NEON SERPENT/Challenge Rule")]
    public class ChallengeRule : ScriptableObject
    {
        [Header("Identity")]
        public string ruleId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;
        public Sprite icon;

        [Header("Target")]
        public LevelData targetLevel;

        [Header("Modifiers")]
        [Range(0.5f, 3f)]
        public float speedMultiplier = 1f;
        public bool disableDash = false;
        public bool disableWallRun = false;
        public bool disableSlide = false;
        public bool disableGrapple = false;
        public bool mirrorControls = false;
        public float timeLimit = 0f; // 0 = no limit

        [Header("Objectives")]
        public int targetLength = 20;
        public int targetScore = 0;
        public int maxDeaths = 3;

        [Header("Rewards")]
        public int completionReward = 100;
        public string unlockItemId; // Item unlocked on first completion
    }
}
