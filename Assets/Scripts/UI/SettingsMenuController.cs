using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonSerpent.Progression;

namespace NeonSerpent.UI
{
    /// <summary>
    /// Settings menu with audio, graphics, and control options.
    /// Reads from and writes to SaveSystem.GameSettings.
    /// </summary>
    public class SettingsMenuController : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private TextMeshProUGUI masterVolumeValue;
        [SerializeField] private TextMeshProUGUI musicVolumeValue;
        [SerializeField] private TextMeshProUGUI sfxVolumeValue;

        [Header("Graphics")]
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private TMP_Dropdown frameRateDropdown;
        [SerializeField] private TMP_Dropdown qualityDropdown;

        [Header("Controls")]
        [SerializeField] private Slider mouseSensitivitySlider;
        [SerializeField] private TextMeshProUGUI mouseSensitivityValue;
        [SerializeField] private Toggle showTutorialToggle;

        [Header("Buttons")]
        [SerializeField] private Button applyButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button backButton;

        private GameSettings _currentSettings;

        private void Start()
        {
            WireButtons();
            WireUIListeners();
            LoadCurrentSettings();
        }

        private void WireButtons()
        {
            if (applyButton != null)
                applyButton.onClick.AddListener(ApplySettings);
            if (resetButton != null)
                resetButton.onClick.AddListener(ResetToDefault);
            if (backButton != null)
                backButton.onClick.AddListener(OnBack);
        }

        private void WireUIListeners()
        {
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(v => UpdateVolumeLabel(v, masterVolumeValue));
            if (musicVolumeSlider != null)
                musicVolumeSlider.onValueChanged.AddListener(v => UpdateVolumeLabel(v, musicVolumeValue));
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.onValueChanged.AddListener(v => UpdateVolumeLabel(v, sfxVolumeValue));
            if (mouseSensitivitySlider != null)
                mouseSensitivitySlider.onValueChanged.AddListener(v => UpdateSensitivityLabel(v));
        }

        /// <summary>
        /// Load settings from SaveSystem and update UI.
        /// </summary>
        public void LoadCurrentSettings()
        {
            var saveSystem = SaveSystem.Instance;
            _currentSettings = saveSystem?.CurrentData?.settings ?? new GameSettings();

            // Audio
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.value = _currentSettings.masterVolume;
                UpdateVolumeLabel(_currentSettings.masterVolume, masterVolumeValue);
            }
            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.value = _currentSettings.musicVolume;
                UpdateVolumeLabel(_currentSettings.musicVolume, musicVolumeValue);
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.value = _currentSettings.sfxVolume;
                UpdateVolumeLabel(_currentSettings.sfxVolume, sfxVolumeValue);
            }

            // Graphics
            if (fullscreenToggle != null)
                fullscreenToggle.isOn = _currentSettings.fullscreen;

            if (frameRateDropdown != null)
            {
                int frameRateIndex = _currentSettings.targetFrameRate switch
                {
                    30 => 0,
                    60 => 1,
                    120 => 2,
                    _ => 3 // Unlimited
                };
                frameRateDropdown.value = frameRateIndex;
            }

            if (qualityDropdown != null)
            {
                qualityDropdown.value = QualitySettings.GetQualityLevel();
            }

            // Controls
            if (mouseSensitivitySlider != null)
            {
                mouseSensitivitySlider.value = _currentSettings.mouseSensitivity;
                UpdateSensitivityLabel(_currentSettings.mouseSensitivity);
            }

            if (showTutorialToggle != null)
                showTutorialToggle.isOn = _currentSettings.showTutorial;
        }

        /// <summary>
        /// Apply current UI values to settings and save.
        /// </summary>
        public void ApplySettings()
        {
            if (_currentSettings == null) _currentSettings = new GameSettings();

            // Audio
            if (masterVolumeSlider != null)
                _currentSettings.masterVolume = masterVolumeSlider.value;
            if (musicVolumeSlider != null)
                _currentSettings.musicVolume = musicVolumeSlider.value;
            if (sfxVolumeSlider != null)
                _currentSettings.sfxVolume = sfxVolumeSlider.value;

            // Graphics
            if (fullscreenToggle != null)
                _currentSettings.fullscreen = fullscreenToggle.isOn;

            if (frameRateDropdown != null)
            {
                _currentSettings.targetFrameRate = frameRateDropdown.value switch
                {
                    0 => 30,
                    1 => 60,
                    2 => 120,
                    _ => -1 // Unlimited
                };
            }

            if (qualityDropdown != null)
            {
                QualitySettings.SetQualityLevel(qualityDropdown.value, true);
            }

            // Controls
            if (mouseSensitivitySlider != null)
                _currentSettings.mouseSensitivity = mouseSensitivitySlider.value;
            if (showTutorialToggle != null)
                _currentSettings.showTutorial = showTutorialToggle.isOn;

            // Apply immediately
            ApplyToEngine();

            // Save
            var saveSystem = SaveSystem.Instance;
            if (saveSystem != null && saveSystem.CurrentData != null)
            {
                saveSystem.CurrentData.settings = _currentSettings;
                saveSystem.SaveProgress();
            }

            Debug.Log("[SettingsMenu] Settings applied and saved.");
        }

        /// <summary>
        /// Reset all settings to default values.
        /// </summary>
        public void ResetToDefault()
        {
            _currentSettings = new GameSettings();

            if (masterVolumeSlider != null) masterVolumeSlider.value = _currentSettings.masterVolume;
            if (musicVolumeSlider != null) musicVolumeSlider.value = _currentSettings.musicVolume;
            if (sfxVolumeSlider != null) sfxVolumeSlider.value = _currentSettings.sfxVolume;
            if (fullscreenToggle != null) fullscreenToggle.isOn = _currentSettings.fullscreen;
            if (frameRateDropdown != null) frameRateDropdown.value = 1; // 60fps
            if (mouseSensitivitySlider != null) mouseSensitivitySlider.value = _currentSettings.mouseSensitivity;
            if (showTutorialToggle != null) showTutorialToggle.isOn = _currentSettings.showTutorial;

            ApplyToEngine();

            var saveSystem = SaveSystem.Instance;
            if (saveSystem != null && saveSystem.CurrentData != null)
            {
                saveSystem.CurrentData.settings = _currentSettings;
                saveSystem.SaveProgress();
            }
        }

        /// <summary>
        /// Apply settings directly to Unity engine.
        /// </summary>
        private void ApplyToEngine()
        {
            if (_currentSettings == null) return;

            // Audio
            AudioListener.volume = _currentSettings.masterVolume;

            // Graphics
            Screen.fullScreen = _currentSettings.fullscreen;
            Application.targetFrameRate = _currentSettings.targetFrameRate;

            // Update mouse sensitivity on player controller
            var playerController = FindObjectOfType<Player.SnakeHeadController>();
            if (playerController != null)
            {
                // SnakeHeadController would need a public method to set sensitivity
                // For now, this is prepared for when that method exists
            }
        }

        private void UpdateVolumeLabel(float value, TextMeshProUGUI label)
        {
            if (label != null)
                label.text = $"{Mathf.RoundToInt(value * 100f)}%";
        }

        private void UpdateSensitivityLabel(float value)
        {
            if (mouseSensitivityValue != null)
                mouseSensitivityValue.text = $"{value:F2}x";
        }

        private void OnBack()
        {
            // Notify parent to close settings and return to previous panel
            // This is handled by the caller (PauseMenuController or MainMenuController)
            gameObject.SetActive(false);
        }
    }
}
