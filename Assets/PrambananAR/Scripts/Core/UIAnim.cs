using System.Collections;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>
    /// Kumpulan coroutine animasi UI ringan (tanpa DOTween).
    /// Memakai unscaledDeltaTime agar tetap berjalan saat Time.timeScale = 0.
    /// </summary>
    public static class UIAnim
    {
        public static float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);

        // Efek "membal" khas UI kartun (sedikit melewati target lalu kembali)
        public static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }

        public static IEnumerator Fade(CanvasGroup group, float from, float to, float duration)
        {
            if (group == null) yield break;
            float t = 0f;
            group.alpha = from;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, EaseOutCubic(Mathf.Clamp01(t / duration)));
                yield return null;
            }
            group.alpha = to;
        }

        public static IEnumerator Scale(Transform target, Vector3 from, Vector3 to, float duration, bool overshoot = true)
        {
            if (target == null) yield break;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                float e = overshoot ? EaseOutBack(k) : EaseOutCubic(k);
                target.localScale = Vector3.LerpUnclamped(from, to, e);
                yield return null;
            }
            target.localScale = to;
        }

        /// <summary>Efek "punch": membesar sebentar lalu kembali ke ukuran awal.</summary>
        public static IEnumerator Punch(Transform target, float strength = 0.2f, float duration = 0.25f)
        {
            if (target == null) yield break;
            Vector3 baseScale = Vector3.one;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                // Kurva sinus 0 -> 1 -> 0 untuk membesar lalu mengecil
                target.localScale = baseScale * (1f + Mathf.Sin(k * Mathf.PI) * strength);
                yield return null;
            }
            target.localScale = baseScale;
        }

        public static IEnumerator Move(RectTransform target, Vector2 from, Vector2 to, float duration)
        {
            if (target == null) yield break;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                target.anchoredPosition = Vector2.LerpUnclamped(from, to, EaseOutBack(Mathf.Clamp01(t / duration)));
                yield return null;
            }
            target.anchoredPosition = to;
        }

        public static IEnumerator FillTo(UnityEngine.UI.Image image, float from, float to, float duration)
        {
            if (image == null) yield break;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                image.fillAmount = Mathf.Lerp(from, to, EaseOutCubic(Mathf.Clamp01(t / duration)));
                yield return null;
            }
            image.fillAmount = to;
        }
    }
}
