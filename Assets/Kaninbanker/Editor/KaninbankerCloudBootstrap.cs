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
        private static bool startupFallbackExecuted;

        public int callbackOrder => -1000;

        static KaninbankerCloudBootstrap()
        {
            EditorApplication.update += PrepareOnFirstEditorUpdate;
        }

        private static void PrepareOnFirstEditorUpdate()
        {
            EditorApplication.update -= PrepareOnFirstEditorUpdate;
            if (startupFallbackExecuted)
                return;

            startupFallbackExecuted = true;
            try
            {
                EnsureProjectReady();
                Debug.Log("[Kaninbanker] First-update FULL TRUE-2D cloud-build fallback completed.");
            }
            catch (Exception exception)
            {
                Debug.LogError("[Kaninbanker] First-update cloud-build fallback failed: " + exception);
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
            ConfigureEditorFor2D();
            ConfigurePlayerSettings();
            ConfigureLegacyInput();
            RegenerateTrue2DScene();
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
                              PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) == ApplicationIdentifier &&
                              PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait &&
                              !PlayerSettings.allowedAutorotateToLandscapeLeft &&
                              !PlayerSettings.allowedAutorotateToLandscapeRight &&
                              EditorSettings.defaultBehaviorMode == EditorBehaviorMode.Mode2D;

            if (sceneExists && sceneConfigured && settingsOk)
                Debug.Log("[Kaninbanker] SELF CHECK: PASS - FULL TRUE 2D + Android portrait configuration is ready.");
            else
                Debug.LogError($"[Kaninbanker] SELF CHECK: FAIL sceneExists={sceneExists} sceneConfigured={sceneConfigured} settingsOk={settingsOk}");
        }

        private static void ConfigureEditorFor2D()
        {
            EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.companyName = "casp664c";
            PlayerSettings.productName = ProductName;
            PlayerSettings.bundleVersion = "0.10.0";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, ApplicationIdentifier);
            PlayerSettings.Android.bundleVersionCode = 10;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.runInBackground = false;
            PlayerSettings.use32BitDisplayBuffer = true;
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

        private static void RegenerateTrue2DScene()
        {
            // Main.unity is generated build input, not hand-authored content. Recreate it every time so a
            // cached/untracked scene from an older 3D cloud workspace can NEVER become the next Android build.
            Directory.CreateDirectory(SceneDirectory);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "Main";

            GameObject root = new GameObject("KaninbankerGame2D");
            root.AddComponent<global::Kaninbanker.KaninbankerGame2D>();
            SceneManager.MoveGameObjectToScene(root, scene);

            if (!EditorSceneManager.SaveScene(scene, ScenePath, false))
                throw new InvalidOperationException("Could not save regenerated Kaninbanker TRUE-2D scene to " + ScenePath);

            AssetDatabase.ImportAsset(ScenePath, ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("[Kaninbanker] Regenerated FULL TRUE 2D cloud-build scene at " + ScenePath + " (stale 3D scene cannot be reused).");
        }

        private static void EnsureBuildSettings()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            Debug.Log("[Kaninbanker] Build Settings configured with FULL TRUE 2D scene " + ScenePath);
        }
    }
}
#endif