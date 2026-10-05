using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI.EditorTools
{
    /// <summary>
    /// Kumpulan helper pembuat elemen uGUI untuk Editor Builder.
    /// Semua sprite diambil dari UITheme; bila kosong memakai sprite bawaan Unity.
    /// </summary>
    internal static class UIBuilderKit
    {
        public static UITheme Theme;
        public static Sprite Rounded;   // UISprite bawaan (sliced, sudut membulat)
        public static Sprite Circle;    // Knob bawaan (lingkaran)
        public static Sprite Square;    // Background bawaan

        public const int UILayer = 5;

        public static void Init(UITheme theme)
        {
            Theme = theme;
            Rounded = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            // Lingkaran tema (tajam) bila ada; jika tidak, Knob bawaan
            Circle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/PrambananAR/Art/Theme/circle.png");
            if (Circle == null) Circle = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
            Square = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
        }

        public static Sprite S(Sprite themed, Sprite fallback) => themed != null ? themed : fallback;

        /// <summary>Sprite tema berwarna dipakai apa adanya (putih); tanpa sprite tema pakai warna cadangan.</summary>
        public static Color Tint(Sprite themed, Color fallback) => themed != null ? Color.white : fallback;

        /// <summary>Teks putih bergaris tepi gelap (font judul) agar terbaca di atas gambar/tombol berwarna.</summary>
        public static TextMeshProUGUI Outlined(TextMeshProUGUI tmp)
        {
            if (Theme != null && Theme.titleFont != null) tmp.font = Theme.titleFont;
            if (Theme != null && Theme.titleOutlineMaterial != null) tmp.fontSharedMaterial = Theme.titleOutlineMaterial;
            tmp.fontStyle = FontStyles.Normal;
            return tmp;
        }

        // ------------------------------------------------------------------
        // RectTransform
        // ------------------------------------------------------------------
        public static RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = UILayer;
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Regangkan penuh ke parent dengan padding (kiri, atas, kanan, bawah).</summary>
        public static RectTransform Stretch(RectTransform rt, float left = 0, float top = 0, float right = 0, float bottom = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        /// <summary>Tempatkan pada titik anchor; pivot = anchor agar offset mudah dibaca.</summary>
        public static RectTransform Place(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = anchor;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        /// <summary>Pita horizontal menempel di atas (y = jarak dari atas).</summary>
        public static RectTransform TopBand(RectTransform rt, float height, float y = 0, float side = 0)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.offsetMin = new Vector2(side, -y - height);
            rt.offsetMax = new Vector2(-side, -y);
            return rt;
        }

        /// <summary>Pita horizontal menempel di bawah (y = jarak dari bawah).</summary>
        public static RectTransform BottomBand(RectTransform rt, float height, float y = 0, float side = 0)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.offsetMin = new Vector2(side, y);
            rt.offsetMax = new Vector2(-side, y + height);
            return rt;
        }

        // ------------------------------------------------------------------
        // Komponen visual
        // ------------------------------------------------------------------
        public static Image AddImage(GameObject go, Sprite sprite, Color color, bool raycast = false)
        {
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = raycast;
            img.type = sprite != null && sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            return img;
        }

        public static Image Img(string name, Transform parent, Sprite sprite, Color color, bool raycast = false)
            => AddImage(Rect(name, parent).gameObject, sprite, color, raycast);

        public static Image Filled(string name, Transform parent, Sprite sprite, Color color, Image.FillMethod method, int origin = 0)
        {
            var img = Img(name, parent, sprite != null ? sprite : Square, color);
            img.type = Image.Type.Filled;
            img.fillMethod = method;
            img.fillOrigin = origin;
            img.fillAmount = 1f;
            return img;
        }

        public static TextMeshProUGUI Text(string name, Transform parent, string text, float size, Color color,
            TextAlignmentOptions align = TextAlignmentOptions.Center, bool bold = false, bool title = false)
        {
            var rt = Stretch(Rect(name, parent));
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            var font = title && Theme != null && Theme.titleFont != null ? Theme.titleFont : Theme != null ? Theme.font : null;
            if (font != null) tmp.font = font;
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = align;
            tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
            tmp.raycastTarget = false; // teks tidak perlu menerima sentuhan (hemat raycast)
            tmp.overflowMode = TextOverflowModes.Ellipsis;
            return tmp;
        }

        public static Button Button(string name, Transform parent, Sprite bg, Color bgColor, string label, float fontSize,
            Vector2 size, Color? labelColor = null)
        {
            var rt = Rect(name, parent);
            rt.sizeDelta = size;
            // Tombol tema sudah berwarna; hanya tombol bulat & chip (putih) yang diwarnai
            bool tintable = bg == null || bg == Theme.buttonCircle || bg == Theme.chip;
            var img = AddImage(rt.gameObject, S(bg, Rounded), tintable ? bgColor : Color.white, true);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = img;
            var colors = btn.colors;
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
            colors.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.6f);
            btn.colors = colors;
            rt.gameObject.AddComponent<ButtonPressFeedback>();

            if (!string.IsNullOrEmpty(label))
            {
                var text = Text("Label", rt, label, fontSize, labelColor ?? Theme.textLight, TextAlignmentOptions.Center, true, true);
                Stretch(text.rectTransform, 16, 6, 16, 10);
                if (!labelColor.HasValue) Outlined(text);
            }
            return btn;
        }

        /// <summary>Tombol bulat ikon. Jika ikon kosong, tampilkan glyph teks pengganti.</summary>
        public static Button IconButton(string name, Transform parent, Sprite icon, string glyph, float size, Color bgColor, Sprite bg = null)
        {
            var btn = Button(name, parent, S(bg, S(Theme.buttonCircle, Circle)), bgColor, null, 0, new Vector2(size, size));
            if (icon != null)
            {
                var ic = Img("Icon", btn.transform, icon, Color.white);
                Stretch(ic.rectTransform, size * 0.2f, size * 0.2f, size * 0.2f, size * 0.2f);
                ic.preserveAspect = true;
            }
            else
            {
                Outlined(Text("Glyph", btn.transform, glyph, size * 0.42f, Theme.textLight, TextAlignmentOptions.Center, true, true));
            }
            return btn;
        }

        /// <summary>Ikon kecil (gambar bila ada, jika tidak glyph teks).</summary>
        public static RectTransform Icon(string name, Transform parent, Sprite icon, string glyph, float size, Color glyphColor)
        {
            if (icon != null)
            {
                var img = Img(name, parent, icon, Color.white);
                img.preserveAspect = true;
                img.rectTransform.sizeDelta = new Vector2(size, size);
                return img.rectTransform;
            }
            var rt = Rect(name, parent);
            rt.sizeDelta = new Vector2(size, size);
            AddImage(rt.gameObject, Circle, glyphColor);
            Text("Glyph", rt, glyph, size * 0.5f, Color.white, TextAlignmentOptions.Center, true);
            return rt;
        }

        public static Toggle Toggle(string name, Transform parent, string label, float fontSize, Vector2 size)
        {
            var rt = Rect(name, parent);
            rt.sizeDelta = size;
            var toggle = rt.gameObject.AddComponent<Toggle>();

            var box = Img("Box", rt, S(Theme.toggleBox, Rounded), Color.white, true);
            Place(box.rectTransform, new Vector2(0, 0.5f), Vector2.zero, new Vector2(size.y, size.y));
            var check = Img("Check", box.transform, S(Theme.toggleCheck, Circle), Theme.success);
            Stretch(check.rectTransform, 14, 14, 14, 14);

            var text = Text("Label", rt, label, fontSize, Theme.textDark, TextAlignmentOptions.MidlineLeft);
            Stretch(text.rectTransform, size.y + 24, 0, 0, 0);

            toggle.targetGraphic = box;
            toggle.graphic = check;
            toggle.isOn = false;
            return toggle;
        }

        /// <summary>ScrollRect vertikal/horizontal dengan RectMask2D (lebih ringan dari Mask).</summary>
        public static ScrollRect Scroll(string name, Transform parent, bool vertical, out RectTransform content)
        {
            var rt = Rect(name, parent);
            var scroll = rt.gameObject.AddComponent<ScrollRect>();
            var viewport = Stretch(Rect("Viewport", rt));
            viewport.gameObject.AddComponent<RectMask2D>();
            AddImage(viewport.gameObject, null, new Color(0, 0, 0, 0), true); // area sentuh untuk drag

            content = Rect("Content", viewport);
            if (vertical)
            {
                content.anchorMin = new Vector2(0, 1);
                content.anchorMax = new Vector2(1, 1);
                content.pivot = new Vector2(0.5f, 1);
                content.sizeDelta = Vector2.zero;
                var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = !vertical;
            scroll.vertical = vertical;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 30f;
            return scroll;
        }

        /// <summary>Daftar geser horizontal: konten melebar otomatis mengikuti jumlah item.</summary>
        public static ScrollRect HorizontalList(string name, Transform parent, float spacing, RectOffset padding, out RectTransform content)
        {
            var scroll = Scroll(name, parent, false, out content);
            content.anchorMin = new Vector2(0, 0);
            content.anchorMax = new Vector2(0, 1);
            content.pivot = new Vector2(0, 0.5f);
            content.sizeDelta = Vector2.zero;
            content.anchoredPosition = Vector2.zero;
            var hlg = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = spacing;
            hlg.padding = padding;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            return scroll;
        }

        // ------------------------------------------------------------------
        // Binding field [SerializeField] lewat SerializedObject
        // ------------------------------------------------------------------
        public static void Bind(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogError($"[UIBuilder] Field '{field}' tidak ditemukan pada {target.GetType().Name}");
                return;
            }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void BindArray(Object target, string field, IList<Object> values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null || !prop.isArray)
            {
                Debug.LogError($"[UIBuilder] Array '{field}' tidak ditemukan pada {target.GetType().Name}");
                return;
            }
            prop.arraySize = values.Count;
            for (int i = 0; i < values.Count; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetBool(Object target, string field, bool value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogError($"[UIBuilder] Field '{field}' tidak ditemukan pada {target.GetType().Name}");
                return;
            }
            prop.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public static void SetInt(Object target, string field, int value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogError($"[UIBuilder] Field '{field}' tidak ditemukan pada {target.GetType().Name}");
                return;
            }
            prop.intValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
