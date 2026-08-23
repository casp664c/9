#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using PackageManagerPackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Kaninbanker.Editor
{
    /// <summary>
    /// Discovers every currently loaded official Unity package (com.unity.*) through
    /// PackageManagerPackageInfo.GetAllRegisteredPackages(). Cloud builds automatically import all
    /// non-interactive samples from TRUE-2D-safe official packages before Kaninbanker's
    /// asset catalog is rebuilt.
    ///
    /// We intentionally do NOT auto-import samples from 3D/HDRP/XR/Multiplayer/Industry
    /// packages into the production Android build. They are still discovered and reported,
    /// but mixing them into this project could re-introduce 3D dependencies or compile-time
    /// sample code that violates the TRUE-2D contract.
    ///
    /// Asset Store products that require account ownership / license acceptance are not
    /// downloaded here. This class only uses Unity Package Manager's documented PackageInfo
    /// and Sample APIs.
    /// </summary>
    public static class KaninbankerOfficialUnitySamples
    {
        private static bool importAttemptedThisDomain;

        [MenuItem("Tools/Kaninbanker/Import ALL Safe Official Unity 2D Package Samples")]
        public static void ImportAllNonInteractiveSamples()
        {
            if (importAttemptedThisDomain)
                return;

            importAttemptedThisDomain = true;

            PackageManagerPackageInfo[] packages = PackageManagerPackageInfo.GetAllRegisteredPackages();
            if (packages == null || packages.Length == 0)
            {
                Debug.LogWarning("[Kaninbanker][UnitySamples] Unity reported no registered packages. Asset catalog fallback remains active.");
                return;
            }

            int unityOwnedPackages = 0;
            int safe2DPackages = 0;
            int skippedNon2DUnityPackages = 0;
            int discoveredSamples = 0;
            int alreadyImported = 0;
            int newlyImported = 0;
            int skippedInteractive = 0;
            int failures = 0;

            for (int p = 0; p < packages.Length; p++)
            {
                PackageManagerPackageInfo package = packages[p];
                if (package == null || string.IsNullOrEmpty(package.name) || !IsUnityOwned(package.name))
                    continue;

                unityOwnedPackages++;

                if (!IsCloudSafe2DPackage(package.name))
                {
                    skippedNon2DUnityPackages++;
                    Debug.Log($"[Kaninbanker][UnitySamples] Discovered Unity-owned package {package.name}@{package.version}; not imported because it is outside the TRUE-2D-safe sample set.");
                    continue;
                }

                safe2DPackages++;
                ImportPackageSamples(
                    package.name,
                    package.version,
                    ref discoveredSamples,
                    ref alreadyImported,
                    ref newlyImported,
                    ref skippedInteractive,
                    ref failures);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            Debug.Log(
                $"[Kaninbanker][UnitySamples] Automatic Unity-owned content pass complete. " +
                $"unityOwnedPackages={unityOwnedPackages}, safe2DPackages={safe2DPackages}, " +
                $"non2DReportedOnly={skippedNon2DUnityPackages}, samplesDiscovered={discoveredSamples}, " +
                $"samplesImported={newlyImported}, alreadyImported={alreadyImported}, " +
                $"interactiveSkipped={skippedInteractive}, failures={failures}.");
        }

        [MenuItem("Tools/Kaninbanker/Report ALL Installed Unity-Owned Packages")]
        public static void ReportAllUnityOwnedPackages()
        {
            PackageManagerPackageInfo[] packages = PackageManagerPackageInfo.GetAllRegisteredPackages();
            int count = 0;
            if (packages != null)
            {
                for (int i = 0; i < packages.Length; i++)
                {
                    PackageManagerPackageInfo package = packages[i];
                    if (package == null || string.IsNullOrEmpty(package.name) || !IsUnityOwned(package.name))
                        continue;

                    count++;
                    string mode = IsCloudSafe2DPackage(package.name) ? "AUTO-IMPORT-2D" : "DISCOVER-ONLY";
                    Debug.Log($"[Kaninbanker][UnityPackages] {mode} {package.name}@{package.version}");
                }
            }

            Debug.Log($"[Kaninbanker][UnityPackages] Registered Unity-owned package count: {count}.");
        }

        private static void ImportPackageSamples(
            string packageName,
            string packageVersion,
            ref int discovered,
            ref int alreadyImported,
            ref int newlyImported,
            ref int skippedInteractive,
            ref int failures)
        {
            try
            {
                foreach (Sample sample in Sample.FindByPackage(packageName, packageVersion))
                {
                    discovered++;

                    if (sample.interactiveImport)
                    {
                        skippedInteractive++;
                        Debug.Log($"[Kaninbanker][UnitySamples] Skipped interactive sample '{sample.displayName}' from {packageName}@{packageVersion} in batch/cloud mode.");
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
                        Debug.Log($"[Kaninbanker][UnitySamples] Imported official Unity sample '{sample.displayName}' from {packageName}@{packageVersion}.");
                    }
                    else
                    {
                        failures++;
                        Debug.LogWarning($"[Kaninbanker][UnitySamples] Unity returned false while importing '{sample.displayName}' from {packageName}@{packageVersion}.");
                    }
                }
            }
            catch (Exception exception)
            {
                failures++;
                Debug.LogWarning($"[Kaninbanker][UnitySamples] Could not enumerate/import samples from {packageName}@{packageVersion}: {exception.Message}");
            }
        }

        private static bool IsUnityOwned(string packageName)
        {
            return packageName.StartsWith("com.unity.", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCloudSafe2DPackage(string packageName)
        {
            // All official Unity 2D packages currently loaded by this project are eligible.
            // This automatically includes future com.unity.2d.* packages without another code edit.
            return packageName.StartsWith("com.unity.2d.", StringComparison.OrdinalIgnoreCase);
        }
    }
}
#endif
