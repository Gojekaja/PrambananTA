using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>
    /// Mengambil foto tampilan AR tanpa UI, lalu menyimpan PNG ke persistentDataPath.
    /// Untuk menyimpan ke Galeri Android gunakan plugin NativeGallery (yasirkula).
    /// </summary>
    public class ScreenshotService : MonoBehaviour
    {
        [SerializeField] private string filePrefix = "Prambanan_AR_";

        private bool busy;

        public void Capture(Action<string> onSaved)
        {
            if (busy) return;
            StartCoroutine(CaptureRoutine(onSaved));
        }

        private IEnumerator CaptureRoutine(Action<string> onSaved)
        {
            busy = true;
            var ui = UIManager.Instance;
            if (ui != null) ui.SetUIVisible(false);

            // Tunggu frame selesai dirender TANPA UI
            yield return new WaitForEndOfFrame();
            Texture2D tex = ScreenCapture.CaptureScreenshotAsTexture();

            if (ui != null) ui.SetUIVisible(true);

            string path = Path.Combine(Application.persistentDataPath,
                $"{filePrefix}{DateTime.Now:yyyyMMdd_HHmmss}.png");
            try
            {
                File.WriteAllBytes(path, tex.EncodeToPNG());
            }
            catch (Exception e)
            {
                Debug.LogError($"[Screenshot] Gagal menyimpan: {e.Message}");
                path = null;
            }
            finally
            {
                Destroy(tex); // penting: bebaskan memori tekstur
                busy = false;
            }
            onSaved?.Invoke(path);
        }
    }
}
