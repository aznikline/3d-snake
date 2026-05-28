using NUnit.Framework;
using NeonSerpent.Core;

namespace NeonSerpent.Tests.EditMode
{
    /// <summary>
    /// Edit-mode tests for level scoring logic.
    /// These tests run without the Unity runtime and verify pure logic.
    /// </summary>
    public class LevelScoringTests
    {
        [Test]
        public void CalculateRank_SRank_PerfectRun()
        {
            // Arrange
            float levelTargetTime = 60f;
            float completionTime = levelTargetTime * GameConstants.SRankTimeMultiplier; // 36s
            int deaths = GameConstants.SRankMaxDeaths; // 0
            float collectRate = 1f;

            // Act
            Rank rank = CalculateRank(levelTargetTime, completionTime, deaths, collectRate);

            // Assert
            Assert.AreEqual(Rank.S, rank, "Perfect run should earn S rank.");
        }

        [Test]
        public void CalculateRank_ARank_GoodRun()
        {
            float levelTargetTime = 60f;
            float completionTime = levelTargetTime * GameConstants.ARankTimeMultiplier; // 48s
            int deaths = GameConstants.ARankMaxDeaths; // 1
            float collectRate = 0.9f;

            Rank rank = CalculateRank(levelTargetTime, completionTime, deaths, collectRate);

            Assert.AreEqual(Rank.A, rank, "Good run with 1 death should earn A rank.");
        }

        [Test]
        public void CalculateRank_BRank_AverageRun()
        {
            float levelTargetTime = 60f;
            float completionTime = levelTargetTime * GameConstants.BRankTimeMultiplier; // 60s
            int deaths = GameConstants.BRankMaxDeaths; // 3
            float collectRate = 0.7f;

            Rank rank = CalculateRank(levelTargetTime, completionTime, deaths, collectRate);

            Assert.AreEqual(Rank.B, rank, "Average run should earn B rank.");
        }

        [Test]
        public void CalculateRank_CRank_PoorRun()
        {
            float levelTargetTime = 60f;
            float completionTime = levelTargetTime * 1.5f; // 90s
            int deaths = 5;
            float collectRate = 0.5f;

            Rank rank = CalculateRank(levelTargetTime, completionTime, deaths, collectRate);

            Assert.AreEqual(Rank.C, rank, "Poor run should earn C rank.");
        }

        [Test]
        public void CalculateRank_Boundary_TimeExactlyAtThreshold()
        {
            float levelTargetTime = 60f;
            float completionTime = levelTargetTime * GameConstants.ARankTimeMultiplier; // Exactly at A threshold
            int deaths = 0;
            float collectRate = 1f;

            Rank rank = CalculateRank(levelTargetTime, completionTime, deaths, collectRate);

            // At exact threshold, should get the lower rank (B, not A)
            Assert.AreEqual(Rank.B, rank, "Exactly at threshold should get lower rank.");
        }

        [Test]
        public void CalculateRank_DeathPenalty_OverridesTime()
        {
            float levelTargetTime = 60f;
            float completionTime = levelTargetTime * 0.3f; // Very fast
            int deaths = 2; // But too many deaths for S or A
            float collectRate = 1f;

            Rank rank = CalculateRank(levelTargetTime, completionTime, deaths, collectRate);

            Assert.AreEqual(Rank.B, rank, "Too many deaths should penalize rank regardless of time.");
        }

        [Test]
        public void CalculateRank_CollectRate_MinimumRequired()
        {
            float levelTargetTime = 60f;
            float completionTime = levelTargetTime * 0.5f;
            int deaths = 0;
            float collectRate = 0.5f; // Below typical threshold

            Rank rank = CalculateRank(levelTargetTime, completionTime, deaths, collectRate);

            Assert.AreEqual(Rank.B, rank, "Low collection rate should prevent S rank.");
        }

        // ── Helper ──

        /// <summary>
        /// Rank calculation logic to be implemented in LevelScoring class.
        /// This is a testable prototype of the scoring algorithm.
        /// </summary>
        private Rank CalculateRank(float targetTime, float completionTime, int deaths, float collectRate)
        {
            // Time score (lower is better)
            float timeRatio = completionTime / targetTime;

            // Base rank from time
            Rank baseRank;
            if (timeRatio <= GameConstants.SRankTimeMultiplier && deaths <= GameConstants.SRankMaxDeaths && collectRate >= 0.95f)
                baseRank = Rank.S;
            else if (timeRatio <= GameConstants.ARankTimeMultiplier && deaths <= GameConstants.ARankMaxDeaths && collectRate >= 0.8f)
                baseRank = Rank.A;
            else if (timeRatio <= GameConstants.BRankTimeMultiplier && deaths <= GameConstants.BRankMaxDeaths && collectRate >= 0.6f)
                baseRank = Rank.B;
            else
                baseRank = Rank.C;

            // Death penalty: each death beyond threshold drops rank by one
            int deathPenalty = 0;
            if (baseRank == Rank.S && deaths > GameConstants.SRankMaxDeaths)
                deathPenalty = deaths - GameConstants.SRankMaxDeaths;
            else if (baseRank == Rank.A && deaths > GameConstants.ARankMaxDeaths)
                deathPenalty = deaths - GameConstants.ARankMaxDeaths;

            for (int i = 0; i < deathPenalty; i++)
            {
                baseRank = DropRank(baseRank);
            }

            return baseRank;
        }

        private Rank DropRank(Rank rank)
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
