#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Kaninbanker.Editor
{
    /// <summary>
    /// Imports every non-interactive sample exposed by the official Unity 2D packages
    /// installed in Packages/manifest.json. Imported package samples are copied under
    /// Assets/Samples by Unity itself, after which Kaninbanker's asset catalog can index
    /// compatible Sprites and AudioClips automatically.
    ///
    /// Asset Store products that require account ownership / license acceptance are not
    /// downloaded here. This class only uses Unity Package Manager's official Sample API.
    /// </summary>
    public static class KaninbankerOfficialUnitySamples
    {
        private readonly struct PackageSource
        {
            public readonly string Name;
            public readonly string Version;

            public PackageSource(string name, string version)
            {
                Name = name;
                Version = version;
            }
        }

        private static readonly PackageSource[] Official2DPackages =
        {
            new PackageSource("com.unity.2d.animation", "10.2.2"),
            new PackageSource("com.unity.2d.aseprite", "1.1.10"),
            new PackageSource("com.unity.2d.pixel-perfect", "5.0.3"),
            new PackageSource("com.unity.2d.psdimporter", "9.1.1"),
            new PackageSource("com.unity.2d.spriteshape", "10.0.7"),
            new PackageSource("com.unity.2d.tilemap.extras", "4.1.0")
        };

        private static bool importAttemptedThisDomain;

        [MenuItem("Tools/Kaninbanker/Import ALL Official Unity 2D Package Samples")]
        public static void ImportAllNonInteractiveSamples()
        {
            if (importAttemptedThisDomain)
                return;

            importAttemptedThisDomain = true;
            int discovered = 0;
            int alreadyImported = 0;
            int newlyImported = 0;
            int skippedInteractive = 0;
            int failures = 0;

            for (int p = 0; p < Official2DPackages.Length; p++)
            {
                PackageSource package = Official2DPackages[p];
                try
                {
                    foreach (Sample sample in Sample.FindByPackage(package.Name, package.Version))
                    {
                        discovered++;

                        if (sample.interactiveImport)
                        {
                            skippedInteractive++;
                            Debug.Log($"[Kaninbanker][UnitySamples] Skipped interactive sample '{sample.displayName}' from {package.Name}@{package.Version} in batch/cloud mode.");
                            continue;
                        }

                        if (sample.isImported)
                        {
                            alreadyImported++;
                            continue;
                        }

                        bool imported = sample.Import(Sample.ImportOptions.OverridePreviousImports);
                        if (imported)
                        {
                            newlyImported++;
                            Debug.Log($"[Kaninbanker][UnitySamples] Imported '{sample.displayName}' from {package.Name}@{package.Version}.");
                        }
                        else
                        {
                            failures++;
                            Debug.LogWarning($"[Kaninbanker][UnitySamples] Unity returned false while importing '{sample.displayName}' from {package.Name}@{package.Version}.");
                        }
                    }
                }
                catch (Exception exception)
                {
                    failures++;
                    Debug.LogWarning($"[Kaninbanker][UnitySamples] Could not enumerate/import samples from {package.Name}@{package.Version}: {exception.Message}");
                }
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log($"[Kaninbanker][UnitySamples] Official Unity 2D sample pass complete. discovered={discovered}, imported={newlyImported}, alreadyImported={alreadyImported}, interactiveSkipped={skippedInteractive}, failures={failures}.");
        }
    }
}
#endif
