using UnityEngine;
using UnityEngine.UI;

namespace IkanPintar
{
    public class ResultScreen : GameScreen
    {
        readonly string mode;
        readonly string heading;
        readonly int score;
        readonly string[] lines;
        readonly int[] thresholds;
        readonly System.Func<GameScreen> again;

        Text scoreText;
        Image[] icons;
        bool[] earned;
        float shown;
        float clock;
        int rank;
        bool record;

        public ResultScreen(GameApp app, string mode, string heading, int score, string[] lines, int[] thresholds, System.Func<GameScreen> again) : base(app)
        {
            this.mode = mode;
            this.heading = heading;
            this.score = score;
            this.lines = lines;
            this.thresholds = thresholds;
            this.again = again;
        }

        protected override void Build()
        {
            record = Scores.Submit(mode, score);
            rank = 0;
            for (int i = 0; i < thresholds.Length; i++)
            {
                if (score >= thresholds[i]) rank = i + 1;
            }

            Ui.Picture(Root, "Bg", Art.Get("Bg Text Input"), 640f, 360f, 1280f, 720f, Color.white);
            Ui.Panel(Root, "Panel", Ui.Hex(0x04243F, 0.32f), 640f, 360f, 900f, 640f);

            Ui.Label(Root, "Heading", heading, 96, Color.white, TextAnchor.MiddleCenter, 640f, 95f, 860f, 130f, Art.Title, true);

            icons = new Image[3];
            earned = new bool[3];
            for (int i = 0; i < 3; i++)
            {
                earned[i] = i < rank;
                icons[i] = Ui.Picture(Root, "Rank" + i, Art.Get("Ikan Kecil"), 520f + i * 120f, 205f, 96f, 96f,
                    earned[i] ? Ui.Hex(0xFFD54F) : Ui.Hex(0xFFFFFF, 0.22f));
                icons[i].rectTransform.localScale = earned[i] ? Vector3.zero : Vector3.one;
            }

            Ui.Label(Root, "ScoreTitle", "SKOR", 26, Ui.Hex(0xD6EEFF), TextAnchor.MiddleCenter, 640f, 285f, 400f, 36f);
            scoreText = Ui.Label(Root, "Score", "0", 120, Color.white, TextAnchor.MiddleCenter, 640f, 355f, 600f, 130f, Art.Title, true);

            string recordText = record ? "Rekor baru!" : "Rekor terbaik: " + Scores.Best(mode);
            Color recordColor = record ? Ui.Hex(0xFFE066) : Ui.Hex(0xD6EEFF);
            Ui.Label(Root, "Record", recordText, 30, recordColor, TextAnchor.MiddleCenter, 640f, 428f, 700f, 40f);

            for (int i = 0; i < lines.Length; i++)
            {
                Ui.Label(Root, "Line" + i, lines[i], 28, Color.white, TextAnchor.MiddleCenter, 640f, 478f + i * 38f, 800f, 36f);
            }

            AddButton(Root, "Again", "Main Lagi", 440f, 625f, 340f, 72f, 32, PlayAgain);
            AddButton(Root, "Menu", "Menu Utama", 840f, 625f, 340f, 72f, 32, ToMenu);

            App.Sound.Play(rank > 0 ? "win" : "lose");
        }

        void PlayAgain()
        {
            App.Go(again);
        }

        void ToMenu()
        {
            App.Go(() => new MenuScreen(App));
        }

        public override void Back()
        {
            ToMenu();
        }

        public override void Tick(float dt)
        {
            clock += dt;

            shown = Mathf.MoveTowards(shown, score, Mathf.Max(1f, score) * dt / 0.9f);
            scoreText.text = Mathf.RoundToInt(shown).ToString();

            for (int i = 0; i < icons.Length; i++)
            {
                if (!earned[i]) continue;
                float t = (clock - (0.5f + i * 0.35f)) / 0.4f;
                float s = Util.EaseOutBack(t);
                icons[i].rectTransform.localScale = new Vector3(s, s, 1f);
            }

            if (GameInput.WasPressed(GameKey.Enter) || GameInput.WasPressed(GameKey.Space)) PlayAgain();
        }
    }
}
