using System.Collections.Generic;
using UnityEngine;

namespace StackUp
{
    public enum Sfx
    {
        Pick,
        Place,
        Scan,
        VerifyPass,
        VerifyFail,
        Warning,
        OrderComplete,
        Countdown
    }

    /// <summary>
    /// Persistent audio manager. Clips are synthesised at runtime (simple tones)
    /// so there are no audio assets to ship yet — real SFX replace these in M5/M6.
    /// Volume follows the player's settings. See CLAUDE_CODE_SPEC.md Section 22.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;

        private AudioSource source;
        private AudioSource ambience;
        private readonly Dictionary<Sfx, AudioClip> clips = new Dictionary<Sfx, AudioClip>();

        public static AudioManager Ensure()
        {
            if (instance != null) return instance;
            var go = new GameObject("AudioManager");
            DontDestroyOnLoad(go);
            instance = go.AddComponent<AudioManager>();
            return instance;
        }

        private void Awake()
        {
            if (instance != null && instance != this) { Destroy(gameObject); return; }
            instance = this;

            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            ambience = gameObject.AddComponent<AudioSource>();
            ambience.playOnAwake = false;
            ambience.loop = true;

            BuildClips();
        }

        private void BuildClips()
        {
            clips[Sfx.Pick] = Tone("pick", 660f, 0.08f, 0.5f);
            clips[Sfx.Place] = Tone("place", 440f, 0.08f, 0.5f);
            clips[Sfx.Scan] = Tone("scan", 880f, 0.06f, 0.4f);
            clips[Sfx.VerifyPass] = Tone("pass", 520f, 0.18f, 0.5f, false, 1040f);
            clips[Sfx.VerifyFail] = Tone("fail", 300f, 0.22f, 0.5f, true, 150f);
            clips[Sfx.Warning] = Tone("warn", 200f, 0.16f, 0.5f, true);
            clips[Sfx.OrderComplete] = Tone("complete", 523f, 0.25f, 0.55f, false, 1046f);
            clips[Sfx.Countdown] = Tone("count", 784f, 0.10f, 0.45f);
        }

        public static void Play(Sfx sfx)
        {
            var a = Ensure();
            if (!a.clips.TryGetValue(sfx, out var clip) || clip == null) return;
            a.source.PlayOneShot(clip, Volume());
        }

        public static void StartAmbience()
        {
            var a = Ensure();
            if (a.ambience.isPlaying) return;
            a.ambience.clip = a.Hum();
            a.ambience.volume = Volume() * 0.25f;
            a.ambience.Play();
        }

        public static void StopAmbience()
        {
            if (instance != null) instance.ambience.Stop();
        }

        private static float Volume()
        {
            var s = SaveService.Settings;
            return Mathf.Clamp01(s.MasterVolume) * Mathf.Clamp01(s.SfxVolume);
        }

        private static AudioClip Tone(string name, float freq, float dur, float vol, bool square = false, float endFreq = -1f)
        {
            const int rate = 44100;
            int n = Mathf.Max(1, (int)(rate * dur));
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float p = (float)i / n;
                float f = endFreq > 0f ? Mathf.Lerp(freq, endFreq, p) : freq;
                float phase = 2f * Mathf.PI * f * i / rate;
                float wave = square ? Mathf.Sign(Mathf.Sin(phase)) : Mathf.Sin(phase);
                float attack = Mathf.Clamp01(i / (rate * 0.005f));
                float release = Mathf.Clamp01((n - i) / (float)n * 4f);
                data[i] = wave * vol * attack * release;
            }
            var clip = AudioClip.Create(name, n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip Hum()
        {
            const int rate = 44100;
            int n = rate * 2; // 2s loop
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / rate;
                data[i] = (Mathf.Sin(2f * Mathf.PI * 70f * t) * 0.6f + Mathf.Sin(2f * Mathf.PI * 105f * t) * 0.4f) * 0.3f;
            }
            var clip = AudioClip.Create("ambience", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
