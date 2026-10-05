using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace PrambananAR.UI
{
    /// <summary>
    /// Area transparan pendeteksi usapan ke atas memakai EventSystem (tanpa Update).
    /// Butuh Graphic dengan Raycast Target aktif (mis. Image alpha 0) di objek yang sama.
    /// </summary>
    public class SwipeGestureArea : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Tooltip("Jarak minimal usapan, dalam proporsi tinggi layar.")]
        [SerializeField, Range(0.05f, 0.5f)] private float minDistance = 0.12f;
        [Tooltip("Seberapa lurus ke atas (1 = harus vertikal sempurna).")]
        [SerializeField, Range(0f, 1f)] private float minUpDot = 0.6f;

        /// <summary>Parameter: kekuatan usapan 0..1.</summary>
        public event Action<float> OnSwipeUp;

        private Vector2 startPos;

        public void OnBeginDrag(PointerEventData eventData) => startPos = eventData.position;

        public void OnDrag(PointerEventData eventData) { } // wajib ada agar Begin/EndDrag terpanggil

        public void OnEndDrag(PointerEventData eventData)
        {
            Vector2 delta = eventData.position - startPos;
            float distance = delta.magnitude / Mathf.Max(1, Screen.height);
            if (distance < minDistance) return;
            if (Vector2.Dot(delta.normalized, Vector2.up) < minUpDot) return;

            OnSwipeUp?.Invoke(Mathf.Clamp01(distance / 0.5f));
        }
    }
}
