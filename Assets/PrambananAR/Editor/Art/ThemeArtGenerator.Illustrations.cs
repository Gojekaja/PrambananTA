using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PrambananAR.UI.EditorTools.Art
{
    /// <summary>
    /// Ilustrasi prosedural: peta kompleks Prambanan, latar loading, pemandu "Kak Pandu",
    /// potret Roro Jonggrang (dirender dari rig PSB), serta sampul & gambar halaman cerita
    /// (memakai sprite benda dari Art/item.psb).
    /// </summary>
    public static partial class ThemeArtGenerator
    {
        public const string ItemPsb = "Assets/Art/item.psb";
        public const string RoroPortraitName = "portrait_roro_jonggrang";

        public static void GenerateIllustrations()
        {
            GenerateMap();
            GenerateLoadingBackground();
            GenerateGuidePortrait();
            GenerateRoroPortrait();
            GenerateStoryArt();
        }

        // ==================================================================
        // Tema waktu (siang, senja, malam, fajar)
        // ==================================================================
        private class SceneTheme
        {
            public Color[] sky;
            public Color sun;
            public bool moon, stars;
            public Color cloud, merapi, hills;
            public Color ground1, ground2;
            public Color templeLight, templeDark, templeOutline;
            public Color tree;
        }

        private static readonly SceneTheme Day = new SceneTheme
        {
            sky = new[] { Pal.Hex("5DB8EC"), Pal.Hex("A7DDF7"), Pal.Hex("E4F6FF") },
            sun = Pal.Hex("FFE680"), cloud = Pal.A(Color.white, 0.92f), merapi = Pal.Hex("8FB3C9"), hills = Pal.Hex("7FB86A"),
            ground1 = Pal.Hex("86C45E"), ground2 = Pal.Hex("4F9A3E"),
            templeLight = Pal.Hex("C9B8A4"), templeDark = Pal.Hex("8A7564"), templeOutline = Pal.Hex("4A3A30"), tree = Pal.Hex("3E7F35"),
        };

        private static readonly SceneTheme Sunset = new SceneTheme
        {
            sky = new[] { Pal.Hex("3B2C7A"), Pal.Hex("9C4C83"), Pal.Hex("EE7F5A"), Pal.Hex("FFD27A") },
            sun = Pal.Hex("FFE3A0"), cloud = Pal.Hex("F7B39A", 0.85f), merapi = Pal.Hex("7A4E7E"), hills = Pal.Hex("5E4A6E"),
            ground1 = Pal.Hex("5E8F3E"), ground2 = Pal.Hex("2F5A28"),
            templeLight = Pal.Hex("8E6A64"), templeDark = Pal.Hex("4E3442"), templeOutline = Pal.Hex("2E1E2A"), tree = Pal.Hex("2B3A2A"),
        };

        private static readonly SceneTheme Night = new SceneTheme
        {
            sky = new[] { Pal.Hex("0B1433"), Pal.Hex("1D2E63"), Pal.Hex("34508A") },
            sun = Pal.Hex("FFF4D0"), moon = true, stars = true, cloud = Pal.Hex("5C6FA8", 0.6f), merapi = Pal.Hex("26335E"), hills = Pal.Hex("1E3050"),
            ground1 = Pal.Hex("2D4B3B"), ground2 = Pal.Hex("172B22"),
            templeLight = Pal.Hex("5E6688"), templeDark = Pal.Hex("343A58"), templeOutline = Pal.Hex("161A2E"), tree = Pal.Hex("13241C"),
        };

        private static readonly SceneTheme Dawn = new SceneTheme
        {
            sky = new[] { Pal.Hex("2E3F7E"), Pal.Hex("8D78B5"), Pal.Hex("F2A29A"), Pal.Hex("FFDCA8") },
            sun = Pal.Hex("FFF0B8"), cloud = Pal.Hex("FFD0C4", 0.8f), merapi = Pal.Hex("6A6A9A"), hills = Pal.Hex("5B6E78"),
            ground1 = Pal.Hex("6C9A4E"), ground2 = Pal.Hex("3C6634"),
            templeLight = Pal.Hex("A08A8E"), templeDark = Pal.Hex("5E4E62"), templeOutline = Pal.Hex("33283A"), tree = Pal.Hex("2E3E30"),
        };

        // ==================================================================
        // Candi bergaya Prambanan (tampak depan, kartun)
        // ==================================================================
        private static ArtCanvas.Sdf CandiShape(float cx, float baseY, float w, float h)
        {
            var parts = new List<ArtCanvas.Sdf>
            {
                Sdf_.Box(cx - w * 0.5f, baseY - 0.12f * h, w, 0.12f * h, 0.01f * h),          // batur (kaki)
                Sdf_.Box(cx - w * 0.35f, baseY - 0.45f * h, w * 0.70f, 0.34f * h, 0.01f * h),  // tubuh
            };
            float ty = baseY - 0.44f * h;
            for (int i = 0; i < 4; i++)
            {
                float tw = Mathf.Lerp(0.82f * w, 0.30f * w, i / 3f);
                float th = 0.105f * h;
                parts.Add(Sdf_.Polygon(new Vector2(cx - tw * 0.5f, ty), new Vector2(cx + tw * 0.5f, ty),
                    new Vector2(cx + tw * 0.42f, ty - th), new Vector2(cx - tw * 0.42f, ty - th)));
                // Ratna (puncak kecil) di ujung tiap tingkat atap
                float r = 0.032f * h * (1f - i * 0.12f);
                foreach (float sx in new[] { -1f, 1f })
                {
                    float rx = cx + sx * tw * 0.42f;
                    parts.Add(Sdf_.Circle(rx, ty - th - r * 0.4f, r));
                    parts.Add(Sdf_.Polygon(new Vector2(rx - r * 0.7f, ty - th - r * 0.6f), new Vector2(rx + r * 0.7f, ty - th - r * 0.6f),
                        new Vector2(rx, ty - th - r * 2.6f)));
                }
                ty -= th;
            }
            parts.Add(Sdf_.Circle(cx, ty - 0.03f * h, 0.05f * h));
            parts.Add(Sdf_.Polygon(new Vector2(cx - 0.06f * w, ty - 0.03f * h), new Vector2(cx + 0.06f * w, ty - 0.03f * h), new Vector2(cx, baseY - h)));
            return Sdf_.Union(parts.ToArray());
        }

        private static Rect CandiBounds(float cx, float baseY, float w, float h) =>
            new Rect(cx - w * 0.6f, baseY - h * 1.05f, w * 1.2f, h * 1.1f);

        private static void DrawCandi(ArtCanvas c, float cx, float baseY, float w, float h, SceneTheme t, float outline, bool details = true)
        {
            var shape = CandiShape(cx, baseY, w, h);
            var bounds = CandiBounds(cx, baseY, w, h);
            if (outline > 0f) c.Fill(Sdf_.Offset(shape, outline), t.templeOutline, 1f, bounds);
            c.Fill(shape, (x, y) =>
            {
                // Sisi kiri terkena cahaya, sisi kanan lebih gelap + tekstur batu halus
                float lit = x < cx ? 0.85f : 0.45f;
                float n = 0.92f + 0.08f * Pal.Noise(x * 0.12f, y * 0.12f);
                var col = Color.Lerp(t.templeDark, t.templeLight, lit);
                return new Color(col.r * n, col.g * n, col.b * n, 1f);
            }, 1f, bounds);

            if (!details || h < 60f) return;
            float lw = Mathf.Max(1.2f, outline * 0.45f);
            // Garis tingkat atap
            float ty = baseY - 0.44f * h;
            for (int i = 0; i < 4; i++)
            {
                float tw = Mathf.Lerp(0.82f * w, 0.30f * w, i / 3f) * 0.5f;
                c.Fill(Sdf_.Capsule(cx - tw, ty, cx + tw, ty, lw), Pal.A(t.templeOutline, 0.7f), 1f, bounds);
                ty -= 0.105f * h;
            }
            c.Fill(Sdf_.Capsule(cx - w * 0.5f, baseY - 0.12f * h, cx + w * 0.5f, baseY - 0.12f * h, lw), Pal.A(t.templeOutline, 0.7f), 1f, bounds);
            // Pintu masuk (relung gelap) + tangga
            var door = Sdf_.Union(Sdf_.Box(cx - 0.1f * w, baseY - 0.36f * h, 0.2f * w, 0.24f * h, 0.02f * h), Sdf_.Circle(cx, baseY - 0.36f * h, 0.1f * w));
            c.Fill(door, Pal.A(t.templeOutline, 0.9f), 1f, bounds);
            c.Fill(Sdf_.Polygon(new Vector2(cx - 0.13f * w, baseY), new Vector2(cx + 0.13f * w, baseY),
                new Vector2(cx + 0.09f * w, baseY - 0.12f * h), new Vector2(cx - 0.09f * w, baseY - 0.12f * h)),
                Color.Lerp(t.templeLight, Color.white, 0.25f), 1f, bounds);
        }

        private static void DrawTree(ArtCanvas c, float x, float y, float r, Color leaf, Color dark, bool trunk = true, float outline = 3f)
        {
            var canopy = Sdf_.Union(Sdf_.Circle(x - r * 0.45f, y, r * 0.7f), Sdf_.Circle(x + r * 0.45f, y, r * 0.7f), Sdf_.Circle(x, y - r * 0.45f, r * 0.8f));
            var b = new Rect(x - r * 1.4f, y - r * 1.4f, r * 2.8f, r * 2.8f);
            c.Fill(Sdf_.Ellipse(x, y + r * 0.85f, r * 1.1f, r * 0.35f), Pal.A(Color.black, 0.22f), 4f, b);
            if (trunk) c.Fill(Sdf_.Box(x - r * 0.15f, y, r * 0.3f, r * 0.85f, r * 0.08f), Pal.Hex("6B4426"), 1f, b);
            c.Fill(Sdf_.Offset(canopy, outline), dark, 1f, b);
            c.Fill(canopy, Pal.VGrad(y - r * 1.2f, y + r * 0.7f, Color.Lerp(leaf, Color.white, 0.18f), leaf), 1f, b);
            c.Fill(Sdf_.Circle(x - r * 0.3f, y - r * 0.6f, r * 0.28f), Pal.A(Color.white, 0.25f), 1f, b);
        }

        private static void DrawPalm(ArtCanvas c, float x, float baseY, float h, Color color)
        {
            float lean = h * 0.12f;
            Vector2 top = new Vector2(x + lean, baseY - h);
            // Batang melengkung yang menyempit ke atas
            for (int i = 0; i < 12; i++)
            {
                float t0 = i / 12f, t1 = (i + 1) / 12f;
                var a0 = new Vector2(x + lean * t0 * t0, baseY - h * t0); var a1 = new Vector2(x + lean * t1 * t1, baseY - h * t1);
                c.Fill(Sdf_.Capsule(a0.x, a0.y, a1.x, a1.y, h * 0.026f * (1.25f - t0 * 0.5f)), color, 1f, SegBounds(a0, a1, h * 0.04f));
            }
            // Pelepah: melengkung ke bawah, menipis, dengan anak daun di kedua sisi
            foreach (float ang in new[] { -170f, -140f, -108f, -72f, -40f, -10f })
            {
                float a = ang * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                float len = h * 0.42f;
                Vector2 prev = top;
                for (int s = 1; s <= 10; s++)
                {
                    float t = s / 10f;
                    Vector2 p = top + dir * len * t + new Vector2(0, len * 0.6f * t * t);
                    float r = Mathf.Lerp(h * 0.02f, h * 0.005f, t);
                    c.Fill(Sdf_.Capsule(prev.x, prev.y, p.x, p.y, r), color, 1f, SegBounds(prev, p, r + 2f));
                    Vector2 tangent = (p - prev).normalized;
                    Vector2 normal = new Vector2(-tangent.y, tangent.x);
                    float leaf = h * 0.1f * (1f - t * 0.6f);
                    foreach (float side in new[] { -1f, 1f })
                    {
                        Vector2 end = p + (normal * side * 0.8f + tangent * 0.6f + Vector2.up * 0.6f).normalized * leaf;
                        c.Fill(Sdf_.Capsule(p.x, p.y, end.x, end.y, r * 0.6f + 1f), color, 1f, SegBounds(p, end, r + 3f));
                    }
                    prev = p;
                }
            }
        }

        private static void DrawCloud(ArtCanvas c, float x, float y, float s, Color col)
        {
            var cloud = Sdf_.Intersect(Sdf_.Union(Sdf_.Circle(x - s * 0.6f, y, s * 0.42f), Sdf_.Circle(x, y - s * 0.22f, s * 0.6f),
                Sdf_.Circle(x + s * 0.62f, y - s * 0.02f, s * 0.45f), Sdf_.Circle(x + s * 0.2f, y + s * 0.1f, s * 0.45f)), Sdf_.Above(y + s * 0.3f));
            c.Fill(cloud, col, 2f, new Rect(x - s * 1.2f, y - s, s * 2.4f, s * 1.5f));
        }

        /// <summary>Latar pemandangan: langit, matahari/bulan, awan, Merapi, deretan candi, tanah.</summary>
        private static void DrawScene(ArtCanvas c, SceneTheme t, float horizon, float templeH, int seed, bool skyline = true)
        {
            int W = c.W, H = c.H;
            c.Fill((x, y) => -10f, Pal.VGrad(0, horizon, t.sky));

            if (t.stars)
                for (int i = 0; i < 90; i++)
                {
                    float sx = Pal.Hash(i, seed) * W, sy = Pal.Hash(seed, i * 7) * horizon * 0.8f;
                    float r = 1.2f + Pal.Hash(i * 3, seed + 1) * 2.4f;
                    c.Fill(Sdf_.Circle(sx, sy, r), Pal.A(Color.white, 0.5f + 0.5f * Pal.Hash(i, i)), 1f, new Rect(sx - 4, sy - 4, 8, 8));
                }

            // Matahari / bulan + cahaya
            float sunX = W * (t.moon ? 0.78f : 0.5f), sunY = t.moon ? horizon * 0.28f : horizon * 0.82f, sunR = W * 0.11f;
            c.Fill(Sdf_.Circle(sunX, sunY, sunR * 3f), Pal.Radial(sunX, sunY, sunR * 3f, Pal.A(t.sun, 0.55f), Pal.A(t.sun, 0f)), 2f);
            var disk = Sdf_.Circle(sunX, sunY, sunR);
            // Bulan sabit: lingkaran dikurangi lingkaran bergeser (tanpa menutup langit/awan)
            c.Fill(t.moon ? Sdf_.Subtract(disk, Sdf_.Circle(sunX + sunR * 0.38f, sunY - sunR * 0.2f, sunR * 0.85f)) : disk, t.sun, 1.5f);

            for (int i = 0; i < 4; i++)
                DrawCloud(c, W * (0.12f + 0.27f * i + 0.08f * Pal.Hash(i, seed)), horizon * (0.18f + 0.35f * Pal.Hash(seed, i)), W * (0.09f + 0.05f * Pal.Hash(i, 2)), t.cloud);

            // Gunung Merapi di kejauhan + bukit
            c.Fill(Sdf_.Offset(Sdf_.Polygon(new Vector2(W * 0.48f, horizon + 4), new Vector2(W * 0.78f, horizon - W * 0.26f), new Vector2(W * 0.86f, horizon - W * 0.26f),
                new Vector2(W * 1.15f, horizon + 4)), 6f), t.merapi, 2f);
            c.Fill((x, y) => (horizon - W * 0.05f - Mathf.Sin(x / W * 7f + seed) * W * 0.025f) - y, t.hills, 1.5f, new Rect(0, horizon - W * 0.1f, W, W * 0.11f));

            // Kompleks candi (Siwa di tengah, Brahma & Wisnu mengapit, candi perwara kecil)
            if (skyline)
            {
                float cx = W * 0.5f;
                float ol = Mathf.Max(2f, templeH * 0.012f);
                for (int i = -6; i <= 6; i++)
                {
                    if (Mathf.Abs(i) < 2) continue;
                    DrawCandi(c, cx + i * templeH * 0.14f, horizon + 6, templeH * 0.11f, templeH * 0.22f, t, ol * 0.6f, false);
                }
                DrawCandi(c, cx - templeH * 0.55f, horizon + 8, templeH * 0.32f, templeH * 0.5f, t, ol);
                DrawCandi(c, cx + templeH * 0.55f, horizon + 8, templeH * 0.32f, templeH * 0.5f, t, ol);
                DrawCandi(c, cx - templeH * 0.3f, horizon + 10, templeH * 0.4f, templeH * 0.72f, t, ol);
                DrawCandi(c, cx + templeH * 0.3f, horizon + 10, templeH * 0.4f, templeH * 0.72f, t, ol);
                DrawCandi(c, cx, horizon + 12, templeH * 0.5f, templeH, t, ol);
            }

            // Tanah berumput
            c.Fill((x, y) => (horizon + 6 + Mathf.Sin(x / W * 3.2f + seed) * H * 0.012f) - y, Pal.VGrad(horizon, H, t.ground1, t.ground2), 1.5f,
                new Rect(0, horizon - H * 0.02f, W, H));
        }

        // ==================================================================
        // Peta kompleks Prambanan (posisi zona mengikuti ZoneDatabase, 0..1 kiri-bawah)
        // ==================================================================
        private const int MapW = 1772, MapH = 2000;

        private static Vector2 MP(float nx, float ny) => new Vector2(nx * MapW, (1f - ny) * MapH);

        private static float RiverX(float ny) => (0.145f + 0.025f * Mathf.Sin(ny * 9f + 1f) + 0.012f * Mathf.Sin(ny * 23f)) * MapW;

        private static void GenerateMap()
        {
            var c = new ArtCanvas(MapW, MapH);
            var blockers = new List<Rect>();
            var roads = new List<Vector2[]>();

            // 1) Rumput bertekstur
            c.Fill((x, y) => -10f, (x, y) =>
            {
                float n = Pal.Noise(x / 90f, y / 90f) * 0.7f + Pal.Noise(x / 17f, y / 17f) * 0.3f;
                return Color.Lerp(Pal.Hex("7DBE58"), Pal.Hex("93CF69"), n);
            });

            // 2) Taman mandala (rumput lebih terang) di sekitar zona
            foreach (var z in new[] { new Vector2(0.30f, 0.66f), new Vector2(0.40f, 0.665f), new Vector2(0.51f, 0.55f), new Vector2(0.51f, 0.39f), new Vector2(0.28f, 0.39f) })
            {
                var p = MP(z.x, z.y);
                c.Fill(Sdf_.RoundRect(p.x, p.y, 120, 105, 60), Pal.Hex("A3D97B"), 3f);
                for (int i = 0; i < 6; i++)
                {
                    float fx = p.x - 80 + Pal.Hash(i, (int)p.x) * 160, fy = p.y - 70 + Pal.Hash((int)p.y, i) * 140;
                    c.Fill(Sdf_.Circle(fx, fy, 6), i % 2 == 0 ? Pal.Hex("F48FB1") : Pal.Hex("FFE082"), 1f, new Rect(fx - 8, fy - 8, 16, 16));
                }
            }

            // 3) Lapangan selatan (garis potong rumput)
            {
                var p = MP(0.40f, 0.255f);
                var field = Sdf_.RoundRect(p.x, p.y, 270, 140, 40);
                c.Fill(Sdf_.Offset(field, 6), Pal.Hex("6BAA4A"));
                c.Fill(field, (x, y) => Mathf.Repeat(x / 60f, 1f) < 0.5f ? Pal.Hex("9BD672") : Pal.Hex("8BC964"));
                blockers.Add(new Rect(p.x - 290, p.y - 160, 580, 320));
            }

            // 4) Sungai Opak di sisi barat
            {
                var pts = new List<Vector2>();
                for (float ny = -0.03f; ny <= 1.03f; ny += 0.02f) pts.Add(new Vector2(RiverX(ny), (1f - ny) * MapH));
                for (int i = 0; i < pts.Count - 1; i++) c.Fill(Sdf_.Capsule(pts[i].x, pts[i].y, pts[i + 1].x, pts[i + 1].y, 62), Pal.Hex("E3CF98"), 1f, SegBounds(pts[i], pts[i + 1], 64));
                for (int i = 0; i < pts.Count - 1; i++) c.Fill(Sdf_.Capsule(pts[i].x, pts[i].y, pts[i + 1].x, pts[i + 1].y, 44), (x, y) => Color.Lerp(Pal.Hex("4FB4E2"), Pal.Hex("3C98D2"), Pal.Noise(x / 50f, y / 50f)), 1f, SegBounds(pts[i], pts[i + 1], 46));
                for (int i = 2; i < pts.Count - 2; i += 2)
                {
                    float off = (Pal.Hash(i, 5) - 0.5f) * 40f;
                    c.Fill(Sdf_.Capsule(pts[i].x + off - 12, pts[i].y, pts[i].x + off + 12, pts[i].y - 4, 3f), Pal.A(Color.white, 0.55f), 1f, SegBounds(pts[i], pts[i], 30));
                }
            }

            // 5) Jalan setapak
            roads.Add(Pts(0.99f, 0.265f, 0.84f, 0.29f, 0.74f, 0.31f, 0.62f, 0.33f, 0.50f, 0.335f, 0.39f, 0.35f));       // pintu masuk -> candi
            roads.Add(Pts(0.249f, 0.35f, 0.531f, 0.35f, 0.531f, 0.645f, 0.249f, 0.645f, 0.249f, 0.35f));               // jalan keliling
            roads.Add(Pts(0.39f, 0.645f, 0.43f, 0.72f, 0.49f, 0.78f, 0.56f, 0.815f));                                  // ke utara (Candi Sewu)
            roads.Add(Pts(0.531f, 0.50f, 0.64f, 0.52f, 0.70f, 0.45f, 0.74f, 0.31f));                                   // jalur timur
            roads.Add(Pts(0.84f, 0.29f, 0.86f, 0.20f));                                                                // parkir
            roads.Add(Pts(0.249f, 0.42f, 0.215f, 0.34f, 0.205f, 0.30f));                                               // ke panggung Ramayana
            foreach (var r in roads) DrawRoad(c, r, 23f, Pal.Hex("C7A974"));
            foreach (var r in roads) DrawRoad(c, r, 16f, Pal.Hex("EEDDB0"));

            // 6) Kompleks Candi Prambanan (Roro Jonggrang)
            var main = MP(0.39f, 0.49f);
            DrawCompound(c, main, 215f, 120f, 26, true);
            blockers.Add(new Rect(main.x - 245, main.y - 245, 490, 490));

            // 7) Candi Sewu (utara) + Lumbung & Bubrah
            var sewu = MP(0.56f, 0.87f);
            DrawCompound(c, sewu, 150f, 70f, 18, false);
            blockers.Add(new Rect(sewu.x - 175, sewu.y - 175, 350, 350));
            foreach (var sp in new[] { MP(0.455f, 0.745f), MP(0.525f, 0.785f) })
            {
                c.FillOutlined(Sdf_.RoundRect(sp.x, sp.y, 46, 40, 6), Pal.Hex("D8C8AE"), Pal.Hex("A99886"), 5);
                DrawCandi(c, sp.x, sp.y + 30, 48, 70, Day, 3f);
                blockers.Add(new Rect(sp.x - 60, sp.y - 70, 120, 120));
            }

            // 8) Loket tiket (joglo) + parkir
            {
                var t = MP(0.80f, 0.315f);
                c.Fill(Sdf_.Ellipse(t.x, t.y + 40, 90, 16), Pal.A(Color.black, 0.2f), 4f);
                c.FillOutlined(Sdf_.Box(t.x - 62, t.y - 10, 124, 50, 4), Pal.Cream, Pal.Outline, 4);
                c.FillOutlined(Sdf_.Polygon(new Vector2(t.x - 90, t.y - 6), new Vector2(t.x + 90, t.y - 6), new Vector2(t.x + 46, t.y - 50),
                    new Vector2(t.x + 26, t.y - 74), new Vector2(t.x - 26, t.y - 74), new Vector2(t.x - 46, t.y - 50)), Pal.VGrad(t.y - 74, t.y, Pal.Hex("D06A3C"), Pal.Hex("9C4426")), Pal.Outline, 4);
                c.Fill(Sdf_.Box(t.x - 14, t.y + 8, 28, 32, 3), Pal.Brown);
                blockers.Add(new Rect(t.x - 100, t.y - 90, 200, 150));

                var pk = MP(0.865f, 0.19f);
                c.FillOutlined(Sdf_.RoundRect(pk.x, pk.y, 120, 80, 14), Pal.Hex("9A9A9A"), Pal.Hex("6E6E6E"), 5);
                Color[] cars = { Pal.Red, Pal.Teal, Pal.Gold, Color.white };
                for (int i = 0; i < 5; i++)
                {
                    float x = pk.x - 96 + i * 48;
                    c.Fill(Sdf_.Capsule(x + 22, pk.y - 70, x + 22, pk.y + 70, 2f), Pal.A(Color.white, 0.8f));
                    if (i < 4 && Pal.Hash(i, 9) > 0.25f) c.FillOutlined(Sdf_.RoundRect(x, pk.y - 28 + (i % 2) * 56, 14, 22, 6), cars[i], Pal.Outline, 2);
                }
                blockers.Add(new Rect(pk.x - 135, pk.y - 95, 270, 190));
            }

            // 9) Panggung Ramayana (amfiteater)
            {
                var s = MP(0.205f, 0.275f);
                for (int i = 3; i >= 0; i--)
                    c.Fill(Sdf_.Intersect(Sdf_.Circle(s.x, s.y, 70 + i * 22), Sdf_.Below(s.y)), i % 2 == 0 ? Pal.Hex("BFB2A2") : Pal.Hex("A89886"));
                c.FillOutlined(Sdf_.Box(s.x - 60, s.y - 40, 120, 44, 6), Pal.Hex("C9A27A"), Pal.Outline, 3);
                blockers.Add(new Rect(s.x - 170, s.y - 60, 340, 230));
            }

            // 10) Pepohonan (diurutkan dari atas agar tumpukan rapi)
            blockers.Add(new Rect(30, MapH - 310, 250, 300)); // area mata angin
            var trees = new List<Vector3>();
            for (int i = 0; i < 2600 && trees.Count < 330; i++)
            {
                float x = 20 + Pal.Hash(i, 77) * (MapW - 40), y = 20 + Pal.Hash(91, i) * (MapH - 40);
                float r = 17 + Pal.Hash(i, i * 3) * 13;
                if (Mathf.Abs(x - RiverX(1f - y / MapH)) < 95) continue;
                if (NearRoad(roads, x, y, 40 + r)) continue;
                bool blocked = false;
                foreach (var b in blockers) if (b.Contains(new Vector2(x, y))) { blocked = true; break; }
                if (blocked) continue;
                bool crowded = false;
                foreach (var t in trees) if ((t.x - x) * (t.x - x) + (t.y - y) * (t.y - y) < (r + t.z) * (r + t.z) * 0.8f) { crowded = true; break; }
                if (!crowded) trees.Add(new Vector3(x, y, r));
            }
            trees.Sort((a, b) => a.y.CompareTo(b.y));
            foreach (var t in trees)
            {
                bool dark = Pal.Hash((int)t.x, (int)t.y) > 0.6f;
                DrawTree(c, t.x, t.y, t.z, dark ? Pal.Hex("4E9A3E") : Pal.Hex("62B04A"), Pal.Hex("2F6328"));
            }

            // 11) Mata angin (U = Utara)
            {
                float x = 150, y = MapH - 150;
                c.Fill(Sdf_.Circle(x, y, 92), Pal.A(Pal.Cream, 0.85f));
                c.FillOutlined(Sdf_.Star(x, y, 78, 20, 4), Pal.Hex("F5EBD7"), Pal.Outline, 4);
                c.Fill(Sdf_.Polygon(new Vector2(x - 20, y), new Vector2(x + 20, y), new Vector2(x, y - 78)), Pal.Red);
                c.Fill(Sdf_.Circle(x, y, 9), Pal.Outline);
                // huruf "U"
                c.Fill(Sdf_.Union(Sdf_.Capsule(x - 13, y - 140, x - 13, y - 122, 5), Sdf_.Capsule(x + 13, y - 140, x + 13, y - 122, 5),
                    Sdf_.Intersect(Sdf_.Annulus(Sdf_.Circle(x, y - 122, 13), 5), Sdf_.Below(y - 122))), Pal.Outline);
            }

            // 12) Vinyet tepi
            c.Fill((x, y) => -10f, (x, y) =>
            {
                float d = Mathf.Min(Mathf.Min(x, MapW - x), Mathf.Min(y, MapH - y));
                return Pal.A(Pal.Hex("1E3A16"), 0.4f * (1f - Mathf.SmoothStep(0f, 1f, d / 70f)));
            });

            Save(c, "map_prambanan", Vector4.zero, IlluDir);
        }

        private static void DrawCompound(ArtCanvas c, Vector2 p, float outer, float inner, int perwaraPerSide, bool isMain)
        {
            // Halaman luar + pagar batu
            c.Fill(Sdf_.RoundRect(p.x, p.y, outer, outer, 10), Pal.Hex("8CC664"));
            c.Fill(Sdf_.Annulus(Sdf_.RoundRect(p.x, p.y, outer, outer, 10), 6), Pal.Hex("A99886"));

            // Deretan candi perwara kecil
            float ring = outer * 0.8f;
            int n = perwaraPerSide / 2;
            for (int side = 0; side < 4; side++)
                for (int i = 0; i <= n; i++)
                {
                    float t = -1f + 2f * i / n;
                    Vector2 q = side switch
                    {
                        0 => new Vector2(p.x + t * ring, p.y - ring),
                        1 => new Vector2(p.x + ring, p.y + t * ring),
                        2 => new Vector2(p.x + t * ring, p.y + ring),
                        _ => new Vector2(p.x - ring, p.y + t * ring),
                    };
                    if (Mathf.Abs(t) < 0.15f) continue; // celah gerbang
                    DrawCandi(c, q.x, q.y + 12, 18, 30, Day, 2f, false);
                }

            // Halaman dalam berlantai batu + pagar
            var yard = Sdf_.RoundRect(p.x, p.y, inner, inner, 6);
            c.Fill(yard, (x, y) =>
            {
                bool line = Mathf.Repeat(x - p.x, 24f) < 2f || Mathf.Repeat(y - p.y, 24f) < 2f;
                return line ? Pal.Hex("C4B192") : Pal.Hex("DCCDB3");
            });
            c.Fill(Sdf_.Annulus(yard, 6), Pal.Hex("9C8A76"));
            // Gerbang di 4 sisi
            foreach (var g in new[] { new Vector2(0, -1), new Vector2(1, 0), new Vector2(0, 1), new Vector2(-1, 0) })
            {
                var gp = p + g * inner;
                c.FillOutlined(Sdf_.RoundRect(gp.x, gp.y, 16, 16, 3), Pal.Hex("EEDDB0"), Pal.Hex("9C8A76"), 3);
            }

            // Candi utama (dari belakang ke depan)
            if (isMain)
            {
                DrawCandi(c, p.x - 70, p.y + 10, 66, 120, Day, 3f);   // Brahma
                DrawCandi(c, p.x + 70, p.y + 10, 66, 120, Day, 3f);   // Wisnu
                DrawCandi(c, p.x, p.y + 40, 100, 190, Day, 4f);       // Siwa
                for (int i = -1; i <= 1; i++) DrawCandi(c, p.x + i * 66, p.y + 108, 46, 70, Day, 3f); // candi wahana
            }
            else
            {
                DrawCandi(c, p.x, p.y + 40, 96, 140, Day, 4f);
            }
        }

        private static void DrawRoad(ArtCanvas c, Vector2[] pts, float r, Color col)
        {
            for (int i = 0; i < pts.Length - 1; i++)
                c.Fill(Sdf_.Capsule(pts[i].x, pts[i].y, pts[i + 1].x, pts[i + 1].y, r), col, 1f, SegBounds(pts[i], pts[i + 1], r + 2));
        }

        private static Vector2[] Pts(params float[] n)
        {
            var v = new Vector2[n.Length / 2];
            for (int i = 0; i < v.Length; i++) v[i] = MP(n[i * 2], n[i * 2 + 1]);
            return v;
        }

        private static bool NearRoad(List<Vector2[]> roads, float x, float y, float dist)
        {
            foreach (var r in roads)
                for (int i = 0; i < r.Length - 1; i++)
                    if (Sdf_.Capsule(r[i].x, r[i].y, r[i + 1].x, r[i + 1].y, 0)(x, y) < dist) return true;
            return false;
        }

        private static Rect SegBounds(Vector2 a, Vector2 b, float r) =>
            Rect.MinMaxRect(Mathf.Min(a.x, b.x) - r, Mathf.Min(a.y, b.y) - r, Mathf.Max(a.x, b.x) + r, Mathf.Max(a.y, b.y) + r);

        // ==================================================================
        // Latar layar Loading (1080 x 1920)
        // ==================================================================
        private static void GenerateLoadingBackground()
        {
            var c = new ArtCanvas(1080, 1920);
            DrawScene(c, Sunset, 1340f, 560f, 3);
            DrawPalm(c, 90, 1600, 520, Pal.Hex("1E2A1E"));
            DrawPalm(c, 1010, 1640, 460, Pal.Hex("1E2A1E"));
            // Area bawah lebih gelap agar teks tips & bar progres terbaca
            c.Fill((x, y) => -10f, (x, y) => Pal.A(Pal.Hex("1A1020"), 0.75f * Mathf.SmoothStep(0f, 1f, (y - 1500f) / 420f)));
            Save(c, "bg_loading", Vector4.zero, IlluDir);
        }

        // ==================================================================
        // Pemandu "Kak Pandu" (pemuda Jawa berblangkon, kemeja lurik)
        // ==================================================================
        private static void GenerateGuidePortrait()
        {
            var c = new ArtCanvas(600, 900);
            Color skin = Pal.Hex("EDB98A"), skinDark = Pal.Hex("D29A6C"), ol = Pal.Outline;

            // Lengan kiri (di belakang badan)
            c.FillOutlined(Sdf_.Capsule(170, 600, 120, 860, 46), Pal.Hex("6B4426"), ol, 6);

            // Badan: kemeja lurik bergaris
            var torso = Sdf_.Offset(Sdf_.Polygon(new Vector2(170, 545), new Vector2(430, 545), new Vector2(490, 900), new Vector2(110, 900)), 30);
            c.FillOutlined(torso, (x, y) =>
            {
                float s = Mathf.Repeat(x / 34f, 1f);
                return s < 0.18f ? Pal.Hex("4A2C17") : s < 0.32f ? Pal.Hex("C9A066") : Pal.Hex("7A4E2D");
            }, ol, 6);
            // Kerah & kancing
            c.FillOutlined(Sdf_.Polygon(new Vector2(240, 515), new Vector2(360, 515), new Vector2(300, 610)), Pal.Cream, ol, 5);
            for (int i = 0; i < 3; i++) c.FillOutlined(Sdf_.Circle(300, 650 + i * 60, 9), Pal.Gold, ol, 3);
            // Tali kartu pemandu + kartu
            c.Fill(Sdf_.Capsule(262, 540, 300, 700, 5), Pal.Red);
            c.Fill(Sdf_.Capsule(338, 540, 300, 700, 5), Pal.Red);
            c.FillOutlined(Sdf_.Box(258, 690, 84, 104, 10), Color.white, ol, 4);
            DrawCandi(c, 300, 778, 40, 72, Day, 2f, false);

            // Leher
            c.FillOutlined(Sdf_.Box(262, 470, 76, 80, 20), skinDark, ol, 5);

            // Lengan kanan melambai
            c.FillOutlined(Sdf_.Capsule(430, 600, 520, 430, 44), Pal.Hex("6B4426"), ol, 6);
            var hand = Sdf_.Union(Sdf_.Circle(532, 404, 38),
                Sdf_.Capsule(512, 384, 498, 336, 11), Sdf_.Capsule(527, 376, 523, 324, 11),
                Sdf_.Capsule(543, 376, 549, 326, 11), Sdf_.Capsule(557, 386, 571, 344, 10),
                Sdf_.Capsule(504, 414, 476, 388, 11)); // jempol
            c.FillOutlined(hand, skin, ol, 6);

            // Telinga + kepala
            c.FillOutlined(Sdf_.Circle(152, 350, 34), skin, ol, 6);
            c.FillOutlined(Sdf_.Circle(448, 350, 34), skin, ol, 6);
            c.FillOutlined(Sdf_.Ellipse(300, 340, 150, 158), Pal.VGrad(190, 500, skin, skinDark), ol, 6);
            c.Fill(Sdf_.Ellipse(205, 405, 30, 18), Pal.A(Pal.Hex("F07A7A"), 0.4f), 6f);
            c.Fill(Sdf_.Ellipse(395, 405, 30, 18), Pal.A(Pal.Hex("F07A7A"), 0.4f), 6f);

            // Blangkon bermotif kawung
            var cap = Sdf_.Union(Sdf_.Intersect(Sdf_.Ellipse(300, 318, 166, 152), Sdf_.Above(300)), Sdf_.Box(136, 268, 328, 40, 18));
            c.FillOutlined(cap, (x, y) =>
            {
                float gx = Mathf.Repeat(x, 44f) - 22f, gy = Mathf.Repeat(y, 44f) - 22f;
                float d = Mathf.Min(Mathf.Min(Ell(gx - 9, gy, 8, 5), Ell(gx + 9, gy, 8, 5)), Mathf.Min(Ell(gx, gy - 9, 5, 8), Ell(gx, gy + 9, 5, 8)));
                return d < 0f ? Pal.Hex("C9973F") : Pal.Hex("5A3418");
            }, ol, 6);
            c.FillOutlined(Sdf_.Box(140, 280, 320, 24, 10), Pal.Hex("3E2210"), ol, 3);

            // Alis, mata, hidung, senyum
            c.Fill(Sdf_.Capsule(215, 318, 268, 308, 9), Pal.Hex("2B1B12"));
            c.Fill(Sdf_.Capsule(332, 308, 385, 318, 9), Pal.Hex("2B1B12"));
            foreach (float ex in new[] { 245f, 355f })
            {
                c.Fill(Sdf_.Ellipse(ex, 362, 19, 26), Pal.Hex("2B1B12"));
                c.Fill(Sdf_.Circle(ex + 6, 351, 7), Color.white);
            }
            c.Fill(Sdf_.Ellipse(300, 395, 16, 11), skinDark);
            var mouth = Sdf_.Intersect(Sdf_.Circle(300, 412, 46), Sdf_.Below(420));
            c.FillOutlined(mouth, Pal.Hex("7A2A1E"), ol, 4);
            c.Fill(Sdf_.Intersect(Sdf_.Circle(300, 470, 30), mouth), Pal.Hex("E8747A"));
            c.Fill(Sdf_.Intersect(Sdf_.Box(262, 420, 76, 12, 3), mouth), Color.white);

            Save(c, "guide_kak_pandu", Vector4.zero, IlluDir);
        }

        private static float Ell(float x, float y, float rx, float ry) => (x * x) / (rx * rx) + (y * y) / (ry * ry) - 1f;

        // ==================================================================
        // Potret Roro Jonggrang: render rig 2D di scene (mata terbuka)
        // ==================================================================
        public static void GenerateRoroPortrait()
        {
            GameObject roro = FindSceneObject("roroJonggrang");
            if (roro == null)
            {
                Debug.LogWarning("[Prambanan AR] Objek 'roroJonggrang' tidak ada di scene aktif; potret tidak dirender.");
                return;
            }
            var idle = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Animation/idle.anim");
            var img = RenderCharacter(roro, 1200, idle, 0f);
            if (img == null) return;
            var c = new ArtCanvas(img.W, img.H);
            c.Draw(img, 0, 0, img.W, img.H);
            Save(c, RoroPortraitName, Vector4.zero, IlluDir);

            // Ikon kepala & bahu untuk pin peta / bingkai bulat (potret penuh terlalu kecil di lingkaran)
            int bustH = Mathf.RoundToInt(img.H * 0.4f);
            var bust = img.Crop(0, 0, img.W, bustH).Trim(4);
            int side = Mathf.Max(bust.W, bust.H);
            var ic = new ArtCanvas(side, side);
            ic.Draw(bust, (side - bust.W) * 0.5f, side - bust.H, bust.W, bust.H);
            Save(ic, RoroPortraitName + "_icon", Vector4.zero, IlluDir);
        }

        /// <summary>
        /// Render objek sprite (rig) ke gambar transparan memakai kamera sementara.
        /// poseClip (opsional) di-sample lewat AnimationMode sehingga scene tidak berubah permanen.
        /// </summary>
        public static Img RenderCharacter(GameObject root, int height, AnimationClip poseClip = null, float poseTime = 0f)
        {
            const int tempLayer = 31;
            bool wasActive = root.activeSelf;
            root.SetActive(true);

            // Nyalakan mata terbuka sementara (di Edit Mode animasi berkedip tidak berjalan)
            var toggled = new List<GameObject>();
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
                if (t.name == "open eye" && !t.gameObject.activeSelf) { t.gameObject.SetActive(true); toggled.Add(t.gameObject); }

            bool sampled = poseClip != null;
            if (sampled)
            {
                AnimationMode.StartAnimationMode();
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(root, poseClip, poseTime);
                AnimationMode.EndSampling();
            }
            ForceSpriteSkinDeform();
            ForceSpriteSkinDeform();

            var renderers = root.GetComponentsInChildren<Renderer>(false);
            var layers = new Dictionary<GameObject, int>();
            Bounds b = default;
            bool first = true;
            foreach (var r in renderers)
            {
                layers[r.gameObject] = r.gameObject.layer;
                r.gameObject.layer = tempLayer;
                if (first) { b = r.bounds; first = false; } else b.Encapsulate(r.bounds);
            }

            Img img = null;
            var camGo = new GameObject("PortraitCam") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                if (first) return null;
                var cam = camGo.AddComponent<Camera>();
                cam.orthographic = true;
                cam.orthographicSize = b.extents.y * 1.04f;
                cam.transform.position = new Vector3(b.center.x, b.center.y, b.center.z - 10f);
                cam.transform.rotation = Quaternion.identity;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0, 0, 0, 0);
                cam.cullingMask = 1 << tempLayer;
                cam.nearClipPlane = 0.01f;
                cam.farClipPlane = 50f;

                int h = height;
                int w = Mathf.CeilToInt(h * (b.size.x / b.size.y)) + 40;
                cam.aspect = (float)w / h;
                var rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                cam.targetTexture = rt;
                cam.Render();
                img = Img.FromRenderTexture(rt, true).Trim(6);
                cam.targetTexture = null;
                RenderTexture.ReleaseTemporary(rt);
            }
            finally
            {
                Object.DestroyImmediate(camGo);
                foreach (var kv in layers) kv.Key.layer = kv.Value;
                foreach (var g in toggled) g.SetActive(false);
                if (sampled) AnimationMode.StopAnimationMode();
                root.SetActive(wasActive);
                ForceSpriteSkinDeform();
            }
            return img;
        }

        // Di Edit Mode deformasi SpriteSkin baru dihitung saat LateUpdate editor; paksa sekarang
        // agar pose hasil sample animasi ikut terender (API internal paket 2D Animation).
        private static void ForceSpriteSkinDeform()
        {
            var type = System.Type.GetType("UnityEngine.U2D.Animation.DeformationManager, Unity.2D.Animation.Runtime");
            if (type == null) return;
            const System.Reflection.BindingFlags Any = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic;
            var instance = type.GetProperty("instance", Any | System.Reflection.BindingFlags.Static)?.GetValue(null);
            var update = type.GetMethod("Update", Any | System.Reflection.BindingFlags.Instance);
            if (instance != null && update != null) update.Invoke(instance, null);
        }

        private static GameObject FindSceneObject(string name)
        {
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            foreach (var go in scene.GetRootGameObjects())
            {
                if (go.name == name) return go;
                foreach (var t in go.GetComponentsInChildren<Transform>(true))
                    if (t.name == name) return t.gameObject;
            }
            return null;
        }

        // ==================================================================
        // Sampul & ilustrasi halaman cerita
        // ==================================================================
        private static void GenerateStoryArt()
        {
            var items = LoadItemImages();
            var roro = AssetDatabase.LoadAssetAtPath<Sprite>($"{IlluDir}/{RoroPortraitName}.png");
            Img roroImg = roro != null ? Img.Load($"{IlluDir}/{RoroPortraitName}.png") : null;
            Img Item(string n) => items.TryGetValue(n, out var i) ? i : null;

            // Sampul
            {
                var c = new ArtCanvas(1024, 1024);
                DrawScene(c, Sunset, 700f, 430f, 11);
                if (roroImg != null)
                {
                    c.Fill(Sdf_.Circle(760, 640, 300), Pal.Radial(760, 640, 300, Pal.A(Pal.GoldLight, 0.5f), Pal.A(Pal.GoldLight, 0f)), 2f);
                    c.DrawFit(roroImg, new Rect(560, 300, 420, 700), true);
                }
                FrameBorder(c);
                Save(c, "story_cover", Vector4.zero, IlluDir, 1024);
            }

            // Halaman 1..6 (1080 x 820)
            var pages = new (SceneTheme theme, string[] props, bool roro)[]
            {
                (Day, new string[0], true),
                (Sunset, new[] { "keris" }, false),
                (Night, new[] { "batu candi" }, false),
                (Night, new[] { "batu candi", "batu candi", "batu candi" }, false),
                (Dawn, new[] { "lesung", "jerami" }, false),
                (Day, new[] { "arca durga" }, false),
            };
            for (int p = 0; p < pages.Length; p++)
            {
                var c = new ArtCanvas(1080, 820);
                var page = pages[p];
                DrawScene(c, page.theme, 560f, p == 2 ? 250f : 380f, 20 + p);

                if (p == 2)
                {
                    // Seribu candi: deretan candi kecil di depan
                    for (int row = 0; row < 3; row++)
                        for (int i = 0; i < 12; i++)
                            DrawCandi(c, 40 + i * 92 + row * 30, 640 + row * 70, 40 + row * 10, 70 + row * 18, page.theme, 2.5f, false);
                }

                var props = page.props;
                for (int i = 0; i < props.Length; i++)
                {
                    var img = Item(props[i]);
                    if (img == null) continue;
                    float size = props.Length == 1 ? 420f : props.Length == 2 ? 320f : 220f;
                    float cx = props.Length == 1 ? 540f : 270f + i * (540f / Mathf.Max(1, props.Length - 1));
                    float cy = p == 3 ? 380f + (i % 2) * 110f : 560f;
                    c.Fill(Sdf_.Circle(cx, cy, size * 0.62f), Pal.Radial(cx, cy, size * 0.62f, Pal.A(p == 3 ? Pal.Hex("B9A2FF") : Pal.GoldLight, 0.6f), Pal.A(Pal.GoldLight, 0f)), 2f);
                    c.DrawFit(img, new Rect(cx - size * 0.5f, cy - size * 0.5f, size, size));
                }
                if (p == 3) Sparkles(c, 30, Pal.Hex("E6DAFF"), 33);
                if (page.roro && roroImg != null) c.DrawFit(roroImg, new Rect(700, 230, 320, 590), true);
                Save(c, $"story_page_{p + 1}", Vector4.zero, IlluDir, 1024);
            }
        }

        private static Dictionary<string, Img> LoadItemImages()
        {
            var result = new Dictionary<string, Img>();
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(ItemPsb);
            if (tex == null) return result;
            var sheet = Img.FromTextureGPU(tex);
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(ItemPsb))
                if (o is Sprite s) result[s.name] = sheet.CropSprite(s, 0).Trim(2);
            return result;
        }

        private static void Sparkles(ArtCanvas c, int count, Color col, int seed)
        {
            for (int i = 0; i < count; i++)
            {
                float x = Pal.Hash(i, seed) * c.W, y = Pal.Hash(seed, i) * c.H * 0.8f, r = 6 + Pal.Hash(i, i + seed) * 12;
                c.Fill(Sdf_.Star(x, y, r, r * 0.3f, 4), col, 1f, new Rect(x - r, y - r, r * 2, r * 2));
            }
        }

        private static void FrameBorder(ArtCanvas c)
        {
            var frame = Sdf_.Annulus(Sdf_.RoundRect(c.W * 0.5f, c.H * 0.5f, c.W * 0.5f - 14, c.H * 0.5f - 14, 40), 10);
            c.Fill(Sdf_.Offset(frame, 3), Pal.Outline);
            c.Fill(frame, Pal.VGrad(0, c.H, Pal.GoldLight, Pal.GoldDark));
        }
    }
}
