using UnityEngine;

namespace NeonSerpent.Procedural.Art.Environment
{
    /// <summary>
    /// Generates procedural neon signs for cyberpunk city streets.
    /// Creates various sign types: kanji-style characters, geometric patterns,
    /// and abstract symbols using primitive shapes and emissive materials.
    /// </summary>
    public class ProceduralNeonSignGenerator : MonoBehaviour
    {
        [Header("Sign Dimensions")]
        [SerializeField] private float minWidth = 2f;
        [SerializeField] private float maxWidth = 8f;
        [SerializeField] private float minHeight = 1f;
        [SerializeField] private float maxHeight = 3f;
        [SerializeField] private float thickness = 0.15f;

        [Header("Neon Colors")]
        [SerializeField] private Color[] neonColors = new[]
        {
            new Color(0f, 1f, 1f),      // Cyan
            new Color(1f, 0f, 0.8f),    // Magenta
            new Color(1f, 0.5f, 0f),    // Orange
            new Color(0.5f, 0f, 1f),    // Purple
            new Color(0f, 1f, 0.5f),    // Lime
            new Color(1f, 0.2f, 0.2f),  // Red
            new Color(1f, 1f, 0f)       // Yellow
        };

        [Header("Emission")]
        [SerializeField] private float emissionIntensity = 4f;
        [SerializeField] private float flickerChance = 0.2f;

        [Header("Sign Types")]
        [SerializeField] private float textSignChance = 0.4f;
        [SerializeField] private float geometricSignChance = 0.35f;
        [SerializeField] private float abstractSignChance = 0.25f;

        private static int _signSeed = 0;

        /// <summary>
        /// Generate a random neon sign at the specified position.
        /// </summary>
        public GameObject GenerateSign(Vector3 position, int? seed = null)
        {
            int signSeed = seed ?? _signSeed++;
            Random.InitState(signSeed);

            float width = Random.Range(minWidth, maxWidth);
            float height = Random.Range(minHeight, maxHeight);
            Color color = neonColors[Random.Range(0, neonColors.Length)];

            GameObject sign = new GameObject($"NeonSign_{signSeed}");
            sign.transform.position = position;

            // Choose sign type
            float typeRoll = Random.value;
            if (typeRoll < textSignChance)
            {
                CreateTextSign(sign.transform, width, height, color);
            }
            else if (typeRoll < textSignChance + geometricSignChance)
            {
                CreateGeometricSign(sign.transform, width, height, color);
            }
            else
            {
                CreateAbstractSign(sign.transform, width, height, color);
            }

            // Add flicker effect
            if (Random.value < flickerChance)
            {
                sign.AddComponent<NeonFlickerEffect>().Initialize(color, emissionIntensity);
            }

            return sign;
        }

        private void CreateTextSign(Transform parent, float width, float height, Color color)
        {
            // Create a grid of "pixels" that form abstract text-like shapes
            int cols = Mathf.RoundToInt(width / thickness);
            int rows = Mathf.RoundToInt(height / thickness);

            Material material = CreateNeonMaterial(color);

            // Generate a random pattern that looks like stylized text
            bool[,] pattern = GenerateTextLikePattern(cols, rows);

            for (int x = 0; x < cols; x++)
            {
                for (int y = 0; y < rows; y++)
                {
                    if (pattern[x, y])
                    {
                        Vector3 pixelPos = new Vector3(
                            (x - cols * 0.5f) * thickness,
                            (y - rows * 0.5f) * thickness + height * 0.5f,
                            0f
                        );

                        GameObject pixel = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        pixel.name = "SignPixel";
                        pixel.transform.SetParent(parent);
                        pixel.transform.localPosition = pixelPos;
                        pixel.transform.localScale = Vector3.one * thickness * 0.9f;

                        var renderer = pixel.GetComponent<Renderer>();
                        renderer.material = material;
                        Destroy(pixel.GetComponent<Collider>());
                    }
                }
            }

            // Add backing plate
            CreateBackingPlate(parent, width, height);
        }

        private void CreateGeometricSign(Transform parent, float width, float height, Color color)
        {
            Material material = CreateNeonMaterial(color);

            int shapeType = Random.Range(0, 5);
            switch (shapeType)
            {
                case 0: // Circle
                    CreateCircleSign(parent, width, height, material);
                    break;
                case 1: // Triangle
                    CreateTriangleSign(parent, width, height, material);
                    break;
                case 2: // Cross
                    CreateCrossSign(parent, width, height, material);
                    break;
                case 3: // Diamond
                    CreateDiamondSign(parent, width, height, material);
                    break;
                case 4: // Hexagon
                    CreateHexagonSign(parent, width, height, material);
                    break;
            }

            CreateBackingPlate(parent, width, height);
        }

        private void CreateAbstractSign(Transform parent, float width, float height, Color color)
        {
            Material material = CreateNeonMaterial(color);

            // Create random lines and curves
            int lineCount = Random.Range(3, 8);

            for (int i = 0; i < lineCount; i++)
            {
                Vector3 start = new Vector3(
                    Random.Range(-width * 0.4f, width * 0.4f),
                    Random.Range(0f, height),
                    0f
                );
                Vector3 end = new Vector3(
                    Random.Range(-width * 0.4f, width * 0.4f),
                    Random.Range(0f, height),
                    0f
                );

                CreateNeonLine(parent, start, end, thickness, material);
            }

            CreateBackingPlate(parent, width, height);
        }

        private void CreateCircleSign(Transform parent, float width, float height, Material material)
        {
            float radius = Mathf.Min(width, height) * 0.4f;
            int segments = Mathf.RoundToInt(radius * 20f);

            Vector3 lastPoint = Vector3.zero;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                Vector3 point = new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius + height * 0.5f,
                    0f
                );

                if (i > 0)
                {
                    CreateNeonLine(parent, lastPoint, point, thickness, material);
                }
                lastPoint = point;
            }
        }

        private void CreateTriangleSign(Transform parent, float width, float height, Material material)
        {
            Vector3[] points = new[]
            {
                new Vector3(0f, height * 0.9f, 0f),
                new Vector3(-width * 0.4f, height * 0.1f, 0f),
                new Vector3(width * 0.4f, height * 0.1f, 0f),
                new Vector3(0f, height * 0.9f, 0f)
            };

            for (int i = 0; i < points.Length - 1; i++)
            {
                CreateNeonLine(parent, points[i], points[i + 1], thickness, material);
            }
        }

        private void CreateCrossSign(Transform parent, float width, float height, Material material)
        {
            float halfWidth = width * 0.4f;
            float halfHeight = height * 0.4f;
            float centerY = height * 0.5f;

            // Horizontal
            CreateNeonLine(parent,
                new Vector3(-halfWidth, centerY, 0f),
                new Vector3(halfWidth, centerY, 0f),
                thickness, material);

            // Vertical
            CreateNeonLine(parent,
                new Vector3(0f, centerY - halfHeight, 0f),
                new Vector3(0f, centerY + halfHeight, 0f),
                thickness, material);
        }

        private void CreateDiamondSign(Transform parent, float width, float height, Material material)
        {
            Vector3[] points = new[]
            {
                new Vector3(0f, height * 0.9f, 0f),
                new Vector3(width * 0.4f, height * 0.5f, 0f),
                new Vector3(0f, height * 0.1f, 0f),
                new Vector3(-width * 0.4f, height * 0.5f, 0f),
                new Vector3(0f, height * 0.9f, 0f)
            };

            for (int i = 0; i < points.Length - 1; i++)
            {
                CreateNeonLine(parent, points[i], points[i + 1], thickness, material);
            }
        }

        private void CreateHexagonSign(Transform parent, float width, float height, Material material)
        {
            float radius = Mathf.Min(width, height) * 0.35f;
            int segments = 6;

            Vector3 lastPoint = Vector3.zero;
            for (int i = 0; i <= segments; i++)
            {
                float angle = (i / (float)segments) * Mathf.PI * 2f;
                Vector3 point = new Vector3(
                    Mathf.Cos(angle) * radius,
                    Mathf.Sin(angle) * radius + height * 0.5f,
                    0f
                );

                if (i > 0)
                {
                    CreateNeonLine(parent, lastPoint, point, thickness, material);
                }
                lastPoint = point;
            }
        }

        private void CreateNeonLine(Transform parent, Vector3 start, Vector3 end, float lineThickness, Material material)
        {
            Vector3 direction = end - start;
            float length = direction.magnitude;

            GameObject line = GameObject.CreatePrimitive(PrimitiveType.Cube);
            line.name = "NeonLine";
            line.transform.SetParent(parent);
            line.transform.localPosition = (start + end) * 0.5f;
            line.transform.localRotation = Quaternion.LookRotation(Vector3.forward, direction);
            line.transform.localScale = new Vector3(lineThickness, length, lineThickness);

            var renderer = line.GetComponent<Renderer>();
            renderer.material = material;
            Destroy(line.GetComponent<Collider>());
        }

        private void CreateBackingPlate(Transform parent, float width, float height)
        {
            GameObject plate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            plate.name = "BackingPlate";
            plate.transform.SetParent(parent);
            plate.transform.localPosition = new Vector3(0f, height * 0.5f, -thickness);
            plate.transform.localScale = new Vector3(width + 0.5f, height + 0.5f, thickness * 0.5f);

            var renderer = plate.GetComponent<Renderer>();
            renderer.material = CreateBackingMaterial();
            Destroy(plate.GetComponent<Collider>());
        }

        private bool[,] GenerateTextLikePattern(int cols, int rows)
        {
            bool[,] pattern = new bool[cols, rows];

            // Generate random vertical strokes (like stylized kanji)
            int strokeCount = Random.Range(2, 5);
            for (int s = 0; s < strokeCount; s++)
            {
                int strokeX = Random.Range(1, cols - 1);
                int strokeStart = Random.Range(0, rows / 2);
                int strokeEnd = Random.Range(rows / 2, rows);

                for (int y = strokeStart; y < strokeEnd; y++)
                {
                    pattern[strokeX, y] = true;
                }
            }

            // Add horizontal connections
            int connectionCount = Random.Range(1, 4);
            for (int c = 0; c < connectionCount; c++)
            {
                int y = Random.Range(1, rows - 1);
                int startX = Random.Range(0, cols / 2);
                int endX = Random.Range(cols / 2, cols);

                for (int x = startX; x < endX; x++)
                {
                    pattern[x, y] = true;
                }
            }

            return pattern;
        }

        private Material CreateNeonMaterial(Color color)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * emissionIntensity);
            return mat;
        }

        private Material CreateBackingMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.02f, 0.02f, 0.03f);
            return mat;
        }
    }
}
