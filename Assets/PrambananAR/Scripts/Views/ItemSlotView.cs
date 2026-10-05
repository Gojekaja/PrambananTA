using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>Satu slot benda di tas (layar Kamera AR). Ketuk untuk memilih.</summary>
    public class ItemSlotView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private GameObject selectedFrame;

        private Action<ItemSlotView> onClick;

        public ItemData Item { get; private set; }
        public Image IconImage => icon;

        private void Awake()
        {
            if (button != null) button.onClick.AddListener(() => onClick?.Invoke(this));
        }

        public void Setup(ItemData item, Action<ItemSlotView> onClick)
        {
            Item = item;
            this.onClick = onClick;
            if (icon != null)
            {
                icon.sprite = item.icon;
                icon.enabled = item.icon != null;
            }
            if (nameText != null) nameText.text = item.displayName;
            SetSelected(false);
        }

        public void SetSelected(bool selected)
        {
            if (selectedFrame != null) selectedFrame.SetActive(selected);
            if (selected && isActiveAndEnabled) StartCoroutine(UIAnim.Punch(transform, 0.15f, 0.25f));
        }
    }
}
