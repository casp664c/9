using System.Collections.Generic;
using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// TRUE-2D boss phase presentation. Bosses gain stronger 2D motion and aura feedback as HP falls.
    /// No 3D lights, meshes, colliders or perspective effects are used.
    /// </summary>
    public sealed class KaninbankerBossPhases2D : MonoBehaviour
    {
        private sealed class BossVisual
        {
            public KaninbankerHole2D hole;
            public Transform rabbit;
            public Vector3 baseLocalPosition;
            public SpriteRenderer[] rings;
            public int phase;
        }

        private readonly Dictionary<int, BossVisual> visuals = new Dictionary<int, BossVisual>();
        private float scanTimer;
        private bool reducedFx;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerBossPhases2D>() != null)
                return;

            GameObject go = new GameObject("KaninbankerBossPhases2D");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerBossPhases2D>();
        }

        private void Update()
        {
            reducedFx = PlayerPrefs.GetInt(KaninbankerSettingsPanel.ReducedFxKey, 0) != 0;
            scanTimer -= Time.unscaledDeltaTime;
            if (scanTimer <= 0f)
            {
                scanTimer = 0.20f;
                EnsureBindings();
            }

            float t = Time.unscaledTime;
            foreach (BossVisual visual in visuals.Values)
                AnimateBoss(visual, t);
        }

        private void EnsureBindings()
        {
            KaninbankerHole2D[] holes = FindObjectsByType<KaninbankerHole2D>(FindObjectsSortMode.None);
            for (int i = 0; i < holes.Length; i++)
            {
                KaninbankerHole2D hole = holes[i];
                if (hole == null)
                    continue;

                int id = hole.GetInstanceID();
                if (visuals.ContainsKey(id))
                    continue;

                Transform rabbit = hole.transform.Find("Rabbit2D");
                if (rabbit == null)
                    continue;

                BossVisual visual = new BossVisual
                {
                    hole = hole,
                    rabbit = rabbit,
                    baseLocalPosition = rabbit.localPosition,
                    rings = CreateRings(rabbit),
                    phase = 0
                };
                visuals.Add(id, visual);
            }
        }

        private static SpriteRenderer[] CreateRings(Transform parent)
        {
            SpriteRenderer[] rings = new SpriteRenderer[3];
            for (int i = 0; i < rings.Length; i++)
            {
                GameObject go = new GameObject("BossPhase2D_Ring_" + (i + 1));
                go.transform.SetParent(parent, false);
                go.transform.localPosition = new Vector3(0f, 0.48f, 0f);
                SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
                renderer.sprite = Kaninbanker2DArt.Circle;
                renderer.sortingOrder = 9 + i;
                renderer.color = new Color(1f, 0.14f, 0.12f, 0f);
                rings[i] = renderer;
            }
            return rings;
        }

        private void AnimateBoss(BossVisual visual, float time)
        {
            if (visual == null || visual.hole == null || visual.rabbit == null)
                return;

            bool activeBoss = visual.hole.IsVisible && visual.hole.Kind == RabbitKind2D.Boss;
            if (!activeBoss)
            {
                visual.phase = 0;
                visual.rabbit.localPosition = visual.baseLocalPosition;
                visual.rabbit.localRotation = Quaternion.identity;
                SetRingsVisible(visual, false, 0, time);
                return;
            }

            float hp01 = visual.hole.MaxHealth <= 0 ? 0f : Mathf.Clamp01(visual.hole.Health / (float)visual.hole.MaxHealth);
            int phase = hp01 > 0.66f ? 1 : hp01 > 0.33f ? 2 : 3;
            visual.phase = phase;

            float motionScale = reducedFx ? 0.35f : 1f;
            float speed = (1.8f + phase * 0.75f) * motionScale;
            float bob = Mathf.Sin(time * speed + visual.hole.GetInstanceID() * 0.01f) * (0.025f + phase * 0.020f) * motionScale;
            float angle = Mathf.Sin(time * (speed + 0.65f)) * (1.5f + phase * 1.7f) * motionScale;
            visual.rabbit.localPosition = visual.baseLocalPosition + new Vector3(0f, bob, 0f);
            visual.rabbit.localRotation = Quaternion.Euler(0f, 0f, angle);
            SetRingsVisible(visual, true, phase, time);
        }

        private void SetRingsVisible(BossVisual visual, bool visible, int phase, float time)
        {
            if (visual.rings == null)
                return;

            for (int i = 0; i < visual.rings.Length; i++)
            {
                SpriteRenderer ring = visual.rings[i];
                if (ring == null)
                    continue;

                if (!visible)
                {
                    Color hidden = ring.color;
                    hidden.a = 0f;
                    ring.color = hidden;
                    continue;
                }

                float phaseStrength = reducedFx ? 0.45f : 1f;
                float pulse = 1f + Mathf.Sin(time * (1.7f + i * 0.42f + phase * 0.55f)) * 0.08f * phaseStrength;
                float baseScale = 1.55f + i * 0.34f + phase * 0.13f;
                ring.transform.localScale = Vector3.one * baseScale * pulse;
                ring.transform.localRotation = Quaternion.Euler(0f, 0f, time * (8f + phase * 6f) * (i % 2 == 0 ? 1f : -1f) * phaseStrength);

                Color color;
                if (phase == 1) color = new Color(1f, 0.18f, 0.10f, 0.055f + i * 0.012f);
                else if (phase == 2) color = new Color(1f, 0.56f, 0.08f, 0.075f + i * 0.015f);
                else color = new Color(0.86f, 0.12f, 1f, 0.10f + i * 0.018f);
                if (reducedFx) color.a *= 0.45f;
                ring.color = color;
            }
        }
    }
}
