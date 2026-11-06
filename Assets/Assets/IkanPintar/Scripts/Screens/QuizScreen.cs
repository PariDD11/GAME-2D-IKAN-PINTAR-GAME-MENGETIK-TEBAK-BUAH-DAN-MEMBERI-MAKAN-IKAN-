using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IkanPintar
{
    public class QuizScreen : PlayScreen
    {
        class Fruit
        {
            public string Sprite;
            public string Name;
            public int Tint;

            public Fruit(string sprite, string name, int tint)
            {
                Sprite = sprite;
                Name = name;
                Tint = tint;
            }
        }

        static readonly Fruit[] All =
        {
            new Fruit("Apple", "Apel", 0xFF6B6B),
            new Fruit("Banana", "Pisang", 0xFFE066),
            new Fruit("Cherry", "Ceri", 0xFF4D7D),
            new Fruit("Grape", "Anggur", 0xB88CFF),
            new Fruit("Melon", "Melon", 0xA8E063),
            new Fruit("Orange", "Jeruk", 0xFFA94D),
            new Fruit("Pear", "Pir", 0xC5E384),
            new Fruit("Pineapple", "Nanas", 0xFFD43B),
            new Fruit("Strawberry", "Stroberi", 0xFF5C7A),
            new Fruit("Watermelon", "Semangka", 0xFF8FA3)
        };

        const int Total = 10;
        const float TimePerQuestion = 12f;
        const float FeedbackTime = 1.4f;
        const float BarWidth = 300f;

        static readonly string[] Letters = { "A", "B", "C", "D" };

        readonly List<Fruit> order = new List<Fruit>();
        readonly Fruit[] options = new Fruit[4];
        readonly UiButton[] answerButtons = new UiButton[4];

        Text txtProgress;
        Text txtScore;
        Text txtFeedback;
        Text txtSeconds;
        Image imgFruit;
        UiBar bar;

        Fruit target;
        int index;
        int score;
        int correct;
        int streak;
        int bestStreak;
        float timeLeft;
        float feedbackLeft;
        float popClock;
        bool answered;

        public QuizScreen(GameApp app) : base(app)
        {
        }

        protected override GameScreen Again()
        {
            return new QuizScreen(App);
        }

        protected override void Build()
        {
            Ui.Picture(Root, "Bg", Art.Get("Bg Button Input"), 640f, 360f, 1280f, 720f, Color.white);

            txtProgress = Ui.Label(Root, "Progress", "", 30, Color.white, TextAnchor.MiddleCenter, 235f, 84f, 340f, 44f);
            txtScore = Ui.Label(Root, "Score", "", 30, Color.white, TextAnchor.MiddleCenter, 1045f, 84f, 340f, 44f);
            bar = new UiBar(Root, "Timer", 640f, 111f, BarWidth, 12f, Ui.Hex(0x04243F, 0.35f), Ui.Hex(0x7BE495));
            txtSeconds = Ui.Label(Root, "Seconds", "", 22, Color.white, TextAnchor.MiddleCenter, 850f, 111f, 70f, 30f);

            Ui.Label(Root, "Prompt", "Buah apakah ini?", 30, Ui.Hex(0xE3F4FF), TextAnchor.MiddleCenter, 640f, 152f, 600f, 40f);
            imgFruit = Ui.Picture(Root, "Fruit", null, 640f, 282f, 180f, 180f, Color.white);
            txtFeedback = Ui.Label(Root, "Feedback", "", 28, Color.white, TextAnchor.MiddleCenter, 640f, 396f, 760f, 40f);

            float[] xs = { 353f, 928f, 353f, 928f };
            float[] ys = { 500f, 500f, 616f, 616f };
            for (int i = 0; i < 4; i++)
            {
                int slot = i;
                answerButtons[i] = AddButton(Root, "Answer" + i, "", xs[i], ys[i], 440f, 72f, 32, () => Choose(slot));
            }

            order.AddRange(All);
            Util.Shuffle(order);
            BeginQuestion();
        }

        static bool Clash(Fruit a, Fruit b)
        {
            return (a.Sprite == "Melon" && b.Sprite == "Watermelon") || (a.Sprite == "Watermelon" && b.Sprite == "Melon");
        }

        void BeginQuestion()
        {
            if (index >= Total || index >= order.Count)
            {
                Finish();
                return;
            }

            target = order[index];

            List<Fruit> pool = new List<Fruit>();
            for (int i = 0; i < All.Length; i++)
            {
                Fruit f = All[i];
                if (f == target || Clash(f, target)) continue;
                pool.Add(f);
            }
            Util.Shuffle(pool);

            int slot = Random.Range(0, 4);
            int next = 0;
            for (int i = 0; i < 4; i++)
            {
                if (i == slot)
                {
                    options[i] = target;
                }
                else
                {
                    options[i] = pool[next];
                    next++;
                }

                answerButtons[i].SetText(Letters[i] + ". " + options[i].Name);
                answerButtons[i].ResetTint();
                answerButtons[i].Enabled = true;
            }

            imgFruit.sprite = Art.Get(target.Sprite);
            imgFruit.color = Ui.Hex(target.Tint);

            answered = false;
            timeLeft = TimePerQuestion;
            popClock = 0f;
            txtFeedback.text = "";
            RefreshHud();
        }

        void RefreshHud()
        {
            txtProgress.text = "Soal " + Mathf.Min(index + 1, Total) + " / " + Total;
            txtScore.text = "Skor: " + score;
        }

        void Choose(int slot)
        {
            if (answered || Paused || Finished) return;
            answered = true;
            feedbackLeft = FeedbackTime;

            bool ok = slot >= 0 && options[slot] == target;

            for (int i = 0; i < 4; i++)
            {
                answerButtons[i].Enabled = false;
                if (options[i] == target) answerButtons[i].SetTint(Ui.Hex(0x7BE495));
                else if (i == slot) answerButtons[i].SetTint(Ui.Hex(0xFF8A8A));
            }

            if (ok)
            {
                correct++;
                streak++;
                if (streak > bestStreak) bestStreak = streak;
                int gain = 10 + Mathf.FloorToInt(timeLeft / 3f) + (streak >= 3 ? 5 : 0);
                score += gain;
                txtFeedback.color = Ui.Hex(0xC9FFD6);
                txtFeedback.text = "Benar! +" + gain;
                App.Sound.Play("correct");
            }
            else
            {
                streak = 0;
                txtFeedback.color = Ui.Hex(0xFFD0D0);
                txtFeedback.text = (slot < 0 ? "Waktu habis! " : "Salah! ") + "Jawabannya: " + target.Name;
                App.Sound.Play("wrong");
            }

            RefreshHud();
        }

        void Finish()
        {
            int accuracy = Mathf.RoundToInt(100f * correct / Total);
            string[] lines =
            {
                "Jawaban benar: " + correct + " dari " + Total,
                "Akurasi: " + accuracy + "%",
                "Beruntun terbaik: " + bestStreak
            };
            int finalScore = score;
            EndGame(0f, () => new ResultScreen(App, "quiz", "Kuis Selesai!", finalScore, lines, new int[] { 70, 110, 150 }, () => new QuizScreen(App)));
        }

        public override void Tick(float dt)
        {
            if (TickEnd(dt)) return;
            if (Paused) return;

            popClock += dt;
            float s = 0.55f + 0.45f * Util.EaseOutBack(popClock / 0.35f);
            imgFruit.rectTransform.localScale = new Vector3(s, s, 1f);

            if (!answered)
            {
                timeLeft -= dt;
                float frac = timeLeft / TimePerQuestion;
                bar.Set(frac);
                bar.Fill.color = frac > 0.5f ? Ui.Hex(0x7BE495) : (frac > 0.25f ? Ui.Hex(0xFFD54F) : Ui.Hex(0xFF6B6B));
                txtSeconds.text = Mathf.Max(0, Mathf.CeilToInt(timeLeft)) + " dtk";

                if (GameInput.WasPressed(GameKey.Alpha1) || GameInput.WasPressed(GameKey.A)) Choose(0);
                else if (GameInput.WasPressed(GameKey.Alpha2) || GameInput.WasPressed(GameKey.B)) Choose(1);
                else if (GameInput.WasPressed(GameKey.Alpha3) || GameInput.WasPressed(GameKey.C)) Choose(2);
                else if (GameInput.WasPressed(GameKey.Alpha4) || GameInput.WasPressed(GameKey.D)) Choose(3);
                else if (timeLeft <= 0f) Choose(-1);
            }
            else
            {
                feedbackLeft -= dt;
                if (feedbackLeft <= 0f)
                {
                    index++;
                    BeginQuestion();
                }
            }
        }
    }
}
