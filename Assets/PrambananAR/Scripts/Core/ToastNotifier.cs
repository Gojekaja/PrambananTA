using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>
    /// Pesan singkat di tengah layar (mis. "Roro Jonggrang menjadi temanmu!").
    /// Objek ini harus tetap AKTIF (alpha 0 saat tidak tampil) agar coroutine bisa berjalan.
    /// </summary>
    public class ToastNotifier : MonoBehaviour
    {
        public static ToastNotifier Instance { get; private set; }

        [SerializeField] private CanvasGroup group;
        [SerializeField] private TextMeshProUGUI label;
        [SerializeField] private float holdDuration = 1.6f;

        private readonly Queue<string> queue = new Queue<string>();
        private Coroutine routine;

        private void Awake()
        {
            Instance = this;
            if (group != null)
            {
                group.alpha = 0f;
                group.blocksRaycasts = false; // toast tidak menghalangi sentuhan
                group.interactable = false;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Show(string message)
        {
            if (Instance != null) Instance.Enqueue(message);
            else Debug.Log($"[Toast] {message}");
        }

        private void Enqueue(string message)
        {
            queue.Enqueue(message);
            if (routine == null) routine = StartCoroutine(PlayQueue());
        }

        private IEnumerator PlayQueue()
        {
            while (queue.Count > 0)
            {
                label.text = queue.Dequeue();
                StartCoroutine(UIAnim.Punch(label.transform, 0.15f, 0.25f));
                yield return UIAnim.Fade(group, 0f, 1f, 0.15f);
                yield return new WaitForSecondsRealtime(holdDuration);
                yield return UIAnim.Fade(group, 1f, 0f, 0.2f);
            }
            routine = null;
        }
    }
}
