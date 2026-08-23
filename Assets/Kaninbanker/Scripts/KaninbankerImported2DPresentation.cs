using System.Collections.Generic;
using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Presentation bridge between the generated TRUE-2D gameplay and imported project art.
    /// It never changes scoring/physics. If categorized imported sprites exist, they replace the
    /// procedural presentation; missing categories keep the deterministic procedural fallback.
    /// </summary>
    public sealed class KaninbankerImported2DPresentation : MonoBehaviour
    {
        private sealed class RabbitBinding
        {
            public KaninbankerHole2D hole;
            public SpriteRenderer imported;
            public SpriteRenderer[] procedural;
            public RabbitKind2D lastKind;
            public Sprite lastSprite;
            public int seed;
        }

        private readonly List<RabbitBinding> rabbits = new List<RabbitBinding>();
        private Kaninbanker2DAssetCatalog catalog;
        private KaninbankerGame2D game;
        private bool initialized;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerImported2DPresentation>() != null)
                return;
            GameObject go = new GameObject("KaninbankerImported2DPresentation");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerImported2DPresentation>();
        }

        private void Start()
        {
            TryInitialize();
        }

        private void Update()
        {
            if (!initialized)
            {
                TryInitialize();
                return;
            }

            for (int i = 0; i < rabbits.Count; i++)
                RefreshRabbit(rabbits[i]);
        }

        private void TryInitialize()
        {
            catalog = Kaninbanker2DAssetCatalog.Load();
            game = FindFirstObjectByType<KaninbankerGame2D>();
            if (catalog == null || game == null)
                return;

            ApplyBackground();
            BindHoles();
            initialized = true;
        }

        private void ApplyBackground()
        {
            Sprite sprite = catalog.PickBackground(0);
            if (sprite == null)
                return;

            GameObject go = new GameObject("Imported2D_Background");
            go.transform.SetParent(game.transform, false);
            go.transform.localPosition = Vector3.zero;
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = -19;
            renderer.color = Color.white;
            ScaleSpriteToWorldSize(renderer, new Vector2(8.2f, 17.8f), true);
        }

        private void BindHoles()
        {
            KaninbankerHole2D[] holes = FindObjectsByType<KaninbankerHole2D>(FindObjectsSortMode.None);
            for (int i = 0; i < holes.Length; i++)
            {
                KaninbankerHole2D hole = holes[i];
                Sprite holeSprite = catalog.PickHole(i);
                if (holeSprite != null)
                {
                    GameObject holeArt = new GameObject("Imported2D_HoleArt");
                    holeArt.transform.SetParent(hole.transform, false);
                    holeArt.transform.localPosition = new Vector3(0f, 0f, 0f);
                    SpriteRenderer holeRenderer = holeArt.AddComponent<SpriteRenderer>();
                    holeRenderer.sprite = holeSprite;
                    holeRenderer.sortingOrder = 4;
                    ScaleSpriteToWorldSize(holeRenderer, new Vector2(1.9f, 1.05f), false);
                }

                Transform rabbitRoot = hole.transform.Find("Rabbit2D");
                if (rabbitRoot == null)
                    continue;

                SpriteRenderer[] existing = rabbitRoot.GetComponentsInChildren<SpriteRenderer>(true);
                GameObject importedObject = new GameObject("Imported2D_RabbitArt");
                importedObject.transform.SetParent(rabbitRoot, false);
                importedObject.transform.localPosition = new Vector3(0f, 0.83f, 0f);
                SpriteRenderer imported = importedObject.AddComponent<SpriteRenderer>();
                imported.sortingOrder = 24;
                imported.enabled = false;

                rabbits.Add(new RabbitBinding
                {
                    hole = hole,
                    imported = imported,
                    procedural = existing,
                    seed = i * 37 + 11,
                    lastKind = (RabbitKind2D)(-1),
                    lastSprite = null
                });
            }
        }

        private void RefreshRabbit(RabbitBinding binding)
        {
            if (binding == null || binding.hole == null || binding.imported == null)
                return;

            bool visible = binding.hole.IsVisible;
            if (!visible)
            {
                binding.imported.enabled = false;
                SetProceduralEnabled(binding, true);
                return;
            }

            RabbitKind2D kind = binding.hole.Kind;
            Sprite sprite = PickExactRabbit(kind, binding.seed + (int)kind * 101);
            if (sprite == null)
            {
                binding.imported.enabled = false;
                SetProceduralEnabled(binding, true);
                binding.lastSprite = null;
                binding.lastKind = kind;
                return;
            }

            if (binding.lastSprite != sprite || binding.lastKind != kind)
            {
                binding.imported.sprite = sprite;
                ScaleSpriteToWorldSize(binding.imported, kind == RabbitKind2D.Boss ? new Vector2(2.10f, 2.85f) : new Vector2(1.55f, 2.10f), false);
                binding.lastSprite = sprite;
                binding.lastKind = kind;
            }

            SetProceduralEnabled(binding, false);
            binding.imported.enabled = true;
        }

        private Sprite PickExactRabbit(RabbitKind2D kind, int seed)
        {
            Sprite[] items;
            switch (kind)
            {
                case RabbitKind2D.Fast: items = catalog.fastRabbits; break;
                case RabbitKind2D.Gold: items = catalog.goldRabbits; break;
                case RabbitKind2D.Armored: items = catalog.armoredRabbits; break;
                case RabbitKind2D.Bomb: items = catalog.bombRabbits; break;
                case RabbitKind2D.Boss: items = catalog.bossRabbits; break;
                default: items = catalog.normalRabbits; break;
            }

            if (items == null || items.Length == 0)
                return null;
            int safe = seed == int.MinValue ? 0 : Mathf.Abs(seed);
            return items[safe % items.Length];
        }

        private static void SetProceduralEnabled(RabbitBinding binding, bool enabled)
        {
            if (binding.procedural == null)
                return;
            for (int i = 0; i < binding.procedural.Length; i++)
            {
                if (binding.procedural[i] != null)
                    binding.procedural[i].enabled = enabled;
            }
        }

        private static void ScaleSpriteToWorldSize(SpriteRenderer renderer, Vector2 targetSize, bool cover)
        {
            if (renderer == null || renderer.sprite == null)
                return;
            Vector2 bounds = renderer.sprite.bounds.size;
            if (bounds.x <= 0.0001f || bounds.y <= 0.0001f)
                return;

            float sx = targetSize.x / bounds.x;
            float sy = targetSize.y / bounds.y;
            if (cover)
            {
                float s = Mathf.Max(sx, sy);
                renderer.transform.localScale = new Vector3(s, s, 1f);
            }
            else
            {
                float s = Mathf.Min(sx, sy);
                renderer.transform.localScale = new Vector3(s, s, 1f);
            }
        }
    }
}
