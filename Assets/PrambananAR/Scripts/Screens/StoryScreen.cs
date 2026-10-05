using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>
    /// Halaman cerita dua tampilan:
    /// 1) Sampul (judul, sinopsis, tag, progres baca)
    /// 2) Pembaca (ilustrasi, mode Audio/Teks, stabilo baca-bersama, ukuran huruf, navigasi halaman)
    /// </summary>
    public class StoryScreen : UIScreen
    {
        [SerializeField] private StoryData story;
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private UITheme theme;

        [Header("Sampul")]
        [SerializeField] private GameObject coverView;
        [SerializeField] private Button backButton;
        [SerializeField] private Image coverImage;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI subtitleText;
        [SerializeField] private TextMeshProUGUI synopsisText;
        [SerializeField] private TextMeshProUGUI progressText;
        [SerializeField] private Image progressFill;
        [SerializeField] private Button readButton;
        [SerializeField] private GameObject tagTemplate;

        [Header("Pembaca")]
        [SerializeField] private GameObject readerView;
        [SerializeField] private Button readerBackButton;
        [SerializeField] private Image illustration;
        [SerializeField] private TextMeshProUGUI chapterText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private TextMeshProUGUI pageText;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button playPauseButton;
        [SerializeField] private TextMeshProUGUI playPauseLabel;

        [Header("Mode Audio / Teks")]
        [SerializeField] private Button audioModeButton;
        [SerializeField] private Button textModeButton;
        [SerializeField] private Image audioModeBg;
        [SerializeField] private Image textModeBg;
        [SerializeField] private Color selectedTabColor = new Color(0.96f, 0.64f, 0.22f);
        [SerializeField] private Color idleTabColor = new Color(1f, 1f, 1f, 0.25f);

        [Header("Ukuran Huruf")]
        [SerializeField] private Button fontMinusButton;
        [SerializeField] private Button fontPlusButton;
        [SerializeField] private TextMeshProUGUI fontSizeText;
        [SerializeField] private float fontSize = 46f;
        [SerializeField] private float minFontSize = 34f;
        [SerializeField] private float maxFontSize = 66f;
        [SerializeField] private float fontStep = 4f;

        private readonly StringBuilder sb = new StringBuilder(512);
        private int page;
        private bool audioMode = true;
        private bool tagsBuilt;
        private Coroutine readRoutine;
        private string highlightHex;

        private void Awake()
        {
            if (backButton != null) backButton.onClick.AddListener(() => UIManager.Instance.Back());
            if (readButton != null) readButton.onClick.AddListener(() => OpenReader(ResumePage()));
            if (readerBackButton != null) readerBackButton.onClick.AddListener(ShowCover);
            if (prevButton != null) prevButton.onClick.AddListener(() => GoToPage(page - 1));
            if (nextButton != null) nextButton.onClick.AddListener(() => GoToPage(page + 1));
            if (playPauseButton != null) playPauseButton.onClick.AddListener(TogglePlay);
            if (audioModeButton != null) audioModeButton.onClick.AddListener(() => SetMode(true));
            if (textModeButton != null) textModeButton.onClick.AddListener(() => SetMode(false));
            if (fontMinusButton != null) fontMinusButton.onClick.AddListener(() => ChangeFont(-fontStep));
            if (fontPlusButton != null) fontPlusButton.onClick.AddListener(() => ChangeFont(fontStep));

            Color hl = theme != null ? theme.readAlongHighlight : new Color(1f, 0.85f, 0.35f, 0.55f);
            highlightHex = ColorUtility.ToHtmlStringRGBA(hl);
        }

        protected override void OnShow() => ShowCover();
        protected override void OnHide() => StopReading();

        public override bool HandleBack()
        {
            if (readerView != null && readerView.activeSelf)
            {
                ShowCover();
                return true;
            }
            return false;
        }

        // ------------------------------------------------------------------
        // Sampul
        // ------------------------------------------------------------------
        private void ShowCover()
        {
            StopReading();
            if (coverView != null) coverView.SetActive(true);
            if (readerView != null) readerView.SetActive(false);
            if (story == null) return;

            if (titleText != null) titleText.text = story.title;
            if (subtitleText != null) subtitleText.text = story.subtitle;
            if (synopsisText != null) synopsisText.text = story.synopsis;
            if (coverImage != null && story.cover != null) coverImage.sprite = story.cover;

            int read = Mathf.Min(PlayerProgressService.Data.storyPagesRead, story.pages.Count);
            if (progressText != null) progressText.text = $"{read}/{story.pages.Count} halaman dibaca";
            if (progressFill != null) progressFill.fillAmount = story.pages.Count > 0 ? (float)read / story.pages.Count : 0f;

            BuildTags();
        }

        private void BuildTags()
        {
            if (tagsBuilt || tagTemplate == null || story.tags == null) return;
            tagsBuilt = true;
            tagTemplate.SetActive(false);
            foreach (var tag in story.tags)
            {
                var chip = Instantiate(tagTemplate, tagTemplate.transform.parent);
                chip.SetActive(true);
                var label = chip.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = tag;
            }
        }

        private int ResumePage()
        {
            if (story == null || story.pages.Count == 0) return 0;
            int read = PlayerProgressService.Data.storyPagesRead;
            return read >= story.pages.Count ? 0 : read; // sudah tamat -> mulai dari awal
        }

        // ------------------------------------------------------------------
        // Pembaca
        // ------------------------------------------------------------------
        private void OpenReader(int startPage)
        {
            if (story == null || story.pages.Count == 0) return;
            if (coverView != null) coverView.SetActive(false);
            if (readerView != null) readerView.SetActive(true);
            ApplyFontSize();
            SetMode(audioMode);
            GoToPage(startPage);
        }

        private void GoToPage(int index)
        {
            if (story == null || story.pages.Count == 0) return;
            page = Mathf.Clamp(index, 0, story.pages.Count - 1);
            RenderPage();
            if (audioMode) StartReading();
        }

        private void RenderPage()
        {
            StopReading();
            var p = story.pages[page];

            if (chapterText != null) chapterText.text = $"Bab {page + 1}: {p.chapterTitle}";
            if (illustration != null)
            {
                illustration.sprite = p.illustration;
                illustration.enabled = p.illustration != null;
            }
            if (bodyText != null) bodyText.text = BuildBody(p, -1);
            if (pageText != null) pageText.text = $"{page + 1} / {story.pages.Count}";
            if (prevButton != null) prevButton.interactable = page > 0;
            if (nextButton != null) nextButton.interactable = page < story.pages.Count - 1;

            PlayerProgressService.MarkStoryPageRead(page);
        }

        // Gabungkan kalimat; kalimat yang sedang dibacakan diberi tag <mark> (stabilo)
        private string BuildBody(StoryPage p, int highlightIndex)
        {
            sb.Clear();
            if (p.sentences == null) return string.Empty;
            for (int i = 0; i < p.sentences.Length; i++)
            {
                if (i > 0) sb.Append(' ');
                if (i == highlightIndex) sb.Append("<mark=#").Append(highlightHex).Append('>').Append(p.sentences[i]).Append("</mark>");
                else sb.Append(p.sentences[i]);
            }
            return sb.ToString();
        }

        private void TogglePlay()
        {
            if (readRoutine != null)
            {
                StopReading();
                if (bodyText != null) bodyText.text = BuildBody(story.pages[page], -1);
            }
            else StartReading();
        }

        private void StartReading()
        {
            StopReading();
            if (!isActiveAndEnabled) return;
            readRoutine = StartCoroutine(ReadAlong());
            UpdatePlayLabel();
        }

        private void StopReading()
        {
            if (readRoutine != null) StopCoroutine(readRoutine);
            readRoutine = null;
            if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
            UpdatePlayLabel();
        }

        private IEnumerator ReadAlong()
        {
            var p = story.pages[page];
            int count = p.sentences != null ? p.sentences.Length : 0;
            if (count == 0) { readRoutine = null; yield break; }

            float perSentence = p.secondsPerSentence;
            if (p.narration != null && audioSource != null)
            {
                audioSource.clip = p.narration;
                audioSource.Play();
                perSentence = p.narration.length / count; // perkiraan rata per kalimat
            }

            for (int i = 0; i < count; i++)
            {
                if (bodyText != null) bodyText.text = BuildBody(p, i);
                yield return new WaitForSecondsRealtime(perSentence);
            }

            if (bodyText != null) bodyText.text = BuildBody(p, -1);

            // Mode audio: otomatis lanjut ke halaman berikutnya (masih bisa dijeda selama jeda 1 detik)
            if (page < story.pages.Count - 1)
            {
                yield return new WaitForSecondsRealtime(1f);
                readRoutine = null; // lepas handle dulu agar GoToPage tidak menghentikan coroutine ini
                GoToPage(page + 1);
                yield break;
            }
            readRoutine = null;
            UpdatePlayLabel();
        }

        private void UpdatePlayLabel()
        {
            if (playPauseLabel != null) playPauseLabel.text = readRoutine != null ? "Jeda" : "Putar";
        }

        private void SetMode(bool audio)
        {
            audioMode = audio;
            if (audioModeBg != null) audioModeBg.color = audio ? selectedTabColor : idleTabColor;
            if (textModeBg != null) textModeBg.color = audio ? idleTabColor : selectedTabColor;
            if (playPauseButton != null) playPauseButton.gameObject.SetActive(audio);

            if (!audio)
            {
                StopReading();
                if (bodyText != null && story != null && story.pages.Count > 0) bodyText.text = BuildBody(story.pages[page], -1);
            }
        }

        private void ChangeFont(float delta)
        {
            fontSize = Mathf.Clamp(fontSize + delta, minFontSize, maxFontSize);
            ApplyFontSize();
        }

        private void ApplyFontSize()
        {
            if (bodyText != null) bodyText.fontSize = fontSize;
            if (fontSizeText != null) fontSizeText.text = Mathf.RoundToInt(fontSize).ToString();
        }
    }
}
