#if UNITY_EDITOR
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Kaninbanker.Editor
{
    /// <summary>
    /// Keeps the official Unity Package Manager sample-import pipeline wired into every cloud build.
    /// This validator does not require an Asset Store login and does not bypass license acceptance.
    /// </summary>
    public sealed class KaninbankerOfficialUnitySamplesValidator : IPreprocessBuildWithReport
    {
        private const string ImporterPath = "Assets/Kaninbanker/Editor/KaninbankerOfficialUnitySamples.cs";
        private const string BootstrapPath = "Assets/Kaninbanker/Editor/KaninbankerCloudBootstrap.cs";
        private const string ManifestPath = "Packages/manifest.json";

        public int callbackOrder => 900;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!File.Exists(ImporterPath))
                throw new BuildFailedException("KANINBANKER UNITY-SAMPLES: official Unity sample importer is missing.");

            if (!File.Exists(BootstrapPath))
                throw new BuildFailedException("KANINBANKER UNITY-SAMPLES: Cloud Bootstrap is missing.");

            string bootstrap = File.ReadAllText(BootstrapPath);
            if (!bootstrap.Contains("KaninbankerOfficialUnitySamples.ImportAllNonInteractiveSamples();"))
                throw new BuildFailedException("KANINBANKER UNITY-SAMPLES: Cloud Bootstrap no longer imports official Unity package samples before catalog generation.");

            string manifest = File.ReadAllText(ManifestPath);
            string[] packageMarkers =
            {
                "com.unity.2d.animation",
                "com.unity.2d.aseprite",
                "com.unity.2d.pixel-perfect",
                "com.unity.2d.psdimporter",
                "com.unity.2d.spriteshape",
                "com.unity.2d.tilemap.extras"
            };

            for (int i = 0; i < packageMarkers.Length; i++)
            {
                if (!manifest.Contains(packageMarkers[i]))
                    throw new BuildFailedException("KANINBANKER UNITY-SAMPLES: official 2D sample source package missing: " + packageMarkers[i]);
            }

            if (!Directory.Exists("Assets/Samples"))
                Debug.LogWarning("[Kaninbanker][UnitySamples] Assets/Samples does not exist yet. Packages may expose no non-interactive samples in this Unity version, or import may have been skipped; procedural/imported project assets remain the fallback.");
            else
                Debug.Log("[Kaninbanker][UnitySamples] Assets/Samples exists and official package sample content is available to the asset catalog.");
        }
    }
}
#endif
