using UnityEditor;
using UnityEngine;
using NeonSerpent.Core;

namespace NeonSerpent.Editor
{
    public static class PolyMaterialCreator
    {
        [MenuItem("POLY SERPENT/Create Default Poly Materials")]
        public static void CreateMaterials()
        {
            string path = "Assets/Resources/Materials";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Materials");
            }

            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                Debug.LogError("URP Unlit shader not found!");
                return;
            }

            CreateMaterial(path, "PolyDefault", shader, new Color(0.5f, 0.5f, 0.5f));
            CreateMaterial(path, "PolySnake", shader, new Color(0.2f, 0.8f, 0.9f));
            CreateMaterial(path, "PolyFood", shader, new Color(0.9f, 0.8f, 0.2f));
            CreateMaterial(path, "PolyGround", shader, new Color(0.15f, 0.18f, 0.15f));
            CreateMaterial(path, "PolyWall", shader, new Color(0.3f, 0.35f, 0.4f));

            var particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (particleShader != null)
            {
                CreateMaterial(path, "PolyParticle", particleShader, Color.white);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Default poly materials created in Resources/Materials/");
        }

        private static void CreateMaterial(string path, string name, Shader shader, Color color)
        {
            string assetPath = $"{path}/{name}.mat";
            if (AssetDatabase.LoadAssetAtPath<Material>(assetPath) != null) return;

            var mat = new Material(shader);
            mat.color = color;
            AssetDatabase.CreateAsset(mat, assetPath);
        }
    }
}
