using UnityEngine;
using UnityEngine.UI;

namespace NeonSerpent.Procedural.Art.UI
{
    /// <summary>
    /// Generates procedural UI elements with cyberpunk styling.
    /// Creates dynamic borders, scanlines, and holographic effects.
    /// </summary>
    public class ProceduralUIRenderer : MonoBehaviour
    {
        [Header("Border")]
        [SerializeField] private float borderWidth = 2f;
        [SerializeField] private Color borderColor = new Color(0f, 1f, 1f);
        [SerializeField] private float borderGlow = 2f;

        [Header("Scanlines")]
        [SerializeField] private bool enableScanlines = true;
        [SerializeField] private float scanlineSpacing = 4f;
        [SerializeField] private float scanlineAlpha = 0.1f;

        [Header("Glitch")]
        [SerializeField] private bool enableGlitch = true;
        [SerializeField] private float glitchChance = 0.01f;
        [SerializeField] private float glitchDuration = 0.1f;

        [Header("Hologram")]
        [SerializeField] private bool enableHologram = true;
        [SerializeField] private float hologramFlicker = 0.05f;
        [SerializeField] private Color hologramColor = new Color(0f, 1f, 1f, 0.8f);

        private Material _uiMaterial;
        private float _glitchTimer;
        private bool _isGlitching;
        private float _hologramPhase;

        private void Start()
        {
            CreateUIMaterial();
        }

        private void Update()
        {
            UpdateGlitch();
            UpdateHologram();
            UpdateMaterialProperties();
        }

        private void CreateUIMaterial()
        {
            _uiMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            _uiMaterial.EnableKeyword("_EMISSION");
        }

        private void UpdateGlitch()
        {
            if (!enableGlitch) return;

            if (_isGlitching)
            {
                _glitchTimer -= Time.deltaTime;
                if (_glitchTimer <= 0)
                {
                    _isGlitching = false;
                }
            }
            else if (Random.value < glitchChance)
            {
                _isGlitching = true;
                _glitchTimer = glitchDuration;
            }
        }

        private void UpdateHologram()
        {
            if (!enableHologram) return;

            _hologramPhase += Time.deltaTime * 10f;
        }

        private void UpdateMaterialProperties()
        {
            if (_uiMaterial == null) return;

            // Border glow
            Color glowColor = borderColor * (borderGlow + (_isGlitching ? 3f : 0f));
            _uiMaterial.SetColor("_EmissionColor", glowColor);

            // Hologram flicker
            if (enableHologram)
            {
                float flicker = 1f + Mathf.Sin(_hologramPhase) * hologramFlicker;
                _uiMaterial.SetColor("_BaseColor", hologramColor * flicker);
            }
        }

        /// <summary>
        /// Apply procedural styling to an Image component.
        /// </summary>
        public void StyleImage(Image image)
        {
            if (image == null) return;

            image.material = _uiMaterial;

            // Add outline effect via shadow component
            var outline = image.gameObject.GetComponent<Outline>();
            if (outline == null)
            {
                outline = image.gameObject.AddComponent<Outline>();
            }
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(borderWidth, borderWidth);
        }

        /// <summary>
        /// Create a procedural scanline overlay.
        /// </summary>
        public void CreateScanlineOverlay(RectTransform parent)
        {
            if (!enableScanlines) return;

            GameObject overlay = new GameObject("ScanlineOverlay");
            overlay.transform.SetParent(parent);

            var rect = overlay.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = overlay.AddComponent<Image>();
            image.material = CreateScanlineMaterial();
            image.raycastTarget = false;
        }

        private Material CreateScanlineMaterial()
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
            mat.SetColor("_BaseColor", new Color(0f, 0f, 0f, scanlineAlpha));
            return mat;
        }

        /// <summary>
        /// Trigger a glitch effect manually.
        /// </summary>
        public void TriggerGlitch()
        {
            _isGlitching = true;
            _glitchTimer = glitchDuration;
        }

        /// <summary>
        /// Set border color dynamically.
        /// </summary>
        public void SetBorderColor(Color color)
        {
            borderColor = color;
        }
    }
}
