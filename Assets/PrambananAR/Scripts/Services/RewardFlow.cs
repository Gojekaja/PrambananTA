using System;
using System.Collections.Generic;

namespace PrambananAR.UI
{
    [Serializable]
    public struct RewardLine
    {
        public string label;
        public int xp;
        public RewardLine(string label, int xp) { this.label = label; this.xp = xp; }
    }

    /// <summary>
    /// Merangkai alur hadiah: simpan progres -> popup Hadiah -> (opsional) popup Naik Level.
    /// Dipisah dari layar AR agar bisa dipakai ulang oleh minigame lain.
    /// </summary>
    public static class RewardFlow
    {
        public const int XpNewCharacter = 500;
        public const int CoinsPerFind = 10;
        public const int CoinsPerLevel = 20;

        public static void GrantCharacter(CharacterData character, string gradeLabel, int bonusXp, Action onDone,
            IList<RewardLine> extraLines = null)
        {
            var ui = UIManager.Instance;
            bool isNew = PlayerProgressService.UnlockCharacter(character.id);

            var lines = new List<RewardLine> { new RewardLine("Menyapa karakter", character.baseXp) };
            if (isNew) lines.Add(new RewardLine("Teman baru!", XpNewCharacter));
            if (bonusXp > 0) lines.Add(new RewardLine($"Sapaan {gradeLabel}", bonusXp));
            if (extraLines != null) lines.AddRange(extraLines);

            int totalXp = 0;
            foreach (var l in lines) totalXp += l.xp;

            int levelsGained = PlayerProgressService.AddXp(totalXp);
            PlayerProgressService.AddCoins(CoinsPerFind);

            var reward = ui != null ? ui.Get<RewardPopup>(UIScreenId.RewardPopup) : null;
            if (reward == null) { onDone?.Invoke(); return; }

            reward.Setup(character, lines, CoinsPerFind);
            reward.SetOnClosed(() =>
            {
                if (levelsGained <= 0) { onDone?.Invoke(); return; }

                int bonusCoins = CoinsPerLevel * levelsGained;
                PlayerProgressService.AddCoins(bonusCoins);

                var levelUp = ui.Get<LevelUpPopup>(UIScreenId.LevelUpPopup);
                if (levelUp == null) { onDone?.Invoke(); return; }
                levelUp.Setup(PlayerProgressService.Data.level, bonusCoins);
                levelUp.SetOnClosed(onDone);
                ui.OpenPopup(UIScreenId.LevelUpPopup);
            });
            ui.OpenPopup(UIScreenId.RewardPopup);
        }
    }
}
