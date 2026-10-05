using System.Collections;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>
    /// Kelas dasar semua layar. Layar yang tersembunyi di-SetActive(false)
    /// sehingga tidak ikut dirender maupun di-raycast (hemat untuk mobile).
    ///
    /// Pola wajib untuk turunan:
    /// - Pasang listener tombol di Awake().
    /// - Method Setup(...) hanya MENYIMPAN data; render data di OnShow().
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class UIScreen : MonoBehaviour
    {
        [SerializeField] private UIScreenId screenId;
        [SerializeField, Min(0f)] protected float transitionDuration = 0.2f;

        private CanvasGroup group;
        private Coroutine transition;

        public UIScreenId Id => screenId;
        public bool IsVisible { get; private set; }

        /// <summary>True untuk popup/overlay yang tampil DI ATAS layar aktif.</summary>
        public virtual bool IsOverlay => false;

        // Lazy getter: aman dipanggil walaupun Awake() belum berjalan
        protected CanvasGroup Group => group != null ? group : (group = GetComponent<CanvasGroup>());

        /// <summary>Dipanggil UIManager saat start: sembunyikan tanpa memicu OnHide().</summary>
        internal void InitHidden()
        {
            IsVisible = false;
            Group.alpha = 0f;
            gameObject.SetActive(false);
        }

        public void Show()
        {
            if (transition != null) StopCoroutine(transition);

            gameObject.SetActive(true); // Awake() turunan berjalan di sini (jika belum)
            IsVisible = true;
            Group.interactable = true;
            Group.blocksRaycasts = true;

            OnShow();
            transition = StartCoroutine(AnimateIn());
        }

        public void Hide(bool instant = false)
        {
            if (!gameObject.activeSelf)
            {
                IsVisible = false;
                return;
            }

            IsVisible = false;
            Group.interactable = false;
            Group.blocksRaycasts = false;
            OnHide();

            if (transition != null) StopCoroutine(transition);

            if (instant || !gameObject.activeInHierarchy)
            {
                Group.alpha = 0f;
                gameObject.SetActive(false);
                return;
            }
            transition = StartCoroutine(HideRoutine());
        }

        private IEnumerator HideRoutine()
        {
            yield return AnimateOut();
            transition = null;
            gameObject.SetActive(false);
        }

        protected virtual IEnumerator AnimateIn() => UIAnim.Fade(Group, 0f, 1f, transitionDuration);
        protected virtual IEnumerator AnimateOut() => UIAnim.Fade(Group, Group.alpha, 0f, transitionDuration);

        protected virtual void OnShow() { }
        protected virtual void OnHide() { }

        /// <summary>Tombol Back Android. Return true jika layar sudah menanganinya sendiri.</summary>
        public virtual bool HandleBack() => false;
    }
}
