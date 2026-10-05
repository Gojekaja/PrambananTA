using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>
    /// Efek mesin ketik memakai maxVisibleCharacters TMP (tanpa membuat string baru tiap frame).
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TypewriterText : MonoBehaviour
    {
        [SerializeField] private float charactersPerSecond = 35f;

        private TextMeshProUGUI label;
        private Coroutine routine;

        public bool IsTyping => routine != null;
        public event Action OnCompleted;

        private TextMeshProUGUI Label => label != null ? label : (label = GetComponent<TextMeshProUGUI>());

        public void Play(string text)
        {
            Stop();
            Label.text = text;
            Label.maxVisibleCharacters = 0;
            if (isActiveAndEnabled) routine = StartCoroutine(TypeRoutine());
            else Complete();
        }

        /// <summary>Tampilkan seluruh teks seketika (dipakai saat anak mengetuk layar).</summary>
        public void Complete()
        {
            Stop();
            Label.maxVisibleCharacters = int.MaxValue;
            OnCompleted?.Invoke();
        }

        private void Stop()
        {
            if (routine != null) StopCoroutine(routine);
            routine = null;
        }

        private void OnDisable() => routine = null;

        private IEnumerator TypeRoutine()
        {
            Label.ForceMeshUpdate();
            int total = Label.textInfo.characterCount;
            float visible = 0f;
            while (visible < total)
            {
                visible += charactersPerSecond * Time.unscaledDeltaTime;
                Label.maxVisibleCharacters = Mathf.Min(total, Mathf.FloorToInt(visible));
                yield return null;
            }
            routine = null;
            Label.maxVisibleCharacters = int.MaxValue;
            OnCompleted?.Invoke();
        }
    }
}
