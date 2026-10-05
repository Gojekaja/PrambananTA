using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>
    /// Bar atas: lencana level, bar XP, dan koin. Diperbarui lewat event (tanpa Update).
    /// </summary>
    public class PlayerHUD : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image xpFill;
        [SerializeField] private TextMeshProUGUI xpText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI playerNameText;

        private void OnEnable()
        {
            PlayerProgressService.OnChanged += Refresh;
            Refresh();
        }

        private void OnDisable() => PlayerProgressService.OnChanged -= Refresh;

        private void Refresh()
        {
            var d = PlayerProgressService.Data;
            int need = PlayerProgressService.XpToNextLevel(d.level);

            if (levelText != null) levelText.text = d.level.ToString();
            if (xpFill != null) xpFill.fillAmount = need > 0 ? (float)d.xp / need : 0f;
            if (xpText != null) xpText.text = $"{d.xp}/{need}";
            if (coinText != null) coinText.text = d.coins.ToString();
            if (playerNameText != null) playerNameText.text = "Penjelajah Cilik";
        }
    }
}
