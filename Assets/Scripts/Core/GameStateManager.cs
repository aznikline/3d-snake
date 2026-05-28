using System;
using UnityEngine;

namespace NeonSerpent.Core
{
    /// <summary>
    /// Central state machine for the game. All systems subscribe to
    /// OnStateChanged rather than polling state every frame.
    /// </summary>
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.None;
        public GameMode CurrentMode { get; private set; } = GameMode.Campaign;

        /// <summary>
        /// Fired whenever the game state changes. New state is passed as argument.
        /// </summary>
        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Transition to a new game state. No-op if already in target state.
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            var previousState = CurrentState;
            CurrentState = newState;

            Debug.Log($"[GameState] {previousState} -> {newState}");
            OnStateChanged?.Invoke(newState);
        }

        /// <summary>
        /// Set the active game mode. Should be called before starting a session.
        /// </summary>
        public void SetGameMode(GameMode mode)
        {
            CurrentMode = mode;
        }

        /// <summary>
        /// Convenience check: is the game currently in a playable state?
        /// </summary>
        public bool IsPlaying => CurrentState == GameState.Playing;

        /// <summary>
        /// Convenience check: is the game paused or in a menu?
        /// </summary>
        public bool IsPaused => CurrentState == GameState.Paused;
    }
}
