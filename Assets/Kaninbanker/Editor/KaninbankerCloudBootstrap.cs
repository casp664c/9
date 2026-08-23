#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
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

        // Unity Build Automation checks EditorBuildSettings before normal build preprocessors run.
        // Prepare the scene synchronously when the editor assembly is loaded so Cloud Build can
        // never reach its "no scenes configured" guard with an empty scene list.
        static KaninbankerCloudBootstrap()
        {
            try
            {
                EnsureCloudBuildInputs();
                Debug.Log("[Kaninbanker] Early cloud-build bootstrap completed.");
            }
            catch (Exception exception)
            {
                Debug.LogError("[Kaninbanker] Early cloud-build bootstrap failed: " + exception);
                throw;
            }
        }

        public void OnPreprocessBuild(BuildReport report)
        {
            EnsureProjectReady();
        }

        [MenuItem("Tools/Kaninbanker/Prepare Cloud Build")]
        public static void EnsureProjectReady()
        {
            ConfigurePlayerSettings();
            ConfigureLegacyInput();
            EnsureCloudBuildInputs();
            AssetDatabase.SaveAssets();
        }

        private static void EnsureCloudBuildInputs()
        {
            EnsureSceneExists();
            EnsureBuildSettings();
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Tools/Kaninbanker/Run Static Self Check")]
        public static void RunStaticSelfCheck()
        {
            bool sceneExists = File.Exists(ScenePath) || AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null;
            bool sceneConfigured = EditorBuildSettings.scenes != null &&
                                   EditorBuildSettings.scenes.Length > 0 &&
                                   EditorBuildSettings.scenes[0].enabled &&
                                   EditorBuildSettings.scenes[0].path == ScenePath;
            bool settingsOk = PlayerSettings.productName == ProductName &&
                              PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) == ApplicationIdentifier;

            if (sceneExists && sceneConfigured && settingsOk)
                Debug.Log("[Kaninbanker] SELF CHECK: PASS");
            else
                Debug.LogError($"[Kaninbanker] SELF CHECK: FAIL sceneExists={sceneExists} sceneConfigured={sceneConfigured} settingsOk={settingsOk}");
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "casp664c";
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = "0.1.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, ApplicationIdentifier);
            PlayerSettings.Android.bundleVersionCode = 1;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

            // Keep external audio (for example YouTube Music) alive while Kaninbanker is foregrounded.
            PlayerSettings.muteOtherAudioSources = false;
        }

        private static void ConfigureLegacyInput()
        {
            MethodInfo getter = typeof(PlayerSettings).GetMethod(
                "GetSerializedObject",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

            if (getter == null || getter.Invoke(null, null) is not SerializedObject settings)
            {
                Debug.LogWarning("[Kaninbanker] Could not inspect activeInputHandler; leaving Unity default input backend unchanged.");
                return;
            }

            settings.Update();
            SerializedProperty activeInputHandler = settings.FindProperty("activeInputHandler");
            if (activeInputHandler != null && activeInputHandler.intValue != 0)
            {
                activeInputHandler.intValue = 0;
                settings.ApplyModifiedPropertiesWithoutUndo();
                Debug.Log("[Kaninbanker] Active Input Handling set to legacy Input Manager for touch/mouse compatibility.");
            }
        }

        private static void EnsureSceneExists()
        {
            SceneAsset existing = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
            if (existing != null)
                return;

            Directory.CreateDirectory(SceneDirectory);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Main";

            var root = new GameObject("KaninbankerGame");
            root.AddComponent<global::Kaninbanker.KaninbankerGame>();
            SceneManager.MoveGameObjectToScene(root, scene);

            if (!EditorSceneManager.SaveScene(scene, ScenePath, false))
                throw new InvalidOperationException("Could not save generated Kaninbanker scene to " + ScenePath);

            AssetDatabase.ImportAsset(ScenePath, ImportAssetOptions.ForceSynchronousImport);
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

            Debug.Log("[Kaninbanker] Build Settings configured with " + ScenePath);
        }
    }
}
#endif
