using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>
    /// Layar pemuatan: bar progres + tips bergilir.
    /// Setelah selesai: persetujuan orang tua (sekali) -> Peta -> dialog pengenalan (sekali).
    /// </summary>
    public class LoadingScreen : UIScreen
    {
        [SerializeField] private Image progressFill;
        [SerializeField] private TextMeshProUGUI percentText;
        [SerializeField] private TextMeshProUGUI tipText;
        [SerializeField] private DialogueData introDialogue;
        [SerializeField] private float minDuration = 2.5f;
        [SerializeField] private float tipInterval = 2f;
        [SerializeField, TextArea] private string[] tips =
        {
            "Selalu didampingi Ayah, Bunda, atau Guru saat bermain, ya!",
            "Perhatikan jalan di sekitarmu ketika memegang ponsel.",
            "Kunjungi titik bertanda di peta untuk bertemu tokoh cerita.",
            "Candi Prambanan dibangun pada abad ke-9 Masehi.",
            "Pilih benda yang tepat, lalu usap layar ke atas untuk memberikannya kepada tokoh!"
        };

        protected override void OnShow()
        {
            StartCoroutine(LoadRoutine());
        }

        private IEnumerator LoadRoutine()
        {
            float t = 0f;
            float nextTip = 0f;
            int tipIndex = Random.Range(0, Mathf.Max(1, tips.Length));

            // Simulasi pemuatan. Ganti dengan AsyncOperation bila memuat scene/asset sungguhan.
            while (t < minDuration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / minDuration);
                if (progressFill != null) progressFill.fillAmount = p;
                if (percentText != null) percentText.text = $"{Mathf.RoundToInt(p * 100)}%";

                if (t >= nextTip && tips.Length > 0 && tipText != null)
                {
                    tipText.text = tips[tipIndex % tips.Length];
                    tipIndex++;
                    nextTip = t + tipInterval;
                }
                yield return null;
            }
            OnLoadFinished();
        }

        private void OnLoadFinished()
        {
            var ui = UIManager.Instance;
            if (!PlayerProgressService.Data.consentGiven)
            {
                var consent = ui.Get<ConsentPopup>(UIScreenId.ConsentPopup);
                if (consent != null)
                {
                    consent.SetOnAccepted(GoToMap);
                    ui.OpenPopup(UIScreenId.ConsentPopup);
                    return;
                }
            }
            GoToMap();
        }

        private void GoToMap()
        {
            var ui = UIManager.Instance;
            ui.ShowRoot(UIScreenId.Map);

            if (PlayerProgressService.Data.introSeen || introDialogue == null) return;
            var dialogue = ui.Get<DialogueScreen>(UIScreenId.Dialogue);
            if (dialogue == null) return;

            dialogue.Play(introDialogue, () =>
            {
                PlayerProgressService.SetIntroSeen();
                UIManager.Instance.ShowRoot(UIScreenId.Map);
            });
        }

        // Layar loading tidak bisa di-"back"
        public override bool HandleBack() => true;
    }
}
