using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace PrambananAR.UI.EditorTools.Art
{
    /// <summary>
    /// Membuat aset visual bertema "Batu Candi & Emas" untuk UI Prambanan AR:
    /// - potongan 2D Game UI Kit (300Mind) yang diwarnai ulang sesuai tema,
    /// - ikon & elemen UI prosedural yang tidak ada di kit,
    /// - ilustrasi (peta, latar loading, pemandu, cerita) — lihat file .Illustrations.
    /// Hasil disimpan di Assets/PrambananAR/Art lalu dipasang ke UITheme oleh SampleContentFactory.
    /// </summary>
    public static partial class ThemeArtGenerator
    {
        public const string ThemeDir = "Assets/PrambananAR/Art/Theme";
        public const string IlluDir = "Assets/PrambananAR/Art/Illustrations";
        private const string KitSheet1 = "Assets/300Mind/2D Game UI Kit/Sprites/UI-pack_Sprite_1.png";
        private const string KitSheet2 = "Assets/300Mind/2D Game UI Kit/Sprites/UI-pack_Sprite_2.png";

        [MenuItem("Tools/Prambanan AR/0. Buat Aset Visual Tema", priority = 0)]
        public static void GenerateAll()
        {
            try
            {
                EditorUtility.DisplayProgressBar("Prambanan AR", "Membuat elemen UI...", 0.1f);
                GenerateUI();
                EditorUtility.DisplayProgressBar("Prambanan AR", "Membuat ilustrasi...", 0.5f);
                GenerateIllustrations();
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Prambanan AR] Aset visual tema dibuat di " + ThemeDir + " dan " + IlluDir);
        }

        public static void GenerateUI()
        {
            GenerateKitPieces();
            GenerateElements();
            GenerateIcons();
        }

        // ==================================================================
        // 1) Potongan 2D Game UI Kit diwarnai ulang
        // ==================================================================
        private static void GenerateKitPieces()
        {
            var sheet1 = Img.Load(KitSheet1);
            var sheet2 = Img.Load(KitSheet2);
            var sprites = new Dictionary<string, Sprite>();
            foreach (var path in new[] { KitSheet1, KitSheet2 })
                foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path))
                    if (o is Sprite s) sprites[s.name.Replace("UI-pack_Sprite_", "S")] = s;

            Img Kit(string id) => (id.StartsWith("S1") ? sheet1 : sheet2).CropSprite(sprites[id]);

            // Tombol: pil hijau & merah dipakai apa adanya, pil emas & biru-toska hasil geser hue
            var greenPill = Kit("S2_16");
            SaveImg(greenPill, "btn_success", 1f, new Vector4(70, 44, 70, 44));
            SaveImg(Kit("S2_17"), "btn_danger", 1f, new Vector4(70, 44, 70, 44));
            SaveImg(greenPill.GradientMap(Pal.Hex("6E3A12"), Pal.Hex("D9821F"), Pal.Hex("F7B733"), Pal.Hex("FFE38A"), Pal.Hex("FFFBEA")),
                "btn_primary", 1f, new Vector4(70, 44, 70, 44));
            SaveImg(greenPill.HueShift(72f, 0.95f, 1f), "btn_secondary", 1f, new Vector4(70, 44, 70, 44));

            // Panel popup: bingkai bertakik (mirip relief candi) -> warna perkamen
            var notched = Kit("S2_4").GradientMap(Pal.Hex("6B4423"), Pal.Hex("B9844A"), Pal.Hex("EFD3A0"), Pal.Hex("FFF6E3"));
            SaveImg(notched, "panel_popup", 0.5f, new Vector4(72, 72, 72, 72));

            // Kartu: panel biru polos -> krem
            var card = Kit("S2_0").GradientMap(Pal.Hex("9A6A3C"), Pal.Hex("E9CB98"), Pal.Hex("FFF4DC"), Pal.Hex("FFFCF4"));
            SaveImg(card, "panel_card", 0.35f, new Vector4(40, 40, 40, 40));

            // Banner misi: pita ungu bergaris -> terakota
            SaveImg(Kit("S2_1").HueShift(86f, 0.95f, 1.05f), "panel_header", 0.7f, new Vector4(60, 40, 60, 40));

            // Bar HUD: biru tua -> coklat tua
            var bar = Kit("S2_5").GradientMap(Pal.Hex("24160D"), Pal.Hex("3E2819"), Pal.Hex("5E3E26"));
            SaveImg(bar, "top_bar", 0.6f, new Vector4(40, 30, 40, 30));

            // Kotak dialog: tiket kuning (sudah bernuansa emas & krem)
            var ticket = Kit("S1_67");
            SaveImg(ticket, "dialog_box", 0.75f, new Vector4(60, 40, 60, ticket.H * 0.75f * 0.58f));
            // Pita nama tokoh
            SaveImg(Kit("S1_45"), "name_tag", 1f, new Vector4(80, 20, 80, 20));

            // Ikon berwarna dari kit
            SaveImg(Kit("S1_16"), "icon_coin", 1f, Vector4.zero);
            SaveImg(Kit("S2_14"), "hand_pointer", 1f, Vector4.zero);
            SaveImg(Kit("S2_13"), "icon_badge", 1f, Vector4.zero);
        }

        // ==================================================================
        // 2) Elemen UI prosedural (putih = bisa diwarnai lewat Image.color)
        // ==================================================================
        private static void GenerateElements()
        {
            // Tombol bulat mengilap (putih, diwarnai builder)
            {
                var c = new ArtCanvas(160, 160);
                c.Shadow(Sdf_.Circle(80, 82, 74), 0, 5, 8, Pal.A(Color.black, 0.3f));
                c.Fill(Sdf_.Circle(80, 80, 76), Pal.Hex("2E2A28"));
                c.Fill(Sdf_.Circle(80, 84, 70), Pal.Hex("9C9C9C"));
                c.Fill(Sdf_.Circle(80, 78, 68), Pal.VGrad(12, 146, Color.white, Pal.Hex("E4E4E4")));
                c.Fill(Sdf_.Ellipse(70, 42, 36, 16), Pal.A(Color.white, 0.75f), 3f);
                Save(c, "btn_circle", Vector4.zero);
            }

            // Lingkaran putih tajam (pengganti Knob bawaan Unity yang buram bila diperbesar)
            {
                var c = new ArtCanvas(256, 256);
                c.Fill(Sdf_.Circle(128, 128, 126), Color.white);
                Save(c, "circle", Vector4.zero);
            }

            // Pil kecil (chip/tag), putih
            {
                var c = new ArtCanvas(160, 72);
                c.Fill(Sdf_.Box(2, 4, 156, 66, 33), Pal.Hex("BDBDBD"));
                c.Fill(Sdf_.Box(2, 2, 156, 62, 31), Color.white);
                Save(c, "chip", new Vector4(36, 30, 36, 30));
            }

            // Cincin target AR & cincin progres (putih + tepi gelap tipis agar terlihat di kamera)
            {
                var c = new ArtCanvas(256, 256);
                var ring = Sdf_.Annulus(Sdf_.Circle(128, 128, 112), 11);
                c.Fill(Sdf_.Offset(ring, 3), Pal.A(Color.black, 0.35f), 2f);
                c.Fill(ring, Color.white);
                Save(c, "ring", Vector4.zero);
            }

            // Sinar cahaya (sunburst) untuk naik level & loading
            {
                var c = new ArtCanvas(512, 512);
                c.Fill(Sdf_.Circle(256, 256, 255), (x, y) =>
                {
                    float dx = x - 256, dy = y - 256;
                    float r = Mathf.Sqrt(dx * dx + dy * dy) / 256f;
                    float ang = Mathf.Atan2(dy, dx);
                    float ray = Mathf.Pow(0.5f + 0.5f * Mathf.Cos(ang * 14f), 3f);
                    float fall = Mathf.Pow(Mathf.Clamp01(1f - r), 1.6f);
                    return Pal.A(Color.white, fall * (0.25f + 0.75f * ray));
                }, 2f);
                Save(c, "glow", Vector4.zero);
            }

            // Lampu sorot (kerucut cahaya) untuk layar dialog
            {
                var c = new ArtCanvas(512, 1024);
                var cone = Sdf_.Polygon(new Vector2(196, 0), new Vector2(316, 0), new Vector2(500, 1024), new Vector2(12, 1024));
                c.Fill(cone, (x, y) => Pal.A(Color.white, Mathf.Lerp(0.9f, 0.25f, y / 1024f)), 40f);
                Save(c, "spotlight", Vector4.zero);
            }

            // Lencana level: matahari bersudut 8 (terinspirasi Surya Majapahit)
            {
                var c = new ArtCanvas(256, 256);
                var sun = Sdf_.Offset(Sdf_.Star(128, 128, 112, 78, 8, 22.5f), 6);
                c.Shadow(sun, 0, 6, 10, Pal.A(Color.black, 0.35f));
                c.FillOutlined(sun, Pal.VGrad(10, 246, Pal.GoldLight, Pal.GoldDark), Pal.Outline, 6);
                var rays = Sdf_.Offset(Sdf_.Star(128, 128, 100, 70, 8), 3);
                c.Fill(rays, Pal.VGrad(30, 230, Pal.Hex("FFE9A3"), Pal.Gold));
                c.FillOutlined(Sdf_.Circle(128, 128, 76), Pal.VGrad(52, 204, Pal.GoldLight, Pal.GoldDark), Pal.Outline, 5);
                c.FillOutlined(Sdf_.Circle(128, 128, 62), Pal.VGrad(66, 190, Pal.Hex("E8744A"), Pal.Hex("A8361F")), Pal.Hex("7A2A16"), 3);
                c.Fill(Sdf_.Intersect(Sdf_.Annulus(Sdf_.Circle(128, 128, 52), 4), Sdf_.Above(110)), Pal.A(Color.white, 0.35f));
                Save(c, "level_badge", Vector4.zero);
            }

            // Pin peta (kepala bulat + ekor), putih untuk diwarnai
            {
                var c = new ArtCanvas(160, 160);
                c.Fill(Sdf_.Circle(80, 80, 76), Pal.Hex("2E2A28"));
                c.Fill(Sdf_.Circle(80, 80, 68), Pal.VGrad(12, 148, Color.white, Pal.Hex("DADADA")));
                c.Fill(Sdf_.Ellipse(66, 40, 30, 13), Pal.A(Color.white, 0.8f), 3f);
                Save(c, "map_pin", Vector4.zero);

                var t = new ArtCanvas(96, 96);
                var tail = Sdf_.Offset(Sdf_.Polygon(new Vector2(18, 6), new Vector2(78, 6), new Vector2(48, 82)), 4);
                t.FillOutlined(tail, Color.white, Pal.Hex("2E2A28"), 6);
                Save(t, "map_pin_tail", Vector4.zero);
            }

            // Penanda pemain
            {
                var c = new ArtCanvas(128, 128);
                c.Shadow(Sdf_.Circle(64, 64, 50), 0, 4, 10, Pal.A(Color.black, 0.4f));
                c.FillOutlined(Sdf_.Circle(64, 64, 50), Color.white, Pal.Hex("2E2A28"), 6);
                Save(c, "player_marker", Vector4.zero);
            }

            // Kotak centang (persetujuan orang tua)
            {
                var c = new ArtCanvas(96, 96);
                c.FillOutlined(Sdf_.Box(8, 8, 80, 80, 18), Pal.VGrad(8, 88, Color.white, Pal.Hex("F3EADB")), Pal.Brown, 5);
                Save(c, "toggle_box", new Vector4(30, 30, 30, 30));
                Glyph("toggle_check", CheckMark(96f / 128f), 96);
            }

            // Bar progres: latar coklat gelap + isi mengilap (putih, diwarnai)
            {
                var c = new ArtCanvas(128, 56);
                c.Fill(Sdf_.Box(0, 0, 128, 56, 28), Pal.A(Pal.Hex("2A1A10"), 0.92f));
                c.Fill(Sdf_.Box(4, 4, 120, 48, 24), Pal.VGrad(4, 52, Pal.Hex("1C120B"), Pal.Hex("3B2818")));
                Save(c, "progress_bg", new Vector4(28, 0, 28, 0));

                var f = new ArtCanvas(64, 32);
                f.Fill(Sdf_.Box(0, 0, 64, 32, 0), Pal.VGrad(0, 32, Color.white, Pal.Hex("E8E8E8"), Pal.Hex("C9C9C9")));
                f.Fill(Sdf_.Box(0, 5, 64, 6, 0), Pal.A(Color.white, 0.7f));
                Save(f, "progress_fill", Vector4.zero);
            }
        }

        // ==================================================================
        // 3) Ikon
        // ==================================================================
        private static void GenerateIcons()
        {
            // --- Ikon glyph putih bergaris tepi (diletakkan di atas tombol bulat berwarna) ---
            Glyph("icon_close", Sdf_.Union(Sdf_.Capsule(38, 38, 90, 90, 12), Sdf_.Capsule(90, 38, 38, 90, 12)));
            Glyph("icon_back", Sdf_.Union(Sdf_.Capsule(36, 64, 94, 64, 11), Sdf_.Capsule(36, 64, 62, 38, 11), Sdf_.Capsule(36, 64, 62, 90, 11)));
            Glyph("icon_prev", Sdf_.Union(Sdf_.Capsule(76, 34, 46, 64, 13), Sdf_.Capsule(46, 64, 76, 94, 13)));
            Glyph("icon_next", Sdf_.Union(Sdf_.Capsule(52, 34, 82, 64, 13), Sdf_.Capsule(82, 64, 52, 94, 13)));
            Glyph("icon_menu", Sdf_.Union(Sdf_.Capsule(32, 38, 96, 38, 9), Sdf_.Capsule(32, 64, 96, 64, 9), Sdf_.Capsule(32, 90, 96, 90, 9)));
            Glyph("icon_play", Sdf_.Offset(Sdf_.Polygon(new Vector2(50, 40), new Vector2(50, 88), new Vector2(90, 64)), 9));

            // Tanda tanya
            var arc = Sdf_.Subtract(Sdf_.Annulus(Sdf_.Circle(64, 46, 22), 10), (x, y) => Mathf.Max(x - 64, 46 - y));
            Glyph("icon_help", Sdf_.Union(arc, Sdf_.Circle(42, 46, 10), Sdf_.Capsule(64, 68, 64, 80, 10), Sdf_.Circle(64, 102, 10)));

            // Kamera
            var camBody = Sdf_.Union(Sdf_.RoundRect(64, 72, 48, 32, 12), Sdf_.RoundRect(48, 40, 16, 10, 5));
            var cam = Sdf_.Union(Sdf_.Subtract(camBody, Sdf_.Circle(64, 72, 21)), Sdf_.Circle(64, 72, 11));
            Glyph("icon_camera", cam, 128, c => c.Fill(Sdf_.Circle(96, 56, 5), Pal.Outline));

            // Radar "Di Sekitar"
            var radar = Sdf_.Union(Sdf_.Annulus(Sdf_.Circle(64, 64, 44), 7), Sdf_.Annulus(Sdf_.Circle(64, 64, 24), 7), Sdf_.Circle(64, 64, 8),
                Sdf_.Capsule(64, 64, 94, 34, 5));
            Glyph("icon_nearby", radar);

            // Gelembung bicara (Panduan)
            var bubble = Sdf_.Union(Sdf_.RoundRect(64, 56, 46, 32, 16),
                Sdf_.Polygon(new Vector2(38, 80), new Vector2(62, 84), new Vector2(30, 106)));
            Glyph("icon_guide", bubble, 128, c =>
            {
                for (int i = 0; i < 3; i++) c.Fill(Sdf_.Circle(42 + i * 22, 56, 7), Pal.Outline);
            });

            // Buku terbuka (Cerita)
            var book = Sdf_.Union(
                Sdf_.Offset(Sdf_.Polygon(new Vector2(18, 38), new Vector2(60, 46), new Vector2(60, 100), new Vector2(18, 92)), 4),
                Sdf_.Offset(Sdf_.Polygon(new Vector2(68, 46), new Vector2(110, 38), new Vector2(110, 92), new Vector2(68, 100)), 4));
            Glyph("icon_story", book, 128, c =>
            {
                for (int i = 0; i < 3; i++)
                {
                    float y = 60 + i * 12;
                    c.Fill(Sdf_.Capsule(26, y, 52, y + 4, 2.5f), Pal.Outline);
                    c.Fill(Sdf_.Capsule(76, y + 4, 102, y, 2.5f), Pal.Outline);
                }
            });

            // Peta terlipat
            var map = Sdf_.Offset(Sdf_.Polygon(new Vector2(20, 34), new Vector2(46, 24), new Vector2(82, 34), new Vector2(108, 24),
                new Vector2(108, 94), new Vector2(82, 104), new Vector2(46, 94), new Vector2(20, 104)), 3);
            Glyph("icon_map", map, 128, c =>
            {
                c.Fill(Sdf_.Capsule(46, 28, 46, 92, 2.5f), Pal.Outline);
                c.Fill(Sdf_.Capsule(82, 38, 82, 100, 2.5f), Pal.Outline);
                c.FillOutlined(Sdf_.Circle(64, 58, 9), Pal.Red, Pal.Outline, 3);
            });

            // Kartu koleksi (dua kartu bertumpuk + bintang)
            {
                var c = new ArtCanvas(128, 128);
                var back = Sdf_.Rotate(Sdf_.RoundRect(52, 64, 26, 36, 8), 52, 64, -14);
                var front = Sdf_.Rotate(Sdf_.RoundRect(74, 66, 26, 36, 8), 74, 66, 10);
                c.Shadow(Sdf_.Union(back, front), 0, 4, 6, Pal.A(Color.black, 0.35f));
                c.FillOutlined(back, Pal.Hex("F3E7D3"), Pal.Outline, 7);
                c.FillOutlined(front, Pal.VGrad(28, 104, Color.white, Pal.Hex("F3E7D3")), Pal.Outline, 7);
                c.FillOutlined(Sdf_.Offset(Sdf_.Star(74, 68, 16, 7), 1.5f), Pal.Gold, Pal.Outline, 3);
                Save(c, "icon_collection", Vector4.zero);
            }

            // --- Ikon berwarna (berdiri sendiri tanpa tombol) ---
            // Gembok emas
            {
                var c = new ArtCanvas(128, 128);
                var shackle = Sdf_.Union(Sdf_.Intersect(Sdf_.Annulus(Sdf_.Circle(64, 52, 24), 7), Sdf_.Above(54)),
                    Sdf_.Capsule(40, 52, 40, 66, 7), Sdf_.Capsule(88, 52, 88, 66, 7));
                var body = Sdf_.RoundRect(64, 84, 38, 28, 9);
                c.Shadow(Sdf_.Union(shackle, body), 0, 4, 6, Pal.A(Color.black, 0.35f));
                c.FillOutlined(shackle, Pal.VGrad(24, 70, Pal.Hex("E9E4DC"), Pal.Hex("9C948A")), Pal.Outline, 6);
                c.FillOutlined(body, Pal.VGrad(56, 112, Pal.GoldLight, Pal.GoldDark), Pal.Outline, 6);
                c.Fill(Sdf_.Union(Sdf_.Circle(64, 80, 8), Sdf_.Polygon(new Vector2(60, 82), new Vector2(68, 82), new Vector2(71, 98), new Vector2(57, 98))), Pal.Outline);
                Save(c, "icon_lock", Vector4.zero);
            }

            // Centang hijau (sudah ditemukan)
            {
                var c = new ArtCanvas(128, 128);
                c.Shadow(Sdf_.Circle(64, 64, 52), 0, 4, 6, Pal.A(Color.black, 0.35f));
                c.FillOutlined(Sdf_.Circle(64, 64, 50), Pal.VGrad(14, 114, Pal.Hex("7FD15F"), Pal.LeafDark), Pal.Outline, 6);
                c.FillOutlined(CheckMark(1f, 10), Color.white, Pal.Outline, 3);
                Save(c, "icon_check", Vector4.zero);
            }

            // Bintang emas
            {
                var c = new ArtCanvas(128, 128);
                var star = Sdf_.Offset(Sdf_.Star(64, 68, 50, 23), 5);
                c.Shadow(star, 0, 4, 6, Pal.A(Color.black, 0.35f));
                c.FillOutlined(star, Pal.VGrad(14, 116, Pal.GoldLight, Pal.GoldDark), Pal.Outline, 6);
                c.Fill(Sdf_.Intersect(Sdf_.Offset(Sdf_.Star(64, 68, 34, 15), 2), Sdf_.Above(64)), Pal.A(Color.white, 0.45f));
                Save(c, "icon_star", Vector4.zero);
            }

            // Mahkota (hadiah koleksi)
            {
                var c = new ArtCanvas(128, 128);
                var crown = Sdf_.Offset(Sdf_.Polygon(new Vector2(22, 94), new Vector2(18, 44), new Vector2(44, 68), new Vector2(64, 30),
                    new Vector2(84, 68), new Vector2(110, 44), new Vector2(106, 94)), 4);
                c.Shadow(crown, 0, 4, 6, Pal.A(Color.black, 0.35f));
                c.FillOutlined(crown, Pal.VGrad(26, 98, Pal.GoldLight, Pal.GoldDark), Pal.Outline, 6);
                c.FillOutlined(Sdf_.Box(24, 84, 80, 14, 4), Pal.Gold, Pal.Outline, 3);
                c.FillOutlined(Sdf_.Circle(64, 66, 8), Pal.Red, Pal.Outline, 3);
                c.FillOutlined(Sdf_.Circle(40, 76, 6), Pal.Teal, Pal.Outline, 3);
                c.FillOutlined(Sdf_.Circle(88, 76, 6), Pal.Teal, Pal.Outline, 3);
                foreach (var p in new[] { new Vector2(18, 40), new Vector2(64, 26), new Vector2(110, 40) })
                    c.FillOutlined(Sdf_.Circle(p.x, p.y, 6), Pal.GoldLight, Pal.Outline, 3);
                Save(c, "icon_crown", Vector4.zero);
            }

            // Selendang penari (pita bergelombang)
            {
                var c = new ArtCanvas(128, 128);
                var parts = new List<ArtCanvas.Sdf>();
                for (int i = 0; i < 12; i++)
                {
                    float t0 = i / 12f, t1 = (i + 1) / 12f;
                    parts.Add(Sdf_.Capsule(20 + t0 * 88, 64 + Mathf.Sin(t0 * 6.5f) * 22, 20 + t1 * 88, 64 + Mathf.Sin(t1 * 6.5f) * 22, 12));
                }
                var scarf = Sdf_.Union(parts.ToArray());
                c.Shadow(scarf, 0, 4, 6, Pal.A(Color.black, 0.35f));
                c.FillOutlined(scarf, (x, y) => Mathf.Repeat(x * 0.08f + y * 0.03f, 1f) < 0.5f ? Pal.Hex("E0457B") : Pal.Hex("F07AA2"), Pal.Outline, 6);
                for (int i = 0; i < 4; i++)
                    c.Fill(Sdf_.Capsule(100 + i * 4, 76, 104 + i * 4, 100, 2f), Pal.Gold);
                Save(c, "icon_scarf", Vector4.zero);
            }
        }

        // ------------------------------------------------------------------
        // Helper
        // ------------------------------------------------------------------
        // Tanda centang untuk kanvas 128 px (s = skala kanvas)
        private static ArtCanvas.Sdf CheckMark(float s, float radius = 12f) =>
            Sdf_.Union(Sdf_.Capsule(38 * s, 66 * s, 56 * s, 84 * s, radius * s), Sdf_.Capsule(56 * s, 84 * s, 92 * s, 44 * s, radius * s));

        /// <summary>Ikon glyph: isi putih-krem, garis tepi coklat tua, bayangan lembut.</summary>
        private static void Glyph(string name, ArtCanvas.Sdf shape, int size = 128, System.Action<ArtCanvas> details = null)
        {
            var c = new ArtCanvas(size, size);
            c.Shadow(Sdf_.Offset(shape, 7), 0, 4, 6, Pal.A(Color.black, 0.35f));
            c.FillOutlined(shape, Pal.VGrad(size * 0.1f, size * 0.9f, Color.white, Pal.Hex("F3E7D3")), Pal.Outline, 7);
            details?.Invoke(c);
            Save(c, name, Vector4.zero);
        }

        private static void SaveImg(Img img, string name, float scale, Vector4 border)
        {
            int w = Mathf.Max(1, Mathf.RoundToInt(img.W * scale));
            int h = Mathf.Max(1, Mathf.RoundToInt(img.H * scale));
            var c = new ArtCanvas(w, h);
            c.Draw(img, 0, 0, w, h);
            Save(c, name, border);
        }

        private static void Save(ArtCanvas c, string name, Vector4 border, string dir = ThemeDir, int maxSize = 2048)
        {
            string path = $"{dir}/{name}.png";
            c.SavePng(path);
            ConfigureImporter(path, border, maxSize);
        }

        /// <summary>Atur importer: Sprite UI, tanpa mipmap, border 9-slice (kiri, bawah, kanan, atas).</summary>
        public static void ConfigureImporter(string path, Vector4 border, int maxSize = 2048)
        {
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var ti = (TextureImporter)AssetImporter.GetAtPath(path);
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled = false;
            ti.spritePixelsPerUnit = 100;
            ti.maxTextureSize = maxSize;
            ti.wrapMode = TextureWrapMode.Clamp;

            var settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect; // wajib untuk 9-slice
            settings.spriteBorder = border;
            ti.SetTextureSettings(settings);
            ti.SaveAndReimport();
        }

        public static Sprite Load(string name, string dir = ThemeDir) => AssetDatabase.LoadAssetAtPath<Sprite>($"{dir}/{name}.png");
    }
}
