using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>Ikon hadiah koleksi (terkunci sampai jumlah karakter tercapai).</summary>
    public class MilestoneView : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI requirementText;
        [SerializeField] private GameObject lockOverlay;

        public void Setup(CollectionMilestone milestone, int unlockedCount)
        {
            bool achieved = unlockedCount >= milestone.requiredCount;
            if (icon != null)
            {
                if (milestone.icon != null) icon.sprite = milestone.icon;
                icon.color = achieved ? Color.white : new Color(1f, 1f, 1f, 0.45f);
            }
            if (nameText != null) nameText.text = milestone.rewardName;
            if (requirementText != null)
                requirementText.text = achieved ? "Didapat!" : $"{milestone.requiredCount} teman";
            if (lockOverlay != null) lockOverlay.SetActive(!achieved);
        }
    }
}
