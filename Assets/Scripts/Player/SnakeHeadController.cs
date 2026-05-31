using UnityEngine;
using UnityEngine.InputSystem;
using NeonSerpent.Core;

// Conditional using for direct device access (fallback when no PlayerInput)
using Keyboard = UnityEngine.InputSystem.Keyboard;
using Mouse = UnityEngine.InputSystem.Mouse;
using Gamepad = UnityEngine.InputSystem.Gamepad;

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
        private float _baseSpeed;

        [Header("Mouse Look")]
        [SerializeField] private float lookSensitivity = 0.5f;
        [SerializeField] private float lookPitchMin = -80f;
        [SerializeField] private float lookPitchMax = 80f;

        [Header("References")]
        public Transform cameraTransform;
        public VerletSnakeBody snakeBody;

        private CharacterController _controller;
        private Vector3 _velocity;
        private Vector2 _lookInput;
        private Vector2 _moveInput;
        private float _verticalInput;
        private float _pitch;
        private float _yaw;
        private NeonSerpent.Gameplay.ComboSystem _cachedCombo;
        private SlideAbility _slideAbility;
        private GrappleAbility _grappleAbility;
        private bool _slideInputHeld;

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
            _baseSpeed = moveSpeed;
            _cachedCombo = FindObjectOfType<NeonSerpent.Gameplay.ComboSystem>();
            _slideAbility = GetComponent<SlideAbility>();
            _grappleAbility = GetComponent<GrappleAbility>();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void Update()
        {
            if (!GameStateManager.Instance.IsPlaying) return;

            ReadInputFallback();
            HandleLook();
            HandleMovement();
            UpdateHeadBob();
        }

        /// <summary>
        /// Direct device input fallback when PlayerInput messages are not wired.
        /// </summary>
        private void ReadInputFallback()
        {
            if (Keyboard.current != null)
            {
                var move = Vector2.zero;
                if (Keyboard.current.wKey.isPressed) move.y += 1;
                if (Keyboard.current.sKey.isPressed) move.y -= 1;
                if (Keyboard.current.aKey.isPressed) move.x -= 1;
                if (Keyboard.current.dKey.isPressed) move.x += 1;
                _moveInput = move;

                _verticalInput = 0;
                if (Keyboard.current.spaceKey.isPressed) _verticalInput += 1;
                if (Keyboard.current.leftCtrlKey.isPressed) _verticalInput -= 1;

                HandleSlideInput(Keyboard.current.leftCtrlKey.isPressed);

                if (Keyboard.current.leftShiftKey.wasPressedThisFrame && !_isDashing)
                {
                    if (_cachedCombo == null)
                        _cachedCombo = FindObjectOfType<NeonSerpent.Gameplay.ComboSystem>();
                    if (_cachedCombo != null && _cachedCombo.IsDashReady)
                    {
                        _cachedCombo.ActivateDash();
                        SetDashState(true);
                    }
                }

                if (Keyboard.current.eKey.wasPressedThisFrame)
                    TryActivateGrapple();

                if (Mouse.current != null)
                {
                    _lookInput = Mouse.current.delta.ReadValue() * 0.1f;
                }
            }
            else if (Gamepad.current != null)
            {
                _moveInput = Gamepad.current.leftStick.ReadValue();
                _lookInput = Gamepad.current.rightStick.ReadValue() * 2f;
                _verticalInput = Gamepad.current.dpad.up.isPressed ? 1f :
                    Gamepad.current.dpad.down.isPressed ? -1f : 0f;

                if (Gamepad.current.rightShoulder.wasPressedThisFrame && !_isDashing)
                {
                    if (_cachedCombo == null)
                        _cachedCombo = FindObjectOfType<NeonSerpent.Gameplay.ComboSystem>();
                    if (_cachedCombo != null && _cachedCombo.IsDashReady)
                    {
                        _cachedCombo.ActivateDash();
                        SetDashState(true);
                    }
                }

                HandleSlideInput(Gamepad.current.leftShoulder.isPressed);

                if (Gamepad.current.buttonWest.wasPressedThisFrame)
                    TryActivateGrapple();
            }
        }

        private void HandleSlideInput(bool pressed)
        {
            if (pressed == _slideInputHeld) return;
            _slideInputHeld = pressed;

            if (_slideAbility == null)
                _slideAbility = GetComponent<SlideAbility>();
            if (_slideAbility == null || !_slideAbility.enabled) return;

            if (pressed)
                _slideAbility.StartSlide();
            else
                _slideAbility.EndSlide();
        }

        private void TryActivateGrapple()
        {
            if (_grappleAbility == null)
                _grappleAbility = GetComponent<GrappleAbility>();
            if (_grappleAbility == null || !_grappleAbility.enabled) return;

            _grappleAbility.ActivateGrapple();
        }

        private void HandleLook()
        {
            _yaw += _lookInput.x * lookSensitivity;
            _pitch -= _lookInput.y * lookSensitivity;
            _pitch = Mathf.Clamp(_pitch, lookPitchMin, lookPitchMax);

            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            if (cameraTransform != null)
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
            HandleSlideInput(value.isPressed);
        }

        public void OnGrapple()
        {
            if (_isGrappling) return;
            TryActivateGrapple();
        }

        // ── Ability State Setters (called by ability components) ──

        public void SetDashState(bool active)
        {
            _isDashing = active;
        }

        public void SetWallRunState(bool active)
        {
            _isWallRunning = active;
            if (active) _velocity.y = 0f; // Cancel gravity during wall run
        }

        public void SetSlideState(bool active)
        {
            _isSliding = active;
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
