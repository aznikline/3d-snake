using UnityEngine;
using NeonSerpent.Core;
using NeonSerpent.Player;

namespace NeonSerpent.Gameplay
{
    /// <summary>
    /// Collectible energy core. Pulses with light and particles.
    /// Detects snake head proximity directly (CharacterController doesn't fire OnTriggerEnter).
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class Food : MonoBehaviour
    {
        [Header("Visuals")]
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseScale = 0.2f;
        [SerializeField] private Color glowColor = Color.cyan;
        [SerializeField] private ParticleSystem collectParticles;

        [Header("Audio")]
        [SerializeField] private AudioClip collectSFX;

        private Vector3 _baseScale;
        private bool _collected;
        private float _collectRadius = 0.8f;

        public System.Action<Food> OnCollected;

        private void Awake()
        {
            _baseScale = transform.localScale;
            var col = GetComponent<SphereCollider>();
            col.isTrigger = true;
            _collectRadius = col.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z) * 1.2f;
        }

        private void Update()
        {
            if (_collected) return;

            // Pulse animation
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            transform.localScale = _baseScale * pulse;

            // Slow rotation
            transform.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);

            // Manual detection: CharacterController.Move() doesn't fire OnTriggerEnter
            CheckSnakeHeadProximity();
        }

        private void CheckSnakeHeadProximity()
        {
            // Search broadly since snake head may be on default layer
            var hits = Physics.OverlapSphere(transform.position, _collectRadius);
            foreach (var hit in hits)
            {
                var head = hit.GetComponent<SnakeHeadController>();
                if (head != null)
                {
                    Collect();
                    return;
                }
            }
        }

        /// <summary>
        /// Legacy support: if PlayerInput or Rigidbody-based player triggers this.
        /// </summary>
        private void OnTriggerEnter(Collider other)
        {
            if (_collected) return;
            var head = other.GetComponent<SnakeHeadController>();
            if (head != null)
            {
                Collect();
            }
        }

        private void Collect()
        {
            _collected = true;

            // Visual feedback
            if (collectParticles != null)
                Instantiate(collectParticles, transform.position, Quaternion.identity);

            // Audio feedback
            if (collectSFX != null)
                AudioSource.PlayClipAtPoint(collectSFX, transform.position);

            OnCollected?.Invoke(this);

            Destroy(gameObject);
        }

        /// <summary>
        /// Set the food's glow color dynamically (used for combo levels).
        /// </summary>
        public void SetGlowColor(Color color)
        {
            glowColor = color;
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = color;
            }
        }
    }
}
