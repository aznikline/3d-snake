using System;
using System.Reflection;
using NeonSerpent.Core;
using NeonSerpent.Gameplay;
using NeonSerpent.Level;
using NeonSerpent.Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonSerpent.Editor
{
    /// <summary>
    /// Batchmode-friendly validation for the core playable loop.
    /// </summary>
    public static class GameplaySmokeValidator
    {
        public static void RunCoreGameplaySmoke()
        {
            try
            {
                CleanupScene();
                RunCoreLoopChecks();
                Debug.Log("[GameplaySmokeValidator] PASS core gameplay smoke.");
                Exit(0);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameplaySmokeValidator] FAIL core gameplay smoke: {ex}");
                Exit(1);
            }
        }

        private static void RunCoreLoopChecks()
        {
            var stateManager = new GameObject("GameStateManager").AddComponent<GameStateManager>();
            SetStaticProperty(typeof(GameStateManager), "Instance", stateManager);
            var gameplayRoot = new GameObject("GameplaySystems");

            var player = new GameObject("Player");
            player.tag = "SnakeHead";
            player.AddComponent<CharacterController>();

            var controller = player.AddComponent<SnakeHeadController>();
            var body = player.AddComponent<VerletSnakeBody>();
            var slide = player.AddComponent<SlideAbility>();
            var grapple = player.AddComponent<GrappleAbility>();
            controller.snakeBody = body;
            body.ResetBody(Vector3.zero);

            var combo = gameplayRoot.AddComponent<ComboSystem>();
            var death = gameplayRoot.AddComponent<DeathManager>();
            var spawner = gameplayRoot.AddComponent<FoodSpawner>();
            var levelManager = gameplayRoot.AddComponent<LevelManager>();

            death.snakeController = controller;
            death.snakeBody = body;
            death.comboSystem = combo;

            spawner.snakeBody = body;
            spawner.comboSystem = combo;
            spawner.levelManager = levelManager;
            spawner.foodPrefab = CreateFoodPrefab();

            levelManager.snakeController = controller;
            levelManager.snakeBody = body;
            levelManager.comboSystem = combo;
            levelManager.foodSpawner = spawner;
            levelManager.deathManager = death;

            var level = CreateSmokeLevel();
            bool completed = false;
            levelManager.OnLevelCompleted += (_, _, _) => completed = true;

            levelManager.LoadLevel(level);
            Assert(stateManager.CurrentState == GameState.Playing, "Level load should enter Playing state.");
            Assert(spawner.transform.childCount >= 1, "Level load should spawn active food.");
            Assert(spawner.GetComponentInChildren<Food>(true).gameObject.activeInHierarchy, "Spawned food should be active.");

            int initialNodes = body.NodeCount;
            spawner.OnFoodCollected(spawner.GetComponentInChildren<Food>(true));
            Assert(body.NodeCount == initialNodes + GameConstants.SegmentsPerFood, "Food collection should grow snake.");
            Assert(combo.CurrentComboCount == 1, "Food collection should build combo.");

            body.Grow(level.targetLength);
            SetPrivateField(levelManager, "_levelStartTime", Time.time - 3f);
            InvokePrivate(levelManager, "CheckLevelCompletion");
            Assert(completed, "Reaching target length after food collection should complete level.");
            Assert(stateManager.CurrentState == GameState.LevelComplete, "Completed level should enter LevelComplete state.");

            combo.Reset();
            for (int i = 0; i < 5; i++)
                combo.OnFoodEaten();
            Assert(combo.ActivateDash(), "Full combo should activate dash.");
            bool prevented = combo.TryPreventDeath();
            Assert(prevented, "Active dash should prevent a fatal collision.");

            Assert(slide != null && grapple != null, "Player should carry slide and grapple abilities.");
        }

        private static LevelData CreateSmokeLevel()
        {
            var level = ScriptableObject.CreateInstance<LevelData>();
            level.levelId = "smoke_level";
            level.displayName = "Smoke Level";
            level.zone = ZoneType.PolyCity;
            level.zoneOrder = 1;
            level.targetLength = GameConstants.InitialSegmentCount + GameConstants.SegmentsPerFood;
            level.targetTime = 120f;
            level.maxDeaths = 3;
            level.enableDash = true;
            level.enableSlide = true;
            level.enableGrapple = true;
            level.enableWallRun = true;
            return level;
        }

        private static Food CreateFoodPrefab()
        {
            var food = new GameObject("SmokeFoodPrefab");
            food.SetActive(false);
            var collider = food.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            return food.AddComponent<Food>();
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

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
                throw new MissingFieldException(target.GetType().Name, fieldName);
            field.SetValue(target, value);
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
