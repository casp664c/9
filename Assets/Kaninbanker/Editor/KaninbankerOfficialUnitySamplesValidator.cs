#if UNITY_EDITOR
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Kaninbanker.Editor
{
    /// <summary>
    /// Keeps the automatic official Unity Package Manager sample-import pipeline wired into every cloud build.
    /// All installed com.unity.* packages are discovered; only TRUE-2D-safe com.unity.2d.* package samples are
    /// imported automatically. Asset Store account/license gates are never bypassed.
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
            if (!File.Exists(ManifestPath))
                throw new BuildFailedException("KANINBANKER UNITY-SAMPLES: Packages/manifest.json is missing.");

            string importer = File.ReadAllText(ImporterPath);
            string[] discoveryMarkers =
            {
                "PackageManagerPackageInfo = UnityEditor.PackageManager.PackageInfo",
                "PackageManagerPackageInfo.GetAllRegisteredPackages()",
                "packageName.StartsWith(\"com.unity.\"",
                "packageName.StartsWith(\"com.unity.2d.\"",
                "Sample.FindByPackage",
                "Sample.ImportOptions.OverridePreviousImports",
                "sample.interactiveImport",
                "sample.isImported"
            };

            for (int i = 0; i < discoveryMarkers.Length; i++)
            {
                if (!importer.Contains(discoveryMarkers[i]))
                    throw new BuildFailedException("KANINBANKER UNITY-SAMPLES: automatic Unity-owned package discovery/import marker missing: " + discoveryMarkers[i]);
            }

            if (importer.Contains("using UnityEditor.PackageManager;"))
                throw new BuildFailedException("KANINBANKER UNITY-SAMPLES: broad PackageManager using can reintroduce Unity 6 PackageInfo ambiguity. Use the explicit PackageManagerPackageInfo alias.");

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
                "com.unity.2d.sprite",
                "com.unity.2d.spriteshape",
                "com.unity.2d.tilemap",
                "com.unity.2d.tilemap.extras"
            };

            for (int i = 0; i < packageMarkers.Length; i++)
            {
                if (!manifest.Contains(packageMarkers[i]))
                    throw new BuildFailedException("KANINBANKER UNITY-SAMPLES: official 2D source package missing: " + packageMarkers[i]);
            }

            if (!Directory.Exists("Assets/Samples"))
                Debug.LogWarning("[Kaninbanker][UnitySamples] Assets/Samples does not exist yet. Installed official 2D packages may expose no non-interactive samples in this Unity version; project/imported-art fallbacks remain active.");
            else
                Debug.Log("[Kaninbanker][UnitySamples] Assets/Samples exists. Official package sample content can be indexed by the Kaninbanker asset catalog.");
        }
    }
}
#endif
