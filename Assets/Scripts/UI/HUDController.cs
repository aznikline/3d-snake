using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonSerpent.Core;
using NeonSerpent.Gameplay;
using NeonSerpent.Player;

namespace NeonSerpent.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Speed")]
        public TextMeshProUGUI speedText;
        public Image speedBar;
        [SerializeField] private float speedBarSmoothing = 8f;

        [Header("Combo")]
        public Image comboFill;
        public TextMeshProUGUI comboCountText;
        [SerializeField] private CanvasGroup comboReadyIndicator;
        [SerializeField] private Color comboNormalColor = Color.white;
        [SerializeField] private Color comboReadyColor = Color.cyan;
        [SerializeField] private float comboPulseScale = 1.15f;
        [SerializeField] private float comboPulseSpeed = 8f;

        [Header("Score")]
        public TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI rankText;

        [Header("Snake Length")]
        public TextMeshProUGUI lengthText;

        [Header("Dash")]
        public Image dashIcon;
        [SerializeField] private Color dashReadyColor = Color.cyan;
        [SerializeField] private Color dashCooldownColor = Color.gray;
        [SerializeField] private Color dashActiveColor = Color.white;

        [Header("Collection Flash")]
        public Image collectionFlash;
        [SerializeField] private float flashDuration = 0.15f;
        [SerializeField] private Color flashColor = new Color(0f, 1f, 1f, 0.3f);

        [Header("Damage Vignette")]
        public Image damageVignette;
        [SerializeField] private Color damageColor = new Color(1f, 0f, 0f, 0.4f);
        [SerializeField] private float damageFadeSpeed = 3f;

        [Header("Minimap")]
        [SerializeField] private RectTransform minimapContainer;
        [SerializeField] private RectTransform minimapPlayerIcon;
        [SerializeField] private float minimapScale = 0.1f;

        [Header("References")]
        public SnakeHeadController snakeController;
        public VerletSnakeBody snakeBody;
        public ComboSystem comboSystem;

        private float _displayedSpeed;
        private float _comboPulsePhase;
        private bool _comboReady;
        private bool _isDashing;
        private float _previousComboMeter;
        private Coroutine _flashRoutine;
        private float _currentDamageAlpha;

        private void OnEnable()
        {
            if (comboSystem != null)
            {
                comboSystem.OnComboChanged += OnComboChanged;
                comboSystem.OnComboReady += OnComboReady;
                comboSystem.OnDashStarted += OnDashStarted;
                comboSystem.OnDashEnded += OnDashEnded;
                comboSystem.OnDeathPrevented += OnDeathPrevented;
            }
            if (collectionFlash != null)
                collectionFlash.gameObject.SetActive(false);
            if (damageVignette != null)
                damageVignette.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (comboSystem != null)
            {
                comboSystem.OnComboChanged -= OnComboChanged;
                comboSystem.OnComboReady -= OnComboReady;
                comboSystem.OnDashStarted -= OnDashStarted;
                comboSystem.OnDashEnded -= OnDashEnded;
                comboSystem.OnDeathPrevented -= OnDeathPrevented;
            }
        }

        private void Update()
        {
            if (!GameStateManager.Instance.IsPlaying) return;

            UpdateSpeed();
            UpdateLength();
            UpdateDashIcon();
            UpdateComboPulse();
            UpdateDamageVignette();
            UpdateMinimap();
        }

        private void UpdateSpeed()
        {
            if (snakeController == null) return;

            float targetSpeed = snakeController.CurrentSpeed;
            _displayedSpeed = Mathf.Lerp(_displayedSpeed, targetSpeed, Time.deltaTime * speedBarSmoothing);

            if (speedText != null)
                speedText.text = $"{_displayedSpeed:F1} m/s";

            if (speedBar != null)
            {
                float maxSpeed = GameConstants.BaseSpeed * GameConstants.DashSpeedMultiplier;
                float targetFill = Mathf.Clamp01(targetSpeed / maxSpeed);
                speedBar.fillAmount = Mathf.Lerp(speedBar.fillAmount, targetFill, Time.deltaTime * speedBarSmoothing);
                speedBar.color = Color.Lerp(Color.green, Color.red, speedBar.fillAmount);
            }
        }

        private void UpdateLength()
        {
            if (snakeBody == null || lengthText == null) return;
            lengthText.text = $"Length: {snakeBody.NodeCount}";
        }

        private void UpdateComboPulse()
        {
            if (!_comboReady || comboFill == null) return;

            _comboPulsePhase += Time.deltaTime * comboPulseSpeed;
            float scale = 1f + (Mathf.Sin(_comboPulsePhase) * 0.5f + 0.5f) * (comboPulseScale - 1f);
            comboFill.rectTransform.localScale = Vector3.one * scale;
        }

        private void UpdateDamageVignette()
        {
            if (damageVignette == null) return;
            _currentDamageAlpha = Mathf.Lerp(_currentDamageAlpha, 0f, Time.deltaTime * damageFadeSpeed);
            var c = damageColor;
            c.a *= _currentDamageAlpha;
            damageVignette.color = c;
            damageVignette.gameObject.SetActive(_currentDamageAlpha > 0.01f);
        }

        private void UpdateDashIcon()
        {
            if (dashIcon == null || comboSystem == null) return;

            if (comboSystem.IsDashing)
            {
                dashIcon.color = dashActiveColor;
                float pulse = 1f + Mathf.Sin(Time.time * 20f) * 0.3f;
                dashIcon.rectTransform.localScale = Vector3.one * pulse;
            }
            else if (comboSystem.IsDashReady)
            {
                dashIcon.color = dashReadyColor;
                dashIcon.rectTransform.localScale = Vector3.one;
            }
            else
            {
                dashIcon.color = dashCooldownColor;
                dashIcon.rectTransform.localScale = Vector3.one;
            }
        }

        private void UpdateMinimap()
        {
            if (minimapContainer == null || minimapPlayerIcon == null) return;
            if (snakeController == null) return;

            Vector3 playerPos = snakeController.transform.position;
            Vector2 minimapPos = new Vector2(playerPos.x, playerPos.z) * minimapScale;
            minimapPlayerIcon.anchoredPosition = minimapPos;

            float yaw = snakeController.transform.eulerAngles.y;
            minimapPlayerIcon.rotation = Quaternion.Euler(0f, 0f, -yaw);
        }

        private void OnComboChanged(float normalizedMeter)
        {
            bool meterIncreased = normalizedMeter > _previousComboMeter;
            _previousComboMeter = normalizedMeter;

            if (comboFill != null)
            {
                comboFill.fillAmount = normalizedMeter;
                comboFill.color = normalizedMeter >= 1f ? comboReadyColor : comboNormalColor;
            }

            if (comboCountText != null && comboSystem != null)
                comboCountText.text = comboSystem.CurrentComboCount > 0 ? $"x{comboSystem.CurrentComboCount}" : "";

            if (normalizedMeter >= 1f && !_comboReady)
            {
                _comboReady = true;
                TriggerCollectionFlash(new Color(0f, 1f, 0.5f, 0.4f), 0.25f);
            }
            else if (normalizedMeter < 1f && _comboReady)
            {
                _comboReady = false;
                if (comboFill != null)
                    comboFill.rectTransform.localScale = Vector3.one;
            }
            else if (meterIncreased && normalizedMeter > 0f)
            {
                TriggerCollectionFlash(flashColor, flashDuration);
            }
        }

        private void OnComboReady()
        {
            _comboReady = true;
            if (comboReadyIndicator != null)
                comboReadyIndicator.alpha = 1f;
        }

        private void OnDashStarted()
        {
            _isDashing = true;
            if (comboReadyIndicator != null)
                comboReadyIndicator.alpha = 0f;
        }

        private void OnDashEnded()
        {
            _isDashing = false;
            _comboReady = false;
            if (comboFill != null)
                comboFill.rectTransform.localScale = Vector3.one;
        }

        private void OnDeathPrevented()
        {
            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine(new Color(1f, 0.3f, 0f, 0.5f), 0.3f));
        }

        public void TriggerCollectionFlash(Color? color = null, float? duration = null)
        {
            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine(
                color ?? flashColor, duration ?? flashDuration));
        }

        public void TriggerDamageFlash()
        {
            _currentDamageAlpha = 1f;
            if (damageVignette != null)
                damageVignette.gameObject.SetActive(true);
        }

        private IEnumerator FlashRoutine(Color color, float duration)
        {
            if (collectionFlash == null) yield break;

            collectionFlash.color = color;
            collectionFlash.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var c = color;
                c.a = color.a * (1f - elapsed / duration);
                collectionFlash.color = c;
                yield return null;
            }

            collectionFlash.gameObject.SetActive(false);
        }

        public void UpdateScore(int score, Rank rank)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {score:N0}";
            if (rankText != null && rank != Rank.None)
            {
                rankText.text = rank.ToString();
                rankText.color = rank switch
                {
                    Rank.S => new Color(1f, 0.85f, 0f),
                    Rank.A => new Color(1f, 0.3f, 0.3f),
                    Rank.B => new Color(0.3f, 0.7f, 1f),
                    _ => Color.gray
                };
            }
        }

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
