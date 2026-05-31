using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonSerpent.Core;
using NeonSerpent.Progression;

namespace NeonSerpent.UI
{
    /// <summary>
    /// First-time player tutorial. Shows contextual tips for movement,
    /// eating, combo, dash, and abilities. Can be skipped at any time.
    /// </summary>
    public class TutorialController : MonoBehaviour
    {
        [Header("Tutorial Steps")]
        public TutorialStep[] steps;

        [Header("UI")]
        public GameObject tutorialPanel;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI instructionText;
        [SerializeField] private TextMeshProUGUI hintText;
        [SerializeField] private Image hintImage;
        public Button skipButton;
        public Button continueButton;
        [SerializeField] private Slider progressSlider;

        [Header("Timing")]
        [SerializeField] private float autoAdvanceDelay = 5f;
        [SerializeField] private float fadeDuration = 0.3f;

        private int _currentStepIndex = -1;
        private bool _isTutorialActive;
        private Coroutine _autoAdvanceCoroutine;

        [System.Serializable]
        public struct TutorialStep
        {
            public string title;
            [TextArea(2, 4)]
            public string instruction;
            [TextArea(1, 2)]
            public string hint;
            public KeyCode waitForKey;
            public bool waitForAction;
            public float displayDuration;
        }

        private void Start()
        {
            if (skipButton != null)
                skipButton.onClick.AddListener(SkipTutorial);
            if (continueButton != null)
                continueButton.onClick.AddListener(AdvanceStep);

            // Check if tutorial should show
            CheckShowTutorial();
        }

        private void Update()
        {
            if (!_isTutorialActive) return;
            if (_currentStepIndex < 0 || _currentStepIndex >= steps.Length) return;

            var step = steps[_currentStepIndex];

            // Check for key press to advance
            if (step.waitForKey != KeyCode.None && Input.GetKeyDown(step.waitForKey))
            {
                AdvanceStep();
                return;
            }

            // Check for action-based advancement (simplified)
            if (step.waitForAction)
            {
                if (CheckActionComplete(step))
                {
                    AdvanceStep();
                }
            }
        }

        /// <summary>
        /// Check if tutorial should be shown based on save data.
        /// </summary>
        public void CheckShowTutorial()
        {
            var saveSystem = SaveSystem.Instance;
            bool showTutorial = saveSystem?.CurrentData?.settings?.showTutorial ?? true;

            if (showTutorial && steps.Length > 0)
            {
                StartTutorial();
            }
            else
            {
                HideTutorial();
            }
        }

        /// <summary>
        /// Start the tutorial sequence.
        /// </summary>
        public void StartTutorial()
        {
            _isTutorialActive = true;
            _currentStepIndex = -1;

            if (tutorialPanel != null)
                tutorialPanel.SetActive(true);

            AdvanceStep();
        }

        /// <summary>
        /// Advance to the next tutorial step.
        /// </summary>
        public void AdvanceStep()
        {
            if (_autoAdvanceCoroutine != null)
            {
                StopCoroutine(_autoAdvanceCoroutine);
                _autoAdvanceCoroutine = null;
            }

            _currentStepIndex++;

            if (_currentStepIndex >= steps.Length)
            {
                CompleteTutorial();
                return;
            }

            ShowStep(_currentStepIndex);
        }

        /// <summary>
        /// Skip the entire tutorial.
        /// </summary>
        public void SkipTutorial()
        {
            if (_autoAdvanceCoroutine != null)
            {
                StopCoroutine(_autoAdvanceCoroutine);
                _autoAdvanceCoroutine = null;
            }

            CompleteTutorial();
        }

        private void ShowStep(int index)
        {
            if (index < 0 || index >= steps.Length) return;

            var step = steps[index];

            if (titleText != null)
                titleText.text = step.title;
            if (instructionText != null)
                instructionText.text = step.instruction;
            if (hintText != null)
                hintText.text = step.hint;

            if (progressSlider != null)
                progressSlider.value = (float)(index + 1) / steps.Length;

            // Show/hide continue button based on whether we wait for input
            if (continueButton != null)
            {
                bool showContinue = step.waitForKey == KeyCode.None && !step.waitForAction;
                continueButton.gameObject.SetActive(showContinue);
            }

            // Auto-advance if duration is set
            float duration = step.displayDuration > 0f ? step.displayDuration : autoAdvanceDelay;
            if (step.waitForKey == KeyCode.None && !step.waitForAction)
            {
                _autoAdvanceCoroutine = StartCoroutine(AutoAdvance(duration));
            }

            Debug.Log($"[Tutorial] Step {index + 1}/{steps.Length}: {step.title}");
        }

        private IEnumerator AutoAdvance(float delay)
        {
            yield return new WaitForSeconds(delay);
            AdvanceStep();
        }

        private bool CheckActionComplete(TutorialStep step)
        {
            // Simplified action checks - can be expanded based on gameplay events
            // For now, we use time-based or key-based advancement primarily
            return false;
        }

        private void CompleteTutorial()
        {
            _isTutorialActive = false;
            _currentStepIndex = -1;

            // Mark tutorial as completed
            var saveSystem = SaveSystem.Instance;
            if (saveSystem != null && saveSystem.CurrentData != null)
            {
                if (saveSystem.CurrentData.settings == null)
                    saveSystem.CurrentData.settings = new GameSettings();

                saveSystem.CurrentData.settings.showTutorial = false;
                saveSystem.SaveProgress();
            }

            HideTutorial();
            Debug.Log("[Tutorial] Tutorial completed.");
        }

        private void HideTutorial()
        {
            if (tutorialPanel != null)
                tutorialPanel.SetActive(false);
        }

        /// <summary>
        /// Reset tutorial flag so it shows again next time.
        /// </summary>
        public void ResetTutorialFlag()
        {
            var saveSystem = SaveSystem.Instance;
            if (saveSystem != null && saveSystem.CurrentData != null)
            {
                if (saveSystem.CurrentData.settings == null)
                    saveSystem.CurrentData.settings = new GameSettings();

                saveSystem.CurrentData.settings.showTutorial = true;
                saveSystem.SaveProgress();
            }
        }
    }
}
