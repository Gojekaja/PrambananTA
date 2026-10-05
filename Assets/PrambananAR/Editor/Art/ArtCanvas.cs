using System;
using System.IO;
using UnityEngine;

namespace PrambananAR.UI.EditorTools.Art
{
    /// <summary>
    /// Kanvas raster sederhana untuk membuat aset visual secara prosedural (tanpa software gambar).
    /// Bentuk digambar memakai Signed Distance Function (SDF) sehingga tepinya halus (anti-alias).
    /// Koordinat piksel: (0,0) = kiri ATAS, y bertambah ke bawah.
    /// </summary>
    public class ArtCanvas
    {
        public delegate float Sdf(float x, float y);
        public delegate Color Paint(float x, float y);

        public readonly int W;
        public readonly int H;
        public readonly Color[] Px; // warna straight-alpha, indeks = y * W + x

        public ArtCanvas(int w, int h) : this(w, h, Color.clear) { }

        public ArtCanvas(int w, int h, Color fill)
        {
            W = w;
            H = h;
            Px = new Color[w * h];
            for (int i = 0; i < Px.Length; i++) Px[i] = fill;
        }

        // ------------------------------------------------------------------
        // Menggambar bentuk
        // ------------------------------------------------------------------

        /// <summary>Isi bentuk SDF. soften = lebar tepi halus (piksel); besar = buram (untuk bayangan/cahaya).</summary>
        public void Fill(Sdf sdf, Paint paint, float soften = 1f, Rect? bounds = null)
        {
            ClampBounds(bounds, soften, out int x0, out int y0, out int x1, out int y1);
            float inv = 1f / Mathf.Max(0.001f, soften);
            for (int y = y0; y < y1; y++)
            {
                float fy = y + 0.5f;
                for (int x = x0; x < x1; x++)
                {
                    float fx = x + 0.5f;
                    float d = sdf(fx, fy);
                    if (d > soften) continue;
                    float cov = Mathf.Clamp01(0.5f - d * inv);
                    if (cov <= 0f) continue;
                    Blend(y * W + x, paint(fx, fy), cov);
                }
            }
        }

        public void Fill(Sdf sdf, Color color, float soften = 1f, Rect? bounds = null) =>
            Fill(sdf, (x, y) => color, soften, bounds);

        /// <summary>Bentuk dengan garis tepi (outline) di luar isinya — gaya kartun.</summary>
        public void FillOutlined(Sdf sdf, Paint paint, Color outline, float outlineWidth, Rect? bounds = null)
        {
            if (outlineWidth > 0f) Fill(Sdf_.Offset(sdf, outlineWidth), outline, 1f, Grow(bounds, outlineWidth));
            Fill(sdf, paint, 1f, bounds);
        }

        public void FillOutlined(Sdf sdf, Color fill, Color outline, float outlineWidth, Rect? bounds = null) =>
            FillOutlined(sdf, (x, y) => fill, outline, outlineWidth, bounds);

        /// <summary>Bayangan lembut (offset + blur).</summary>
        public void Shadow(Sdf sdf, float dx, float dy, float blur, Color color, Rect? bounds = null)
        {
            Fill((x, y) => sdf(x - dx, y - dy) + blur * 0.5f, color, blur, Grow(Offset(bounds, dx, dy), blur));
        }

        // ------------------------------------------------------------------
        // Menempel gambar lain (sprite, potret)
        // ------------------------------------------------------------------

        /// <summary>Tempel gambar (bilinear) dengan pojok kiri-atas di (x, y) dan ukuran (w, h).</summary>
        public void Draw(Img src, float x, float y, float w, float h, Color? tint = null, float alpha = 1f)
        {
            Color t = tint ?? Color.white;
            int x0 = Mathf.Max(0, Mathf.FloorToInt(x)), y0 = Mathf.Max(0, Mathf.FloorToInt(y));
            int x1 = Mathf.Min(W, Mathf.CeilToInt(x + w)), y1 = Mathf.Min(H, Mathf.CeilToInt(y + h));
            for (int py = y0; py < y1; py++)
            {
                float v = (py + 0.5f - y) / h * src.H - 0.5f;
                for (int px = x0; px < x1; px++)
                {
                    float u = (px + 0.5f - x) / w * src.W - 0.5f;
                    Color c = src.Sample(u, v);
                    if (c.a <= 0f) continue;
                    c.r *= t.r; c.g *= t.g; c.b *= t.b;
                    Blend(py * W + px, c, alpha * t.a);
                }
            }
        }

        /// <summary>Tempel gambar agar muat dalam kotak (menjaga rasio), rata tengah-bawah opsional.</summary>
        public void DrawFit(Img src, Rect box, bool alignBottom = false, Color? tint = null, float alpha = 1f)
        {
            float s = Mathf.Min(box.width / src.W, box.height / src.H);
            float w = src.W * s, h = src.H * s;
            float x = box.x + (box.width - w) * 0.5f;
            float y = alignBottom ? box.yMax - h : box.y + (box.height - h) * 0.5f;
            Draw(src, x, y, w, h, tint, alpha);
        }

        // ------------------------------------------------------------------
        // Piksel & penyimpanan
        // ------------------------------------------------------------------
        public void Blend(int i, Color c, float cov)
        {
            float sa = c.a * cov;
            if (sa <= 0f) return;
            Color d = Px[i];
            float oa = sa + d.a * (1f - sa);
            if (oa <= 0f) { Px[i] = Color.clear; return; }
            float k = d.a * (1f - sa);
            Px[i] = new Color((c.r * sa + d.r * k) / oa, (c.g * sa + d.g * k) / oa, (c.b * sa + d.b * k) / oa, oa);
        }

        public Img ToImg() => new Img(W, H, (Color[])Px.Clone());

        /// <summary>Simpan sebagai PNG (Texture2D memakai origin kiri-bawah, jadi baris dibalik).</summary>
        public void SavePng(string assetPath)
        {
            var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
            var flipped = new Color[Px.Length];
            for (int y = 0; y < H; y++) Array.Copy(Px, y * W, flipped, (H - 1 - y) * W, W);
            tex.SetPixels(flipped);
            tex.Apply();
            string full = Path.Combine(Path.GetDirectoryName(Application.dataPath), assetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(full));
            File.WriteAllBytes(full, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
        }

        private void ClampBounds(Rect? bounds, float pad, out int x0, out int y0, out int x1, out int y1)
        {
            if (bounds.HasValue)
            {
                var b = bounds.Value;
                x0 = Mathf.Max(0, Mathf.FloorToInt(b.xMin - pad - 1));
                y0 = Mathf.Max(0, Mathf.FloorToInt(b.yMin - pad - 1));
                x1 = Mathf.Min(W, Mathf.CeilToInt(b.xMax + pad + 1));
                y1 = Mathf.Min(H, Mathf.CeilToInt(b.yMax + pad + 1));
            }
            else
            {
                x0 = 0; y0 = 0; x1 = W; y1 = H;
            }
        }

        private static Rect? Grow(Rect? r, float d) =>
            r.HasValue ? new Rect(r.Value.x - d, r.Value.y - d, r.Value.width + 2 * d, r.Value.height + 2 * d) : (Rect?)null;

        private static Rect? Offset(Rect? r, float dx, float dy) =>
            r.HasValue ? new Rect(r.Value.x + dx, r.Value.y + dy, r.Value.width, r.Value.height) : (Rect?)null;
    }

    /// <summary>Gambar sumber (piksel straight-alpha, origin kiri-atas).</summary>
    public class Img
    {
        public readonly int W;
        public readonly int H;
        public readonly Color[] Px;

        public Img(int w, int h, Color[] px)
        {
            W = w;
            H = h;
            Px = px;
        }

        public Color Get(int x, int y)
        {
            if (x < 0 || y < 0 || x >= W || y >= H) return Color.clear;
            return Px[y * W + x];
        }

        // Bilinear dengan premultiply agar tepi transparan tidak "berhalo" gelap
        public Color Sample(float u, float v)
        {
            int x = Mathf.FloorToInt(u), y = Mathf.FloorToInt(v);
            float fx = u - x, fy = v - y;
            Color a = Get(x, y), b = Get(x + 1, y), c = Get(x, y + 1), d = Get(x + 1, y + 1);
            float wa = (1 - fx) * (1 - fy) * a.a, wb = fx * (1 - fy) * b.a, wc = (1 - fx) * fy * c.a, wd = fx * fy * d.a;
            float alpha = wa + wb + wc + wd;
            if (alpha <= 0.0001f) return Color.clear;
            return new Color(
                (a.r * wa + b.r * wb + c.r * wc + d.r * wd) / alpha,
                (a.g * wa + b.g * wb + c.g * wc + d.g * wd) / alpha,
                (a.b * wa + b.b * wb + c.b * wc + d.b * wd) / alpha,
                alpha);
        }

        /// <summary>Muat PNG dari path aset (tanpa perlu Read/Write Enabled di importer).</summary>
        public static Img Load(string assetPath)
        {
            string full = Path.Combine(Path.GetDirectoryName(Application.dataPath), assetPath);
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.LoadImage(File.ReadAllBytes(full));
            var img = FromTexture(tex);
            UnityEngine.Object.DestroyImmediate(tex);
            return img;
        }

        /// <summary>Baca tekstur yang tidak Read/Write (mis. dari PSB) lewat GPU blit.</summary>
        public static Img FromTextureGPU(Texture tex)
        {
            var rt = RenderTexture.GetTemporary(tex.width, tex.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            var prev = RenderTexture.active;
            Graphics.Blit(tex, rt);
            var img = FromRenderTexture(rt, false);
            RenderTexture.active = prev;
            RenderTexture.ReleaseTemporary(rt);
            return img;
        }

        /// <summary>Baca RenderTexture. premultiplied = true untuk hasil render kamera bershader Sprite.</summary>
        public static Img FromRenderTexture(RenderTexture rt, bool premultiplied)
        {
            var prev = RenderTexture.active;
            RenderTexture.active = rt;
            var t2 = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, false);
            t2.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            t2.Apply();
            RenderTexture.active = prev;
            var img = FromTexture(t2);
            UnityEngine.Object.DestroyImmediate(t2);
            if (premultiplied)
            {
                for (int i = 0; i < img.Px.Length; i++)
                {
                    var c = img.Px[i];
                    if (c.a > 0.004f) img.Px[i] = new Color(Mathf.Clamp01(c.r / c.a), Mathf.Clamp01(c.g / c.a), Mathf.Clamp01(c.b / c.a), c.a);
                }
            }
            return img;
        }

        /// <summary>Potong area bergambar (alpha > 0) + margin.</summary>
        public Img Trim(int margin = 4)
        {
            int minX = W, minY = H, maxX = -1, maxY = -1;
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                    if (Px[y * W + x].a > 0.02f)
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
            if (maxX < 0) return this;
            minX = Mathf.Max(0, minX - margin); minY = Mathf.Max(0, minY - margin);
            maxX = Mathf.Min(W - 1, maxX + margin); maxY = Mathf.Min(H - 1, maxY + margin);
            return Crop(minX, minY, maxX - minX + 1, maxY - minY + 1);
        }

        public static Img FromTexture(Texture2D tex)
        {
            int w = tex.width, h = tex.height;
            var src = tex.GetPixels();
            var px = new Color[src.Length];
            for (int y = 0; y < h; y++) Array.Copy(src, (h - 1 - y) * w, px, y * w, w);
            return new Img(w, h, px);
        }

        /// <summary>Potong area (x, y kiri-atas).</summary>
        public Img Crop(int x, int y, int w, int h)
        {
            var px = new Color[w * h];
            for (int j = 0; j < h; j++)
                for (int i = 0; i < w; i++)
                    px[j * w + i] = Get(x + i, y + j);
            return new Img(w, h, px);
        }

        /// <summary>Potong sesuai rect Sprite (koordinat tekstur Unity, kiri-bawah) dengan skala tekstur asli.</summary>
        public Img CropSprite(Sprite sprite, int pad = 2)
        {
            float k = (float)W / sprite.texture.width; // tekstur bisa diperkecil oleh Max Size importer
            var r = sprite.rect;
            int x = Mathf.FloorToInt(r.x * k) - pad;
            int w = Mathf.CeilToInt(r.width * k) + pad * 2;
            int h = Mathf.CeilToInt(r.height * k) + pad * 2;
            int yTop = H - Mathf.CeilToInt((r.y + r.height) * k) - pad;
            return Crop(x, yTop, w, h);
        }

        public Img Map(Func<Color, Color> f)
        {
            var px = new Color[Px.Length];
            for (int i = 0; i < Px.Length; i++) px[i] = Px[i].a > 0f ? f(Px[i]) : Px[i];
            return new Img(W, H, px);
        }

        /// <summary>Geser hue (derajat) + kali saturasi/kecerahan; mempertahankan bayangan & kilau asli.</summary>
        public Img HueShift(float degrees, float satMul = 1f, float valMul = 1f) => Map(c =>
        {
            Color.RGBToHSV(c, out float h, out float s, out float v);
            h = Mathf.Repeat(h + degrees / 360f, 1f);
            var o = Color.HSVToRGB(h, Mathf.Clamp01(s * satMul), Mathf.Clamp01(v * valMul));
            o.a = c.a;
            return o;
        });

        /// <summary>Ganti warna berdasar kecerahan (gelap -> terang) memakai gradasi baru.</summary>
        public Img GradientMap(params Color[] stops)
        {
            float lo = 1f, hi = 0f;
            foreach (var c in Px)
            {
                if (c.a < 0.5f) continue;
                float l = Luma(c);
                lo = Mathf.Min(lo, l);
                hi = Mathf.Max(hi, l);
            }
            float range = Mathf.Max(0.001f, hi - lo);
            return Map(c =>
            {
                var o = Pal.Sample(stops, (Luma(c) - lo) / range);
                o.a = c.a;
                return o;
            });
        }

        public static float Luma(Color c) => c.r * 0.299f + c.g * 0.587f + c.b * 0.114f;
    }

    /// <summary>Kumpulan SDF dasar (nilai negatif = di dalam bentuk).</summary>
    public static class Sdf_
    {
        public static ArtCanvas.Sdf Circle(float cx, float cy, float r) =>
            (x, y) => Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) - r;

        public static ArtCanvas.Sdf Ellipse(float cx, float cy, float rx, float ry) => (x, y) =>
        {
            float nx = (x - cx) / rx, ny = (y - cy) / ry;
            float k = Mathf.Sqrt(nx * nx + ny * ny);
            return (k - 1f) * Mathf.Min(rx, ry);
        };

        public static ArtCanvas.Sdf RoundRect(float cx, float cy, float hw, float hh, float r) => (x, y) =>
        {
            float qx = Mathf.Abs(x - cx) - (hw - r);
            float qy = Mathf.Abs(y - cy) - (hh - r);
            float ox = Mathf.Max(qx, 0f), oy = Mathf.Max(qy, 0f);
            return Mathf.Sqrt(ox * ox + oy * oy) + Mathf.Min(Mathf.Max(qx, qy), 0f) - r;
        };

        /// <summary>Kotak membulat dari pojok kiri-atas + ukuran.</summary>
        public static ArtCanvas.Sdf Box(float x, float y, float w, float h, float r) =>
            RoundRect(x + w * 0.5f, y + h * 0.5f, w * 0.5f, h * 0.5f, r);

        /// <summary>Garis tebal berujung bulat (kapsul).</summary>
        public static ArtCanvas.Sdf Capsule(float ax, float ay, float bx, float by, float r) => (x, y) =>
        {
            float pax = x - ax, pay = y - ay, bax = bx - ax, bay = by - ay;
            float h = Mathf.Clamp01((pax * bax + pay * bay) / Mathf.Max(0.0001f, bax * bax + bay * bay));
            float dx = pax - bax * h, dy = pay - bay * h;
            return Mathf.Sqrt(dx * dx + dy * dy) - r;
        };

        /// <summary>Poligon sembarang (titik berurutan).</summary>
        public static ArtCanvas.Sdf Polygon(params Vector2[] v) => (x, y) =>
        {
            var p = new Vector2(x, y);
            float d = Vector2.Dot(p - v[0], p - v[0]);
            float s = 1f;
            for (int i = 0, j = v.Length - 1; i < v.Length; j = i, i++)
            {
                Vector2 e = v[j] - v[i];
                Vector2 w = p - v[i];
                Vector2 b = w - e * Mathf.Clamp01(Vector2.Dot(w, e) / Vector2.Dot(e, e));
                d = Mathf.Min(d, Vector2.Dot(b, b));
                bool c1 = p.y >= v[i].y, c2 = p.y < v[j].y, c3 = e.x * w.y > e.y * w.x;
                if ((c1 && c2 && c3) || (!c1 && !c2 && !c3)) s = -s;
            }
            return s * Mathf.Sqrt(d);
        };

        /// <summary>Bintang n sudut (sudut pertama menghadap ke atas).</summary>
        public static ArtCanvas.Sdf Star(float cx, float cy, float outer, float inner, int points = 5, float rotDeg = 0f)
        {
            var v = new Vector2[points * 2];
            for (int i = 0; i < v.Length; i++)
            {
                float a = (rotDeg - 90f + i * 180f / points) * Mathf.Deg2Rad;
                float r = i % 2 == 0 ? outer : inner;
                v[i] = new Vector2(cx + Mathf.Cos(a) * r, cy + Mathf.Sin(a) * r);
            }
            return Polygon(v);
        }

        public static ArtCanvas.Sdf Union(params ArtCanvas.Sdf[] s) => (x, y) =>
        {
            float d = float.MaxValue;
            for (int i = 0; i < s.Length; i++) d = Mathf.Min(d, s[i](x, y));
            return d;
        };

        public static ArtCanvas.Sdf Subtract(ArtCanvas.Sdf a, ArtCanvas.Sdf b) => (x, y) => Mathf.Max(a(x, y), -b(x, y));
        public static ArtCanvas.Sdf Intersect(ArtCanvas.Sdf a, ArtCanvas.Sdf b) => (x, y) => Mathf.Max(a(x, y), b(x, y));
        public static ArtCanvas.Sdf Offset(ArtCanvas.Sdf a, float d) => (x, y) => a(x, y) - d;
        public static ArtCanvas.Sdf Annulus(ArtCanvas.Sdf a, float halfWidth) => (x, y) => Mathf.Abs(a(x, y)) - halfWidth;

        /// <summary>Putar bentuk (derajat) terhadap titik pusat.</summary>
        public static ArtCanvas.Sdf Rotate(ArtCanvas.Sdf a, float cx, float cy, float deg)
        {
            float c = Mathf.Cos(-deg * Mathf.Deg2Rad), s = Mathf.Sin(-deg * Mathf.Deg2Rad);
            return (x, y) =>
            {
                float dx = x - cx, dy = y - cy;
                return a(cx + dx * c - dy * s, cy + dx * s + dy * c);
            };
        }

        /// <summary>Cerminkan horizontal terhadap garis x = cx.</summary>
        public static ArtCanvas.Sdf MirrorX(ArtCanvas.Sdf a, float cx) => (x, y) => a(2f * cx - x, y);

        /// <summary>Setengah bidang: di dalam bila y lebih kecil dari yLine (bagian atas).</summary>
        public static ArtCanvas.Sdf Above(float yLine) => (x, y) => y - yLine;
        public static ArtCanvas.Sdf Below(float yLine) => (x, y) => yLine - y;
        public static ArtCanvas.Sdf LeftOf(float xLine) => (x, y) => x - xLine;
        public static ArtCanvas.Sdf RightOf(float xLine) => (x, y) => xLine - x;
    }

    /// <summary>Palet tema "Batu Candi & Emas" + helper warna.</summary>
    public static class Pal
    {
        public static readonly Color Outline = Hex("4A2E1B");
        public static readonly Color DarkBrown = Hex("3D2A1E");
        public static readonly Color Brown = Hex("7A4E2D");
        public static readonly Color Sogan = Hex("8B5A2B");
        public static readonly Color Tan = Hex("D9B27C");
        public static readonly Color Cream = Hex("FFF4DC");
        public static readonly Color Parchment = Hex("F6E3BC");
        public static readonly Color Gold = Hex("F2B33D");
        public static readonly Color GoldLight = Hex("FFD966");
        public static readonly Color GoldDark = Hex("C98A1B");
        public static readonly Color Terracotta = Hex("D9693A");
        public static readonly Color Red = Hex("D64541");
        public static readonly Color Leaf = Hex("5DAA4A");
        public static readonly Color LeafDark = Hex("3E7F35");
        public static readonly Color Teal = Hex("2F9DB3");
        public static readonly Color Sky = Hex("8FD3F4");
        public static readonly Color Stone = Hex("8C7B6E");
        public static readonly Color StoneLight = Hex("B9AA9A");
        public static readonly Color StoneDark = Hex("5E5048");

        public static Color Hex(string hex, float a = 1f)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            c.a = a;
            return c;
        }

        public static Color A(Color c, float a)
        {
            c.a = a;
            return c;
        }

        public static Color Sample(Color[] stops, float t)
        {
            t = Mathf.Clamp01(t) * (stops.Length - 1);
            int i = Mathf.Min(stops.Length - 2, Mathf.FloorToInt(t));
            return Color.Lerp(stops[i], stops[i + 1], t - i);
        }

        /// <summary>Gradasi vertikal antara y0 (atas) dan y1 (bawah).</summary>
        public static ArtCanvas.Paint VGrad(float y0, float y1, params Color[] stops) =>
            (x, y) => Sample(stops, (y - y0) / Mathf.Max(0.001f, y1 - y0));

        public static ArtCanvas.Paint Radial(float cx, float cy, float r, params Color[] stops) =>
            (x, y) => Sample(stops, Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / r);

        /// <summary>Noise nilai sederhana (deterministik) untuk tekstur rumput/batu.</summary>
        public static float Noise(float x, float y)
        {
            int xi = Mathf.FloorToInt(x), yi = Mathf.FloorToInt(y);
            float fx = x - xi, fy = y - yi;
            fx = fx * fx * (3 - 2 * fx);
            fy = fy * fy * (3 - 2 * fy);
            float a = Hash(xi, yi), b = Hash(xi + 1, yi), c = Hash(xi, yi + 1), d = Hash(xi + 1, yi + 1);
            return Mathf.Lerp(Mathf.Lerp(a, b, fx), Mathf.Lerp(c, d, fx), fy);
        }

        public static float Hash(int x, int y)
        {
            unchecked
            {
                int h = x * 374761393 + y * 668265263;
                h = (h ^ (h >> 13)) * 1274126177;
                return ((h ^ (h >> 16)) & 0x7fffffff) / (float)0x7fffffff;
            }
        }
    }
}
