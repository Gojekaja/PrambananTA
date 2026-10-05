using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrambananAR.UI
{
    [Serializable]
    public class DialogueLine
    {
        public string speaker;
        [Tooltip("Kosongkan untuk memakai potret baris sebelumnya.")]
        public Sprite portrait;
        [TextArea(2, 5)] public string text;
    }

    [CreateAssetMenu(menuName = "Prambanan AR/Dialogue Data", fileName = "Dialogue_")]
    public class DialogueData : ScriptableObject
    {
        public List<DialogueLine> lines = new List<DialogueLine>();
    }
}
