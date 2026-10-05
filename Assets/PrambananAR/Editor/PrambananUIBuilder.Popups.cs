using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PrambananAR.UI.EditorTools.UIBuilderKit;

namespace PrambananAR.UI.EditorTools
{
    /// <summary>Tata letak popup/overlay dan toast.</summary>
    public static partial class PrambananUIBuilder
    {
        // ==================================================================
        // MENU CEPAT (ref: menu daftar rata kanan di atas latar gradasi)
        // ==================================================================
        private static UIScreen BuildQuickMenu(Transform root)
        {
            var rt = Stretch(Rect("Popup_QuickMenu", root));
            rt.gameObject.AddComponent<CanvasGroup>();
            var popup = rt.gameObject.AddComponent<QuickMenuPopup>();
            SetInt(popup, "screenId", (int)UIScreenId.QuickMenu);
            AddImage(rt.gameObject, null, new Color(0.16f, 0.10f, 0.07f, 0.94f), true);

            var safe = Stretch(Rect("SafeArea", rt));
            safe.gameObject.AddComponent<SafeAreaFitter>();

            var list = Rect("Items", safe);
            Place(list, new Vector2(1, 0), new Vector2(-40, 260), new Vector2(620, 780));
            var vlg = list.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 30;
            vlg.childAlignment = TextAnchor.LowerRight;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            var items = new List<Object>();
            Button MenuItem(string label, Sprite icon, string glyph)
            {
                var item = Rect("Item_" + label, list);
                item.sizeDelta = new Vector2(620, 150);
                item.pivot = new Vector2(1, 0.5f); // animasi skala dari sisi kanan
                var hit = AddImage(item.gameObject, null, new Color(0, 0, 0, 0), true);
                var btn = item.gameObject.AddComponent<Button>();
                btn.targetGraphic = hit;
                item.gameObject.AddComponent<ButtonPressFeedback>();

                var text = Outlined(Text("Label", item, label.ToUpperInvariant(), 48, new Color(1f, 0.93f, 0.78f), TextAlignmentOptions.MidlineRight, true, true));
                Stretch(text.rectTransform, 0, 0, 170, 0);
                text.characterSpacing = 4;

                var circle = Img("IconBg", item, S(T.buttonCircle, Circle), T.primary);
                Place(circle.rectTransform, new Vector2(1, 0.5f), Vector2.zero, new Vector2(140, 140));
                var ic = Icon("Icon", circle.transform, icon, glyph, 90, T.primary);
                Place(ic, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(90, 90));
                items.Add(item);
                return btn;
            }

            var collection = MenuItem("Koleksi", T.iconCollection, "K");
            var story = MenuItem("Cerita", T.iconStory, "C");
            var guide = MenuItem("Panduan", T.iconGuide, "P");
            var ar = MenuItem("Kamera AR", T.iconCamera, "AR");

            var close = IconButton("CloseButton", safe, T.iconClose, "X", 150, Terracotta);
            Place((RectTransform)close.transform, new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(150, 150));

            Bind(popup, "panel", list);
            Bind(popup, "closeButton", close);
            Bind(popup, "collectionButton", collection);
            Bind(popup, "storyButton", story);
            Bind(popup, "guideButton", guide);
            Bind(popup, "arButton", ar);
            BindArray(popup, "items", items);
            Bind(popup, "guideDialogue", content.intro);
            return popup;
        }

        // ==================================================================
        // DI SEKITAR (ref: grid siluet "Nearby")
        // ==================================================================
        private static UIScreen BuildNearbyPanel(Transform root)
        {
            var popup = NewPopup<NearbyPanel>(root, "Popup_Nearby", UIScreenId.NearbyPanel, new Vector2(960, 1350), out var panel);
            var title = Text("Title", panel, "D I   S E K I T A R", 44, T.stone, TextAlignmentOptions.Center, true);
            TopBand(title.rectTransform, 90, 40);
            var tab = Text("Tab", panel, "TOKOH", 36, T.textDark, TextAlignmentOptions.Center, true);
            TopBand(tab.rectTransform, 60, 140, 300);
            var underline = Img("Underline", panel, Square, T.textDark);
            TopBand(underline.rectTransform, 6, 205, 330);

            var scroll = Scroll("Grid", panel, true, out var gridContent);
            Stretch((RectTransform)scroll.transform, 20, 240, 20, 40);
            var grid = gridContent.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(280, 340);
            grid.spacing = new Vector2(20, 24);
            grid.padding = new RectOffset(10, 10, 10, 10);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            var item = BuildCardTemplate(gridContent, true);

            Bind(popup, "characterDatabase", content.characters);
            Bind(popup, "zoneDatabase", content.zones);
            Bind(popup, "itemTemplate", item);
            return popup;
        }

        // ==================================================================
        // INFO ZONA
        // ==================================================================
        private static UIScreen BuildZoneInfoPopup(Transform root)
        {
            var popup = NewPopup<ZoneInfoPopup>(root, "Popup_ZoneInfo", UIScreenId.ZoneInfoPopup, new Vector2(900, 1150), out var panel);

            var badge = Img("Badge", panel, S(T.levelBadge, Circle), Tint(T.levelBadge, T.primary));
            Place(badge.rectTransform, new Vector2(0.5f, 1), new Vector2(0, 80), new Vector2(170, 170));
            var badgeText = Outlined(Text("Id", badge.transform, "A", 58, Color.white, TextAlignmentOptions.Center, true, true));

            var title = Text("Title", panel, "Nama Zona", 50, T.textDark, TextAlignmentOptions.Center, true, true);
            TopBand(title.rectTransform, 90, 100, 40);
            var status = Text("Status", panel, "Status", 34, Terracotta, TextAlignmentOptions.Center, true);
            TopBand(status.rectTransform, 60, 190, 40);

            var glow = Img("Glow", panel, S(T.glow, Circle), new Color(1f, 0.9f, 0.5f, 0.6f));
            Place(glow.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -260), new Vector2(480, 480));
            var character = Img("Character", panel, null, Color.white);
            Place(character.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -270), new Vector2(360, 460));
            character.preserveAspect = true;

            var desc = Text("Description", panel, "Deskripsi", 34, T.textDark);
            TopBand(desc.rectTransform, 200, 750, 60);

            var start = Button("StartButton", panel, T.buttonSuccess, T.success, "Buka Kamera AR", 44, Vector2.zero);
            Place((RectTransform)start.transform, new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(580, 140));

            Bind(popup, "badgeText", badgeText);
            Bind(popup, "titleText", title);
            Bind(popup, "descriptionText", desc);
            Bind(popup, "statusText", status);
            Bind(popup, "characterImage", character);
            Bind(popup, "startButton", start);
            Bind(popup, "startButtonLabel", LabelOf(start));
            return popup;
        }

        // ==================================================================
        // DETAIL TOKOH
        // ==================================================================
        private static UIScreen BuildCharacterDetailPopup(Transform root)
        {
            var popup = NewPopup<CharacterDetailPopup>(root, "Popup_CharacterDetail", UIScreenId.CharacterDetailPopup, new Vector2(920, 1420), out var panel);

            var glow = Img("Glow", panel, S(T.glow, Circle), new Color(1f, 0.9f, 0.5f, 0.6f));
            Place(glow.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(600, 600));
            var portrait = Img("Portrait", panel, null, Color.white);
            Place(portrait.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(440, 580));
            portrait.preserveAspect = true;

            var name = Text("Name", panel, "Nama", 56, T.textDark, TextAlignmentOptions.Center, true, true);
            TopBand(name.rectTransform, 90, 660, 40);
            var title = Text("TitleText", panel, "Gelar", 34, Terracotta, TextAlignmentOptions.Center, true);
            TopBand(title.rectTransform, 60, 750, 40);
            var zone = Text("Zone", panel, "Ditemukan di", 30, T.stone);
            TopBand(zone.rectTransform, 50, 815, 40);
            var desc = Text("Description", panel, "Deskripsi", 34, T.textDark);
            TopBand(desc.rectTransform, 300, 885, 60);

            var talk = Button("TalkButton", panel, T.buttonSecondary, Teal, "Ngobrol", 42, Vector2.zero);
            Place((RectTransform)talk.transform, new Vector2(0, 0), new Vector2(60, 50), new Vector2(380, 130));
            var storyBtn = Button("StoryButton", panel, T.buttonPrimary, T.primary, "Baca Cerita", 42, Vector2.zero);
            Place((RectTransform)storyBtn.transform, new Vector2(1, 0), new Vector2(-60, 50), new Vector2(380, 130));

            Bind(popup, "zoneDatabase", content.zones);
            Bind(popup, "portrait", portrait);
            Bind(popup, "nameText", name);
            Bind(popup, "titleText", title);
            Bind(popup, "descriptionText", desc);
            Bind(popup, "zoneText", zone);
            Bind(popup, "talkButton", talk);
            Bind(popup, "storyButton", storyBtn);
            return popup;
        }

        // ==================================================================
        // HADIAH (ref: ringkasan XP setelah menangkap)
        // ==================================================================
        private static UIScreen BuildRewardPopup(Transform root)
        {
            var popup = NewPopup<RewardPopup>(root, "Popup_Reward", UIScreenId.RewardPopup, new Vector2(900, 1150), out var panel, false);

            var character = Img("Character", panel, null, Color.white);
            Place(character.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(260, 340));
            character.preserveAspect = true;
            var title = Text("Title", panel, "Teman baru!", 44, T.textDark, TextAlignmentOptions.Center, true, true);
            TopBand(title.rectTransform, 80, 390, 40);

            var coinRow = Rect("CoinRow", panel);
            Place(coinRow, new Vector2(0.5f, 1), new Vector2(0, -480), new Vector2(360, 100));
            var coinIcon = Icon("CoinIcon", coinRow, T.iconCoin, "$", 84, T.gold);
            Place(coinIcon, new Vector2(0, 0.5f), Vector2.zero, new Vector2(84, 84));
            var coinText = Text("CoinText", coinRow, "+10", 60, T.textDark, TextAlignmentOptions.MidlineLeft, true);
            Stretch(coinText.rectTransform, 110, 0, 0, 0);

            var lines = Rect("Lines", panel);
            TopBand(lines, 232, 606, 70);
            var vlg = lines.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 4;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var line = Rect("LineTemplate", lines);
            line.sizeDelta = new Vector2(0, 52); // muat 4 baris (sapa, teman baru, nilai, benda tepat)
            var lineView = line.gameObject.AddComponent<RewardLineView>();
            var lineLabel = Text("Label", line, "LABEL", 28, T.stone, TextAlignmentOptions.MidlineLeft, true);
            var lineValue = Text("Value", line, "100 XP", 28, Terracotta, TextAlignmentOptions.MidlineRight, true);
            Bind(lineView, "labelText", lineLabel);
            Bind(lineView, "valueText", lineValue);

            var divider = Img("Divider", panel, Square, new Color(0.85f, 0.85f, 0.85f));
            TopBand(divider.rectTransform, 4, 850, 70);
            var total = Text("Total", panel, "TOTAL 0 XP", 52, T.textDark, TextAlignmentOptions.Center, true, true);
            TopBand(total.rectTransform, 90, 870, 40);

            var ok = Button("OKButton", panel, T.buttonSuccess, T.success, "OK", 48, Vector2.zero);
            Place((RectTransform)ok.transform, new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(520, 130));

            Bind(popup, "closeButton", ok); // OK = tutup popup -> memicu callback berikutnya
            Bind(popup, "characterImage", character);
            Bind(popup, "titleText", title);
            Bind(popup, "coinText", coinText);
            Bind(popup, "totalText", total);
            Bind(popup, "lineTemplate", lineView);
            return popup;
        }

        // ==================================================================
        // NAIK LEVEL (ref: lencana angka besar bercahaya)
        // ==================================================================
        private static UIScreen BuildLevelUpPopup(Transform root)
        {
            var popup = NewPopup<LevelUpPopup>(root, "Popup_LevelUp", UIScreenId.LevelUpPopup, new Vector2(900, 1100), out var panel, false);
            // Panel transparan: konten "melayang" di atas latar gelap
            var panelImg = panel.GetComponent<Image>();
            panelImg.color = new Color(0, 0, 0, 0);

            var title = Outlined(Text("Title", panel, "Naik Level!", 110, T.gold, TextAlignmentOptions.Center, false, true));
            TopBand(title.rectTransform, 160, 0);

            var badge = Rect("Badge", panel);
            Place(badge, new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(440, 440));
            badge.pivot = new Vector2(0.5f, 0.5f);
            var glow = Img("Glow", badge, S(T.glow, Circle), new Color(1f, 0.9f, 0.55f, 0.85f));
            Place(glow.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(800, 800));
            glow.gameObject.AddComponent<Rotator>();
            var outer = Img("Outer", badge, S(T.levelBadge, Circle), Tint(T.levelBadge, new Color(0.6f, 0.95f, 0.95f)));
            Stretch(outer.rectTransform);
            if (T.levelBadge == null)
            {
                var inner = Img("Inner", badge, Circle, Color.white);
                Stretch(inner.rectTransform, 45, 45, 45, 45);
            }
            var level = Outlined(Text("Level", badge, "2", 170, Color.white, TextAlignmentOptions.Center, true, true));

            var reward = Outlined(Text("Reward", panel, "Hadiah: +20 Koin", 46, T.gold, TextAlignmentOptions.Center, true, true));
            Place(reward.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 200), new Vector2(860, 80));

            var ok = Button("OKButton", panel, T.buttonPrimary, T.primary, "Hore!", 50, Vector2.zero);
            Place((RectTransform)ok.transform, new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(460, 140));

            Bind(popup, "closeButton", ok);
            Bind(popup, "badge", badge);
            Bind(popup, "levelText", level);
            Bind(popup, "rewardText", reward);
            return popup;
        }

        // ==================================================================
        // PERSETUJUAN ORANG TUA (ref: popup Ketentuan Layanan / Privasi)
        // ==================================================================
        private static UIScreen BuildConsentPopup(Transform root)
        {
            var popup = NewPopup<ConsentPopup>(root, "Popup_Consent", UIScreenId.ConsentPopup, new Vector2(920, 1180), out var panel, false);

            var title = Text("Title", panel, "Halo, Ayah & Bunda!", 54, Terracotta, TextAlignmentOptions.Center, true, true);
            TopBand(title.rectTransform, 90, 50, 40);

            var body = Text("Body", panel,
                "Aplikasi ini menggunakan <b>kamera</b> dan <b>lokasi (GPS)</b> agar tokoh legenda Roro Jonggrang " +
                "dapat muncul di area Candi Prambanan.\n\nData lokasi hanya diproses di perangkat dan tidak dikirim ke server. " +
                "Mohon dampingi anak selama bermain.", 34, T.textDark);
            TopBand(body.rectTransform, 460, 165, 60);

            var toggle = Toggle("AgreeToggle", panel, "Saya orang tua/pendamping dan menyetujui.", 30, new Vector2(800, 90));
            Place((RectTransform)toggle.transform, new Vector2(0.5f, 1), new Vector2(0, -650), new Vector2(800, 90));

            var accept = Button("AcceptButton", panel, T.buttonSuccess, T.success, "SETUJU", 48, Vector2.zero);
            Place((RectTransform)accept.transform, new Vector2(0.5f, 0), new Vector2(0, 170), new Vector2(640, 140));

            var privacy = Button("PrivacyButton", panel, null, new Color(0, 0, 0, 0), "KEBIJAKAN PRIVASI", 32, Vector2.zero, T.success);
            Place((RectTransform)privacy.transform, new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(520, 90));

            Bind(popup, "agreeToggle", toggle);
            Bind(popup, "acceptButton", accept);
            Bind(popup, "privacyButton", privacy);
            return popup;
        }

        // ==================================================================
        // TOAST (Canvas tersendiri agar selalu paling atas)
        // ==================================================================
        private static void BuildToast(Transform root)
        {
            var rt = Rect("Toast", root);
            Place(rt, new Vector2(0.5f, 0.5f), new Vector2(0, 250), new Vector2(900, 170));
            rt.pivot = new Vector2(0.5f, 0.5f);
            var canvas = rt.gameObject.AddComponent<Canvas>();
            canvas.overrideSorting = true;
            canvas.sortingOrder = 100;
            var group = rt.gameObject.AddComponent<CanvasGroup>();
            group.alpha = 0;
            group.blocksRaycasts = false;

            AddImage(rt.gameObject, S(T.chip, Rounded), new Color(0.16f, 0.10f, 0.07f, 0.88f));
            var label = Text("Label", rt, "Pesan", 40, Color.white, TextAlignmentOptions.Center, true);
            Stretch(label.rectTransform, 30, 10, 30, 10);

            var toast = rt.gameObject.AddComponent<ToastNotifier>();
            Bind(toast, "group", group);
            Bind(toast, "label", label);
        }
    }
}
