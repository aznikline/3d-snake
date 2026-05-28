using UnityEngine;

namespace NeonSerpent.Procedural.Art.Effects
{
    /// <summary>
    /// Adds realistic neon flicker to emissive materials.
    /// Simulates electrical instability with random brightness variations.
    /// </summary>
    public class NeonFlickerEffect : MonoBehaviour
    {
        [Header("Flicker")]
        [SerializeField] private float baseIntensity = 4f;
        [SerializeField] private float flickerAmount = 0.3f;
        [SerializeField] private float flickerSpeed = 10f;
        [SerializeField] private float malfunctionChance = 0.05f;
        [SerializeField] private float malfunctionDuration = 0.5f;

        private Color _baseColor;
        private Material _material;
        private float _time;
        private bool _isMalfunctioning;
        private float _malfunctionTimer;
        private Renderer _renderer;

        public void Initialize(Color color, float intensity)
        {
            _baseColor = color;
            baseIntensity = intensity;
        }

        private void Start()
        {
            _renderer = GetComponentInChildren<Renderer>();
            if (_renderer == null)
            {
                enabled = false;
                return;
            }

            _material = _renderer.material;
            _material.EnableKeyword("_EMISSION");
        }

        private void Update()
        {
            if (_material == null) return;

            _time += Time.deltaTime * flickerSpeed;

            // Check for malfunction
            if (!_isMalfunctioning && Random.value < malfunctionChance * Time.deltaTime)
            {
                _isMalfunctioning = true;
                _malfunctionTimer = malfunctionDuration;
            }

            float intensity;

            if (_isMalfunctioning)
            {
                _malfunctionTimer -= Time.deltaTime;
                if (_malfunctionTimer <= 0)
                {
                    _isMalfunctioning = false;
                }

                // Erratic flicker during malfunction
                intensity = baseIntensity * Random.Range(0f, 1.5f);
            }
            else
            {
                // Normal subtle flicker
                float noise = Mathf.PerlinNoise(_time, 0f) * 2f - 1f;
                intensity = baseIntensity + noise * flickerAmount * baseIntensity;
            }

            _material.SetColor("_EmissionColor", _baseColor * intensity);
        }

        private void OnDestroy()
        {
            if (_material != null)
            {
                Destroy(_material);
            }
        }
    }
}
