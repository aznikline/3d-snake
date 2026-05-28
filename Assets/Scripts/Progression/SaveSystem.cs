using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using NeonSerpent.Core;
using NeonSerpent.Level;

namespace NeonSerpent.Progression
{
    /// <summary>
    /// Persistent save system using JSON serialization.
    /// Supports local saves with optional Steam Cloud sync.
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        [Header("Settings")]
        [SerializeField] private bool enableSteamCloud = true;
        [SerializeField] private int maxBackupSaves = 3;

        private const string SAVE_FILE_NAME = "save.json";
        private const string BACKUP_SUFFIX = ".backup";

        private SaveData _cachedData;
        private bool _isLoaded;

        public SaveData CurrentData => _cachedData;
        public bool IsLoaded => _isLoaded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadProgress();
        }

        /// <summary>
        /// Load progress from disk. Creates new save if none exists.
        /// </summary>
        public void LoadProgress()
        {
            string path = GetSavePath();

            if (File.Exists(path))
            {
                try
                {
                    string json = File.ReadAllText(path);
                    _cachedData = JsonUtility.FromJson<SaveData>(json);

                    if (_cachedData == null)
                    {
                        Debug.LogWarning("[SaveSystem] Save file corrupted. Creating new save.");
                        _cachedData = CreateNewSave();
                    }
                    else
                    {
                        // Version migration if needed
                        MigrateSaveData(_cachedData);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[SaveSystem] Failed to load save: {e.Message}");
                    _cachedData = CreateNewSave();
                }
            }
            else
            {
                _cachedData = CreateNewSave();
            }

            _isLoaded = true;
            Debug.Log("[SaveSystem] Progress loaded.");
        }

        /// <summary>
        /// Save current progress to disk.
        /// </summary>
        public void SaveProgress()
        {
            if (!_isLoaded) return;

            string path = GetSavePath();
            string json = JsonUtility.ToJson(_cachedData, true);

            try
            {
                // Create backup
                CreateBackup(path);

                // Write new save
                File.WriteAllText(path, json);

                // Steam Cloud sync
                if (enableSteamCloud)
                {
                    SyncToSteamCloud(path);
                }

                Debug.Log("[SaveSystem] Progress saved.");
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to save: {e.Message}");
            }
        }

        /// <summary>
        /// Record level completion in save data.
        /// </summary>
        public void RecordLevelCompletion(string levelId, Rank rank, int score, float completionTime)
        {
            if (!_isLoaded) return;

            var record = _cachedData.levelRecords.Find(r => r.levelId == levelId);
            if (record == null)
            {
                record = new LevelRecord { levelId = levelId };
                _cachedData.levelRecords.Add(record);
            }

            record.completionCount++;
            record.bestTime = Mathf.Min(record.bestTime, completionTime);
            record.bestScore = Mathf.Max(record.bestScore, score);

            if ((int)rank > (int)record.bestRank)
            {
                record.bestRank = rank;
            }

            // Update total stats
            _cachedData.totalScore += score;
            _cachedData.totalDeaths += record.completionCount > 1 ? 0 : 0; // Deaths tracked per-attempt elsewhere

            SaveProgress();
        }

        /// <summary>
        /// Get level record if it exists.
        /// </summary>
        public LevelRecord GetLevelRecord(string levelId)
        {
            return _cachedData?.levelRecords.Find(r => r.levelId == levelId);
        }

        /// <summary>
        /// Check if a level is unlocked.
        /// </summary>
        public bool IsLevelUnlocked(string levelId)
        {
            var record = GetLevelRecord(levelId);
            return record != null && record.completionCount > 0;
        }

        /// <summary>
        /// Get the progress data reference for other systems to modify.
        /// </summary>
        public SaveData GetProgressData()
        {
            return _cachedData;
        }

        /// <summary>
        /// Delete all save data and start fresh.
        /// </summary>
        public void DeleteAllSaves()
        {
            string path = GetSavePath();
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            // Delete backups
            for (int i = 0; i < maxBackupSaves; i++)
            {
                string backupPath = path + BACKUP_SUFFIX + i;
                if (File.Exists(backupPath))
                    File.Delete(backupPath);
            }

            _cachedData = CreateNewSave();
            _isLoaded = true;

            Debug.Log("[SaveSystem] All saves deleted.");
        }

        /// <summary>
        /// Export save data as JSON string (for debugging or manual backup).
        /// </summary>
        public string ExportSaveData()
        {
            return JsonUtility.ToJson(_cachedData, true);
        }

        /// <summary>
        /// Import save data from JSON string.
        /// </summary>
        public bool ImportSaveData(string json)
        {
            try
            {
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data != null)
                {
                    _cachedData = data;
                    SaveProgress();
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Failed to import save: {e.Message}");
            }
            return false;
        }

        // ── Private ──

        private string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        }

        private SaveData CreateNewSave()
        {
            return new SaveData
            {
                version = SaveData.CURRENT_VERSION,
                createdAt = DateTime.UtcNow.ToString("O"),
                lastPlayedAt = DateTime.UtcNow.ToString("O"),
                levelRecords = new List<LevelRecord>(),
                unlockedItems = new List<string>(),
                settings = new GameSettings()
            };
        }

        private void CreateBackup(string originalPath)
        {
            if (!File.Exists(originalPath)) return;

            // Shift backups
            for (int i = maxBackupSaves - 1; i > 0; i--)
            {
                string oldPath = originalPath + BACKUP_SUFFIX + (i - 1);
                string newPath = originalPath + BACKUP_SUFFIX + i;
                if (File.Exists(oldPath))
                {
                    File.Copy(oldPath, newPath, true);
                }
            }

            // Create newest backup
            File.Copy(originalPath, originalPath + BACKUP_SUFFIX + 0, true);
        }

        private void MigrateSaveData(SaveData data)
        {
            if (data.version < SaveData.CURRENT_VERSION)
            {
                // Migration logic for future versions
                data.version = SaveData.CURRENT_VERSION;
                Debug.Log($"[SaveSystem] Migrated save from v{data.version} to v{SaveData.CURRENT_VERSION}");
            }
        }

        private void SyncToSteamCloud(string localPath)
        {
            // TODO: Implement Steam Cloud sync using Steamworks.NET
            // This is a placeholder for Steam integration
            Debug.Log("[SaveSystem] Steam Cloud sync requested (not yet implemented).");
        }
    }

    // ── Serializable Data Types ──

    [Serializable]
    public class SaveData
    {
        public const int CURRENT_VERSION = 1;

        public int version;
        public string createdAt;
        public string lastPlayedAt;
        public List<LevelRecord> levelRecords;
        public List<string> unlockedItems;
        public GameSettings settings;
        public int totalScore;
        public int totalDeaths;
        public float totalPlayTime;
    }

    [Serializable]
    public class LevelRecord
    {
        public string levelId;
        public int completionCount;
        public Rank bestRank = Rank.None;
        public int bestScore;
        public float bestTime = float.MaxValue;
    }

    [Serializable]
    public class GameSettings
    {
        public float masterVolume = 1f;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
        public float mouseSensitivity = 0.5f;
        public bool fullscreen = true;
        public int targetFrameRate = 60;
        public bool showTutorial = true;
        public string selectedSkinId;
        public string selectedThemeId;
    }
}
