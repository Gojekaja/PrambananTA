using System.Collections;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>Ikon tangan yang bergerak dari bawah ke atas sebagai petunjuk gestur usap.</summary>
    public class SwipeHintAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform hand;
        [SerializeField] private CanvasGroup handGroup;
        [SerializeField] private Vector2 startOffset = new Vector2(80f, -300f);
        [SerializeField] private Vector2 endOffset = new Vector2(40f, 150f);
        [SerializeField] private float moveDuration = 0.9f;
        [SerializeField] private float pause = 0.6f;

        private void OnEnable()
        {
            if (hand != null) StartCoroutine(Loop());
        }

        private IEnumerator Loop()
        {
            while (true)
            {
                float t = 0f;
                while (t < moveDuration)
                {
                    t += Time.unscaledDeltaTime;
                    float k = Mathf.Clamp01(t / moveDuration);
                    hand.anchoredPosition = Vector2.Lerp(startOffset, endOffset, UIAnim.EaseOutCubic(k));
                    // Muncul di awal, memudar di akhir gerakan
                    if (handGroup != null) handGroup.alpha = Mathf.Sin(k * Mathf.PI);
                    yield return null;
                }
                yield return new WaitForSecondsRealtime(pause);
            }
        }
    }
}
