using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>
    /// Overlay UI di atas kamera AR.
    /// Alur: Memindai (GPS geofence) -> Bertemu tokoh (billboard AR + cincin target)
    ///       -> Pilih benda di tas -> Usap ke atas untuk memberikan -> Hadiah -> Dialog.
    ///
    /// GPSManager menentukan kapan pemain masuk radius zona; TampilKarakterAR memunculkan
    /// tokoh 2D di dunia AR dan memainkan reaksinya. Tanpa GPS (mis. di Editor),
    /// 'simulateEncounter' memunculkan tokoh otomatis setelah jeda.
    /// </summary>
    public class ARCameraScreen : UIScreen
    {
        [Header("Referensi")]
        [SerializeField] private CharacterDatabase characterDatabase;
        [SerializeField] private ZoneDatabase zoneDatabase;
        [SerializeField] private ItemDatabase itemDatabase;
        [Tooltip("Root AR Session + XR Origin. Dinyalakan hanya saat layar AR aktif (hemat baterai).")]
        [SerializeField] private GameObject arSessionRoot;
        [SerializeField] private ScreenshotService screenshotService;

        [Header("Integrasi GPS & Tokoh AR")]
        [SerializeField] private GPSManager gps;
        [SerializeField] private TampilKarakterAR characterSpawner;

        [Header("Tombol")]
        [SerializeField] private Button closeButton;
        [SerializeField] private Button helpButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button captureButton;

        [Header("Grup Tampilan")]
        [SerializeField] private GameObject scanningGroup;
        [SerializeField] private GameObject encounterGroup;

        [Header("Memindai")]
        [SerializeField] private TextMeshProUGUI scanText;

        [Header("Encounter")]
        [SerializeField] private TextMeshProUGUI encounterText;
        [SerializeField] private TextMeshProUGUI gradeText;
        [Tooltip("Pratinjau tokoh di UI. Otomatis disembunyikan bila tokoh punya model di TampilKarakterAR.")]
        [SerializeField] private Image previewCharacterImage;
        [SerializeField] private ShrinkingRing ring;
        [SerializeField] private SwipeGestureArea gestureArea;
        [SerializeField] private GameObject swipeHint;
        [SerializeField] private TextMeshProUGUI swipeHintText;
        [SerializeField] private CanvasGroup flashOverlay;

        [Header("Tas Benda")]
        [SerializeField] private GameObject itemTray;
        [SerializeField] private ItemSlotView itemSlotTemplate;
        [SerializeField] private Image flyingItem;
        [SerializeField] private int correctItemXp = 100;

        [Header("Simulasi (tanpa GPS, mis. di Editor)")]
        [Tooltip("Bila GPS tidak aktif, tokoh muncul otomatis setelah jeda. Matikan untuk uji lapangan murni GPS.")]
        [SerializeField] private bool simulateEncounter = true;
        [SerializeField] private float simulatedScanDelay = 2f;
        [SerializeField] private float scanInterval = 0.5f;

        private readonly List<ItemSlotView> slots = new List<ItemSlotView>();
        private CharacterData pending;
        private CharacterData active;
        private ItemSlotView selected;
        private Coroutine flow;
        private bool busy;
        private int wrongAttempts;

        public event Action<CharacterData> OnBefriended;

        private bool WantsItem => active != null && active.wantedItem != null && slots.Count > 0;

        private void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(() => UIManager.Instance.ShowRoot(UIScreenId.Map));
            if (helpButton != null) helpButton.onClick.AddListener(() =>
                ToastNotifier.Show("Datangi titik bertanda di peta. Saat tokoh muncul, pilih benda lalu usap layar ke atas ketika lingkaran mengecil!"));
            if (resetButton != null) resetButton.onClick.AddListener(ResetScan);
            if (captureButton != null) captureButton.onClick.AddListener(CapturePhoto);
            if (gestureArea != null) gestureArea.OnSwipeUp += HandleSwipe;
        }

        /// <summary>Tentukan tokoh yang akan muncul berikutnya (dipanggil dari popup zona).</summary>
        public void PrepareEncounter(CharacterData character) => pending = character;

        protected override void OnShow()
        {
            if (arSessionRoot != null) arSessionRoot.SetActive(true);
            if (flashOverlay != null) flashOverlay.alpha = 0f;
            ResetScan();
        }

        protected override void OnHide()
        {
            StopFlow();
            if (characterSpawner != null) characterSpawner.SembunyikanSemua();
            if (arSessionRoot != null) arSessionRoot.SetActive(false);
        }

        // ------------------------------------------------------------------
        // Memindai
        // ------------------------------------------------------------------
        private void ResetScan()
        {
            StopFlow();
            if (characterSpawner != null) characterSpawner.SembunyikanSemua();
            EnterScanning();
            if (isActiveAndEnabled) flow = StartCoroutine(ScanRoutine());
        }

        private void EnterScanning()
        {
            active = null;
            busy = false;
            if (scanningGroup != null) scanningGroup.SetActive(true);
            if (encounterGroup != null) encounterGroup.SetActive(false);
            if (gradeText != null) gradeText.gameObject.SetActive(false);
            if (flyingItem != null) flyingItem.gameObject.SetActive(false);
            ShowItemTray(false);
        }

        private IEnumerator ScanRoutine()
        {
            float waited = 0f;
            while (true)
            {
                if (gps != null && gps.LokasiAktif)
                {
                    if (TryGpsEncounter()) yield break;
                }
                else if (simulateEncounter)
                {
                    SetScanText("Arahkan kamera ke sekitarmu...");
                    if (waited >= simulatedScanDelay)
                    {
                        TriggerEncounter(pending != null ? pending : DefaultCharacter());
                        yield break;
                    }
                }
                else
                {
                    SetScanText(gps != null ? gps.Status : "GPS belum terpasang.");
                }

                yield return new WaitForSecondsRealtime(scanInterval);
                waited += scanInterval;
            }
        }

        /// <summary>Cek geofence. Return true bila tokoh dimunculkan.</summary>
        private bool TryGpsEncounter()
        {
            MapZone zone = null;
            float distance = -1f;
            if (pending != null && zoneDatabase != null)
            {
                zone = zoneDatabase.Find(pending.zoneId);
                distance = gps.JarakKeZona(zone);
            }
            if (zone == null) zone = gps.ZonaTerdekat(out distance, IsEncounterZone);

            if (zone == null)
            {
                SetScanText("Belum ada tokoh di sekitar sini.\nLihat peta untuk mencari titik bertanda!");
                return false;
            }
            if (distance > zone.radiusMeters)
            {
                SetScanText($"Ayo dekati <b>{zone.displayName}</b>!\nTinggal {Mathf.CeilToInt(distance - zone.radiusMeters)} meter lagi");
                return false;
            }

            var character = pending != null ? pending : characterDatabase != null ? characterDatabase.Find(zone.characterId) : null;
            if (character == null) return false;
            TriggerEncounter(character);
            return true;
        }

        private bool IsEncounterZone(MapZone zone)
        {
            if (string.IsNullOrEmpty(zone.characterId) || characterDatabase == null) return false;
            var c = characterDatabase.Find(zone.characterId);
            return c != null && !c.comingSoon;
        }

        private CharacterData DefaultCharacter()
        {
            if (characterDatabase == null) return null;
            return characterDatabase.FirstLocked() ?? characterDatabase.FirstAvailable();
        }

        // ------------------------------------------------------------------
        // Bertemu tokoh
        // ------------------------------------------------------------------

        /// <summary>Titik masuk pertemuan: munculkan tokoh di dunia AR dan tampilkan UI pertemuan.</summary>
        public void TriggerEncounter(CharacterData character)
        {
            if (character == null) return;
            StopFlow();
            active = character;
            busy = false;
            wrongAttempts = 0;

            if (scanningGroup != null) scanningGroup.SetActive(false);
            if (encounterGroup != null) encounterGroup.SetActive(true);
            if (swipeHint != null) swipeHint.SetActive(true);

            bool hasModel = characterSpawner != null && characterSpawner.MunculkanDiDepanPemain(character.id) != null;
            ShowItemTray(character.wantedItem != null);

            bool ask = WantsItem && !string.IsNullOrEmpty(character.askLine);
            SetEncounterText(ask
                ? $"Wah! Kamu bertemu <b>{character.displayName}</b>!\n<size=78%>\"{character.askLine}\"</size>"
                : $"Wah! Kamu bertemu\n<b>{character.displayName}</b>!");
            UpdateSwipeHint();

            if (previewCharacterImage != null)
            {
                // Tokoh yang sudah tampil sebagai billboard AR tidak perlu pratinjau 2D
                previewCharacterImage.sprite = character.portrait;
                previewCharacterImage.enabled = !hasModel && character.portrait != null;
                if (previewCharacterImage.enabled)
                    StartCoroutine(UIAnim.Scale(previewCharacterImage.transform, Vector3.zero, Vector3.one, 0.45f));
            }
        }

        private void HandleSwipe(float strength)
        {
            if (active == null || busy) return;

            // Nilai usapan berdasarkan ukuran cincin (semakin kecil semakin bagus)
            float n = ring != null ? ring.Normalized : 1f;
            string grade; int bonus;
            if (n < 0.35f) { grade = "Luar Biasa!"; bonus = 100; }
            else if (n < 0.65f) { grade = "Hebat!"; bonus = 50; }
            else { grade = "Bagus!"; bonus = 10; }

            if (!WantsItem)
            {
                busy = true;
                flow = StartCoroutine(GreetRoutine(grade, bonus));
                return;
            }
            if (selected == null)
            {
                if (characterSpawner != null) characterSpawner.ReaksiTanpaBenda();
                ToastNotifier.Show("Pilih benda dulu dari tas di bawah, ya!");
                if (itemTray != null) StartCoroutine(UIAnim.Punch(itemTray.transform, 0.06f, 0.3f));
                return;
            }
            busy = true;
            flow = StartCoroutine(GiveItemRoutine(selected, grade, bonus));
        }

        private IEnumerator GreetRoutine(string grade, int bonus)
        {
            if (swipeHint != null) swipeHint.SetActive(false);
            ShowGrade(grade);
            if (previewCharacterImage != null && previewCharacterImage.enabled)
                StartCoroutine(UIAnim.Punch(previewCharacterImage.transform, 0.25f, 0.4f));

            yield return new WaitForSecondsRealtime(0.9f);
            yield return FinishEncounter(grade, bonus, null);
        }

        private IEnumerator GiveItemRoutine(ItemSlotView slot, string grade, int bonus)
        {
            var character = active;
            var item = slot.Item;
            if (swipeHint != null) swipeHint.SetActive(false);

            yield return FlyItem(slot);

            bool correct = item == character.wantedItem;
            if (characterSpawner != null) characterSpawner.ReaksiBenda(correct, item.icon);

            if (!correct)
            {
                wrongAttempts++;
                string reject = string.IsNullOrEmpty(character.rejectLine) ? "Hmm, bukan ini yang kucari..." : character.rejectLine;
                SetEncounterText($"<b>{character.displayName}</b>:\n<size=85%>\"{reject}\"</size>");
                ToastNotifier.Show($"Bukan {item.displayName}... Coba benda lain!");

                yield return new WaitForSecondsRealtime(1.2f);
                SelectSlot(null);
                if (swipeHint != null) swipeHint.SetActive(true);
                busy = false;
                flow = null;
                yield break;
            }

            string thanks = string.IsNullOrEmpty(character.thanksLine) ? "Terima kasih, aku senang sekali!" : character.thanksLine;
            SetEncounterText($"<b>{character.displayName}</b>:\n<size=85%>\"{thanks}\"</size>");
            ShowItemTray(false);
            ShowGrade(grade);

            // Beri waktu animasi "terima benda" berjalan
            yield return new WaitForSecondsRealtime(2.4f);

            // Semakin sedikit salah, semakin besar bonus
            var extra = new List<RewardLine> { new RewardLine("Benda yang tepat!", correctItemXp / (1 + wrongAttempts)) };
            yield return FinishEncounter(grade, bonus, extra);
        }

        private IEnumerator FinishEncounter(string grade, int bonus, List<RewardLine> extraLines)
        {
            var character = active;
            if (encounterGroup != null) encounterGroup.SetActive(false);
            if (gradeText != null) gradeText.gameObject.SetActive(false);
            ToastNotifier.Show($"{character.displayName} menjadi temanmu!");

            yield return new WaitForSecondsRealtime(0.6f);
            flow = null;
            pending = null;
            OnBefriended?.Invoke(character);
            RewardFlow.GrantCharacter(character, grade, bonus, () => AfterReward(character), extraLines);
        }

        private static void AfterReward(CharacterData character)
        {
            var ui = UIManager.Instance;
            var dialogue = ui.Get<DialogueScreen>(UIScreenId.Dialogue);
            if (character.encounterDialogue != null && dialogue != null)
                dialogue.Play(character.encounterDialogue, () => UIManager.Instance.ShowRoot(UIScreenId.Map));
            else
                ui.ShowRoot(UIScreenId.Map);
        }

        // ------------------------------------------------------------------
        // Tas benda
        // ------------------------------------------------------------------
        private void ShowItemTray(bool show)
        {
            if (itemTray == null) return;
            if (show) EnsureSlots();
            show &= slots.Count > 0;
            itemTray.SetActive(show);
            SelectSlot(null);
        }

        // Slot dibuat sekali, lalu dipakai ulang (hindari Instantiate berulang)
        private void EnsureSlots()
        {
            if (itemSlotTemplate == null || itemDatabase == null || slots.Count > 0) return;
            itemSlotTemplate.gameObject.SetActive(false);
            foreach (var item in itemDatabase.items)
            {
                if (item == null) continue;
                var slot = Instantiate(itemSlotTemplate, itemSlotTemplate.transform.parent);
                slot.gameObject.SetActive(true);
                slot.Setup(item, OnSlotClicked);
                slots.Add(slot);
            }
        }

        private void OnSlotClicked(ItemSlotView slot)
        {
            if (busy) return;
            SelectSlot(slot);
        }

        private void SelectSlot(ItemSlotView slot)
        {
            selected = slot;
            for (int i = 0; i < slots.Count; i++) slots[i].SetSelected(slots[i] == slot);
            UpdateSwipeHint();
        }

        private void UpdateSwipeHint()
        {
            if (swipeHintText == null) return;
            if (!WantsItem) swipeHintText.text = "Usap ke atas untuk menyapa!";
            else if (selected == null) swipeHintText.text = "Pilih benda di tas, lalu usap ke atas!";
            else swipeHintText.text = $"Usap ke atas untuk memberikan <b>{selected.Item.displayName}</b>!";
        }

        // Ikon benda melayang dari tas menuju tokoh
        private IEnumerator FlyItem(ItemSlotView slot)
        {
            if (flyingItem == null || slot.IconImage == null) yield break;

            var rt = flyingItem.rectTransform;
            Vector3 from = slot.IconImage.rectTransform.position;
            Vector3 to = ring != null ? ring.transform.position : rt.parent.position;
            flyingItem.sprite = slot.Item.icon;
            flyingItem.gameObject.SetActive(true);

            const float duration = 0.45f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                rt.position = Vector3.LerpUnclamped(from, to, UIAnim.EaseOutCubic(k));
                rt.localScale = Vector3.one * Mathf.Lerp(1.3f, 0.5f, k);
                flyingItem.color = new Color(1f, 1f, 1f, 1f - k * k);
                yield return null;
            }
            flyingItem.gameObject.SetActive(false);
        }

        // ------------------------------------------------------------------
        // Helper
        // ------------------------------------------------------------------
        private void ShowGrade(string grade)
        {
            if (gradeText == null) return;
            gradeText.text = grade;
            gradeText.gameObject.SetActive(true);
            StartCoroutine(UIAnim.Scale(gradeText.transform, Vector3.zero, Vector3.one, 0.35f));
        }

        private void SetEncounterText(string text)
        {
            if (encounterText == null) return;
            encounterText.text = text;
            StartCoroutine(UIAnim.Scale(encounterText.transform.parent, Vector3.one * 0.6f, Vector3.one, 0.35f));
        }

        private void SetScanText(string text)
        {
            if (scanText != null && scanText.text != text) scanText.text = text;
        }

        private void CapturePhoto()
        {
            if (screenshotService == null)
            {
                ToastNotifier.Show("ScreenshotService belum dipasang.");
                return;
            }
            screenshotService.Capture(path =>
            {
                if (isActiveAndEnabled && flashOverlay != null) StartCoroutine(UIAnim.Fade(flashOverlay, 1f, 0f, 0.35f));
                ToastNotifier.Show(path != null ? "Foto tersimpan!" : "Gagal menyimpan foto.");
            });
        }

        private void StopFlow()
        {
            if (flow != null) StopCoroutine(flow);
            flow = null;
        }
    }
}
