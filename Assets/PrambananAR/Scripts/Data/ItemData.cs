using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>Benda yang bisa diberikan pemain kepada tokoh (keris, bunga, lesung, ...).</summary>
    [CreateAssetMenu(menuName = "Prambanan AR/Item Data", fileName = "Item_")]
    public class ItemData : ScriptableObject
    {
        public string id;
        public string displayName;
        [TextArea(2, 4)] public string description;
        [Tooltip("Sprite benda (mis. dari Art/item.psb). Dipakai di tas benda dan saat diberikan ke tokoh.")]
        public Sprite icon;
    }
}
