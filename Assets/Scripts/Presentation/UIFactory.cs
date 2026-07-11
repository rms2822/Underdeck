using UnityEngine;
using UnityEngine.UI;

namespace Underdeck.Presentation
{
    /// <summary>
    /// Small helpers for building uGUI at runtime — no scene/prefab authoring
    /// required. Every screen in <see cref="GameBootstrap"/> is assembled from
    /// these. Uses the legacy UI.Text/Image/Button stack (not TextMeshPro) so
    /// the project has zero package-import steps beyond a stock Unity install.
    /// </summary>
    public static class UIFactory
    {
        public static readonly Color Ground = new Color32(14, 18, 22, 255);
        public static readonly Color Panel = new Color32(30, 38, 48, 255);
        public static readonly Color PanelLight = new Color32(42, 52, 64, 255);
        public static readonly Color Gold = new Color32(217, 164, 65, 255);
        public static readonly Color Bone = new Color32(223, 216, 200, 255);
        public static readonly Color BoneDim = new Color32(151, 145, 127, 255);
        public static readonly Color Blood = new Color32(192, 52, 52, 255);
        public static readonly Color Verd = new Color32(127, 168, 143, 255);
        public static readonly Color Steel = new Color32(155, 170, 184, 255);
        public static readonly Color Backdrop = new Color(0.02f, 0.03f, 0.04f, 0.82f);

        private static Font _font;
        public static Font DefaultFont
        {
            get
            {
                if (_font == null)
                {
                    // Unity 2022+ renamed the built-in legacy font resource; fall back for older editors.
                    _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")
                            ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                }
                return _font;
            }
        }

        public static RectTransform Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return Stretch(go.GetComponent<RectTransform>());
        }

        public static VerticalLayoutGroup AddVertical(GameObject go, int spacing = 10, RectOffset padding = null,
            TextAnchor align = TextAnchor.UpperCenter, bool expandWidth = true, bool expandHeight = false)
        {
            var v = go.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing;
            v.padding = padding ?? new RectOffset(16, 16, 16, 16);
            v.childAlignment = align;
            v.childForceExpandWidth = expandWidth;
            v.childForceExpandHeight = expandHeight;
            v.childControlWidth = true;
            v.childControlHeight = true;
            return v;
        }

        public static HorizontalLayoutGroup AddHorizontal(GameObject go, int spacing = 8, RectOffset padding = null,
            TextAnchor align = TextAnchor.MiddleCenter, bool expandWidth = true, bool expandHeight = true)
        {
            var h = go.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.padding = padding ?? new RectOffset(0, 0, 0, 0);
            h.childAlignment = align;
            h.childForceExpandWidth = expandWidth;
            h.childForceExpandHeight = expandHeight;
            h.childControlWidth = true;
            h.childControlHeight = true;
            return h;
        }

        public static GridLayoutGroup AddGrid(GameObject go, Vector2 cellSize, int columns, int spacing = 10)
        {
            var g = go.AddComponent<GridLayoutGroup>();
            g.cellSize = cellSize;
            g.spacing = new Vector2(spacing, spacing);
            g.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            g.constraintCount = columns;
            g.childAlignment = TextAnchor.UpperCenter;
            return g;
        }

        public static RectTransform NewChild(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go.GetComponent<RectTransform>();
        }

        public static Text CreateText(Transform parent, string text, int size, Color color,
            TextAnchor align = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = DefaultFont;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.text = text;
            return t;
        }

        public static LayoutElement Fixed(GameObject go, float height = -1, float width = -1)
        {
            var le = go.AddComponent<LayoutElement>();
            if (height > 0) { le.preferredHeight = height; le.minHeight = height; }
            if (width > 0) { le.preferredWidth = width; le.minWidth = width; }
            return le;
        }

        /// <summary>A full-width button with a title line and an optional dim subtitle line.</summary>
        public static Button CreateOptionButton(Transform parent, string title, string subtitle,
            Color bg, bool interactable, System.Action onClick)
        {
            var go = new GameObject("Option", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = bg;
            Fixed(go, height: string.IsNullOrEmpty(subtitle) ? 64 : 80);

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.interactable = interactable;
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            var col = NewChild(go.transform, "Col");
            Stretch(col).offsetMin = new Vector2(14, 6); col.offsetMax = new Vector2(-14, -6);
            AddVertical(col.gameObject, spacing: 2, padding: new RectOffset(0, 0, 0, 0), align: TextAnchor.MiddleLeft);

            var titleColor = interactable ? Bone : BoneDim;
            CreateText(col, title, 22, titleColor, TextAnchor.MiddleLeft);
            if (!string.IsNullOrEmpty(subtitle))
                CreateText(col, subtitle, 15, BoneDim, TextAnchor.MiddleLeft, FontStyle.Italic);
            return btn;
        }

        /// <summary>A compact button for the room grid / hand row (art glyph + label, no subtitle).</summary>
        public static Button CreateTile(Transform parent, string art, string label, string corner,
            Color bg, bool selected, System.Action onClick)
        {
            var go = new GameObject("Tile", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = selected ? Gold : bg;

            var btn = go.GetComponent<Button>();
            btn.targetGraphic = img;
            if (onClick != null) btn.onClick.AddListener(() => onClick());

            var col = NewChild(go.transform, "Col");
            Stretch(col);
            AddVertical(col.gameObject, spacing: 2, padding: new RectOffset(4, 4, 6, 6));

            if (!string.IsNullOrEmpty(corner))
                CreateText(col, corner, 15, Gold, TextAnchor.UpperCenter);
            CreateText(col, art, 30, Bone, TextAnchor.MiddleCenter);
            CreateText(col, label, 13, BoneDim, TextAnchor.LowerCenter);
            return btn;
        }

        /// <summary>A simple non-interactive HP bar: a filled Image, no Slider needed.</summary>
        public static void CreateHpBar(Transform parent, float fill01)
        {
            var track = new GameObject("HpTrack", typeof(RectTransform), typeof(Image));
            track.transform.SetParent(parent, false);
            Fixed(track, height: 14);
            track.GetComponent<Image>().color = new Color(0, 0, 0, 0.5f);

            var fillGo = new GameObject("HpFill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(track.transform, false);
            var fillRt = Stretch(fillGo.GetComponent<RectTransform>());
            fillRt.pivot = new Vector2(0, 0.5f);
            var fillImg = fillGo.GetComponent<Image>();
            fillImg.color = fill01 > 0.35f ? Verd : Blood;
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImg.fillAmount = Mathf.Clamp01(fill01);
        }
    }
}
