using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrambananAR.UI
{
    [Serializable]
    public class CollectionMilestone
    {
        public string rewardName;
        public int requiredCount = 1;
        public Sprite icon;
    }

    [CreateAssetMenu(menuName = "Prambanan AR/Character Database", fileName = "CharacterDatabase")]
    public class CharacterDatabase : ScriptableObject
    {
        public List<CharacterData> characters = new List<CharacterData>();
        public List<CollectionMilestone> milestones = new List<CollectionMilestone>();

        public CharacterData Find(string id)
        {
            for (int i = 0; i < characters.Count; i++)
                if (characters[i] != null && characters[i].id == id) return characters[i];
            return null;
        }

        public int CountUnlocked()
        {
            int n = 0;
            for (int i = 0; i < characters.Count; i++)
                if (characters[i] != null && PlayerProgressService.IsUnlocked(characters[i].id)) n++;
            return n;
        }

        /// <summary>Karakter pertama yang belum ditemukan dan sudah bisa ditemui (untuk misi aktif).</summary>
        public CharacterData FirstLocked()
        {
            for (int i = 0; i < characters.Count; i++)
            {
                var c = characters[i];
                if (c != null && !c.comingSoon && !PlayerProgressService.IsUnlocked(c.id)) return c;
            }
            return null;
        }

        /// <summary>Karakter pertama yang sudah bisa ditemui (tidak berstatus "segera hadir").</summary>
        public CharacterData FirstAvailable()
        {
            for (int i = 0; i < characters.Count; i++)
                if (characters[i] != null && !characters[i].comingSoon) return characters[i];
            return null;
        }

        public bool HasComingSoon()
        {
            for (int i = 0; i < characters.Count; i++)
                if (characters[i] != null && characters[i].comingSoon) return true;
            return false;
        }
    }
}
