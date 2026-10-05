using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>
    /// Kartu karakter pada layar Koleksi. Dipakai juga oleh panel "Di Sekitar"
    /// (subtitle diisi nama zona).
    /// </summary>
    public class CharacterCardView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image portrait;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject foundBadge;
        [Tooltip("Pakai CharacterData.icon (kepala) bila ada — untuk bingkai bulat.")]
        [SerializeField] private bool preferIcon;

        private CharacterData data;
        private Action<CharacterData> onClick;

        private void Awake()
        {
            if (button != null) button.onClick.AddListener(() => onClick?.Invoke(data));
        }

        public void Setup(CharacterData character, bool unlocked, string subtitle, Action<CharacterData> onClick)
        {
            data = character;
            this.onClick = onClick;

            if (portrait != null)
            {
                portrait.sprite = preferIcon && character.icon != null ? character.icon : character.portrait;
                portrait.enabled = portrait.sprite != null;
                portrait.color = unlocked ? Color.white : new Color(0.12f, 0.12f, 0.18f, 0.85f);
            }
            if (nameText != null) nameText.text = unlocked ? character.displayName : "???";
            if (subtitleText != null) subtitleText.text = subtitle;
            if (lockIcon != null) lockIcon.SetActive(!unlocked);
            if (foundBadge != null) foundBadge.SetActive(unlocked);
        }
    }
}
