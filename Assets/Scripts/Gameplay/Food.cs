using UnityEngine;

namespace NeonSerpent.Gameplay
{
    /// <summary>
    /// Collectible energy core. Pulses with light and particles.
    /// Triggers collection when the snake head enters its trigger volume.
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

        public System.Action<Food> OnCollected;

        private void Awake()
        {
            _baseScale = transform.localScale;
            GetComponent<SphereCollider>().isTrigger = true;
        }

        private void Update()
        {
            if (_collected) return;

            // Pulse animation
            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            transform.localScale = _baseScale * pulse;

            // Slow rotation
            transform.Rotate(Vector3.up, 90f * Time.deltaTime, Space.World);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_collected) return;

            // Check if collider is snake head
            if (other.CompareTag("SnakeHead"))
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
                renderer.material.SetColor("_EmissionColor", color * 2f);
            }
        }
    }
}
