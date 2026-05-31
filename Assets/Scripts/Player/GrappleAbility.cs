using System.Collections;
using UnityEngine;
using NeonSerpent.Core;
using NeonSerpent.Procedural.Audio;

namespace NeonSerpent.Player
{
    /// <summary>
    /// Grapple ability. Launches the snake head toward a targeted grapple point
    /// along a ballistic arc. Player has no control during flight.
    /// </summary>
    public class GrappleAbility : MonoBehaviour
    {
        [Header("Targeting")]
        [SerializeField] private float maxGrappleDistance = 30f;
        [SerializeField] private float targetAngleThreshold = 30f;
        public LayerMask grappleLayer;

        [Header("Flight")]
        [SerializeField] private float flightDuration = GameConstants.GrappleDuration;
        [SerializeField] private AnimationCurve flightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Visuals")]
        [SerializeField] private LineRenderer grappleLine;
        [SerializeField] private ParticleSystem launchParticles;
        [SerializeField] private ParticleSystem impactParticles;

        private SnakeHeadController _controller;
        private bool _isGrappling;
        private Transform _targetPoint;

        public bool IsGrappling => _isGrappling;
        public bool HasValidTarget => FindBestTarget() != null;

        private void Awake()
        {
            _controller = GetComponent<SnakeHeadController>();
            if (grappleLine != null)
                grappleLine.enabled = false;
        }

        /// <summary>
        /// Attempt to activate grapple. Returns true if a valid target was found and grapple initiated.
        /// </summary>
        public bool ActivateGrapple()
        {
            if (_isGrappling) return false;

            Transform target = FindBestTarget();
            if (target == null) return false;

            // Check line of sight
            Vector3 direction = target.position - transform.position;
            if (Physics.Raycast(transform.position, direction.normalized, out RaycastHit hit,
                direction.magnitude, LayerMask.GetMask("Environment")))
            {
                if (hit.transform != target)
                    return false; // Obstructed
            }

            ProceduralSFXSystem.Instance?.PlayGrapple(transform.position);
            StartCoroutine(GrappleFlight(target));
            return true;
        }

        private Transform FindBestTarget()
        {
            Collider[] nearby = Physics.OverlapSphere(transform.position, maxGrappleDistance, grappleLayer);
            Transform bestTarget = null;
            float bestScore = float.MaxValue;

            foreach (var col in nearby)
            {
                Vector3 toTarget = col.transform.position - transform.position;
                float distance = toTarget.magnitude;
                float angle = Vector3.Angle(transform.forward, toTarget);

                // Score: prefer closer and more forward-facing targets
                float score = distance + angle * 0.5f;

                if (angle < targetAngleThreshold && score < bestScore)
                {
                    bestScore = score;
                    bestTarget = col.transform;
                }
            }

            return bestTarget;
        }

        private IEnumerator GrappleFlight(Transform target)
        {
            _isGrappling = true;
            _controller.SetGrappleState(true);
            _targetPoint = target;

            Vector3 startPos = transform.position;
            Vector3 endPos = target.position;
            float elapsed = 0f;

            if (launchParticles != null)
                launchParticles.Play();

            if (grappleLine != null)
            {
                grappleLine.enabled = true;
                grappleLine.positionCount = 2;
            }

            while (elapsed < flightDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flightDuration;
                float curveT = flightCurve.Evaluate(t);

                // Parabolic arc
                Vector3 linearPos = Vector3.Lerp(startPos, endPos, curveT);
                float arcHeight = Vector3.Distance(startPos, endPos) * 0.3f;
                linearPos.y += Mathf.Sin(curveT * Mathf.PI) * arcHeight;

                transform.position = linearPos;

                // Update grapple line
                if (grappleLine != null)
                {
                    grappleLine.SetPosition(0, transform.position);
                    grappleLine.SetPosition(1, endPos);
                }

                yield return null;
            }

            // Arrival
            transform.position = endPos;

            if (impactParticles != null)
                Instantiate(impactParticles, endPos, Quaternion.identity);

            if (grappleLine != null)
                grappleLine.enabled = false;

            _isGrappling = false;
            _controller.SetGrappleState(false);
            _targetPoint = null;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, maxGrappleDistance);

            // Forward cone
            Vector3 forward = transform.forward * maxGrappleDistance;
            Vector3 left = Quaternion.Euler(0f, -targetAngleThreshold, 0f) * forward;
            Vector3 right = Quaternion.Euler(0f, targetAngleThreshold, 0f) * forward;

            Gizmos.DrawLine(transform.position, transform.position + left);
            Gizmos.DrawLine(transform.position, transform.position + right);
        }
    }
}
