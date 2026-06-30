using System;
using UnityEngine;
using UnityEngine.UI;

namespace AutoBattle.App
{
    public static class UIFactory
    {
        private static Font _font;
        public static Font DefaultFont
        {
            get
            {
                if (_font != null) return _font;
                _font = Resources.Load<Font>("ComicNeueSansID");
                if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return _font;
            }
        }

        public static GameObject Panel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return go;
        }

        public static RectTransform Stretch(RectTransform rt, float l = 0, float r = 0, float t = 0, float b = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(l, b);
            rt.offsetMax = new Vector2(-r, -t);
            return rt;
        }

        public static RectTransform Anchor(RectTransform rt, Vector2 corner, Vector2 size, Vector2 offset)
        {
            rt.anchorMin = corner;
            rt.anchorMax = corner;
            rt.pivot = corner;
            rt.sizeDelta = size;
            rt.anchoredPosition = offset;
            return rt;
        }

        public static Text Label(Transform parent, string text, int size, TextAnchor anchor, Color color)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.text = text;
            t.font = DefaultFont;
            t.fontSize = size;
            t.alignment = anchor;
            t.color = color;
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            return t;
        }

        public static Button Button(Transform parent, string label, Color bg, Action onClick)
        {
            var go = new GameObject("Button", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = bg;

            var btn = go.GetComponent<Button>();
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            var text = Label(go.transform, label, 28, TextAnchor.MiddleCenter, Color.white);
            Stretch(text.rectTransform);
            return btn;
        }

        public static Button ImageButton(Transform parent, Sprite sprite, Vector2 size, Action onClick)
        {
            var go = new GameObject("ImageButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            img.type = Image.Type.Simple;
            go.GetComponent<RectTransform>().sizeDelta = size;

            var btn = go.GetComponent<Button>();
            btn.transition = Selectable.Transition.ColorTint;
            if (onClick != null) btn.onClick.AddListener(() => onClick());
            return btn;
        }

        public static Image Icon(Transform parent, Sprite sprite, Vector2 size)
        {
            var go = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            go.GetComponent<RectTransform>().sizeDelta = size;
            return img;
        }
    }
}
