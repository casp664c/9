using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Android performance governor for the TRUE-2D portrait runtime.
    /// It changes only frame pacing and texture/sprite-friendly quality switches.
    /// No 3D lights, shadows, LOD, meshes or 3D-renderer assumptions are used.
    /// </summary>
    public sealed class KaninbankerPerformanceGovernor : MonoBehaviour
    {
        private const float SampleWindow = 3.0f;
        private float sampleTime;
        private int sampleFrames;
        private int qualityTier = 2;
        private float stableHighTime;
        private int targetFps = 60;
        private int lastSavedTarget = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerPerformanceGovernor>() != null)
                return;

            GameObject go = new GameObject("KaninbankerPerformanceGovernor2D");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerPerformanceGovernor>();
        }

        private void Awake()
        {
            QualitySettings.vSyncCount = 0;
            RefreshTargetFps(true);
            ApplyTier(2);
        }

        private void Update()
        {
            RefreshTargetFps(false);
            sampleFrames++;
            sampleTime += Time.unscaledDeltaTime;
            if (sampleTime < SampleWindow)
                return;

            float fps = sampleTime > 0f ? sampleFrames / sampleTime : targetFps;
            sampleFrames = 0;
            sampleTime = 0f;

            float lowThreshold = targetFps >= 60 ? 43f : 24f;
            float mediumThreshold = targetFps >= 60 ? 52f : 27f;
            float highThreshold = targetFps >= 60 ? 57f : 29f;

            if (fps < lowThreshold)
            {
                stableHighTime = 0f;
                ApplyTier(Mathf.Max(0, qualityTier - 1));
            }
            else if (fps < mediumThreshold)
            {
                stableHighTime = 0f;
                if (qualityTier > 1)
                    ApplyTier(1);
            }
            else if (fps > highThreshold)
            {
                stableHighTime += SampleWindow;
                if (stableHighTime >= 12f && qualityTier < 2)
                {
                    stableHighTime = 0f;
                    ApplyTier(qualityTier + 1);
                }
            }
            else
            {
                stableHighTime = Mathf.Max(0f, stableHighTime - SampleWindow * 0.5f);
            }
        }

        private void RefreshTargetFps(bool force)
        {
            int saved = PlayerPrefs.GetInt(KaninbankerSettingsPanel.FpsKey, 60);
            saved = saved <= 30 ? 30 : 60;
            if (!force && saved == lastSavedTarget)
                return;

            lastSavedTarget = saved;
            targetFps = saved;
            Application.targetFrameRate = targetFps;
        }

        private void ApplyTier(int tier)
        {
            tier = Mathf.Clamp(tier, 0, 2);
            if (tier == qualityTier && sampleFrames > 0)
                return;

            qualityTier = tier;

            // TRUE-2D quality policy: preserve sprite clarity and reduce only optional sampling cost.
            // Runtime-generated Kaninbanker sprites do not depend on 3D lights, shadows or LOD.
            if (tier == 0)
            {
                QualitySettings.antiAliasing = 0;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
            }
            else if (tier == 1)
            {
                QualitySettings.antiAliasing = 0;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
            }
            else
            {
                QualitySettings.antiAliasing = 2;
                QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
            }
        }
    }
}
