using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>
    /// Dialog gaya visual novel: potret tokoh di bawah sorotan cahaya,
    /// kotak teks dengan efek mesin ketik, ketuk untuk lanjut, tombol Lewati.
    /// </summary>
    public class DialogueScreen : UIScreen
    {
        [SerializeField] private Image portraitImage;
        [SerializeField] private Sprite defaultPortrait;
        [SerializeField] private TextMeshProUGUI speakerText;
        [SerializeField] private TypewriterText typewriter;
        [SerializeField] private Button advanceButton;
        [SerializeField] private Button skipButton;
        [SerializeField] private GameObject nextIndicator;

        private DialogueData data;
        private Action onFinished;
        private int index;

        private void Awake()
        {
            if (advanceButton != null) advanceButton.onClick.AddListener(Advance);
            if (skipButton != null) skipButton.onClick.AddListener(Finish);
            if (typewriter != null) typewriter.OnCompleted += () =>
            {
                if (nextIndicator != null) nextIndicator.SetActive(true);
            };
        }

        /// <summary>Mainkan dialog. onFinished null = kembali ke layar sebelumnya.</summary>
        public void Play(DialogueData dialogue, Action onFinished = null)
        {
            data = dialogue;
            this.onFinished = onFinished;
            if (IsVisible) Restart();
            else UIManager.Instance.Show(UIScreenId.Dialogue);
        }

        protected override void OnShow() => Restart();

        private void Restart()
        {
            index = -1;
            if (portraitImage != null) portraitImage.sprite = defaultPortrait;
            NextLine();
        }

        private void NextLine()
        {
            index++;
            if (data == null || index >= data.lines.Count)
            {
                Finish();
                return;
            }

            var line = data.lines[index];
            if (speakerText != null) speakerText.text = line.speaker;

            if (portraitImage != null)
            {
                // Animasi "muncul" hanya ketika pembicara berganti
                if (line.portrait != null && portraitImage.sprite != line.portrait)
                {
                    portraitImage.sprite = line.portrait;
                    StartCoroutine(UIAnim.Scale(portraitImage.transform, Vector3.one * 0.85f, Vector3.one, 0.3f));
                }
                portraitImage.enabled = portraitImage.sprite != null;
            }

            if (nextIndicator != null) nextIndicator.SetActive(false);
            if (typewriter != null) typewriter.Play(line.text);
        }

        private void Advance()
        {
            if (typewriter != null && typewriter.IsTyping) typewriter.Complete();
            else NextLine();
        }

        private void Finish()
        {
            var callback = onFinished;
            onFinished = null;
            data = null;
            if (callback != null) callback();
            else UIManager.Instance.Back();
        }

        public override bool HandleBack()
        {
            Finish();
            return true;
        }
    }
}
