#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Kaninbanker.Editor
{
    /// <summary>
    /// Safe import defaults for assets intentionally placed under Kaninbanker/Imported2D.
    /// Third-party packages outside this folder keep their publisher import settings unchanged.
    /// </summary>
    public sealed class Kaninbanker2DAssetImporter : AssetPostprocessor
    {
        private const string ImportRoot = "Assets/Kaninbanker/Imported2D/";

        private bool IsManagedPath => assetPath.Replace('\\', '/').StartsWith(ImportRoot, System.StringComparison.OrdinalIgnoreCase);

        private void OnPreprocessTexture()
        {
            if (!IsManagedPath)
                return;

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.spritePixelsPerUnit = 100f;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Compressed;

            string lower = assetPath.ToLowerInvariant();
            importer.filterMode = lower.Contains("pixel") || lower.Contains("8bit") || lower.Contains("16bit")
                ? FilterMode.Point
                : FilterMode.Bilinear;

            if (importer.spriteImportMode == SpriteImportMode.None)
                importer.spriteImportMode = SpriteImportMode.Single;
        }

        private void OnPreprocessAudio()
        {
            if (!IsManagedPath)
                return;

            AudioImporter importer = (AudioImporter)assetImporter;
            string lower = assetPath.ToLowerInvariant();
            bool music = lower.Contains("music") || lower.Contains("theme") || lower.Contains("bgm") || lower.Contains("soundtrack");

            AudioImporterSampleSettings settings = importer.defaultSampleSettings;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = music ? 0.72f : 0.82f;
            settings.loadType = music ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
            settings.preloadAudioData = !music;
            importer.defaultSampleSettings = settings;
            importer.loadInBackground = music;
        }
    }
}
#endif
