using System.Collections.Generic;
using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Flat 2D atmosphere for the portrait arena. Uses SpriteRenderer only: no meshes,
    /// cylinders, spheres, cubes, 3D lights or perspective depth.
    /// </summary>
    public sealed class KaninbankerAtmosphere : MonoBehaviour
    {
        private readonly List<SpriteRenderer> floaters = new List<SpriteRenderer>();
        private readonly List<Vector3> basePositions = new List<Vector3>();
        private KaninbankerGame2D game;
        private float phase;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerAtmosphere>() != null)
                return;

            GameObject go = new GameObject("KaninbankerAtmosphere2D");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerAtmosphere>();
        }

        private void Start()
        {
            game = FindFirstObjectByType<KaninbankerGame2D>();
            Build2DAtmosphere();
        }

        private void Update()
        {
            if (game == null)
                game = FindFirstObjectByType<KaninbankerGame2D>();

            phase += Time.unscaledDeltaTime;
            bool reduced = PlayerPrefs.GetInt(KaninbankerSettingsPanel.ReducedFxKey, 0) != 0;
            for (int i = 0; i < floaters.Count; i++)
            {
                SpriteRenderer sr = floaters[i];
                if (sr == null) continue;
                Vector3 p = basePositions[i];
                p.y += Mathf.Sin(phase * (0.75f + i * 0.025f) + i * 0.63f) * (reduced ? 0.035f : 0.11f);
                p.x += Mathf.Cos(phase * 0.38f + i) * (reduced ? 0.015f : 0.05f);
                sr.transform.localPosition = p;
                float pulse = 0.78f + 0.22f * Mathf.Sin(phase * 1.7f + i * 0.91f);
                Color c = sr.color;
                c.a = reduced ? 0.34f : Mathf.Clamp01(0.45f * pulse + 0.30f);
                sr.color = c;
            }
        }

        private void Build2DAtmosphere()
        {
            for (int side = -1; side <= 1; side += 2)
            {
                for (int row = 0; row < 8; row++)
                {
                    float y = -6.6f + row * 1.85f;
                    GameObject orb = new GameObject("Atmosphere2D_" + side + "_" + row);
                    orb.transform.SetParent(transform, false);
                    orb.transform.localPosition = new Vector3(side * 3.85f, y, 0f);
                    orb.transform.localScale = Vector3.one * (row % 3 == 0 ? 0.28f : 0.16f);
                    SpriteRenderer sr = orb.AddComponent<SpriteRenderer>();
                    sr.sprite = Kaninbanker2DArt.Circle;
                    sr.sortingOrder = -3;
                    sr.color = row % 2 == 0
                        ? new Color(1f, 0.26f, 0.72f, 0.65f)
                        : new Color(0.16f, 0.82f, 1f, 0.62f);
                    floaters.Add(sr);
                    basePositions.Add(orb.transform.localPosition);
                }
            }

            for (int i = 0; i < 18; i++)
            {
                GameObject spark = new GameObject("Spark2D_" + i.ToString("00"));
                spark.transform.SetParent(transform, false);
                float x = Mathf.Lerp(-3.4f, 3.4f, (i % 6) / 5f);
                float y = -6.5f + (i / 6) * 5.8f + (i % 2) * 0.65f;
                spark.transform.localPosition = new Vector3(x, y, 0f);
                spark.transform.localScale = Vector3.one * (0.045f + (i % 4) * 0.012f);
                SpriteRenderer sr = spark.AddComponent<SpriteRenderer>();
                sr.sprite = Kaninbanker2DArt.Square;
                sr.sortingOrder = -2;
                sr.color = i % 2 == 0
                    ? new Color(1f, 0.16f, 0.82f, 0.55f)
                    : new Color(0.12f, 0.85f, 1f, 0.48f);
                floaters.Add(sr);
                basePositions.Add(spark.transform.localPosition);
            }
        }
    }
}
