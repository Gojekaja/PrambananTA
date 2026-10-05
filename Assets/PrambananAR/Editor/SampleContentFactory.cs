using System.Collections.Generic;
using System.IO;
using PrambananAR.UI.EditorTools.Art;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace PrambananAR.UI.EditorTools
{
    /// <summary>
    /// Membuat aset data (tema, tokoh, benda, zona, dialog, cerita) + memasang aset visual tema.
    /// Aset yang SUDAH ADA tidak ditimpa (kecuali field kosong / sprite placeholder), jadi aman dijalankan berulang kali.
    /// </summary>
    internal static class SampleContentFactory
    {
        public const string Root = "Assets/PrambananAR";
        public const string DataDir = Root + "/Data";
        public const string CharDir = DataDir + "/Characters";
        public const string ItemDir = DataDir + "/Items";
        public const string DialogueDir = DataDir + "/Dialogues";
        public const string PlaceholderDir = Root + "/Art/Placeholders";
        public const string FontDir = Root + "/Art/Fonts";
        private const string TitleFontTtf = "Assets/300Mind/2D Game UI Kit/Fonts/GROBOLD.ttf";

        // Titik geofence Candi Roro Jonggrang hasil ukur di lokasi (dari GPSManager)
        private const double RoroLat = -7.752020;
        private const double RoroLon = 110.491467;
        private const float RoroRadius = 10f;

        public class Bundle
        {
            public UITheme theme;
            public CharacterDatabase characters;
            public ItemDatabase items;
            public ZoneDatabase zones;
            public DialogueData intro;
            public StoryData story;
        }

        // id, nama, gelar, deskripsi, zona, warna placeholder, dialog, status
        private struct CharDef
        {
            public string id, name, title, desc, zone;
            public Color color;
            public string[] lines;
            public bool comingSoon;
            public string wantedItem, ask, thanks, reject;
        }

        private static readonly CharDef[] Characters =
        {
            new CharDef { id = "roro_jonggrang", name = "Roro Jonggrang", title = "Putri Kerajaan Prambanan", zone = "RJ",
                color = new Color(0.93f, 0.45f, 0.55f),
                desc = "Putri cantik dan cerdik dari Kerajaan Prambanan. Ia meminta Bandung Bondowoso membangun seribu candi dalam satu malam.",
                lines = new[] {
                    "Halo, Penjelajah Cilik! Aku Roro Jonggrang, putri Kerajaan Prambanan.",
                    "Dahulu, Bandung Bondowoso ingin menikah denganku. Aku memberinya syarat: membangun seribu candi dalam satu malam!",
                    "Mau tahu kelanjutan ceritaku? Buka halaman Cerita, ya!" },
                wantedItem = "bunga",
                ask = "Aku sedang mencari sesuatu yang harum dan berwarna cantik...",
                thanks = "Wah, bunga yang cantik sekali! Terima kasih, Penjelajah Cilik!",
                reject = "Hmm... bukan itu yang aku cari. Coba benda lain, ya!" },
            new CharDef { id = "bandung_bondowoso", name = "Bandung Bondowoso", title = "Ksatria Sakti dari Pengging", zone = "A",
                color = new Color(0.30f, 0.50f, 0.85f), comingSoon = true,
                desc = "Ksatria sakti dari Kerajaan Pengging. Dengan bantuan para jin, ia hampir menyelesaikan seribu candi.",
                lines = new[] {
                    "Akulah Bandung Bondowoso, ksatria sakti dari Pengging!",
                    "Dengan bantuan para jin, aku membangun candi-candi dengan sangat cepat." } },
            new CharDef { id = "prabu_baka", name = "Prabu Baka", title = "Raja Prambanan", zone = "B",
                color = new Color(0.65f, 0.35f, 0.25f), comingSoon = true,
                desc = "Raja Prambanan yang bertubuh besar, ayah dari Roro Jonggrang.",
                lines = new[] {
                    "Hohoho! Aku Prabu Baka, raja negeri Prambanan.",
                    "Roro Jonggrang adalah putriku yang sangat kusayangi." } },
            new CharDef { id = "patih_gupolo", name = "Patih Gupolo", title = "Patih Setia Prambanan", zone = "C",
                color = new Color(0.45f, 0.60f, 0.35f), comingSoon = true,
                desc = "Patih setia Prabu Baka yang selalu menjaga Roro Jonggrang.",
                lines = new[] {
                    "Aku Patih Gupolo, penjaga setia Kerajaan Prambanan.",
                    "Tugasku adalah melindungi Putri Roro Jonggrang." } },
            new CharDef { id = "gadis_desa", name = "Gadis Penumbuk Padi", title = "Sahabat Roro Jonggrang", zone = "E",
                color = new Color(0.95f, 0.70f, 0.30f), comingSoon = true,
                desc = "Para gadis desa yang menumbuk padi di tengah malam sehingga ayam jantan mengira pagi telah tiba.",
                lines = new[] {
                    "Tok... tok... tok! Kami menumbuk padi di tengah malam.",
                    "Suaranya membuat ayam jantan mengira hari sudah pagi!" } },
            new CharDef { id = "ayam_jago", name = "Ayam Jago", title = "Penanda Pagi", zone = "G",
                color = new Color(0.90f, 0.30f, 0.25f), comingSoon = true,
                desc = "Ayam jantan yang berkokok karena mengira fajar telah tiba.",
                lines = new[] {
                    "Kukuruyuuuk! Apakah hari sudah pagi?",
                    "Mendengar kokokku, para jin berhenti bekerja dan pergi." } },
            new CharDef { id = "para_jin", name = "Para Jin", title = "Pembantu Bandung Bondowoso", zone = "H",
                color = new Color(0.55f, 0.40f, 0.80f), comingSoon = true,
                desc = "Makhluk gaib yang membantu Bandung Bondowoso membangun candi dalam satu malam.",
                lines = new[] {
                    "Wuuush! Kami para jin pembantu Bandung Bondowoso.",
                    "Kami sudah membangun 999 candi... tinggal satu lagi!" } },
        };

        // id, nama sprite di Art/item.psb, nama tampilan, deskripsi (urutan = urutan di tas)
        private static readonly (string id, string sprite, string name, string desc)[] Items =
        {
            ("bunga", "bunga", "Bunga", "Bunga harum berwarna cantik. Cocok sebagai hadiah untuk sang putri."),
            ("keris", "keris", "Keris", "Senjata pusaka khas Jawa dengan bilah berlekuk, milik para ksatria."),
            ("cangkul", "cangkul", "Cangkul", "Alat untuk menggemburkan tanah di sawah dan kebun."),
            ("lesung", "lesung", "Lesung", "Tempat menumbuk padi. Suaranya bisa membangunkan ayam jantan!"),
            ("jerami", "jerami", "Jerami", "Batang padi kering. Dibakar agar langit tampak terang seperti pagi."),
            ("batu_candi", "batu candi", "Batu Candi", "Batu andesit berukir yang disusun untuk membangun candi."),
            ("arca_durga", "arca durga", "Arca Durga", "Patung Dewi Durga. Menurut legenda, inilah wujud Roro Jonggrang setelah dikutuk."),
        };

        // id, nama, deskripsi, posisi pada gambar peta (0..1, kiri-bawah), id tokoh
        private static readonly (string id, string name, string desc, Vector2 pos, string charId)[] Zones =
        {
            ("RJ", "Candi Roro Jonggrang", "Kompleks candi Hindu terbesar di Indonesia, dibangun pada abad ke-9 Masehi.", new Vector2(0.39f, 0.49f), "roro_jonggrang"),
            ("A", "Siwa Mandala", "Taman di sisi barat laut candi utama.", new Vector2(0.30f, 0.66f), "bandung_bondowoso"),
            ("B", "Brahma Mandala", "Taman di sisi utara candi utama.", new Vector2(0.40f, 0.665f), "prabu_baka"),
            ("C", "Wisnu Mandala", "Taman luas di sisi timur laut candi utama.", new Vector2(0.51f, 0.55f), "patih_gupolo"),
            ("D", "Garuda Mandala", "Titik informasi di sisi tenggara candi utama.", new Vector2(0.51f, 0.39f), ""),
            ("E", "Rama Shinta Camping Ground", "Area berkemah di tepi Sungai Opak.", new Vector2(0.245f, 0.585f), "gadis_desa"),
            ("F", "Taman Agastya", "Titik informasi di sisi barat daya candi utama.", new Vector2(0.28f, 0.39f), ""),
            ("G", "Lapangan Selatan", "Lapangan rumput luas di sisi selatan.", new Vector2(0.38f, 0.34f), "ayam_jago"),
            ("H", "Taman Candi Sewu", "Taman di dekat Candi Sewu, candi Buddha di utara kompleks.", new Vector2(0.56f, 0.87f), "para_jin"),
        };

        private static readonly (string title, string[] sentences)[] StoryPages =
        {
            ("Kerajaan Prambanan", new[] {
                "Dahulu kala, ada sebuah kerajaan bernama Prambanan.",
                "Rajanya bernama Prabu Baka.",
                "Prabu Baka mempunyai seorang putri yang cantik bernama Roro Jonggrang." }),
            ("Datangnya Bandung Bondowoso", new[] {
                "Suatu hari, datanglah ksatria sakti bernama Bandung Bondowoso.",
                "Ia mengalahkan Prabu Baka dalam sebuah pertempuran.",
                "Bandung Bondowoso lalu ingin menikahi Roro Jonggrang." }),
            ("Syarat Seribu Candi", new[] {
                "Roro Jonggrang tidak ingin menikah dengan Bandung Bondowoso.",
                "Ia pun memberi syarat yang sangat sulit.",
                "\"Bangunkan aku seribu candi dalam satu malam!\"" }),
            ("Bantuan Para Jin", new[] {
                "Bandung Bondowoso memanggil para jin untuk membantunya.",
                "Wuuush! Candi-candi berdiri dengan sangat cepat.",
                "Sebelum pagi, sudah ada sembilan ratus sembilan puluh sembilan candi!" }),
            ("Kokok Ayam Jago", new[] {
                "Roro Jonggrang menjadi cemas.",
                "Ia mengajak para gadis desa menumbuk padi dan menyalakan api.",
                "Ayam jantan mengira pagi telah tiba, lalu berkokok: Kukuruyuk!",
                "Para jin pun berhenti bekerja dan pergi." }),
            ("Arca Roro Jonggrang", new[] {
                "Bandung Bondowoso tahu bahwa ia telah dicurangi.",
                "Ia sangat marah dan mengubah Roro Jonggrang menjadi arca batu.",
                "Arca itu melengkapi candi yang keseribu.",
                "Hingga kini, kita dapat melihat arcanya di dalam Candi Prambanan.",
                "Pesan cerita: selalu bersikap jujur dan jangan mudah marah, ya!" }),
        };

        // ------------------------------------------------------------------
        public static Bundle CreateOrLoadAll()
        {
            EnsureFolder(DataDir);
            EnsureFolder(CharDir);
            EnsureFolder(ItemDir);
            EnsureFolder(DialogueDir);
            EnsureFolder(PlaceholderDir);
            EnsureFolder(FontDir);

            // Aset visual tema dibuat otomatis bila belum ada (Tools > Prambanan AR > 0)
            if (ThemeArtGenerator.Load("btn_primary") == null) ThemeArtGenerator.GenerateAll();

            var ring = MakeTexture("Placeholder_Ring", 256, 256, (x, y) => RingPixel(x, y, 256, 0.80f, 1f));
            var glow = MakeTexture("Placeholder_Glow", 256, 256, (x, y) => GlowPixel(x, y, 256));
            var guide = Illu("guide_kak_pandu") ?? MakeTexture("Placeholder_Guide", 256, 384, (x, y) => PawnPixel(x, y, new Color(0.35f, 0.65f, 0.55f)));
            var roroPortrait = Illu(ThemeArtGenerator.RoroPortraitName);

            var b = new Bundle();
            b.theme = LoadOrCreate<UITheme>(DataDir + "/UITheme.asset", t => { });
            ApplyThemeArt(b.theme);
            if (b.theme.ring == null) b.theme.ring = ring;
            if (b.theme.glow == null) b.theme.glow = glow;
            if (b.theme.spotlight == null) b.theme.spotlight = glow;
            if (b.theme.guidePortrait == null || IsPlaceholder(b.theme.guidePortrait)) b.theme.guidePortrait = guide;
            EditorUtility.SetDirty(b.theme);

            b.items = CreateItems();

            b.intro = LoadOrCreate<DialogueData>(DialogueDir + "/Dialogue_Intro.asset", d =>
            {
                string[] intro =
                {
                    "Halo, Penjelajah Cilik! Aku Kak Pandu. Selamat datang di Taman Wisata Candi Prambanan!",
                    "Tahukah kamu? Di tempat ini tersimpan legenda tentang seorang putri bernama Roro Jonggrang.",
                    "Ayo jelajahi area candi bersama Ayah, Bunda, atau Guru. Di titik bertanda pada peta, tokoh cerita akan muncul!",
                    "Saat tokoh muncul, pilih benda yang tepat dari tas, lalu usap layar ke atas untuk memberikannya. Siap berpetualang?"
                };
                foreach (var line in intro)
                    d.lines.Add(new DialogueLine { speaker = "Kak Pandu", portrait = guide, text = line });
            });
            ReplacePlaceholderPortraits(b.intro, guide);

            b.characters = LoadOrCreate<CharacterDatabase>(DataDir + "/CharacterDatabase.asset", db =>
            {
                db.milestones.Add(new CollectionMilestone { rewardName = "Lencana Penjelajah", requiredCount = 1, icon = ThemeArtGenerator.Load("icon_badge") });
                db.milestones.Add(new CollectionMilestone { rewardName = "Selendang Penari", requiredCount = 4, icon = ThemeArtGenerator.Load("icon_scarf") });
                db.milestones.Add(new CollectionMilestone { rewardName = "Mahkota Prambanan", requiredCount = 7, icon = ThemeArtGenerator.Load("icon_crown") });
            });

            if (b.characters.characters.Count == 0)
            {
                foreach (var def in Characters)
                {
                    var c = def; // salin untuk lambda
                    var portrait = c.id == "roro_jonggrang" && roroPortrait != null
                        ? roroPortrait
                        : MakeTexture("Placeholder_" + c.id, 256, 384, (x, y) => PawnPixel(x, y, c.color));
                    var dialogue = LoadOrCreate<DialogueData>($"{DialogueDir}/Dialogue_{c.id}.asset", d =>
                    {
                        foreach (var line in c.lines)
                            d.lines.Add(new DialogueLine { speaker = c.name, portrait = portrait, text = line });
                    });
                    var data = LoadOrCreate<CharacterData>($"{CharDir}/Char_{c.id}.asset", cd =>
                    {
                        cd.id = c.id;
                        cd.displayName = c.name;
                        cd.title = c.title;
                        cd.description = c.desc;
                        cd.zoneId = c.zone;
                        cd.portrait = portrait;
                        cd.encounterDialogue = dialogue;
                        cd.comingSoon = c.comingSoon;
                        cd.wantedItem = string.IsNullOrEmpty(c.wantedItem) ? null : b.items.Find(c.wantedItem);
                        cd.askLine = c.ask;
                        cd.thanksLine = c.thanks;
                        cd.rejectLine = c.reject;
                    });
                    b.characters.characters.Add(data);
                }
                EditorUtility.SetDirty(b.characters);
            }

            // Potret Roro Jonggrang asli menggantikan placeholder (bila data dibuat sebelum potret dirender)
            var roro = b.characters.Find("roro_jonggrang");
            if (roro != null && roroPortrait != null && IsPlaceholder(roro.portrait))
            {
                roro.portrait = roroPortrait;
                EditorUtility.SetDirty(roro);
                ReplacePlaceholderPortraits(roro.encounterDialogue, roroPortrait);
            }
            var roroIcon = Illu(ThemeArtGenerator.RoroPortraitName + "_icon");
            if (roro != null && roro.icon == null && roroIcon != null)
            {
                roro.icon = roroIcon;
                EditorUtility.SetDirty(roro);
            }

            b.zones = LoadOrCreate<ZoneDatabase>(DataDir + "/ZoneDatabase.asset", zdb =>
            {
                foreach (var z in Zones)
                {
                    zdb.MapToGeo(z.pos, out double lat, out double lon);
                    float radius = 25f;
                    if (z.id == "RJ")
                    {
                        lat = RoroLat;
                        lon = RoroLon;
                        radius = RoroRadius;
                    }
                    zdb.zones.Add(new MapZone
                    {
                        id = z.id, displayName = z.name, description = z.desc, mapPosition = z.pos,
                        latitude = lat, longitude = lon, radiusMeters = radius, characterId = z.charId
                    });
                }
            });
            if (b.zones.mapSprite == null)
            {
                b.zones.mapSprite = Illu("map_prambanan");
                EditorUtility.SetDirty(b.zones);
            }

            b.story = LoadOrCreate<StoryData>(DataDir + "/Story_RoroJonggrang.asset", s =>
            {
                s.title = "Legenda Roro Jonggrang";
                s.subtitle = "Cerita Rakyat dari Prambanan";
                s.synopsis = "Kisah Putri Roro Jonggrang dan Bandung Bondowoso yang ingin membangun seribu candi dalam satu malam. " +
                             "Ikuti ceritanya, lalu temui para tokohnya di area Candi Prambanan!";
                s.tags = new[] { "Legenda", "Yogyakarta", "Budaya" };
                foreach (var p in StoryPages)
                    s.pages.Add(new StoryPage { chapterTitle = p.title, sentences = p.sentences, secondsPerSentence = 3.5f });
            });
            if (b.story.cover == null) b.story.cover = Illu("story_cover");
            for (int i = 0; i < b.story.pages.Count; i++)
                if (b.story.pages[i].illustration == null) b.story.pages[i].illustration = Illu($"story_page_{i + 1}");
            EditorUtility.SetDirty(b.story);

            AssetDatabase.SaveAssets();
            return b;
        }

        // ------------------------------------------------------------------
        // Tema: sprite hasil ThemeArtGenerator + font judul
        // ------------------------------------------------------------------
        private static void ApplyThemeArt(UITheme t)
        {
            Sprite L(string n) => ThemeArtGenerator.Load(n);
            void Set(ref Sprite field, string n)
            {
                if (field == null) field = L(n);
            }

            Set(ref t.panelPopup, "panel_popup");
            Set(ref t.panelCard, "panel_card");
            Set(ref t.panelHeader, "panel_header");
            Set(ref t.topBar, "top_bar");
            Set(ref t.bottomBar, "top_bar");
            Set(ref t.dialogBox, "dialog_box");
            Set(ref t.nameTag, "name_tag");
            Set(ref t.chip, "chip");
            Set(ref t.buttonPrimary, "btn_primary");
            Set(ref t.buttonSecondary, "btn_secondary");
            Set(ref t.buttonSuccess, "btn_success");
            Set(ref t.buttonDanger, "btn_danger");
            Set(ref t.buttonCircle, "btn_circle");
            Set(ref t.iconClose, "icon_close");
            Set(ref t.iconHelp, "icon_help");
            Set(ref t.iconBack, "icon_back");
            Set(ref t.iconMenu, "icon_menu");
            Set(ref t.iconCamera, "icon_camera");
            Set(ref t.iconMap, "icon_map");
            Set(ref t.iconCollection, "icon_collection");
            Set(ref t.iconStory, "icon_story");
            Set(ref t.iconGuide, "icon_guide");
            Set(ref t.iconNearby, "icon_nearby");
            Set(ref t.iconLock, "icon_lock");
            Set(ref t.iconCheck, "icon_check");
            Set(ref t.iconStar, "icon_star");
            Set(ref t.iconCoin, "icon_coin");
            Set(ref t.iconPlay, "icon_play");
            Set(ref t.iconNext, "icon_next");
            Set(ref t.iconPrev, "icon_prev");
            Set(ref t.progressBarBg, "progress_bg");
            Set(ref t.progressBarFill, "progress_fill");
            Set(ref t.ring, "ring");
            Set(ref t.glow, "glow");
            Set(ref t.levelBadge, "level_badge");
            Set(ref t.mapPin, "map_pin");
            Set(ref t.mapPinTail, "map_pin_tail");
            Set(ref t.playerMarker, "player_marker");
            Set(ref t.handPointer, "hand_pointer");
            Set(ref t.toggleBox, "toggle_box");
            Set(ref t.toggleCheck, "toggle_check");
            Set(ref t.spotlight, "spotlight");
            if (t.loadingBackground == null) t.loadingBackground = Illu("bg_loading");

            if (t.titleFont == null || t.titleOutlineMaterial == null)
            {
                var font = CreateTitleFont(out var outline);
                if (t.titleFont == null) t.titleFont = font;
                if (t.titleOutlineMaterial == null) t.titleOutlineMaterial = outline;
            }
        }

        /// <summary>Font judul kartun (GROBOLD dari 2D Game UI Kit) + material bergaris tepi.</summary>
        private static TMP_FontAsset CreateTitleFont(out Material outline)
        {
            string assetPath = FontDir + "/GROBOLD SDF.asset";
            string outlinePath = FontDir + "/GROBOLD SDF Outline.mat";
            var fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            outline = AssetDatabase.LoadAssetAtPath<Material>(outlinePath);

            if (fontAsset == null)
            {
                var ttf = AssetDatabase.LoadAssetAtPath<Font>(TitleFontTtf);
                if (ttf == null) return null;
                // Atlas tunggal (tanpa multi-atlas) agar material turunan selalu memakai tekstur yang sama
                fontAsset = TMP_FontAsset.CreateFontAsset(ttf, 72, 9, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, false);
                fontAsset.name = "GROBOLD SDF";
                AssetDatabase.CreateAsset(fontAsset, assetPath);
                fontAsset.atlasTextures[0].name = "GROBOLD SDF Atlas";
                AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
                fontAsset.material.name = "GROBOLD SDF Material";
                AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                // Isi glyph umum bahasa Indonesia sekarang supaya tidak perlu membangun atlas saat main
                fontAsset.TryAddCharacters(" !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~", out _);
                EditorUtility.SetDirty(fontAsset);
            }

            if (outline == null && fontAsset != null)
            {
                outline = new Material(fontAsset.material) { name = "GROBOLD SDF Outline" };
                outline.EnableKeyword(ShaderUtilities.Keyword_Outline);
                outline.SetFloat(ShaderUtilities.ID_OutlineWidth, 0.22f);
                outline.SetColor(ShaderUtilities.ID_OutlineColor, new Color(0.24f, 0.14f, 0.08f));
                outline.EnableKeyword(ShaderUtilities.Keyword_Underlay);
                outline.SetColor(ShaderUtilities.ID_UnderlayColor, new Color(0f, 0f, 0f, 0.45f));
                outline.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, -0.6f);
                outline.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0.2f);
                AssetDatabase.CreateAsset(outline, outlinePath);
            }
            return fontAsset;
        }

        private static ItemDatabase CreateItems()
        {
            var sprites = new Dictionary<string, Sprite>();
            foreach (var o in AssetDatabase.LoadAllAssetsAtPath(ThemeArtGenerator.ItemPsb))
                if (o is Sprite s) sprites[s.name] = s;

            var db = LoadOrCreate<ItemDatabase>(DataDir + "/ItemDatabase.asset", d => { });
            if (db.items.Count > 0) return db;

            foreach (var def in Items)
            {
                var item = LoadOrCreate<ItemData>($"{ItemDir}/Item_{def.id}.asset", it =>
                {
                    it.id = def.id;
                    it.displayName = def.name;
                    it.description = def.desc;
                    it.icon = sprites.TryGetValue(def.sprite, out var sp) ? sp : null;
                });
                db.items.Add(item);
            }
            EditorUtility.SetDirty(db);
            return db;
        }

        private static Sprite Illu(string name) => ThemeArtGenerator.Load(name, ThemeArtGenerator.IlluDir);

        private static bool IsPlaceholder(Sprite s) =>
            s == null || AssetDatabase.GetAssetPath(s).StartsWith(PlaceholderDir);

        private static void ReplacePlaceholderPortraits(DialogueData d, Sprite portrait)
        {
            if (d == null || portrait == null) return;
            foreach (var line in d.lines)
                if (IsPlaceholder(line.portrait)) line.portrait = portrait;
            EditorUtility.SetDirty(d);
        }

        // ------------------------------------------------------------------
        // Helper aset
        // ------------------------------------------------------------------
        private static T LoadOrCreate<T>(string path, System.Action<T> init) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;
            asset = ScriptableObject.CreateInstance<T>();
            init(asset);
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        // ------------------------------------------------------------------
        // Pembuat sprite placeholder prosedural (dipakai sampai aset asli tersedia)
        // ------------------------------------------------------------------
        private static Sprite MakeTexture(string name, int w, int h, System.Func<int, int, Color> pixel)
        {
            string path = $"{PlaceholderDir}/{name}.png";
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing != null) return existing;

            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            var pixels = new Color[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    pixels[y * w + x] = pixel(x, y);
            tex.SetPixels(pixels);
            tex.Apply();

            string fullPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), path);
            File.WriteAllBytes(fullPath, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 100;
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        // Bentuk "pion": kepala bulat + badan setengah elips, dengan garis tepi gelap
        private static Color PawnPixel(int x, int y, Color body)
        {
            float cx = 128f;
            float head = Vector2.Distance(new Vector2(x, y), new Vector2(cx, 280f)) - 72f;
            float dx = (x - cx) / 105f, dy = (y - 10f) / 200f;
            float torso = y < 10 ? 1f : (Mathf.Sqrt(dx * dx + dy * dy) - 1f) * 100f;
            float d = Mathf.Min(head, torso);

            if (d > 1.5f) return Color.clear;
            float edge = Mathf.Clamp01(1.5f - d); // anti-alias tepi
            Color c = d > -7f ? Color.Lerp(body, Color.black, 0.45f) : body; // garis tepi
            // Highlight sederhana di kepala agar tidak terlalu datar
            if (head < -40f && x < cx - 10 && y > 300) c = Color.Lerp(c, Color.white, 0.25f);
            c.a = edge;
            return c;
        }

        private static Color RingPixel(int x, int y, int size, float inner, float outer)
        {
            float r = size * 0.5f;
            float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r)) / r;
            float a = Mathf.Clamp01((outer - d) * r) * Mathf.Clamp01((d - inner) * r);
            return new Color(1f, 1f, 1f, a);
        }

        private static Color GlowPixel(int x, int y, int size)
        {
            float r = size * 0.5f;
            float d = Vector2.Distance(new Vector2(x, y), new Vector2(r, r)) / r;
            float a = Mathf.Clamp01(1f - d);
            return new Color(1f, 1f, 1f, a * a);
        }
    }
}
