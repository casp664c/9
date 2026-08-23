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

        public int callbackOrder => 1000;

        public void OnPreprocessBuild(BuildReport report)
        {
            ValidatePortrait();
            ValidateBuildScene();
            ValidateRequiredSource();
            ValidateModules();
            Debug.Log("[Kaninbanker] PREFLIGHT PASS: portrait, scene, mega source set and required Unity modules are ready.");
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
                "Assets/Kaninbanker/Scripts/KaninbankerGame.cs",
                "Assets/Kaninbanker/Scripts/KaninbankerAudio.cs",
                "Assets/Kaninbanker/Scripts/KaninbankerFeedback.cs",
                "Assets/Kaninbanker/Scripts/KaninbankerProfile.cs",
                "Assets/Kaninbanker/Scripts/KaninbankerMusicPanel.cs",
                "Assets/Kaninbanker/Scripts/KaninbankerMayhemPass.cs",
                "Assets/Kaninbanker/Scripts/KaninbankerEventCircuit.cs",
                "Assets/Kaninbanker/Scripts/KaninbankerCareerBook.cs",
                "Assets/Kaninbanker/Scripts/KaninbankerWorldTour.cs",
                "Assets/Kaninbanker/Scripts/KaninbankerSettingsPanel.cs",
                "Assets/Kaninbanker/Scripts/KaninbankerTutorial.cs",
                "Assets/Kaninbanker/Scripts/KaninbankerPerformanceGovernor.cs"
            };

            for (int i = 0; i < required.Length; i++)
            {
                if (!File.Exists(required[i]))
                    throw new BuildFailedException("KANINBANKER PREFLIGHT: required source file missing: " + required[i]);
            }
        }

        private static void ValidateModules()
        {
            if (!File.Exists(ManifestPath))
                throw new BuildFailedException("KANINBANKER PREFLIGHT: Packages/manifest.json is missing.");

            string manifest = File.ReadAllText(ManifestPath);
            string[] requiredModules =
            {
                "com.unity.modules.androidjni",
                "com.unity.modules.audio",
                "com.unity.modules.imgui",
                "com.unity.modules.particlesystem",
                "com.unity.modules.physics"
            };

            for (int i = 0; i < requiredModules.Length; i++)
            {
                if (!manifest.Contains(requiredModules[i]))
                    throw new BuildFailedException("KANINBANKER PREFLIGHT: required Unity module missing from manifest: " + requiredModules[i]);
            }
        }
    }
}
#endif