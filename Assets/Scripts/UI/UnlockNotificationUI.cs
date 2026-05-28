using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NeonSerpent.Progression;

namespace NeonSerpent.UI
{
    /// <summary>
    /// Displays unlock notifications when new content is unlocked.
    /// Shows a sliding panel with item icon, name, and description.
    /// </summary>
    public class UnlockNotificationUI : MonoBehaviour
    {
        [Header("Animation")]
        [SerializeField] private float slideInDuration = 0.5f;
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float slideOutDuration = 0.3f;
        [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("UI Elements")]
        [SerializeField] private RectTransform notificationPanel;
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private TextMeshProUGUI unlockLabelText;

        [Header("Audio")]
        [SerializeField] private AudioClip unlockSFX;

        private Vector2 _hiddenPosition;
        private Vector2 _visiblePosition;
        private bool _isShowing;
        private Coroutine _currentAnimation;

        private void Awake()
        {
            if (notificationPanel != null)
            {
                _visiblePosition = notificationPanel.anchoredPosition;
                _hiddenPosition = _visiblePosition + new Vector2(0f, notificationPanel.sizeDelta.y + 50f);
                notificationPanel.anchoredPosition = _hiddenPosition;
            }
        }

        private void Start()
        {
            // Subscribe to unlock events
            var unlockSystem = FindObjectOfType<UnlockSystem>();
            if (unlockSystem != null)
            {
                unlockSystem.OnItemUnlocked += ShowUnlockNotification;
            }
        }

        private void OnDestroy()
        {
            var unlockSystem = FindObjectOfType<UnlockSystem>();
            if (unlockSystem != null)
            {
                unlockSystem.OnItemUnlocked -= ShowUnlockNotification;
            }
        }

        /// <summary>
        /// Show unlock notification for an item.
        /// </summary>
        public void ShowUnlockNotification(UnlockableItem item)
        {
            if (item == null) return;

            // Cancel current animation if showing
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
            }

            _currentAnimation = StartCoroutine(AnimateNotification(item));
        }

        private IEnumerator AnimateNotification(UnlockableItem item)
        {
            _isShowing = true;

            // Update UI
            if (itemIcon != null)
                itemIcon.sprite = item.icon;
            if (itemNameText != null)
                itemNameText.text = item.displayName;
            if (itemDescriptionText != null)
                itemDescriptionText.text = item.description;

            // Play sound
            if (unlockSFX != null)
                AudioSource.PlayClipAtPoint(unlockSFX, Camera.main.transform.position);

            // Slide in
            yield return StartCoroutine(SlidePanel(_hiddenPosition, _visiblePosition, slideInDuration));

            // Wait
            yield return new WaitForSeconds(displayDuration);

            // Slide out
            yield return StartCoroutine(SlidePanel(_visiblePosition, _hiddenPosition, slideOutDuration));

            _isShowing = false;
            _currentAnimation = null;
        }

        private IEnumerator SlidePanel(Vector2 from, Vector2 to, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = slideCurve.Evaluate(Mathf.Clamp01(elapsed / duration));

                notificationPanel.anchoredPosition = Vector2.Lerp(from, to, t);
                yield return null;
            }

            notificationPanel.anchoredPosition = to;
        }

        /// <summary>
        /// Force hide the notification immediately.
        /// </summary>
        public void HideNotification()
        {
            if (_currentAnimation != null)
            {
                StopCoroutine(_currentAnimation);
                _currentAnimation = null;
            }

            if (notificationPanel != null)
                notificationPanel.anchoredPosition = _hiddenPosition;

            _isShowing = false;
        }
    }
}
