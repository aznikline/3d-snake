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

        public bool HasPlayableMusic =>
            ambientSource != null && ambientSource.clip != null &&
            dnbSource != null && dnbSource.clip != null;

        public void EnsurePlayableMusic()
        {
            EnsureProceduralFallbackMusic();
        }

        private void Awake()
        {
            if (audioMixer != null)
            {
                _ambientSnapshot = audioMixer.FindSnapshot(ambientSnapshotName);
                _dnbSnapshot = audioMixer.FindSnapshot(dnbSnapshotName);
            }

            EnsureProceduralFallbackMusic();
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
            EnsureProceduralFallbackMusic();
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
            EnsureProceduralFallbackMusic();
            ambientSource?.Pause();
            dnbSource?.Pause();
        }

        /// <summary>
        /// Resume music.
        /// </summary>
        public void Resume()
        {
            EnsureProceduralFallbackMusic();
            ambientSource?.Play();
            dnbSource?.Play();
        }

        private void EnsureProceduralFallbackMusic()
        {
            ambientSource = EnsureMusicSource(ambientSource, "AmbientStem", 0.65f);
            dnbSource = EnsureMusicSource(dnbSource, "DnBStem", 0f);

            if (ambientSource.clip == null)
                ambientSource.clip = GenerateAmbientLoop();

            if (dnbSource.clip == null)
                dnbSource.clip = GenerateDnBLoop();

            if (!ambientSource.isPlaying)
                ambientSource.Play();

            if (!dnbSource.isPlaying)
                dnbSource.Play();
        }

        private AudioSource EnsureMusicSource(AudioSource source, string sourceName, float volume)
        {
            if (source == null)
            {
                var sourceGO = new GameObject(sourceName);
                sourceGO.transform.SetParent(transform);
                source = sourceGO.AddComponent<AudioSource>();
            }

            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = volume;
            return source;
        }

        private AudioClip GenerateAmbientLoop()
        {
            const int sampleRate = 22050;
            const float duration = 8f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float time = i / (float)sampleRate;
                float beatPhase = Mathf.Repeat(time * GameConstants.MusicBPMAmbient / 60f, 1f);
                float pulse = Mathf.Exp(-beatPhase * 4f) * 0.25f;
                float pad =
                    Mathf.Sin(2f * Mathf.PI * 55f * time) * 0.28f +
                    Mathf.Sin(2f * Mathf.PI * 110f * time) * 0.12f +
                    Mathf.Sin(2f * Mathf.PI * 220f * time) * 0.05f;

                data[i] = Mathf.Clamp((pad + pulse) * 0.45f, -1f, 1f);
            }

            var clip = AudioClip.Create("ProceduralAmbientLoop", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateDnBLoop()
        {
            const int sampleRate = 22050;
            const float duration = 4f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float time = i / (float)sampleRate;
                float beat = time * GameConstants.MusicBPMDnB / 60f;
                float quarterPhase = Mathf.Repeat(beat, 1f);
                int sixteenth = Mathf.FloorToInt(Mathf.Repeat(beat * 4f, 16f));

                float kick = EnvelopePulse(quarterPhase, 12f) * Mathf.Sin(2f * Mathf.PI * 58f * time) * 0.75f;
                float snare = (sixteenth == 4 || sixteenth == 12)
                    ? EnvelopePulse(Mathf.Repeat(beat * 4f, 1f), 18f) * HashNoise(i) * 0.35f
                    : 0f;
                float hat = (sixteenth % 2 == 1)
                    ? EnvelopePulse(Mathf.Repeat(beat * 4f, 1f), 28f) * HashNoise(i + 17) * 0.12f
                    : 0f;
                float bass = Mathf.Sin(2f * Mathf.PI * (beat % 2f < 1f ? 82f : 98f) * time) * 0.18f;

                data[i] = Mathf.Clamp((kick + snare + hat + bass) * 0.6f, -1f, 1f);
            }

            var clip = AudioClip.Create("ProceduralDnBLoop", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private float EnvelopePulse(float phase, float decay)
        {
            return Mathf.Exp(-phase * decay);
        }

        private float HashNoise(int index)
        {
            int n = index * 1103515245 + 12345;
            n = (n >> 16) & 0x7fff;
            return (n / 16383.5f) - 1f;
        }
    }
}
