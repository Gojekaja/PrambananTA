using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>
    /// Pustaka koleksi tokoh: cincin progres, grid kartu (siluet bila terkunci),
    /// dan baris hadiah koleksi.
    /// </summary>
    public class CollectionScreen : UIScreen
    {
        [SerializeField] private CharacterDatabase database;
        [SerializeField] private ZoneDatabase zoneDatabase;
        [SerializeField] private Button backButton;

        [Header("Progres")]
        [SerializeField] private Image progressRing;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private TextMeshProUGUI subtitleText;

        [Header("Template (dibiarkan nonaktif di Hierarchy)")]
        [SerializeField] private CharacterCardView cardTemplate;
        [SerializeField] private MilestoneView milestoneTemplate;

        private readonly List<CharacterCardView> cards = new List<CharacterCardView>();
        private readonly List<MilestoneView> milestoneViews = new List<MilestoneView>();

        private void Awake()
        {
            if (backButton != null) backButton.onClick.AddListener(() => UIManager.Instance.Back());
        }

        protected override void OnShow()
        {
            if (database == null) return;
            EnsureViews();

            int unlocked = database.CountUnlocked();
            int total = database.characters.Count;

            for (int i = 0; i < cards.Count; i++)
            {
                var c = database.characters[i];
                bool isUnlocked = PlayerProgressService.IsUnlocked(c.id);
                string subtitle = isUnlocked ? c.title : c.comingSoon ? "Segera hadir" : $"Zona {c.zoneId}";
                cards[i].Setup(c, isUnlocked, subtitle, OnCardClicked);
            }
            for (int i = 0; i < milestoneViews.Count; i++)
                milestoneViews[i].Setup(database.milestones[i], unlocked);

            if (progressText != null) progressText.text = $"{unlocked}<size=60%>/{total}</size>";
            if (subtitleText != null) subtitleText.text = $"Temukan {total} tokoh legenda Roro Jonggrang di area candi.";
            if (progressRing != null)
                StartCoroutine(UIAnim.FillTo(progressRing, 0f, total > 0 ? (float)unlocked / total : 0f, 0.8f));
        }

        // Buat view sekali saja, lalu dipakai ulang (hindari Instantiate berulang)
        private void EnsureViews()
        {
            if (cardTemplate != null)
            {
                cardTemplate.gameObject.SetActive(false);
                while (cards.Count < database.characters.Count)
                {
                    var card = Instantiate(cardTemplate, cardTemplate.transform.parent);
                    card.gameObject.SetActive(true);
                    cards.Add(card);
                }
            }
            if (milestoneTemplate != null)
            {
                milestoneTemplate.gameObject.SetActive(false);
                while (milestoneViews.Count < database.milestones.Count)
                {
                    var m = Instantiate(milestoneTemplate, milestoneTemplate.transform.parent);
                    m.gameObject.SetActive(true);
                    milestoneViews.Add(m);
                }
            }
        }

        private void OnCardClicked(CharacterData character)
        {
            if (character.comingSoon)
            {
                ToastNotifier.Show("Tokoh ini segera hadir. Nantikan, ya!");
                return;
            }
            if (!PlayerProgressService.IsUnlocked(character.id))
            {
                var zone = zoneDatabase != null ? zoneDatabase.Find(character.zoneId) : null;
                ToastNotifier.Show($"Belum ditemukan.\nCari di {(zone != null ? zone.displayName : "area candi")}!");
                return;
            }
            var ui = UIManager.Instance;
            var popup = ui.Get<CharacterDetailPopup>(UIScreenId.CharacterDetailPopup);
            if (popup == null) return;
            popup.Setup(character);
            ui.OpenPopup(UIScreenId.CharacterDetailPopup);
        }
    }
}
