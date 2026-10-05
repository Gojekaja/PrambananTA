using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrambananAR.UI
{
    [Serializable]
    public class StoryPage
    {
        public string chapterTitle;
        public Sprite illustration;
        [Tooltip("Opsional: rekaman narasi halaman ini.")]
        public AudioClip narration;
        [Tooltip("Satu elemen = satu kalimat (dipakai untuk efek stabilo baca-bersama).")]
        [TextArea(2, 4)] public string[] sentences;
        [Tooltip("Durasi per kalimat bila narration kosong.")]
        public float secondsPerSentence = 3.5f;
    }

    [CreateAssetMenu(menuName = "Prambanan AR/Story Data", fileName = "Story_")]
    public class StoryData : ScriptableObject
    {
        public string title;
        public string subtitle;
        public Sprite cover;
        [TextArea(3, 6)] public string synopsis;
        public string[] tags;
        public List<StoryPage> pages = new List<StoryPage>();
    }
}
