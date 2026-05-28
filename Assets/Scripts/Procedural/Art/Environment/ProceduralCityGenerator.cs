using System.Collections.Generic;
using UnityEngine;

namespace NeonSerpent.Procedural.Art.Environment
{
    /// <summary>
    /// Generates an entire cyberpunk cityscape at runtime.
    /// Orchestrates building placement, street layout, and atmospheric effects.
    /// </summary>
    public class ProceduralCityGenerator : MonoBehaviour
    {
        [Header("City Grid")]
        [SerializeField] private int cityBlocksX = 10;
        [SerializeField] private int cityBlocksZ = 10;
        [SerializeField] private float blockSize = 50f;
        [SerializeField] private float streetWidth = 15f;

        [Header("Building Density")]
        [SerializeField] private int minBuildingsPerBlock = 3;
        [SerializeField] private int maxBuildingsPerBlock = 6;
        [SerializeField] private float buildingSpacing = 5f;

        [Header("Height Variation")]
        [SerializeField] private AnimationCurve heightDistribution = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 1f);
        [SerializeField] private float centerHeightMultiplier = 1.5f;

        [Header("Atmosphere")]
        [SerializeField] private bool generateFog = true;
        [SerializeField] private Color fogColor = new Color(0.02f, 0.02f, 0.05f);
        [SerializeField] private float fogDensity = 0.02f;
        [SerializeField] private bool generateRain = true;
        [SerializeField] private float rainChance = 0.3f;

        [Header("Ground")]
        [SerializeField] private Material groundMaterial;
        [SerializeField] private float groundHeight = 0f;

        [Header("References")]
        [SerializeField] private ProceduralBuildingGenerator buildingGenerator;
        [SerializeField] private ProceduralNeonSignGenerator signGenerator;

        private List<GameObject> _cityObjects = new List<GameObject>();
        private bool _hasRain;

        /// <summary>
        /// Generate the entire city.
        /// </summary>
        public void GenerateCity(int? seed = null)
        {
            ClearCity();

            int citySeed = seed ?? Random.Range(0, int.MaxValue);
            Random.InitState(citySeed);

            // Generate ground plane
            CreateGround();

            // Generate city blocks
            Vector3 cityCenter = new Vector3(
                (cityBlocksX - 1) * blockSize * 0.5f,
                0f,
                (cityBlocksZ - 1) * blockSize * 0.5f
            );

            for (int x = 0; x < cityBlocksX; x++)
            {
                for (int z = 0; z < cityBlocksZ; z++)
                {
                    Vector3 blockCenter = new Vector3(
                        x * blockSize,
                        0f,
                        z * blockSize
                    );

                    GenerateCityBlock(blockCenter, cityCenter);
                }
            }

            // Generate atmosphere
            if (generateFog)
            {
                SetupFog();
            }

            if (generateRain && Random.value < rainChance)
            {
                CreateRainEffect();
            }

            // Generate neon signs on streets
            GenerateStreetSigns();

            Debug.Log($"[ProceduralCityGenerator] Generated city with {_cityObjects.Count} objects.");
        }

        /// <summary>
        /// Clear all generated city objects.
        /// </summary>
        public void ClearCity()
        {
            foreach (var obj in _cityObjects)
            {
                if (obj != null)
                    Destroy(obj);
            }
            _cityObjects.Clear();

            // Reset atmosphere
            RenderSettings.fog = false;
        }

        private void CreateGround()
        {
            float totalWidth = cityBlocksX * blockSize + streetWidth;
            float totalDepth = cityBlocksZ * blockSize + streetWidth;

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "CityGround";
            ground.transform.position = new Vector3(
                (cityBlocksX - 1) * blockSize * 0.5f,
                groundHeight,
                (cityBlocksZ - 1) * blockSize * 0.5f
            );
            ground.transform.localScale = new Vector3(totalWidth * 0.1f, 1f, totalDepth * 0.1f);

            var renderer = ground.GetComponent<Renderer>();
            if (groundMaterial != null)
            {
                renderer.material = groundMaterial;
            }
            else
            {
                renderer.material = CreateGroundMaterial();
            }

            Destroy(ground.GetComponent<Collider>());
            _cityObjects.Add(ground);
        }

        private void GenerateCityBlock(Vector3 blockCenter, Vector3 cityCenter)
        {
            float distanceFromCenter = Vector3.Distance(
                new Vector3(blockCenter.x, 0f, blockCenter.z),
                new Vector3(cityCenter.x, 0f, cityCenter.z)
            );
            float maxDistance = Vector3.Distance(Vector3.zero, cityCenter);
            float centerFactor = 1f - (distanceFromCenter / maxDistance);

            int buildingCount = Random.Range(minBuildingsPerBlock, maxBuildingsPerBlock + 1);

            for (int i = 0; i < buildingCount; i++)
            {
                // Position within block
                float xOffset = Random.Range(-blockSize * 0.35f, blockSize * 0.35f);
                float zOffset = Random.Range(-blockSize * 0.35f, blockSize * 0.35f);
                Vector3 buildingPos = blockCenter + new Vector3(xOffset, groundHeight, zOffset);

                // Height multiplier based on distance from center
                float heightMult = Mathf.Lerp(1f, centerHeightMultiplier, centerFactor);

                if (buildingGenerator != null)
                {
                    var building = buildingGenerator.GenerateBuilding(buildingPos);
                    ApplyHeightMultiplier(building, heightMult);
                    _cityObjects.Add(building);
                }
            }
        }

        private void ApplyHeightMultiplier(GameObject building, float multiplier)
        {
            // Scale the building height
            foreach (Transform child in building.transform)
            {
                if (child.name == "Body")
                {
                    Vector3 scale = child.localScale;
                    scale.y *= multiplier;
                    child.localScale = scale;

                    // Adjust position
                    Vector3 pos = child.localPosition;
                    pos.y = scale.y * 0.5f;
                    child.localPosition = pos;
                }
                else if (child.name.Contains("Trim") || child.name == "Antenna")
                {
                    // Adjust vertical elements
                    Vector3 pos = child.localPosition;
                    pos.y *= multiplier;
                    child.localPosition = pos;
                }
            }
        }

        private void SetupFog()
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
        }

        private void CreateRainEffect()
        {
            _hasRain = true;

            // Create particle system for rain
            GameObject rainGO = new GameObject("RainEffect");
            rainGO.transform.position = new Vector3(
                (cityBlocksX - 1) * blockSize * 0.5f,
                50f,
                (cityBlocksZ - 1) * blockSize * 0.5f
            );

            var particleSystem = rainGO.AddComponent<ParticleSystem>();
            var main = particleSystem.main;
            main.startLifetime = 2f;
            main.startSpeed = 20f;
            main.startSize = 0.05f;
            main.maxParticles = 10000;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = particleSystem.emission;
            emission.rateOverTime = 5000;

            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(
                cityBlocksX * blockSize,
                1f,
                cityBlocksZ * blockSize
            );

            var velocityOverLifetime = particleSystem.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            velocityOverLifetime.z = -5f;

            // Rain material
            var renderer = rainGO.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            renderer.material.color = new Color(0.7f, 0.8f, 1f, 0.3f);

            _cityObjects.Add(rainGO);
        }

        private void GenerateStreetSigns()
        {
            if (signGenerator == null) return;

            // Place signs along streets between blocks
            for (int x = 0; x < cityBlocksX; x++)
            {
                for (int z = 0; z < cityBlocksZ; z++)
                {
                    if (Random.value > 0.3f) continue;

                    Vector3 signPos = new Vector3(
                        x * blockSize + Random.Range(-blockSize * 0.4f, blockSize * 0.4f),
                        groundHeight,
                        z * blockSize + Random.Range(-blockSize * 0.4f, blockSize * 0.4f)
                    );

                    var sign = signGenerator.GenerateSign(signPos);
                    if (sign != null)
                        _cityObjects.Add(sign);
                }
            }
        }

        private Material CreateGroundMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.03f, 0.03f, 0.04f);
            mat.SetFloat("_Smoothness", 0.9f);
            mat.SetFloat("_Metallic", 0.1f);
            return mat;
        }
    }
}
