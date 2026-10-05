# Prambanan AR (Legenda Roro Jonggrang)

Aplikasi AR edukasi anak (Unity 6000.5, uGUI + TextMeshPro, **portrait 1080×1920**, Android/ARCore, Built-in RP, Gamma).
Scene utama: `Assets/rorojonggrang.unity`. UI dibangun otomatis lewat menu Editor dan sudah tersambung ke GPS & tokoh AR.

## Struktur

```
Assets/
├─ Script/                 Skrip AR milik proyek (namespace global)
│  ├─ GPSManager           GPS + geofence per zona ZoneDatabase; event OnLokasiBerubah; mulai setelah persetujuan
│  ├─ TampilKarakterAR     Daftar tokoh AR (idKarakter -> objek scene), billboard, reaksi Animator
│  └─ EfekPemberianBunga   Animasi benda masuk ke tangan tokoh (MulaiBerikan(sprite))
├─ Art/                    roroJonggrang.psb (rig 2D), item.psb (7 benda)
├─ Animation/              roroJonggrang.controller (trigTerimaBerhasil, trigTerimaGagal, trigNoItem, isTalking)
├─ 300Mind/2D Game UI Kit  Aset GUI sumber (sprite sheet + font GROBOLD)
└─ PrambananAR/
   ├─ Scripts/
   │  ├─ Core/      UIManager, UIScreen, UIPopup, UIAnim, UITheme, ToastNotifier, SafeAreaFitter, ButtonPressFeedback
   │  ├─ Data/      PlayerProgress(+Service), CharacterData, CharacterDatabase, ItemData, ItemDatabase, ZoneDatabase, DialogueData, StoryData
   │  ├─ Effects/   PulseScale, Rotator, ShrinkingRing, SwipeHintAnimator, SwipeGestureArea
   │  ├─ Screens/   LoadingScreen, MapScreen, ARCameraScreen, CollectionScreen, StoryScreen, DialogueScreen
   │  ├─ Popups/    ConsentPopup, QuickMenuPopup, NearbyPanel, ZoneInfoPopup, CharacterDetailPopup, RewardPopup, LevelUpPopup
   │  ├─ Views/     MapZonePin, CharacterCardView, ItemSlotView, MilestoneView, RewardLineView, PlayerHUD, TypewriterText
   │  └─ Services/  RewardFlow, ScreenshotService
   ├─ Editor/       PrambananUIBuilder (+ .Screens, .Popups), UIBuilderKit, SampleContentFactory
   │  └─ Art/       ArtCanvas (rasterizer SDF), ThemeArtGenerator (+ .Illustrations)
   ├─ Art/Theme/          Sprite UI tema (hasil generator, jangan diedit manual — buat ulang lewat menu 0)
   ├─ Art/Illustrations/  Peta, latar loading, Kak Pandu, potret Roro, sampul & halaman cerita
   ├─ Art/Fonts/          GROBOLD SDF (+ material Outline)
   └─ Data/               UITheme, CharacterDatabase, ItemDatabase, ZoneDatabase, dialog, cerita (hasil menu 1)
```

## Menu Editor (Tools → Prambanan AR)

0. **Buat Aset Visual Tema** — membuat ulang semua sprite tema & ilustrasi (±2 menit). Potret Roro dirender dari objek `roroJonggrang` di scene aktif (pose `idle`).
1. **Buat Aset Data dan Tema** — membuat/mengisi aset di `Data/`. Aset yang sudah ada tidak ditimpa (hanya field kosong / sprite placeholder).
2. **Build UI di Scene Aktif** — membangun ulang `UI_Root` lalu menyambungkan GPSManager, TampilKarakterAR, dan `AR_Root` (AR Session + XR Origin). **Perubahan manual di `UI_Root` ikut hilang.**
3. **Reset Progres Pemain** — hapus persetujuan, intro, koleksi, XP.

## Tema visual ("Batu Candi & Emas")

- Potongan 2D Game UI Kit diwarnai ulang (panel bertakik → perkamen, pil hijau → emas/toska, pita ungu → terakota, bar biru → coklat tua); pil hijau/merah, tiket kuning, pita oranye, koin, sarung tangan, perisai bintang dipakai apa adanya.
- Sprite tema yang sudah berwarna dipasang dengan `Image.color = putih` (helper `Tint()`); yang putih (btn_circle, chip, ring, glow, map_pin) diwarnai builder.
- Judul/tombol memakai **GROBOLD SDF** + material Outline (`UIBuilderKit.Outlined()`); teks isi memakai LiberationSans SDF.
- Palet: terakota `#D9693A`, emas `#F2B33D`, toska `#2F9DB3`, hijau daun `#5DAA4A`, coklat `#3D2A1E`, krem `#FFF4DC`.

## Alur aplikasi

```
Loading ─► Persetujuan Orang Tua (sekali; minta izin Kamera & Lokasi) ─► Peta ─► Dialog Intro "Kak Pandu" (sekali)
Peta: pin zona ─► Info Zona (jarak GPS) ─► Kamera AR
Kamera AR: GPS aktif  → "Ayo dekati ... tinggal N meter" sampai masuk radius zona → tokoh muncul (TampilKarakterAR)
           tanpa GPS  → simulasi 2 dtk (simulateEncounter, untuk Editor)
  Tokoh minta benda (askLine) ─► pilih benda di TAS BENDA ─► usap ke atas (nilai dari cincin mengecil)
    benar → trigTerimaBerhasil + benda ke tangan (EfekPemberianBunga) → Hadiah (+XP "Benda yang tepat") → Naik Level → Dialog tokoh → Peta
    salah → trigTerimaGagal + rejectLine, boleh coba lagi (bonus berkurang)
    tanpa memilih → trigNoItem + toast
  Tokoh tanpa wantedItem → cukup usap ke atas (sapaan biasa)
Peta: Menu ─► Koleksi | Cerita | Panduan | Kamera AR;  Sekitar ─► grid siluet ─► peta bergeser ke zona
```

Tombol **Back Android** ditangani `UIManager`. `AR_Root` hanya aktif saat layar Kamera AR terbuka (hemat baterai, kamera tidak menyala sebelum persetujuan).

## Menambah tokoh baru

1. Taruh rig tokoh di scene sebagai objek root bernama camelCase dari id (mis. `bandungBondowoso` untuk `bandung_bondowoso`), atau tambahkan manual ke `TampilKarakterAR.daftarKarakter`.
2. Di `Data/Characters/Char_<id>.asset`: hapus centang **comingSoon**, isi `portrait` (badan penuh) & `icon` (kepala, untuk pin), `wantedItem`, `askLine/thanksLine/rejectLine`.
3. Pastikan zona di `ZoneDatabase` (characterId) punya lat/lon & radius hasil ukur di lokasi.
4. Jalankan menu 2 (atau isi referensi manual) lalu uji.

## Titik integrasi

| Kebutuhan | API |
|---|---|
| Posisi GPS & geofence | `GPSManager.Instance` → `LokasiAktif`, `Lintang/Bujur`, `JarakKeZona(zona)`, `ZonaDimasuki()`, event `OnLokasiBerubah` |
| Uji GPS di Editor | `GPSManager.gunakanPosisiSimulasi` + `simulasiLintang/Bujur` |
| Paksa tokoh muncul | `ARCameraScreen.TriggerEncounter(CharacterData)` |
| Reaksi tokoh | `TampilKarakterAR.ReaksiBenda(bool, Sprite)`, `ReaksiTanpaBenda()`, `SetBicara(bool)` |
| Hadiah dari minigame lain | `RewardFlow.GrantCharacter(character, grade, bonus, onDone, extraLines)` |

## Hal yang perlu diperhatikan

- **Koordinat zona:** hanya `RJ` yang memakai titik ukur lapangan (-7.752020, 110.491467, radius 10 m). Zona lain masih turunan kasar dari peta — ukur ulang sebelum uji lapangan.
- **Peta** `Art/Illustrations/map_prambanan.png` digambar ulang secara prosedural (bukan salinan brosur); posisi zona = `MapZone.mapPosition` (0..1, kiri-bawah).
- **Benda di tangan tokoh:** sprite `item.psb` ber-PPU 50 (bunga ±11 m pada skala 1), jadi `EfekPemberianBunga.skalaNormal` = 0.04 dan `skalaBesar` = 0.12. Sesuaikan bila PPU diubah.
- **Input System baru saja** (activeInputHandler = 1). Jangan memakai `Input.GetKey*`; tombol Back memakai `Keyboard.current`. `Input.location` tetap dipakai untuk GPS.
- **Performa:** layar tersembunyi di-`SetActive(false)`; `Update()` hanya di UIManager (tombol Back), GPSManager (data lokasi baru), TampilKarakterAR (billboard). Teks `raycastTarget = false`.
- **Uji di Editor:** Play mode berhenti saat jendela Unity tidak fokus (`Run In Background` mati); warning XR subsystem di Editor normal tanpa XR Simulation.
- Canvas debug lama (`Canvas/KoordinatText`) dinonaktifkan; aktifkan untuk melihat koordinat saat uji lapangan.
- Foto AR disimpan ke `Application.persistentDataPath` (Galeri Android butuh plugin NativeGallery).
- Hak cipta: aset 2D Game UI Kit & font GROBOLD berasal dari paket 300Mind milik pengembang; cantumkan sumbernya di skripsi.
