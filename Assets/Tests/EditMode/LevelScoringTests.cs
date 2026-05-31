using NUnit.Framework;
using NeonSerpent.Core;
using NeonSerpent.Level;

namespace NeonSerpent.Tests.EditMode
{
    public class LevelScoringTests
    {
        private LevelData CreateTestLevel(float targetTime = 60f)
        {
            var level = UnityEngine.ScriptableObject.CreateInstance<LevelData>();
            level.levelId = "test_level";
            level.displayName = "Test Level";
            level.targetTime = targetTime;
            level.targetLength = 20;
            level.maxDeaths = 3;
            level.sRankTimeMultiplier = GameConstants.SRankTimeMultiplier;
            level.aRankTimeMultiplier = GameConstants.ARankTimeMultiplier;
            level.bRankTimeMultiplier = GameConstants.BRankTimeMultiplier;
            level.sRankMaxDeaths = GameConstants.SRankMaxDeaths;
            level.aRankMaxDeaths = GameConstants.ARankMaxDeaths;
            level.bRankMaxDeaths = GameConstants.BRankMaxDeaths;
            level.sRankMinCollectRate = 0.95f;
            level.aRankMinCollectRate = 0.8f;
            level.bRankMinCollectRate = 0.6f;
            return level;
        }

        [Test]
        public void CalculateRank_SRank_PerfectRun()
        {
            var level = CreateTestLevel();
            float completionTime = level.targetTime * GameConstants.SRankTimeMultiplier;
            int deaths = GameConstants.SRankMaxDeaths;
            float collectRate = 1f;

            Rank rank = LevelScoring.CalculateRank(level, completionTime, deaths, collectRate);

            Assert.AreEqual(Rank.S, rank, "Perfect run should earn S rank.");
        }

        [Test]
        public void CalculateRank_ARank_GoodRun()
        {
            var level = CreateTestLevel();
            float completionTime = level.targetTime * GameConstants.ARankTimeMultiplier;
            int deaths = GameConstants.ARankMaxDeaths;
            float collectRate = 0.9f;

            Rank rank = LevelScoring.CalculateRank(level, completionTime, deaths, collectRate);

            Assert.AreEqual(Rank.A, rank, "Good run with 1 death should earn A rank.");
        }

        [Test]
        public void CalculateRank_BRank_AverageRun()
        {
            var level = CreateTestLevel();
            float completionTime = level.targetTime * GameConstants.BRankTimeMultiplier;
            int deaths = GameConstants.BRankMaxDeaths;
            float collectRate = 0.7f;

            Rank rank = LevelScoring.CalculateRank(level, completionTime, deaths, collectRate);

            Assert.AreEqual(Rank.B, rank, "Average run should earn B rank.");
        }

        [Test]
        public void CalculateRank_CRank_PoorRun()
        {
            var level = CreateTestLevel();
            float completionTime = level.targetTime * 1.5f;
            int deaths = 5;
            float collectRate = 0.5f;

            Rank rank = LevelScoring.CalculateRank(level, completionTime, deaths, collectRate);

            Assert.AreEqual(Rank.C, rank, "Poor run should earn C rank.");
        }

        [Test]
        public void CalculateRank_DeathPenalty_OverridesTime()
        {
            var level = CreateTestLevel();
            float completionTime = level.targetTime * 0.3f;
            int deaths = 2;
            float collectRate = 1f;

            Rank rank = LevelScoring.CalculateRank(level, completionTime, deaths, collectRate);

            Assert.AreEqual(Rank.B, rank, "Too many deaths should penalize rank regardless of time.");
        }

        [Test]
        public void CalculateScore_SRank_GivesHighestMultiplier()
        {
            var level = CreateTestLevel();
            float completionTime = level.targetTime * 0.5f;
            float collectRate = 1f;

            int score = LevelScoring.CalculateScore(level, Rank.S, completionTime, 0, collectRate);

            Assert.Greater(score, 2500, "S rank should give a high score.");
        }

        [Test]
        public void CalculateScore_CRank_GivesLowestMultiplier()
        {
            var level = CreateTestLevel();
            float completionTime = level.targetTime * 1.5f;
            float collectRate = 0.5f;

            int score = LevelScoring.CalculateScore(level, Rank.C, completionTime, 3, collectRate);

            Assert.Less(score, 2000, "C rank should give a low score.");
        }
    }
}
