using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>
    /// Menyesuaikan RectTransform dengan Screen.safeArea agar UI tidak tertutup
    /// notch/punch-hole kamera. Hanya dihitung ulang saat ukuran layar berubah (tanpa Update).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        private Rect lastSafeArea;
        private Vector2Int lastScreen;

        private void OnEnable() => Apply();
        private void OnRectTransformDimensionsChange() => Apply();

        private void Apply()
        {
            Rect safe = Screen.safeArea;
            var screenSize = new Vector2Int(Screen.width, Screen.height);
            if (safe == lastSafeArea && screenSize == lastScreen) return;
            if (Screen.width <= 0 || Screen.height <= 0) return;

            lastSafeArea = safe;
            lastScreen = screenSize;

            // Konversi piksel safe area menjadi anchor 0..1
            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;
            min.x /= Screen.width; min.y /= Screen.height;
            max.x /= Screen.width; max.y /= Screen.height;

            var rt = (RectTransform)transform;
            rt.anchorMin = min;
            rt.anchorMax = max;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
