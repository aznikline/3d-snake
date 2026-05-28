#if UNITY_STANDALONE
using UnityEngine;

namespace NeonSerpent.Steam
{
    /// <summary>
    /// Central Steamworks manager. Handles initialization, authentication,
    /// and provides a safe wrapper for all Steam API calls.
    /// </summary>
    public class SteamManager : MonoBehaviour
    {
        public static SteamManager Instance { get; private set; }

        [Header("Steam")]
        [SerializeField] private uint appId = 480; // Default: Spacewar (test app). Replace with your App ID.
        [SerializeField] private bool initializeOnStart = true;

        [Header("Events")]
        public System.Action OnSteamInitialized;
        public System.Action OnSteamShutdown;
        public System.Action<string> OnSteamError;

        public bool IsInitialized => _isInitialized;
        public bool IsOnline => _isOnline;
        public string PlayerName => _playerName;
        public ulong PlayerSteamId => _playerSteamId;

        private bool _isInitialized;
        private bool _isOnline;
        private string _playerName = "Player";
        private ulong _playerSteamId;

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

        private void Start()
        {
            if (initializeOnStart)
            {
                InitializeSteam();
            }
        }

        private void Update()
        {
            if (_isInitialized)
            {
                // Run Steam callbacks
                // SteamAPI.RunCallbacks();
            }
        }

        private void OnDestroy()
        {
            ShutdownSteam();
        }

        private void OnApplicationQuit()
        {
            ShutdownSteam();
        }

        /// <summary>
        /// Initialize Steamworks.
        /// </summary>
        public bool InitializeSteam()
        {
            if (_isInitialized)
            {
                Debug.Log("[SteamManager] Already initialized.");
                return true;
            }

            try
            {
                // TODO: Initialize Steamworks.NET
                // bool success = SteamAPI.Init();
                // if (!success) throw new System.Exception("SteamAPI.Init() failed");

                // Placeholder initialization
                _isInitialized = true;
                _isOnline = true;
                _playerName = "SteamPlayer"; // SteamFriends.GetPersonaName()
                _playerSteamId = 123456789; // SteamUser.GetSteamID().m_SteamID

                Debug.Log($"[SteamManager] Initialized. Player: {_playerName}");
                OnSteamInitialized?.Invoke();

                // Initialize dependent systems
                InitializeDependentSystems();

                return true;
            }
            catch (System.Exception e)
            {
                _isInitialized = false;
                _isOnline = false;
                Debug.LogWarning($"[SteamManager] Steam initialization failed: {e.Message}");
                OnSteamError?.Invoke(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Shutdown Steamworks cleanly.
        /// </summary>
        public void ShutdownSteam()
        {
            if (!_isInitialized) return;

            // TODO: SteamAPI.Shutdown();

            _isInitialized = false;
            _isOnline = false;

            Debug.Log("[SteamManager] Shutdown.");
            OnSteamShutdown?.Invoke();
        }

        /// <summary>
        /// Check if Steam is running (for overlay, etc.).
        /// </summary>
        public bool IsSteamRunning()
        {
            // TODO: return SteamAPI.IsSteamRunning();
            return _isInitialized;
        }

        /// <summary>
        /// Get the Steam overlay URL for the store page.
        /// </summary>
        public void OpenStorePage()
        {
            // TODO: SteamFriends.ActivateGameOverlayToStore(new AppId_t(appId), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
            Application.OpenURL($"https://store.steampowered.com/app/{appId}");
        }

        /// <summary>
        /// Open the Steam overlay to the community hub.
        /// </summary>
        public void OpenCommunityHub()
        {
            // TODO: SteamFriends.ActivateGameOverlayToWebPage($"https://steamcommunity.com/app/{appId}");
            Application.OpenURL($"https://steamcommunity.com/app/{appId}");
        }

        private void InitializeDependentSystems()
        {
            // Initialize LeaderboardManager
            var leaderboardManager = FindObjectOfType<LeaderboardManager>();
            if (leaderboardManager != null)
            {
                // leaderboardManager.Initialize();
            }

            // Initialize AchievementManager
            var achievementManager = FindObjectOfType<AchievementManager>();
            if (achievementManager != null)
            {
                // achievementManager.Initialize();
            }
        }
    }
}
#endif
