using UnityEngine;
using NeonSerpent.Core;
using NeonSerpent.Player;
using NeonSerpent.Level;
using NeonSerpent.Procedural.Audio;

namespace NeonSerpent.Gameplay
{
    /// <summary>
    /// Spawns food (energy cores) in valid locations within the level bounds.
    /// Ensures food does not overlap with the snake body or environment.
    /// </summary>
    public class FoodSpawner : MonoBehaviour
    {
        [Header("Spawning")]
        public Food foodPrefab;
        [SerializeField] private int maxFoodCount = 3;
        [SerializeField] private float spawnRadius = 20f;
        [SerializeField] private float spawnHeightMin = 1f;
        [SerializeField] private float spawnHeightMax = 10f;

        [Header("References")]
        public VerletSnakeBody snakeBody;
        public ComboSystem comboSystem;
        public LevelManager levelManager;

        [SerializeField] private Transform levelCenter;

        [Header("Difficulty Scaling")]
        [SerializeField] private bool scaleWithSnakeLength = true;
        [SerializeField] private float difficultyMultiplier = 1f;

        private int _currentFoodCount;
        private bool _initialized;

        /// <summary>
        /// Called by GameBootstrap after all references are wired.
        /// </summary>
        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            for (int i = 0; i < maxFoodCount; i++)
            {
                SpawnFood();
            }
        }

        /// <summary>
        /// Clear active food and pending respawns before starting or restarting a level.
        /// </summary>
        public void ResetSpawner()
        {
            CancelInvoke(nameof(SpawnFood));

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i);
                if (child.GetComponent<Food>() == null) continue;

                if (Application.isPlaying)
                    Destroy(child.gameObject);
                else
                    DestroyImmediate(child.gameObject);
            }

            _currentFoodCount = 0;
            _initialized = false;
        }

        /// <summary>
        /// Called when a food is collected. Grows snake, builds combo, schedules replacement spawn.
        /// </summary>
        public void OnFoodCollected(Food food)
        {
            _currentFoodCount = Mathf.Max(0, _currentFoodCount - 1);
            Vector3 collectPosition = food != null ? food.transform.position : transform.position;

            // Grow the snake
            snakeBody?.Grow(GameConstants.SegmentsPerFood);

            // Build combo
            comboSystem?.OnFoodEaten();
            int comboLevel = comboSystem != null ? comboSystem.CurrentComboCount : 0;
            ProceduralSFXSystem.Instance?.PlayFoodCollect(collectPosition, comboLevel);

            // Notify scoring systems
            levelManager?.OnFoodCollected();
            Invoke(nameof(SpawnFood), GameConstants.FoodSpawnDelay);
        }

        private void SpawnFood()
        {
            if (_currentFoodCount >= maxFoodCount) return;
            if (foodPrefab == null) return;

            Vector3? spawnPos = FindValidSpawnPosition();
            if (!spawnPos.HasValue)
            {
                Debug.LogWarning("[FoodSpawner] Could not find valid spawn position after max retries.");
                return;
            }

            Food newFood = Instantiate(foodPrefab, spawnPos.Value, Quaternion.identity, transform);
            newFood.gameObject.SetActive(true);
            newFood.OnCollected += OnFoodCollected;
            _currentFoodCount++;
            levelManager?.OnFoodSpawned();
        }

        private Vector3? FindValidSpawnPosition()
        {
            Vector3 center = levelCenter != null ? levelCenter.position : Vector3.zero;

            for (int attempt = 0; attempt < GameConstants.FoodSpawnMaxRetries; attempt++)
            {
                Vector2 circle = Random.insideUnitCircle * spawnRadius;
                Vector3 randomPos = center + new Vector3(circle.x, 1.5f, circle.y);

                if (IsPositionValid(randomPos))
                {
                    return randomPos;
                }
            }

            return null;
        }

        private bool IsPositionValid(Vector3 position)
        {
            if (snakeBody != null && snakeBody.NodeCount > 0)
            {
                float distToHead = Vector3.Distance(position, snakeBody.Nodes[0].Position);
                if (distToHead < GameConstants.FoodMinDistanceFromSnake)
                    return false;
            }

            Collider[] nearbyFood = Physics.OverlapSphere(position, 1f, LayerMask.GetMask("Food"));
            if (nearbyFood.Length > 0)
                return false;

            return true;
        }

        /// <summary>
        /// Increase difficulty by reducing spawn radius or adding constraints.
        /// Called as snake grows in endless mode.
        /// </summary>
        public void IncreaseDifficulty(float multiplier)
        {
            difficultyMultiplier = multiplier;
            spawnRadius = Mathf.Max(5f, spawnRadius / multiplier);
        }
    }
}
