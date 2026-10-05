using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrambananAR.UI
{
    [CreateAssetMenu(menuName = "Prambanan AR/Character Data", fileName = "Char_")]
    public class CharacterData : ScriptableObject
    {
        public string id;
        public string displayName;
        public string title;
        [TextArea(3, 6)] public string description;
        [Tooltip("Sprite 2D karakter. Gunakan PPU yang sama untuk semua karakter (mis. 100).")]
        public Sprite portrait;
        [Tooltip("Potret kepala & bahu untuk pin peta / bingkai bulat. Kosong = memakai portrait.")]
        public Sprite icon;
        [Tooltip("ID zona pada ZoneDatabase tempat karakter muncul.")]
        public string zoneId;
        public DialogueData encounterDialogue;
        public int baseXp = 100;

        [Tooltip("Centang bila model/potret tokoh belum siap. Tokoh tampil sebagai siluet 'Segera hadir' dan belum bisa ditemui.")]
        public bool comingSoon;

        [Header("Pemberian Benda (AR)")]
        [Tooltip("Benda yang diinginkan tokoh. Kosongkan bila tokoh cukup disapa dengan usapan.")]
        public ItemData wantedItem;
        [Tooltip("Petunjuk saat tokoh muncul, mis. \"Aku suka sesuatu yang harum...\"")]
        [TextArea(2, 3)] public string askLine;
        [TextArea(2, 3)] public string thanksLine;
        [TextArea(2, 3)] public string rejectLine;
    }
}
