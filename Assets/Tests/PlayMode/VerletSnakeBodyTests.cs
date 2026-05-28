using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonSerpent.Player;
using NeonSerpent.Core;

namespace NeonSerpent.Tests.PlayMode
{
    /// <summary>
    /// Play-mode tests for VerletSnakeBody physics simulation.
    /// These tests run in the Unity runtime and verify actual physics behavior.
    /// </summary>
    public class VerletSnakeBodyTests
    {
        private GameObject _snakeGO;
        private VerletSnakeBody _snakeBody;

        [SetUp]
        public void Setup()
        {
            _snakeGO = new GameObject("TestSnake");
            _snakeBody = _snakeGO.AddComponent<VerletSnakeBody>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_snakeGO);
        }

        [UnityTest]
        public IEnumerator SnakeBody_InitializesWithCorrectSegmentCount()
        {
            yield return null; // Wait one frame for Start()

            Assert.AreEqual(GameConstants.InitialSegmentCount, _snakeBody.NodeCount,
                "Snake should initialize with the correct number of segments.");
        }

        [UnityTest]
        public IEnumerator SnakeBody_Grow_IncreasesNodeCount()
        {
            yield return null;

            int initialCount = _snakeBody.NodeCount;
            _snakeBody.Grow(5);

            Assert.AreEqual(initialCount + 5, _snakeBody.NodeCount,
                "Growing by 5 segments should increase node count by 5.");
        }

        [UnityTest]
        public IEnumerator SnakeBody_NodesFollowHead()
        {
            yield return null;

            Vector3 headPos = new Vector3(10f, 0f, 10f);
            _snakeBody.SetHeadPosition(headPos);

            // Simulate several frames
            for (int i = 0; i < 30; i++)
            {
                headPos += Vector3.forward * 0.5f;
                _snakeBody.SetHeadPosition(headPos);
                yield return null;
            }

            // Check that nodes are roughly following the head path
            var nodes = _snakeBody.Nodes;
            Assert.That(Vector3.Distance(nodes[0].Position, headPos), Is.LessThan(0.1f),
                "Head node should be at the set head position.");

            // Nodes should be spaced approximately by segment length
            for (int i = 1; i < Mathf.Min(5, nodes.Count); i++)
            {
                float distance = Vector3.Distance(nodes[i - 1].Position, nodes[i].Position);
                Assert.That(distance, Is.InRange(GameConstants.SegmentLength * 0.8f, GameConstants.SegmentLength * 1.2f),
                    $"Node {i} should be approximately one segment length from node {i - 1}.");
            }
        }

        [UnityTest]
        public IEnumerator SnakeBody_SelfCollision_DetectsWhenHeadHitsBody()
        {
            yield return null;

            // Grow snake to make it long enough
            _snakeBody.Grow(50);

            // Move in a tight circle to create a loop
            Vector3 center = Vector3.zero;
            float radius = 2f;

            for (int frame = 0; frame < 120; frame++)
            {
                float angle = frame * 0.1f;
                Vector3 headPos = center + new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                _snakeBody.SetHeadPosition(headPos);
                yield return null;
            }

            int collisionIndex = _snakeBody.CheckSelfCollision();
            Assert.GreaterOrEqual(collisionIndex, 0,
                "Self-collision should be detected when snake forms a loop.");
        }

        [UnityTest]
        public IEnumerator SnakeBody_SelfCollision_NoFalsePositiveForShortSnake()
        {
            yield return null;

            // Short snake should not self-collide
            int collisionIndex = _snakeBody.CheckSelfCollision();
            Assert.AreEqual(-1, collisionIndex,
                "Short snake should not detect self-collision.");
        }

        [UnityTest]
        public IEnumerator SnakeBody_Reset_ClearsAndReinitializes()
        {
            yield return null;

            _snakeBody.Grow(20);
            Assert.Greater(_snakeBody.NodeCount, GameConstants.InitialSegmentCount);

            Vector3 resetPos = new Vector3(5f, 5f, 5f);
            _snakeBody.ResetBody(resetPos);

            Assert.AreEqual(GameConstants.InitialSegmentCount, _snakeBody.NodeCount,
                "Reset should restore initial segment count.");

            Assert.That(Vector3.Distance(_snakeBody.Nodes[0].Position, resetPos), Is.LessThan(0.1f),
                "Head node should be at reset position.");
        }

        [UnityTest]
        public IEnumerator SnakeBody_Performance_100NodesAt60FPS()
        {
            yield return null;

            _snakeBody.Grow(90); // Total 100 nodes

            float startTime = Time.realtimeSinceStartup;
            int frames = 0;

            while (frames < 60)
            {
                Vector3 headPos = new Vector3(Mathf.Sin(frames * 0.1f) * 5f, 0f, frames * 0.2f);
                _snakeBody.SetHeadPosition(headPos);
                yield return null;
                frames++;
            }

            float elapsed = Time.realtimeSinceStartup - startTime;
            float averageFrameTime = elapsed / frames;

            Assert.That(averageFrameTime, Is.LessThan(0.0167f),
                $"Average frame time ({averageFrameTime * 1000f:F2}ms) should be under 16.67ms (60fps) for 100 nodes.");
        }
    }
}
