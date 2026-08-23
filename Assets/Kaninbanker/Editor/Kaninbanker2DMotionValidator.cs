#if UNITY_EDITOR
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Kaninbanker.Editor
{
    /// <summary>
    /// Build gate for the portrait TRUE-2D motion layer. Motion may animate SpriteRenderer
    /// presentation only; it must never reintroduce meshes, perspective cameras or 3D physics.
    /// </summary>
    public sealed class Kaninbanker2DMotionValidator : IPreprocessBuildWithReport
    {
        private const string MotionPath = "Assets/Kaninbanker/Scripts/Kaninbanker2DMotionDirector.cs";

        public int callbackOrder => 950;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!File.Exists(MotionPath))
                throw new BuildFailedException("KANINBANKER 2D MOTION: Kaninbanker2DMotionDirector.cs is missing.");

            string source = File.ReadAllText(MotionPath);
            string[] required =
            {
                "RuntimeInitializeOnLoadMethod",
                "SpriteRenderer",
                "2D_SideOrb_",
                "2D_Stripe_",
                "ReducedFxKey",
                "Time.unscaledTime"
            };

            for (int i = 0; i < required.Length; i++)
            {
                if (!source.Contains(required[i]))
                    throw new BuildFailedException("KANINBANKER 2D MOTION: required motion marker missing: " + required[i]);
            }

            string[] forbidden =
            {
                "GameObject.CreatePrimitive(",
                "Physics.Raycast(",
                "MeshRenderer",
                "MeshFilter",
                "Perspective",
                "LightType.Directional",
                "LightType.Point",
                "AddComponent<Rigidbody>",
                "AddComponent<BoxCollider>"
            };

            for (int i = 0; i < forbidden.Length; i++)
            {
                if (source.Contains(forbidden[i]))
                    throw new BuildFailedException("KANINBANKER 2D MOTION: 3D marker found in motion system: " + forbidden[i]);
            }

            Debug.Log("[Kaninbanker][2DMotion] PREFLIGHT PASS: portrait TRUE-2D motion layer is present and 3D-free.");
        }
    }
}
#endif
