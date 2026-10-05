using TMPro;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>Satu baris rincian hadiah, mis. "Teman baru!   500 XP".</summary>
    public class RewardLineView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI labelText;
        [SerializeField] private TextMeshProUGUI valueText;

        public void Setup(RewardLine line)
        {
            if (labelText != null) labelText.text = line.label.ToUpperInvariant();
            if (valueText != null) valueText.text = $"{line.xp} XP";
        }
    }
}
