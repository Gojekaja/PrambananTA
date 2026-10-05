using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>
    /// Menu utama berupa peta kompleks Candi Prambanan yang bisa digeser,
    /// berisi pin zona, penanda pemain, banner misi, dan tombol navigasi bawah.
    /// </summary>
    public class MapScreen : UIScreen
    {
        [Header("Data")]
        [SerializeField] private ZoneDatabase zoneDatabase;
        [SerializeField] private CharacterDatabase characterDatabase;
        [Tooltip("Sumber posisi pemain. Kosong = penanda pemain diam di posisi default.")]
        [SerializeField] private GPSManager gps;

        [Header("Peta")]
        [SerializeField] private ScrollRect mapScroll;
        [SerializeField] private RectTransform mapContent;
        [SerializeField] private Image mapImage;
        [SerializeField] private MapZonePin pinTemplate;
        [SerializeField] private RectTransform playerMarker;
        [SerializeField] private Vector2 defaultPlayerPosition = new Vector2(0.77f, 0.35f); // dekat loket tiket
        [SerializeField] private Color characterPinColor = new Color(0.96f, 0.45f, 0.2f);
        [SerializeField] private Color infoPinColor = new Color(0.23f, 0.62f, 0.88f);

        [Header("Tombol & Banner")]
        [SerializeField] private Button menuButton;
        [SerializeField] private Button collectionButton;
        [SerializeField] private Button nearbyButton;
        [SerializeField] private Button arButton;
        [SerializeField] private Button questBannerButton;
        [SerializeField] private TextMeshProUGUI questText;

        private readonly List<MapZonePin> pins = new List<MapZonePin>();
        private bool built;
        private Coroutine scrollRoutine;

        private void Awake()
        {
            if (menuButton != null) menuButton.onClick.AddListener(() => UIManager.Instance.OpenPopup(UIScreenId.QuickMenu));
            if (collectionButton != null) collectionButton.onClick.AddListener(() => UIManager.Instance.Show(UIScreenId.Collection));
            if (nearbyButton != null) nearbyButton.onClick.AddListener(() => UIManager.Instance.OpenPopup(UIScreenId.NearbyPanel));
            if (arButton != null) arButton.onClick.AddListener(() => UIManager.Instance.Show(UIScreenId.ARCamera));
            if (questBannerButton != null) questBannerButton.onClick.AddListener(FocusActiveQuest);
        }

        protected override void OnShow()
        {
            if (!built) BuildPins();
            for (int i = 0; i < pins.Count; i++) pins[i].Refresh();
            RefreshQuest();

            if (gps != null)
            {
                gps.OnLokasiBerubah += SetPlayerGeo;
                if (gps.LokasiAktif) SetPlayerGeo(gps.Lintang, gps.Bujur);
            }
        }

        protected override void OnHide()
        {
            if (gps != null) gps.OnLokasiBerubah -= SetPlayerGeo;
        }

        private void BuildPins()
        {
            built = true;
            if (mapImage != null && zoneDatabase != null && zoneDatabase.mapSprite != null)
            {
                mapImage.sprite = zoneDatabase.mapSprite;
                mapImage.color = Color.white; // hapus warna placeholder
                var hint = mapImage.transform.Find("PlaceholderHint");
                if (hint != null) hint.gameObject.SetActive(false);
            }

            if (pinTemplate == null || zoneDatabase == null) return;
            pinTemplate.gameObject.SetActive(false);

            foreach (var zone in zoneDatabase.zones)
            {
                var character = characterDatabase != null ? characterDatabase.Find(zone.characterId) : null;
                var pin = Instantiate(pinTemplate, pinTemplate.transform.parent);
                pin.name = $"Pin_{zone.id}";
                pin.gameObject.SetActive(true);
                bool encounterable = character != null && !character.comingSoon;
                pin.Setup(zone, character, encounterable ? characterPinColor : infoPinColor, OpenZoneInfo);
                pins.Add(pin);
            }

            SetPlayerMapPosition(defaultPlayerPosition);
            if (playerMarker != null) playerMarker.SetAsLastSibling();
            scrollRoutine = StartCoroutine(ScrollTo(defaultPlayerPosition, 0f));
        }

        private void OpenZoneInfo(MapZone zone)
        {
            var ui = UIManager.Instance;
            var popup = ui.Get<ZoneInfoPopup>(UIScreenId.ZoneInfoPopup);
            if (popup == null) return;
            popup.Setup(zone, characterDatabase != null ? characterDatabase.Find(zone.characterId) : null);
            ui.OpenPopup(UIScreenId.ZoneInfoPopup);
        }

        private void RefreshQuest()
        {
            if (questText == null || characterDatabase == null) return;
            var next = characterDatabase.FirstLocked();
            if (next == null)
            {
                questText.text = characterDatabase.HasComingSoon()
                    ? "Hebat! Tokoh lainnya segera hadir, nantikan ya!"
                    : "Hebat! Semua tokoh sudah menjadi temanmu!";
                return;
            }
            var zone = zoneDatabase != null ? zoneDatabase.Find(next.zoneId) : null;
            questText.text = $"Misi: Temukan tokoh misterius di {(zone != null ? zone.displayName : "area candi")}!";
        }

        private void FocusActiveQuest()
        {
            var next = characterDatabase != null ? characterDatabase.FirstLocked() : null;
            if (next != null) FocusZone(next.zoneId);
        }

        /// <summary>Geser peta ke zona tertentu dan sorot pin-nya.</summary>
        public void FocusZone(string zoneId)
        {
            var pin = pins.Find(p => p.Zone != null && p.Zone.id == zoneId);
            if (pin == null) return;
            if (scrollRoutine != null) StopCoroutine(scrollRoutine);
            if (isActiveAndEnabled) scrollRoutine = StartCoroutine(ScrollTo(pin.Zone.mapPosition, 0.45f));
            pin.Highlight();
        }

        /// <summary>API untuk sistem GPS nanti: posisikan penanda pemain (0..1).</summary>
        public void SetPlayerMapPosition(Vector2 normalized)
        {
            if (playerMarker == null) return;
            playerMarker.anchorMin = playerMarker.anchorMax = normalized;
            playerMarker.anchoredPosition = Vector2.zero;
        }

        public void SetPlayerGeo(double latitude, double longitude)
        {
            if (zoneDatabase != null) SetPlayerMapPosition(zoneDatabase.GeoToMap(latitude, longitude));
        }

        private IEnumerator ScrollTo(Vector2 mapPos, float duration)
        {
            yield return null; // tunggu layout selesai dihitung
            if (mapScroll == null || mapContent == null) yield break;

            RectTransform viewport = mapScroll.viewport != null ? mapScroll.viewport : (RectTransform)mapScroll.transform;
            Vector2 content = mapContent.rect.size;
            Vector2 view = viewport.rect.size;
            var target = new Vector2(ToNormalized(content.x, view.x, mapPos.x), ToNormalized(content.y, view.y, mapPos.y));

            mapScroll.StopMovement();
            Vector2 from = mapScroll.normalizedPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                mapScroll.normalizedPosition = Vector2.Lerp(from, target, UIAnim.EaseOutCubic(Mathf.Clamp01(t / duration)));
                yield return null;
            }
            mapScroll.normalizedPosition = target;
            scrollRoutine = null;
        }

        // Konversi posisi titik pada konten menjadi normalizedPosition ScrollRect agar titik berada di tengah viewport
        private static float ToNormalized(float contentSize, float viewSize, float point)
        {
            if (contentSize <= viewSize) return 0.5f;
            return Mathf.Clamp01((point * contentSize - viewSize * 0.5f) / (contentSize - viewSize));
        }
    }
}
