using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Reusable visual feedback: pooled hit bursts plus small camera shake.
    /// </summary>
    public sealed class KaninbankerFeedback : MonoBehaviour
    {
        private const int BurstPoolSize = 5;

        private Camera targetCamera;
        private Vector3 cameraBasePosition;
        private ParticleSystem[] bursts;
        private int nextBurst;
        private float shakeTimeLeft;
        private float shakeStrength;

        public void Configure(Camera cameraToShake)
        {
            targetCamera = cameraToShake;
            if (targetCamera != null)
                cameraBasePosition = targetCamera.transform.position;
            EnsurePool();
        }

        private void LateUpdate()
        {
            if (targetCamera == null)
                return;

            if (shakeTimeLeft > 0f)
            {
                shakeTimeLeft -= Time.unscaledDeltaTime;
                float fade = Mathf.Clamp01(shakeTimeLeft / 0.12f);
                Vector2 offset = Random.insideUnitCircle * shakeStrength * fade;
                targetCamera.transform.position = cameraBasePosition + new Vector3(offset.x, offset.y, 0f);
            }
            else if (targetCamera.transform.position != cameraBasePosition)
            {
                targetCamera.transform.position = cameraBasePosition;
            }
        }

        public void PlayHit(Vector3 worldPosition, int combo)
        {
            EnsurePool();
            if (bursts.Length > 0)
            {
                ParticleSystem burst = bursts[nextBurst];
                nextBurst = (nextBurst + 1) % bursts.Length;
                burst.transform.position = worldPosition + Vector3.up * 0.65f;

                var main = burst.main;
                main.startColor = combo >= 5
                    ? new ParticleSystem.MinMaxGradient(new Color(1f, 0.78f, 0.12f), new Color(1f, 0.32f, 0.16f))
                    : new ParticleSystem.MinMaxGradient(new Color(0.45f, 0.95f, 0.55f), new Color(0.25f, 0.72f, 1f));
                main.startSize = combo >= 5 ? 0.28f : 0.22f;
                burst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                burst.Play(true);
            }

            shakeTimeLeft = combo >= 5 ? 0.12f : 0.08f;
            shakeStrength = combo >= 5 ? 0.14f : 0.08f;
        }

        public void PlayMiss(Vector3 worldPosition)
        {
            EnsurePool();
            if (bursts.Length == 0)
                return;

            ParticleSystem burst = bursts[nextBurst];
            nextBurst = (nextBurst + 1) % bursts.Length;
            burst.transform.position = worldPosition + Vector3.up * 0.4f;
            var main = burst.main;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.55f, 0.55f, 0.60f), new Color(0.28f, 0.28f, 0.32f));
            main.startSize = 0.14f;
            burst.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            burst.Play(true);
        }

        private void EnsurePool()
        {
            if (bursts != null && bursts.Length == BurstPoolSize)
                return;

            bursts = new ParticleSystem[BurstPoolSize];
            for (int i = 0; i < bursts.Length; i++)
            {
                GameObject go = new GameObject("FeedbackBurst_" + i.ToString("00"));
                go.transform.SetParent(transform, false);
                var system = go.AddComponent<ParticleSystem>();
                var main = system.main;
                main.duration = 0.35f;
                main.loop = false;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.18f, 0.34f);
                main.startSpeed = new ParticleSystem.MinMaxCurve(2.6f, 4.2f);
                main.maxParticles = 20;
                main.simulationSpace = ParticleSystemSimulationSpace.World;
                main.stopAction = ParticleSystemStopAction.None;

                var emission = system.emission;
                emission.enabled = false;
                emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 12, 18) });

                var shape = system.shape;
                shape.enabled = true;
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.12f;

                var sizeOverLifetime = system.sizeOverLifetime;
                sizeOverLifetime.enabled = true;
                AnimationCurve sizeCurve = new AnimationCurve(
                    new Keyframe(0f, 1f),
                    new Keyframe(0.65f, 0.72f),
                    new Keyframe(1f, 0f));
                sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

                system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                bursts[i] = system;
            }
        }
    }
}
