#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Kaninbanker.Editor
{
    public sealed class KaninbankerPreflightValidator : IPreprocessBuildWithReport
    {
        private const string ScenePath = "Assets/Kaninbanker/Scenes/Main.unity";
        private const string ManifestPath = "Packages/manifest.json";
        private const string RuntimeDirectory = "Assets/Kaninbanker/Scripts";
        private const string EditorDirectory = "Assets/Kaninbanker/Editor";
        private const string True2DGamePath = RuntimeDirectory + "/KaninbankerGame2D.cs";

        public int callbackOrder => 1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            ValidatePortrait();
            ValidateEditor2DMode();
            ValidateBuildScene();
            ValidateRequiredSourceAndAssetPipeline();
            ValidateModulesAnd2DToolset();
            ValidateTrue2DSource();
            ValidateEntireRuntimeIs2D();
            ValidateAssetCatalogPipeline();
            ValidateVersion();
            Debug.Log("[Kaninbanker] PREFLIGHT PASS: FULL APP TRUE 2D ONLY + OFFICIAL UNITY 2D TOOLSET + IMPORTED ASSET CATALOG, portrait, version 0.10.0.");
        }

        private static void ValidatePortrait()
        {
            bool valid = PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait &&
                         PlayerSettings.allowedAutorotateToPortrait &&
                         !PlayerSettings.allowedAutorotateToPortraitUpsideDown &&
                         !PlayerSettings.allowedAutorotateToLandscapeLeft &&
                         !PlayerSettings.allowedAutorotateToLandscapeRight;
            if (!valid)
                throw new BuildFailedException("KANINBANKER PREFLIGHT: Android orientation is not hard-locked to portrait.");
        }

        private static void ValidateEditor2DMode()
        {
            if (EditorSettings.defaultBehaviorMode != EditorBehaviorMode.Mode2D)
                throw new BuildFailedException("KANINBANKER PREFLIGHT: Unity Default Behavior Mode is not 2D.");
        }

        private static void ValidateBuildScene()
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            if (scenes == null || scenes.Length == 0 || !scenes[0].enabled || scenes[0].path != ScenePath)
                throw new BuildFailedException("KANINBANKER PREFLIGHT: Main.unity is missing from Build Settings.");
            if (!File.Exists(ScenePath))
                throw new BuildFailedException("KANINBANKER PREFLIGHT: generated Main.unity does not exist on disk.");
        }

        private static void ValidateRequiredSourceAndAssetPipeline()
        {
            string[] required =
            {
                True2DGamePath,
                RuntimeDirectory + "/Kaninbanker2DAssetCatalog.cs",
                RuntimeDirectory + "/KaninbankerImported2DPresentation.cs",
                RuntimeDirectory + "/KaninbankerAudio.cs",
                RuntimeDirectory + "/KaninbankerFeedback.cs",
                RuntimeDirectory + "/KaninbankerHammer2D.cs",
                RuntimeDirectory + "/KaninbankerAtmosphere.cs",
                RuntimeDirectory + "/KaninbankerScreenJuice.cs",
                RuntimeDirectory + "/KaninbankerProfile.cs",
                RuntimeDirectory + "/KaninbankerMusicPanel.cs",
                RuntimeDirectory + "/KaninbankerMayhemPass.cs",
                RuntimeDirectory + "/KaninbankerEventCircuit.cs",
                RuntimeDirectory + "/KaninbankerCareerBook.cs",
                RuntimeDirectory + "/KaninbankerWorldTour.cs",
                RuntimeDirectory + "/KaninbankerSettingsPanel.cs",
                RuntimeDirectory + "/KaninbankerTutorial.cs",
                RuntimeDirectory + "/KaninbankerPerformanceGovernor.cs",
                EditorDirectory + "/Kaninbanker2DAssetCatalogBuilder.cs",
                EditorDirectory + "/Kaninbanker2DAssetImporter.cs",
                EditorDirectory + "/Kaninbanker2DAssetPostprocessor.cs",
                EditorDirectory + "/KaninbankerCloudBootstrap.cs",
                EditorDirectory + "/KaninbankerCloudImportBootstrap.cs"
            };

            for (int i = 0; i < required.Length; i++)
            {
                if (!File.Exists(required[i]))
                    throw new BuildFailedException("KANINBANKER PREFLIGHT: required TRUE-2D source/tool file missing: " + required[i]);
            }

            if (File.Exists(RuntimeDirectory + "/KaninbankerGame.cs"))
                throw new BuildFailedException("KANINBANKER PREFLIGHT: legacy 3D KaninbankerGame.cs has returned. TRUE 2D build refused.");
        }

        private static void ValidateModulesAnd2DToolset()
        {
            if (!File.Exists(ManifestPath))
                throw new BuildFailedException("KANINBANKER PREFLIGHT: Packages/manifest.json is missing.");

            string manifest = File.ReadAllText(ManifestPath);
            string[] requiredPackages =
            {
                "com.unity.2d.animation",
                "com.unity.2d.aseprite",
                "com.unity.2d.pixel-perfect",
                "com.unity.2d.psdimporter",
                "com.unity.2d.spriteshape",
                "com.unity.2d.tilemap.extras",
                "com.unity.addressables",
                "com.unity.modules.androidjni",
                "com.unity.modules.audio",
                "com.unity.modules.imgui",
                "com.unity.modules.physics2d"
            };
            for (int i = 0; i < requiredPackages.Length; i++)
            {
                if (!manifest.Contains(requiredPackages[i]))
                    throw new BuildFailedException("KANINBANKER PREFLIGHT: required official Unity 2D package/module missing: " + requiredPackages[i]);
            }

            string[] forbidden3DModules =
            {
                "com.unity.modules.physics\"",
                "com.unity.modules.particlesystem\""
            };
            for (int i = 0; i < forbidden3DModules.Length; i++)
            {
                if (manifest.Contains(forbidden3DModules[i]))
                    throw new BuildFailedException("KANINBANKER PREFLIGHT: legacy 3D-oriented module found in TRUE 2D manifest: " + forbidden3DModules[i]);
            }
        }

        private static void ValidateTrue2DSource()
        {
            string source = File.ReadAllText(True2DGamePath);
            string[] required2D =
            {
                "orthographic = true",
                "SpriteRenderer",
                "CircleCollider2D",
                "Physics2D.OverlapPoint"
            };
            for (int i = 0; i < required2D.Length; i++)
            {
                if (!source.Contains(required2D[i]))
                    throw new BuildFailedException("KANINBANKER PREFLIGHT: TRUE 2D marker missing: " + required2D[i]);
            }
        }

        private static void ValidateEntireRuntimeIs2D()
        {
            string[] forbidden =
            {
                "GameObject.CreatePrimitive(",
                "Physics.Raycast(",
                "Physics.RaycastAll(",
                "Physics.SphereCast(",
                "Physics.Overlap",
                "MeshRenderer",
                "MeshFilter",
                "SkinnedMeshRenderer",
                "new Mesh(",
                "AddComponent<BoxCollider>",
                "AddComponent<SphereCollider>",
                "AddComponent<CapsuleCollider>",
                "AddComponent<MeshCollider>",
                "AddComponent<Rigidbody>",
                "LightType.Directional",
                "LightType.Point",
                "LightType.Spot",
                "ShadowQuality.",
                "QualitySettings.shadowDistance",
                "QualitySettings.pixelLightCount",
                "QualitySettings.lodBias"
            };

            string[] files = Directory.GetFiles(RuntimeDirectory, "*.cs", SearchOption.AllDirectories);
            for (int f = 0; f < files.Length; f++)
            {
                string source = File.ReadAllText(files[f]);
                for (int i = 0; i < forbidden.Length; i++)
                {
                    if (source.Contains(forbidden[i]))
                        throw new BuildFailedException("KANINBANKER PREFLIGHT: 3D-only runtime API found in " + files[f] + ": " + forbidden[i]);
                }
            }
        }

        private static void ValidateAssetCatalogPipeline()
        {
            string builder = File.ReadAllText(EditorDirectory + "/Kaninbanker2DAssetCatalogBuilder.cs");
            string bootstrap = File.ReadAllText(EditorDirectory + "/KaninbankerCloudBootstrap.cs");
            string presentation = File.ReadAllText(RuntimeDirectory + "/KaninbankerImported2DPresentation.cs");
            string audio = File.ReadAllText(RuntimeDirectory + "/KaninbankerAudio.cs");
            string feedback = File.ReadAllText(RuntimeDirectory + "/KaninbankerFeedback.cs");

            string[] builderMarkers =
            {
                "AssetDatabase.FindAssets(\"t:Sprite\"",
                "AssetDatabase.FindAssets(\"t:AudioClip\"",
                "catalog.normalRabbits",
                "catalog.hammers",
                "catalog.music"
            };
            for (int i = 0; i < builderMarkers.Length; i++)
            {
                if (!builder.Contains(builderMarkers[i]))
                    throw new BuildFailedException("KANINBANKER PREFLIGHT: asset catalog builder marker missing: " + builderMarkers[i]);
            }

            if (!bootstrap.Contains("Kaninbanker2DAssetCatalogBuilder.BuildCatalog();"))
                throw new BuildFailedException("KANINBANKER PREFLIGHT: Cloud Bootstrap no longer rebuilds the imported 2D asset catalog.");
            if (!presentation.Contains("Kaninbanker2DAssetCatalog.Load()"))
                throw new BuildFailedException("KANINBANKER PREFLIGHT: imported 2D presentation is not reading the generated asset catalog.");
            if (!audio.Contains("ApplyImportedAudioOverrides"))
                throw new BuildFailedException("KANINBANKER PREFLIGHT: imported audio overrides are not connected.");
            if (!feedback.Contains("catalog.PickEffect"))
                throw new BuildFailedException("KANINBANKER PREFLIGHT: imported 2D effect sprites are not connected.");
        }

        private static void ValidateVersion()
        {
            if (PlayerSettings.bundleVersion != "0.10.0" || PlayerSettings.Android.bundleVersionCode != 10)
                throw new BuildFailedException("KANINBANKER PREFLIGHT: expected app version 0.10.0 / versionCode 10.");
        }
    }
}
#endif