using UnityEngine;
using NeonSerpent.Player;
using NeonSerpent.Gameplay;
using NeonSerpent.Level;
using NeonSerpent.Audio;
using NeonSerpent.UI;
using NeonSerpent.Progression;
using NeonSerpent.Procedural.Art.Effects;
using NeonSerpent.Procedural.Art.Environment;
using NeonSerpent.Procedural.Audio;

namespace NeonSerpent.Core
{
    /// <summary>
    /// Runtime bootstrap that creates all game systems and wires them up.
    /// UI construction is delegated to UIFactory.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoBootstrap()
        {
            if (FindObjectOfType<GameBootstrap>() != null) return;
            var go = new GameObject("GameBootstrap");
            go.AddComponent<GameBootstrap>();
        }

        [Header("Game Mode")]
        [SerializeField] private GameMode startMode = GameMode.Campaign;
        [SerializeField] private bool showMainMenu = true;

        [Header("Generation")]
        [SerializeField] private bool generateCity = true;
        [SerializeField] private int cityBlocksX = 8;
        [SerializeField] private int cityBlocksZ = 8;

        [Header("Level")]
        [SerializeField] private int targetLength = 20;
        [SerializeField] private float targetTime = 120f;
        [SerializeField] private int maxDeaths = 3;

        private SnakeHeadController _snakeController;
        private VerletSnakeBody _snakeBody;
        private ComboSystem _comboSystem;
        private FoodSpawner _foodSpawner;
        private DeathManager _deathManager;
        private LevelManager _levelManager;
        private MusicManager _musicManager;
        private ProceduralSFXSystem _sfxSystem;

        private void Awake()
        {
            CreateCoreSystems();
        }

        private void Start()
        {
            if (showMainMenu)
            {
                UIFactory.BuildMainMenu(this);
                GameStateManager.Instance.ChangeState(GameState.MainMenu);
            }
            else
            {
                StartGameplay();
            }
            Debug.Log("[GameBootstrap] Game initialized.");
        }

        // ── Public Entry Points ──

        public void StartGameplay()
        {
            var mainMenuCanvas = GameObject.Find("MainMenuCanvas");
            if (mainMenuCanvas != null)
                mainMenuCanvas.SetActive(false);

            BuildGameWorld();
            BuildPlayer();
            BuildGameplaySystems();
            BuildAudio();
            BuildAllUI();
            BuildTutorial();

            GameStateManager.Instance.SetGameMode(startMode);

            var levelData = CreateDefaultLevelData();
            if (_levelManager != null)
                _levelManager.LoadLevel(levelData);
            else
                GameStateManager.Instance.ChangeState(GameState.Playing);
        }

        public void ReturnToMenu()
        {
            DestroyGameplayObjects();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var mainMenu = FindObjectOfType<MainMenuController>();
            if (mainMenu != null)
                mainMenu.gameObject.SetActive(true);
            else
                UIFactory.BuildMainMenu(this);

            GameStateManager.Instance.ChangeState(GameState.MainMenu);
        }

        public void StartCampaignMode() { startMode = GameMode.Campaign; StartGameplay(); }

        // ── Core Systems ──

        private void CreateCoreSystems()
        {
            if (GameStateManager.Instance == null)
            {
                var gsm = new GameObject("GameStateManager");
                gsm.AddComponent<GameStateManager>();
            }

            if (SaveSystem.Instance == null)
            {
                var save = new GameObject("SaveSystem");
                save.AddComponent<SaveSystem>();
            }

            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                var evt = new GameObject("EventSystem");
                evt.AddComponent<UnityEngine.EventSystems.EventSystem>();
                evt.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
            }
        }

        // ── Game World ──

        private void BuildGameWorld()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(200f, 1f, 200f);
            ground.layer = GameConstants.LayerEnvironment;

            var groundMat = PolyMaterials.CreateUnlit(new Color(0.15f, 0.18f, 0.15f));
            ground.GetComponent<Renderer>().material = groundMat;

            CreateBoundaryWalls();

            if (generateCity)
            {
                var buildingGen = FindOrCreateComponent<ProceduralBuildingGenerator>("BuildingGenerator");
                var cityGen = FindOrCreateComponent<ProceduralCityGenerator>("CityGenerator");

                cityGen.cityBlocksX = cityBlocksX;
                cityGen.cityBlocksZ = cityBlocksZ;
                cityGen.buildingGenerator = buildingGen;
                cityGen.GenerateCity();

                var particleFX = FindOrCreateComponent<ProceduralParticleEffects>("ParticleEffects");
                particleFX.CreateAtmosphericEffects(transform);
            }

            CreateGrapplePoints();
        }

        private void CreateBoundaryWalls()
        {
            float size = 200f;
            float wallHeight = 20f;
            float wallThickness = 1f;

            CreateWall("Wall_North", new Vector3(0f, wallHeight * 0.5f, size * 0.5f),
                new Vector3(size + wallThickness * 2, wallHeight, wallThickness));
            CreateWall("Wall_South", new Vector3(0f, wallHeight * 0.5f, -size * 0.5f),
                new Vector3(size + wallThickness * 2, wallHeight, wallThickness));
            CreateWall("Wall_East", new Vector3(size * 0.5f, wallHeight * 0.5f, 0f),
                new Vector3(wallThickness, wallHeight, size));
            CreateWall("Wall_West", new Vector3(-size * 0.5f, wallHeight * 0.5f, 0f),
                new Vector3(wallThickness, wallHeight, size));
        }

        private void CreateWall(string name, UnityEngine.Vector3 position, UnityEngine.Vector3 scale)
        {
            var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.position = position;
            wall.transform.localScale = scale;
            wall.layer = GameConstants.LayerEnvironment;
            wall.tag = "LevelGeometry";

            var mat = PolyMaterials.CreateUnlit(new Color(0.3f, 0.35f, 0.4f));
            wall.GetComponent<Renderer>().material = mat;
        }

        private void CreateGrapplePoints()
        {
            UnityEngine.Vector3[] positions =
            {
                new UnityEngine.Vector3(18f, 8f, 24f),
                new UnityEngine.Vector3(-22f, 10f, 18f),
                new UnityEngine.Vector3(28f, 12f, -12f),
                new UnityEngine.Vector3(-16f, 9f, -28f),
                new UnityEngine.Vector3(0f, 14f, 34f),
                new UnityEngine.Vector3(34f, 11f, 0f)
            };

            for (int i = 0; i < positions.Length; i++)
            {
                var point = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                point.name = $"GrapplePoint_{i + 1}";
                point.tag = "GrapplePoint";
                point.layer = GameConstants.LayerGrapplePoint;
                point.transform.position = positions[i];
                point.transform.localScale = UnityEngine.Vector3.one * 1.25f;

                var renderer = point.GetComponent<Renderer>();
                renderer.material = PolyMaterials.CreateUnlit(i % 2 == 0
                    ? new Color(0.2f, 0.95f, 1f)
                    : new Color(1f, 0.25f, 0.85f));

                var light = point.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 8f;
                light.intensity = 1.2f;
                light.color = i % 2 == 0 ? Color.cyan : Color.magenta;
            }
        }

        // ── Player ──

        private void BuildPlayer()
        {
            var playerGO = new GameObject("Player");
            playerGO.tag = "SnakeHead";

            var cc = playerGO.AddComponent<CharacterController>();
            cc.height = 1.8f;
            cc.radius = 0.5f;
            cc.center = new UnityEngine.Vector3(0f, 0.9f, 0f);

            var cameraGO = new GameObject("FirstPersonCamera");
            cameraGO.transform.SetParent(playerGO.transform);
            cameraGO.transform.localPosition = new UnityEngine.Vector3(0f, 1.6f, 0f);
            cameraGO.transform.localRotation = UnityEngine.Quaternion.identity;

            var cam = cameraGO.AddComponent<Camera>();
            cam.fieldOfView = GameConstants.CameraFOV;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 1000f;
            cam.tag = "MainCamera";
            cameraGO.AddComponent<AudioListener>();
            cameraGO.AddComponent<CameraController>();

            _snakeController = playerGO.AddComponent<SnakeHeadController>();
            _snakeController.cameraTransform = cameraGO.transform;

            _snakeBody = playerGO.AddComponent<VerletSnakeBody>();
            _snakeController.snakeBody = _snakeBody;

            var bodyRenderer = playerGO.AddComponent<SnakeBodyRenderer>();
            bodyRenderer.snakeBody = _snakeBody;

            var snakeMat = PolyMaterials.CreateUnlit(new Color(0.2f, 0.8f, 0.9f));
            bodyRenderer.snakeMaterial = snakeMat;

            var wallRun = playerGO.AddComponent<WallRunAbility>();
            var slide = playerGO.AddComponent<SlideAbility>();
            var grapple = playerGO.AddComponent<GrappleAbility>();

            wallRun.wallLayer = LayerMask.GetMask("Environment");
            grapple.grappleLayer = LayerMask.GetMask("GrapplePoint");
            _snakeBody.environmentLayer = LayerMask.GetMask("Environment");

            playerGO.transform.position = new UnityEngine.Vector3(0f, 2f, 0f);
        }

        // ── Gameplay Systems ──

        private void BuildGameplaySystems()
        {
            var gameplayGO = new GameObject("GameplaySystems");

            _comboSystem = gameplayGO.AddComponent<ComboSystem>();
            _comboSystem.OnDashStarted += () => _snakeController.SetDashState(true);
            _comboSystem.OnDashEnded += () => _snakeController.SetDashState(false);

            _foodSpawner = gameplayGO.AddComponent<FoodSpawner>();
            _foodSpawner.snakeBody = _snakeBody;
            _foodSpawner.comboSystem = _comboSystem;
            _foodSpawner.foodPrefab = CreateFoodPrefab();

            _deathManager = gameplayGO.AddComponent<DeathManager>();
            _deathManager.snakeController = _snakeController;
            _deathManager.snakeBody = _snakeBody;
            _deathManager.comboSystem = _comboSystem;

            _levelManager = gameplayGO.AddComponent<LevelManager>();
            _levelManager.snakeController = _snakeController;
            _levelManager.snakeBody = _snakeBody;
            _levelManager.comboSystem = _comboSystem;
            _levelManager.foodSpawner = _foodSpawner;
            _levelManager.deathManager = _deathManager;

            _foodSpawner.levelManager = _levelManager;
        }

        private Food CreateFoodPrefab()
        {
            var foodGO = new GameObject("FoodPrefab");
            foodGO.SetActive(false);

            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.SetParent(foodGO.transform);
            sphere.transform.localPosition = UnityEngine.Vector3.zero;
            sphere.transform.localScale = UnityEngine.Vector3.one * 0.5f;

            Destroy(sphere.GetComponent<SphereCollider>());
            var parentCollider = foodGO.AddComponent<SphereCollider>();
            parentCollider.isTrigger = true;
            parentCollider.radius = 0.5f;

            sphere.layer = GameConstants.LayerFood;
            foodGO.layer = GameConstants.LayerFood;

            var rb = foodGO.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;

            var mat = PolyMaterials.CreateUnlit(new Color(0.9f, 0.8f, 0.2f));
            sphere.GetComponent<Renderer>().material = mat;

            return foodGO.AddComponent<Food>();
        }

        // ── Audio ──

        private void BuildAudio()
        {
            var audioGO = new GameObject("AudioSystems");

            _musicManager = audioGO.AddComponent<MusicManager>();
            _levelManager.musicManager = _musicManager;

            _sfxSystem = audioGO.AddComponent<ProceduralSFXSystem>();
            _comboSystem.OnComboChanged += _musicManager.SetIntensity;
            _comboSystem.OnDashStarted += () => _sfxSystem.PlayDash(_snakeController.HeadPosition);
            _comboSystem.OnDashStarted += () => _musicManager.SetIntensity(1f);
            _comboSystem.OnDashEnded += () => _musicManager.SetIntensity(_comboSystem.ComboMeter);
        }

        // ── UI ──

        private void BuildAllUI()
        {
            UIFactory.BuildHUD(_comboSystem, _snakeController, _snakeBody);
            UIFactory.BuildPauseMenu();
            UIFactory.BuildGameOverScreen();
            UIFactory.BuildLevelCompleteScreen();
        }

        private void BuildTutorial()
        {
            UIFactory.BuildTutorial();
        }

        // ── Level Data ──

        private LevelData CreateDefaultLevelData()
        {
            var levelData = ScriptableObject.CreateInstance<LevelData>();
            levelData.levelId = "bootstrap_level_01";
            levelData.displayName = "Poly City Grid";
            levelData.description = "Procedurally generated cyberpunk training ground.";
            levelData.zone = ZoneType.PolyCity;
            levelData.zoneOrder = 1;
            levelData.targetLength = targetLength;
            levelData.targetTime = targetTime;
            levelData.maxDeaths = maxDeaths;
            levelData.unlockedByDefault = true;
            levelData.environmentTheme = EnvironmentTheme.PolyCity;
            levelData.musicTheme = MusicTheme.Ambient;
            levelData.enableWallRun = true;
            levelData.enableSlide = true;
            levelData.enableGrapple = true;
            levelData.enableDash = true;
            return levelData;
        }

        // ── Cleanup ──

        private void DestroyGameplayObjects()
        {
            var names = new[] { "Ground", "Wall_North", "Wall_South", "Wall_East", "Wall_West", "GrapplePoint_1", "GrapplePoint_2", "GrapplePoint_3", "GrapplePoint_4", "GrapplePoint_5", "GrapplePoint_6", "Player", "GameplaySystems", "AudioSystems", "HUDCanvas", "PauseCanvas", "GameOverCanvas", "LevelCompleteCanvas", "TutorialCanvas", "EndlessMode" };
            foreach (var name in names)
            {
                var obj = GameObject.Find(name);
                if (obj != null) Destroy(obj);
            }

            var cityGen = FindObjectOfType<ProceduralCityGenerator>();
            if (cityGen != null) Destroy(cityGen.gameObject);

            _snakeController = null;
            _snakeBody = null;
            _comboSystem = null;
            _foodSpawner = null;
            _deathManager = null;
            _levelManager = null;
            _musicManager = null;
            _sfxSystem = null;
        }

        private T FindOrCreateComponent<T>(string name) where T : Component
        {
            var existing = FindObjectOfType<T>();
            if (existing != null) return existing;
            var go = new GameObject(name);
            return go.AddComponent<T>();
        }
    }
}
