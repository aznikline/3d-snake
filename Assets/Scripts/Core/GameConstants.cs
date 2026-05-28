namespace NeonSerpent.Core
{
    /// <summary>
    /// Centralized game constants. All gameplay-tunable values live here
    /// so designers can balance without digging into implementation code.
    /// </summary>
    public static class GameConstants
    {
        // ── Camera ──
        public const float CameraFOV = 110f;
        public const float CameraFOVDash = 120f;
        public const float CameraBobFrequency = 8f;
        public const float CameraBobAmplitude = 0.05f;
        public const float CameraImpactShakeDuration = 0.3f;
        public const float CameraImpactShakeIntensity = 0.2f;

        // ── Movement ──
        public const float BaseSpeed = 8f;
        public const float DashSpeedMultiplier = 2.5f;
        public const float DashDuration = 3f;
        public const float DashCooldown = 1f;
        public const float WallRunDurationMax = 2f;
        public const float WallRunAngleMin = 30f;
        public const float WallRunAngleMax = 60f;
        public const float SlideSpeedBoost = 1.2f;
        public const float SlideDurationMax = 1f;
        public const float SlideHeightReduction = 0.5f;
        public const float GrappleDuration = 0.5f;

        // ── Snake Body (Verlet) ──
        public const float SegmentLength = 0.6f;
        public const int VerletIterations = 4;
        public const float VerletStiffness = 0.8f;
        public const float VerletDamping = 0.98f;
        public const float NodeRadiusHead = 0.5f;
        public const float NodeRadiusTail = 0.2f;
        public const int InitialSegmentCount = 10;
        public const int SegmentsPerFood = 1;
        public const int MaxNodesTarget = 200;

        // ── Combo / Dash ──
        public const float ComboFillPerFood = 0.20f;
        public const float ComboDecayPerSecond = 0.02f;
        public const float ComboTimeout = 3f;
        public const int MaxDashCharges = 1;
        public const float DeathPreventedInvincibility = 0.5f;

        // ── Food ──
        public const float FoodSpawnDelay = 0.5f;
        public const int FoodSpawnMaxRetries = 10;
        public const float FoodMinDistanceFromSnake = 2f;

        // ── Death / Replay ──
        public const float DeathTimeScale = 0.2f;
        public const float DeathReplayDuration = 1.5f;
        public const float RespawnDelay = 2f;

        // ── Scoring ──
        public const int ScorePerFood = 100;
        public const int ScorePerFoodCombo = 50; // bonus per combo level
        public const float SRankTimeMultiplier = 0.6f;
        public const float ARankTimeMultiplier = 0.8f;
        public const float BRankTimeMultiplier = 1.0f;
        public const int SRankMaxDeaths = 0;
        public const int ARankMaxDeaths = 1;
        public const int BRankMaxDeaths = 3;

        // ── Audio ──
        public const float MusicCrossfadeDuration = 2f;
        public const float MusicBPMAmbient = 90f;
        public const float MusicBPMDnB = 174f;

        // ── Layers ──
        public const int LayerEnvironment = 6;
        public const int LayerSnake = 7;
        public const int LayerFood = 8;
        public const int LayerGrapplePoint = 9;
    }
}
