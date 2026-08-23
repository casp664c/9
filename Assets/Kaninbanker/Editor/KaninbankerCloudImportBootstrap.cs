#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Kaninbanker.Editor
{
    /// <summary>
    /// Unity recommends doing asset operations after domain reload from
    /// AssetPostprocessor.OnPostprocessAllAssets rather than directly in
    /// InitializeOnLoad. Unity Build Automation checks the scene list very
    /// early, so prepare the generated scene and EditorBuildSettings here.
    /// </summary>
    public sealed class KaninbankerCloudImportBootstrap : AssetPostprocessor
    {
        private static bool preparedForCurrentDomain;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths,
            bool didDomainReload)
        {
            if (!didDomainReload || preparedForCurrentDomain)
                return;

            preparedForCurrentDomain = true;

            try
            {
                KaninbankerCloudBootstrap.EnsureProjectReady();
                Debug.Log("[Kaninbanker] Post-domain-reload cloud-build preparation completed.");
            }
            catch (Exception exception)
            {
                Debug.LogError("[Kaninbanker] Post-domain-reload cloud-build preparation failed: " + exception);
                throw;
            }
        }
    }
}
#endif
