using UnityEngine;
using UnityEngine.Audio;

namespace NeonSerpent.Procedural.Audio
{
    /// <summary>
    /// Procedural music system using synthesizer principles.
    /// Generates electronic music in real-time with dynamic BPM and intensity.
    /// No external audio files required.
    /// </summary>
    public class ProceduralMusicSystem : MonoBehaviour
    {
        [Header("Audio Output")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private int sampleRate = 44100;
        [SerializeField] private int bufferSize = 1024;

        [Header("Music Parameters")]
        [SerializeField] private float baseBPM = 90f;
        [SerializeField] private float maxBPM = 174f;
        [SerializeField] private float bpmTransitionSpeed = 2f;

        [Header("Synth Voices")]
        [SerializeField] private int maxVoices = 8;
        [SerializeField] private float masterVolume = 0.3f;

        // Runtime state
        private float _currentBPM;
        private float _targetBPM;
        private float _intensity;
        private double _sampleTime;
        private SynthVoice[] _voices;
        private Sequencer _sequencer;
        private bool _isPlaying;

        // Scale: A minor pentatonic (A, C, D, E, G)
        private readonly float[] _scale = { 440f, 523.25f, 587.33f, 659.25f, 783.99f };
        private readonly float[] _bassScale = { 110f, 130.81f, 146.83f, 164.81f, 196f };

        private void Awake()
        {
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();

            InitializeAudio();
        }

        private void Start()
        {
            _currentBPM = baseBPM;
            _targetBPM = baseBPM;
            _sequencer = new Sequencer(_scale, _bassScale);
            _voices = new SynthVoice[maxVoices];

            for (int i = 0; i < maxVoices; i++)
            {
                _voices[i] = new SynthVoice(sampleRate);
            }
        }

        private void Update()
        {
            // Smooth BPM transition
            _currentBPM = Mathf.Lerp(_currentBPM, _targetBPM, Time.deltaTime * bpmTransitionSpeed);

            // Update sequencer
            if (_isPlaying)
            {
                _sequencer.Update(_currentBPM, Time.deltaTime, _intensity);
            }
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (!_isPlaying || _sequencer == null) return;

            double samplesPerBeat = sampleRate * 60.0 / _currentBPM;

            for (int i = 0; i < data.Length; i += channels)
            {
                float sample = 0f;

                // Get active notes from sequencer
                var activeNotes = _sequencer.GetActiveNotes(_sampleTime, samplesPerBeat);

                // Assign voices to notes
                int voiceIndex = 0;
                foreach (var note in activeNotes)
                {
                    if (voiceIndex >= maxVoices) break;

                    _voices[voiceIndex].SetNote(note.frequency, note.amplitude, note.waveform);
                    sample += _voices[voiceIndex].GetSample(_sampleTime);
                    voiceIndex++;
                }

                // Mute unused voices
                for (int v = voiceIndex; v < maxVoices; v++)
                {
                    _voices[v].SetNote(0f, 0f, Waveform.Sine);
                }

                sample *= masterVolume;

                // Apply to all channels
                for (int c = 0; c < channels; c++)
                {
                    data[i + c] = sample;
                }

                _sampleTime += 1.0 / sampleRate;
            }
        }

        /// <summary>
        /// Start playing procedural music.
        /// </summary>
        public void Play()
        {
            _isPlaying = true;
            audioSource.Play();
        }

        /// <summary>
        /// Stop music.
        /// </summary>
        public void Stop()
        {
            _isPlaying = false;
            audioSource.Stop();
        }

        /// <summary>
        /// Set music intensity (0-1). Affects BPM and arrangement complexity.
        /// </summary>
        public void SetIntensity(float intensity)
        {
            _intensity = Mathf.Clamp01(intensity);
            _targetBPM = Mathf.Lerp(baseBPM, maxBPM, _intensity);
        }

        /// <summary>
        /// Set BPM directly.
        /// </summary>
        public void SetBPM(float bpm)
        {
            _targetBPM = Mathf.Clamp(bpm, baseBPM, maxBPM);
        }

        private void InitializeAudio()
        {
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 0f; // 2D music

            // Create silent clip to drive OnAudioFilterRead
            AudioClip clip = AudioClip.Create("ProceduralMusic", sampleRate, 2, sampleRate, true);
            audioSource.clip = clip;
        }
    }

    // ── Synth Voice ──

    public enum Waveform { Sine, Square, Saw, Triangle, Noise }

    public class SynthVoice
    {
        private readonly int _sampleRate;
        private float _frequency;
        private float _amplitude;
        private Waveform _waveform;
        private double _phase;
        private float _envelope;
        private float _attack = 0.01f;
        private float _release = 0.1f;
        private float _envelopeState;

        public SynthVoice(int sampleRate)
        {
            _sampleRate = sampleRate;
        }

        public void SetNote(float frequency, float amplitude, Waveform waveform)
        {
            if (frequency != _frequency && frequency > 0)
            {
                _phase = 0;
                _envelopeState = 0f;
            }

            _frequency = frequency;
            _amplitude = amplitude;
            _waveform = waveform;
        }

        public float GetSample(double time)
        {
            if (_frequency <= 0 || _amplitude <= 0) return 0f;

            // Update envelope
            float deltaTime = 1f / _sampleRate;
            if (_envelopeState < 1f)
            {
                _envelopeState += deltaTime / _attack;
                _envelopeState = Mathf.Min(_envelopeState, 1f);
            }
            _envelope = _envelopeState * _amplitude;

            // Generate waveform
            float sample = 0f;
            double phaseIncrement = _frequency / _sampleRate;
            _phase += phaseIncrement;

            while (_phase >= 1.0) _phase -= 1.0;

            switch (_waveform)
            {
                case Waveform.Sine:
                    sample = Mathf.Sin((float)(_phase * 2.0 * Mathf.PI));
                    break;
                case Waveform.Square:
                    sample = _phase < 0.5 ? 1f : -1f;
                    break;
                case Waveform.Saw:
                    sample = (float)(_phase * 2.0 - 1.0);
                    break;
                case Waveform.Triangle:
                    sample = _phase < 0.5
                        ? (float)(_phase * 4.0 - 1.0)
                        : (float)((1.0 - _phase) * 4.0 - 1.0);
                    break;
                case Waveform.Noise:
                    sample = Random.Range(-1f, 1f);
                    break;
            }

            // Apply low-pass filter for smoother sound
            sample = ApplyLowPass(sample, _frequency > 500f ? 0.3f : 0.1f);

            return sample * _envelope;
        }

        private float _lastSample;
        private float ApplyLowPass(float input, float factor)
        {
            _lastSample = Mathf.Lerp(_lastSample, input, factor);
            return _lastSample;
        }
    }

    // ── Sequencer ──

    public class Sequencer
    {
        private readonly float[] _scale;
        private readonly float[] _bassScale;
        private float _beatTimer;
        private int _currentBeat;
        private int _currentBar;

        // Pattern state
        private int[] _melodyPattern;
        private int[] _bassPattern;
        private int[] _drumPattern;

        public Sequencer(float[] scale, float[] bassScale)
        {
            _scale = scale;
            _bassScale = bassScale;
            GenerateNewPattern();
        }

        public void Update(float bpm, float deltaTime, float intensity)
        {
            float beatDuration = 60f / bpm;
            _beatTimer += deltaTime;

            if (_beatTimer >= beatDuration)
            {
                _beatTimer -= beatDuration;
                _currentBeat++;

                if (_currentBeat >= 16) // 16 beats per bar
                {
                    _currentBeat = 0;
                    _currentBar++;

                    // Generate new pattern every 4 bars
                    if (_currentBar % 4 == 0)
                    {
                        GenerateNewPattern();
                    }
                }
            }
        }

        public System.Collections.Generic.List<NoteEvent> GetActiveNotes(double sampleTime, double samplesPerBeat)
        {
            var notes = new System.Collections.Generic.List<NoteEvent>();

            // Bass (every beat)
            int bassNote = _bassPattern[_currentBeat % _bassPattern.Length];
            if (bassNote >= 0)
            {
                notes.Add(new NoteEvent
                {
                    frequency = _bassScale[bassNote % _bassScale.Length],
                    amplitude = 0.5f,
                    waveform = Waveform.Saw
                });
            }

            // Melody (varied rhythm)
            int melodyNote = _melodyPattern[_currentBeat % _melodyPattern.Length];
            if (melodyNote >= 0)
            {
                notes.Add(new NoteEvent
                {
                    frequency = _scale[melodyNote % _scale.Length],
                    amplitude = 0.3f,
                    waveform = Waveform.Square
                });
            }

            // Hi-hat (off-beats)
            if (_currentBeat % 2 == 1)
            {
                notes.Add(new NoteEvent
                {
                    frequency = 8000f,
                    amplitude = 0.1f,
                    waveform = Waveform.Noise
                });
            }

            // Kick (beat 0 and 8)
            if (_currentBeat % 8 == 0)
            {
                notes.Add(new NoteEvent
                {
                    frequency = 60f,
                    amplitude = 0.6f,
                    waveform = Waveform.Sine
                });
            }

            return notes;
        }

        private void GenerateNewPattern()
        {
            _melodyPattern = new int[16];
            _bassPattern = new int[16];
            _drumPattern = new int[16];

            // Generate bass pattern (simple, repetitive)
            for (int i = 0; i < 16; i++)
            {
                _bassPattern[i] = i % 4 == 0 ? Random.Range(0, _bassScale.Length) : -1;
            }

            // Generate melody pattern (more varied)
            for (int i = 0; i < 16; i++)
            {
                _melodyPattern[i] = Random.value > 0.4f ? Random.Range(0, _scale.Length) : -1;
            }

            // Generate drum pattern
            for (int i = 0; i < 16; i++)
            {
                _drumPattern[i] = i % 2 == 0 ? 1 : 0;
            }
        }
    }

    public struct NoteEvent
    {
        public float frequency;
        public float amplitude;
        public Waveform waveform;
    }
}
