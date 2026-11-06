using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IkanPintar
{
    public class MenuScreen : GameScreen
    {
        class Swimmer
        {
            public Image Img;
            public float X;
            public float Y;
            public float Speed;
            public float Phase;
            public float Dir;
        }

        readonly List<Swimmer> swimmers = new List<Swimmer>();
        Text soundLabel;
        float clock;

        public MenuScreen(GameApp app) : base(app)
        {
        }

        protected override void Build()
        {
            Ui.Picture(Root, "Bg", Art.Get("Bg Text Input"), 640f, 360f, 1280f, 720f, Color.white);
            AddSwimmers();

            Ui.Label(Root, "Title", "Ikan Pintar", 170, Color.white, TextAnchor.MiddleCenter, 640f, 105f, 1100f, 200f, Art.Title, true);
            Ui.Label(Root, "Subtitle", "Kuis buah, tangkap makanan, dan ngetik cepat", 30, Ui.Hex(0xE3F4FF), TextAnchor.MiddleCenter, 640f, 212f, 1100f, 44f);

            string[] names = { "Kuis Buah", "Ikan Lapar", "Ketik Cepat" };
            string[] modes = { "quiz", "grid", "typing" };

            for (int i = 0; i < 3; i++)
            {
                int index = i;
                UiButton b = AddButton(Root, "Mode" + i, names[i], 640f, 296f + i * 92f, 560f, 78f, 36, () => Launch(index));
                Ui.Label(b.Rt, "Key", (i + 1).ToString(), 28, Ui.Hex(0x7FB4DB), TextAnchor.MiddleCenter, 38f, 39f, 40f, 60f);
                Ui.Label(b.Rt, "Best", "Rekor " + Scores.Best(modes[i]), 20, Ui.Hex(0x4C8DBF), TextAnchor.MiddleRight, 530f, 39f, 190f, 60f);
            }

            bool canQuit = Application.platform != RuntimePlatform.WebGLPlayer;
            float[] xs = canQuit ? new float[] { 340f, 640f, 940f } : new float[] { 480f, 800f, 0f };

            AddButton(Root, "Help", "Cara Bermain", xs[0], 590f, 270f, 64f, 26, () => App.Go(() => new HelpScreen(App)));
            UiButton sound = AddButton(Root, "Sound", SoundText(), xs[1], 590f, 270f, 64f, 26, ToggleSound);
            soundLabel = sound.Label;

            if (canQuit) AddButton(Root, "Quit", "Keluar", xs[2], 590f, 270f, 64f, 26, App.Quit);

            Ui.Label(Root, "Hint", "Tekan 1, 2, atau 3 untuk memulai. Esc untuk jeda saat bermain.", 22, Ui.Hex(0xE3F4FF, 0.9f), TextAnchor.MiddleCenter, 640f, 675f, 1100f, 36f);
        }

        void AddSwimmers()
        {
            string[] sprites = { "Ikan Besar", "Ikan Kecil", "Ikan Kecil", "Ikan Besar", "Ikan Kecil", "Ikan Kecil" };
            float[] sizes = { 170f, 80f, 64f, 130f, 90f, 56f };
            float[] ys = { 330f, 450f, 540f, 640f, 250f, 400f };
            float[] speeds = { 38f, 70f, 55f, 30f, 62f, 46f };
            int[] tints = { 0xFFB347, 0xFFFFFF, 0xFFE066, 0xFF8FA3, 0xBDF2FF, 0xC5F58A };

            for (int i = 0; i < sprites.Length; i++)
            {
                bool big = sprites[i] == "Ikan Besar";
                float w = sizes[i];
                float h = big ? sizes[i] * 0.65f : sizes[i];

                Swimmer s = new Swimmer();
                s.Img = Ui.Picture(Root, "Swimmer" + i, Art.Get(sprites[i]), 0f, 0f, w, h, Ui.Hex(tints[i], 0.5f));
                s.X = Random.Range(0f, 1280f);
                s.Y = ys[i];
                s.Speed = speeds[i];
                s.Phase = Random.Range(0f, 6.28f);
                s.Dir = Random.value > 0.5f ? 1f : -1f;
                swimmers.Add(s);
                Place(s);
            }
        }

        void Place(Swimmer s)
        {
            float y = s.Y + Mathf.Sin(clock * 1.6f + s.Phase) * 12f;
            Ui.SetCenter(s.Img.rectTransform, s.X, y);
            s.Img.rectTransform.localScale = new Vector3(s.Dir, 1f, 1f);
        }

        string SoundText()
        {
            return App.Sound.Enabled ? "Suara: Hidup" : "Suara: Mati";
        }

        void ToggleSound()
        {
            App.Sound.Enabled = !App.Sound.Enabled;
            soundLabel.text = SoundText();
            App.Sound.Play("click");
        }

        void Launch(int index)
        {
            if (index == 0) App.Go(() => new QuizScreen(App));
            else if (index == 1) App.Go(() => new GridScreen(App));
            else App.Go(() => new TypingScreen(App));
        }

        public override void Tick(float dt)
        {
            clock += dt;

            for (int i = 0; i < swimmers.Count; i++)
            {
                Swimmer s = swimmers[i];
                s.X += s.Dir * s.Speed * dt;
                if (s.X > 1380f) s.Dir = -1f;
                else if (s.X < -100f) s.Dir = 1f;
                Place(s);
            }

            if (GameInput.WasPressed(GameKey.Alpha1)) Launch(0);
            else if (GameInput.WasPressed(GameKey.Alpha2)) Launch(1);
            else if (GameInput.WasPressed(GameKey.Alpha3)) Launch(2);
        }
    }
}
