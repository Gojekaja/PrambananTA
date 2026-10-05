using System.Collections.Generic;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>Daftar benda di tas pemain (ditampilkan di layar kamera AR).</summary>
    [CreateAssetMenu(menuName = "Prambanan AR/Item Database", fileName = "ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemData> items = new List<ItemData>();

        public ItemData Find(string id)
        {
            for (int i = 0; i < items.Count; i++)
                if (items[i] != null && items[i].id == id) return items[i];
            return null;
        }
    }
}
