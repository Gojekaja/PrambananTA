using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>Pin zona di peta. Menampilkan siluet karakter bila belum ditemukan.</summary>
    public class MapZonePin : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image pinImage;
        [SerializeField] private Image tailImage;
        [SerializeField] private Image characterImage;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private GameObject foundBadge;

        private MapZone zone;
        private CharacterData character;
        private Action<MapZone> onClick;

        public MapZone Zone => zone;

        private void Awake()
        {
            if (button != null) button.onClick.AddListener(() => onClick?.Invoke(zone));
        }

        public void Setup(MapZone zone, CharacterData character, Color pinColor, Action<MapZone> onClick)
        {
            this.zone = zone;
            this.character = character;
            this.onClick = onClick;

            // Tempatkan pin memakai anchor = posisi normal pada gambar peta
            var rt = (RectTransform)transform;
            rt.anchorMin = rt.anchorMax = zone.mapPosition;
            rt.anchoredPosition = Vector2.zero;

            if (pinImage != null) pinImage.color = pinColor;
            if (tailImage != null) tailImage.color = pinColor;
            if (label != null) label.text = zone.id;
            Refresh();
        }

        public void Refresh()
        {
            bool hasCharacter = character != null;
            bool unlocked = hasCharacter && PlayerProgressService.IsUnlocked(character.id);

            if (characterImage != null)
            {
                var sprite = hasCharacter ? (character.icon != null ? character.icon : character.portrait) : null;
                characterImage.gameObject.SetActive(sprite != null);
                if (hasCharacter)
                {
                    characterImage.sprite = sprite;
                    // Belum ditemukan = siluet hitam (memancing rasa penasaran anak)
                    characterImage.color = unlocked ? Color.white : new Color(0.1f, 0.1f, 0.15f, 0.9f);
                }
            }
            if (foundBadge != null) foundBadge.SetActive(unlocked);
        }

        public void Highlight()
        {
            if (isActiveAndEnabled) StartCoroutine(UIAnim.Punch(transform, 0.35f, 0.4f));
        }
    }
}
