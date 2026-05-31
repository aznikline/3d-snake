using UnityEngine;
using NeonSerpent.Core;

namespace NeonSerpent.Level
{
    /// <summary>
    /// ScriptableObject containing level metadata. The actual geometry
    /// is stored separately in JSON to allow editor import/export.
    /// </summary>
    [CreateAssetMenu(fileName = "NewLevel", menuName = "POLY SERPENT/Level Data")]
    public class LevelData : ScriptableObject
    {
        [Header("Identity")]
        public string levelId;
        public string displayName;
        [TextArea(2, 4)]
        public string description;

        [Header("Zone")]
        public ZoneType zone;
        public int zoneOrder; // Order within zone (1-based)

        [Header("Objectives")]
        public int targetLength = 20;
        public float targetTime = 60f;
        public int maxDeaths = 3;

        [Header("Scoring Thresholds")]
        public float sRankTimeMultiplier = 0.6f;
        public float aRankTimeMultiplier = 0.8f;
        public float bRankTimeMultiplier = 1.0f;
        public int sRankMaxDeaths = 0;
        public int aRankMaxDeaths = 1;
        public int bRankMaxDeaths = 3;
        public float sRankMinCollectRate = 0.95f;
        public float aRankMinCollectRate = 0.8f;
        public float bRankMinCollectRate = 0.6f;

        [Header("Unlock Requirements")]
        public bool unlockedByDefault = false;
        public string requiredLevelId; // Previous level that must be completed
        public Rank requiredRank = Rank.None; // Minimum rank required on previous level

        [Header("Environment")]
        public EnvironmentTheme environmentTheme;
        public MusicTheme musicTheme;

        [Header("Special Mechanics")]
        public bool enableWallRun = true;
        public bool enableSlide = true;
        public bool enableGrapple = true;
        public bool enableDash = true;

        [Header("Geometry Reference")]
        public TextAsset geometryJson; // Reference to JSON geometry file

        /// <summary>
        /// Full level number (e.g., "Zone 1-5").
        /// </summary>
        public string FullLevelName => $"{zone.DisplayName()} - {zoneOrder}";

        /// <summary>
        /// Unique key for save data and leaderboards.
        /// </summary>
        public string SaveKey => $"level_{levelId}";
    }

    /// <summary>
    /// Campaign zone definitions.
    /// </summary>
    public enum ZoneType
    {
        PolyCity,
        DataCore,
        AbyssRift,
        CoreFurnace,
        VoidEdge
    }

    public static class ZoneTypeExtensions
    {
        public static string DisplayName(this ZoneType zone)
        {
            return zone switch
            {
                ZoneType.PolyCity => "低模都市",
                ZoneType.DataCore => "数据核心",
                ZoneType.AbyssRift => "深渊裂隙",
                ZoneType.CoreFurnace => "核心熔炉",
                ZoneType.VoidEdge => "虚空边界",
                _ => zone.ToString()
            };
        }

        public static int LevelCount(this ZoneType zone)
        {
            return zone switch
            {
                ZoneType.PolyCity => 15,
                ZoneType.DataCore => 15,
                ZoneType.AbyssRift => 20,
                ZoneType.CoreFurnace => 15,
                ZoneType.VoidEdge => 15,
                _ => 10
            };
        }

        public static int StartLevelNumber(this ZoneType zone)
        {
            return zone switch
            {
                ZoneType.PolyCity => 1,
                ZoneType.DataCore => 16,
                ZoneType.AbyssRift => 31,
                ZoneType.CoreFurnace => 51,
                ZoneType.VoidEdge => 66,
                _ => 1
            };
        }
    }

    public enum EnvironmentTheme
    {
        PolyCity,
        DataCore,
        AbyssRift,
        CoreFurnace,
        VoidEdge,
        RainyNight,
        AbandonedFactory
    }

    public enum MusicTheme
    {
        Ambient,
        Action,
        Boss,
        Secret
    }
}
