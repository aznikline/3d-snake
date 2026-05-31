#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
using NeonSerpent.Level;

namespace NeonSerpent.Editor
{
    /// <summary>
    /// Unity Editor window for creating and editing POLY SERPENT levels.
    /// Supports 3D tile placement, export to JSON, and preview testing.
    /// </summary>
    public class LevelEditorWindow : EditorWindow
    {
        private LevelData _currentLevel;
        private string _levelName = "NewLevel";
        private Vector3 _brushPosition = Vector3.zero;
        private int _selectedTool = 0;
        private bool _isPainting = false;

        private readonly string[] _toolNames = { "Floor", "Wall", "Obstacle", "Player Start", "Food Spawn", "Grapple Point", "Erase" };
        private readonly string[] _toolIcons = { "Grid.Default", "sv_icon_dot4_pix16_gizmo", "sv_icon_dot0_pix16_gizmo", "AvatarSelector", "PreMatSphere", "sv_icon_dot3_pix16_gizmo", "TreeEditor.Trash" };

        [MenuItem("POLY SERPENT/Level Editor")]
        public static void ShowWindow()
        {
            GetWindow<LevelEditorWindow>("Level Editor");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("POLY SERPENT Level Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Level selection/creation
            DrawLevelSection();
            EditorGUILayout.Space();

            if (_currentLevel == null) return;

            // Tools
            DrawToolsSection();
            EditorGUILayout.Space();

            // Level settings
            DrawSettingsSection();
            EditorGUILayout.Space();

            // Actions
            DrawActionsSection();
        }

        private void DrawLevelSection()
        {
            EditorGUILayout.LabelField("Level", EditorStyles.boldLabel);

            _currentLevel = EditorGUILayout.ObjectField("Level Data", _currentLevel, typeof(LevelData), false) as LevelData;

            EditorGUILayout.BeginHorizontal();
            _levelName = EditorGUILayout.TextField("New Level Name", _levelName);
            if (GUILayout.Button("Create New", GUILayout.Width(100)))
            {
                CreateNewLevel();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawToolsSection()
        {
            EditorGUILayout.LabelField("Tools", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i < _toolNames.Length; i++)
            {
                GUIStyle style = new GUIStyle(GUI.skin.button);
                if (_selectedTool == i)
                {
                    style.normal.background = Texture2D.grayTexture;
                }

                if (GUILayout.Button(_toolNames[i], style, GUILayout.Height(40)))
                {
                    _selectedTool = i;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Active Tool: {_toolNames[_selectedTool]}", EditorStyles.helpBox);
        }

        private void DrawSettingsSection()
        {
            EditorGUILayout.LabelField("Level Settings", EditorStyles.boldLabel);

            SerializedObject so = new SerializedObject(_currentLevel);
            so.Update();

            EditorGUILayout.PropertyField(so.FindProperty("displayName"));
            EditorGUILayout.PropertyField(so.FindProperty("description"));
            EditorGUILayout.PropertyField(so.FindProperty("zone"));
            EditorGUILayout.PropertyField(so.FindProperty("zoneOrder"));
            EditorGUILayout.PropertyField(so.FindProperty("targetLength"));
            EditorGUILayout.PropertyField(so.FindProperty("targetTime"));
            EditorGUILayout.PropertyField(so.FindProperty("maxDeaths"));

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Mechanics", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(so.FindProperty("enableWallRun"));
            EditorGUILayout.PropertyField(so.FindProperty("enableSlide"));
            EditorGUILayout.PropertyField(so.FindProperty("enableGrapple"));
            EditorGUILayout.PropertyField(so.FindProperty("enableDash"));

            so.ApplyModifiedProperties();
        }

        private void DrawActionsSection()
        {
            EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Export to JSON", GUILayout.Height(30)))
            {
                ExportToJson();
            }
            if (GUILayout.Button("Import from JSON", GUILayout.Height(30)))
            {
                ImportFromJson();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Test Level", GUILayout.Height(30)))
            {
                TestLevel();
            }
            if (GUILayout.Button("Clear All", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog("Clear Level", "Are you sure you want to clear all placed objects?", "Yes", "No"))
                {
                    ClearLevel();
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (_currentLevel == null) return;

            HandleSceneInput();
            DrawSceneGrid();
        }

        private void HandleSceneInput()
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    _brushPosition = hit.point;
                    PlaceObject(_brushPosition);
                    e.Use();
                }
            }
        }

        private void DrawSceneGrid()
        {
            // Draw a grid at the brush position for visual reference
            Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            Vector3 gridOrigin = new Vector3(
                Mathf.Floor(_brushPosition.x),
                Mathf.Floor(_brushPosition.y),
                Mathf.Floor(_brushPosition.z)
            );

            for (int x = -5; x <= 5; x++)
            {
                Handles.DrawLine(
                    gridOrigin + new Vector3(x, 0, -5),
                    gridOrigin + new Vector3(x, 0, 5)
                );
            }
            for (int z = -5; z <= 5; z++)
            {
                Handles.DrawLine(
                    gridOrigin + new Vector3(-5, 0, z),
                    gridOrigin + new Vector3(5, 0, z)
                );
            }
        }

        private void PlaceObject(Vector3 position)
        {
            GameObject prefab = GetPrefabForTool(_selectedTool);
            if (prefab == null) return;

            if (_selectedTool == 6) // Erase
            {
                // Remove object at position
                Collider[] colliders = Physics.OverlapSphere(position, 0.5f);
                foreach (var col in colliders)
                {
                    if (col.CompareTag("LevelGeometry"))
                    {
                        Undo.DestroyObjectImmediate(col.gameObject);
                    }
                }
            }
            else
            {
                GameObject obj = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                obj.transform.position = SnapToGrid(position);
                obj.transform.SetParent(GetLevelContainer());
                obj.tag = "LevelGeometry";
                Undo.RegisterCreatedObjectUndo(obj, $"Place {_toolNames[_selectedTool]}");
            }
        }

        private GameObject GetPrefabForTool(int tool)
        {
            // These would be configured in project settings or editor prefs
            string[] prefabPaths = {
                "Assets/Prefabs/Environment/FloorTile.prefab",
                "Assets/Prefabs/Environment/WallTile.prefab",
                "Assets/Prefabs/Environment/Obstacle.prefab",
                "Assets/Prefabs/Environment/PlayerStart.prefab",
                "Assets/Prefabs/Environment/FoodSpawn.prefab",
                "Assets/Prefabs/Environment/GrapplePoint.prefab",
                null
            };

            if (tool >= prefabPaths.Length || string.IsNullOrEmpty(prefabPaths[tool]))
                return null;

            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPaths[tool]);
        }

        private Vector3 SnapToGrid(Vector3 position)
        {
            float gridSize = 1f;
            return new Vector3(
                Mathf.Round(position.x / gridSize) * gridSize,
                Mathf.Round(position.y / gridSize) * gridSize,
                Mathf.Round(position.z / gridSize) * gridSize
            );
        }

        private Transform GetLevelContainer()
        {
            GameObject container = GameObject.Find("LevelGeometry");
            if (container == null)
            {
                container = new GameObject("LevelGeometry");
                Undo.RegisterCreatedObjectUndo(container, "Create Level Container");
            }
            return container.transform;
        }

        private void CreateNewLevel()
        {
            string path = $"Assets/LevelData/Campaign/{_levelName}.asset";
            LevelData asset = ScriptableObject.CreateInstance<LevelData>();
            asset.levelId = _levelName.ToLower().Replace(" ", "_");
            asset.displayName = _levelName;

            AssetDatabase.CreateAsset(asset, path);
            AssetDatabase.SaveAssets();

            _currentLevel = asset;
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            Debug.Log($"[LevelEditor] Created new level: {path}");
        }

        private void ExportToJson()
        {
            if (_currentLevel == null) return;

            LevelGeometryData geometry = new LevelGeometryData();

            // Collect all level geometry objects
            Transform container = GetLevelContainer();
            foreach (Transform child in container)
            {
                geometry.objects.Add(new GeometryObject
                {
                    type = child.tag,
                    position = child.position,
                    rotation = child.rotation.eulerAngles,
                    scale = child.localScale
                });
            }

            string json = JsonUtility.ToJson(geometry, true);
            string path = EditorUtility.SaveFilePanel("Export Level", "Assets/LevelData", _currentLevel.levelId, "json");

            if (!string.IsNullOrEmpty(path))
            {
                File.WriteAllText(path, json);
                Debug.Log($"[LevelEditor] Exported to: {path}");
            }
        }

        private void ImportFromJson()
        {
            string path = EditorUtility.OpenFilePanel("Import Level", "Assets/LevelData", "json");
            if (string.IsNullOrEmpty(path)) return;

            string json = File.ReadAllText(path);
            LevelGeometryData geometry = JsonUtility.FromJson<LevelGeometryData>(json);

            if (geometry == null)
            {
                Debug.LogError("[LevelEditor] Failed to parse JSON.");
                return;
            }

            ClearLevel();

            foreach (var obj in geometry.objects)
            {
                // Recreate objects from JSON data
                // This is simplified - actual implementation would map types to prefabs
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = obj.position;
                cube.transform.rotation = Quaternion.Euler(obj.rotation);
                cube.transform.localScale = obj.scale;
                cube.transform.SetParent(GetLevelContainer());
                cube.tag = "LevelGeometry";
            }

            Debug.Log($"[LevelEditor] Imported {geometry.objects.Count} objects.");
        }

        private void TestLevel()
        {
            if (!EditorApplication.isPlaying)
            {
                EditorApplication.EnterPlaymode();
            }

            // The level would be loaded by LevelManager on scene start
            Debug.Log("[LevelEditor] Entering play mode for testing.");
        }

        private void ClearLevel()
        {
            Transform container = GetLevelContainer();
            while (container.childCount > 0)
            {
                Undo.DestroyObjectImmediate(container.GetChild(0).gameObject);
            }
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
    }

    // ── Serializable Geometry Data ──

    [System.Serializable]
    public class LevelGeometryData
    {
        public string version = "1.0";
        public System.Collections.Generic.List<GeometryObject> objects = new System.Collections.Generic.List<GeometryObject>();
    }

    [System.Serializable]
    public class GeometryObject
    {
        public string type;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }
}
#endif
