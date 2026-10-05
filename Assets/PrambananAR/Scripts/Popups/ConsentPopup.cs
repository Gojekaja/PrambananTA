using System;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

namespace PrambananAR.UI
{
    /// <summary>
    /// Persetujuan orang tua/pendamping (anak usia dini) + permintaan izin Kamera & Lokasi.
    /// Tombol "Setuju" baru aktif setelah kotak centang dicentang.
    /// </summary>
    public class ConsentPopup : UIPopup
    {
        [SerializeField] private Toggle agreeToggle;
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button privacyButton;

        private Action onAccepted;

        protected override void Awake()
        {
            base.Awake();
            if (agreeToggle != null) agreeToggle.onValueChanged.AddListener(v =>
            {
                if (acceptButton != null) acceptButton.interactable = v;
            });
            if (acceptButton != null) acceptButton.onClick.AddListener(Accept);
            if (privacyButton != null) privacyButton.onClick.AddListener(() =>
                ToastNotifier.Show("Data lokasi hanya diproses di perangkat dan tidak dikirim ke server."));
        }

        public void SetOnAccepted(Action callback) => onAccepted = callback;

        protected override void OnShow()
        {
            if (agreeToggle != null) agreeToggle.isOn = false;
            if (acceptButton != null) acceptButton.interactable = false;
        }

        private void Accept()
        {
            PlayerProgressService.SetConsent(true);
            RequestPermissions();

            var callback = onAccepted;
            onAccepted = null;
            Close();
            callback?.Invoke();
        }

        private static void RequestPermissions()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            bool cam = Permission.HasUserAuthorizedPermission(Permission.Camera);
            bool loc = Permission.HasUserAuthorizedPermission(Permission.FineLocation);
            if (!cam || !loc)
                Permission.RequestUserPermissions(new[] { Permission.Camera, Permission.FineLocation });
#endif
        }

        // Wajib disetujui: tombol back tidak menutup popup ini
        public override bool HandleBack() => true;
    }
}
