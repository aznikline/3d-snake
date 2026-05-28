using UnityEngine;
using NeonSerpent.Core;

namespace NeonSerpent.Gameplay
{
    /// <summary>
    /// Manages the combo meter and neon dash ability.
    /// Combo builds by eating food consecutively; decay starts when
    /// no food is eaten within the timeout window.
    /// </summary>
    public class ComboSystem : MonoBehaviour
    {
        [Header("Combo")]
        [SerializeField] private float fillPerFood = GameConstants.ComboFillPerFood;
        [SerializeField] private float decayPerSecond = GameConstants.ComboDecayPerSecond;
        [SerializeField] private float timeoutDuration = GameConstants.ComboTimeout;

        [Header("Dash")]
        [SerializeField] private float dashDuration = GameConstants.DashDuration;
        [SerializeField] private float dashCooldown = GameConstants.DashCooldown;
        [SerializeField] private int maxDashCharges = GameConstants.MaxDashCharges;

        [Header("Events")]
        public System.Action<float> OnComboChanged;     // normalized 0-1
        public System.Action OnComboReady;
        public System.Action OnDashStarted;
        public System.Action OnDashEnded;
        public System.Action OnDeathPrevented;

        public float ComboMeter { get; private set; }
        public bool IsDashReady => ComboMeter >= 1f && _dashCharges > 0;
        public bool IsDashing => _isDashing;
        public int CurrentComboCount => _comboCount;

        private float _comboTimer;
        private int _comboCount;
        private bool _isDashing;
        private float _dashTimer;
        private float _dashCooldownTimer;
        private int _dashCharges;

        private void Start()
        {
            _dashCharges = maxDashCharges;
        }

        private void Update()
        {
            UpdateComboDecay();
            UpdateDashTimer();
        }

        /// <summary>
        /// Call when food is eaten. Increases combo meter and count.
        /// </summary>
        public void OnFoodEaten()
        {
            _comboCount++;
            _comboTimer = timeoutDuration;

            float previousMeter = ComboMeter;
            ComboMeter = Mathf.Min(ComboMeter + fillPerFood, 1f);

            OnComboChanged?.Invoke(ComboMeter);

            if (previousMeter < 1f && ComboMeter >= 1f)
            {
                OnComboReady?.Invoke();
            }
        }

        /// <summary>
        /// Activate dash. Returns true if dash was successfully activated.
        /// </summary>
        public bool ActivateDash()
        {
            if (!IsDashReady || _isDashing || _dashCooldownTimer > 0) return false;

            _isDashing = true;
            _dashTimer = dashDuration;
            _dashCharges--;
            ComboMeter = 0f; // Consume combo meter
            _comboCount = 0;

            OnDashStarted?.Invoke();
            OnComboChanged?.Invoke(ComboMeter);

            return true;
        }

        /// <summary>
        /// Call when a fatal collision occurs during dash.
        /// Consumes dash to prevent death if possible.
        /// </summary>
        public bool TryPreventDeath()
        {
            if (!_isDashing) return false;

            // End dash early, trigger invincibility
            _isDashing = false;
            _dashTimer = 0f;

            OnDeathPrevented?.Invoke();
            OnDashEnded?.Invoke();

            return true;
        }

        /// <summary>
        /// Reset combo and dash state (e.g., on death or level restart).
        /// </summary>
        public void Reset()
        {
            ComboMeter = 0f;
            _comboCount = 0;
            _comboTimer = 0f;
            _isDashing = false;
            _dashTimer = 0f;
            _dashCooldownTimer = 0f;
            _dashCharges = maxDashCharges;

            OnComboChanged?.Invoke(0f);
        }

        private void UpdateComboDecay()
        {
            if (_isDashing) return; // No decay during dash

            _comboTimer -= Time.deltaTime;

            if (_comboTimer <= 0f && ComboMeter > 0f)
            {
                ComboMeter = Mathf.Max(ComboMeter - decayPerSecond * Time.deltaTime, 0f);
                OnComboChanged?.Invoke(ComboMeter);

                if (ComboMeter <= 0f)
                {
                    _comboCount = 0;
                }
            }
        }

        private void UpdateDashTimer()
        {
            if (_isDashing)
            {
                _dashTimer -= Time.deltaTime;
                if (_dashTimer <= 0f)
                {
                    _isDashing = false;
                    _dashCooldownTimer = dashCooldown;
                    OnDashEnded?.Invoke();
                }
            }
            else if (_dashCooldownTimer > 0f)
            {
                _dashCooldownTimer -= Time.deltaTime;
            }
        }
    }
}
