using System.Collections.Generic;
using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Bridges approved production sprites into the code-driven TRUE-2D prototype.
    /// If a type-specific sprite exists in Resources/KaninbankerArt2D it replaces the
    /// procedural composite rabbit. If it is missing, the safe procedural fallback remains.
    /// No 3D model/material path exists here.
    /// </summary>
    public sealed class KaninbankerProductionArt2D : MonoBehaviour
    {
        private sealed class RabbitVisual
        {
            public KaninbankerHole2D hole;
            public SpriteRenderer overlay;
            public SpriteRenderer[] fallbackParts;
            public RabbitKind2D lastKind = (RabbitKind2D)(-1);
            public Sprite lastSprite;
        }

        private readonly List<RabbitVisual> visuals = new List<RabbitVisual>();
        private KaninbankerGame2D game;
        private float refreshTimer;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerProductionArt2D>() != null)
                return;

            GameObject go = new GameObject("KaninbankerProductionArt2D");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerProductionArt2D>();
        }

        private void Start()
        {
            ResolveAndBuild();
        }

        private void Update()
        {
            if (game == null)
            {
                ResolveAndBuild();
                return;
            }

            refreshTimer -= Time.unscaledDeltaTime;
            if (refreshTimer > 0f)
                return;

            refreshTimer = 0.10f;
            RefreshRabbitArt();
        }

        private void ResolveAndBuild()
        {
            game = FindFirstObjectByType<KaninbankerGame2D>();
            if (game == null)
                return;

            visuals.Clear();
            KaninbankerHole2D[] holes = game.GetComponentsInChildren<KaninbankerHole2D>(true);
            for (int i = 0; i < holes.Length; i++)
            {
                Transform rabbit = FindChildRecursive(holes[i].transform, "Rabbit2D");
                if (rabbit == null)
                    continue;

                SpriteRenderer[] fallback = rabbit.GetComponentsInChildren<SpriteRenderer>(true);
                GameObject overlayGo = new GameObject("ProductionRabbitSprite2D");
                overlayGo.transform.SetParent(rabbit, false);
                overlayGo.transform.localPosition = new Vector3(0f, 0.90f, -0.1f);
                overlayGo.transform.localScale = Vector3.one;

                SpriteRenderer overlay = overlayGo.AddComponent<SpriteRenderer>();
                overlay.sortingOrder = 30;
                overlay.enabled = false;

                visuals.Add(new RabbitVisual
                {
                    hole = holes[i],
                    overlay = overlay,
                    fallbackParts = fallback
                });
            }

            RefreshRabbitArt();
        }

        private void RefreshRabbitArt()
        {
            for (int i = 0; i < visuals.Count; i++)
            {
                RabbitVisual visual = visuals[i];
                if (visual.hole == null || visual.overlay == null)
                    continue;

                if (!visual.hole.IsVisible)
                    continue;

                RabbitKind2D kind = visual.hole.Kind;
                if (kind != visual.lastKind || visual.lastSprite == null)
                {
                    visual.lastKind = kind;
                    visual.lastSprite = LoadRabbitSprite(kind);
                    ApplyVisual(visual, visual.lastSprite);
                }
            }
        }

        private static Sprite LoadRabbitSprite(RabbitKind2D kind)
        {
            string folder;
            switch (kind)
            {
                case RabbitKind2D.Fast: folder = "Fast"; break;
                case RabbitKind2D.Gold: folder = "Gold"; break;
                case RabbitKind2D.Armored: folder = "Armored"; break;
                case RabbitKind2D.Bomb: folder = "Bomb"; break;
                case RabbitKind2D.Boss: folder = "Boss"; break;
                default: folder = "Normal"; break;
            }

            return Resources.Load<Sprite>("KaninbankerArt2D/Rabbits/" + folder + "/Idle");
        }

        private static void ApplyVisual(RabbitVisual visual, Sprite sprite)
        {
            bool useProduction = sprite != null;
            visual.overlay.sprite = sprite;
            visual.overlay.enabled = useProduction;
            visual.overlay.color = Color.white;

            if (useProduction)
            {
                Bounds bounds = sprite.bounds;
                float height = Mathf.Max(0.01f, bounds.size.y);
                float targetHeight = visual.hole.Kind == RabbitKind2D.Boss ? 2.9f : 2.15f;
                float uniform = targetHeight / height;
                visual.overlay.transform.localScale = Vector3.one * uniform;
            }

            for (int i = 0; i < visual.fallbackParts.Length; i++)
            {
                if (visual.fallbackParts[i] != null)
                    visual.fallbackParts[i].enabled = !useProduction;
            }
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            if (root.name == childName)
                return root;

            for (int i = 0; i < root.childCount; i++)
            {
                Transform found = FindChildRecursive(root.GetChild(i), childName);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}
