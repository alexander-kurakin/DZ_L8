using System.IO;
using Assets._Project.Develop.Runtime.Configs.Gameplay.Sectors;
using Assets._Project.Develop.Runtime.Gameplay.Features.SectorsFeature;
using UnityEditor;
using UnityEngine;

namespace Assets._Project.Develop.Editor
{
    public static class SectorFloorTextureGenerator
    {
        private const string SECTOR_GRID_CONFIG_PATH = "Configs/Gameplay/Sectors/SectorGridConfig";
        private const string BASE_TEXTURE_ASSET_PATH = "Assets/_Project/Art/Environment/ice_baby2_spellcore.png";
        private const string OUTPUT_TEXTURE_ASSET_PATH = "Assets/_Project/Art/Environment/ice_baby2_spellcore.png";

        private const float FLOOR_PLANE_LOCAL_SIZE = 10f;
        private const float FLOOR_WORLD_SCALE = 25f;
        private const float GRID_LINE_WIDTH_PIXELS = 2.5f;
        private const float GRID_LINE_BRIGHTNESS_THRESHOLD = 200f;
        private const byte GRID_LINE_RED = 255;
        private const byte GRID_LINE_GREEN = 255;
        private const byte GRID_LINE_BLUE = 255;
        private const byte GRID_LINE_ALPHA = 255;

        [MenuItem("Tools/Sectors/Regenerate Floor Grid Texture")]
        public static void Regenerate()
        {
            SectorGridConfig gridConfig = Resources.Load<SectorGridConfig>(SECTOR_GRID_CONFIG_PATH);

            if (gridConfig == null)
            {
                Debug.LogError($"SectorGridConfig not found at Resources/{SECTOR_GRID_CONFIG_PATH}");
                return;
            }

            Texture2D sourceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(BASE_TEXTURE_ASSET_PATH);

            if (sourceTexture == null)
            {
                Debug.LogError($"Floor texture not found at {BASE_TEXTURE_ASSET_PATH}");
                return;
            }

            TextureImporter textureImporter = GetReadableImporter(BASE_TEXTURE_ASSET_PATH);

            if (textureImporter == null)
                return;

            bool restoreReadable = textureImporter.isReadable == false;
            textureImporter.isReadable = true;
            AssetDatabase.ImportAsset(BASE_TEXTURE_ASSET_PATH, ImportAssetOptions.ForceUpdate);

            sourceTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(BASE_TEXTURE_ASSET_PATH);
            int width = sourceTexture.width;
            int height = sourceTexture.height;
            Color32[] pixels = sourceTexture.GetPixels32();
            Color32[] cleanedPixels = RemoveExistingGridLines(pixels, width, height);
            DrawConfigGrid(cleanedPixels, width, height, gridConfig);

            Texture2D outputTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            outputTexture.SetPixels32(cleanedPixels);
            outputTexture.Apply();

            byte[] pngBytes = outputTexture.EncodeToPNG();
            Object.DestroyImmediate(outputTexture);

            File.WriteAllBytes(OUTPUT_TEXTURE_ASSET_PATH, pngBytes);

            if (restoreReadable)
            {
                textureImporter.isReadable = false;
                AssetDatabase.ImportAsset(BASE_TEXTURE_ASSET_PATH, ImportAssetOptions.ForceUpdate);
            }
            else
            {
                AssetDatabase.ImportAsset(BASE_TEXTURE_ASSET_PATH, ImportAssetOptions.ForceUpdate);
            }

            AssetDatabase.Refresh();
            Debug.Log($"Floor grid texture regenerated from SectorGridConfig: {OUTPUT_TEXTURE_ASSET_PATH}");
        }

        private static TextureImporter GetReadableImporter(string assetPath)
        {
            TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (textureImporter == null)
                Debug.LogError($"TextureImporter not found for {assetPath}");

            return textureImporter;
        }

        private static Color32[] RemoveExistingGridLines(Color32[] pixels, int width, int height)
        {
            Color32[] result = new Color32[pixels.Length];
            bool[] isGridPixel = new bool[pixels.Length];

            for (int pixelIndex = 0; pixelIndex < pixels.Length; pixelIndex++)
            {
                Color32 pixel = pixels[pixelIndex];
                float luminance = pixel.r * 0.299f + pixel.g * 0.587f + pixel.b * 0.114f;
                int channelSpread = Mathf.Max(pixel.r, Mathf.Max(pixel.g, pixel.b))
                    - Mathf.Min(pixel.r, Mathf.Min(pixel.g, pixel.b));
                isGridPixel[pixelIndex] = luminance >= GRID_LINE_BRIGHTNESS_THRESHOLD && channelSpread <= 80;
            }

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int pixelIndex = y * width + x;

                    if (isGridPixel[pixelIndex] == false)
                    {
                        result[pixelIndex] = pixels[pixelIndex];
                        continue;
                    }

                    result[pixelIndex] = SampleNeighborhoodAverage(pixels, isGridPixel, width, height, x, y);
                }
            }

            return result;
        }

        private static Color32 SampleNeighborhoodAverage(
            Color32[] pixels,
            bool[] isGridPixel,
            int width,
            int height,
            int centerX,
            int centerY)
        {
            int sampleCount = 0;
            int totalRed = 0;
            int totalGreen = 0;
            int totalBlue = 0;
            int totalAlpha = 0;

            for (int offsetY = -2; offsetY <= 2; offsetY++)
            {
                for (int offsetX = -2; offsetX <= 2; offsetX++)
                {
                    if (offsetX == 0 && offsetY == 0)
                        continue;

                    int sampleX = centerX + offsetX;
                    int sampleY = centerY + offsetY;

                    if (sampleX < 0 || sampleY < 0 || sampleX >= width || sampleY >= height)
                        continue;

                    int sampleIndex = sampleY * width + sampleX;

                    if (isGridPixel[sampleIndex])
                        continue;

                    Color32 samplePixel = pixels[sampleIndex];
                    totalRed += samplePixel.r;
                    totalGreen += samplePixel.g;
                    totalBlue += samplePixel.b;
                    totalAlpha += samplePixel.a;
                    sampleCount++;
                }
            }

            if (sampleCount == 0)
                return pixels[centerY * width + centerX];

            return new Color32(
                (byte)(totalRed / sampleCount),
                (byte)(totalGreen / sampleCount),
                (byte)(totalBlue / sampleCount),
                (byte)(totalAlpha / sampleCount));
        }

        private static void DrawConfigGrid(Color32[] pixels, int width, int height, SectorGridConfig gridConfig)
        {
            float worldHalfExtent = FLOOR_PLANE_LOCAL_SIZE * 0.5f * FLOOR_WORLD_SCALE;
            float centerX = (width - 1) * 0.5f;
            float centerY = (height - 1) * 0.5f;
            float pixelsPerWorldUnit = (width * 0.5f) / worldHalfExtent;
            float maxGridRadius = gridConfig.SpawnBeltMaxRadius;

            DrawRing(pixels, width, height, centerX, centerY, gridConfig.InnerBeltMaxRadius * pixelsPerWorldUnit);
            DrawRing(pixels, width, height, centerX, centerY, gridConfig.MiddleBeltMaxRadius * pixelsPerWorldUnit);
            DrawRing(pixels, width, height, centerX, centerY, gridConfig.OuterBeltMaxRadius * pixelsPerWorldUnit);
            DrawRing(pixels, width, height, centerX, centerY, gridConfig.SpawnBeltMaxRadius * pixelsPerWorldUnit);

            float fullCircleRadians = Mathf.PI * 2f;
            float sectorWidthRadians = fullCircleRadians / SectorId.SectorsPerRing;

            for (int sectorBoundaryIndex = 0; sectorBoundaryIndex < SectorId.SectorsPerRing; sectorBoundaryIndex++)
            {
                float angleRadians = sectorBoundaryIndex * sectorWidthRadians;
                float directionX = Mathf.Cos(angleRadians);
                float directionZ = Mathf.Sin(angleRadians);
                float endX = centerX + directionX * maxGridRadius * pixelsPerWorldUnit;
                float endY = centerY - directionZ * maxGridRadius * pixelsPerWorldUnit;
                DrawLine(pixels, width, height, centerX, centerY, endX, endY);
            }
        }

        private static void DrawRing(Color32[] pixels, int width, int height, float centerX, float centerY, float radiusPixels)
        {
            int steps = Mathf.CeilToInt(radiusPixels * Mathf.PI * 2f);

            for (int stepIndex = 0; stepIndex < steps; stepIndex++)
            {
                float angleRadians = stepIndex / (float)steps * Mathf.PI * 2f;
                float pointX = centerX + Mathf.Cos(angleRadians) * radiusPixels;
                float pointY = centerY - Mathf.Sin(angleRadians) * radiusPixels;
                StampBrush(pixels, width, height, pointX, pointY);
            }
        }

        private static void DrawLine(
            Color32[] pixels,
            int width,
            int height,
            float startX,
            float startY,
            float endX,
            float endY)
        {
            float deltaX = endX - startX;
            float deltaY = endY - startY;
            float length = Mathf.Sqrt(deltaX * deltaX + deltaY * deltaY);
            int steps = Mathf.Max(1, Mathf.CeilToInt(length));

            for (int stepIndex = 0; stepIndex <= steps; stepIndex++)
            {
                float t = stepIndex / (float)steps;
                float pointX = Mathf.Lerp(startX, endX, t);
                float pointY = Mathf.Lerp(startY, endY, t);
                StampBrush(pixels, width, height, pointX, pointY);
            }
        }

        private static void StampBrush(Color32[] pixels, int width, int height, float centerX, float centerY)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(centerX - GRID_LINE_WIDTH_PIXELS));
            int maxX = Mathf.Min(width - 1, Mathf.CeilToInt(centerX + GRID_LINE_WIDTH_PIXELS));
            int minY = Mathf.Max(0, Mathf.FloorToInt(centerY - GRID_LINE_WIDTH_PIXELS));
            int maxY = Mathf.Min(height - 1, Mathf.CeilToInt(centerY + GRID_LINE_WIDTH_PIXELS));
            float radiusSquared = GRID_LINE_WIDTH_PIXELS * GRID_LINE_WIDTH_PIXELS;
            Color32 lineColor = new Color32(GRID_LINE_RED, GRID_LINE_GREEN, GRID_LINE_BLUE, GRID_LINE_ALPHA);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    float offsetX = x - centerX;
                    float offsetY = y - centerY;

                    if (offsetX * offsetX + offsetY * offsetY > radiusSquared)
                        continue;

                    pixels[y * width + x] = lineColor;
                }
            }
        }
    }
}
