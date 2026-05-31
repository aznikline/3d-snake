using UnityEngine;
using NeonSerpent.Core;

namespace NeonSerpent.Player
{
    /// <summary>
    /// Wall-run ability. Automatically triggers when the snake head
    /// approaches a wall at the correct angle. Cancels gravity and
    /// redirects movement along the wall surface.
    /// </summary>
    public class WallRunAbility : MonoBehaviour
    {
        [Header("Detection")]
        [SerializeField] private float wallCheckDistance = 1f;
        [SerializeField] private float wallRunSpeedMultiplier = 1.1f;
        public LayerMask wallLayer;

        [Header("Visuals")]
        [SerializeField] private ParticleSystem wallRunParticles;
        [SerializeField] private float tiltAngle = 15f;

        private SnakeHeadController _controller;
        private bool _isWallRunning;
        private Vector3 _wallNormal;
        private float _wallRunTimer;
        private Transform _cameraTransform;
        private Quaternion _originalCameraLocalRotation;

        public bool IsWallRunning => _isWallRunning;

        private void Awake()
        {
            _controller = GetComponent<SnakeHeadController>();
            _cameraTransform = GetComponentInChildren<Camera>()?.transform;
        }

        private void Update()
        {
            if (!GameStateManager.Instance.IsPlaying) return;

            if (_isWallRunning)
            {
                UpdateWallRun();
            }
            else
            {
                CheckForWallRunStart();
            }
        }

        private void CheckForWallRunStart()
        {
            // Raycast to the right and left of the head
            Vector3 origin = transform.position + Vector3.up * 0.5f;

            if (Physics.Raycast(origin, transform.right, out RaycastHit rightHit, wallCheckDistance, wallLayer))
            {
                TryStartWallRun(rightHit.normal, rightHit.point);
            }
            else if (Physics.Raycast(origin, -transform.right, out RaycastHit leftHit, wallCheckDistance, wallLayer))
            {
                TryStartWallRun(leftHit.normal, leftHit.point);
            }
        }

        private void TryStartWallRun(Vector3 wallNormal, Vector3 wallPoint)
        {
            // Check approach angle
            float approachAngle = Vector3.Angle(transform.forward, -wallNormal);
            if (approachAngle < GameConstants.WallRunAngleMin || approachAngle > GameConstants.WallRunAngleMax)
                return;

            // Check if player is moving forward
            if (_controller.CurrentSpeed < GameConstants.BaseSpeed * 0.5f)
                return;

            StartWallRun(wallNormal);
        }

        private void StartWallRun(Vector3 wallNormal)
        {
            _isWallRunning = true;
            _wallNormal = wallNormal;
            _wallRunTimer = GameConstants.WallRunDurationMax;
            _controller.SetWallRunState(true);

            // Save camera rotation before tilting
            if (_cameraTransform != null)
            {
                _originalCameraLocalRotation = _cameraTransform.localRotation;
                float tiltDir = Vector3.Dot(transform.right, -wallNormal) > 0 ? 1f : -1f;
                _cameraTransform.localRotation *= Quaternion.Euler(0f, 0f, tiltAngle * tiltDir);
            }

            if (wallRunParticles != null)
                wallRunParticles.Play();
        }

        private void UpdateWallRun()
        {
            _wallRunTimer -= Time.deltaTime;

            // Check if wall still exists
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            if (!Physics.Raycast(origin, -_wallNormal, out RaycastHit hit, wallCheckDistance * 1.5f, wallLayer))
            {
                EndWallRun();
                return;
            }

            // Update wall normal in case of curved surfaces
            _wallNormal = hit.normal;

            // Redirect movement along wall
            Vector3 wallForward = Vector3.Cross(_wallNormal, Vector3.up).normalized;
            if (Vector3.Dot(wallForward, transform.forward) < 0)
                wallForward = -wallForward;

            // End wall run if timer expires
            if (_wallRunTimer <= 0f)
            {
                EndWallRun();
            }
        }

        private void EndWallRun()
        {
            _isWallRunning = false;
            _controller.SetWallRunState(false);

            // Restore camera rotation
            if (_cameraTransform != null)
            {
                _cameraTransform.localRotation = _originalCameraLocalRotation;
            }

            if (wallRunParticles != null)
                wallRunParticles.Stop();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Vector3 origin = transform.position + Vector3.up * 0.5f;
            Gizmos.DrawRay(origin, transform.right * wallCheckDistance);
            Gizmos.DrawRay(origin, -transform.right * wallCheckDistance);
        }
    }
}
