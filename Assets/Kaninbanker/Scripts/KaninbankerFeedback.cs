using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// True-2D hit feedback. Uses only SpriteRenderer objects plus orthographic camera shake.
    /// Imported effect sprites are preferred; procedural circles remain the fallback.
    /// </summary>
    public sealed class KaninbankerFeedback : MonoBehaviour
    {
        private const int BurstPoolSize = 16;
        private Camera targetCamera;
        private Vector3 cameraBasePosition;
        private SpriteRenderer[] bursts;
        private float[] burstTimes;
        private float[] burstDurations;
        private int nextBurst;
        private float shakeTimeLeft;
        private float shakeStrength;

        private bool ReducedFx => PlayerPrefs.GetInt(KaninbankerSettingsPanel.ReducedFxKey, 0) != 0;

        public void Configure(Camera cameraToShake)
        {
            targetCamera = cameraToShake;
            if (targetCamera != null)
                cameraBasePosition = targetCamera.transform.position;
            EnsurePool();
        }

        private void LateUpdate()
        {
            UpdateBursts();
            if (targetCamera == null)
                return;

            if (shakeTimeLeft > 0f)
            {
                shakeTimeLeft -= Time.unscaledDeltaTime;
                float fade = Mathf.Clamp01(shakeTimeLeft / 0.14f);
                float multiplier = ReducedFx ? 0.25f : 1f;
                Vector2 offset = Random.insideUnitCircle * shakeStrength * fade * multiplier;
                targetCamera.transform.position = cameraBasePosition + new Vector3(offset.x, offset.y, 0f);
            }
            else
            {
                targetCamera.transform.position = cameraBasePosition;
            }
        }

        public void PlayHit(Vector3 worldPosition, int combo)
        {
            Color color = combo >= 8
                ? new Color(1f, 0.28f, 0.10f)
                : combo >= 5
                    ? new Color(1f, 0.78f, 0.10f)
                    : new Color(0.18f, 0.92f, 1f);
            SpawnBurst(worldPosition, color, ReducedFx ? 0.45f : combo >= 8 ? 1.25f : combo >= 5 ? 0.95f : 0.72f, ReducedFx ? 0.14f : 0.24f);
            shakeTimeLeft = ReducedFx ? 0.025f : combo >= 8 ? 0.15f : combo >= 5 ? 0.11f : 0.07f;
            shakeStrength = ReducedFx ? 0.02f : combo >= 8 ? 0.16f : combo >= 5 ? 0.11f : 0.065f;
        }

        public void PlayMiss(Vector3 worldPosition)
        {
            SpawnBurst(worldPosition, new Color(0.42f, 0.45f, 0.52f), ReducedFx ? 0.34f : 0.58f, ReducedFx ? 0.12f : 0.20f);
        }

        private void SpawnBurst(Vector3 worldPosition, Color color, float scale, float duration)
        {
            EnsurePool();
            int index = nextBurst;
            nextBurst = (nextBurst + 1) % bursts.Length;
            SpriteRenderer burst = bursts[index];
            burst.gameObject.SetActive(true);
            burst.transform.position = new Vector3(worldPosition.x, worldPosition.y + 0.35f, -0.2f);
            burst.transform.localScale = Vector3.one * scale * 0.35f;
            burst.color = new Color(color.r, color.g, color.b, 0.95f);
            burstTimes[index] = duration;
            burstDurations[index] = duration;
        }

        private void UpdateBursts()
        {
            if (bursts == null)
                return;

            for (int i = 0; i < bursts.Length; i++)
            {
                if (burstTimes[i] <= 0f || bursts[i] == null || !bursts[i].gameObject.activeSelf)
                    continue;

                burstTimes[i] -= Time.unscaledDeltaTime;
                float duration = Mathf.Max(0.001f, burstDurations[i]);
                float t = 1f - Mathf.Clamp01(burstTimes[i] / duration);
                Vector3 start = Vector3.one * 0.35f;
                Vector3 end = Vector3.one * (ReducedFx ? 1.4f : 2.2f);
                bursts[i].transform.localScale = Vector3.Lerp(start, end, t);
                Color c = bursts[i].color;
                c.a = Mathf.Lerp(0.9f, 0f, t);
                bursts[i].color = c;
                if (burstTimes[i] <= 0f)
                    bursts[i].gameObject.SetActive(false);
            }
        }

        private void EnsurePool()
        {
            if (bursts != null && bursts.Length == BurstPoolSize)
                return;

            Kaninbanker2DAssetCatalog catalog = Kaninbanker2DAssetCatalog.Load();
            bursts = new SpriteRenderer[BurstPoolSize];
            burstTimes = new float[BurstPoolSize];
            burstDurations = new float[BurstPoolSize];
            for (int i = 0; i < BurstPoolSize; i++)
            {
                GameObject go = new GameObject("Feedback2D_" + i.ToString("00"));
                go.transform.SetParent(transform, false);
                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                Sprite imported = catalog != null ? catalog.PickEffect(i * 17 + 5) : null;
                sr.sprite = imported != null ? imported : Kaninbanker2DArt.Circle;
                sr.sortingOrder = 40;
                go.SetActive(false);
                bursts[i] = sr;
            }
        }
    }
}
