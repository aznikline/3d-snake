using UnityEngine;
using NeonSerpent.Core;

namespace NeonSerpent.Procedural.Art.Effects
{
    /// <summary>
    /// Generates procedural particle effects for atmosphere:
    /// sparks, data stream particles, and dust.
    /// </summary>
    public class ProceduralParticleEffects : MonoBehaviour
    {
        [Header("Energy Sparks")]
        [SerializeField] private int sparkCount = 50;
        [SerializeField] private float sparkLifetime = 1f;
        [SerializeField] private float sparkSpeed = 5f;
        [SerializeField] private Color sparkColor = new Color(0.8f, 0.75f, 0.5f);

        [Header("Data Stream")]
        [SerializeField] private int streamCount = 100;
        [SerializeField] private float streamSpeed = 2f;
        [SerializeField] private float streamHeight = 20f;
        [SerializeField] private Color streamColor = new Color(0.5f, 0.7f, 0.5f);

        [Header("Dust")]
        [SerializeField] private int dustCount = 200;
        [SerializeField] private float dustSize = 0.02f;
        [SerializeField] private float dustDrift = 0.5f;
        [SerializeField] private Color dustColor = new Color(0.6f, 0.55f, 0.5f);

        [Header("Area")]
        [SerializeField] private Vector3 effectArea = new Vector3(100f, 30f, 100f);

        /// <summary>
        /// Create all atmospheric particle effects.
        /// </summary>
        public void CreateAtmosphericEffects(Transform parent)
        {
            CreateEnergySparks(parent);
            CreateDataStreams(parent);
            CreateDust(parent);
        }

        private void CreateEnergySparks(Transform parent)
        {
            GameObject sparks = new GameObject("EnergySparks");
            sparks.transform.SetParent(parent);

            var particleSystem = sparks.AddComponent<ParticleSystem>();
            var main = particleSystem.main;
            main.startLifetime = sparkLifetime;
            main.startSpeed = sparkSpeed;
            main.startSize = 0.05f;
            main.startColor = new Color(sparkColor.r, sparkColor.g, sparkColor.b, 0.8f);
            main.maxParticles = sparkCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = 0.5f;

            var emission = particleSystem.emission;
            emission.rateOverTime = sparkCount / sparkLifetime;

            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = effectArea;

            var velocityOverLifetime = particleSystem.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-sparkSpeed, sparkSpeed);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0f, sparkSpeed);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-sparkSpeed, sparkSpeed);

            var colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(sparkColor, 0f), new GradientColorKey(sparkColor, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var renderer = sparks.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateParticleMaterial(sparkColor);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private void CreateDataStreams(Transform parent)
        {
            GameObject streams = new GameObject("DataStreams");
            streams.transform.SetParent(parent);

            var particleSystem = streams.AddComponent<ParticleSystem>();
            var main = particleSystem.main;
            main.startLifetime = streamHeight / streamSpeed;
            main.startSpeed = streamSpeed;
            main.startSize = 0.03f;
            main.startColor = new Color(streamColor.r, streamColor.g, streamColor.b, 0.6f);
            main.maxParticles = streamCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = particleSystem.emission;
            emission.rateOverTime = streamCount / main.startLifetime.constant;

            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(effectArea.x, 0.1f, effectArea.z);

            var velocityOverLifetime = particleSystem.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            velocityOverLifetime.y = streamSpeed;

            var trails = particleSystem.trails;
            trails.enabled = true;
            trails.lifetime = 0.5f;
            // minimumVertexDistance removed in Unity 2022.3, using lifetimeMultiplier instead
            trails.lifetimeMultiplier = 1.0f;

            var renderer = streams.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateParticleMaterial(streamColor);
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.lengthScale = 3f;
        }

        private void CreateDust(Transform parent)
        {
            GameObject dust = new GameObject("Dust");
            dust.transform.SetParent(parent);

            var particleSystem = dust.AddComponent<ParticleSystem>();
            var main = particleSystem.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(5f, 10f);
            main.startSpeed = dustDrift;
            main.startSize = dustSize;
            main.startColor = new Color(dustColor.r, dustColor.g, dustColor.b, 0.3f);
            main.maxParticles = dustCount;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = particleSystem.emission;
            emission.rateOverTime = dustCount / 7f;

            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = effectArea;

            var velocityOverLifetime = particleSystem.velocityOverLifetime;
            velocityOverLifetime.enabled = true;
            velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
            velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-dustDrift, dustDrift);
            velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-dustDrift * 0.5f, dustDrift * 0.5f);
            velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-dustDrift, dustDrift);

            var colorOverLifetime = particleSystem.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(dustColor, 0f), new GradientColorKey(dustColor, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.3f, 0.5f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = gradient;

            var renderer = dust.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateParticleMaterial(dustColor);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        private Material CreateParticleMaterial(Color color)
        {
            return PolyMaterials.CreateParticleUnlit(color);
        }
    }
}
