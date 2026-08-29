#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Kaninbanker.Editor
{
    public static class KaninbankerBuild
    {
        private const string OutputDirectory = "Builds/Android";
        private const string ApkPath = OutputDirectory + "/Kaninbanker.apk";

        [MenuItem("Tools/Kaninbanker/Build Android APK")]
        public static void BuildAndroidApk()
        {
            KaninbankerCloudBootstrap.EnsureProjectReady();
            Directory.CreateDirectory(OutputDirectory);
            EditorUserBuildSettings.buildAppBundle = false;

            var options = new BuildPlayerOptions
            {
                scenes = EnabledScenes(),
                locationPathName = ApkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;
            Debug.Log($"[Kaninbanker] Android build result: {summary.result}; size={summary.totalSize}; time={summary.totalTime}");

            if (summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Kaninbanker Android build failed: " + summary.result);
        }

        private static string[] EnabledScenes()
        {
            var scenes = EditorBuildSettings.scenes;
            int count = 0;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled)
                    count++;
            }

            var result = new string[count];
            int index = 0;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].enabled)
                    result[index++] = scenes[i].path;
            }
            return result;
        }
    }
}
#endif
