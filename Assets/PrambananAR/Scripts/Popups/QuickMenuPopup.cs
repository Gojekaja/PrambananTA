using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>
    /// Menu cepat (overlay gradasi) berisi daftar menu rata kanan yang muncul bertahap.
    /// </summary>
    public class QuickMenuPopup : UIPopup
    {
        [SerializeField] private Button collectionButton;
        [SerializeField] private Button storyButton;
        [SerializeField] private Button guideButton;
        [SerializeField] private Button arButton;
        [SerializeField] private RectTransform[] items;
        [SerializeField] private DialogueData guideDialogue;
        [SerializeField] private float stagger = 0.06f;

        protected override void Awake()
        {
            base.Awake();
            if (collectionButton != null) collectionButton.onClick.AddListener(() => UIManager.Instance.Show(UIScreenId.Collection));
            if (storyButton != null) storyButton.onClick.AddListener(() => UIManager.Instance.Show(UIScreenId.Story));
            if (arButton != null) arButton.onClick.AddListener(() => UIManager.Instance.Show(UIScreenId.ARCamera));
            if (guideButton != null) guideButton.onClick.AddListener(PlayGuide);
        }

        protected override void OnShow()
        {
            if (items == null) return;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] == null) continue;
                items[i].localScale = Vector3.zero;
                StartCoroutine(PopDelayed(items[i], (items.Length - 1 - i) * stagger)); // dari bawah ke atas
            }
        }

        private static IEnumerator PopDelayed(Transform t, float delay)
        {
            if (delay > 0f) yield return new WaitForSecondsRealtime(delay);
            yield return UIAnim.Scale(t, Vector3.zero, Vector3.one, 0.25f);
        }

        private void PlayGuide()
        {
            var dialogue = UIManager.Instance.Get<DialogueScreen>(UIScreenId.Dialogue);
            if (dialogue != null && guideDialogue != null) dialogue.Play(guideDialogue);
        }
    }
}
