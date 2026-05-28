using System.Collections.Generic;
using UnityEngine;
using NeonSerpent.Core;
using NeonSerpent.Player;

namespace NeonSerpent.Level
{
    /// <summary>
    /// Procedural level generator for Endless mode.
    /// Uses rule-based tile placement with difficulty scaling.
    /// Generates path segments ahead of the player and cleans up behind.
    /// </summary>
    public class ProceduralGenerator : MonoBehaviour
    {
        [Header("Generation")]
        [SerializeField] private int preloadDistance = 100;
        [SerializeField] private int cleanupDistance = 50;
        [SerializeField] private float segmentLength = 10f;
        [SerializeField] private float minPathWidth = 3f;
        [SerializeField] private float maxPathWidth = 8f;

        [Header("Difficulty Scaling")]
        [SerializeField] private float baseObstacleDensity = 0.1f;
        [SerializeField] private float maxObstacleDensity = 0.5f;
        [SerializeField] private float baseVerticalVariation = 2f;
        [SerializeField] private float maxVerticalVariation = 8f;

        [Header("Prefabs")]
        [SerializeField] private GameObject[] floorPrefabs;
        [SerializeField] private GameObject[] wallPrefabs;
        [SerializeField] private GameObject[] obstaclePrefabs;
        [SerializeField] private GameObject[] decorationPrefabs;

        [Header("References")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private VerletSnakeBody snakeBody;

        private Queue<GameObject> _activeSegments = new Queue<GameObject>();
        private Vector3 _lastGeneratedPosition;
        private float _currentPathWidth;
        private float _currentDifficulty;
        private int _segmentsGenerated;

        public float CurrentDifficulty => _currentDifficulty;

        private void Start()
        {
            _lastGeneratedPosition = playerTransform?.position ?? Vector3.zero;
            _currentPathWidth = maxPathWidth;
            _currentDifficulty = 0f;

            // Generate initial segments
            GenerateInitialSegments();
        }

        private void Update()
        {
            if (playerTransform == null) return;

            float playerZ = playerTransform.position.z;

            // Generate ahead
            while (_lastGeneratedPosition.z - playerZ < preloadDistance)
            {
                GenerateSegment();
            }

            // Cleanup behind
            CleanupOldSegments(playerZ);

            // Update difficulty based on snake length
            UpdateDifficulty();
        }

        private void GenerateInitialSegments()
        {
            for (int i = 0; i < 5; i++)
            {
                GenerateSegment();
            }
        }

        private void GenerateSegment()
        {
            Vector3 segmentPos = _lastGeneratedPosition;
            GameObject segment = new GameObject($"Segment_{_segmentsGenerated}");
            segment.transform.position = segmentPos;
            segment.transform.SetParent(transform);

            // Generate floor
            GenerateFloor(segment.transform, segmentPos);

            // Generate walls
            GenerateWalls(segment.transform, segmentPos);

            // Generate obstacles based on difficulty
            if (Random.value < GetCurrentObstacleDensity())
            {
                GenerateObstacle(segment.transform, segmentPos);
            }

            // Generate decorations
            if (Random.value < 0.3f)
            {
                GenerateDecoration(segment.transform, segmentPos);
            }

            _activeSegments.Enqueue(segment);
            _lastGeneratedPosition += Vector3.forward * segmentLength;
            _segmentsGenerated++;
        }

        private void GenerateFloor(Transform parent, Vector3 position)
        {
            if (floorPrefabs.Length == 0) return;

            GameObject floor = Instantiate(
                floorPrefabs[Random.Range(0, floorPrefabs.Length)],
                position,
                Quaternion.identity,
                parent
            );
            floor.transform.localScale = new Vector3(_currentPathWidth, 0.2f, segmentLength);
        }

        private void GenerateWalls(Transform parent, Vector3 position)
        {
            if (wallPrefabs.Length == 0) return;

            float halfWidth = _currentPathWidth * 0.5f;
            float wallHeight = 5f + Random.Range(0f, GetCurrentVerticalVariation());

            // Left wall
            Vector3 leftPos = position + Vector3.left * halfWidth;
            GameObject leftWall = Instantiate(
                wallPrefabs[Random.Range(0, wallPrefabs.Length)],
                leftPos,
                Quaternion.identity,
                parent
            );
            leftWall.transform.localScale = new Vector3(0.5f, wallHeight, segmentLength);

            // Right wall
            Vector3 rightPos = position + Vector3.right * halfWidth;
            GameObject rightWall = Instantiate(
                wallPrefabs[Random.Range(0, wallPrefabs.Length)],
                rightPos,
                Quaternion.identity,
                parent
            );
            rightWall.transform.localScale = new Vector3(0.5f, wallHeight, segmentLength);
        }

        private void GenerateObstacle(Transform parent, Vector3 position)
        {
            if (obstaclePrefabs.Length == 0) return;

            float halfWidth = _currentPathWidth * 0.5f - 1f;
            Vector3 offset = new Vector3(
                Random.Range(-halfWidth, halfWidth),
                0f,
                Random.Range(-segmentLength * 0.3f, segmentLength * 0.3f)
            );

            GameObject obstacle = Instantiate(
                obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)],
                position + offset,
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f),
                parent
            );
        }

        private void GenerateDecoration(Transform parent, Vector3 position)
        {
            if (decorationPrefabs.Length == 0) return;

            Vector3 offset = new Vector3(
                Random.Range(-_currentPathWidth, _currentPathWidth),
                Random.Range(2f, 5f),
                Random.Range(-segmentLength * 0.4f, segmentLength * 0.4f)
            );

            GameObject decoration = Instantiate(
                decorationPrefabs[Random.Range(0, decorationPrefabs.Length)],
                position + offset,
                Quaternion.identity,
                parent
            );
        }

        private void CleanupOldSegments(float playerZ)
        {
            while (_activeSegments.Count > 0)
            {
                GameObject segment = _activeSegments.Peek();
                if (segment == null)
                {
                    _activeSegments.Dequeue();
                    continue;
                }

                if (segment.transform.position.z < playerZ - cleanupDistance)
                {
                    _activeSegments.Dequeue();
                    Destroy(segment);
                }
                else
                {
                    break;
                }
            }
        }

        private void UpdateDifficulty()
        {
            if (snakeBody == null) return;

            // Difficulty scales with snake length
            float lengthFactor = Mathf.Clamp01((float)snakeBody.NodeCount / GameConstants.MaxNodesTarget);
            _currentDifficulty = lengthFactor;

            // Path width narrows as difficulty increases
            _currentPathWidth = Mathf.Lerp(maxPathWidth, minPathWidth, lengthFactor);
        }

        private float GetCurrentObstacleDensity()
        {
            return Mathf.Lerp(baseObstacleDensity, maxObstacleDensity, _currentDifficulty);
        }

        private float GetCurrentVerticalVariation()
        {
            return Mathf.Lerp(baseVerticalVariation, maxVerticalVariation, _currentDifficulty);
        }

        /// <summary>
        /// Reset the generator for a new endless run.
        /// </summary>
        public void ResetGenerator()
        {
            // Clear all active segments
            while (_activeSegments.Count > 0)
            {
                GameObject segment = _activeSegments.Dequeue();
                if (segment != null)
                    Destroy(segment);
            }

            _lastGeneratedPosition = playerTransform?.position ?? Vector3.zero;
            _currentPathWidth = maxPathWidth;
            _currentDifficulty = 0f;
            _segmentsGenerated = 0;

            GenerateInitialSegments();
        }
    }
}
