using System.Collections;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>Memutar objek UI terus-menerus (cahaya di belakang lencana level, ikon loading).</summary>
    public class Rotator : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond = 30f;

        private void OnEnable() => StartCoroutine(Loop());

        private IEnumerator Loop()
        {
            while (true)
            {
                transform.Rotate(0f, 0f, -degreesPerSecond * Time.unscaledDeltaTime);
                yield return null;
            }
        }
    }
}
