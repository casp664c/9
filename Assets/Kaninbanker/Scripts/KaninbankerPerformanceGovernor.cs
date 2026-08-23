using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Lightweight Android performance governor that keeps the large portrait arena responsive.
    /// It watches sustained frame rate and scales expensive quality settings gradually. The player
    /// can select a 30 or 60 FPS target through KaninbankerSettingsPanel.
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

            GameObject go = new GameObject("KaninbankerPerformanceGovernor");
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
            switch (tier)
            {
                case 0:
                    QualitySettings.shadows = ShadowQuality.Disable;
                    QualitySettings.shadowDistance = 0f;
                    QualitySettings.pixelLightCount = 1;
                    QualitySettings.antiAliasing = 0;
                    QualitySettings.lodBias = 0.70f;
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                    break;

                case 1:
                    QualitySettings.shadows = ShadowQuality.HardOnly;
                    QualitySettings.shadowDistance = 16f;
                    QualitySettings.pixelLightCount = 2;
                    QualitySettings.antiAliasing = 0;
                    QualitySettings.lodBias = 0.90f;
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                    break;

                default:
                    QualitySettings.shadows = ShadowQuality.All;
                    QualitySettings.shadowDistance = 28f;
                    QualitySettings.pixelLightCount = 3;
                    QualitySettings.antiAliasing = 2;
                    QualitySettings.lodBias = 1.15f;
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                    break;
            }
        }
    }
}
