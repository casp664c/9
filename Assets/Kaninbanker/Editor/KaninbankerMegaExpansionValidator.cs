#if UNITY_EDITOR
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Kaninbanker.Editor
{
    /// <summary>
    /// Build gate for the post-0.10 TRUE-2D mega expansion systems.
    /// </summary>
    public sealed class KaninbankerMegaExpansionValidator : IPreprocessBuildWithReport
    {
        private const string MegaHubPath = "Assets/Kaninbanker/Scripts/KaninbankerMegaHub.cs";
        private const string BossPhasesPath = "Assets/Kaninbanker/Scripts/KaninbankerBossPhases2D.cs";

        public int callbackOrder => 960;

        public void OnPreprocessBuild(BuildReport report)
        {
            ValidateFile(MegaHubPath, new[]
            {
                "RuntimeInitializeOnLoadMethod",
                "Screen.safeArea",
                "QuestClaimKey",
                "CoinUpgradeKey",
                "XpUpgradeKey",
                "QuestUpgradeKey",
                "VaultUpgradeKey",
                "CodexNames",
                "Kaninbanker2D.Coins",
                "Kaninbanker2D.Xp"
            });

            ValidateFile(BossPhasesPath, new[]
            {
                "RuntimeInitializeOnLoadMethod",
                "RabbitKind2D.Boss",
                "SpriteRenderer",
                "Kaninbanker2DArt.Circle",
                "ReducedFxKey",
                "Health",
                "MaxHealth"
            });

            Debug.Log("[Kaninbanker] MEGA EXPANSION VALIDATOR PASS: daily quests + upgrades + codex + TRUE-2D boss phases are present.");
        }

        private static void ValidateFile(string path, string[] required)
        {
            if (!File.Exists(path))
                throw new BuildFailedException("KANINBANKER MEGA EXPANSION: required file missing: " + path);

            string source = File.ReadAllText(path);
            for (int i = 0; i < required.Length; i++)
            {
                if (!source.Contains(required[i]))
                    throw new BuildFailedException("KANINBANKER MEGA EXPANSION: required marker missing in " + path + ": " + required[i]);
            }

            string[] forbidden =
            {
                "GameObject.CreatePrimitive(",
                "Physics.Raycast(",
                "MeshRenderer",
                "MeshFilter",
                "SkinnedMeshRenderer",
                "AddComponent<Rigidbody>",
                "AddComponent<BoxCollider>",
                "AddComponent<SphereCollider>",
                "LightType.Directional",
                "LightType.Point",
                "LightType.Spot",
                "orthographic = false"
            };

            for (int i = 0; i < forbidden.Length; i++)
            {
                if (source.Contains(forbidden[i]))
                    throw new BuildFailedException("KANINBANKER MEGA EXPANSION: 3D-only marker found in " + path + ": " + forbidden[i]);
            }
        }
    }
}
#endif
