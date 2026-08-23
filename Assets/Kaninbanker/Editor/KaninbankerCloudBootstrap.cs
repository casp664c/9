#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kaninbanker.Editor
{
    [InitializeOnLoad]
    public sealed class KaninbankerCloudBootstrap : IPreprocessBuildWithReport
    {
        private const string SceneDirectory = "Assets/Kaninbanker/Scenes";
        private const string ScenePath = SceneDirectory + "/Main.unity";
        private const string ProductName = "Kaninbanker";
        private const string ApplicationIdentifier = "com.casp664c.kaninbanker";

        public int callbackOrder => -1000;

        static KaninbankerCloudBootstrap()
        {
            EditorApplication.delayCall += EnsureProjectReady;
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            EnsureProjectReady();
        }

        [MenuItem("Tools/Kaninbanker/Prepare Cloud Build")]
        public static void EnsureProjectReady()
        {
            ConfigurePlayerSettings();
            EnsureSceneExists();
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Tools/Kaninbanker/Run Static Self Check")]
        public static void RunStaticSelfCheck()
        {
            bool ok = File.Exists(ScenePath) || AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null;
            ok &= PlayerSettings.productName == ProductName;
            ok &= PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android) == ApplicationIdentifier;

            if (ok)
                Debug.Log("[Kaninbanker] SELF CHECK: PASS");
            else
                Debug.LogError("[Kaninbanker] SELF CHECK: FAIL");
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "casp664c";
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, ApplicationIdentifier);
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        }

        private static void EnsureSceneExists()
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null)
                return;

            Directory.CreateDirectory(SceneDirectory);
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            scene.name = "Main";

            var root = new GameObject("KaninbankerGame");
            root.AddComponent<global::Kaninbanker.KaninbankerGame>();
            SceneManager.MoveGameObjectToScene(root, scene);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorSceneManager.CloseScene(scene, true);
            Debug.Log("[Kaninbanker] Generated cloud-build scene at " + ScenePath);
        }

        private static void EnsureBuildSettings()
        {
            var current = EditorBuildSettings.scenes;
            if (current != null && current.Length == 1 && current[0].path == ScenePath && current[0].enabled)
                return;

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
        }
    }
}
#endif
