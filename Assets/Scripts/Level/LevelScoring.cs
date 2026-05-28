using UnityEngine;
using NeonSerpent.Core;

namespace NeonSerpent.Level
{
    /// <summary>
    /// Calculates level rank based on completion time, death count,
    /// and collection rate. Uses thresholds defined in LevelData.
    /// </summary>
    public static class LevelScoring
    {
        /// <summary>
        /// Calculate rank for a completed level.
        /// </summary>
        public static Rank CalculateRank(LevelData level, float completionTime, int deaths, float collectRate)
        {
            if (level == null) return Rank.C;

            // Time score (lower is better)
            float timeRatio = completionTime / level.targetTime;

            // Base rank from time + deaths + collect rate
            Rank baseRank = DetermineBaseRank(level, timeRatio, deaths, collectRate);

            // Death penalty: each death beyond threshold drops rank by one
            int deathPenalty = CalculateDeathPenalty(level, baseRank, deaths);

            for (int i = 0; i < deathPenalty; i++)
            {
                baseRank = DropRank(baseRank);
            }

            return baseRank;
        }

        /// <summary>
        /// Calculate score points for leaderboard.
        /// </summary>
        public static int CalculateScore(LevelData level, Rank rank, float completionTime, int deaths, float collectRate)
        {
            int baseScore = 1000;

            // Rank multiplier
            float rankMultiplier = rank switch
            {
                Rank.S => 3.0f,
                Rank.A => 2.0f,
                Rank.B => 1.5f,
                Rank.C => 1.0f,
                _ => 1.0f
            };

            // Time bonus (faster = more points)
            float timeBonus = Mathf.Max(0, (level.targetTime - completionTime) * 10f);

            // Collection bonus
            float collectBonus = collectRate * 500f;

            // Death penalty
            int deathPenalty = deaths * 100;

            return Mathf.RoundToInt((baseScore * rankMultiplier + timeBonus + collectBonus - deathPenalty));
        }

        private static Rank DetermineBaseRank(LevelData level, float timeRatio, int deaths, float collectRate)
        {
            // S rank: best time, no deaths, high collection
            if (timeRatio <= level.sRankTimeMultiplier &&
                deaths <= level.sRankMaxDeaths &&
                collectRate >= level.sRankMinCollectRate)
            {
                return Rank.S;
            }

            // A rank: good time, minimal deaths, decent collection
            if (timeRatio <= level.aRankTimeMultiplier &&
                deaths <= level.aRankMaxDeaths &&
                collectRate >= level.aRankMinCollectRate)
            {
                return Rank.A;
            }

            // B rank: acceptable time, some deaths, basic collection
            if (timeRatio <= level.bRankTimeMultiplier &&
                deaths <= level.bRankMaxDeaths &&
                collectRate >= level.bRankMinCollectRate)
            {
                return Rank.B;
            }

            // C rank: everything else
            return Rank.C;
        }

        private static int CalculateDeathPenalty(LevelData level, Rank baseRank, int deaths)
        {
            int threshold = baseRank switch
            {
                Rank.S => level.sRankMaxDeaths,
                Rank.A => level.aRankMaxDeaths,
                Rank.B => level.bRankMaxDeaths,
                _ => int.MaxValue // C rank has no death penalty
            };

            if (deaths <= threshold) return 0;
            return deaths - threshold;
        }

        private static Rank DropRank(Rank rank)
        {
            return rank switch
            {
                Rank.S => Rank.A,
                Rank.A => Rank.B,
                Rank.B => Rank.C,
                _ => Rank.C
            };
        }
    }
}
