using TMPro;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>
    /// Satu tempat untuk semua sprite GUIPackCartoon, font, dan warna.
    /// Isi field ini SEKALI (drag sprite dari folder GUIPackCartoon), lalu jalankan
    /// Tools > Prambanan AR > Build UI. Field yang kosong memakai sprite bawaan Unity.
    /// </summary>
    [CreateAssetMenu(menuName = "Prambanan AR/UI Theme", fileName = "UITheme")]
    public class UITheme : ScriptableObject
    {
        [Header("Font (TextMeshPro)")]
        public TMP_FontAsset font;
        public TMP_FontAsset titleFont;
        [Tooltip("Material titleFont bergaris tepi gelap, untuk teks di atas gambar/tombol berwarna.")]
        public Material titleOutlineMaterial;

        [Header("Panel & Frame")]
        public Sprite panelPopup;
        public Sprite panelCard;
        public Sprite panelHeader;
        public Sprite topBar;
        public Sprite bottomBar;
        public Sprite dialogBox;
        public Sprite nameTag;
        public Sprite chip;

        [Header("Tombol")]
        public Sprite buttonPrimary;   // kuning/oranye (aksi utama)
        public Sprite buttonSecondary; // biru
        public Sprite buttonSuccess;   // hijau
        public Sprite buttonDanger;    // merah
        public Sprite buttonCircle;    // tombol bulat ikon

        [Header("Ikon")]
        public Sprite iconClose;
        public Sprite iconHelp;
        public Sprite iconBack;
        public Sprite iconMenu;
        public Sprite iconCamera;
        public Sprite iconMap;
        public Sprite iconCollection;
        public Sprite iconStory;
        public Sprite iconGuide;
        public Sprite iconNearby;
        public Sprite iconReset;
        public Sprite iconLock;
        public Sprite iconCheck;
        public Sprite iconStar;
        public Sprite iconCoin;
        public Sprite iconXP;
        public Sprite iconPlay;
        public Sprite iconNext;
        public Sprite iconPrev;

        [Header("Elemen Khusus")]
        public Sprite progressBarBg;
        public Sprite progressBarFill;
        public Sprite ring;           // lingkaran target AR & cincin progres koleksi
        public Sprite glow;           // cahaya di belakang lencana level
        public Sprite levelBadge;
        public Sprite mapPin;
        public Sprite mapPinTail;
        public Sprite playerMarker;
        public Sprite handPointer;    // ikon tangan petunjuk usap
        public Sprite toggleBox;
        public Sprite toggleCheck;
        public Sprite spotlight;      // sorot cahaya di layar dialog

        [Header("Konten")]
        public Sprite logo;
        public Sprite loadingBackground;
        public Sprite guidePortrait;   // karakter pemandu "Kak Pandu"

        [Header("Warna (nuansa batu candi & emas)")]
        public Color primary = new Color(0.95f, 0.70f, 0.24f);
        public Color secondary = new Color(0.18f, 0.62f, 0.70f);
        public Color success = new Color(0.36f, 0.67f, 0.29f);
        public Color danger = new Color(0.84f, 0.27f, 0.25f);
        public Color terracotta = new Color(0.85f, 0.41f, 0.23f);
        public Color panel = new Color(1f, 0.96f, 0.86f);
        public Color panelDark = new Color(0.17f, 0.11f, 0.07f, 0.9f);
        public Color stone = new Color(0.48f, 0.38f, 0.30f);
        public Color gold = new Color(1f, 0.82f, 0.30f);
        public Color textDark = new Color(0.24f, 0.16f, 0.10f);
        public Color textLight = Color.white;
        public Color mapBackground = new Color(0.49f, 0.75f, 0.35f);
        public Color overlayDim = new Color(0f, 0f, 0f, 0.6f);
        [Tooltip("Warna stabilo kalimat yang sedang dibacakan (mode Audio).")]
        public Color readAlongHighlight = new Color(1f, 0.85f, 0.35f, 0.55f);
    }
}
