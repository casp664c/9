#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Kaninbanker.Editor
{
    /// <summary>
    /// Machine-readable registry of canonical Unity-owned web/content hubs that are relevant
    /// to Kaninbanker's TRUE-2D Android production direction. This is metadata and discovery,
    /// not a license bypass or an uncontrolled web scraper. Package Manager samples are imported
    /// by KaninbankerOfficialUnitySamples; Asset Store products still require legitimate account
    /// ownership/license acceptance before their files can be imported into Assets/.
    /// </summary>
    public static class KaninbankerUnityOfficialSourceRegistry
    {
        private readonly struct Source
        {
            public readonly string Name;
            public readonly string Url;
            public readonly string Kind;
            public readonly bool RequiresAssetStoreAccount;

            public Source(string name, string url, string kind, bool requiresAssetStoreAccount)
            {
                Name = name;
                Url = url;
                Kind = kind;
                RequiresAssetStoreAccount = requiresAssetStoreAccount;
            }
        }

        private static readonly Source[] Sources =
        {
            new Source("Unity Home", "https://www.unity.com", "official-home", false),
            new Source("Unity 2D", "https://unity.com/features/2d", "2d-hub", false),
            new Source("Unity 6 Resources Hub", "https://unity.com/campaign/unity-6-resources", "unity6-hub", false),
            new Source("Unity 2D Asset Store", "https://assetstore.unity.com/2d", "2d-asset-catalog", true),
            new Source("Unity Tutorial Projects", "https://assetstore.unity.com/essentials/tutorial-projects", "tutorial-catalog", true),
            new Source("Unity Technologies Asset Store Publisher", "https://assetstore.unity.com/publishers/1", "publisher", true),
            new Source("Unity Learn - Create a 2D game", "https://learn.unity.com/collection/create-a-2d-game", "2d-learning", false),
            new Source("Unity Learn - 2D Game Kit", "https://learn.unity.com/project/2d-game-kit", "2d-game-kit", false),
            new Source("2D Game Kit - Asset Store", "https://assetstore.unity.com/packages/templates/tutorials/2d-game-kit-107098", "2d-game-kit", true),
            new Source("Unity Manual - Set up a project for 2D games", "https://docs.unity3d.com/6000.3/Documentation/Manual/setup-project-2d-game.html", "2d-docs", false),
            new Source("Unity Package Manager Sample API", "https://docs.unity3d.com/6000.0/Documentation/ScriptReference/PackageManager.UI.Sample.html", "package-api", false),
            new Source("Lost Crypt - 2D Sample Project", "https://assetstore.unity.com/packages/essentials/tutorial-projects/lost-crypt-2d-sample-project-158673", "2d-sample", true),
            new Source("Happy Harvest - 2D Sample Project", "https://assetstore.unity.com/packages/essentials/tutorial-projects/happy-harvest-2d-sample-project-259218", "2d-sample", true),
            new Source("Dragon Crashers - URP 2D Sample Project", "https://assetstore.unity.com/packages/essentials/tutorial-projects/dragon-crashers-urp-2d-sample-project-190721", "2d-sample", true),
            new Source("Dragon Crashers - UI Toolkit Sample Project", "https://assetstore.unity.com/packages/essentials/tutorial-projects/dragon-crashers-ui-toolkit-sample-project-231178", "ui-sample", true),
            new Source("QuizU - A UI Toolkit Sample", "https://assetstore.unity.com/packages/essentials/tutorial-projects/quizu-a-ui-toolkit-sample-268492", "ui-sample", true),
            new Source("2D Animation Samples", "https://assetstore.unity.com/packages/2d/characters/2d-animation-samples-354550", "2d-animation", true),
            new Source("Unity 2D Renderer Samples", "https://github.com/Unity-Technologies/2d-renderer-samples", "official-source-repo", false),
            new Source("Unity 2D Tech Demos", "https://github.com/Unity-Technologies/2d-techdemos", "official-source-repo", false),
            new Source("Unity Technologies GitHub", "https://github.com/Unity-Technologies", "official-source-org", false)
        };

        [MenuItem("Tools/Kaninbanker/Report Official Unity Web Sources")]
        public static void ReportOfficialSources()
        {
            for (int i = 0; i < Sources.Length; i++)
            {
                Source source = Sources[i];
                string access = source.RequiresAssetStoreAccount ? "ACCOUNT/LICENSE REQUIRED" : "PUBLIC OFFICIAL SOURCE";
                Debug.Log($"[Kaninbanker][UnityOfficialSource] {source.Kind} | {access} | {source.Name} | {source.Url}");
            }

            Debug.Log($"[Kaninbanker][UnityOfficialSource] Registered canonical official sources: {Sources.Length}. Unity Package Manager content is discovered dynamically in addition to this registry.");
        }
    }
}
#endif
