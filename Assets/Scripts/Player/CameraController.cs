using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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

        [Header("Post-Processing")]
        [SerializeField] private Volume postProcessVolume;

        private Camera _camera;
        private SnakeHeadController _snakeController;
        private float _targetFOV;
        private float _currentShake;
        private ChromaticAberration _chromaticAberration;
        private MotionBlur _motionBlur;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _snakeController = GetComponentInParent<SnakeHeadController>();

            if (postProcessVolume != null && postProcessVolume.profile != null)
            {
                postProcessVolume.profile.TryGet(out _chromaticAberration);
                postProcessVolume.profile.TryGet(out _motionBlur);
            }
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
            UpdatePostProcessing();
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

        private void UpdatePostProcessing()
        {
            bool isDashing = _snakeController != null && _snakeController.IsDashing;

            float targetCA = isDashing ? chromaticAberrationIntensity : 0f;
            float targetMB = isDashing ? motionBlurIntensity : 0f;

            if (_chromaticAberration != null)
                _chromaticAberration.intensity.value = Mathf.Lerp(
                    _chromaticAberration.intensity.value, targetCA, Time.deltaTime * 10f);

            if (_motionBlur != null)
                _motionBlur.intensity.value = Mathf.Lerp(
                    _motionBlur.intensity.value, targetMB, Time.deltaTime * 10f);
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
        /// Set chromatic aberration intensity. Called during dash or high-speed moments.
        /// </summary>
        public void SetChromaticAberration(float intensity)
        {
            if (_chromaticAberration != null)
                _chromaticAberration.intensity.value = intensity;
        }

        /// <summary>
        /// Set motion blur intensity.
        /// </summary>
        public void SetMotionBlur(float intensity)
        {
            if (_motionBlur != null)
                _motionBlur.intensity.value = intensity;
        }
    }
}
