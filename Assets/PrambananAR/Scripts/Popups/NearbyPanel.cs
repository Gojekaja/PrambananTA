using System.Collections.Generic;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>
    /// Panel "Di Sekitar": grid tokoh per zona (siluet bila belum ditemukan).
    /// Ketuk item -> panel tertutup dan peta bergeser ke zona tersebut.
    /// </summary>
    public class NearbyPanel : UIPopup
    {
        [SerializeField] private CharacterDatabase characterDatabase;
        [SerializeField] private ZoneDatabase zoneDatabase;
        [SerializeField] private CharacterCardView itemTemplate;

        private readonly List<CharacterCardView> items = new List<CharacterCardView>();

        protected override void OnShow()
        {
            if (characterDatabase == null || itemTemplate == null) return;
            itemTemplate.gameObject.SetActive(false);

            while (items.Count < characterDatabase.characters.Count)
            {
                var v = Instantiate(itemTemplate, itemTemplate.transform.parent);
                v.gameObject.SetActive(true);
                items.Add(v);
            }

            for (int i = 0; i < items.Count; i++)
            {
                var c = characterDatabase.characters[i];
                var zone = zoneDatabase != null ? zoneDatabase.Find(c.zoneId) : null;
                string subtitle = c.comingSoon ? "Segera hadir" : zone != null ? zone.displayName : c.zoneId;
                items[i].Setup(c, PlayerProgressService.IsUnlocked(c.id), subtitle, OnItemClicked);
            }
        }

        private void OnItemClicked(CharacterData character)
        {
            Close();
            var map = UIManager.Instance.Get<MapScreen>(UIScreenId.Map);
            if (map != null) map.FocusZone(character.zoneId);
        }
    }
}
