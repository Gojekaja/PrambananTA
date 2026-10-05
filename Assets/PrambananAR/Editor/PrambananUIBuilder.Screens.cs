using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PrambananAR.UI.EditorTools.UIBuilderKit;

namespace PrambananAR.UI.EditorTools
{
    /// <summary>Tata letak layar penuh (resolusi acuan 1080 x 1920, portrait).</summary>
    public static partial class PrambananUIBuilder
    {
        // Palet tema "Batu Candi & Emas"
        private static readonly Color Terracotta = new Color(0.85f, 0.41f, 0.23f);
        private static readonly Color Leaf = new Color(0.36f, 0.67f, 0.29f);
        private static readonly Color Teal = new Color(0.18f, 0.62f, 0.70f);
        private static readonly Color Cream = new Color(0.98f, 0.94f, 0.85f);
        private static readonly Color DarkGlass = new Color(0.12f, 0.08f, 0.05f, 0.72f);

        // ==================================================================
        // LOADING (ref: layar loading game kasual)
        // ==================================================================
        private static UIScreen BuildLoadingScreen(Transform root)
        {
            var bgColor = T.loadingBackground != null ? Color.white : new Color(0.98f, 0.66f, 0.38f);
            var screen = NewScreen<LoadingScreen>(root, "Screen_Loading", UIScreenId.Loading, bgColor, out var safe, T.loadingBackground);

            var glow = Img("Glow", safe, S(T.glow, Circle), new Color(1f, 0.9f, 0.6f, 0.35f));
            Place(glow.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -360), new Vector2(1100, 1100));
            glow.rectTransform.pivot = new Vector2(0.5f, 0.5f); // berputar di tengah, di belakang judul
            glow.gameObject.AddComponent<Rotator>();

            if (T.logo != null)
            {
                var logo = Img("Logo", safe, T.logo, Color.white);
                logo.preserveAspect = true;
                Place(logo.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -180), new Vector2(860, 440));
            }
            else
            {
                var title = Outlined(Text("Title", safe, "PETUALANGAN\n<color=#FFD54A>RORO JONGGRANG</color>", 100, Color.white, TextAlignmentOptions.Center, true, true));
                TopBand(title.rectTransform, 300, 200, 40);
                var sub = Outlined(Text("Subtitle", safe, "Legenda Candi Prambanan", 48, new Color(1f, 0.95f, 0.85f), TextAlignmentOptions.Center, true, true));
                TopBand(sub.rectTransform, 80, 500, 40);
            }

            var tip = Text("Tip", safe, "Tips", 38, Color.white, TextAlignmentOptions.Bottom);
            BottomBand(tip.rectTransform, 170, 170, 70);

            var barBg = Img("ProgressBar", safe, S(T.progressBarBg, Rounded), Tint(T.progressBarBg, new Color(0, 0, 0, 0.45f)));
            BottomBand(barBg.rectTransform, 72, 70, 60);
            var fill = Filled("Fill", barBg.transform, T.progressBarFill, T.primary, Image.FillMethod.Horizontal);
            Stretch(fill.rectTransform, 10, 10, 10, 10);
            var pct = Outlined(Text("Percent", barBg.transform, "0%", 40, Color.white, TextAlignmentOptions.Center, true, true));

            Bind(screen, "progressFill", fill);
            Bind(screen, "percentText", pct);
            Bind(screen, "tipText", tip);
            Bind(screen, "introDialogue", content.intro);
            return screen;
        }

        // ==================================================================
        // PETA / MENU UTAMA (ref: peta dunia game lokasi + bar atas game kasual)
        // ==================================================================
        private static UIScreen BuildMapScreen(Transform root)
        {
            var screen = NewScreen<MapScreen>(root, "Screen_Map", UIScreenId.Map, T.mapBackground, out var safe);
            var screenRt = (RectTransform)screen.transform;

            // --- Peta yang bisa digeser (di luar SafeArea agar memenuhi layar) ---
            var scroll = Scroll("MapScroll", screenRt, false, out var mapContent);
            Stretch((RectTransform)scroll.transform);
            scroll.transform.SetSiblingIndex(0);
            scroll.horizontal = true;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.decelerationRate = 0.1f;

            // Rasio gambar peta kira-kira 692 x 781 (sama dengan peta brosur)
            Place(mapContent, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(2000, 2257));
            mapContent.pivot = new Vector2(0.5f, 0.5f);
            var mapSprite = content.zones.mapSprite;
            var mapImg = AddImage(mapContent.gameObject, mapSprite, mapSprite != null ? Color.white : new Color(0.62f, 0.82f, 0.5f), true);
            if (mapSprite == null)
            {
                var hint = Text("PlaceholderHint", mapContent, "Isi ZoneDatabase > Map Sprite\ndengan gambar peta Prambanan", 44, new Color(0, 0, 0, 0.35f));
                hint.rectTransform.anchoredPosition = new Vector2(0, -700);
            }

            var pins = Stretch(Rect("Pins", mapContent));
            var pin = BuildPinTemplate(pins);
            var player = BuildPlayerMarker(pins);

            // --- Bar atas (HUD) ---
            var hud = Img("TopHUD", safe, S(T.topBar, Rounded), Tint(T.topBar, T.panelDark));
            TopBand(hud.rectTransform, 150, 20, 24);
            var hudComp = hud.gameObject.AddComponent<PlayerHUD>();

            var badge = Img("LevelBadge", hud.transform, S(T.levelBadge, Circle), Tint(T.levelBadge, T.secondary));
            Place(badge.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0), new Vector2(150, 150));
            var levelText = Outlined(Text("Level", badge.transform, "1", 54, Color.white, TextAlignmentOptions.Center, true, true));

            var nameText = Outlined(Text("PlayerName", hud.transform, "Penjelajah Cilik", 32, Color.white, TextAlignmentOptions.MidlineLeft, true, true));
            Place(nameText.rectTransform, new Vector2(0, 0.5f), new Vector2(160, 26), new Vector2(380, 50));

            var xpBg = Img("XPBar", hud.transform, S(T.progressBarBg, Rounded), Tint(T.progressBarBg, new Color(0, 0, 0, 0.5f)));
            Place(xpBg.rectTransform, new Vector2(0, 0.5f), new Vector2(160, -30), new Vector2(380, 44));
            var xpFill = Filled("Fill", xpBg.transform, T.progressBarFill, new Color(0.40f, 0.80f, 1f), Image.FillMethod.Horizontal);
            Stretch(xpFill.rectTransform, 6, 6, 6, 6);
            var xpText = Outlined(Text("XPText", xpBg.transform, "0/500", 24, Color.white, TextAlignmentOptions.Center, true, true));

            var coinPill = Img("Coins", hud.transform, S(T.chip, Rounded), new Color(0, 0, 0, 0.35f));
            Place(coinPill.rectTransform, new Vector2(1, 0.5f), new Vector2(-20, 0), new Vector2(300, 96));
            var coinIcon = Icon("CoinIcon", coinPill.transform, T.iconCoin, "$", 76, T.gold);
            Place(coinIcon, new Vector2(0, 0.5f), new Vector2(10, 0), new Vector2(76, 76));
            var coinText = Outlined(Text("CoinText", coinPill.transform, "0", 44, T.gold, TextAlignmentOptions.MidlineRight, true, true));
            Stretch(coinText.rectTransform, 96, 0, 20, 0);

            Bind(hudComp, "levelText", levelText);
            Bind(hudComp, "xpFill", xpFill);
            Bind(hudComp, "xpText", xpText);
            Bind(hudComp, "coinText", coinText);
            Bind(hudComp, "playerNameText", nameText);

            // --- Banner misi (pita terakota) ---
            var quest = Button("QuestBanner", safe, T.panelHeader, Terracotta, "Misi", 32, Vector2.zero);
            TopBand((RectTransform)quest.transform, 96, 186, 90);

            // --- Bar bawah ---
            var menuBtn = IconButton("MenuButton", safe, T.iconMenu, "MENU", 220, T.primary);
            Place((RectTransform)menuBtn.transform, new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(220, 220));

            var collectionBtn = CaptionedIcon("CollectionButton", safe, T.iconCollection, "K", "Koleksi", 150, Teal, new Vector2(0, 0), new Vector2(30, 50));
            var nearbyBtn = CaptionedIcon("NearbyButton", safe, T.iconNearby, "?", "Sekitar", 150, Leaf, new Vector2(1, 0), new Vector2(-30, 50));
            var arBtn = CaptionedIcon("ARButton", safe, T.iconCamera, "AR", "Kamera", 130, Terracotta, new Vector2(1, 0), new Vector2(-40, 300));

            Bind(screen, "zoneDatabase", content.zones);
            Bind(screen, "characterDatabase", content.characters);
            Bind(screen, "mapScroll", scroll);
            Bind(screen, "mapContent", mapContent);
            Bind(screen, "mapImage", mapImg);
            Bind(screen, "pinTemplate", pin);
            Bind(screen, "playerMarker", player);
            Bind(screen, "menuButton", menuBtn);
            Bind(screen, "collectionButton", collectionBtn);
            Bind(screen, "nearbyButton", nearbyBtn);
            Bind(screen, "arButton", arBtn);
            Bind(screen, "questBannerButton", quest);
            Bind(screen, "questText", LabelOf(quest));
            return screen;
        }

        private static MapZonePin BuildPinTemplate(Transform parent)
        {
            var rt = Rect("PinTemplate", parent);
            rt.pivot = new Vector2(0.5f, 0f); // ujung bawah pin = titik lokasi
            rt.sizeDelta = new Vector2(150, 200);
            var view = rt.gameObject.AddComponent<MapZonePin>();

            // Ekor pin (diwarnai sama dengan kepala pin)
            Image tail;
            if (T.mapPinTail != null)
            {
                tail = Img("Tail", rt, T.mapPinTail, Color.white);
                Place(tail.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(76, 76));
            }
            else
            {
                tail = Img("Tail", rt, Square, Color.white);
                Place(tail.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(56, 56));
                tail.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                tail.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
            }

            var head = Rect("Head", rt);
            Place(head, new Vector2(0.5f, 1), Vector2.zero, new Vector2(150, 150));
            head.pivot = new Vector2(0.5f, 0.5f); // pivot tengah agar denyut tidak bergeser
            head.anchoredPosition = new Vector2(0, -75);
            head.gameObject.AddComponent<PulseScale>();

            var pinImg = Img("PinImage", head, S(T.mapPin, Circle), Color.white, true);
            Stretch(pinImg.rectTransform);
            var inner = Img("Inner", head, Circle, new Color(1f, 0.97f, 0.9f, 0.97f));
            Stretch(inner.rectTransform, 16, 16, 16, 16);
            var charImg = Img("Character", head, null, Color.white);
            Stretch(charImg.rectTransform, 26, 14, 26, 18);
            charImg.preserveAspect = true;

            var idBadge = Img("IdBadge", head, Circle, T.panelDark);
            Place(idBadge.rectTransform, new Vector2(0, 1), new Vector2(-6, 6), new Vector2(58, 58));
            var idText = Outlined(Text("Id", idBadge.transform, "A", 28, Color.white, TextAlignmentOptions.Center, true, true));

            var found = Icon("FoundBadge", head, T.iconCheck, "v", 56, T.success);
            Place(found, new Vector2(1, 1), new Vector2(6, 6), new Vector2(56, 56));

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = pinImg;
            btn.transition = Selectable.Transition.None;

            Bind(view, "button", btn);
            Bind(view, "pinImage", pinImg);
            Bind(view, "tailImage", tail);
            Bind(view, "characterImage", charImg);
            Bind(view, "label", idText);
            Bind(view, "foundBadge", found.gameObject);
            return view;
        }

        private static RectTransform BuildPlayerMarker(Transform parent)
        {
            var rt = Rect("PlayerMarker", parent);
            rt.sizeDelta = new Vector2(110, 110);

            var range = Img("Range", rt, S(T.ring, Circle), new Color(0.55f, 0.85f, 1f, 0.6f));
            Place(range.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(300, 300));
            range.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            range.gameObject.AddComponent<PulseScale>();

            var dot = Img("Dot", rt, S(T.playerMarker, Circle), T.secondary);
            Stretch(dot.rectTransform);
            var core = Img("Core", rt, Circle, Color.white);
            Stretch(core.rectTransform, 34, 34, 34, 34);

            var label = Img("Label", rt, S(T.chip, Rounded), T.panelDark);
            Place(label.rectTransform, new Vector2(0.5f, 1), new Vector2(0, 70), new Vector2(150, 58));
            label.rectTransform.pivot = new Vector2(0.5f, 0);
            Outlined(Text("Text", label.transform, "Kamu", 30, Color.white, TextAlignmentOptions.Center, true, true));
            return rt;
        }

        // ==================================================================
        // KAMERA AR (geofence GPS + tokoh AR + tas benda)
        // ==================================================================
        private static UIScreen BuildARScreen(Transform root, ScreenshotService screenshot)
        {
            // Tanpa latar: transparan agar feed kamera AR terlihat
            var screen = NewScreen<ARCameraScreen>(root, "Screen_ARCamera", UIScreenId.ARCamera, null, out var safe);

            // --- Grup memindai (status GPS / jarak ke zona) ---
            var scanning = Stretch(Rect("ScanningGroup", safe));
            var reticle = Img("Reticle", scanning, S(T.ring, Circle), new Color(1, 1, 1, 0.85f));
            Place(reticle.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(420, 420));
            reticle.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            reticle.gameObject.AddComponent<PulseScale>();
            var scanPanel = Img("ScanPanel", scanning, S(T.chip, Rounded), DarkGlass);
            Place(scanPanel.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -340), new Vector2(940, 170));
            scanPanel.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            var scanText = Text("Text", scanPanel.transform, "Arahkan kamera ke sekitarmu...", 36, Color.white);
            Stretch(scanText.rectTransform, 40, 12, 40, 12);

            // --- Grup pertemuan ---
            var encounter = Stretch(Rect("EncounterGroup", safe));
            var gesture = Img("GestureArea", encounter, null, new Color(0, 0, 0, 0), true);
            Stretch(gesture.rectTransform);
            var swipe = gesture.gameObject.AddComponent<SwipeGestureArea>();

            var preview = Img("PreviewCharacter", encounter, null, Color.white);
            Place(preview.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -40), new Vector2(440, 660));
            preview.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            preview.preserveAspect = true;

            var outerRing = Img("OuterRing", encounter, S(T.ring, Circle), new Color(1, 1, 1, 0.7f));
            Place(outerRing.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(460, 460));
            outerRing.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            var ringImg = Img("TargetRing", encounter, S(T.ring, Circle), T.success);
            Place(ringImg.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 40), new Vector2(440, 440));
            ringImg.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            var ring = ringImg.gameObject.AddComponent<ShrinkingRing>();
            Bind(ring, "ringImage", ringImg);

            // Banner pertemuan: kotak dialog bergaya tiket emas
            var banner = Rect("EncounterBanner", encounter);
            Place(banner, new Vector2(0.5f, 1), new Vector2(0, -270), new Vector2(960, 250));
            var bannerBg = Img("Bg", banner, S(T.dialogBox, Rounded), Tint(T.dialogBox, Color.white));
            Stretch(bannerBg.rectTransform);
            var encounterText = Text("Text", banner, "Wah! Kamu bertemu ...", 38, T.textDark);
            Stretch(encounterText.rectTransform, 44, 66, 44, 26);
            encounterText.enableAutoSizing = true;
            encounterText.fontSizeMin = 24;
            encounterText.fontSizeMax = 38;

            var hint = Stretch(Rect("SwipeHint", encounter));
            var hintAnim = hint.gameObject.AddComponent<SwipeHintAnimator>();
            var hand = Icon("Hand", hint, T.handPointer, "^", 150, new Color(1, 1, 1, 0.8f));
            Place(hand, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(150, 150));
            hand.pivot = new Vector2(0.5f, 0.5f);
            var handGroup = hand.gameObject.AddComponent<CanvasGroup>();
            handGroup.blocksRaycasts = false;
            var hintText = Outlined(Text("HintText", hint, "Usap ke atas untuk menyapa!", 38, Color.white, TextAlignmentOptions.Center, true, true));
            BottomBand(hintText.rectTransform, 90, 470, 40);
            Bind(hintAnim, "hand", hand);
            Bind(hintAnim, "handGroup", handGroup);

            // --- Bar atas ---
            var close = IconButton("CloseButton", safe, T.iconClose, "X", 120, Terracotta);
            Place((RectTransform)close.transform, new Vector2(0, 1), new Vector2(30, -30), new Vector2(120, 120));
            var help = IconButton("HelpButton", safe, T.iconHelp, "?", 120, Teal);
            Place((RectTransform)help.transform, new Vector2(0, 1), new Vector2(170, -30), new Vector2(120, 120));
            var reset = Button("ResetButton", safe, T.buttonPrimary, T.primary, "Ulangi", 40, Vector2.zero);
            Place((RectTransform)reset.transform, new Vector2(1, 1), new Vector2(-30, -40), new Vector2(260, 100));

            var safety = Img("SafetyBanner", safe, S(T.chip, Rounded), DarkGlass);
            TopBand(safety.rectTransform, 76, 175, 100);
            Text("Text", safety.transform, "Selalu didampingi orang dewasa & perhatikan sekitarmu!", 28, Color.white);

            // --- Tas benda (muncul saat tokoh menginginkan sesuatu) ---
            var tray = Img("ItemTray", safe, S(T.panelCard, Rounded), Tint(T.panelCard, Color.white), true);
            BottomBand(tray.rectTransform, 240, 215, 24);
            var trayTitle = Img("Title", tray.transform, S(T.chip, Rounded), Terracotta);
            Place(trayTitle.rectTransform, new Vector2(0, 1), new Vector2(30, 30), new Vector2(250, 60));
            Outlined(Text("Label", trayTitle.transform, "TAS BENDA", 28, Color.white, TextAlignmentOptions.Center, true, true));
            var list = HorizontalList("Items", tray.transform, 14, new RectOffset(16, 16, 0, 0), out var listContent);
            Stretch((RectTransform)list.transform, 12, 36, 12, 12);
            var slot = BuildItemSlotTemplate(listContent);

            // --- Kontrol bawah ---
            var capture = Button("CaptureButton", safe, T.buttonPrimary, T.primary, "Ambil Foto", 44, Vector2.zero);
            Place((RectTransform)capture.transform, new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(440, 150));

            // --- Benda melayang, teks nilai & kilat foto (paling atas) ---
            var flying = Img("FlyingItem", safe, null, Color.white);
            Place(flying.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(170, 170));
            flying.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            flying.preserveAspect = true;

            var grade = Outlined(Text("GradeText", safe, "Hebat!", 110, T.gold, TextAlignmentOptions.Center, true, true));
            Place(grade.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 330), new Vector2(900, 170));
            grade.rectTransform.pivot = new Vector2(0.5f, 0.5f);

            var flash = Img("Flash", screen.transform, null, Color.white);
            Stretch(flash.rectTransform);
            var flashGroup = flash.gameObject.AddComponent<CanvasGroup>();
            flashGroup.alpha = 0;
            flashGroup.blocksRaycasts = false;

            Bind(screen, "characterDatabase", content.characters);
            Bind(screen, "zoneDatabase", content.zones);
            Bind(screen, "itemDatabase", content.items);
            Bind(screen, "screenshotService", screenshot);
            Bind(screen, "closeButton", close);
            Bind(screen, "helpButton", help);
            Bind(screen, "resetButton", reset);
            Bind(screen, "captureButton", capture);
            Bind(screen, "scanningGroup", scanning.gameObject);
            Bind(screen, "encounterGroup", encounter.gameObject);
            Bind(screen, "scanText", scanText);
            Bind(screen, "encounterText", encounterText);
            Bind(screen, "gradeText", grade);
            Bind(screen, "previewCharacterImage", preview);
            Bind(screen, "ring", ring);
            Bind(screen, "gestureArea", swipe);
            Bind(screen, "swipeHint", hint.gameObject);
            Bind(screen, "swipeHintText", hintText);
            Bind(screen, "flashOverlay", flashGroup);
            Bind(screen, "itemTray", tray.gameObject);
            Bind(screen, "itemSlotTemplate", slot);
            Bind(screen, "flyingItem", flying);

            tray.gameObject.SetActive(false);
            flying.gameObject.SetActive(false);
            grade.gameObject.SetActive(false);
            encounter.gameObject.SetActive(false);
            return screen;
        }

        private static ItemSlotView BuildItemSlotTemplate(Transform parent)
        {
            var rt = Rect("ItemSlotTemplate", parent);
            rt.sizeDelta = new Vector2(128, 172);
            var view = rt.gameObject.AddComponent<ItemSlotView>();

            // Cahaya emas di belakang benda yang dipilih
            var selected = Img("Selected", rt, S(T.glow, Circle), new Color(1f, 0.8f, 0.2f, 1f));
            Place(selected.rectTransform, new Vector2(0.5f, 1), new Vector2(0, 32), new Vector2(196, 196));
            var card = Img("Card", rt, S(T.buttonCircle, Circle), new Color(1f, 0.96f, 0.86f), true);
            Place(card.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -4), new Vector2(120, 120));
            var icon = Img("Icon", card.transform, null, Color.white);
            Stretch(icon.rectTransform, 16, 14, 16, 18);
            icon.preserveAspect = true;
            var name = Outlined(Text("Name", rt, "Benda", 22, Color.white, TextAlignmentOptions.Center, true, true));
            BottomBand(name.rectTransform, 36, 2);

            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = card;
            rt.gameObject.AddComponent<ButtonPressFeedback>();

            Bind(view, "button", btn);
            Bind(view, "icon", icon);
            Bind(view, "nameText", name);
            Bind(view, "selectedFrame", selected.gameObject);
            selected.gameObject.SetActive(false);
            return view;
        }

        // ==================================================================
        // KOLEKSI (ref: progres koleksi regional + grid kartu)
        // ==================================================================
        private static UIScreen BuildCollectionScreen(Transform root)
        {
            var screen = NewScreen<CollectionScreen>(root, "Screen_Collection", UIScreenId.Collection, Cream, out var safe);

            var back = IconButton("BackButton", safe, T.iconBack, "<", 110, Teal);
            Place((RectTransform)back.transform, new Vector2(0, 1), new Vector2(30, -30), new Vector2(110, 110));
            var title = Text("Title", safe, "Koleksi Tokoh", 64, T.textDark, TextAlignmentOptions.Center, true, true);
            TopBand(title.rectTransform, 100, 35, 150);
            var subtitle = Text("Subtitle", safe, "Temukan tokoh legenda", 32, T.stone);
            TopBand(subtitle.rectTransform, 90, 135, 90);

            // Cincin progres
            var ringRoot = Rect("ProgressRing", safe);
            Place(ringRoot, new Vector2(0.5f, 1), new Vector2(0, -240), new Vector2(360, 360));
            var ringBg = Img("Track", ringRoot, S(T.ring, Circle), new Color(0.88f, 0.80f, 0.66f));
            Stretch(ringBg.rectTransform);
            var ringFill = Filled("Fill", ringRoot, S(T.ring, Circle), T.primary, Image.FillMethod.Radial360, (int)Image.Origin360.Top);
            Stretch(ringFill.rectTransform);
            ringFill.fillClockwise = true;
            var innerCircle = Img("Inner", ringRoot, Circle, Color.white);
            Stretch(innerCircle.rectTransform, 45, 45, 45, 45);
            var progressText = Text("Count", ringRoot, "0/7", 96, Terracotta, TextAlignmentOptions.Center, true, true);
            Stretch(progressText.rectTransform, 0, 0, 0, 40);
            var caption = Text("Caption", ringRoot, "teman", 30, T.stone);
            Stretch(caption.rectTransform, 0, 220, 0, 70);

            // Grid kartu
            var scroll = Scroll("CardScroll", safe, true, out var gridContent);
            Stretch((RectTransform)scroll.transform, 0, 630, 0, 340);
            var grid = gridContent.gameObject.AddComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(300, 380);
            grid.spacing = new Vector2(30, 30);
            grid.padding = new RectOffset(30, 30, 20, 20);
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 3;
            var card = BuildCardTemplate(gridContent, false);

            // Baris hadiah koleksi
            var rewardTitle = Text("RewardsTitle", safe, "HADIAH KOLEKSI", 32, T.stone, TextAlignmentOptions.Center, true, true);
            BottomBand(rewardTitle.rectTransform, 50, 270);
            var row = Rect("Milestones", safe);
            BottomBand(row, 230, 30, 30);
            var hlg = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 20;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;
            var milestone = BuildMilestoneTemplate(row);

            Bind(screen, "database", content.characters);
            Bind(screen, "zoneDatabase", content.zones);
            Bind(screen, "backButton", back);
            Bind(screen, "progressRing", ringFill);
            Bind(screen, "progressText", progressText);
            Bind(screen, "subtitleText", subtitle);
            Bind(screen, "cardTemplate", card);
            Bind(screen, "milestoneTemplate", milestone);
            return screen;
        }

        /// <summary>Template kartu tokoh. round = true untuk gaya bingkai bulat (panel Di Sekitar).</summary>
        private static CharacterCardView BuildCardTemplate(Transform parent, bool round)
        {
            var rt = Rect("CardTemplate", parent);
            rt.sizeDelta = new Vector2(300, 380);
            var bg = AddImage(rt.gameObject, round ? null : S(T.panelCard, Rounded), round ? new Color(0, 0, 0, 0) : Color.white, true);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            rt.gameObject.AddComponent<ButtonPressFeedback>();
            var view = rt.gameObject.AddComponent<CharacterCardView>();

            Image portrait;
            if (round)
            {
                var frame = Img("Frame", rt, S(T.buttonCircle, Circle), T.primary);
                Place(frame.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -5), new Vector2(230, 230));
                var inner = Img("Inner", frame.transform, Circle, Cream);
                Stretch(inner.rectTransform, 16, 16, 16, 16);
                portrait = Img("Portrait", frame.transform, null, Color.white);
                Stretch(portrait.rectTransform, 34, 22, 34, 22);
            }
            else
            {
                portrait = Img("Portrait", rt, null, Color.white);
                Place(portrait.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(220, 250));
            }
            portrait.preserveAspect = true;

            var name = Text("Name", rt, "???", 30, T.textDark, TextAlignmentOptions.Center, true, true);
            Place(name.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 58), new Vector2(290, 50));
            var sub = Text("Subtitle", rt, "Zona", 24, T.stone);
            Place(sub.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 18), new Vector2(290, 40));

            var lockIcon = Icon("LockIcon", rt, T.iconLock, "?", 64, T.stone);
            Place(lockIcon, new Vector2(1, 1), new Vector2(-12, -12), new Vector2(64, 64));
            var found = Icon("FoundBadge", rt, T.iconCheck, "v", 64, T.success);
            Place(found, new Vector2(1, 1), new Vector2(-12, -12), new Vector2(64, 64));

            Bind(view, "button", btn);
            Bind(view, "portrait", portrait);
            Bind(view, "nameText", name);
            Bind(view, "subtitleText", sub);
            Bind(view, "lockIcon", lockIcon.gameObject);
            Bind(view, "foundBadge", found.gameObject);
            SetBool(view, "preferIcon", round);
            return view;
        }

        private static MilestoneView BuildMilestoneTemplate(Transform parent)
        {
            var rt = Rect("MilestoneTemplate", parent);
            AddImage(rt.gameObject, S(T.panelCard, Rounded), Color.white);
            var view = rt.gameObject.AddComponent<MilestoneView>();

            var icon = Img("Icon", rt, S(T.iconStar, Circle), Color.white);
            Place(icon.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -14), new Vector2(110, 110));
            icon.preserveAspect = true;
            if (T.iconStar == null) icon.color = T.gold;

            var lockOverlay = Icon("Lock", rt, T.iconLock, "?", 56, T.secondary);
            Place(lockOverlay, new Vector2(0.5f, 1), new Vector2(46, -80), new Vector2(56, 56));

            var name = Text("Name", rt, "Hadiah", 26, T.textDark, TextAlignmentOptions.Center, true, true);
            Place(name.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(300, 44));
            var req = Text("Requirement", rt, "2 teman", 24, T.stone);
            Place(req.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 12), new Vector2(300, 40));

            Bind(view, "icon", icon);
            Bind(view, "nameText", name);
            Bind(view, "requirementText", req);
            Bind(view, "lockOverlay", lockOverlay.gameObject);
            return view;
        }

        // ==================================================================
        // CERITA (ref: aplikasi buku cerita anak: sampul + pembaca)
        // ==================================================================
        private static UIScreen BuildStoryScreen(Transform root)
        {
            var screen = NewScreen<StoryScreen>(root, "Screen_Story", UIScreenId.Story, Cream, out var safe);
            var audio = screen.gameObject.AddComponent<AudioSource>();
            audio.playOnAwake = false;

            // ---------------- Sampul ----------------
            var cover = Stretch(Rect("CoverView", safe));
            var back = IconButton("BackButton", cover, T.iconBack, "<", 110, Teal);
            Place((RectTransform)back.transform, new Vector2(0, 1), new Vector2(30, -30), new Vector2(110, 110));
            var header = Text("Header", cover, "Buku Cerita", 52, T.textDark, TextAlignmentOptions.Center, true, true);
            TopBand(header.rectTransform, 100, 35, 150);

            var coverSprite = content.story.cover;
            var coverImg = Img("Cover", cover, S(coverSprite, Rounded), coverSprite != null ? Color.white : new Color(0.85f, 0.72f, 0.55f));
            Place(coverImg.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -170), new Vector2(620, 620));
            coverImg.preserveAspect = coverSprite != null;

            var title = Text("Title", cover, "Judul", 60, T.textDark, TextAlignmentOptions.Center, true, true);
            TopBand(title.rectTransform, 90, 815, 50);
            var subtitle = Text("Subtitle", cover, "Subjudul", 34, T.stone);
            TopBand(subtitle.rectTransform, 60, 905, 50);

            var progBg = Img("Progress", cover, S(T.progressBarBg, Rounded), Tint(T.progressBarBg, new Color(0.85f, 0.85f, 0.85f)));
            TopBand(progBg.rectTransform, 30, 990, 140);
            var progFill = Filled("Fill", progBg.transform, T.progressBarFill, T.primary, Image.FillMethod.Horizontal);
            Stretch(progFill.rectTransform, 5, 5, 5, 5);
            var progText = Text("ProgressText", cover, "0/6 halaman dibaca", 28, T.stone);
            TopBand(progText.rectTransform, 50, 1025, 50);

            var read = Button("ReadButton", cover, T.buttonPrimary, T.primary, "Baca Cerita", 48, Vector2.zero);
            Place((RectTransform)read.transform, new Vector2(0.5f, 1), new Vector2(0, -1100), new Vector2(460, 150));

            var tags = Rect("Tags", cover);
            TopBand(tags, 80, 1275, 80);
            var tagLayout = tags.gameObject.AddComponent<HorizontalLayoutGroup>();
            tagLayout.spacing = 20;
            tagLayout.childAlignment = TextAnchor.MiddleCenter;
            tagLayout.childControlWidth = false;
            tagLayout.childControlHeight = false;
            tagLayout.childForceExpandWidth = false;
            var tag = Img("TagTemplate", tags, S(T.chip, Rounded), new Color(0.95f, 0.85f, 0.66f));
            tag.rectTransform.sizeDelta = new Vector2(240, 70);
            Text("Label", tag.transform, "Tag", 30, T.textDark, TextAlignmentOptions.Center, true, true);

            var synopsis = Text("Synopsis", cover, "Sinopsis", 34, T.textDark, TextAlignmentOptions.TopLeft);
            TopBand(synopsis.rectTransform, 400, 1380, 70);

            // ---------------- Pembaca ----------------
            var reader = Stretch(Rect("ReaderView", safe));
            var illuBg = Img("IllustrationArea", reader, null, new Color(0.24f, 0.16f, 0.12f));
            TopBand(illuBg.rectTransform, 820);
            var illu = Img("Illustration", illuBg.transform, null, Color.white);
            Stretch(illu.rectTransform);
            illu.preserveAspect = true;

            var readerBack = IconButton("BackButton", reader, T.iconBack, "<", 110, Teal);
            Place((RectTransform)readerBack.transform, new Vector2(0, 1), new Vector2(30, -30), new Vector2(110, 110));

            var seg = Img("ModeToggle", reader, S(T.chip, Rounded), DarkGlass);
            Place(seg.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(380, 96));
            var audioBtn = SegmentButton("AudioMode", seg.transform, "Audio", 0f, 0.5f, out var audioBg);
            var textBtn = SegmentButton("TextMode", seg.transform, "Teks", 0.5f, 1f, out var textBg);

            var sheet = Img("Sheet", reader, S(T.panelPopup, Rounded), Tint(T.panelPopup, Color.white));
            Stretch(sheet.rectTransform, 0, 760, 0, -30); // sedikit melewati bawah agar sudut bawah tersembunyi
            var chapter = Text("Chapter", sheet.transform, "Bab 1", 44, Terracotta, TextAlignmentOptions.MidlineLeft, true, true);
            TopBand(chapter.rectTransform, 80, 50, 60);
            var body = Text("Body", sheet.transform, "Isi cerita", 46, T.textDark, TextAlignmentOptions.TopLeft);
            Stretch(body.rectTransform, 60, 150, 60, 300);
            body.overflowMode = TextOverflowModes.Overflow;
            body.lineSpacing = 12;
            body.richText = true;

            var pageText = Text("Page", sheet.transform, "1 / 6", 30, T.stone, TextAlignmentOptions.MidlineRight);
            Place(pageText.rectTransform, new Vector2(1, 0), new Vector2(-60, 250), new Vector2(240, 50));

            var controls = Rect("Controls", sheet.transform);
            BottomBand(controls, 200, 60, 30);
            var minus = IconButton("FontMinus", controls, null, "A-", 110, T.stone);
            Place((RectTransform)minus.transform, new Vector2(0, 0.5f), new Vector2(10, 0), new Vector2(110, 110));
            var fontSizeText = Text("FontSize", controls, "46", 40, T.textDark, TextAlignmentOptions.Center, true, true);
            Place(fontSizeText.rectTransform, new Vector2(0, 0.5f), new Vector2(130, 0), new Vector2(100, 80));
            var plus = IconButton("FontPlus", controls, null, "A+", 110, T.stone);
            Place((RectTransform)plus.transform, new Vector2(0, 0.5f), new Vector2(240, 0), new Vector2(110, 110));

            var play = Button("PlayPause", controls, S(T.buttonCircle, Circle), Leaf, "Putar", 36, Vector2.zero);
            Place((RectTransform)play.transform, new Vector2(0.5f, 0.5f), new Vector2(60, 0), new Vector2(180, 180));
            ((RectTransform)play.transform).pivot = new Vector2(0.5f, 0.5f);

            var prev = IconButton("PrevButton", controls, T.iconPrev, "<", 110, Teal);
            Place((RectTransform)prev.transform, new Vector2(1, 0.5f), new Vector2(-140, 0), new Vector2(110, 110));
            var next = IconButton("NextButton", controls, T.iconNext, ">", 110, Teal);
            Place((RectTransform)next.transform, new Vector2(1, 0.5f), new Vector2(-10, 0), new Vector2(110, 110));

            Bind(screen, "story", content.story);
            Bind(screen, "audioSource", audio);
            Bind(screen, "theme", T);
            Bind(screen, "coverView", cover.gameObject);
            Bind(screen, "backButton", back);
            Bind(screen, "coverImage", coverImg);
            Bind(screen, "titleText", title);
            Bind(screen, "subtitleText", subtitle);
            Bind(screen, "synopsisText", synopsis);
            Bind(screen, "progressText", progText);
            Bind(screen, "progressFill", progFill);
            Bind(screen, "readButton", read);
            Bind(screen, "tagTemplate", tag.gameObject);
            Bind(screen, "readerView", reader.gameObject);
            Bind(screen, "readerBackButton", readerBack);
            Bind(screen, "illustration", illu);
            Bind(screen, "chapterText", chapter);
            Bind(screen, "bodyText", body);
            Bind(screen, "pageText", pageText);
            Bind(screen, "prevButton", prev);
            Bind(screen, "nextButton", next);
            Bind(screen, "playPauseButton", play);
            Bind(screen, "playPauseLabel", LabelOf(play));
            Bind(screen, "audioModeButton", audioBtn);
            Bind(screen, "textModeButton", textBtn);
            Bind(screen, "audioModeBg", audioBg);
            Bind(screen, "textModeBg", textBg);
            Bind(screen, "fontMinusButton", minus);
            Bind(screen, "fontPlusButton", plus);
            Bind(screen, "fontSizeText", fontSizeText);

            reader.gameObject.SetActive(false);
            return screen;
        }

        private static Button SegmentButton(string name, Transform parent, string label, float xMin, float xMax, out Image bg)
        {
            var rt = Rect(name, parent);
            rt.anchorMin = new Vector2(xMin, 0);
            rt.anchorMax = new Vector2(xMax, 1);
            rt.offsetMin = new Vector2(6, 6);
            rt.offsetMax = new Vector2(-6, -6);
            bg = AddImage(rt.gameObject, S(T.chip, Rounded), new Color(1, 1, 1, 0.25f), true);
            var btn = rt.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            btn.transition = Selectable.Transition.None; // warna diatur script (tab terpilih)
            Outlined(Text("Label", rt, label, 34, Color.white, TextAlignmentOptions.Center, true, true));
            return btn;
        }

        // ==================================================================
        // DIALOG VISUAL NOVEL (ref: pemandu di bawah sorotan + kotak dialog berbingkai)
        // ==================================================================
        private static UIScreen BuildDialogueScreen(Transform root)
        {
            var screen = NewScreen<DialogueScreen>(root, "Screen_Dialogue", UIScreenId.Dialogue, new Color(0.15f, 0.10f, 0.13f), out var safe);

            var spot = Img("Spotlight", safe, S(T.spotlight, Circle), new Color(1f, 0.92f, 0.7f, 0.35f));
            Place(spot.rectTransform, new Vector2(0.5f, 1), new Vector2(0, 250), new Vector2(1100, 1800));
            var floor = Img("FloorGlow", safe, S(T.glow, Circle), new Color(1f, 0.85f, 0.5f, 0.45f));
            Place(floor.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 360), new Vector2(760, 160));

            var portrait = Img("Portrait", safe, T.guidePortrait, Color.white);
            Place(portrait.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 400), new Vector2(700, 1050));
            portrait.preserveAspect = true;

            // Tombol transparan layar penuh: ketuk di mana saja untuk lanjut
            var advImg = Img("AdvanceArea", safe, null, new Color(0, 0, 0, 0), true);
            Stretch(advImg.rectTransform);
            var advance = advImg.gameObject.AddComponent<Button>();
            advance.targetGraphic = advImg;
            advance.transition = Selectable.Transition.None;

            var box = Rect("DialogBox", safe);
            BottomBand(box, 400, 50, 34);
            if (T.dialogBox != null)
            {
                Stretch(Img("Frame", box, T.dialogBox, Color.white).rectTransform);
            }
            else
            {
                Stretch(Img("Border", box, Rounded, new Color(0.55f, 0.85f, 0.75f)).rectTransform);
                Stretch(Img("Inner", box, Rounded, Color.white).rectTransform, 10, 10, 10, 10);
            }

            var tag = Img("NameTag", box, S(T.nameTag, Rounded), Tint(T.nameTag, T.primary));
            Place(tag.rectTransform, new Vector2(0, 1), new Vector2(30, 52), new Vector2(430, 96));
            var speaker = Outlined(Text("Speaker", tag.transform, "Kak Pandu", 38, Color.white, TextAlignmentOptions.Center, true, true));
            Stretch(speaker.rectTransform, 20, 4, 20, 22);

            var body = Text("Body", box, "...", 40, T.textDark, TextAlignmentOptions.TopLeft);
            Stretch(body.rectTransform, 56, 102, 56, 44); // di bawah garis putus-putus tiket
            var typewriter = body.gameObject.AddComponent<TypewriterText>();

            var nextInd = Icon("NextIndicator", box, T.iconNext, ">", 64, T.secondary);
            Place(nextInd, new Vector2(1, 0), new Vector2(-40, 30), new Vector2(64, 64));
            nextInd.gameObject.AddComponent<PulseScale>();

            var skip = Button("SkipButton", safe, T.chip, new Color(0, 0, 0, 0.35f), "Lewati", 36, Vector2.zero);
            Place((RectTransform)skip.transform, new Vector2(1, 1), new Vector2(-30, -30), new Vector2(220, 90));

            Bind(screen, "portraitImage", portrait);
            Bind(screen, "defaultPortrait", T.guidePortrait);
            Bind(screen, "speakerText", speaker);
            Bind(screen, "typewriter", typewriter);
            Bind(screen, "advanceButton", advance);
            Bind(screen, "skipButton", skip);
            Bind(screen, "nextIndicator", nextInd.gameObject);
            return screen;
        }
    }
}
