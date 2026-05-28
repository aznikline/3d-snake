using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeonSerpent.Steam
{
    /// <summary>
    /// Steam leaderboard wrapper. Handles score submission, retrieval,
    /// and local caching for offline play.
    /// </summary>
    public class LeaderboardManager : MonoBehaviour
    {
        public static LeaderboardManager Instance { get; private set; }

        [Header("Leaderboards")]
        [SerializeField] private string endlessLeaderboardName = "endless_high_score";
        [SerializeField] private string campaignLeaderboardName = "campaign_total_score";

        [Header("Caching")]
        [SerializeField] private int cacheExpiryHours = 1;

        public bool IsInitialized => _isInitialized;
        public event Action OnLeaderboardsReady;

        private bool _isInitialized;
        private Dictionary<string, List<LeaderboardEntry>> _cachedEntries = new Dictionary<string, List<LeaderboardEntry>>();
        private Dictionary<string, DateTime> _cacheTimestamps = new Dictionary<string, DateTime>();
        private Queue<PendingScore> _pendingScores = new Queue<PendingScore>();

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
            InitializeSteamLeaderboards();
        }

        /// <summary>
        /// Submit a score to a leaderboard.
        /// </summary>
        public void SubmitScore(string leaderboardName, int score)
        {
            if (!_isInitialized)
            {
                // Queue for later submission
                _pendingScores.Enqueue(new PendingScore
                {
                    leaderboardName = leaderboardName,
                    score = score,
                    timestamp = DateTime.UtcNow
                });
                Debug.Log($"[LeaderboardManager] Score queued for {leaderboardName}: {score}");
                return;
            }

            // TODO: Implement Steamworks leaderboard submission
            // SteamUserStats.UploadLeaderboardScore(leaderboardHandle, score, null, 0);
            Debug.Log($"[LeaderboardManager] Submitting score to {leaderboardName}: {score}");
        }

        /// <summary>
        /// Submit endless mode high score.
        /// </summary>
        public void SubmitEndlessScore(int score)
        {
            SubmitScore(endlessLeaderboardName, score);
        }

        /// <summary>
        /// Submit campaign total score.
        /// </summary>
        public void SubmitCampaignScore(int score)
        {
            SubmitScore(campaignLeaderboardName, score);
        }

        /// <summary>
        /// Get global leaderboard entries.
        /// </summary>
        public void GetGlobalEntries(string leaderboardName, int count, Action<List<LeaderboardEntry>> callback)
        {
            if (IsCacheValid(leaderboardName))
            {
                callback?.Invoke(_cachedEntries[leaderboardName]);
                return;
            }

            // TODO: Implement Steamworks leaderboard download
            // SteamUserStats.DownloadLeaderboardEntries(leaderboardHandle, ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal, 1, count);

            // Return dummy data for now
            var dummyEntries = GenerateDummyEntries(count);
            CacheEntries(leaderboardName, dummyEntries);
            callback?.Invoke(dummyEntries);
        }

        /// <summary>
        /// Get friend leaderboard entries.
        /// </summary>
        public void GetFriendEntries(string leaderboardName, Action<List<LeaderboardEntry>> callback)
        {
            // TODO: Implement Steamworks friend leaderboard download
            // SteamUserStats.DownloadLeaderboardEntries(leaderboardHandle, ELeaderboardDataRequest.k_ELeaderboardDataRequestFriends, 1, 100);

            var dummyEntries = GenerateDummyEntries(10);
            callback?.Invoke(dummyEntries);
        }

        /// <summary>
        /// Get the player's personal best.
        /// </summary>
        public void GetPlayerBest(string leaderboardName, Action<LeaderboardEntry> callback)
        {
            // TODO: Implement Steamworks personal best retrieval
            callback?.Invoke(new LeaderboardEntry
            {
                rank = 42,
                playerName = "Player",
                score = 15000,
                timestamp = DateTime.UtcNow.AddDays(-1)
            });
        }

        /// <summary>
        /// Process any queued scores (call after Steam initialization).
        /// </summary>
        public void ProcessPendingScores()
        {
            while (_pendingScores.Count > 0)
            {
                var pending = _pendingScores.Dequeue();
                SubmitScore(pending.leaderboardName, pending.score);
            }
        }

        private void InitializeSteamLeaderboards()
        {
            // TODO: Initialize Steamworks and find/create leaderboards
            // This would be called after SteamManager confirms Steam is ready

            _isInitialized = true; // Placeholder
            OnLeaderboardsReady?.Invoke();
            ProcessPendingScores();
        }

        private bool IsCacheValid(string leaderboardName)
        {
            if (!_cachedEntries.ContainsKey(leaderboardName)) return false;
            if (!_cacheTimestamps.ContainsKey(leaderboardName)) return false;

            TimeSpan age = DateTime.UtcNow - _cacheTimestamps[leaderboardName];
            return age.TotalHours < cacheExpiryHours;
        }

        private void CacheEntries(string leaderboardName, List<LeaderboardEntry> entries)
        {
            _cachedEntries[leaderboardName] = entries;
            _cacheTimestamps[leaderboardName] = DateTime.UtcNow;
        }

        private List<LeaderboardEntry> GenerateDummyEntries(int count)
        {
            var entries = new List<LeaderboardEntry>();
            System.Random rng = new System.Random();

            for (int i = 0; i < count; i++)
            {
                entries.Add(new LeaderboardEntry
                {
                    rank = i + 1,
                    playerName = $"Player_{rng.Next(1000, 9999)}",
                    score = (count - i) * 1000 + rng.Next(0, 999),
                    timestamp = DateTime.UtcNow.AddDays(-rng.Next(0, 30))
                });
            }

            return entries;
        }
    }

    // ── Data Types ──

    [Serializable]
    public class LeaderboardEntry
    {
        public int rank;
        public string playerName;
        public int score;
        public DateTime timestamp;
    }

    [Serializable]
    public struct PendingScore
    {
        public string leaderboardName;
        public int score;
        public DateTime timestamp;
    }
}
