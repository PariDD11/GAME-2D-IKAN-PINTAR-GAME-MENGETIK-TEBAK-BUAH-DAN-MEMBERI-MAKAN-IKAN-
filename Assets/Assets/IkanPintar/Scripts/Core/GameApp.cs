using UnityEngine;
using UnityEngine.UI;

namespace IkanPintar
{
    public static class Bootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Begin()
        {
            if (GameObject.Find("IkanPintar") != null) return;
            GameObject go = new GameObject("IkanPintar");
            go.AddComponent<GameApp>();
        }
    }

    public class GameApp : MonoBehaviour
    {
        public const float StageWidth = 1280f;
        public const float StageHeight = 720f;

        public Sfx Sound { get; private set; }
        public RectTransform Stage { get; private set; }

        CanvasScaler scaler;
        GameScreen current;
        System.Func<GameScreen> pending;

        void Awake()
        {
            Application.runInBackground = true;
            PrepareCamera();
            Art.Load();
            BuildCanvas();
            Sound = new Sfx(gameObject);
            Go(() => new MenuScreen(this));
        }

        void PrepareCamera()
        {
            Camera cam = Camera.main;
            if (cam == null && Camera.allCamerasCount > 0) cam = Camera.allCameras[0];

            if (cam == null)
            {
                GameObject go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
                go.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Ui.Hex(0x1080CA);
        }

        void BuildCanvas()
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(StageWidth, StageHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

            RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
            Ui.Fill(canvasRect, "Backdrop", Ui.Hex(0x1080CA));

            RectTransform stage = Ui.Node(canvasRect, "Stage");
            stage.anchorMin = new Vector2(0.5f, 0.5f);
            stage.anchorMax = new Vector2(0.5f, 0.5f);
            stage.pivot = new Vector2(0.5f, 0.5f);
            stage.sizeDelta = new Vector2(StageWidth, StageHeight);
            stage.anchoredPosition = Vector2.zero;
            stage.gameObject.AddComponent<RectMask2D>();
            Stage = stage;

            FitCanvas();
        }

        void FitCanvas()
        {
            if (scaler == null || Screen.height <= 0) return;
            float aspect = (float)Screen.width / Screen.height;
            scaler.matchWidthOrHeight = aspect >= StageWidth / StageHeight ? 1f : 0f;
        }

        public void Go(System.Func<GameScreen> factory)
        {
            pending = factory;
        }

        void Swap()
        {
            System.Func<GameScreen> make = pending;
            pending = null;

            if (current != null)
            {
                current.Close();
                current = null;
            }

            GameInput.ClearTyped();
            current = make();
            current.Open();
        }

        void Update()
        {
            GameInput.Poll();
            FitCanvas();

            if (pending != null) Swap();

            if (current != null)
            {
                float dt = Mathf.Min(Time.unscaledDeltaTime, 0.1f);
                if (GameInput.WasPressed(GameKey.Escape)) current.Back();
                current.UpdateButtons();
                current.Tick(dt);
            }

            if (pending != null) Swap();
        }

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
