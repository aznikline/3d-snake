using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace NeonSerpent.Editor
{
    public static class BuildScript
    {
        [MenuItem("POLY SERPENT/Build and Run")]
        public static void BuildAndRun()
        {
            var scenes = new[] { "Assets/Scenes/Boot.unity", "Assets/Scenes/Gameplay.unity" };
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = "Builds/PolySerpent.app",
                target = BuildTarget.StandaloneOSX,
                options = BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Build succeeded: {report.summary.totalSize} bytes");
            }
            else
            {
                Debug.LogError($"Build failed: {report.summary.result}");
            }
        }

        public static void BuildFromCommandLine()
        {
            BuildAndRun();
        }
    }
}
