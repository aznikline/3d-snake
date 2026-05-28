#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using NeonSerpent.Platform;

namespace NeonSerpent.Editor
{
    /// <summary>
    /// Automated build pipeline for NEON SERPENT.
    /// Supports Windows, macOS, and Linux builds with version management.
    /// </summary>
    public static class BuildPipeline
    {
        private const string BUILD_FOLDER = "Builds";
        private const string WINDOWS_FOLDER = "Windows";
        private const string MACOS_FOLDER = "macOS";
        private const string LINUX_FOLDER = "Linux";

        [MenuItem("NEON SERPENT/Build/All Platforms")]
        public static void BuildAllPlatforms()
        {
            BuildWindows();
            BuildMacOS();
            BuildLinux();

            Debug.Log("[BuildPipeline] All platform builds complete.");
        }

        [MenuItem("NEON SERPENT/Build/Windows")]
        public static void BuildWindows()
        {
            string buildPath = GetBuildPath(WINDOWS_FOLDER, BuildTarget.StandaloneWindows64);
            BuildPlayer(buildPath, BuildTarget.StandaloneWindows64);
        }

        [MenuItem("NEON SERPENT/Build/macOS")]
        public static void BuildMacOS()
        {
            string buildPath = GetBuildPath(MACOS_FOLDER, BuildTarget.StandaloneOSX);
            BuildPlayer(buildPath, BuildTarget.StandaloneOSX);
        }

        [MenuItem("NEON SERPENT/Build/Linux")]
        public static void BuildLinux()
        {
            string buildPath = GetBuildPath(LINUX_FOLDER, BuildTarget.StandaloneLinux64);
            BuildPlayer(buildPath, BuildTarget.StandaloneLinux64);
        }

        [MenuItem("NEON SERPENT/Build/Development Build (Current Platform)")]
        public static void BuildDevelopment()
        {
            string platformFolder = EditorUserBuildSettings.activeBuildTarget switch
            {
                BuildTarget.StandaloneWindows64 => WINDOWS_FOLDER,
                BuildTarget.StandaloneOSX => MACOS_FOLDER,
                BuildTarget.StandaloneLinux64 => LINUX_FOLDER,
                _ => "Development"
            };

            string buildPath = GetBuildPath(platformFolder + "_Dev", EditorUserBuildSettings.activeBuildTarget);
            BuildPlayer(buildPath, EditorUserBuildSettings.activeBuildTarget, true);
        }

        [MenuItem("NEON SERPENT/Build/Increment Version")]
        public static void IncrementVersion()
        {
            // Parse current version
            string[] versionParts = BuildConfig.Version.Split('.');
            if (versionParts.Length == 3 &&
                int.TryParse(versionParts[0], out int major) &&
                int.TryParse(versionParts[1], out int minor) &&
                int.TryParse(versionParts[2], out int patch))
            {
                patch++;
                string newVersion = $"{major}.{minor}.{patch}";

                // Update PlayerSettings
                PlayerSettings.bundleVersion = newVersion;

                Debug.Log($"[BuildPipeline] Version incremented to {newVersion}");
            }
        }

        private static void BuildPlayer(string buildPath, BuildTarget target, bool development = false)
        {
            // Ensure build directory exists
            Directory.CreateDirectory(buildPath);

            // Configure build options
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = GetBuildScenes(),
                locationPathName = GetExecutablePath(buildPath, target),
                target = target,
                options = development ? BuildOptions.Development : BuildOptions.None
            };

            if (development)
            {
                buildOptions.options |= BuildOptions.AllowDebugging;
                buildOptions.options |= BuildOptions.ConnectWithProfiler;
            }

            // Update build number
            UpdateBuildNumber();

            // Perform build
            Debug.Log($"[BuildPipeline] Building for {target}...");
            BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(buildOptions.scenes, buildOptions.locationPathName, target, buildOptions.options);
            BuildSummary summary = report.summary;

            // Report results
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[BuildPipeline] Build succeeded: {summary.totalSize / 1024 / 1024} MB in {summary.totalTime.TotalSeconds:F1}s");
                Debug.Log($"[BuildPipeline] Output: {buildPath}");
            }
            else if (summary.result == BuildResult.Failed)
            {
                Debug.LogError($"[BuildPipeline] Build failed with {summary.totalErrors} errors.");
            }
        }

        private static string[] GetBuildScenes()
        {
            // Get all scenes in build settings
            var scenes = new System.Collections.Generic.List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                {
                    scenes.Add(scene.path);
                }
            }
            return scenes.ToArray();
        }

        private static string GetBuildPath(string platformFolder, BuildTarget target)
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            return Path.Combine(BUILD_FOLDER, $"{platformFolder}_{BuildConfig.Version}_{timestamp}");
        }

        private static string GetExecutablePath(string buildPath, BuildTarget target)
        {
            string exeName = BuildConfig.GameName.Replace(" ", "");

            return target switch
            {
                BuildTarget.StandaloneWindows64 => Path.Combine(buildPath, $"{exeName}.exe"),
                BuildTarget.StandaloneOSX => Path.Combine(buildPath, $"{exeName}.app"),
                BuildTarget.StandaloneLinux64 => Path.Combine(buildPath, exeName),
                _ => Path.Combine(buildPath, exeName)
            };
        }

        private static void UpdateBuildNumber()
        {
            // Increment build number in PlayerSettings
            int buildNumber = PlayerSettings.Android.bundleVersionCode + 1;
            PlayerSettings.Android.bundleVersionCode = buildNumber;
            PlayerSettings.iOS.buildNumber = buildNumber.ToString();
        }
    }
}
#endif
