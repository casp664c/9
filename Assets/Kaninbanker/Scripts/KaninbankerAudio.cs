using System;
using UnityEngine;

namespace Kaninbanker
{
    /// <summary>
    /// 2D arcade audio. Imported project AudioClips are preferred when the generated asset catalog
    /// contains matching categories; procedural synthesis remains a zero-dependency fallback.
    /// </summary>
    public sealed class KaninbankerAudio : MonoBehaviour
    {
        private const string MuteKey = "Kaninbanker.AudioMuted";
        private const string InternalMusicKey = "Kaninbanker.InternalMusicEnabled";
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
        private AudioClip powerClip;
        private AudioClip bombClip;
        private AudioClip bossClip;
        private AudioClip rewardClip;
        private AudioClip musicClip;
        private bool muted;
        private bool internalMusicEnabled;

        public bool IsMuted => muted;
        public bool InternalMusicEnabled => internalMusicEnabled;

        private void Awake()
        {
            muted = PlayerPrefs.GetInt(MuteKey, 0) != 0;
            internalMusicEnabled = PlayerPrefs.GetInt(InternalMusicKey, 1) != 0;

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.spatialBlend = 0f;
            musicSource.volume = 0.18f;

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = 0.90f;

            hitClip = CreateTone("SFX_Bonk", 0.12f, t =>
            {
                float body = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(210f, 88f, t) * t);
                float click = Mathf.Sin(2f * Mathf.PI * 730f * t) * Mathf.Exp(-34f * t);
                float crunch = Mathf.Sin(2f * Mathf.PI * 330f * t) * Mathf.Sin(2f * Mathf.PI * 17f * t) * 0.12f;
                return (body * 0.78f + click * 0.16f + crunch) * Mathf.Exp(-13f * t);
            });

            comboClip = CreateTone("SFX_Combo", 0.18f, t =>
            {
                float f = t < 0.33f ? 520f : (t < 0.66f ? 680f : 880f);
                return Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Exp(-6f * t) * 0.75f;
            });

            missClip = CreateTone("SFX_Miss", 0.17f, t =>
            {
                float f = Mathf.Lerp(190f, 72f, t);
                float wobble = Mathf.Sin(2f * Mathf.PI * 8f * t) * 0.28f;
                return Mathf.Sin(2f * Mathf.PI * f * t + wobble) * Mathf.Exp(-7f * t) * 0.58f;
            });

            popClip = CreateTone("SFX_RabbitPop", 0.085f, t =>
            {
                float f = Mathf.Lerp(390f, 760f, t);
                return Mathf.Sin(2f * Mathf.PI * f * t) * Mathf.Exp(-21f * t) * 0.36f;
            });

            powerClip = CreateArpeggio("SFX_Power", new[] { 440f, 660f, 880f, 1100f }, 0.075f, 0.58f);
            rewardClip = CreateArpeggio("SFX_Reward", new[] { 523.25f, 659.25f, 783.99f, 1046.5f }, 0.10f, 0.62f);
            startClip = CreateArpeggio("SFX_Start", new[] { 330f, 440f, 660f }, 0.11f, 0.62f);
            gameOverClip = CreateArpeggio("SFX_GameOver", new[] { 392f, 294f, 196f }, 0.16f, 0.62f);
            uiClip = CreateTone("SFX_UI", 0.055f, t => Mathf.Sin(2f * Mathf.PI * 880f * t) * Mathf.Exp(-34f * t) * 0.42f);

            bombClip = CreateTone("SFX_Bomb", 0.28f, t =>
            {
                float low = Mathf.Sin(2f * Mathf.PI * Mathf.Lerp(105f, 38f, t) * t);
                float mid = Mathf.Sin(2f * Mathf.PI * 215f * t) * Mathf.Exp(-9f * t);
                float grit = Mathf.Sin(2f * Mathf.PI * 1700f * t) * Mathf.Sin(2f * Mathf.PI * 23f * t);
                return (low * 0.74f + mid * 0.20f + grit * 0.06f) * Mathf.Exp(-5.2f * t);
            });

            bossClip = CreateTone("SFX_Boss", 0.20f, t =>
            {
                float f = Mathf.Lerp(145f, 72f, t);
                float fundamental = Mathf.Sin(2f * Mathf.PI * f * t);
                float harmonic = Mathf.Sin(2f * Mathf.PI * f * 2.01f * t) * 0.35f;
                return (fundamental + harmonic) * Mathf.Exp(-7f * t) * 0.70f;
            });

            musicClip = CreateMusicLoop();
            ApplyImportedAudioOverrides();
            musicSource.clip = musicClip;
            ApplyMuteState();
        }

        private void ApplyImportedAudioOverrides()
        {
            Kaninbanker2DAssetCatalog catalog = Kaninbanker2DAssetCatalog.Load();
            if (catalog == null)
                return;

            hitClip = catalog.PickHit(11) ?? hitClip;
            comboClip = catalog.PickCombo(13) ?? comboClip;
            missClip = catalog.PickMiss(17) ?? missClip;
            popClip = catalog.PickRabbitPop(19) ?? popClip;
            startClip = catalog.PickRoundStart(23) ?? startClip;
            gameOverClip = catalog.PickGameOver(29) ?? gameOverClip;
            uiClip = catalog.PickUiAudio(31) ?? uiClip;
            powerClip = catalog.PickPower(37) ?? powerClip;
            bombClip = catalog.PickBomb(41) ?? bombClip;
            bossClip = catalog.PickBoss(43) ?? bossClip;
            rewardClip = catalog.PickReward(47) ?? rewardClip;
            musicClip = catalog.PickMusic(53) ?? musicClip;
        }

        public void StartMusic()
        {
            if (!musicSource.isPlaying)
                musicSource.Play();
        }

        public void SetInternalMusicEnabled(bool enabled)
        {
            internalMusicEnabled = enabled;
            PlayerPrefs.SetInt(InternalMusicKey, enabled ? 1 : 0);
            PlayerPrefs.Save();
            ApplyMuteState();

            if (enabled && !musicSource.isPlaying)
                musicSource.Play();
        }

        public void PlayRoundStart() => Play(startClip, 1f, 1f);
        public void PlayGameOver() => Play(gameOverClip, 0.95f, 1f);
        public void PlayPowerUp() => Play(powerClip, 0.72f, 1f);
        public void PlayBomb() => Play(bombClip, 0.88f, 1f);
        public void PlayBossHit() => Play(bossClip, 0.68f, 1f);
        public void PlayReward() => Play(rewardClip, 0.75f, 1f);

        public void PlayRabbitPop(float difficulty01)
        {
            Play(popClip, 0.30f, Mathf.Lerp(0.94f, 1.20f, difficulty01));
        }

        public void PlayHit(int combo)
        {
            float pitch = Mathf.Clamp(0.96f + Mathf.Min(combo, 10) * 0.034f, 0.96f, 1.30f);
            Play(hitClip, 0.92f, pitch);
            if (combo >= 3 && combo % 3 == 0)
                Play(comboClip, 0.54f, Mathf.Clamp(0.9f + combo * 0.02f, 0.9f, 1.28f));
        }

        public void PlayMiss() => Play(missClip, 0.44f, 1f);
        public void PlayUi() => Play(uiClip, 0.42f, 1f);

        public void ToggleMute()
        {
            muted = !muted;
            PlayerPrefs.SetInt(MuteKey, muted ? 1 : 0);
            PlayerPrefs.Save();
            ApplyMuteState();
        }

        private void ApplyMuteState()
        {
            musicSource.mute = muted || !internalMusicEnabled;
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
            float[] data = new float[count];
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
            float[] data = new float[count];

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
            const float seconds = 12f;
            int count = Mathf.CeilToInt(seconds * SampleRate);
            float[] data = new float[count];
            float[] melody = { 261.63f, 329.63f, 392f, 523.25f, 293.66f, 349.23f, 440f, 587.33f, 329.63f, 392f, 493.88f, 659.25f };
            float[] bass = { 130.81f, 130.81f, 146.83f, 130.81f, 110f, 110f, 130.81f, 146.83f, 123.47f, 123.47f, 146.83f, 164.81f };

            for (int i = 0; i < count; i++)
            {
                float time = i / (float)SampleRate;
                int step = Mathf.FloorToInt(time) % melody.Length;
                float local = time - Mathf.Floor(time);
                float gate = local < 0.72f ? 1f : Mathf.Lerp(1f, 0f, (local - 0.72f) / 0.28f);

                float leadSine = Mathf.Sin(2f * Mathf.PI * melody[step] * time);
                float lead = Mathf.Sign(leadSine) * 0.10f + leadSine * 0.075f;
                float low = Mathf.Sin(2f * Mathf.PI * bass[step] * time) * 0.09f;
                float octave = Mathf.Sin(2f * Mathf.PI * melody[step] * 2f * time) * 0.025f * gate;
                float tickEnvelope = local < 0.032f ? 1f - local / 0.032f : 0f;
                float tick = Mathf.Sin(2f * Mathf.PI * 2800f * time) * tickEnvelope * 0.032f;
                float offBeat = local > 0.48f && local < 0.53f ? (1f - Mathf.Abs(local - 0.505f) / 0.025f) * 0.025f : 0f;
                data[i] = Mathf.Clamp((lead * gate) + low + octave + tick + offBeat, -0.30f, 0.30f);
            }

            AudioClip clip = AudioClip.Create("Music_KaninbankerMayhem", count, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}