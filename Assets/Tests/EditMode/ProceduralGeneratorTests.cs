using NUnit.Framework;
using UnityEngine;

namespace NeonSerpent.Tests.EditMode
{
    /// <summary>
    /// Edit-mode tests for procedural level generation logic.
    /// </summary>
    public class ProceduralGeneratorTests
    {
        [Test]
        public void Generator_CreatePath_ReturnsValidPath()
        {
            // This is a placeholder test for the procedural generator
            // Actual implementation will be created in Phase 2 (Unit 7)

            // Arrange
            Vector3 startPos = Vector3.zero;
            int targetLength = 100;

            // Act - simulate path generation
            Vector3[] path = GenerateSimplePath(startPos, targetLength);

            // Assert
            Assert.IsNotNull(path);
            Assert.Greater(path.Length, 0);
            Assert.AreEqual(startPos, path[0], "Path should start at the specified position.");
        }

        [Test]
        public void Generator_Path_NoOverlappingSegments()
        {
            Vector3[] path = GenerateSimplePath(Vector3.zero, 50);

            // Check that no two adjacent segments overlap
            for (int i = 1; i < path.Length; i++)
            {
                float distance = Vector3.Distance(path[i - 1], path[i]);
                Assert.Greater(distance, 0.1f, $"Segment {i} should not overlap with segment {i - 1}.");
            }
        }

        [Test]
        public void Generator_Path_StaysWithinBounds()
        {
            Vector3 startPos = new Vector3(50f, 5f, 50f);
            float maxDistance = 200f;

            Vector3[] path = GenerateSimplePath(startPos, 100);

            foreach (var point in path)
            {
                float distFromStart = Vector3.Distance(startPos, point);
                Assert.LessOrEqual(distFromStart, maxDistance,
                    "Path should stay within reasonable bounds from start.");
            }
        }

        // ── Helper: Simple path generator for testing ──

        private Vector3[] GenerateSimplePath(Vector3 start, int length)
        {
            Vector3[] path = new Vector3[length];
            path[0] = start;

            Vector3[] directions = new[]
            {
                Vector3.forward, Vector3.back,
                Vector3.right, Vector3.left,
                Vector3.up, Vector3.down
            };

            Vector3 currentPos = start;
            Vector3 lastDirection = Vector3.forward;

            for (int i = 1; i < length; i++)
            {
                // Pick a random direction, avoiding immediate reversal
                Vector3 direction;
                do
                {
                    direction = directions[Random.Range(0, directions.Length)];
                } while (direction == -lastDirection);

                currentPos += direction * 2f; // 2-unit segments
                path[i] = currentPos;
                lastDirection = direction;
            }

            return path;
        }
    }
}
