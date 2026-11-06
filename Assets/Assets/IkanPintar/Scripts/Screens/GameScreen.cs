using System.Collections.Generic;
using UnityEngine;

namespace IkanPintar
{
    public abstract class GameScreen
    {
        protected readonly GameApp App;
        protected readonly List<UiButton> Buttons = new List<UiButton>();
        List<UiButton> modal;

        public RectTransform Root { get; private set; }

        protected GameScreen(GameApp app)
        {
            App = app;
        }

        public void Open()
        {
            Root = Ui.Node(App.Stage, GetType().Name);
            Ui.Stretch(Root);
            Build();
        }

        public void Close()
        {
            OnClose();
            if (Root != null) UnityEngine.Object.Destroy(Root.gameObject);
        }

        protected abstract void Build();

        protected virtual void OnClose()
        {
        }

        public virtual void Tick(float dt)
        {
        }

        public virtual void Back()
        {
        }

        protected void SetModal(List<UiButton> list)
        {
            modal = list;
        }

        protected UiButton AddButton(Transform parent, string name, string text, float cx, float cy, float w, float h, int size, System.Action click)
        {
            UiButton b = new UiButton(parent, name, text, cx, cy, w, h, size, click);
            Buttons.Add(b);
            return b;
        }

        public void UpdateButtons()
        {
            List<UiButton> list = modal != null ? modal : Buttons;
            Vector2 p = GameInput.PointerPosition;
            UiButton hit = null;

            for (int i = 0; i < list.Count; i++)
            {
                UiButton b = list[i];
                if (b.Enabled && b.Contains(p)) hit = b;
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i].SetHover(list[i] == hit);
            }

            if (hit != null && GameInput.PointerPressed)
            {
                App.Sound.Play("click");
                if (hit.Click != null) hit.Click();
            }
        }

        protected static bool Pressed(GameKey a, GameKey b)
        {
            return GameInput.WasPressed(a) || GameInput.WasPressed(b);
        }

        protected static bool Held(GameKey a, GameKey b)
        {
            return GameInput.IsHeld(a) || GameInput.IsHeld(b);
        }
    }

    public abstract class PlayScreen : GameScreen
    {
        protected bool Paused;
        protected bool Finished;

        RectTransform pauseRoot;
        readonly List<UiButton> pauseButtons = new List<UiButton>();
        float endDelay;
        System.Func<GameScreen> endFactory;

        protected PlayScreen(GameApp app) : base(app)
        {
        }

        protected abstract GameScreen Again();

        public override void Back()
        {
            if (Finished) return;
            if (Paused) Resume();
            else Pause();
        }

        protected void Pause()
        {
            if (pauseRoot == null) BuildPause();
            Paused = true;
            pauseRoot.gameObject.SetActive(true);
            pauseRoot.SetAsLastSibling();
            SetModal(pauseButtons);
            OnPaused(true);
        }

        protected void Resume()
        {
            Paused = false;
            if (pauseRoot != null) pauseRoot.gameObject.SetActive(false);
            SetModal(null);
            OnPaused(false);
        }

        protected virtual void OnPaused(bool paused)
        {
        }

        void BuildPause()
        {
            pauseRoot = Ui.Node(Root, "Pause");
            Ui.Stretch(pauseRoot);
            Ui.Fill(pauseRoot, "Dim", Ui.Hex(0x04243F, 0.74f));
            Ui.Label(pauseRoot, "Title", "Jeda", 120, Color.white, TextAnchor.MiddleCenter, 640f, 150f, 800f, 160f, Art.Title, true);

            pauseButtons.Add(new UiButton(pauseRoot, "Resume", "Lanjutkan", 640f, 300f, 460f, 78f, 34, Resume));
            pauseButtons.Add(new UiButton(pauseRoot, "Restart", "Ulangi", 640f, 400f, 460f, 78f, 34, Restart));
            pauseButtons.Add(new UiButton(pauseRoot, "Menu", "Menu Utama", 640f, 500f, 460f, 78f, 34, ToMenu));

            Ui.Label(pauseRoot, "Hint", "Tekan Esc untuk melanjutkan", 24, Ui.Hex(0xD6EEFF), TextAnchor.MiddleCenter, 640f, 600f, 800f, 40f);
        }

        void Restart()
        {
            App.Go(Again);
        }

        void ToMenu()
        {
            App.Go(() => new MenuScreen(App));
        }

        protected void EndGame(float delay, System.Func<GameScreen> next)
        {
            if (Finished) return;
            Finished = true;
            endDelay = delay;
            endFactory = next;
        }

        protected bool TickEnd(float dt)
        {
            if (!Finished) return false;
            if (endFactory != null)
            {
                endDelay -= dt;
                if (endDelay <= 0f)
                {
                    System.Func<GameScreen> next = endFactory;
                    endFactory = null;
                    App.Go(next);
                }
            }
            return true;
        }
    }
}
