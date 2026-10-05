using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>
    /// Cincin target yang mengecil berulang (terinspirasi UX lingkaran target).
    /// Semakin kecil cincin saat pemain mengusap, semakin tinggi nilai sapaan.
    /// </summary>
    public class ShrinkingRing : MonoBehaviour
    {
        [SerializeField] private Image ringImage;
        [SerializeField] private float maxScale = 1f;
        [SerializeField] private float minScale = 0.3f;
        [SerializeField] private float period = 1.8f;
        [SerializeField] private Color largeColor = new Color(0.4f, 0.85f, 0.4f);
        [SerializeField] private Color smallColor = new Color(1f, 0.75f, 0.15f);

        /// <summary>1 = cincin paling besar, 0 = paling kecil.</summary>
        public float Normalized { get; private set; } = 1f;

        private void OnEnable() => StartCoroutine(Loop());

        private IEnumerator Loop()
        {
            float t = 0f;
            while (true)
            {
                t += Time.unscaledDeltaTime;
                // Pola gergaji: mengecil perlahan lalu kembali besar
                Normalized = 1f - Mathf.Repeat(t, period) / period;
                float scale = Mathf.Lerp(minScale, maxScale, Normalized);
                transform.localScale = new Vector3(scale, scale, 1f);
                if (ringImage != null) ringImage.color = Color.Lerp(smallColor, largeColor, Normalized);
                yield return null;
            }
        }
    }
}
