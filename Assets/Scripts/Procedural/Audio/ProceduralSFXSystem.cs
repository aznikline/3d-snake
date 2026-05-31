using UnityEngine;

namespace NeonSerpent.Procedural.Audio
{
    /// <summary>
    /// Procedural sound effects generated at runtime.
    /// No external audio files required.
    /// </summary>
    public class ProceduralSFXSystem : MonoBehaviour
    {
        public static ProceduralSFXSystem Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private int maxAudioSources = 16;
        [SerializeField] private float sfxVolume = 0.5f;

        private AudioSource[] _audioSources;
        private int _nextSourceIndex;

        public int AudioSourceCount => _audioSources?.Length ?? 0;
        public bool HasAudioPool => AudioSourceCount > 0;

        public void EnsureAudioPool()
        {
            EnsureAudioSources();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsureAudioSources();
        }

        // ── SFX Playback ──

        /// <summary>
        /// Play food collection sound.
        /// </summary>
        public void PlayFoodCollect(Vector3 position, int comboLevel = 0)
        {
            float pitch = 1f + comboLevel * 0.1f;
            PlaySFX(position, GenerateFoodCollectClip(pitch));
        }

        /// <summary>
        /// Play dash activation sound.
        /// </summary>
        public void PlayDash(Vector3 position)
        {
            PlaySFX(position, GenerateDashClip());
        }

        /// <summary>
        /// Play death impact sound.
        /// </summary>
        public void PlayDeath(Vector3 position)
        {
            PlaySFX(position, GenerateDeathClip());
        }

        /// <summary>
        /// Play wall-run sound.
        /// </summary>
        public void PlayWallRun(Vector3 position)
        {
            PlaySFX(position, GenerateWallRunClip());
        }

        /// <summary>
        /// Play grapple launch sound.
        /// </summary>
        public void PlayGrapple(Vector3 position)
        {
            PlaySFX(position, GenerateGrappleClip());
        }

        /// <summary>
        /// Play UI click sound.
        /// </summary>
        public void PlayUIClick()
        {
            PlaySFX(Vector3.zero, GenerateUIClickClip(), 0f);
        }

        /// <summary>
        /// Play unlock notification sound.
        /// </summary>
        public void PlayUnlock()
        {
            PlaySFX(Vector3.zero, GenerateUnlockClip(), 0f);
        }

        // ── Clip Generation ──

        private AudioClip GenerateFoodCollectClip(float pitch)
        {
            int sampleRate = 44100;
            float duration = 0.15f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            float baseFreq = 880f * pitch; // A5
            float freqSweep = 1760f * pitch; // A6

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float freq = Mathf.Lerp(baseFreq, freqSweep, t);
                float envelope = Mathf.Sin(t * Mathf.PI); // Simple sine envelope

                data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / sampleRate) * envelope * 0.5f;
            }

            AudioClip clip = AudioClip.Create("FoodCollect", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateDashClip()
        {
            int sampleRate = 44100;
            float duration = 0.5f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float freq = Mathf.Lerp(200f, 2000f, t);
                float envelope = Mathf.Exp(-t * 3f);

                // Saw wave with lowpass
                float saw = (t * freq % 1f) * 2f - 1f;
                data[i] = saw * envelope * 0.4f;
            }

            AudioClip clip = AudioClip.Create("Dash", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateDeathClip()
        {
            int sampleRate = 44100;
            float duration = 0.8f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float freq = Mathf.Lerp(400f, 50f, t);
                float envelope = Mathf.Exp(-t * 2f);

                data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / sampleRate) * envelope * 0.6f;
            }

            AudioClip clip = AudioClip.Create("Death", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateWallRunClip()
        {
            int sampleRate = 44100;
            float duration = 0.3f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                // Metallic scrape sound using noise + resonance
                float noise = Random.Range(-1f, 1f);
                float resonance = Mathf.Sin(2f * Mathf.PI * 2000f * i / sampleRate);
                float envelope = Mathf.Exp(-t * 5f);

                data[i] = (noise * 0.3f + resonance * 0.7f) * envelope * 0.3f;
            }

            AudioClip clip = AudioClip.Create("WallRun", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateGrappleClip()
        {
            int sampleRate = 44100;
            float duration = 0.4f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float freq = Mathf.Lerp(1000f, 100f, t);
                float envelope = Mathf.Exp(-t * 4f);

                data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / sampleRate) * envelope * 0.5f;
            }

            AudioClip clip = AudioClip.Create("Grapple", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateUIClickClip()
        {
            int sampleRate = 44100;
            float duration = 0.05f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float envelope = Mathf.Exp(-t * 10f);
                data[i] = Mathf.Sin(2f * Mathf.PI * 2000f * i / sampleRate) * envelope * 0.3f;
            }

            AudioClip clip = AudioClip.Create("UIClick", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateUnlockClip()
        {
            int sampleRate = 44100;
            float duration = 1f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];

            // Arpeggio: A minor (A, C, E)
            float[] notes = { 440f, 523.25f, 659.25f, 880f };
            float noteDuration = duration / notes.Length;

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                int noteIndex = Mathf.FloorToInt(t * notes.Length);
                noteIndex = Mathf.Clamp(noteIndex, 0, notes.Length - 1);

                float freq = notes[noteIndex];
                float envelope = Mathf.Exp(-(t % noteDuration) * 5f);

                data[i] = Mathf.Sin(2f * Mathf.PI * freq * i / sampleRate) * envelope * 0.4f;
            }

            AudioClip clip = AudioClip.Create("Unlock", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        // ── Playback ──

        private void PlaySFX(Vector3 position, AudioClip clip, float spatialBlend = 0.5f)
        {
            if (clip == null) return;

            EnsureAudioSources();

            AudioSource source = GetAvailableSource();
            if (source == null) return;

            source.transform.position = position;
            source.spatialBlend = spatialBlend;
            source.volume = sfxVolume;
            source.clip = clip;
            source.Play();
        }

        private AudioSource GetAvailableSource()
        {
            if (_audioSources == null || _audioSources.Length == 0) return null;

            // Find a source that's not playing
            for (int i = 0; i < _audioSources.Length; i++)
            {
                int index = (_nextSourceIndex + i) % _audioSources.Length;
                if (!_audioSources[index].isPlaying)
                {
                    _nextSourceIndex = (index + 1) % _audioSources.Length;
                    return _audioSources[index];
                }
            }

            // All sources busy - steal the oldest one
            var source = _audioSources[_nextSourceIndex];
            _nextSourceIndex = (_nextSourceIndex + 1) % _audioSources.Length;
            return source;
        }

        private void EnsureAudioSources()
        {
            if (_audioSources != null && _audioSources.Length > 0)
                return;

            int sourceCount = Mathf.Max(1, maxAudioSources);
            _audioSources = new AudioSource[sourceCount];
            for (int i = 0; i < sourceCount; i++)
            {
                var source = gameObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0.5f;
                _audioSources[i] = source;
            }
        }
    }
}
