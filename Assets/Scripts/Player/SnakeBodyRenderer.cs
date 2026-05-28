using System.Collections.Generic;
using UnityEngine;

namespace NeonSerpent.Player
{
    /// <summary>
    /// Generates a smooth tubular mesh from Verlet snake nodes using
    /// Catmull-Rom spline interpolation. Supports dynamic segment count
    /// and vertex-color-based emission for neon effects.
    /// </summary>
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class SnakeBodyRenderer : MonoBehaviour
    {
        [Header("Mesh Generation")]
        [SerializeField] private int radialSegments = 8;
        [SerializeField] private int splineSubdivisions = 4;
        [SerializeField] private float baseRadius = 0.3f;
        [SerializeField] private float radiusTaper = 0.5f;

        [Header("Visuals")]
        [SerializeField] private Material snakeMaterial;
        [SerializeField] private Color headColor = Color.cyan;
        [SerializeField] private Color tailColor = Color.magenta;
        [SerializeField] private float emissionIntensity = 2f;

        [Header("References")]
        [SerializeField] private VerletSnakeBody snakeBody;

        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            _mesh = new Mesh { name = "SnakeBody" };
            _meshFilter.mesh = _mesh;

            if (snakeMaterial != null)
                _meshRenderer.material = snakeMaterial;
        }

        private void Update()
        {
            if (snakeBody == null || snakeBody.NodeCount < 2) return;

            GenerateMesh();
        }

        private void GenerateMesh()
        {
            var nodes = snakeBody.Nodes;
            int nodeCount = nodes.Count;

            // Build smooth spline points from Verlet nodes
            List<Vector3> splinePoints = new List<Vector3>();
            List<float> splineRadii = new List<float>();

            for (int i = 0; i < nodeCount - 1; i++)
            {
                Vector3 p0 = i > 0 ? nodes[i - 1].Position : nodes[i].Position;
                Vector3 p1 = nodes[i].Position;
                Vector3 p2 = nodes[i + 1].Position;
                Vector3 p3 = i < nodeCount - 2 ? nodes[i + 2].Position : nodes[i + 1].Position;

                for (int s = 0; s < splineSubdivisions; s++)
                {
                    float t = s / (float)splineSubdivisions;
                    Vector3 point = CatmullRom(p0, p1, p2, p3, t);
                    float radius = Mathf.Lerp(nodes[i].Radius, nodes[i + 1].Radius, t);
                    splinePoints.Add(point);
                    splineRadii.Add(radius);
                }
            }

            // Add final node
            splinePoints.Add(nodes[nodeCount - 1].Position);
            splineRadii.Add(nodes[nodeCount - 1].Radius);

            BuildTubeMesh(splinePoints, splineRadii);
        }

        private void BuildTubeMesh(List<Vector3> centers, List<float> radii)
        {
            int pathCount = centers.Count;
            int vertexCount = pathCount * (radialSegments + 1);
            int triangleCount = (pathCount - 1) * radialSegments * 6;

            Vector3[] vertices = new Vector3[vertexCount];
            Vector3[] normals = new Vector3[vertexCount];
            Color[] colors = new Color[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            int[] triangles = new int[triangleCount];

            // Generate vertices
            for (int i = 0; i < pathCount; i++)
            {
                Vector3 center = centers[i];
                float radius = radii[i] * Mathf.Lerp(1f, radiusTaper, i / (float)pathCount);

                // Frame calculation
                Vector3 forward = Vector3.forward;
                if (i < pathCount - 1)
                    forward = (centers[i + 1] - center).normalized;
                else if (i > 0)
                    forward = (center - centers[i - 1]).normalized;

                Vector3 up = Vector3.up;
                if (Mathf.Abs(Vector3.Dot(forward, up)) > 0.99f)
                    up = Vector3.right;

                Vector3 right = Vector3.Cross(forward, up).normalized;
                up = Vector3.Cross(right, forward).normalized;

                float t = i / (float)(pathCount - 1);
                Color vertexColor = Color.Lerp(headColor, tailColor, t);
                vertexColor *= emissionIntensity;

                for (int r = 0; r <= radialSegments; r++)
                {
                    float angle = (r / (float)radialSegments) * Mathf.PI * 2f;
                    Vector3 offset = (right * Mathf.Cos(angle) + up * Mathf.Sin(angle)) * radius;

                    int vi = i * (radialSegments + 1) + r;
                    vertices[vi] = center + offset;
                    normals[vi] = offset.normalized;
                    colors[vi] = vertexColor;
                    uvs[vi] = new Vector2(r / (float)radialSegments, t);
                }
            }

            // Generate triangles
            int ti = 0;
            for (int i = 0; i < pathCount - 1; i++)
            {
                for (int r = 0; r < radialSegments; r++)
                {
                    int current = i * (radialSegments + 1) + r;
                    int next = current + radialSegments + 1;

                    triangles[ti++] = current;
                    triangles[ti++] = next;
                    triangles[ti++] = current + 1;

                    triangles[ti++] = current + 1;
                    triangles[ti++] = next;
                    triangles[ti++] = next + 1;
                }
            }

            _mesh.Clear();
            _mesh.vertices = vertices;
            _mesh.normals = normals;
            _mesh.colors = colors;
            _mesh.uv = uvs;
            _mesh.triangles = triangles;
            _mesh.RecalculateBounds();
        }

        /// <summary>
        /// Catmull-Rom spline interpolation for smooth path generation.
        /// </summary>
        private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2f * p1) +
                (-p0 + p2) * t +
                (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2 +
                (-p0 + 3f * p1 - 3f * p2 + p3) * t3
            );
        }

        /// <summary>
        /// Set emission intensity dynamically (e.g., during dash).
        /// </summary>
        public void SetEmissionIntensity(float intensity)
        {
            emissionIntensity = intensity;
        }
    }
}
