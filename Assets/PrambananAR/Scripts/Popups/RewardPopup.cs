using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>
    /// Ringkasan hadiah setelah berteman dengan tokoh: koin, rincian XP, dan total.
    /// Tombol OK di-bind ke field 'closeButton'.
    /// </summary>
    public class RewardPopup : UIPopup
    {
        [SerializeField] private Image characterImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI totalText;
        [SerializeField] private RewardLineView lineTemplate;

        private readonly List<RewardLineView> lineViews = new List<RewardLineView>();
        private readonly List<RewardLine> lines = new List<RewardLine>();
        private CharacterData character;
        private int coins;

        public void Setup(CharacterData character, List<RewardLine> rewardLines, int coins)
        {
            this.character = character;
            this.coins = coins;
            lines.Clear();
            lines.AddRange(rewardLines);
        }

        protected override void OnShow()
        {
            if (titleText != null) titleText.text = character != null ? $"{character.displayName} jadi temanmu!" : "Hadiah!";
            if (characterImage != null)
            {
                characterImage.sprite = character != null ? character.portrait : null;
                characterImage.enabled = characterImage.sprite != null;
            }
            if (coinText != null) coinText.text = $"+{coins}";

            // Pool baris hadiah
            if (lineTemplate != null)
            {
                lineTemplate.gameObject.SetActive(false);
                while (lineViews.Count < lines.Count)
                    lineViews.Add(Instantiate(lineTemplate, lineTemplate.transform.parent));
            }

            int total = 0;
            for (int i = 0; i < lineViews.Count; i++)
            {
                bool used = i < lines.Count;
                lineViews[i].gameObject.SetActive(used);
                if (!used) continue;
                lineViews[i].Setup(lines[i]);
                total += lines[i].xp;
            }
            if (totalText != null)
            {
                totalText.text = $"TOTAL <color=#D9693A>{total} XP</color>";
                StartCoroutine(UIAnim.Punch(totalText.transform, 0.2f, 0.35f));
            }
        }
    }
}
