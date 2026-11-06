using System.Collections.Generic;
using UnityEngine;

namespace IkanPintar
{
    public class Sfx
    {
        const string PrefKey = "IkanPintar.Sound";
        const int Rate = 44100;

        readonly AudioSource source;
        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        bool enabled;

        public bool Enabled
        {
            get { return enabled; }
            set
            {
                enabled = value;
                PlayerPrefs.SetInt(PrefKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        public Sfx(GameObject host)
        {
            source = host.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = 1f;
            enabled = PlayerPrefs.GetInt(PrefKey, 1) != 0;
            Build();
        }

        public void Play(string name, float volume = 1f)
        {
            if (!enabled) return;
            AudioClip clip;
            if (clips.TryGetValue(name, out clip)) source.PlayOneShot(clip, volume);
        }

        void Build()
        {
            Add("click", 0, 0.35f, 900f, 700f, 0.05f);
            Add("move", 2, 0.18f, 500f, 420f, 0.04f);
            Add("type", 0, 0.12f, 1300f, 1100f, 0.025f);
            Add("typeBad", 1, 0.18f, 160f, 120f, 0.07f);
            Add("eat", 0, 0.4f, 520f, 880f, 0.10f);
            Add("correct", 2, 0.45f, 660f, 660f, 0.10f, 880f, 880f, 0.16f);
            Add("wrong", 1, 0.3f, 220f, 150f, 0.16f, 150f, 110f, 0.2f);
            Add("hit", 3, 0.5f, 200f, 60f, 0.3f);
            Add("win", 2, 0.45f, 523f, 523f, 0.12f, 659f, 659f, 0.12f, 784f, 784f, 0.12f, 1047f, 1047f, 0.3f);
            Add("lose", 2, 0.45f, 440f, 440f, 0.18f, 349f, 349f, 0.18f, 262f, 262f, 0.4f);
            Add("level", 0, 0.4f, 600f, 600f, 0.08f, 800f, 800f, 0.08f, 1000f, 1000f, 0.14f);
        }

        void Add(string name, int wave, float volume, params float[] notes)
        {
            List<float> data = new List<float>();
            System.Random rng = new System.Random(7);
            double phase = 0.0;
            float noise = 0f;

            for (int n = 0; n + 2 < notes.Length; n += 3)
            {
                float f0 = notes[n];
                float f1 = notes[n + 1];
                float dur = notes[n + 2];
                int count = Mathf.Max(1, (int)(dur * Rate));

                for (int i = 0; i < count; i++)
                {
                    float u = (float)i / count;
                    float freq = Mathf.Lerp(f0, f1, u);
                    phase += 2.0 * System.Math.PI * freq / Rate;

                    float s;
                    if (wave == 1)
                    {
                        s = Mathf.Sin((float)phase) >= 0f ? 0.6f : -0.6f;
                    }
                    else if (wave == 2)
                    {
                        float p = (float)(phase / (2.0 * System.Math.PI));
                        p -= Mathf.Floor(p);
                        s = 4f * Mathf.Abs(p - 0.5f) - 1f;
                    }
                    else if (wave == 3)
                    {
                        noise = noise * 0.6f + ((float)rng.NextDouble() * 2f - 1f) * 0.4f;
                        s = noise * 2f;
                    }
                    else
                    {
                        s = Mathf.Sin((float)phase);
                    }

                    float attack = Mathf.Min(1f, i / (0.004f * Rate));
                    float env = attack * Mathf.Pow(1f - u, 0.6f);
                    data.Add(s * env * volume);
                }
            }

            AudioClip clip = AudioClip.Create("sfx_" + name, data.Count, 1, Rate, false);
            clip.SetData(data.ToArray(), 0);
            clips[name] = clip;
        }
    }
}
