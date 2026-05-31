using UnityEngine;

namespace NeonSerpent.Core
{
    public static class PolyMaterials
    {
        private static Shader _unlitShader;
        private static Shader _particleUnlitShader;

        public static Shader UnlitShader
        {
            get
            {
                if (_unlitShader != null) return _unlitShader;

                _unlitShader = Shader.Find("Universal Render Pipeline/Unlit");
                if (_unlitShader == null)
                    _unlitShader = Shader.Find("Unlit/Color");
                if (_unlitShader == null)
                    _unlitShader = Shader.Find("Hidden/InternalErrorShader");

                return _unlitShader;
            }
        }

        public static Shader ParticleUnlitShader
        {
            get
            {
                if (_particleUnlitShader != null) return _particleUnlitShader;

                _particleUnlitShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                if (_particleUnlitShader == null)
                    _particleUnlitShader = UnlitShader;

                return _particleUnlitShader;
            }
        }

        public static Material CreateUnlit(Color color)
        {
            var shader = UnlitShader;
            if (shader == null) return null;

            var mat = new Material(shader);
            mat.color = color;
            return mat;
        }

        public static Material CreateParticleUnlit(Color color)
        {
            var shader = ParticleUnlitShader;
            if (shader == null) return null;

            var mat = new Material(shader);
            mat.SetColor("_BaseColor", color);
            return mat;
        }
    }
}
