using System.Collections.Generic;
using UnityEngine;

namespace IkanPintar
{
    public static class Art
    {
        const string Root = "IkanPintar/";

        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        static readonly string[] Fallback =
        {
            "Andi pergi ke Bali bersama keluarga dan teman dekatnya",
            "Biasanya pak Budi libur di hari Sabtu minggu terakhir",
            "Jangan lupa berdoa sebelum melakukan aktivitas apapun",
            "Nita selalu menyediakan makanan lebih untuk kita semua",
            "Amin segera bergegas meninggalkan rumah untuk berolahraga",
            "Saat ini Kaylila sudah berjalan dan berlari cukup kencang",
            "Belajar dan praktik adalah rahasia untuk belajar cepat",
            "Jangan lupa mematikan lampu saat keluar dari ruangan",
            "Apakah Anda sudah bisa membuat aplikasi dengan Unity",
            "Apakah Anda memahami kode-kode penting di bahasa Kotlin"
        };

        public static Font Body { get; private set; }
        public static Font Title { get; private set; }

        public static void Load()
        {
            Body = Resources.Load<Font>(Root + "Fonts/Comfortaa-Regular");
            Title = Resources.Load<Font>(Root + "Fonts/Everything");
            if (Body == null) Body = Title;
            if (Body == null) Body = Font.CreateDynamicFontFromOSFont("Arial", 32);
            if (Title == null) Title = Body;
        }

        public static Sprite Get(string name)
        {
            Sprite s;
            if (cache.TryGetValue(name, out s) && s != null) return s;

            string path = Root + "Sprites/" + name;
            s = Resources.Load<Sprite>(path);

            if (s == null)
            {
                Sprite[] all = Resources.LoadAll<Sprite>(path);
                if (all != null && all.Length > 0) s = all[0];
            }

            if (s == null)
            {
                Texture2D tex = Resources.Load<Texture2D>(path);
                if (tex != null)
                {
                    s = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
                }
            }

            cache[name] = s;
            return s;
        }

        public static string[] Sentences()
        {
            List<string> list = new List<string>();
            TextAsset asset = Resources.Load<TextAsset>(Root + "Data/PaketSoal");

            if (asset != null)
            {
                string[] lines = asset.text.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i].Trim().TrimStart('﻿').Trim();
                    if (line.Length > 0) list.Add(line);
                }
            }

            if (list.Count < 3)
            {
                list.Clear();
                list.AddRange(Fallback);
            }

            return list.ToArray();
        }
    }
}
