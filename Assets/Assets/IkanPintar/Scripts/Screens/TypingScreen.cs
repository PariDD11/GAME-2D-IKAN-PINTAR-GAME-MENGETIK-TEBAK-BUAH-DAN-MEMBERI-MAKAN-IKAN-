using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace IkanPintar
{
    public class TypingScreen : PlayScreen
    {
        const int SentenceCount = 6;
        const float TotalTime = 150f;
        const float StartX = 150f;
        const float EndX = 990f;
        const float MouthX = 1085f;
        const float FishY = 170f;
        const int MaxTail = 52;

        static readonly string CorrectColor = "B8FF9E";
        static readonly string WrongColor = "FF8A8A";
        static readonly string PendingColor = "FFFFFF";

        readonly List<string> sentences = new List<string>();

        Text txtSentence;
        Text txtTyped;
        Text txtTime;
        Text txtScore;
        Text txtProgress;
        Text txtStats;
        Image boxImage;
        Image fish;
        Image food;
        Text popup;
        float popupLife;

        string typed = "";
        int index;
        int done;
        int score;
        int mistakesSentence;
        int mistakesTotal;
        int keystrokes;
        int charsDone;
        float timeLeft;
        float elapsed;
        float sentenceTime;
        float fishX;
        float fishTarget;
        float clock;
        float backspaceTimer;
        bool foodEaten;
        bool allDone;
        string lastTime = "";
        string lastTyped = null;
        bool lastCaret;

        public TypingScreen(GameApp app) : base(app)
        {
        }

        protected override GameScreen Again()
        {
            return new TypingScreen(App);
        }

        protected override void Build()
        {
            timeLeft = TotalTime;
            fishX = StartX;
            fishTarget = StartX;

            string[] all = Art.Sentences();
            List<string> pool = new List<string>(all);
            Util.Shuffle(pool);
            int count = Mathf.Min(SentenceCount, pool.Count);
            for (int i = 0; i < count; i++) sentences.Add(pool[i]);

            Ui.Picture(Root, "Bg", Art.Get("Bg Text Input"), 640f, 360f, 1280f, 720f, Color.white);

            txtProgress = Ui.Label(Root, "Progress", "", 30, Color.white, TextAnchor.MiddleCenter, 230f, 42f, 360f, 44f);
            txtTime = Ui.Label(Root, "Time", "", 30, Color.white, TextAnchor.MiddleCenter, 640f, 42f, 300f, 44f);
            txtScore = Ui.Label(Root, "Score", "", 30, Color.white, TextAnchor.MiddleCenter, 1050f, 42f, 360f, 44f);

            Ui.Panel(Root, "Track", Ui.Hex(0xFFFFFF, 0.25f), (StartX + MouthX) / 2f, 245f, MouthX - StartX, 4f);
            food = Ui.Picture(Root, "Food", Art.Get("Makanan Ikan"), 1140f, FishY, 76f, 76f, Ui.Hex(0xFFE066));
            fish = Ui.Picture(Root, "Fish", Art.Get("Ikan Besar"), StartX, FishY, 170f, 110f, Ui.Hex(0xFFA94D));

            Ui.Label(Root, "Prompt", "Ketik kalimat ini:", 26, Ui.Hex(0xE3F4FF), TextAnchor.MiddleCenter, 640f, 300f, 800f, 36f);
            txtSentence = Ui.Label(Root, "Sentence", "", 42, Color.white, TextAnchor.MiddleCenter, 640f, 390f, 1040f, 140f, Art.Body, true);

            boxImage = Ui.Picture(Root, "InputBox", Art.Get("Bg Tombol"), 640f, 530f, 1100f, 84f, Color.white);
            txtTyped = Ui.Label(boxImage.rectTransform, "Typed", "", 30, Ui.Hex(0x0B5C99), TextAnchor.MiddleCenter, 550f, 42f, 1040f, 80f);

            txtStats = Ui.Label(Root, "Stats", "", 24, Ui.Hex(0xE3F4FF), TextAnchor.MiddleCenter, 640f, 612f, 1000f, 34f);
            Ui.Label(Root, "Hint", "Hapus dengan Backspace. Esc untuk jeda.", 22, Ui.Hex(0xE3F4FF, 0.85f), TextAnchor.MiddleCenter, 640f, 675f, 1000f, 32f);

            popup = Ui.Label(Root, "Popup", "", 44, Ui.Hex(0xFFE066), TextAnchor.MiddleCenter, 640f, 250f, 500f, 60f, Art.Body, true);
            popup.gameObject.SetActive(false);

            GameInput.ClearTyped();
            ShowSentence();
            RefreshHud(true);
        }

        string Target()
        {
            return sentences[Mathf.Min(index, sentences.Count - 1)];
        }

        void ShowSentence()
        {
            typed = "";
            mistakesSentence = 0;
            sentenceTime = 0f;
            Redraw();
        }

        void Redraw()
        {
            lastTyped = typed;
            string target = Target();
            StringBuilder sb = new StringBuilder();
            string current = "";
            string currentColor = "";

            for (int i = 0; i < target.Length; i++)
            {
                string color;
                if (i < typed.Length) color = typed[i] == target[i] ? CorrectColor : WrongColor;
                else color = PendingColor;

                if (color != currentColor)
                {
                    if (current.Length > 0) AppendRun(sb, current, currentColor);
                    current = "";
                    currentColor = color;
                }
                current += target[i];
            }
            if (current.Length > 0) AppendRun(sb, current, currentColor);

            txtSentence.text = sb.ToString();
            RedrawTyped();
        }

        static void AppendRun(StringBuilder sb, string text, string color)
        {
            sb.Append("<color=#").Append(color).Append(">").Append(text).Append("</color>");
        }

        void RedrawTyped()
        {
            string target = Target();
            bool onTrack = target.StartsWith(typed, System.StringComparison.Ordinal);

            string tail = typed.Length > MaxTail ? typed.Substring(typed.Length - MaxTail) : typed;
            bool caret = ((int)(clock * 2f)) % 2 == 0;
            lastCaret = caret;
            txtTyped.text = tail + (caret ? "|" : " ");

            boxImage.color = onTrack ? Color.white : Ui.Hex(0xFFC9C9);
        }

        void RefreshHud(bool force)
        {
            int shown = Mathf.Min(done + 1, sentences.Count);
            txtProgress.text = "Kalimat " + shown + " / " + sentences.Count;
            txtScore.text = "Skor: " + score;

            string t = "Waktu: " + Mathf.Max(0, Mathf.CeilToInt(timeLeft));
            if (force || t != lastTime)
            {
                lastTime = t;
                txtTime.text = t;
            }

            float minutes = Mathf.Max(elapsed, 1f) / 60f;
            int wpm = Mathf.RoundToInt(charsDone / 5f / minutes);
            int accuracy = keystrokes > 0 ? Mathf.RoundToInt(100f * (keystrokes - mistakesTotal) / keystrokes) : 100;
            txtStats.text = "Kecepatan: " + wpm + " kata/menit     Akurasi: " + accuracy + "%";
        }

        void TypeChars(string input)
        {
            string target = Target();

            for (int i = 0; i < input.Length; i++)
            {
                char ch = input[i];
                if (ch < ' ' || ch == (char)127) continue;
                if (typed.Length >= target.Length + 4) continue;

                typed += ch;
                keystrokes++;

                int position = typed.Length - 1;
                bool wrong = position >= target.Length || typed[position] != target[position];
                if (wrong)
                {
                    mistakesSentence++;
                    mistakesTotal++;
                    App.Sound.Play("typeBad");
                }
                else
                {
                    App.Sound.Play("type");
                }
            }
        }

        void DeleteChar()
        {
            if (typed.Length == 0) return;
            typed = typed.Substring(0, typed.Length - 1);
        }

        void Complete()
        {
            string target = Target();
            done++;
            charsDone += target.Length;

            int gain = Mathf.Max(5, 20 - 3 * mistakesSentence);
            float goal = target.Length * 0.3f;
            if (sentenceTime < goal) gain += 10;
            else if (sentenceTime < goal * 2f) gain += 5;
            score += gain;

            ShowPopup("+" + gain);
            App.Sound.Play("correct");

            if (done >= sentences.Count)
            {
                allDone = true;
                fishTarget = MouthX;
                typed = "";
                Redraw();
                RefreshHud(false);
                return;
            }

            fishTarget = StartX + (EndX - StartX) * done / sentences.Count;
            index++;
            ShowSentence();
            RefreshHud(false);
        }

        void ShowPopup(string text)
        {
            popup.text = text;
            popup.color = Ui.Hex(0xFFE066);
            popup.gameObject.SetActive(true);
            popupLife = 0f;
        }

        void Conclude(string heading)
        {
            if (allDone)
            {
                int bonus = Mathf.RoundToInt(timeLeft / 2f);
                score += bonus;
            }

            float minutes = Mathf.Max(elapsed, 1f) / 60f;
            int wpm = Mathf.RoundToInt(charsDone / 5f / minutes);
            int accuracy = keystrokes > 0 ? Mathf.RoundToInt(100f * (keystrokes - mistakesTotal) / keystrokes) : 100;
            int total = sentences.Count;
            int finished = done;
            int finalScore = score;

            string[] lines =
            {
                "Kalimat selesai: " + finished + " dari " + total,
                "Kecepatan: " + wpm + " kata/menit",
                "Akurasi: " + accuracy + "%"
            };

            EndGame(allDone ? 1.4f : 0.6f, () => new ResultScreen(App, "typing", heading, finalScore, lines, new int[] { 60, 120, 180 }, () => new TypingScreen(App)));
        }

        void AnimateFish(float dt)
        {
            float k = 1f - Mathf.Exp(-5f * dt);
            fishX = Mathf.Lerp(fishX, fishTarget, k);
            float bob = Mathf.Sin(clock * 4f) * 6f;
            Ui.SetCenter(fish.rectTransform, fishX, FishY + bob);

            if (allDone && !foodEaten && Mathf.Abs(fishX - MouthX) < 10f)
            {
                foodEaten = true;
                food.gameObject.SetActive(false);
                App.Sound.Play("eat");
            }

            if (popup.gameObject.activeSelf)
            {
                popupLife += dt;
                Ui.SetCenter(popup.rectTransform, 640f, 250f - popupLife * 40f);
                Color c = popup.color;
                c.a = Mathf.Clamp01(1f - popupLife / 1f);
                popup.color = c;
                if (popupLife >= 1f) popup.gameObject.SetActive(false);
            }
        }

        public override void Tick(float dt)
        {
            clock += dt;
            if (Paused) return;

            AnimateFish(dt);
            if (TickEnd(dt)) return;

            elapsed += dt;
            sentenceTime += dt;
            timeLeft -= dt;

            string input = GameInput.TakeTyped();
            if (input.Length > 0) TypeChars(input);

            if (GameInput.WasPressed(GameKey.Backspace))
            {
                DeleteChar();
                backspaceTimer = 0.4f;
            }
            else if (GameInput.IsHeld(GameKey.Backspace))
            {
                backspaceTimer -= dt;
                if (backspaceTimer <= 0f)
                {
                    DeleteChar();
                    backspaceTimer = 0.05f;
                }
            }

            if (typed != lastTyped) Redraw();
            else if ((((int)(clock * 2f)) % 2 == 0) != lastCaret) RedrawTyped();

            if (typed == Target())
            {
                Complete();
                if (allDone)
                {
                    Conclude("Semua Kalimat Selesai!");
                    return;
                }
            }

            RefreshHud(false);

            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                RefreshHud(false);
                Conclude("Waktu Habis!");
            }
        }

        protected override void OnPaused(bool paused)
        {
            GameInput.ClearTyped();
        }
    }
}
