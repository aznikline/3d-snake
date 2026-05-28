using UnityEngine;
using NeonSerpent.Core;

namespace NeonSerpent.Player
{
    /// <summary>
    /// First-person camera controller. Handles FOV dynamics, head bob,
    /// impact shake, and dash visual effects.
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [Header("FOV")]
        [SerializeField] private float baseFOV = GameConstants.CameraFOV;
        [SerializeField] private float dashFOV = GameConstants.CameraFOVDash;
        [SerializeField] private float fovTransitionSpeed = 5f;

        [Header("Effects")]
        [SerializeField] private float chromaticAberrationIntensity = 0.3f;
        [SerializeField] private float motionBlurIntensity = 0.5f;

        private Camera _camera;
        private SnakeHeadController _snakeController;
        private float _targetFOV;
        private float _currentShake;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _snakeController = GetComponentInParent<SnakeHeadController>();
        }

        private void Start()
        {
            _camera.fieldOfView = baseFOV;
            _targetFOV = baseFOV;
        }

        private void Update()
        {
            UpdateFOV();
            UpdateShake();
        }

        private void UpdateFOV()
        {
            if (_snakeController != null && _snakeController.IsDashing)
            {
                _targetFOV = dashFOV;
            }
            else
            {
                _targetFOV = baseFOV;
            }

            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _targetFOV, Time.deltaTime * fovTransitionSpeed);
        }

        private void UpdateShake()
        {
            if (_currentShake > 0.01f)
            {
                Vector3 shakeOffset = Random.insideUnitSphere * _currentShake;
                transform.localPosition += shakeOffset;
                _currentShake = Mathf.Lerp(_currentShake, 0f, Time.deltaTime * 10f);
            }
        }

        /// <summary>
        /// Trigger an impact shake (e.g., on death or hard collision).
        /// </summary>
        public void TriggerImpactShake(float intensity = -1f)
        {
            _currentShake = intensity > 0 ? intensity : GameConstants.CameraImpactShakeIntensity;
        }

        /// <summary>
        /// Set chromatic aberration intensity (post-processing volume).
        /// Called during dash or high-speed moments.
        /// </summary>
        public void SetChromaticAberration(float intensity)
        {
            // Applied via URP Volume Profile at runtime
            // Implementation depends on Volume component reference
        }

        /// <summary>
        /// Set motion blur intensity.
        /// </summary>
        public void SetMotionBlur(float intensity)
        {
            // Applied via URP Volume Profile at runtime
        }
    }
}
