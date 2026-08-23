#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Kaninbanker.Editor
{
    /// <summary>
    /// Enforces a mobile-friendly TRUE-2D import policy for production art placed under
    /// Assets/Kaninbanker/Art2D. New PNG/JPG/PSD/PSB assets become sprites automatically
    /// instead of accidentally entering the project as 3D-oriented textures/material inputs.
    /// </summary>
    public sealed class Kaninbanker2DAssetPostprocessor : AssetPostprocessor
    {
        private const string Root = "Assets/Kaninbanker/Art2D/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Root))
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
