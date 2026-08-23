using System;
using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// Lightweight procedural audio for the phone-first prototype.
    /// All clips are generated once at runtime so the repository has no external audio dependency.
    /// </summary>
    public sealed class KaninbankerAudio : MonoBehaviour
    {
        private const string MuteKey = "Kaninbanker.AudioMuted";
        private const int SampleRate = 22050;

        private AudioSource musicSource;
        private AudioSource sfxSource;
        private AudioClip hitClip;
        private AudioClip comboClip;
        private AudioClip missClip;
        private AudioClip popClip;
        private AudioClip startClip;
        private AudioClip gameOverClip;
        private AudioClip uiClip;
        private AudioClip musicClip;
        private bool muted;

        public bool IsMuted => muted;

        private void Awake()
        {
            muted = PlayerPrefs.GetInt(MuteKey, 0) != 0;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            musicSource.volume = 0.20f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = 0.88f;

            hitClip = CreateTone("SFX_Bonk", 0.11f, t =>
            {
                float body = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(185f, 92f, t) * t);
                float click = Mathf.Sin(2f * Mathf.PI * 620f * t) * Mathf.Exp(-30f * t);
                return (body * 0.82f + click * 0.18f) * Mathf.Exp(-15f * t);
            });

            comboClip = CreateTone("SFX_Combo", 0.16f, t =>
            {
                float f = t < 0.5f ? 520f : 780f;
                return Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Exp(-7f * t);
            });

            missClip = CreateTone("SFX_Miss", 0.15f, t =>
            {
                float f = Mathf.Lerp(180f, 85f, t);
                float wobble = Mathf.Sin(2f * Mathf.PI * 7f * t) * 0.25f;
                return Mathf.Sin(2f * Mathf.PI * f * t + wobble) * Mathf.Exp(-8f * t) * 0.55f;
            });

            popClip = CreateTone("SFX_RabbitPop", 0.08f, t =>
            {
                float f = Mathf.Lerp(420f, 690f, t);
                return Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Exp(-22f * t) * 0.34f;
            });

            startClip = CreateArpeggio("SFX_Start", new[] { 330f, 440f, 660f }, 0.11f, 0.62f);
            gameOverClip = CreateArpeggio("SFX_GameOver", new[] { 392f, 294f, 196f }, 0.16f, 0.62f);
            uiClip = CreateTone("SFX_UI", 0.055f, t => Mathf.Sin(2f * Mathf.PI * 880f * t) * Mathf.Exp(-34f * t) * 0.42f);
            musicClip = CreateMusicLoop();
            musicSource.clip = musicClip;

            ApplyMuteState();
        }

        public void StartMusic()
        {
            if (!musicSource.isPlaying)
                musicSource.Play();
        }

        public void PlayRoundStart()
        {
            Play(startClip, 1f, 1f);
        }

        public void PlayGameOver()
        {
            Play(gameOverClip, 0.95f, 1f);
        }

        public void PlayRabbitPop(float difficulty01)
        {
            Play(popClip, 0.30f, Mathf.Lerp(0.94f, 1.18f, difficulty01));
        }

        public void PlayHit(int combo)
        {
            float pitch = Mathf.Clamp(0.96f + Mathf.Min(combo, 8) * 0.035f, 0.96f, 1.26f);
            Play(hitClip, 0.92f, pitch);
            if (combo >= 3 && combo % 3 == 0)
                Play(comboClip, 0.52f, Mathf.Clamp(0.9f + combo * 0.02f, 0.9f, 1.25f));
        }

        public void PlayMiss()
        {
            Play(missClip, 0.42f, 1f);
        }

        public void PlayUi()
        {
            Play(uiClip, 0.42f, 1f);
        }

        public void ToggleMute()
        {
            muted = !muted;
            PlayerPrefs.SetInt(MuteKey, muted ? 1 : 0);
            PlayerPrefs.Save();
            ApplyMuteState();
        }

        private void ApplyMuteState()
        {
            musicSource.mute = muted;
            sfxSource.mute = muted;
        }

        private void Play(AudioClip clip, float volume, float pitch)
        {
            if (clip == null || muted)
                return;

            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, volume);
        }

        private static AudioClip CreateTone(string name, float duration, Func<float, float> sample)
        {
            int count = Mathf.Max(1, Mathf.CeilToInt(duration * SampleRate));
            var data = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)count;
                data[i] = Mathf.Clamp(sample(t), -1f, 1f);
            }

            AudioClip clip = AudioClip.Create(name, count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateArpeggio(string name, float[] notes, float noteSeconds, float gain)
        {
            int noteSamples = Mathf.CeilToInt(noteSeconds * SampleRate);
            int count = noteSamples * notes.Length;
            var data = new float[count];

            for (int n = 0; n < notes.Length; n++)
            {
                for (int i = 0; i < noteSamples; i++)
                {
                    float t = i / (float)SampleRate;
                    float local01 = i / (float)noteSamples;
                    float env = Mathf.Sin(Mathf.PI * Mathf.Clamp01(local01)) * Mathf.Exp(-2.2f * local01);
                    data[n * noteSamples + i] = Mathf.Sin(2f * Mathf.PI * notes[n] * t) * env * gain;
                }
            }

            AudioClip clip = AudioClip.Create(name, count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateMusicLoop()
        {
            const float seconds = 8f;
            int count = Mathf.CeilToInt(seconds * SampleRate);
            var data = new float[count];
            float[] melody = { 261.63f, 329.63f, 392f, 329.63f, 293.66f, 349.23f, 440f, 349.23f };
            float[] bass = { 130.81f, 130.81f, 146.83f, 146.83f, 110f, 110f, 130.81f, 130.81f };

            for (int i = 0; i < count; i++)
            {
                float time = i / (float)SampleRate;
                int step = Mathf.FloorToInt(time) % melody.Length;
                float local = time - Mathf.Floor(time);
                float gate = local < 0.72f ? 1f : Mathf.Lerp(1f, 0f, (local - 0.72f) / 0.28f);

                // Mild square-ish lead plus sine bass; intentionally simple and lightweight.
                float leadSine = Mathf.Sin(2f * Mathf.PI * melody[step] * time);
                float lead = Mathf.Sign(leadSine) * 0.12f + leadSine * 0.08f;
                float low = Mathf.Sin(2f * Mathf.PI * bass[step] * time) * 0.10f;
                float tick = (local < 0.035f ? UnityEngine.Random.Range(-1f, 1f) * (1f - local / 0.035f) : 0f) * 0.035f;
                data[i] = Mathf.Clamp((lead * gate) + low + tick, -0.28f, 0.28f);
            }

            AudioClip clip = AudioClip.Create("Music_KaninbankerLoop", count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
