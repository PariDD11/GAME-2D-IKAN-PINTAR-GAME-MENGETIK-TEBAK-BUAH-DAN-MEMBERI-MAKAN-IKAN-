using UnityEngine;
using UnityEngine.UI;

namespace IkanPintar
{
    public static class Ui
    {
        public static Color Hex(int rgb, float alpha = 1f)
        {
            return new Color(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f, alpha);
        }

        public static RectTransform Node(Transform parent, string name)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        public static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static void Place(RectTransform rt, float cx, float cy, float w, float h)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(w, h);
            rt.anchoredPosition = new Vector2(cx, -cy);
        }

        public static void SetCenter(RectTransform rt, float cx, float cy)
        {
            rt.anchoredPosition = new Vector2(cx, -cy);
        }

        public static Image Picture(Transform parent, string name, Sprite sprite, float cx, float cy, float w, float h, Color color)
        {
            RectTransform rt = Node(parent, name);
            Image image = rt.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            Place(rt, cx, cy, w, h);
            return image;
        }

        public static Image Panel(Transform parent, string name, Color color, float cx, float cy, float w, float h)
        {
            return Picture(parent, name, null, cx, cy, w, h, color);
        }

        public static Image Fill(Transform parent, string name, Color color)
        {
            RectTransform rt = Node(parent, name);
            Image image = rt.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            Stretch(rt);
            return image;
        }

        public static Text Label(Transform parent, string name, string text, int size, Color color, TextAnchor align,
            float cx, float cy, float w, float h, Font font = null, bool shadow = false)
        {
            RectTransform rt = Node(parent, name);
            Text t = rt.gameObject.AddComponent<Text>();
            t.font = font != null ? font : Art.Body;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.supportRichText = true;
            t.raycastTarget = false;
            Place(rt, cx, cy, w, h);

            if (shadow)
            {
                Shadow sh = rt.gameObject.AddComponent<Shadow>();
                sh.effectColor = new Color(0f, 0.12f, 0.25f, 0.45f);
                sh.effectDistance = new Vector2(3f, -3f);
            }

            return t;
        }
    }

    public class UiBar
    {
        public readonly Image Back;
        public readonly Image Fill;
        readonly float width;
        readonly float height;

        public UiBar(Transform parent, string name, float cx, float cy, float w, float h, Color back, Color fill)
        {
            Back = Ui.Panel(parent, name + "Back", back, cx, cy, w, h);
            Fill = Ui.Panel(parent, name + "Fill", fill, cx, cy, w, h);
            RectTransform rt = Fill.rectTransform;
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(cx - w / 2f, -cy);
            width = w;
            height = h;
        }

        public void Set(float fraction)
        {
            fraction = Mathf.Clamp01(fraction);
            Fill.rectTransform.sizeDelta = new Vector2(width * fraction, height);
        }
    }

    public class UiButton
    {
        public readonly RectTransform Rt;
        public readonly Image Back;
        public readonly Text Label;
        public System.Action Click;
        public bool Enabled = true;

        Color normal = Color.white;
        Color hover = new Color(1f, 0.95f, 0.69f, 1f);

        public UiButton(Transform parent, string name, string text, float cx, float cy, float w, float h, int fontSize, System.Action click)
        {
            Back = Ui.Picture(parent, name, Art.Get("Bg Tombol"), cx, cy, w, h, normal);
            Rt = Back.rectTransform;
            Label = Ui.Label(Rt, "Label", text, fontSize, Ui.Hex(0x0B5C99), TextAnchor.MiddleCenter, w / 2f, h / 2f, w - 24f, h);
            Click = click;
        }

        public bool Contains(Vector2 screenPoint)
        {
            return Rt != null && Rt.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(Rt, screenPoint, null);
        }

        public void SetHover(bool on)
        {
            bool active = on && Enabled;
            Back.color = active ? hover : normal;
            float s = active ? 1.04f : 1f;
            Rt.localScale = new Vector3(s, s, 1f);
        }

        public void SetTint(Color color)
        {
            normal = color;
            hover = color;
            Back.color = color;
        }

        public void ResetTint()
        {
            normal = Color.white;
            hover = new Color(1f, 0.95f, 0.69f, 1f);
            Back.color = normal;
        }

        public void SetText(string text)
        {
            Label.text = text;
        }
    }
}
