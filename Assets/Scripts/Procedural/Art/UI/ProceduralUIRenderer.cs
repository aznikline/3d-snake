using UnityEngine;
using UnityEngine.UI;
using NeonSerpent.Core;

namespace NeonSerpent.Procedural.Art.UI
{
    /// <summary>
    /// Generates procedural UI elements with flat poly styling.
    /// Creates simple borders and panel backgrounds.
    /// </summary>
    public class ProceduralUIRenderer : MonoBehaviour
    {
        [Header("Border")]
        [SerializeField] private float borderWidth = 2f;
        [SerializeField] private Color borderColor = new Color(0.2f, 0.7f, 0.3f);

        [Header("Panel")]
        [SerializeField] private Color panelColor = new Color(0.9f, 0.92f, 0.9f, 0.95f);

        private Material _uiMaterial;

        private void Start()
        {
            CreateUIMaterial();
        }

        private void CreateUIMaterial()
        {
            _uiMaterial = PolyMaterials.CreateUnlit(panelColor);
        }

        /// <summary>
        /// Apply poly styling to an Image component.
        /// </summary>
        public void StyleImage(Image image)
        {
            if (image == null) return;

            image.material = _uiMaterial;

            var outline = image.gameObject.GetComponent<Outline>();
            if (outline == null)
            {
                outline = image.gameObject.AddComponent<Outline>();
            }
            outline.effectColor = borderColor;
            outline.effectDistance = new Vector2(borderWidth, borderWidth);
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
