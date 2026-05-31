using System;
using System.Reflection;
using NeonSerpent.Audio;
using NeonSerpent.Core;
using NeonSerpent.Procedural.Art.Environment;
using NeonSerpent.Procedural.Audio;
using NeonSerpent.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonSerpent.Editor
{
    /// <summary>
    /// Batchmode-friendly gate for release-critical playability, audio, and art baselines.
    /// </summary>
    public static class GameplayQualityValidator
    {
        public static void RunQualityGate()
        {
            try
            {
                CleanupScene();
                ValidateProceduralAudio();
                ValidateSfxPool();
                ValidateGrappleTargets();
                ValidateCityGeneration();
                ValidateSnakeBodyPerformance();
                Debug.Log("[GameplayQualityValidator] PASS quality gate.");
                Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameplayQualityValidator] FAIL quality gate: {ex}");
                Exit(1);
            }
        }

        private static void ValidateProceduralAudio()
        {
            var audioRoot = new GameObject("QualityAudio");
            var music = audioRoot.AddComponent<MusicManager>();
            music.EnsurePlayableMusic();
            Assert(music.HasPlayableMusic, "MusicManager should create playable procedural fallback stems.");

            var sources = audioRoot.GetComponentsInChildren<AudioSource>();
            Assert(sources.Length >= 2, "MusicManager should create ambient and DnB audio sources.");
            foreach (var source in sources)
            {
                Assert(source.clip != null, $"Music source {source.name} should have a generated clip.");
                Assert(source.clip.samples > 0, $"Music source {source.name} should have non-empty sample data.");
                Assert(source.loop, $"Music source {source.name} should loop.");
            }
        }

        private static void ValidateSfxPool()
        {
            SetStaticProperty(typeof(ProceduralSFXSystem), "Instance", null);

            var sfx = new GameObject("QualitySFX").AddComponent<ProceduralSFXSystem>();
            sfx.EnsureAudioPool();
            Assert(sfx.HasAudioPool, "ProceduralSFXSystem should create an audio-source pool.");

            sfx.PlayFoodCollect(Vector3.zero, 3);
            sfx.PlayDash(Vector3.right);
            sfx.PlayDeath(Vector3.forward);
            sfx.PlayGrapple(Vector3.up);

            var sources = sfx.GetComponents<AudioSource>();
            Assert(sources.Length == sfx.AudioSourceCount, "SFX pool count should match attached audio sources.");
            Assert(Array.Exists(sources, source => source.clip != null), "SFX playback should assign generated clips to the pool.");
        }

        private static void ValidateGrappleTargets()
        {
            var bootstrap = new GameObject("QualityBootstrap").AddComponent<GameBootstrap>();
            InvokePrivate(bootstrap, "CreateGrapplePoints");

            var colliders = UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude);
            int grappleTargets = 0;
            foreach (var collider in colliders)
            {
                if (collider.gameObject.layer == GameConstants.LayerGrapplePoint)
                    grappleTargets++;
            }

            Assert(grappleTargets >= 4, "Default gameplay should expose several reachable grapple targets.");
        }

        private static void ValidateCityGeneration()
        {
            var buildingGenerator = new GameObject("QualityBuildingGenerator").AddComponent<ProceduralBuildingGenerator>();
            var cityGenerator = new GameObject("QualityCityGenerator").AddComponent<ProceduralCityGenerator>();
            cityGenerator.cityBlocksX = 2;
            cityGenerator.cityBlocksZ = 2;
            cityGenerator.buildingGenerator = buildingGenerator;
            cityGenerator.GenerateCity(12345);

            int buildings = 0;
            int renderers = 0;
            foreach (var renderer in UnityEngine.Object.FindObjectsByType<Renderer>(FindObjectsInactive.Exclude))
            {
                renderers++;
                if (renderer.transform.root.name.StartsWith("Building_", StringComparison.Ordinal))
                    buildings++;
            }

            Assert(buildings >= 8, "Procedural city should generate visible building geometry.");
            Assert(renderers >= 24, "Procedural city should generate enough visible detail for a playable scene.");
        }

        private static void ValidateSnakeBodyPerformance()
        {
            var snake = new GameObject("QualitySnake").AddComponent<VerletSnakeBody>();
            snake.ResetBody(Vector3.zero);
            snake.Grow(GameConstants.MaxNodesTarget - snake.NodeCount);
            Assert(snake.NodeCount >= GameConstants.MaxNodesTarget, "Snake body should support the target 200-node length.");

            MethodInfo update = typeof(VerletSnakeBody).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert(update != null, "VerletSnakeBody.Update should be available for performance validation.");

            const int frames = 120;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            for (int frame = 0; frame < frames; frame++)
            {
                float angle = frame * 0.08f;
                snake.SetHeadPosition(new Vector3(Mathf.Sin(angle) * 8f, 2f, frame * 0.08f));
                update.Invoke(snake, null);
            }
            stopwatch.Stop();

            double averageMs = stopwatch.Elapsed.TotalMilliseconds / frames;
            Assert(averageMs < 10.0, $"200-node Verlet update should average under 10ms in batchmode; measured {averageMs:F3}ms.");
            Debug.Log($"[GameplayQualityValidator] 200-node Verlet average update: {averageMs:F3}ms over {frames} frames.");
        }

        private static void CleanupScene()
        {
            foreach (var obj in SceneManager.GetActiveScene().GetRootGameObjects())
                UnityEngine.Object.DestroyImmediate(obj);
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
                throw new InvalidOperationException(message);
        }

        private static void InvokePrivate(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
                throw new MissingMethodException(target.GetType().Name, methodName);
            method.Invoke(target, null);
        }

        private static void SetStaticProperty(Type targetType, string propertyName, object value)
        {
            var property = targetType.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null)
                throw new MissingMemberException(targetType.Name, propertyName);
            property.SetValue(null, value);
        }

        private static void Exit(int code)
        {
            if (Application.isBatchMode)
                EditorApplication.Exit(code);
        }
    }
}
