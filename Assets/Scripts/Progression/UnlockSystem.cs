using System;
using System.Collections.Generic;
using UnityEngine;
using NeonSerpent.Core;
using NeonSerpent.Level;

namespace NeonSerpent.Progression
{
    /// <summary>
    /// Manages unlockable content: skins, themes, music, zones.
    /// Unlock conditions are checked when levels are completed.
    /// </summary>
    public class UnlockSystem : MonoBehaviour
    {
        [Header("Unlockable Content")]
        [SerializeField] private List<UnlockableSkin> skins = new List<UnlockableSkin>();
        [SerializeField] private List<UnlockableTheme> themes = new List<UnlockableTheme>();
        [SerializeField] private List<UnlockableMusic> musicTracks = new List<UnlockableMusic>();

        [Header("Events")]
        public System.Action<UnlockableItem> OnItemUnlocked;

        private HashSet<string> _unlockedItems = new HashSet<string>();

        private void Awake()
        {
            LoadUnlockedItems();
        }

        /// <summary>
        /// Check and apply unlocks after a level is completed.
        /// </summary>
        public void CheckUnlocks(LevelData completedLevel, Rank rank)
        {
            // Check skin unlocks
            foreach (var skin in skins)
            {
                if (IsUnlocked(skin.id)) continue;
                if (MeetsUnlockCondition(skin.unlockCondition, completedLevel, rank))
                {
                    UnlockItem(skin);
                }
            }

            // Check theme unlocks
            foreach (var theme in themes)
            {
                if (IsUnlocked(theme.id)) continue;
                if (MeetsUnlockCondition(theme.unlockCondition, completedLevel, rank))
                {
                    UnlockItem(theme);
                }
            }

            // Check music unlocks
            foreach (var music in musicTracks)
            {
                if (IsUnlocked(music.id)) continue;
                if (MeetsUnlockCondition(music.unlockCondition, completedLevel, rank))
                {
                    UnlockItem(music);
                }
            }
        }

        /// <summary>
        /// Check if a specific item is unlocked.
        /// </summary>
        public bool IsUnlocked(string itemId)
        {
            return _unlockedItems.Contains(itemId);
        }

        /// <summary>
        /// Get all unlocked skins.
        /// </summary>
        public List<UnlockableSkin> GetUnlockedSkins()
        {
            return skins.FindAll(s => IsUnlocked(s.id));
        }

        /// <summary>
        /// Get all unlocked themes.
        /// </summary>
        public List<UnlockableTheme> GetUnlockedThemes()
        {
            return themes.FindAll(t => IsUnlocked(t.id));
        }

        /// <summary>
        /// Force unlock an item (for debugging or cheats).
        /// </summary>
        public void ForceUnlock(string itemId)
        {
            _unlockedItems.Add(itemId);
            SaveUnlockedItems();
        }

        private void UnlockItem(UnlockableItem item)
        {
            _unlockedItems.Add(item.id);
            SaveUnlockedItems();
            OnItemUnlocked?.Invoke(item);
            Debug.Log($"[UnlockSystem] Unlocked: {item.displayName}");
        }

        private bool MeetsUnlockCondition(UnlockCondition condition, LevelData completedLevel, Rank rank)
        {
            if (condition == null) return true;

            // Check level requirement
            if (!string.IsNullOrEmpty(condition.requiredLevelId))
            {
                if (completedLevel.levelId != condition.requiredLevelId)
                    return false;
            }

            // Check zone requirement
            if (condition.requiredZone != ZoneType.NeonCity || condition.requireSpecificZone)
            {
                if (completedLevel.zone != condition.requiredZone)
                    return false;
            }

            // Check rank requirement
            if (condition.requiredRank != Rank.None)
            {
                if ((int)rank < (int)condition.requiredRank)
                    return false;
            }

            // Check level completion count
            if (condition.requiredLevelCompletions > 0)
            {
                // This would need to be checked against save data
                // For now, assume 1 completion of the current level
            }

            return true;
        }

        private void LoadUnlockedItems()
        {
            // Load from SaveSystem
            var saveData = SaveSystem.Instance?.GetProgressData();
            if (saveData != null)
            {
                _unlockedItems = new HashSet<string>(saveData.unlockedItems);
            }
        }

        private void SaveUnlockedItems()
        {
            var saveData = SaveSystem.Instance?.GetProgressData();
            if (saveData != null)
            {
                saveData.unlockedItems = new List<string>(_unlockedItems);
                SaveSystem.Instance.SaveProgress();
            }
        }
    }

    // ── Data Types ──

    [Serializable]
    public abstract class UnlockableItem
    {
        public string id;
        public string displayName;
        public string description;
        public Sprite icon;
        public UnlockCondition unlockCondition;
    }

    [Serializable]
    public class UnlockableSkin : UnlockableItem
    {
        public Material skinMaterial;
        public Color emissionColor = Color.cyan;
        public ParticleSystem trailEffect;
    }

    [Serializable]
    public class UnlockableTheme : UnlockableItem
    {
        public EnvironmentTheme environmentTheme;
        public Material skyboxMaterial;
        public Color ambientLightColor = Color.gray;
    }

    [Serializable]
    public class UnlockableMusic : UnlockableItem
    {
        public AudioClip musicClip;
        public MusicTheme musicTheme;
    }

    [Serializable]
    public class UnlockCondition
    {
        public string requiredLevelId;
        public ZoneType requiredZone = ZoneType.NeonCity;
        public bool requireSpecificZone = false;
        public Rank requiredRank = Rank.None;
        public int requiredLevelCompletions = 1;
    }
}
