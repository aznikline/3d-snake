using System;
using System.Collections;
using UnityEngine;
using NeonSerpent.Core;
using NeonSerpent.Player;
using NeonSerpent.UI;
using NeonSerpent.Procedural.Audio;

namespace NeonSerpent.Gameplay
{
    /// <summary>
    /// Handles death detection, bullet-time replay, and respawn logic.
    /// </summary>
    public class DeathManager : MonoBehaviour
    {
        [Header("Death Replay")]
        [SerializeField] private float timeScale = GameConstants.DeathTimeScale;
        [SerializeField] private float replayDuration = GameConstants.DeathReplayDuration;
        [SerializeField] private float respawnDelay = GameConstants.RespawnDelay;
        [SerializeField] private float orbitSpeed = 90f;

        [Header("Death Effects")]
        [SerializeField] private Color deathFlashColor = new Color(1f, 0f, 0f, 0.6f);
        [SerializeField] private float deathFlashDuration = 0.5f;

        [Header("References")]
        public SnakeHeadController snakeController;
        public VerletSnakeBody snakeBody;
        public ComboSystem comboSystem;
        [SerializeField] private Camera deathCamera;
        [SerializeField] private Transform checkPoint;

        [Header("Audio")]
        [SerializeField] private AudioClip deathSFX;

        public event Action OnDeath;

        private bool _isDead;
        private Vector3 _deathPosition;
        private Vector3 _respawnPosition;
        private float _graceTimer = 3f;

        private void Awake()
        {
            _graceTimer = 3f;
        }

        public void ResetGracePeriod(float seconds = 3f)
        {
            _graceTimer = seconds;
            _isDead = false;
        }

        private void Update()
        {
            if (_isDead) return;
            if (!GameStateManager.Instance.IsPlaying) return;

            if (_graceTimer > 0f)
            {
                _graceTimer -= Time.deltaTime;
                if (_graceTimer <= 0f)
                {
                    Debug.Log("[DeathManager] Grace period expired, death detection enabled");
                }
                return;
            }

            CheckDeathConditions();
        }

        private void CheckDeathConditions()
        {
            if (CheckEnvironmentCollision())
            {
                Debug.Log("[DeathManager] Death triggered by environment collision");
                if (TryPreventDeath())
                    return;

                TriggerDeath();
                return;
            }

            int collisionIndex = snakeBody.CheckSelfCollision();
            if (collisionIndex >= 0)
            {
                Debug.Log($"[DeathManager] Death triggered by self-collision at segment {collisionIndex}");
                if (TryPreventDeath())
                    return;

                TriggerDeath();
            }
        }

        private bool TryPreventDeath()
        {
            if (comboSystem == null || !comboSystem.TryPreventDeath())
                return false;

            ResetGracePeriod(GameConstants.DeathPreventedInvincibility);
            return true;
        }

        private bool CheckEnvironmentCollision()
        {
            Vector3 headPos = snakeController.HeadPosition;
            float headRadius = GameConstants.NodeRadiusHead;

            if (Physics.SphereCast(
                headPos - snakeController.HeadForward * headRadius * 2f,
                headRadius,
                snakeController.HeadForward,
                out RaycastHit hit,
                headRadius * 2f,
                LayerMask.GetMask("Environment")))
            {
                Debug.Log($"[DeathManager] Environment collision at {headPos}, hit: {hit.collider.gameObject.name} at {hit.point}");
                return true;
            }

            return false;
        }

        private void TriggerDeath()
        {
            if (_isDead) return;

            _isDead = true;
            _deathPosition = snakeController.HeadPosition;
            _respawnPosition = checkPoint != null ? checkPoint.position : Vector3.zero;

            Debug.Log($"[DeathManager] TriggerDeath at position {_deathPosition}, grace timer was {_graceTimer}");

            GameStateManager.Instance.ChangeState(GameState.Dead);
            OnDeath?.Invoke();

            // Death feedback: camera shake
            var camController = Camera.main?.GetComponent<CameraController>();
            camController?.TriggerImpactShake(0.5f);

            // Death feedback: red vignette
            var hud = FindObjectOfType<HUDController>();
            hud?.TriggerDamageFlash();

            // Death feedback: red screen flash
            hud?.TriggerCollectionFlash(deathFlashColor, deathFlashDuration);

            // Audio
            if (deathSFX != null)
                AudioSource.PlayClipAtPoint(deathSFX, _deathPosition);
            else
                ProceduralSFXSystem.Instance?.PlayDeath(_deathPosition);

            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            // Bullet time
            Time.timeScale = timeScale;
            GameStateManager.Instance.ChangeState(GameState.DeathReplay);

            // Show "YOU DIED" overlay
            var deathOverlay = CreateDeathOverlay();
            var deathText = deathOverlay.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (deathText != null)
            {
                deathText.alpha = 0f;
                float fadeElapsed = 0f;
                while (fadeElapsed < 0.4f)
                {
                    fadeElapsed += Time.unscaledDeltaTime;
                    deathText.alpha = fadeElapsed / 0.4f;
                    yield return null;
                }
            }

            // Camera orbit around death point
            Camera orbitCam = deathCamera;
            if (orbitCam == null)
                orbitCam = Camera.main;

            if (orbitCam != null)
            {
                Vector3 originalPos = orbitCam.transform.position;
                Quaternion originalRot = orbitCam.transform.rotation;

                orbitCam.transform.position = _deathPosition + Vector3.up * 2f + Vector3.back * 3f;
                orbitCam.transform.LookAt(_deathPosition);

                float elapsed = 0f;
                while (elapsed < replayDuration)
                {
                    orbitCam.transform.RotateAround(_deathPosition, Vector3.up, orbitSpeed * Time.unscaledDeltaTime);
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }

                // Restore camera position
                orbitCam.transform.position = originalPos;
                orbitCam.transform.rotation = originalRot;
            }
            else
            {
                yield return new WaitForSecondsRealtime(replayDuration);
            }

            // Fade out overlay
            if (deathText != null)
            {
                float fadeElapsed = 0f;
                while (fadeElapsed < 0.3f)
                {
                    fadeElapsed += Time.unscaledDeltaTime;
                    deathText.alpha = 1f - fadeElapsed / 0.3f;
                    yield return null;
                }
            }
            Destroy(deathOverlay);

            // Fade out / respawn
            Time.timeScale = 1f;
            yield return new WaitForSecondsRealtime(respawnDelay);

            Respawn();
        }

        private GameObject CreateDeathOverlay()
        {
            var go = new GameObject("DeathOverlay");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 999;
            go.AddComponent<UnityEngine.UI.CanvasScaler>();
            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var textGO = new GameObject("DeathText");
            textGO.transform.SetParent(go.transform);
            var rect = textGO.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.2f, 0.35f);
            rect.anchorMax = new Vector2(0.8f, 0.65f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var text = textGO.AddComponent<TMPro.TextMeshProUGUI>();
            text.text = "YOU DIED";
            text.fontSize = 72f;
            text.fontStyle = TMPro.FontStyles.Bold;
            text.alignment = TMPro.TextAlignmentOptions.Center;
            text.color = new Color(1f, 0.15f, 0.1f, 1f);
            text.font = TMPro.TMP_Settings.defaultFontAsset;

            return go;
        }

        private void Respawn()
        {
            _isDead = false;

            // Reset snake position
            snakeController.transform.position = _respawnPosition;
            snakeBody.ResetBody(_respawnPosition);

            GameStateManager.Instance.ChangeState(GameState.Playing);
        }

        /// <summary>
        /// Set a new checkpoint for respawn.
        /// </summary>
        public void SetCheckpoint(Vector3 position)
        {
            _respawnPosition = position;
            if (checkPoint != null)
                checkPoint.position = position;
        }

        /// <summary>
        /// Force immediate death (for testing or hazards).
        /// </summary>
        public void ForceDeath()
        {
            if (!_isDead)
                TriggerDeath();
        }
    }
}
