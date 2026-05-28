using UnityEngine;
using NeonSerpent.Core;

namespace NeonSerpent.Player
{
    /// <summary>
    /// Slide ability. Reduces snake head height and increases speed
    /// to pass through low-clearance areas.
    /// </summary>
    public class SlideAbility : MonoBehaviour
    {
        [Header("Slide")]
        [SerializeField] private float slideDuration = GameConstants.SlideDurationMax;
        [SerializeField] private float heightReduction = GameConstants.SlideHeightReduction;
        [SerializeField] private float speedBoost = GameConstants.SlideSpeedBoost;

        [Header("Visuals")]
        [SerializeField] private ParticleSystem slideParticles;
        [SerializeField] private TrailRenderer slideTrail;

        private SnakeHeadController _controller;
        private CharacterController _characterController;
        private float _originalHeight;
        private Vector3 _originalCenter;
        private bool _isSliding;
        private float _slideTimer;

        public bool IsSliding => _isSliding;

        private void Awake()
        {
            _controller = GetComponent<SnakeHeadController>();
            _characterController = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (!GameStateManager.Instance.IsPlaying) return;

            if (_isSliding)
            {
                UpdateSlide();
            }
        }

        /// <summary>
        /// Call from input system when slide button is pressed.
        /// </summary>
        public void StartSlide()
        {
            if (_isSliding) return;
            if (!_characterController.isGrounded) return;

            _isSliding = true;
            _slideTimer = slideDuration;

            // Store original values
            _originalHeight = _characterController.height;
            _originalCenter = _characterController.center;

            // Reduce height
            _characterController.height *= heightReduction;
            _characterController.center = new Vector3(0f, -_characterController.height * 0.25f, 0f);

            // Visual feedback
            if (slideParticles != null)
                slideParticles.Play();
            if (slideTrail != null)
                slideTrail.emitting = true;
        }

        /// <summary>
        /// Call from input system when slide button is released.
        /// </summary>
        public void EndSlide()
        {
            if (!_isSliding) return;

            // Check if there's room to stand up
            if (!CanStandUp())
            {
                // Force continue sliding until clear
                _slideTimer = 0.1f;
                return;
            }

            _isSliding = false;

            // Restore original values
            _characterController.height = _originalHeight;
            _characterController.center = _originalCenter;

            // Visual feedback
            if (slideParticles != null)
                slideParticles.Stop();
            if (slideTrail != null)
                slideTrail.emitting = false;
        }

        private void UpdateSlide()
        {
            _slideTimer -= Time.deltaTime;

            // Auto-end if timer expires
            if (_slideTimer <= 0f)
            {
                EndSlide();
            }
        }

        private bool CanStandUp()
        {
            // SphereCast upward to check for obstacles
            Vector3 origin = transform.position + Vector3.up * _characterController.height * 0.5f;
            float checkDistance = _originalHeight - _characterController.height;

            return !Physics.SphereCast(origin, _characterController.radius * 0.9f,
                Vector3.up, out _, checkDistance, LayerMask.GetMask("Environment"));
        }
    }
}
