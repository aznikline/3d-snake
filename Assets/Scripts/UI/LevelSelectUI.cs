using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonSerpent.Core;
using NeonSerpent.Level;
using NeonSerpent.Progression;

namespace NeonSerpent.UI
{
    /// <summary>
    /// Level selection screen. Displays zones and levels with
    /// unlock status, best rank, and completion info.
    /// </summary>
    public class LevelSelectUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Transform zoneContainer;
        [SerializeField] private GameObject zonePrefab;
        [SerializeField] private Transform levelContainer;
        [SerializeField] private GameObject levelButtonPrefab;
        [SerializeField] private TextMeshProUGUI zoneTitleText;
        [SerializeField] private Button backButton;

        [Header("Colors")]
        [SerializeField] private Color lockedColor = Color.gray;
        [SerializeField] private Color unlockedColor = Color.white;
        [SerializeField] private Color completedColor = Color.green;

        [Header("References")]
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private SaveSystem saveSystem;

        private ZoneType _selectedZone;
        private List<GameObject> _levelButtons = new List<GameObject>();

        private void Start()
        {
            if (backButton != null)
                backButton.onClick.AddListener(ShowZones);

            ShowZones();
        }

        /// <summary>
        /// Show zone selection view.
        /// </summary>
        public void ShowZones()
        {
            ClearContainers();

            if (zoneTitleText != null)
                zoneTitleText.text = "Select Zone";

            // Create zone buttons
            foreach (ZoneType zone in System.Enum.GetValues(typeof(ZoneType)))
            {
                CreateZoneButton(zone);
            }
        }

        /// <summary>
        /// Show levels for a specific zone.
        /// </summary>
        public void ShowZoneLevels(ZoneType zone)
        {
            _selectedZone = zone;
            ClearContainers();

            if (zoneTitleText != null)
                zoneTitleText.text = zone.DisplayName();

            List<LevelData> levels = levelManager?.GetLevelsInZone(zone);
            if (levels == null) return;

            foreach (var level in levels)
            {
                CreateLevelButton(level);
            }
        }

        private void CreateZoneButton(ZoneType zone)
        {
            if (zonePrefab == null || zoneContainer == null) return;

            GameObject button = Instantiate(zonePrefab, zoneContainer);
            var texts = button.GetComponentsInChildren<TextMeshProUGUI>();
            var buttonComponent = button.GetComponent<Button>();
            var image = button.GetComponent<Image>();

            if (texts.Length > 0)
                texts[0].text = zone.DisplayName();

            // Check if zone is unlocked
            bool isUnlocked = IsZoneUnlocked(zone);
            buttonComponent.interactable = isUnlocked;
            image.color = isUnlocked ? unlockedColor : lockedColor;

            buttonComponent.onClick.AddListener(() => ShowZoneLevels(zone));
        }

        private void CreateLevelButton(LevelData level)
        {
            if (levelButtonPrefab == null || levelContainer == null) return;

            GameObject button = Instantiate(levelButtonPrefab, levelContainer);
            _levelButtons.Add(button);

            var texts = button.GetComponentsInChildren<TextMeshProUGUI>();
            var buttonComponent = button.GetComponent<Button>();
            var image = button.GetComponent<Image>();

            // Level number
            if (texts.Length > 0)
                texts[0].text = level.zoneOrder.ToString();

            // Check unlock status
            bool isUnlocked = level.unlockedByDefault || IsLevelUnlocked(level);
            bool isCompleted = IsLevelCompleted(level);
            Rank bestRank = GetBestRank(level);

            buttonComponent.interactable = isUnlocked;

            // Set color based on status
            if (isCompleted)
                image.color = GetRankColor(bestRank);
            else if (isUnlocked)
                image.color = unlockedColor;
            else
                image.color = lockedColor;

            // Show rank icon if completed
            if (texts.Length > 1 && isCompleted)
                texts[1].text = bestRank.ToString();

            buttonComponent.onClick.AddListener(() => OnLevelSelected(level));
        }

        private void OnLevelSelected(LevelData level)
        {
            levelManager?.LoadLevel(level);
            // TODO: Transition to gameplay scene
        }

        private bool IsZoneUnlocked(ZoneType zone)
        {
            // First zone is always unlocked
            if (zone == ZoneType.NeonCity) return true;

            // Check if previous zone is completed
            ZoneType previousZone = zone - 1;
            var previousLevels = levelManager?.GetLevelsInZone(previousZone);
            if (previousLevels == null) return false;

            foreach (var level in previousLevels)
            {
                if (!IsLevelCompleted(level))
                    return false;
            }

            return true;
        }

        private bool IsLevelUnlocked(LevelData level)
        {
            if (level.unlockedByDefault) return true;
            if (string.IsNullOrEmpty(level.requiredLevelId)) return true;

            var record = saveSystem?.GetLevelRecord(level.requiredLevelId);
            if (record == null) return false;

            if (level.requiredRank != Rank.None)
            {
                return (int)record.bestRank >= (int)level.requiredRank;
            }

            return record.completionCount > 0;
        }

        private bool IsLevelCompleted(LevelData level)
        {
            var record = saveSystem?.GetLevelRecord(level.levelId);
            return record != null && record.completionCount > 0;
        }

        private Rank GetBestRank(LevelData level)
        {
            var record = saveSystem?.GetLevelRecord(level.levelId);
            return record?.bestRank ?? Rank.None;
        }

        private Color GetRankColor(Rank rank)
        {
            return rank switch
            {
                Rank.S => new Color(1f, 0.84f, 0f),    // Gold
                Rank.A => new Color(0.8f, 0.8f, 0.8f), // Silver
                Rank.B => new Color(0.8f, 0.5f, 0.2f), // Bronze
                Rank.C => Color.green,
                _ => Color.white
            };
        }

        private void ClearContainers()
        {
            if (zoneContainer != null)
            {
                foreach (Transform child in zoneContainer)
                    Destroy(child.gameObject);
            }

            if (levelContainer != null)
            {
                foreach (Transform child in levelContainer)
                    Destroy(child.gameObject);
            }

            _levelButtons.Clear();
        }
    }
}
