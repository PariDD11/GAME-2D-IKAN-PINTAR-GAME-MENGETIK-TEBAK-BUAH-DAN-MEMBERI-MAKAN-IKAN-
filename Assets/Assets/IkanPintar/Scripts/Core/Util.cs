using System.Collections.Generic;
using UnityEngine;

namespace IkanPintar
{
    public static class Util
    {
        public static void Shuffle<T>(IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        public static float EaseOut(float t)
        {
            t = Mathf.Clamp01(t);
            float inv = 1f - t;
            return 1f - inv * inv * inv;
        }

        public static float EaseOutBack(float t)
        {
            t = Mathf.Clamp01(t);
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float x = t - 1f;
            return 1f + c3 * x * x * x + c1 * x * x;
        }
    }

    public static class Scores
    {
        public static int Best(string mode)
        {
            return PlayerPrefs.GetInt("IkanPintar.Best." + mode, 0);
        }

        public static bool Submit(string mode, int score)
        {
            bool record = score > Best(mode);
            if (record)
            {
                PlayerPrefs.SetInt("IkanPintar.Best." + mode, score);
                PlayerPrefs.Save();
            }
            return record;
        }
    }
}
