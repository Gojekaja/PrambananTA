namespace PrambananAR.UI
{
    /// <summary>
    /// Identitas unik setiap layar/popup. Nilai angka sengaja ditulis eksplisit
    /// agar referensi di Inspector tidak bergeser bila enum ditambah.
    /// </summary>
    public enum UIScreenId
    {
        None = 0,

        // --- Layar penuh ---
        Loading = 1,
        Map = 2,
        ARCamera = 3,
        Collection = 4,
        Story = 5,
        Dialogue = 6,

        // --- Overlay / popup ---
        QuickMenu = 20,
        NearbyPanel = 21,
        ConsentPopup = 22,
        ZoneInfoPopup = 23,
        CharacterDetailPopup = 24,
        RewardPopup = 25,
        LevelUpPopup = 26
    }
}
