using System.Collections.Generic;
using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Keeps the portrait arena visually alive without introducing any 3D rendering or physics.
    /// Only decorative SpriteRenderer transforms are animated. Gameplay holes/colliders stay fixed,
    /// so motion adds arcade energy without making taps less predictable.
    /// </summary>
    public sealed class Kaninbanker2DMotionDirector : MonoBehaviour
    {
        private enum MotionKind
        {
            Orb,
            Stripe,
            Lane,
            Background,
            Foreground,
            Effect
        }

        private sealed class MotionNode
        {
            public Transform transform;
            public Vector3 basePosition;
            public Vector3 baseScale;
            public Quaternion baseRotation;
            public MotionKind kind;
            public float phase;
            public float speed;
            public float amplitude;
        }

        private readonly List<MotionNode> nodes = new List<MotionNode>();
        private KaninbankerGame2D game;
        private bool bound;
        private float retryTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<Kaninbanker2DMotionDirector>() != null)
                return;

            GameObject go = new GameObject("Kaninbanker2DMotionDirector");
            DontDestroyOnLoad(go);
            go.AddComponent<Kaninbanker2DMotionDirector>();
        }

        private void Start()
        {
            TryBind();
        }

        private void Update()
        {
            if (!bound)
            {
                retryTimer -= Time.unscaledDeltaTime;
                if (retryTimer <= 0f)
                {
                    retryTimer = 0.5f;
                    TryBind();
                }
                return;
            }

            Animate(Time.unscaledTime);
        }

        private void TryBind()
        {
            game = FindFirstObjectByType<KaninbankerGame2D>();
            if (game == null)
                return;

            SpriteRenderer[] renderers = FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None);
            nodes.Clear();

            for (int i = 0; i < renderers.Length; i++)
            {
                SpriteRenderer renderer = renderers[i];
                if (renderer == null || renderer.transform == null)
                    continue;

                Transform t = renderer.transform;
                string name = t.name;
                MotionKind kind;
                float speed;
                float amplitude;

                if (name.StartsWith("2D_SideOrb_"))
                {
                    kind = MotionKind.Orb;
                    speed = 1.25f + (i % 5) * 0.13f;
                    amplitude = 0.18f + (i % 3) * 0.035f;
                }
                else if (name.StartsWith("2D_Stripe_"))
                {
                    kind = MotionKind.Stripe;
                    speed = 0.62f + (i % 4) * 0.08f;
                    amplitude = 0.12f;
                }
                else if (name == "2D_CenterLane")
                {
                    kind = MotionKind.Lane;
                    speed = 0.48f;
                    amplitude = 0.018f;
                }
                else if (name == "2D_Background" || name == "Imported2D_Background")
                {
                    kind = MotionKind.Background;
                    speed = 0.28f;
                    amplitude = 0.008f;
                }
                else if (name.Contains("Foreground") || name.Contains("Overlay"))
                {
                    kind = MotionKind.Foreground;
                    speed = 0.40f;
                    amplitude = 0.025f;
                }
                else if (name.Contains("FX") || name.Contains("Effect") || name.Contains("Spark"))
                {
                    kind = MotionKind.Effect;
                    speed = 1.7f;
                    amplitude = 0.06f;
                }
                else
                {
                    continue;
                }

                nodes.Add(new MotionNode
                {
                    transform = t,
                    basePosition = t.localPosition,
                    baseScale = t.localScale,
                    baseRotation = t.localRotation,
                    kind = kind,
                    phase = (i * 0.731f) % (Mathf.PI * 2f),
                    speed = speed,
                    amplitude = amplitude
                });
            }

            bound = nodes.Count > 0;
            if (bound)
                Debug.Log("[Kaninbanker][2DMotion] Bound " + nodes.Count + " decorative TRUE-2D motion nodes.");
        }

        private void Animate(float time)
        {
            bool reducedFx = PlayerPrefs.GetInt(KaninbankerSettingsPanel.ReducedFxKey, 0) != 0;
            float fxScale = reducedFx ? 0.28f : 1f;
            float gameplayBoost = game != null && game.IsRunning ? 1f : 0.72f;

            for (int i = 0; i < nodes.Count; i++)
            {
                MotionNode node = nodes[i];
                if (node == null || node.transform == null)
                    continue;

                float wave = Mathf.Sin(time * node.speed + node.phase);
                float wave2 = Mathf.Cos(time * node.speed * 0.73f + node.phase * 1.37f);
                float amount = node.amplitude * fxScale * gameplayBoost;

                switch (node.kind)
                {
                    case MotionKind.Orb:
                    {
                        node.transform.localPosition = node.basePosition + new Vector3(wave2 * amount * 0.45f, wave * amount, 0f);
                        float pulse = 1f + wave2 * amount * 0.38f;
                        node.transform.localScale = node.baseScale * pulse;
                        node.transform.localRotation = node.baseRotation;
                        break;
                    }
                    case MotionKind.Stripe:
                    {
                        node.transform.localPosition = node.basePosition + new Vector3(wave * amount, 0f, 0f);
                        node.transform.localScale = node.baseScale;
                        node.transform.localRotation = node.baseRotation * Quaternion.Euler(0f, 0f, wave2 * 1.4f * fxScale);
                        break;
                    }
                    case MotionKind.Lane:
                    {
                        node.transform.localPosition = node.basePosition;
                        node.transform.localScale = new Vector3(
                            node.baseScale.x * (1f + wave * amount),
                            node.baseScale.y,
                            node.baseScale.z);
                        node.transform.localRotation = node.baseRotation;
                        break;
                    }
                    case MotionKind.Background:
                    {
                        float pulse = 1f + wave * amount;
                        node.transform.localPosition = node.basePosition;
                        node.transform.localScale = node.baseScale * pulse;
                        node.transform.localRotation = node.baseRotation;
                        break;
                    }
                    case MotionKind.Foreground:
                    {
                        node.transform.localPosition = node.basePosition + new Vector3(wave * amount * 0.45f, wave2 * amount, 0f);
                        node.transform.localScale = node.baseScale;
                        node.transform.localRotation = node.baseRotation;
                        break;
                    }
                    case MotionKind.Effect:
                    {
                        float pulse = 1f + wave * amount;
                        node.transform.localPosition = node.basePosition;
                        node.transform.localScale = node.baseScale * pulse;
                        node.transform.localRotation = node.baseRotation * Quaternion.Euler(0f, 0f, wave2 * 3f * fxScale);
                        break;
                    }
                }
            }
        }

        private void OnDisable()
        {
            RestoreBaseTransforms();
        }

        private void OnDestroy()
        {
            RestoreBaseTransforms();
        }

        private void RestoreBaseTransforms()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                MotionNode node = nodes[i];
                if (node == null || node.transform == null)
                    continue;

                node.transform.localPosition = node.basePosition;
                node.transform.localScale = node.baseScale;
                node.transform.localRotation = node.baseRotation;
            }
        }
    }
}
