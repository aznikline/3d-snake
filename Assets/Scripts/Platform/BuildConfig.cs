using UnityEngine;

namespace NeonSerpent.Platform
{
    /// <summary>
    /// Build configuration and version management.
    /// Updated automatically by build pipeline.
    /// </summary>
    public static class BuildConfig
    {
        public const string GameName = "NEON SERPENT";
        public const string CompanyName = "Your Studio Name";
        public const string Version = "0.1.0";
        public const string BuildDate = "2026-05-28";
        public const int BuildNumber = 1;

        public const bool IsDevelopmentBuild =
#if DEVELOPMENT_BUILD
            true;
#else
            false;
#endif

        public const bool IsDebugBuild =
#if DEBUG
            true;
#else
            false;
#endif

        /// <summary>
        /// Full version string for display.
        /// </summary>
        public static string FullVersion => $"v{Version} (Build {BuildNumber})";

        /// <summary>
        /// Short version for Steam/App Store.
        /// </summary>
        public static string ShortVersion => Version;
    }
}
