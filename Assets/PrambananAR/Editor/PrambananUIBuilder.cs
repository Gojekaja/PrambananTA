using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static PrambananAR.UI.EditorTools.UIBuilderKit;

namespace PrambananAR.UI.EditorTools
{
    /// <summary>
    /// Menu: Tools > Prambanan AR > ...
    /// Membangun seluruh hierarki UI (Canvas, layar, popup) dan menyambungkan referensinya.
    /// File ini berisi alur utama; tata letak tiap layar ada di file partial lain.
    /// </summary>
    public static partial class PrambananUIBuilder
    {
        private const string RootName = "UI_Root";
        private static SampleContentFactory.Bundle content;

        private static UITheme T => Theme;

        [MenuItem("Tools/Prambanan AR/1. Buat Aset Data dan Tema", priority = 1)]
        private static void CreateAssetsMenu()
        {
            var bundle = SampleContentFactory.CreateOrLoadAll();
            Selection.activeObject = bundle.theme;
            EditorGUIUtility.PingObject(bundle.theme);
            Debug.Log("[Prambanan AR] Aset data dibuat di Assets/PrambananAR/Data. UITheme sudah terisi aset tema (Art/Theme); field bisa diganti manual.");
        }

        [MenuItem("Tools/Prambanan AR/2. Build UI di Scene Aktif", priority = 2)]
        private static void BuildMenu()
        {
            var existing = GameObject.Find(RootName);
            if (existing != null)
            {
                if (!EditorUtility.DisplayDialog("Build UI", "UI_Root sudah ada di scene. Ganti dengan yang baru?", "Ganti", "Batal"))
                    return;
                Undo.DestroyObjectImmediate(existing);
            }

            content = SampleContentFactory.CreateOrLoadAll();
            Init(content.theme);
            EnsureEventSystem();

            var canvas = CreateCanvas();
            Transform root = canvas.transform;
            var manager = canvas.gameObject.AddComponent<UIManager>();
            var screenshot = canvas.gameObject.AddComponent<ScreenshotService>();

            // Urutan pembuatan = urutan render (yang dibuat terakhir tampil paling atas)
            var map = BuildMapScreen(root);
            var ar = BuildARScreen(root, screenshot);
            var screens = new List<Object>
            {
                map,
                ar,
                BuildCollectionScreen(root),
                BuildStoryScreen(root),
                BuildDialogueScreen(root),
                BuildLoadingScreen(root),
                BuildQuickMenu(root),
                BuildNearbyPanel(root),
                BuildZoneInfoPopup(root),
                BuildCharacterDetailPopup(root),
                BuildRewardPopup(root),
                BuildLevelUpPopup(root),
                BuildConsentPopup(root),
            };
            BuildToast(root);

            BindArray(manager, "screens", screens);
            Bind(manager, "rootGroup", canvas.GetComponent<CanvasGroup>());
            SetInt(manager, "startScreen", (int)UIScreenId.Loading);
            IntegrateScene(ar, (MapScreen)map);

            // Nonaktifkan semua layar kecuali Peta agar mudah diedit di Scene view.
            // Saat Play, UIManager yang mengatur tampil/sembunyi.
            foreach (var s in screens)
                if (s != map) ((Component)s).gameObject.SetActive(false);

            Undo.RegisterCreatedObjectUndo(canvas.gameObject, "Build Prambanan UI");
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Selection.activeGameObject = canvas.gameObject;
            Debug.Log("[Prambanan AR] UI berhasil dibangun. Tekan Play untuk mencoba alur Loading -> Persetujuan -> Peta.");
        }

        [MenuItem("Tools/Prambanan AR/3. Reset Progres Pemain", priority = 20)]
        private static void ResetProgress()
        {
            PlayerProgressService.ResetAll();
            Debug.Log("[Prambanan AR] Progres pemain direset (persetujuan, intro, koleksi, XP).");
        }

        // ------------------------------------------------------------------
        // Fondasi
        // ------------------------------------------------------------------
        /// <summary>
        /// Sambungkan UI dengan sistem AR di scene: GPSManager (geofence), TampilKarakterAR (tokoh),
        /// root AR Session + XR Origin (hanya menyala di layar kamera), dan Canvas debug lama.
        /// </summary>
        private static void IntegrateScene(UIScreen arScreen, MapScreen map)
        {
            var gps = Object.FindAnyObjectByType<GPSManager>(FindObjectsInactive.Include);
            if (gps != null)
            {
                Bind(arScreen, "gps", gps);
                Bind(map, "gps", gps);
                Undo.RecordObject(gps, "Hubungkan GPSManager");
                gps.zoneDatabase = content.zones;
                // Teks koordinat lama tidak lagi tampil di atas UI baru (aktifkan Canvas lama untuk debug lapangan)
                if (gps.koordinatText != null)
                {
                    var oldCanvas = gps.koordinatText.canvas != null ? gps.koordinatText.canvas.rootCanvas.gameObject : null;
                    if (oldCanvas != null && oldCanvas.name != RootName)
                    {
                        Undo.RecordObject(oldCanvas, "Nonaktifkan Canvas debug");
                        oldCanvas.SetActive(false);
                    }
                }
                EditorUtility.SetDirty(gps);
            }
            else Debug.LogWarning("[Prambanan AR] GPSManager tidak ditemukan di scene; geofence memakai mode simulasi.");

            var spawner = Object.FindAnyObjectByType<TampilKarakterAR>(FindObjectsInactive.Include);
            if (spawner != null)
            {
                Bind(arScreen, "characterSpawner", spawner);
                Undo.RecordObject(spawner, "Daftarkan tokoh AR");
                RegisterCharacterModels(spawner);
                EditorUtility.SetDirty(spawner);
            }
            else Debug.LogWarning("[Prambanan AR] TampilKarakterAR tidak ditemukan; tokoh tampil sebagai gambar 2D di UI.");

            var arRoot = EnsureArRoot();
            if (arRoot != null) Bind(arScreen, "arSessionRoot", arRoot);
        }

        /// <summary>Daftarkan objek tokoh di scene (nama objek = id tanpa garis bawah, mis. roroJonggrang).</summary>
        private static void RegisterCharacterModels(TampilKarakterAR spawner)
        {
            foreach (var c in content.characters.characters)
            {
                if (c == null || spawner.PunyaModel(c.id)) continue;
                string objName = ToCamelCase(c.id);
                GameObject model = null;
                foreach (var go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                    if (string.Equals(go.name, objName, System.StringComparison.OrdinalIgnoreCase)) model = go;
                if (model == null) continue;
                spawner.daftarKarakter.Add(new TampilKarakterAR.KarakterAR { idKarakter = c.id, objek = model });
                Debug.Log($"[Prambanan AR] Tokoh AR terdaftar: {c.id} -> {model.name}");
            }
        }

        private static string ToCamelCase(string id)
        {
            var parts = id.Split('_');
            var sb = new System.Text.StringBuilder(parts[0]);
            for (int i = 1; i < parts.Length; i++)
                if (parts[i].Length > 0) sb.Append(char.ToUpperInvariant(parts[i][0])).Append(parts[i].Substring(1));
            return sb.ToString();
        }

        /// <summary>Kelompokkan AR Session + XR Origin di bawah "AR_Root" agar bisa dinyalakan/dimatikan sekaligus.</summary>
        private static GameObject EnsureArRoot()
        {
            var existing = GameObject.Find("AR_Root");
            if (existing != null) return existing;

            var session = FindByTypeName("UnityEngine.XR.ARFoundation.ARSession, Unity.XR.ARFoundation");
            var origin = FindByTypeName("Unity.XR.CoreUtils.XROrigin, Unity.XR.CoreUtils");
            if (session == null && origin == null) return null;

            var root = new GameObject("AR_Root");
            Undo.RegisterCreatedObjectUndo(root, "Buat AR_Root");
            if (session != null) Undo.SetTransformParent(session.transform.root, root.transform, "AR_Root");
            if (origin != null && origin.transform.root != root.transform) Undo.SetTransformParent(origin.transform.root, root.transform, "AR_Root");
            // Kamera AR baru menyala saat layar Kamera AR dibuka (izin kamera diminta setelah persetujuan)
            root.SetActive(false);
            return root;
        }

        private static Component FindByTypeName(string assemblyQualifiedName)
        {
            var type = System.Type.GetType(assemblyQualifiedName);
            return type != null ? Object.FindAnyObjectByType(type, FindObjectsInactive.Include) as Component : null;
        }

        private static void EnsureEventSystem()
        {
#if UNITY_2022_2_OR_NEWER
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
#else
            if (Object.FindObjectOfType<EventSystem>() != null) return;
#endif
            var go = new GameObject("EventSystem", typeof(EventSystem));
#if ENABLE_INPUT_SYSTEM
            go.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            go.AddComponent<StandaloneInputModule>();
#endif
            Undo.RegisterCreatedObjectUndo(go, "Create EventSystem");
        }

        private static Canvas CreateCanvas()
        {
            var go = new GameObject(RootName, typeof(RectTransform));
            go.layer = UILayer;
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920); // portrait
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            go.AddComponent<GraphicRaycaster>();
            go.AddComponent<CanvasGroup>();
            return canvas;
        }

        /// <summary>Buat layar penuh + child "SafeArea" tempat semua konten diletakkan.</summary>
        private static TScreen NewScreen<TScreen>(Transform parent, string name, UIScreenId id, Color? bg,
            out RectTransform safe, Sprite bgSprite = null) where TScreen : UIScreen
        {
            var rt = Stretch(Rect(name, parent));
            rt.gameObject.AddComponent<CanvasGroup>();
            var screen = rt.gameObject.AddComponent<TScreen>();
            SetInt(screen, "screenId", (int)id);
            if (bg.HasValue)
            {
                var img = AddImage(rt.gameObject, bgSprite, bg.Value, true); // raycast true: blokir sentuhan tembus
                img.type = Image.Type.Simple;
            }
            safe = Stretch(Rect("SafeArea", rt));
            safe.gameObject.AddComponent<SafeAreaFitter>();
            return screen;
        }

        /// <summary>Buat popup: latar gelap + panel tengah (+ tombol X opsional).</summary>
        private static TPopup NewPopup<TPopup>(Transform parent, string name, UIScreenId id, Vector2 panelSize,
            out RectTransform panel, bool withClose = true) where TPopup : UIPopup
        {
            var rt = Stretch(Rect(name, parent));
            rt.gameObject.AddComponent<CanvasGroup>();
            var popup = rt.gameObject.AddComponent<TPopup>();
            SetInt(popup, "screenId", (int)id);
            AddImage(rt.gameObject, null, T.overlayDim, true);

            var safe = Stretch(Rect("SafeArea", rt));
            safe.gameObject.AddComponent<SafeAreaFitter>();

            var panelImg = Img("Panel", safe, S(T.panelPopup, Rounded), Tint(T.panelPopup, T.panel), true);
            panel = Place(panelImg.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, panelSize);
            Bind(popup, "panel", panel);

            if (withClose)
            {
                var close = IconButton("CloseButton", panel, T.iconClose, "X", 110, T.danger);
                Place((RectTransform)close.transform, new Vector2(1, 1), new Vector2(25, 25), new Vector2(110, 110));
                Bind(popup, "closeButton", close);
            }
            return popup;
        }

        /// <summary>Tombol ikon bulat dengan teks keterangan di bawahnya.</summary>
        private static Button CaptionedIcon(string name, Transform parent, Sprite icon, string glyph, string caption,
            float size, Color color, Vector2 anchor, Vector2 pos)
        {
            var holder = Place(Rect(name, parent), anchor, pos, new Vector2(size + 40, size + 50));
            var btn = IconButton("Button", holder, icon, glyph, size, color);
            Place((RectTransform)btn.transform, new Vector2(0.5f, 1), Vector2.zero, new Vector2(size, size));
            // Keterangan putih bergaris tepi agar terbaca di atas peta
            var cap = Outlined(Text("Caption", holder, caption, 30, Color.white, TextAlignmentOptions.Center, true, true));
            TopBand(cap.rectTransform, 46, size + 2);
            return btn;
        }

        private static TextMeshProUGUI LabelOf(Button b) => b.GetComponentInChildren<TextMeshProUGUI>(true);
    }
}
