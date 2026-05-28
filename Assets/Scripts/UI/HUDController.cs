using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonSerpent.Core;
using NeonSerpent.Gameplay;
using NeonSerpent.Player;

namespace NeonSerpent.UI
{
    /// <summary>
    /// In-game HUD. Displays speed, combo meter, snake length, minimap,
    /// and dash readiness indicator.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Speed")]
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private Image speedBar;

        [Header("Combo")]
        [SerializeField] private Image comboFill;
        [SerializeField] private TextMeshProUGUI comboCountText;
        [SerializeField] private CanvasGroup comboReadyIndicator;
        [SerializeField] private Color comboNormalColor = Color.white;
        [SerializeField] private Color comboReadyColor = Color.cyan;

        [Header("Snake Length")]
        [SerializeField] private TextMeshProUGUI lengthText;

        [Header("Minimap")]
        [SerializeField] private RectTransform minimapContainer;
        [SerializeField] private RectTransform minimapPlayerIcon;
        [SerializeField] private RectTransform minimapFoodIconPrefab;
        [SerializeField] private float minimapScale = 0.1f;

        [Header("Dash")]
        [SerializeField] private Image dashIcon;
        [SerializeField] private Color dashReadyColor = Color.cyan;
        [SerializeField] private Color dashCooldownColor = Color.gray;

        [Header("References")]
        [SerializeField] private SnakeHeadController snakeController;
        [SerializeField] private VerletSnakeBody snakeBody;
        [SerializeField] private ComboSystem comboSystem;

        private void OnEnable()
        {
            if (comboSystem != null)
            {
                comboSystem.OnComboChanged += OnComboChanged;
                comboSystem.OnComboReady += OnComboReady;
                comboSystem.OnDashStarted += OnDashStarted;
                comboSystem.OnDashEnded += OnDashEnded;
            }
        }

        private void OnDisable()
        {
            if (comboSystem != null)
            {
                comboSystem.OnComboChanged -= OnComboChanged;
                comboSystem.OnComboReady -= OnComboReady;
                comboSystem.OnDashStarted -= OnDashStarted;
                comboSystem.OnDashEnded -= OnDashEnded;
            }
        }

        private void Update()
        {
            if (!GameStateManager.Instance.IsPlaying) return;

            UpdateSpeed();
            UpdateLength();
            UpdateDashIcon();
        }

        private void UpdateSpeed()
        {
            if (snakeController == null || speedText == null) return;

            float speed = snakeController.CurrentSpeed;
            speedText.text = $"{speed:F1} m/s";

            if (speedBar != null)
            {
                float normalizedSpeed = Mathf.Clamp01(speed / (GameConstants.BaseSpeed * GameConstants.DashSpeedMultiplier));
                speedBar.fillAmount = normalizedSpeed;
                speedBar.color = Color.Lerp(Color.green, Color.red, normalizedSpeed);
            }
        }

        private void UpdateLength()
        {
            if (snakeBody == null || lengthText == null) return;
            lengthText.text = $"Length: {snakeBody.NodeCount}";
        }

        private void OnComboChanged(float normalizedMeter)
        {
            if (comboFill != null)
            {
                comboFill.fillAmount = normalizedMeter;
                comboFill.color = normalizedMeter >= 1f ? comboReadyColor : comboNormalColor;
            }
        }

        private void OnComboReady()
        {
            if (comboReadyIndicator != null)
            {
                comboReadyIndicator.alpha = 1f;
                // Pulse animation could be triggered here
            }
        }

        private void OnDashStarted()
        {
            if (comboReadyIndicator != null)
                comboReadyIndicator.alpha = 0f;
        }

        private void OnDashEnded()
        {
            // Reset handled by combo system
        }

        private void UpdateDashIcon()
        {
            if (dashIcon == null || comboSystem == null) return;

            bool ready = comboSystem.IsDashReady;
            dashIcon.color = ready ? dashReadyColor : dashCooldownColor;
        }

        /// <summary>
        /// Update minimap with player position and food locations.
        /// Call from a gameplay manager that tracks food positions.
        /// </summary>
        public void UpdateMinimap(Vector3 playerPos, Vector3[] foodPositions)
        {
            if (minimapPlayerIcon != null)
            {
                Vector2 minimapPos = new Vector2(playerPos.x, playerPos.z) * minimapScale;
                minimapPlayerIcon.anchoredPosition = minimapPos;

                // Rotate player icon to match heading
                float yaw = snakeController.transform.eulerAngles.y;
                minimapPlayerIcon.rotation = Quaternion.Euler(0f, 0f, -yaw);
            }

            // Food icons would be pooled and updated here
            // Implementation depends on food tracking system
        }

        /// <summary>
        /// Show/hide the entire HUD (e.g., during death replay).
        /// </summary>
        public void SetVisible(bool visible)
        {
            CanvasGroup cg = GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = visible ? 1f : 0f;
                cg.interactable = visible;
                cg.blocksRaycasts = visible;
            }
        }
    }
}
