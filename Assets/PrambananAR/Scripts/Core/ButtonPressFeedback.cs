using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>
    /// Umpan balik "empuk" saat tombol ditekan (mengecil lalu membal),
    /// memberi kesan kartun dan jelas bagi anak usia dini.
    /// </summary>
    public class ButtonPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private float pressedScale = 0.9f;
        [SerializeField] private float duration = 0.12f;

        private Selectable selectable;
        private Coroutine routine;

        private void Awake() => selectable = GetComponent<Selectable>();

        private void OnDisable() => transform.localScale = Vector3.one;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (selectable != null && !selectable.IsInteractable()) return;
            Animate(Vector3.one * pressedScale, false);
        }

        public void OnPointerUp(PointerEventData eventData) => Animate(Vector3.one, true);

        private void Animate(Vector3 target, bool overshoot)
        {
            if (!isActiveAndEnabled) return;
            if (routine != null) StopCoroutine(routine);
            routine = StartCoroutine(Run(target, overshoot));
        }

        private IEnumerator Run(Vector3 target, bool overshoot)
        {
            yield return UIAnim.Scale(transform, transform.localScale, target, duration, overshoot);
            routine = null;
        }
    }
}
