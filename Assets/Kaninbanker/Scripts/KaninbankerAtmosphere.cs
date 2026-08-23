using System.Collections.Generic;
using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Adds lightweight procedural arena atmosphere around the portrait playfield: edge pylons,
    /// floating energy orbs, ambient particles and moving lights. Everything stays outside the
    /// central hit lanes and Reduced FX mode scales the expensive parts down.
    /// </summary>
    public sealed class KaninbankerAtmosphere : MonoBehaviour
    {
        private readonly List<Transform> floaters = new List<Transform>();
        private readonly List<Light> pulseLights = new List<Light>();
        private KaninbankerGame game;
        private ParticleSystem ambientParticles;
        private float phase;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            if (FindFirstObjectByType<KaninbankerAtmosphere>() != null)
                return;

            GameObject go = new GameObject("KaninbankerAtmosphere");
            DontDestroyOnLoad(go);
            go.AddComponent<KaninbankerAtmosphere>();
        }

        private void Start()
        {
            game = FindFirstObjectByType<KaninbankerGame>();
            BuildAtmosphere();
        }

        private void Update()
        {
            if (game == null)
                game = FindFirstObjectByType<KaninbankerGame>();

            phase += Time.unscaledDeltaTime;
            bool reduced = PlayerPrefs.GetInt(KaninbankerSettingsPanel.ReducedFxKey, 0) != 0;

            for (int i = 0; i < floaters.Count; i++)
            {
                Transform t = floaters[i];
                if (t == null) continue;
                float wave = Mathf.Sin(phase * (0.8f + i * 0.035f) + i * 0.7f);
                Vector3 p = t.localPosition;
                p.y = 1.6f + wave * (reduced ? 0.06f : 0.18f);
                t.localPosition = p;
                t.Rotate(0f, (reduced ? 10f : 28f) * Time.unscaledDeltaTime, 0f, Space.Self);
            }

            for (int i = 0; i < pulseLights.Count; i++)
            {
                Light light = pulseLights[i];
                if (light == null) continue;
                float pulse = 0.5f + 0.5f * Mathf.Sin(phase * 2.1f + i * 1.7f);
                light.intensity = reduced ? 0.15f : Mathf.Lerp(0.28f, 0.72f, pulse);
            }

            if (ambientParticles != null)
            {
                var emission = ambientParticles.emission;
                emission.rateOverTime = reduced ? 4f : game != null && game.IsRunning ? 20f : 10f;
            }
        }

        private void BuildAtmosphere()
        {
            Transform root = transform;

            for (int side = -1; side <= 1; side += 2)
            {
                for (int row = 0; row < 7; row++)
                {
                    float z = -5.8f + row * 1.9f;
                    GameObject pylon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    pylon.name = "EnergyPylon_" + side + "_" + row;
                    pylon.transform.SetParent(root, false);
                    pylon.transform.localPosition = new Vector3(side * 5.15f, 0.7f, z);
                    pylon.transform.localScale = new Vector3(0.20f, 0.70f, 0.20f);
                    RemoveCollider(pylon);
                    SetColor(pylon, row % 2 == 0 ? new Color(0.18f, 0.10f, 0.30f) : new Color(0.12f, 0.20f, 0.32f));

                    GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    orb.name = "EnergyOrb_" + side + "_" + row;
                    orb.transform.SetParent(root, false);
                    orb.transform.localPosition = new Vector3(side * 5.15f, 1.6f, z);
                    orb.transform.localScale = Vector3.one * (row % 3 == 0 ? 0.52f : 0.36f);
                    RemoveCollider(orb);
                    SetColor(orb, row % 2 == 0 ? new Color(1f, 0.42f, 0.08f) : new Color(0.55f, 0.18f, 1f));
                    floaters.Add(orb.transform);

                    if (row % 3 == 0)
                    {
                        Light light = orb.AddComponent<Light>();
                        light.type = LightType.Point;
                        light.range = 4.0f;
                        light.intensity = 0.45f;
                        light.color = row % 2 == 0 ? new Color(1f, 0.35f, 0.08f) : new Color(0.50f, 0.18f, 1f);
                        pulseLights.Add(light);
                    }
                }
            }

            for (int i = 0; i < 6; i++)
            {
                GameObject arch = GameObject.CreatePrimitive(PrimitiveType.Cube);
                arch.name = "SkyArch_" + i;
                arch.transform.SetParent(root, false);
                arch.transform.localPosition = new Vector3(0f, 2.2f + i * 0.12f, -7.4f + i * 2.9f);
                arch.transform.localScale = new Vector3(10.8f, 0.10f, 0.10f);
                arch.transform.localRotation = Quaternion.Euler(0f, 0f, i % 2 == 0 ? 4f : -4f);
                RemoveCollider(arch);
                SetColor(arch, i % 2 == 0 ? new Color(0.42f, 0.10f, 0.65f) : new Color(0.72f, 0.22f, 0.04f));
            }

            GameObject ambient = new GameObject("AmbientSparkField");
            ambient.transform.SetParent(root, false);
            ambient.transform.localPosition = new Vector3(0f, 2.4f, 0f);
            ambientParticles = ambient.AddComponent<ParticleSystem>();

            var main = ambientParticles.main;
            main.loop = true;
            main.duration = 4f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(2.8f, 5.2f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.08f, 0.32f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.035f, 0.10f);
            main.maxParticles = 100;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.52f, 0.12f, 0.72f),
                new Color(0.48f, 0.20f, 1f, 0.66f));

            var emission = ambientParticles.emission;
            emission.rateOverTime = 10f;

            var shape = ambientParticles.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(10f, 4.5f, 13.5f);

            var velocity = ambientParticles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.y = new ParticleSystem.MinMaxCurve(0.10f, 0.32f);

            ambientParticles.Play(true);
        }

        private static void RemoveCollider(GameObject go)
        {
            Collider collider = go.GetComponent<Collider>();
            if (collider != null)
                Destroy(collider);
        }

        private static void SetColor(GameObject go, Color color)
        {
            Renderer renderer = go.GetComponent<Renderer>();
            if (renderer == null) return;
            Shader shader = Shader.Find("Standard") ?? Shader.Find("Sprites/Default");
            if (shader == null) return;
            Material material = new Material(shader) { color = color };
            renderer.sharedMaterial = material;
        }
    }
}
