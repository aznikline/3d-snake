using UnityEngine;
using UnityEngine.InputSystem;
using NeonSerpent.Core;

namespace NeonSerpent.Player
{
    /// <summary>
    /// First-person snake head controller. Handles 6-DOF movement input,
    /// mouse look, and delegates body simulation to VerletSnakeBody.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class SnakeHeadController : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float moveSpeed = GameConstants.BaseSpeed;
        [SerializeField] private float gravity = -20f;

        [Header("Mouse Look")]
        [SerializeField] private float lookSensitivity = 0.5f;
        [SerializeField] private float lookPitchMin = -80f;
        [SerializeField] private float lookPitchMax = 80f;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private VerletSnakeBody snakeBody;

        private CharacterController _controller;
        private Vector3 _velocity;
        private Vector2 _lookInput;
        private Vector2 _moveInput;
        private float _verticalInput;
        private float _pitch;
        private float _yaw;

        // Ability states
        private bool _isDashing;
        private bool _isWallRunning;
        private bool _isSliding;
        private bool _isGrappling;

        public Vector3 HeadPosition => transform.position;
        public Vector3 HeadForward => transform.forward;
        public float CurrentSpeed => _velocity.magnitude;
        public bool IsDashing => _isDashing;

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            if (cameraTransform == null)
                cameraTransform = Camera.main?.transform;
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (!GameStateManager.Instance.IsPlaying) return;

            HandleLook();
            HandleMovement();
            UpdateHeadBob();
        }

        private void HandleLook()
        {
            _yaw += _lookInput.x * lookSensitivity;
            _pitch -= _lookInput.y * lookSensitivity;
            _pitch = Mathf.Clamp(_pitch, lookPitchMin, lookPitchMax);

            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            cameraTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        }

        private void HandleMovement()
        {
            if (_isGrappling) return; // Grapple handles its own movement

            Vector3 forward = transform.forward;
            Vector3 right = transform.right;
            Vector3 up = Vector3.up;

            Vector3 move = (forward * _moveInput.y + right * _moveInput.x + up * _verticalInput).normalized;

            float speed = _isDashing ? moveSpeed * GameConstants.DashSpeedMultiplier : moveSpeed;
            if (_isSliding) speed *= GameConstants.SlideSpeedBoost;
            if (_isWallRunning) speed *= 1.1f;

            Vector3 targetVelocity = move * speed;

            // Apply gravity when not grounded and not wall-running
            if (!_controller.isGrounded && !_isWallRunning)
            {
                _velocity.y += gravity * Time.deltaTime;
            }
            else if (_controller.isGrounded && _velocity.y < 0)
            {
                _velocity.y = -0.5f;
            }

            _velocity.x = targetVelocity.x;
            _velocity.z = targetVelocity.z;

            _controller.Move(_velocity * Time.deltaTime);

            // Sync snake body head node
            snakeBody?.SetHeadPosition(transform.position);
        }

        private void UpdateHeadBob()
        {
            if (_moveInput.magnitude < 0.1f || cameraTransform == null) return;

            float bob = Mathf.Sin(Time.time * GameConstants.CameraBobFrequency) * GameConstants.CameraBobAmplitude;
            cameraTransform.localPosition = new Vector3(0f, bob, 0f);
        }

        // ── Input Callbacks (Unity Input System) ──

        public void OnMove(InputValue value)
        {
            _moveInput = value.Get<Vector2>();
        }

        public void OnLook(InputValue value)
        {
            _lookInput = value.Get<Vector2>();
        }

        public void OnVertical(InputValue value)
        {
            _verticalInput = value.Get<float>();
        }

        public void OnDash()
        {
            if (_isDashing) return;
            // Dash activation is gated by ComboSystem
            // This method is called by ComboSystem when dash is ready and player presses button
        }

        public void OnSlide(InputValue value)
        {
            bool pressed = value.isPressed;
            if (pressed && !_isSliding && _controller.isGrounded)
            {
                StartSlide();
            }
            else if (!pressed && _isSliding)
            {
                EndSlide();
            }
        }

        public void OnGrapple()
        {
            if (_isGrappling) return;
            // Grapple activation handled by GrappleAbility
        }

        // ── Ability State Setters (called by ability components) ──

        public void SetDashState(bool active)
        {
            _isDashing = active;
            moveSpeed = active ? GameConstants.BaseSpeed * GameConstants.DashSpeedMultiplier : GameConstants.BaseSpeed;
        }

        public void SetWallRunState(bool active)
        {
            _isWallRunning = active;
            if (active) _velocity.y = 0f; // Cancel gravity during wall run
        }

        private void StartSlide()
        {
            _isSliding = true;
            _controller.height *= GameConstants.SlideHeightReduction;
            _controller.center = new Vector3(0f, -_controller.height * 0.25f, 0f);
        }

        private void EndSlide()
        {
            _isSliding = false;
            _controller.height /= GameConstants.SlideHeightReduction;
            _controller.center = Vector3.zero;
        }

        public void SetGrappleState(bool active)
        {
            _isGrappling = active;
        }

        /// <summary>
        /// Apply an external velocity override (e.g., from grapple launch or explosion).
        /// </summary>
        public void ApplyExternalVelocity(Vector3 velocity)
        {
            _velocity = velocity;
        }
    }
}
