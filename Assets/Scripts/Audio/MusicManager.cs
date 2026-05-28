using UnityEngine;
using UnityEngine.Audio;
using NeonSerpent.Core;

namespace NeonSerpent.Audio
{
    /// <summary>
    /// Dynamic music system with stem-based crossfading.
    /// BPM and intensity adapt to snake speed and combo state.
    /// </summary>
    public class MusicManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource ambientSource;
        [SerializeField] private AudioSource dnbSource;

        [Header("Snapshots")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string ambientSnapshotName = "Ambient";
        [SerializeField] private string dnbSnapshotName = "DnB";

        [Header("Parameters")]
        [SerializeField] private float crossfadeDuration = GameConstants.MusicCrossfadeDuration;

        private AudioMixerSnapshot _ambientSnapshot;
        private AudioMixerSnapshot _dnbSnapshot;
        private float _currentIntensity;

        private void Awake()
        {
            if (audioMixer != null)
            {
                _ambientSnapshot = audioMixer.FindSnapshot(ambientSnapshotName);
                _dnbSnapshot = audioMixer.FindSnapshot(dnbSnapshotName);
            }
        }

        private void Start()
        {
            // Start with ambient
            SetIntensity(0f);
        }

        /// <summary>
        /// Set music intensity (0 = ambient, 1 = full DnB).
        /// Called based on combo meter and snake speed.
        /// </summary>
        public void SetIntensity(float intensity)
        {
            _currentIntensity = Mathf.Clamp01(intensity);

            if (_ambientSnapshot != null && _dnbSnapshot != null)
            {
                float[] weights = { 1f - _currentIntensity, _currentIntensity };
                AudioMixerSnapshot[] snapshots = { _ambientSnapshot, _dnbSnapshot };
                audioMixer.TransitionToSnapshots(snapshots, weights, crossfadeDuration);
            }
            else
            {
                // Fallback: direct volume control
                if (ambientSource != null)
                    ambientSource.volume = 1f - _currentIntensity;
                if (dnbSource != null)
                    dnbSource.volume = _currentIntensity;
            }
        }

        /// <summary>
        /// Update intensity based on gameplay state.
        /// Call from a gameplay manager every frame or on state change.
        /// </summary>
        public void UpdateIntensity(float speed, float comboMeter, bool isDashing)
        {
            float targetIntensity = 0f;

            if (isDashing)
            {
                targetIntensity = 1f;
            }
            else if (comboMeter > 0.5f)
            {
                targetIntensity = 0.5f + comboMeter * 0.5f;
            }
            else if (speed > GameConstants.BaseSpeed * 1.2f)
            {
                targetIntensity = 0.3f;
            }

            SetIntensity(Mathf.Lerp(_currentIntensity, targetIntensity, Time.deltaTime * 2f));
        }

        /// <summary>
        /// Pause music (e.g., on death or pause menu).
        /// </summary>
        public void Pause()
        {
            ambientSource?.Pause();
            dnbSource?.Pause();
        }

        /// <summary>
        /// Resume music.
        /// </summary>
        public void Resume()
        {
            ambientSource?.Play();
            dnbSource?.Play();
        }
    }
}
