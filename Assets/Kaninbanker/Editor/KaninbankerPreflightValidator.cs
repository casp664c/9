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
        private const string True2DGamePath = RuntimeDirectory + "/KaninbankerGame2D.cs";

        public int callbackOrder => 1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            ValidatePortrait();
            ValidateEditor2DMode();
            ValidateBuildScene();
            ValidateRequiredSource();
            ValidateModulesAnd2DToolset();
            ValidateTrue2DSource();
            ValidateEntireRuntimeIs2D();
            ValidateVersion();
            Debug.Log("[Kaninbanker] PREFLIGHT PASS: FULL APP TRUE 2D ONLY + OFFICIAL UNITY 2D TOOLSET, portrait, Physics2D, all runtime sources clean, version 0.10.0.");
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

        private static void ValidateRequiredSource()
        {
            string[] required =
            {
                True2DGamePath,
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
                "Assets/Kaninbanker/Editor/Kaninbanker2DAssetPostprocessor.cs"
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

        private static void ValidateVersion()
        {
            if (PlayerSettings.bundleVersion != "0.10.0" || PlayerSettings.Android.bundleVersionCode != 10)
                throw new BuildFailedException("KANINBANKER PREFLIGHT: expected app version 0.10.0 / versionCode 10.");
        }
    }
}
#endif