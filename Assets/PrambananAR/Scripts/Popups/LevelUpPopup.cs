using TMPro;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>Perayaan naik level: lencana angka besar dengan cahaya berputar.</summary>
    public class LevelUpPopup : UIPopup
    {
        [SerializeField] private RectTransform badge;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI rewardText;

        private int level;
        private int coins;

        public void Setup(int newLevel, int coinBonus)
        {
            level = newLevel;
            coins = coinBonus;
        }

        protected override void OnShow()
        {
            if (levelText != null) levelText.text = level.ToString();
            if (rewardText != null) rewardText.text = $"Hadiah: +{coins} Koin";
            if (badge != null) StartCoroutine(UIAnim.Scale(badge, Vector3.zero, Vector3.one, 0.5f));
        }
    }
}
