using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>Info zona saat pin peta diketuk + tombol menuju kamera AR.</summary>
    public class ZoneInfoPopup : UIPopup
    {
        [SerializeField] private TextMeshProUGUI badgeText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private Image characterImage;
        [SerializeField] private Button startButton;
        [SerializeField] private TextMeshProUGUI startButtonLabel;

        private MapZone zone;
        private CharacterData character;

        protected override void Awake()
        {
            base.Awake();
            if (startButton != null) startButton.onClick.AddListener(StartAR);
        }

        public void Setup(MapZone zone, CharacterData character)
        {
            this.zone = zone;
            this.character = character;
        }

        protected override void OnShow()
        {
            if (zone == null) return;
            bool comingSoon = character != null && character.comingSoon;
            bool hasChar = character != null && !comingSoon;
            bool unlocked = hasChar && PlayerProgressService.IsUnlocked(character.id);

            if (badgeText != null) badgeText.text = zone.id;
            if (titleText != null) titleText.text = zone.displayName;
            if (descriptionText != null) descriptionText.text = zone.description;
            if (statusText != null)
            {
                string status = comingSoon ? "Tokoh di sini segera hadir!"
                              : !hasChar ? "Titik informasi"
                              : unlocked ? $"Teman: {character.displayName}"
                              : "Ada tokoh misterius di sini!";
                // Tampilkan jarak bila GPS sudah aktif
                var gps = GPSManager.Instance;
                if (gps != null && gps.LokasiAktif)
                    status += $"  |  {FormatDistance(gps.JarakKeZona(zone))}";
                statusText.text = status;
            }

            if (characterImage != null)
            {
                characterImage.gameObject.SetActive(character != null && character.portrait != null);
                if (character != null)
                {
                    characterImage.sprite = character.portrait;
                    characterImage.color = unlocked ? Color.white : new Color(0.1f, 0.1f, 0.15f, 0.9f);
                }
            }

            if (startButton != null) startButton.gameObject.SetActive(hasChar);
            if (startButtonLabel != null) startButtonLabel.text = unlocked ? "Sapa Lagi" : "Buka Kamera AR";
        }

        private static string FormatDistance(float meters) =>
            meters >= 1000f ? $"{meters / 1000f:0.0} km" : $"{Mathf.RoundToInt(meters)} m";

        private void StartAR()
        {
            // Geofence dicek di layar Kamera AR: tokoh baru muncul saat pemain masuk radius zona
            var ui = UIManager.Instance;
            var ar = ui.Get<ARCameraScreen>(UIScreenId.ARCamera);
            if (ar != null) ar.PrepareEncounter(character);
            ui.Show(UIScreenId.ARCamera);
        }
    }
}
