using System.Collections;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>
    /// Animasi "berdenyut" (pin peta, penanda pemain, tombol penting).
    /// Coroutine otomatis berhenti saat objek nonaktif, jadi tidak membebani layar tersembunyi.
    /// </summary>
    public class PulseScale : MonoBehaviour
    {
        [SerializeField] private float amplitude = 0.08f;
        [SerializeField] private float speed = 3f;
        [SerializeField] private bool randomPhase = true;

        private Vector3 baseScale = Vector3.one;

        private void OnEnable()
        {
            baseScale = Vector3.one;
            StartCoroutine(Loop());
        }

        private void OnDisable() => transform.localScale = baseScale;

        private IEnumerator Loop()
        {
            float phase = randomPhase ? Random.value * Mathf.PI * 2f : 0f;
            while (true)
            {
                float s = 1f + Mathf.Sin(Time.unscaledTime * speed + phase) * amplitude;
                transform.localScale = baseScale * s;
                yield return null;
            }
        }
    }
}
