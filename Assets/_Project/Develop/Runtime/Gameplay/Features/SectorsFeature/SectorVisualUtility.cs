using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature
{
    public static class SectorVisualUtility
    {
        public static Material CreateTransparentFillMaterial(Color rgb, float alpha)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

            if (shader == null)
                shader = Shader.Find("Unlit/Color");

            if (shader == null)
                shader = Shader.Find("Standard");

            Material material = new Material(shader);
            ApplyTransparentColor(material, rgb, alpha);

            return material;
        }

        public static void ApplyTransparentColor(Material material, Color rgb, float alpha)
        {
            Color color = rgb;
            color.a = alpha;
            material.color = color;
            ConfigureTransparentMaterial(material);
        }

        public static void ConfigureOutline(LineRenderer lineRenderer, SectorVisualConfig visualConfig)
        {
            if (lineRenderer == null || visualConfig == null)
                return;

            ConfigureOutline(lineRenderer, visualConfig.OutlineColor, visualConfig.OutlineWidth);
        }

        public static void ConfigureOutline(LineRenderer lineRenderer, Color color, float width)
        {
            if (lineRenderer == null)
                return;

            lineRenderer.useWorldSpace = false;
            lineRenderer.loop = true;
            lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
            lineRenderer.receiveShadows = false;
            lineRenderer.alignment = LineAlignment.View;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.material = CreateOutlineMaterial(color);
        }

        private static Material CreateOutlineMaterial(Color color)
        {
            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
                shader = Shader.Find("Unlit/Color");

            Material material = new Material(shader);
            material.color = color;

            return material;
        }

        private static void ConfigureTransparentMaterial(Material material)
        {
            if (material.HasProperty("_Surface") == false)
                return;

            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.renderQueue = (int)RenderQueue.Transparent;
        }
    }
}
