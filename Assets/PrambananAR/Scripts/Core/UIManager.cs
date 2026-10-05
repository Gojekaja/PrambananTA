using System;
using System.Collections.Generic;
using UnityEngine;

namespace PrambananAR.UI
{
    /// <summary>
    /// Pengatur navigasi layar (riwayat/back) dan tumpukan popup.
    /// Hanya satu layar penuh yang aktif; popup ditumpuk di atasnya.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [SerializeField] private UIScreenId startScreen = UIScreenId.Loading;
        [SerializeField] private List<UIScreen> screens = new List<UIScreen>();
        [Tooltip("CanvasGroup root UI, dipakai untuk menyembunyikan UI saat screenshot.")]
        [SerializeField] private CanvasGroup rootGroup;

        private readonly Dictionary<UIScreenId, UIScreen> lookup = new Dictionary<UIScreenId, UIScreen>();
        private readonly Stack<UIScreenId> history = new Stack<UIScreenId>();
        private readonly List<UIPopup> popupStack = new List<UIPopup>();
        private UIScreen current;

        public UIScreen Current => current;
        public bool HasOpenPopup => popupStack.Count > 0;
        public event Action<UIScreenId> OnScreenChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            foreach (var screen in screens)
            {
                if (screen == null) continue;
                if (lookup.ContainsKey(screen.Id))
                    Debug.LogWarning($"[UIManager] Screen ID ganda: {screen.Id} ({screen.name})", screen);
                lookup[screen.Id] = screen;
                screen.InitHidden();
            }
        }

        private void Start()
        {
            if (startScreen != UIScreenId.None) Show(startScreen, false);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        // Satu-satunya Update di sistem UI: cek tombol Back Android (sangat ringan)
        private void Update()
        {
            if (BackPressed()) HandleBack();
        }

        private static bool BackPressed()
        {
#if ENABLE_INPUT_SYSTEM
            var kb = UnityEngine.InputSystem.Keyboard.current;
            return kb != null && kb.escapeKey.wasPressedThisFrame;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }

        // ------------------------------------------------------------------
        // Akses layar
        // ------------------------------------------------------------------
        public T Get<T>(UIScreenId id) where T : UIScreen
            => lookup.TryGetValue(id, out var s) ? s as T : null;

        // ------------------------------------------------------------------
        // Navigasi layar penuh
        // ------------------------------------------------------------------
        public void Show(UIScreenId id, bool addToHistory = true)
        {
            if (!lookup.TryGetValue(id, out var target))
            {
                Debug.LogWarning($"[UIManager] Screen {id} belum terdaftar di daftar 'screens'.");
                return;
            }
            if (target.IsOverlay)
            {
                OpenPopup(id);
                return;
            }

            CloseAllPopups();
            if (current == target) return;

            if (current != null)
            {
                if (addToHistory) history.Push(current.Id);
                current.Hide();
            }
            current = target;
            current.Show();
            OnScreenChanged?.Invoke(id);
        }

        /// <summary>Tampilkan layar dan hapus riwayat (mis. kembali ke Peta utama).</summary>
        public void ShowRoot(UIScreenId id)
        {
            history.Clear();
            Show(id, false);
        }

        public void Back()
        {
            if (popupStack.Count > 0)
            {
                ClosePopup(popupStack[popupStack.Count - 1]);
                return;
            }
            if (history.Count == 0) return;
            Show(history.Pop(), false);
        }

        // ------------------------------------------------------------------
        // Popup
        // ------------------------------------------------------------------
        public UIPopup OpenPopup(UIScreenId id)
        {
            if (!lookup.TryGetValue(id, out var s) || !(s is UIPopup popup))
            {
                Debug.LogWarning($"[UIManager] Popup {id} tidak ditemukan.");
                return null;
            }
            if (popupStack.Contains(popup)) return popup;

            popupStack.Add(popup);
            popup.transform.SetAsLastSibling(); // popup terbaru selalu paling atas
            popup.Show();
            return popup;
        }

        public void ClosePopup(UIPopup popup)
        {
            if (popup == null || !popupStack.Remove(popup)) return;
            popup.Hide();
        }

        public void CloseAllPopups()
        {
            // Salin dulu: callback onClosed bisa saja membuka popup baru
            var snapshot = popupStack.ToArray();
            for (int i = snapshot.Length - 1; i >= 0; i--) ClosePopup(snapshot[i]);
        }

        private void HandleBack()
        {
            if (popupStack.Count > 0)
            {
                popupStack[popupStack.Count - 1].HandleBack();
                return;
            }
            if (current != null && current.HandleBack()) return;
            Back();
        }

        /// <summary>Sembunyikan/tampilkan seluruh UI (mis. saat mengambil foto AR).</summary>
        public void SetUIVisible(bool visible)
        {
            if (rootGroup != null) rootGroup.alpha = visible ? 1f : 0f;
        }
    }
}
