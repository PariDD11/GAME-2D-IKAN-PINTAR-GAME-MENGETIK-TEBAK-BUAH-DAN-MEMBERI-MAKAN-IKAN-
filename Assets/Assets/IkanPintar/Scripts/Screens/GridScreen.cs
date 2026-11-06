using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace IkanPintar
{
    public class GridScreen : PlayScreen
    {
        class Actor
        {
            public int C;
            public int R;
            public Vector2 Pos;
            public Image Img;
            public float Timer;
            public float Phase;
            public bool FacingLeft;
        }

        class Popup
        {
            public Text Label;
            public Color Tint;
            public Vector2 Pos;
            public float Life;
        }

        const int Cols = 12;
        const int Rows = 6;
        const float Cell = 100f;
        const float OriginX = 40f;
        const float OriginY = 60f;
        const float StartTime = 60f;
        const int StartLives = 3;
        const int FoodPerLevel = 5;
        const int MaxEnemies = 4;

        static readonly Color PlayerColor = new Color(1f, 0.663f, 0.302f, 1f);
        static readonly Color EnemyColor = new Color(1f, 0.42f, 0.42f, 1f);
        static readonly Color FoodColor = new Color(1f, 0.878f, 0.4f, 1f);

        readonly List<Actor> enemies = new List<Actor>();
        readonly List<Popup> popups = new List<Popup>();

        RectTransform layerFood;
        RectTransform layerEnemy;
        RectTransform layerPlayer;
        RectTransform layerFx;

        Actor player;
        Actor food;
        Text txtScore;
        Text txtLevel;
        Text txtTime;
        Image[] lifeIcons;

        int lives;
        int score;
        int eaten;
        int level;
        float timeLeft;
        float invulnerable;
        float moveCooldown;
        float clock;
        string lastTime = "";

        public GridScreen(GameApp app) : base(app)
        {
        }

        protected override GameScreen Again()
        {
            return new GridScreen(App);
        }

        static Vector2 CellCenter(int c, int r)
        {
            return new Vector2(OriginX + c * Cell + Cell / 2f, OriginY + r * Cell + Cell / 2f);
        }

        static int Distance(int c1, int r1, int c2, int r2)
        {
            return Mathf.Abs(c1 - c2) + Mathf.Abs(r1 - r2);
        }

        protected override void Build()
        {
            lives = StartLives;
            level = 1;
            timeLeft = StartTime;

            Ui.Picture(Root, "Bg", Art.Get("Bg Keyboard Input"), 640f, 360f, 1280f, 720f, Color.white);

            txtScore = Ui.Label(Root, "Score", "", 32, Color.white, TextAnchor.MiddleCenter, 170f, 30f, 280f, 44f);
            txtLevel = Ui.Label(Root, "Level", "", 32, Color.white, TextAnchor.MiddleCenter, 450f, 30f, 240f, 44f);
            txtTime = Ui.Label(Root, "Time", "", 32, Color.white, TextAnchor.MiddleCenter, 720f, 30f, 280f, 44f);

            lifeIcons = new Image[StartLives];
            for (int i = 0; i < StartLives; i++)
            {
                lifeIcons[i] = Ui.Picture(Root, "Life" + i, Art.Get("Ikan Kecil"), 1040f + i * 60f, 30f, 46f, 46f, PlayerColor);
            }

            Ui.Label(Root, "Hint", "Panah atau WASD untuk bergerak. Makan makanan, hindari ikan besar. Esc untuk jeda.", 22,
                Ui.Hex(0xE3F4FF, 0.9f), TextAnchor.MiddleCenter, 640f, 692f, 1200f, 32f);

            layerFood = Ui.Node(Root, "LayerFood");
            Ui.Stretch(layerFood);
            layerEnemy = Ui.Node(Root, "LayerEnemy");
            Ui.Stretch(layerEnemy);
            layerPlayer = Ui.Node(Root, "LayerPlayer");
            Ui.Stretch(layerPlayer);
            layerFx = Ui.Node(Root, "LayerFx");
            Ui.Stretch(layerFx);

            player = MakeActor(layerPlayer, "Ikan Kecil", 90f, 90f, PlayerColor, 5, 3);
            food = MakeActor(layerFood, "Makanan Ikan", 66f, 66f, FoodColor, 0, 0);
            PlaceFood();
            SpawnEnemy();
            UpdateHud(true);
        }

        Actor MakeActor(RectTransform layer, string sprite, float w, float h, Color tint, int c, int r)
        {
            Actor a = new Actor();
            a.C = c;
            a.R = r;
            a.Pos = CellCenter(c, r);
            a.Phase = Random.Range(0f, 6.28f);
            a.Img = Ui.Picture(layer, sprite, Art.Get(sprite), a.Pos.x, a.Pos.y, w, h, tint);
            return a;
        }

        bool IsEnemyAt(int c, int r)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i].C == c && enemies[i].R == r) return true;
            }
            return false;
        }

        void RandomCell(int minDistance, out int c, out int r)
        {
            c = Random.Range(0, Cols);
            r = Random.Range(0, Rows);

            for (int i = 0; i < 200; i++)
            {
                int tc = Random.Range(0, Cols);
                int tr = Random.Range(0, Rows);
                if (Distance(tc, tr, player.C, player.R) < minDistance) continue;
                if (food != null && food.C == tc && food.R == tr) continue;
                if (IsEnemyAt(tc, tr)) continue;
                c = tc;
                r = tr;
                return;
            }
        }

        void PlaceFood()
        {
            int c;
            int r;
            RandomCell(2, out c, out r);
            food.C = c;
            food.R = r;
            food.Pos = CellCenter(c, r);
        }

        void SpawnEnemy()
        {
            int c;
            int r;
            RandomCell(5, out c, out r);
            Actor e = MakeActor(layerEnemy, "Ikan Besar", 104f, 68f, EnemyColor, c, r);
            e.Timer = Random.Range(0.3f, 0.9f);
            enemies.Add(e);
        }

        void RelocateEnemy(Actor e)
        {
            int c;
            int r;
            RandomCell(5, out c, out r);
            e.C = c;
            e.R = r;
            e.Pos = CellCenter(c, r);
            e.Timer = 0.8f;
        }

        float StepInterval()
        {
            return Mathf.Max(0.32f, 0.9f - 0.08f * (level - 1));
        }

        void MovePlayer(int dc, int dr)
        {
            int nc = Mathf.Clamp(player.C + dc, 0, Cols - 1);
            int nr = Mathf.Clamp(player.R + dr, 0, Rows - 1);
            if (nc == player.C && nr == player.R) return;

            if (dc != 0) player.FacingLeft = dc < 0;
            player.C = nc;
            player.R = nr;
            App.Sound.Play("move", 0.6f);

            if (nc == food.C && nr == food.R) Eat();
            CheckHit();
        }

        void Eat()
        {
            score += 10;
            eaten++;
            timeLeft = Mathf.Min(timeLeft + 1.5f, 99f);
            App.Sound.Play("eat");
            AddPopup("+10", food.Pos, Ui.Hex(0xFFE066));

            int newLevel = 1 + eaten / FoodPerLevel;
            if (newLevel > level)
            {
                level = newLevel;
                App.Sound.Play("level");
                AddPopup("Level " + level, new Vector2(640f, 360f), Color.white);
                if (enemies.Count < MaxEnemies) SpawnEnemy();
            }

            PlaceFood();
            UpdateHud(false);
        }

        void CheckHit()
        {
            if (Finished || invulnerable > 0f) return;

            for (int i = 0; i < enemies.Count; i++)
            {
                Actor e = enemies[i];
                if (e.C == player.C && e.R == player.R)
                {
                    Hurt(e);
                    return;
                }
            }
        }

        void Hurt(Actor e)
        {
            lives--;
            invulnerable = 2f;
            App.Sound.Play("hit");
            AddPopup("Aduh!", player.Pos, Ui.Hex(0xFF9A9A));
            RelocateEnemy(e);
            UpdateHud(false);

            if (lives <= 0)
            {
                lives = 0;
                UpdateHud(false);
                Conclude("Ikan Tertangkap!");
            }
        }

        void Conclude(string heading)
        {
            string[] lines =
            {
                "Makanan dimakan: " + eaten,
                "Level tercapai: " + level,
                "Nyawa tersisa: " + lives
            };
            int finalScore = score;
            EndGame(0.9f, () => new ResultScreen(App, "grid", heading, finalScore, lines, new int[] { 80, 150, 250 }, () => new GridScreen(App)));
        }

        void AddPopup(string text, Vector2 pos, Color tint)
        {
            Popup p = new Popup();
            p.Tint = tint;
            p.Pos = pos;
            p.Life = 0f;
            p.Label = Ui.Label(layerFx, "Popup", text, 38, tint, TextAnchor.MiddleCenter, pos.x, pos.y, 300f, 56f, Art.Body, true);
            popups.Add(p);
        }

        void HandleInput(float dt)
        {
            int dc = 0;
            int dr = 0;
            bool fresh = false;

            if (Pressed(GameKey.Left, GameKey.A)) { dc = -1; fresh = true; }
            else if (Pressed(GameKey.Right, GameKey.D)) { dc = 1; fresh = true; }
            else if (Pressed(GameKey.Up, GameKey.W)) { dr = -1; fresh = true; }
            else if (Pressed(GameKey.Down, GameKey.S)) { dr = 1; fresh = true; }

            moveCooldown -= dt;

            if (!fresh && moveCooldown <= 0f)
            {
                if (Held(GameKey.Left, GameKey.A)) dc = -1;
                else if (Held(GameKey.Right, GameKey.D)) dc = 1;
                else if (Held(GameKey.Up, GameKey.W)) dr = -1;
                else if (Held(GameKey.Down, GameKey.S)) dr = 1;
            }

            if (dc != 0 || dr != 0)
            {
                MovePlayer(dc, dr);
                moveCooldown = fresh ? 0.22f : 0.12f;
            }
        }

        void UpdateEnemies(float dt)
        {
            for (int i = 0; i < enemies.Count; i++)
            {
                Actor e = enemies[i];
                e.Timer -= dt;
                if (e.Timer > 0f) continue;
                e.Timer = StepInterval() * Random.Range(0.85f, 1.15f);

                int dc = 0;
                int dr = 0;

                if (Random.value < 0.6f)
                {
                    int diffC = player.C - e.C;
                    int diffR = player.R - e.R;
                    bool horizontal = Mathf.Abs(diffC) >= Mathf.Abs(diffR);
                    if (diffC == 0) horizontal = false;
                    if (diffR == 0) horizontal = true;

                    if (horizontal) dc = diffC > 0 ? 1 : -1;
                    else dr = diffR > 0 ? 1 : -1;
                }
                else
                {
                    int dir = Random.Range(0, 4);
                    if (dir == 0) dc = 1;
                    else if (dir == 1) dc = -1;
                    else if (dir == 2) dr = 1;
                    else dr = -1;
                }

                int nc = e.C + dc;
                int nr = e.R + dr;
                if (nc < 0 || nc >= Cols || nr < 0 || nr >= Rows) continue;
                if ((nc != player.C || nr != player.R) && IsEnemyAt(nc, nr)) continue;

                if (dc != 0) e.FacingLeft = dc < 0;
                e.C = nc;
                e.R = nr;
                CheckHit();
                if (Finished) return;
            }
        }

        void Animate(float dt)
        {
            float k = 1f - Mathf.Exp(-14f * dt);
            AnimateActor(player, k, 3f);
            for (int i = 0; i < enemies.Count; i++) AnimateActor(enemies[i], k, 4f);

            food.Pos = CellCenter(food.C, food.R);
            float pulse = 1f + Mathf.Sin(clock * 6f) * 0.08f;
            Ui.SetCenter(food.Img.rectTransform, food.Pos.x, food.Pos.y);
            food.Img.rectTransform.localScale = new Vector3(pulse, pulse, 1f);

            float alpha = 1f;
            if (invulnerable > 0f) alpha = Mathf.Sin(clock * 30f) > 0f ? 1f : 0.3f;
            Color pc = PlayerColor;
            pc.a = alpha;
            player.Img.color = pc;

            for (int i = popups.Count - 1; i >= 0; i--)
            {
                Popup p = popups[i];
                p.Life += dt;
                p.Pos.y -= 70f * dt;
                Ui.SetCenter(p.Label.rectTransform, p.Pos.x, p.Pos.y);
                Color c = p.Tint;
                c.a = Mathf.Clamp01(1f - p.Life / 0.9f);
                p.Label.color = c;
                if (p.Life >= 0.9f)
                {
                    Object.Destroy(p.Label.gameObject);
                    popups.RemoveAt(i);
                }
            }
        }

        void AnimateActor(Actor a, float k, float bob)
        {
            Vector2 goal = CellCenter(a.C, a.R);
            a.Pos = Vector2.Lerp(a.Pos, goal, k);
            Ui.SetCenter(a.Img.rectTransform, a.Pos.x, a.Pos.y + Mathf.Sin(clock * 4f + a.Phase) * bob);
            a.Img.rectTransform.localScale = new Vector3(a.FacingLeft ? -1f : 1f, 1f, 1f);
        }

        void UpdateHud(bool force)
        {
            txtScore.text = "Skor: " + score;
            txtLevel.text = "Level " + level;

            string t = "Waktu: " + Mathf.Max(0, Mathf.CeilToInt(timeLeft));
            if (force || t != lastTime)
            {
                lastTime = t;
                txtTime.text = t;
            }

            for (int i = 0; i < lifeIcons.Length; i++)
            {
                lifeIcons[i].color = i < lives ? PlayerColor : new Color(1f, 1f, 1f, 0.22f);
            }
        }

        public override void Tick(float dt)
        {
            clock += dt;
            if (Paused) return;

            Animate(dt);
            if (TickEnd(dt)) return;

            HandleInput(dt);
            UpdateEnemies(dt);

            if (invulnerable > 0f) invulnerable -= dt;

            timeLeft -= dt;
            UpdateHud(false);

            if (timeLeft <= 0f)
            {
                timeLeft = 0f;
                UpdateHud(false);
                Conclude("Waktu Habis!");
            }
        }
    }
}
