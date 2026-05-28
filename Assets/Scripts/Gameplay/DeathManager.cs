using System;
using System.Collections;
using UnityEngine;
using NeonSerpent.Core;
using NeonSerpent.Player;

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

        [Header("References")]
        [SerializeField] private SnakeHeadController snakeController;
        [SerializeField] private VerletSnakeBody snakeBody;
        [SerializeField] private Camera deathCamera;
        [SerializeField] private Transform checkPoint;

        [Header("Audio")]
        [SerializeField] private AudioClip deathSFX;

        public event Action OnDeath;

        private bool _isDead;
        private Vector3 _deathPosition;
        private Vector3 _respawnPosition;

        private void Update()
        {
            if (_isDead) return;
            if (!GameStateManager.Instance.IsPlaying) return;

            CheckDeathConditions();
        }

        private void CheckDeathConditions()
        {
            // Check environment collision (head vs wall)
            if (CheckEnvironmentCollision())
            {
                TriggerDeath();
                return;
            }

            // Check self-collision
            int collisionIndex = snakeBody.CheckSelfCollision();
            if (collisionIndex >= 0)
            {
                TriggerDeath();
            }
        }

        private bool CheckEnvironmentCollision()
        {
            Vector3 headPos = snakeController.HeadPosition;
            float headRadius = GameConstants.NodeRadiusHead;

            // SphereCast from previous position to current to prevent tunneling
            if (Physics.SphereCast(
                headPos - snakeController.HeadForward * headRadius * 2f,
                headRadius,
                snakeController.HeadForward,
                out RaycastHit hit,
                headRadius * 2f,
                LayerMask.GetMask("Environment")))
            {
                return true;
            }

            return false;
        }

        private void TriggerDeath()
        {
            _isDead = true;
            _deathPosition = snakeController.HeadPosition;
            _respawnPosition = checkPoint != null ? checkPoint.position : Vector3.zero;

            GameStateManager.Instance.ChangeState(GameState.Dead);
            OnDeath?.Invoke();

            // Audio
            if (deathSFX != null)
                AudioSource.PlayClipAtPoint(deathSFX, _deathPosition);

            StartCoroutine(DeathSequence());
        }

        private IEnumerator DeathSequence()
        {
            // Bullet time
            Time.timeScale = timeScale;
            GameStateManager.Instance.ChangeState(GameState.DeathReplay);

            // Camera orbit around death point
            if (deathCamera != null)
            {
                deathCamera.transform.position = _deathPosition + Vector3.up * 2f + Vector3.back * 3f;
                deathCamera.transform.LookAt(_deathPosition);

                float elapsed = 0f;
                while (elapsed < replayDuration)
                {
                    deathCamera.transform.RotateAround(_deathPosition, Vector3.up, orbitSpeed * Time.unscaledDeltaTime);
                    elapsed += Time.unscaledDeltaTime;
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSecondsRealtime(replayDuration);
            }

            // Fade out / respawn
            Time.timeScale = 1f;
            yield return new WaitForSecondsRealtime(respawnDelay);

            Respawn();
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
