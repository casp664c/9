#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Kaninbanker.Editor
{
    /// <summary>
    /// Enforces a mobile-friendly TRUE-2D import policy for Kaninbanker production art.
    /// New texture art becomes sprites automatically instead of accidentally entering the
    /// project as generic texture/material input for a 3D workflow.
    /// </summary>
    public sealed class Kaninbanker2DAssetPostprocessor : AssetPostprocessor
    {
        private const string AuthoringRoot = "Assets/Kaninbanker/Art2D/";
        private const string CoreRuntimeRoot = "Assets/Kaninbanker/Resources/KaninbankerArt2D/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(AuthoringRoot) && !assetPath.StartsWith(CoreRuntimeRoot))
                return;

            TextureImporter importer = assetImporter as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.crunchedCompression = false;

            TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings("Android");
            android.name = "Android";
            android.overridden = true;
            android.maxTextureSize = 2048;
            android.format = TextureImporterFormat.ASTC_6x6;
            android.compressionQuality = 70;
            importer.SetPlatformTextureSettings(android);
        }
    }
}
#endif
