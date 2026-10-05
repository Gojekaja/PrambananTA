using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>
    /// Kelas dasar popup: latar gelap + panel yang muncul dengan efek "membal".
    /// Mendukung callback satu kali (SetOnClosed) untuk merangkai popup berurutan.
    /// </summary>
    public abstract class UIPopup : UIScreen
    {
        [SerializeField] protected RectTransform panel;
        [SerializeField] protected Button closeButton;
        [SerializeField] private bool closeOnBack = true;

        private Action onClosed;

        public override bool IsOverlay => true;

        protected virtual void Awake()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);
        }

        /// <summary>Callback dipanggil SEKALI ketika popup ditutup.</summary>
        public void SetOnClosed(Action callback) => onClosed = callback;

        public void Close()
        {
            if (UIManager.Instance != null) UIManager.Instance.ClosePopup(this);
            else Hide();
        }

        protected override void OnHide()
        {
            // Ambil lalu kosongkan dulu agar callback tidak terpanggil dua kali
            var callback = onClosed;
            onClosed = null;
            callback?.Invoke();
        }

        public override bool HandleBack()
        {
            if (closeOnBack) Close();
            return true; // popup selalu "menelan" tombol back
        }

        protected override IEnumerator AnimateIn()
        {
            float t = 0f;
            float d = Mathf.Max(0.01f, transitionDuration);
            Vector3 from = Vector3.one * 0.8f;
            while (t < d)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / d);
                Group.alpha = k;
                if (panel != null) panel.localScale = Vector3.LerpUnclamped(from, Vector3.one, UIAnim.EaseOutBack(k));
                yield return null;
            }
            Group.alpha = 1f;
            if (panel != null) panel.localScale = Vector3.one;
        }
    }
}
