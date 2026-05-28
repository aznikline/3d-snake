using System.Collections.Generic;
using UnityEngine;

namespace NeonSerpent.Procedural.Art.Environment
{
    /// <summary>
    /// Generates procedural cyberpunk buildings at runtime.
    /// Creates building shells with neon trim, windows, and variation
    /// without requiring any external 3D models.
    /// </summary>
    public class ProceduralBuildingGenerator : MonoBehaviour
    {
        [Header("Building Dimensions")]
        [SerializeField] private float minWidth = 8f;
        [SerializeField] private float maxWidth = 20f;
        [SerializeField] private float minDepth = 8f;
        [SerializeField] private float maxDepth = 20f;
        [SerializeField] private float minHeight = 20f;
        [SerializeField] private float maxHeight = 80f;

        [Header("Neon Trim")]
        [SerializeField] private float neonTrimWidth = 0.3f;
        [SerializeField] private float neonEmissionIntensity = 3f;
        [SerializeField] private Color[] neonColors = new[]
        {
            new Color(0f, 1f, 1f),    // Cyan
            new Color(1f, 0f, 0.8f),  // Magenta
            new Color(1f, 0.5f, 0f),  // Orange
            new Color(0.5f, 0f, 1f),  // Purple
            new Color(0f, 1f, 0.5f)   // Lime
        };

        [Header("Windows")]
        [SerializeField] private float windowDensity = 0.6f;
        [SerializeField] private float windowEmissionChance = 0.4f;
        [SerializeField] private float windowSize = 1.5f;
        [SerializeField] private float windowSpacing = 2.5f;

        [Header("Materials")]
        [SerializeField] private Material buildingBaseMaterial;
        [SerializeField] private Material neonMaterial;
        [SerializeField] private Material windowLitMaterial;
        [SerializeField] private Material windowDarkMaterial;

        [Header("Detail")]
        [SerializeField] private bool addAntennas = true;
        [SerializeField] private bool addBillboards = true;
        [SerializeField] private int maxBillboardsPerBuilding = 3;

        private static int _buildingSeed = 0;

        /// <summary>
        /// Generate a complete building at the specified position.
        /// </summary>
        public GameObject GenerateBuilding(Vector3 position, int? seed = null)
        {
            int buildingSeed = seed ?? _buildingSeed++;
            Random.InitState(buildingSeed);

            float width = Random.Range(minWidth, maxWidth);
            float depth = Random.Range(minDepth, maxDepth);
            float height = Random.Range(minHeight, maxHeight);

            GameObject building = new GameObject($"Building_{buildingSeed}");
            building.transform.position = position;

            // Main building body
            CreateBuildingBody(building.transform, width, depth, height);

            // Neon trim on edges
            CreateNeonTrim(building.transform, width, depth, height);

            // Windows
            CreateWindows(building.transform, width, depth, height);

            // Antennas
            if (addAntennas && Random.value > 0.5f)
            {
                CreateAntennas(building.transform, width, depth, height);
            }

            // Billboards
            if (addBillboards)
            {
                CreateBillboards(building.transform, width, depth, height);
            }

            return building;
        }

        /// <summary>
        /// Generate a city block of buildings.
        /// </summary>
        public List<GameObject> GenerateCityBlock(Vector3 center, int buildingCount, float blockSize)
        {
            var buildings = new List<GameObject>();

            for (int i = 0; i < buildingCount; i++)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-blockSize * 0.5f, blockSize * 0.5f),
                    0f,
                    Random.Range(-blockSize * 0.5f, blockSize * 0.5f)
                );

                var building = GenerateBuilding(center + offset);
                buildings.Add(building);
            }

            return buildings;
        }

        private void CreateBuildingBody(Transform parent, float width, float depth, float height)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(parent);
            body.transform.localPosition = new Vector3(0f, height * 0.5f, 0f);
            body.transform.localScale = new Vector3(width, height, depth);

            // Apply base material
            if (buildingBaseMaterial != null)
            {
                var renderer = body.GetComponent<Renderer>();
                renderer.material = buildingBaseMaterial;
            }
            else
            {
                // Create procedural material
                var renderer = body.GetComponent<Renderer>();
                renderer.material = CreateProceduralBuildingMaterial();
            }

            // Remove collider (we'll use custom level collision)
            Destroy(body.GetComponent<Collider>());
        }

        private void CreateNeonTrim(Transform parent, float width, float depth, float height)
        {
            Color neonColor = neonColors[Random.Range(0, neonColors.Length)];

            // Vertical edges
            Vector3[] edgePositions = new[]
            {
                new Vector3(-width * 0.5f, height * 0.5f, -depth * 0.5f),
                new Vector3(width * 0.5f, height * 0.5f, -depth * 0.5f),
                new Vector3(-width * 0.5f, height * 0.5f, depth * 0.5f),
                new Vector3(width * 0.5f, height * 0.5f, depth * 0.5f)
            };

            foreach (var pos in edgePositions)
            {
                GameObject trim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                trim.name = "NeonTrim";
                trim.transform.SetParent(parent);
                trim.transform.localPosition = pos;
                trim.transform.localScale = new Vector3(neonTrimWidth, height, neonTrimWidth);

                var renderer = trim.GetComponent<Renderer>();
                renderer.material = CreateNeonMaterial(neonColor);
                Destroy(trim.GetComponent<Collider>());
            }

            // Horizontal trim at top
            GameObject topTrim = GameObject.CreatePrimitive(PrimitiveType.Cube);
            topTrim.name = "TopTrim";
            topTrim.transform.SetParent(parent);
            topTrim.transform.localPosition = new Vector3(0f, height, 0f);
            topTrim.transform.localScale = new Vector3(width + neonTrimWidth, neonTrimWidth, depth + neonTrimWidth);

            var topRenderer = topTrim.GetComponent<Renderer>();
            topRenderer.material = CreateNeonMaterial(neonColor);
            Destroy(topTrim.GetComponent<Collider>());
        }

        private void CreateWindows(Transform parent, float width, float depth, float height)
        {
            int floors = Mathf.FloorToInt(height / windowSpacing);
            int windowsPerFloorX = Mathf.FloorToInt(width / windowSpacing);
            int windowsPerFloorZ = Mathf.FloorToInt(depth / windowSpacing);

            for (int floor = 1; floor < floors; floor++)
            {
                float y = floor * windowSpacing;

                // Front and back faces
                for (int x = 0; x < windowsPerFloorX; x++)
                {
                    if (Random.value > windowDensity) continue;

                    float xPos = (x - windowsPerFloorX * 0.5f + 0.5f) * windowSpacing;

                    // Front
                    CreateWindow(parent, new Vector3(xPos, y, depth * 0.5f), new Vector3(windowSize, windowSize, 0.1f));
                    // Back
                    CreateWindow(parent, new Vector3(xPos, y, -depth * 0.5f), new Vector3(windowSize, windowSize, 0.1f));
                }

                // Left and right faces
                for (int z = 0; z < windowsPerFloorZ; z++)
                {
                    if (Random.value > windowDensity) continue;

                    float zPos = (z - windowsPerFloorZ * 0.5f + 0.5f) * windowSpacing;

                    // Right
                    CreateWindow(parent, new Vector3(width * 0.5f, y, zPos), new Vector3(0.1f, windowSize, windowSize));
                    // Left
                    CreateWindow(parent, new Vector3(-width * 0.5f, y, zPos), new Vector3(0.1f, windowSize, windowSize));
                }
            }
        }

        private void CreateWindow(Transform parent, Vector3 position, Vector3 scale)
        {
            bool isLit = Random.value < windowEmissionChance;

            GameObject window = GameObject.CreatePrimitive(PrimitiveType.Cube);
            window.name = isLit ? "WindowLit" : "WindowDark";
            window.transform.SetParent(parent);
            window.transform.localPosition = position;
            window.transform.localScale = scale;

            var renderer = window.GetComponent<Renderer>();
            renderer.material = isLit ? CreateWindowLitMaterial() : CreateWindowDarkMaterial();
            Destroy(window.GetComponent<Collider>());
        }

        private void CreateAntennas(Transform parent, float width, float depth, float height)
        {
            int antennaCount = Random.Range(1, 4);

            for (int i = 0; i < antennaCount; i++)
            {
                float antennaHeight = Random.Range(5f, 15f);
                float xPos = Random.Range(-width * 0.3f, width * 0.3f);
                float zPos = Random.Range(-depth * 0.3f, depth * 0.3f);

                GameObject antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                antenna.name = "Antenna";
                antenna.transform.SetParent(parent);
                antenna.transform.localPosition = new Vector3(xPos, height + antennaHeight * 0.5f, zPos);
                antenna.transform.localScale = new Vector3(0.2f, antennaHeight * 0.5f, 0.2f);

                var renderer = antenna.GetComponent<Renderer>();
                renderer.material = CreateProceduralBuildingMaterial();
                Destroy(antenna.GetComponent<Collider>());

                // Blinking light on top
                GameObject light = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                light.name = "AntennaLight";
                light.transform.SetParent(parent);
                light.transform.localPosition = new Vector3(xPos, height + antennaHeight, zPos);
                light.transform.localScale = Vector3.one * 0.3f;

                var lightRenderer = light.GetComponent<Renderer>();
                lightRenderer.material = CreateNeonMaterial(Color.red);
                Destroy(light.GetComponent<Collider>());
            }
        }

        private void CreateBillboards(Transform parent, float width, float depth, float height)
        {
            int billboardCount = Random.Range(0, maxBillboardsPerBuilding + 1);

            for (int i = 0; i < billboardCount; i++)
            {
                float billboardWidth = Random.Range(4f, 10f);
                float billboardHeight = Random.Range(2f, 5f);
                float yPos = Random.Range(height * 0.3f, height * 0.8f);

                // Pick a random face
                int face = Random.Range(0, 4);
                Vector3 position = face switch
                {
                    0 => new Vector3(0f, yPos, depth * 0.5f + 0.5f),
                    1 => new Vector3(0f, yPos, -depth * 0.5f - 0.5f),
                    2 => new Vector3(width * 0.5f + 0.5f, yPos, 0f),
                    _ => new Vector3(-width * 0.5f - 0.5f, yPos, 0f)
                };

                Vector3 scale = face < 2
                    ? new Vector3(billboardWidth, billboardHeight, 0.2f)
                    : new Vector3(0.2f, billboardHeight, billboardWidth);

                GameObject billboard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                billboard.name = "Billboard";
                billboard.transform.SetParent(parent);
                billboard.transform.localPosition = position;
                billboard.transform.localScale = scale;

                var renderer = billboard.GetComponent<Renderer>();
                renderer.material = CreateBillboardMaterial();
                Destroy(billboard.GetComponent<Collider>());
            }
        }

        // ── Procedural Materials ──

        private Material CreateProceduralBuildingMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.05f, 0.05f, 0.08f); // Dark blue-black
            mat.SetFloat("_Smoothness", 0.8f);
            mat.SetFloat("_Metallic", 0.3f);
            return mat;
        }

        private Material CreateNeonMaterial(Color color)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * neonEmissionIntensity);
            mat.SetFloat("_EmissionIntensity", neonEmissionIntensity);
            return mat;
        }

        private Material CreateWindowLitMaterial()
        {
            Color windowColor = new Color(0.8f, 0.9f, 1f);
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = windowColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", windowColor * 0.5f);
            return mat;
        }

        private Material CreateWindowDarkMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.02f, 0.02f, 0.03f);
            return mat;
        }

        private Material CreateBillboardMaterial()
        {
            Color billboardColor = neonColors[Random.Range(0, neonColors.Length)];
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = billboardColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", billboardColor * 2f);
            return mat;
        }
    }
}
