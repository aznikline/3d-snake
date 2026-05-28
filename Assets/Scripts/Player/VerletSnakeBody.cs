using System.Collections.Generic;
using UnityEngine;
using NeonSerpent.Core;

namespace NeonSerpent.Player
{
    /// <summary>
    /// Custom Verlet-integration snake body. Manages a chain of nodes that
    /// follow the head with inertia, constraint solving, and collision response.
    /// Designed to support 200+ nodes at 60fps.
    /// </summary>
    public class VerletSnakeBody : MonoBehaviour
    {
        [Header("Verlet Settings")]
        [SerializeField] private float segmentLength = GameConstants.SegmentLength;
        [SerializeField] private int constraintIterations = GameConstants.VerletIterations;
        [SerializeField] private float stiffness = GameConstants.VerletStiffness;
        [SerializeField] private float damping = GameConstants.VerletDamping;
        [SerializeField] private float gravity = -5f;

        [Header("Collision")]
        [SerializeField] private LayerMask environmentLayer;
        [SerializeField] private float collisionOffset = 0.05f;

        [Header("Growth")]
        [SerializeField] private int initialSegments = GameConstants.InitialSegmentCount;

        private List<SnakeNode> _nodes = new List<SnakeNode>();
        private Vector3 _headPosition;
        private bool _headPositionSet;

        public IReadOnlyList<SnakeNode> Nodes => _nodes;
        public int NodeCount => _nodes.Count;
        public float SegmentLength => segmentLength;

        private void Start()
        {
            InitializeNodes(initialSegments);
        }

        private void Update()
        {
            if (!_headPositionSet || _nodes.Count == 0) return;

            Simulate(Time.deltaTime);
            SolveConstraints();
            ResolveCollisions();
        }

        /// <summary>
        /// Called by SnakeHeadController every frame to sync the head node.
        /// </summary>
        public void SetHeadPosition(Vector3 position)
        {
            _headPosition = position;
            _headPositionSet = true;
        }

        /// <summary>
        /// Grow the snake by adding new segments at the tail.
        /// </summary>
        public void Grow(int segments)
        {
            Vector3 tailPos = _nodes.Count > 0 ? _nodes[_nodes.Count - 1].Position : _headPosition;

            for (int i = 0; i < segments; i++)
            {
                _nodes.Add(new SnakeNode
                {
                    Position = tailPos,
                    PreviousPosition = tailPos,
                    Radius = Mathf.Lerp(GameConstants.NodeRadiusHead, GameConstants.NodeRadiusTail,
                        (float)_nodes.Count / GameConstants.MaxNodesTarget)
                });
            }
        }

        /// <summary>
        /// Reset the snake to initial length and position.
        /// </summary>
        public void ResetBody(Vector3 headPosition)
        {
            _nodes.Clear();
            _headPosition = headPosition;
            _headPositionSet = true;
            InitializeNodes(initialSegments);
        }

        /// <summary>
        /// Check if the head is colliding with any body segment (self-collision).
        /// Returns the index of the first colliding segment, or -1 if none.
        /// </summary>
        public int CheckSelfCollision()
        {
            if (_nodes.Count < 10) return -1; // Too short to self-collide

            Vector3 headPos = _nodes[0].Position;
            float headRadius = _nodes[0].Radius;

            // Skip the first several nodes (neck area)
            int skipCount = Mathf.Min(8, _nodes.Count / 4);

            for (int i = skipCount; i < _nodes.Count; i++)
            {
                float combinedRadius = headRadius + _nodes[i].Radius;
                if (Vector3.SqrMagnitude(headPos - _nodes[i].Position) < combinedRadius * combinedRadius)
                {
                    return i;
                }
            }

            return -1;
        }

        // ── Private Simulation ──

        private void InitializeNodes(int count)
        {
            Vector3 startPos = _headPositionSet ? _headPosition : transform.position;

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = startPos - Vector3.forward * (segmentLength * i);
                _nodes.Add(new SnakeNode
                {
                    Position = pos,
                    PreviousPosition = pos,
                    Radius = Mathf.Lerp(GameConstants.NodeRadiusHead, GameConstants.NodeRadiusTail,
                        (float)i / GameConstants.MaxNodesTarget)
                });
            }
        }

        private void Simulate(float dt)
        {
            // Pin head to controller position
            _nodes[0].Position = _headPosition;

            // Verlet integration for all other nodes
            for (int i = 1; i < _nodes.Count; i++)
            {
                ref var node = ref _nodes[i];
                Vector3 velocity = (node.Position - node.PreviousPosition) * damping;
                node.PreviousPosition = node.Position;
                node.Position += velocity + Vector3.up * gravity * dt * dt;
            }
        }

        private void SolveConstraints()
        {
            for (int iteration = 0; iteration < constraintIterations; iteration++)
            {
                // Forward pass: head -> tail
                for (int i = 1; i < _nodes.Count; i++)
                {
                    SolveDistanceConstraint(i - 1, i);
                }

                // Backward pass: tail -> head (for better stiffness)
                for (int i = _nodes.Count - 1; i > 0; i--)
                {
                    SolveDistanceConstraint(i - 1, i);
                }
            }
        }

        private void SolveDistanceConstraint(int indexA, int indexB)
        {
            ref var nodeA = ref _nodes[indexA];
            ref var nodeB = ref _nodes[indexB];

            Vector3 delta = nodeB.Position - nodeA.Position;
            float currentDistance = delta.magnitude;
            float error = currentDistance - segmentLength;

            if (currentDistance < 0.0001f) return;

            Vector3 correction = delta.normalized * error * stiffness;

            // Head node (index 0) is immovable
            if (indexA != 0)
            {
                nodeA.Position += correction * 0.5f;
            }

            nodeB.Position -= correction * 0.5f;
        }

        private void ResolveCollisions()
        {
            for (int i = 1; i < _nodes.Count; i++)
            {
                ref var node = ref _nodes[i];

                if (Physics.SphereCast(
                    node.Position + Vector3.up * node.Radius,
                    node.Radius,
                    Vector3.down,
                    out RaycastHit hit,
                    node.Radius * 2f,
                    environmentLayer))
                {
                    // Push node out of collision along surface normal
                    float penetration = node.Radius - hit.distance + collisionOffset;
                    if (penetration > 0)
                    {
                        node.Position += hit.normal * penetration;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Single node in the Verlet snake chain.
    /// </summary>
    public struct SnakeNode
    {
        public Vector3 Position;
        public Vector3 PreviousPosition;
        public float Radius;
    }
}
