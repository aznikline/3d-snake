using UnityEngine;
using NeonSerpent.Core;
using NeonSerpent.Player;

namespace NeonSerpent.Gameplay
{
    /// <summary>
    /// Spawns food (energy cores) in valid locations within the level bounds.
    /// Ensures food does not overlap with the snake body or environment.
    /// </summary>
    public class FoodSpawner : MonoBehaviour
    {
        [Header("Spawning")]
        [SerializeField] private Food foodPrefab;
        [SerializeField] private int maxFoodCount = 3;
        [SerializeField] private float spawnRadius = 20f;
        [SerializeField] private float spawnHeightMin = 1f;
        [SerializeField] private float spawnHeightMax = 10f;

        [Header("References")]
        [SerializeField] private VerletSnakeBody snakeBody;
        [SerializeField] private Transform levelCenter;

        [Header("Difficulty Scaling")]
        [SerializeField] private bool scaleWithSnakeLength = true;
        [SerializeField] private float difficultyMultiplier = 1f;

        private int _currentFoodCount;

        private void Start()
        {
            // Initial spawn
            for (int i = 0; i < maxFoodCount; i++)
            {
                SpawnFood();
            }
        }

        /// <summary>
        /// Called when a food is collected. Schedules replacement spawn.
        /// </summary>
        public void OnFoodCollected(Food food)
        {
            _currentFoodCount--;
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
            newFood.OnCollected += OnFoodCollected;
            _currentFoodCount++;
        }

        private Vector3? FindValidSpawnPosition()
        {
            Vector3 center = levelCenter != null ? levelCenter.position : Vector3.zero;

            for (int attempt = 0; attempt < GameConstants.FoodSpawnMaxRetries; attempt++)
            {
                Vector3 randomPos = center + Random.insideUnitSphere * spawnRadius;
                randomPos.y = Mathf.Clamp(randomPos.y, spawnHeightMin, spawnHeightMax);

                if (IsPositionValid(randomPos))
                {
                    return randomPos;
                }
            }

            return null;
        }

        private bool IsPositionValid(Vector3 position)
        {
            // Check distance from snake head
            if (snakeBody != null && snakeBody.NodeCount > 0)
            {
                float distToHead = Vector3.Distance(position, snakeBody.Nodes[0].Position);
                if (distToHead < GameConstants.FoodMinDistanceFromSnake)
                    return false;
            }

            // Check overlap with environment
            if (Physics.CheckSphere(position, 0.5f, LayerMask.GetMask("Environment")))
                return false;

            // Check overlap with existing food
            Collider[] nearbyFood = Physics.OverlapSphere(position, 1f, LayerMask.GetMask("Food"));
            if (nearbyFood.Length > 0)
                return false;

            // Ensure position is reachable (raycast down to find ground)
            if (!Physics.Raycast(position + Vector3.up * 5f, Vector3.down, out RaycastHit hit, 10f, LayerMask.GetMask("Environment")))
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
