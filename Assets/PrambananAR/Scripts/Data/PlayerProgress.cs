using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>Data progres pemain (disimpan lokal, tanpa basis data online).</summary>
    [Serializable]
    public class PlayerProgress
    {
        public int level = 1;
        public int xp;
        public int coins;
        public int storyPagesRead;
        public bool consentGiven;
        public bool introSeen;
        public List<string> unlockedCharacters = new List<string>();
    }

    /// <summary>
    /// Layanan progres berbasis PlayerPrefs (JSON). UI cukup berlangganan OnChanged
    /// sehingga tidak perlu polling di Update().
    /// </summary>
    public static class PlayerProgressService
    {
        private const string SaveKey = "prambanan_progress_v1";
        private static PlayerProgress data;

        public static event Action OnChanged;

        public static PlayerProgress Data
        {
            get
            {
                if (data == null) Load();
                return data;
            }
        }

        // Reset static saat "Enter Play Mode Options" (domain reload) dimatikan
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            data = null;
            OnChanged = null;
        }

        /// <summary>XP yang dibutuhkan untuk naik dari level tertentu.</summary>
        public static int XpToNextLevel(int level) => 500 + (level - 1) * 250;

        public static void Load()
        {
            string json = PlayerPrefs.GetString(SaveKey, string.Empty);
            data = string.IsNullOrEmpty(json) ? new PlayerProgress() : JsonUtility.FromJson<PlayerProgress>(json);
            if (data.unlockedCharacters == null) data.unlockedCharacters = new List<string>();
        }

        public static void Save()
        {
            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(Data));
            PlayerPrefs.Save();
        }

        public static bool IsUnlocked(string characterId) =>
            !string.IsNullOrEmpty(characterId) && Data.unlockedCharacters.Contains(characterId);

        /// <summary>Return true jika karakter BARU pertama kali didapat.</summary>
        public static bool UnlockCharacter(string characterId)
        {
            if (string.IsNullOrEmpty(characterId) || IsUnlocked(characterId)) return false;
            Data.unlockedCharacters.Add(characterId);
            SaveAndNotify();
            return true;
        }

        /// <summary>Tambah XP; return jumlah level yang naik (0 jika tidak naik level).</summary>
        public static int AddXp(int amount)
        {
            int levelsGained = 0;
            Data.xp += Mathf.Max(0, amount);
            while (Data.xp >= XpToNextLevel(Data.level))
            {
                Data.xp -= XpToNextLevel(Data.level);
                Data.level++;
                levelsGained++;
            }
            SaveAndNotify();
            return levelsGained;
        }

        public static void AddCoins(int amount)
        {
            Data.coins = Mathf.Max(0, Data.coins + amount);
            SaveAndNotify();
        }

        public static void MarkStoryPageRead(int pageIndex)
        {
            if (pageIndex + 1 <= Data.storyPagesRead) return;
            Data.storyPagesRead = pageIndex + 1;
            SaveAndNotify();
        }

        public static void SetConsent(bool value) { Data.consentGiven = value; SaveAndNotify(); }
        public static void SetIntroSeen() { Data.introSeen = true; SaveAndNotify(); }

        /// <summary>Hapus semua progres (untuk pengujian / uji pengguna berikutnya).</summary>
        public static void ResetAll()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            data = new PlayerProgress();
            OnChanged?.Invoke();
        }

        private static void SaveAndNotify()
        {
            Save();
            OnChanged?.Invoke();
        }
    }
}
