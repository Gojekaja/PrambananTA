using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PrambananAR.UI
{
    /// <summary>Detail tokoh dari layar Koleksi: potret, deskripsi, ngobrol lagi, baca cerita.</summary>
    public class CharacterDetailPopup : UIPopup
    {
        [SerializeField] private ZoneDatabase zoneDatabase;
        [SerializeField] private Image portrait;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI zoneText;
        [SerializeField] private Button talkButton;
        [SerializeField] private Button storyButton;

        private CharacterData character;

        protected override void Awake()
        {
            base.Awake();
            if (talkButton != null) talkButton.onClick.AddListener(Talk);
            if (storyButton != null) storyButton.onClick.AddListener(() => UIManager.Instance.Show(UIScreenId.Story));
        }

        public void Setup(CharacterData data) => character = data;

        protected override void OnShow()
        {
            if (character == null) return;
            if (portrait != null)
            {
                portrait.sprite = character.portrait;
                portrait.enabled = character.portrait != null;
                StartCoroutine(UIAnim.Scale(portrait.transform, Vector3.one * 0.7f, Vector3.one, 0.4f));
            }
            if (nameText != null) nameText.text = character.displayName;
            if (titleText != null) titleText.text = character.title;
            if (descriptionText != null) descriptionText.text = character.description;

            var zone = zoneDatabase != null ? zoneDatabase.Find(character.zoneId) : null;
            if (zoneText != null) zoneText.text = zone != null ? $"Ditemukan di: {zone.displayName}" : string.Empty;
            if (talkButton != null) talkButton.gameObject.SetActive(character.encounterDialogue != null);
        }

        private void Talk()
        {
            var dialogue = UIManager.Instance.Get<DialogueScreen>(UIScreenId.Dialogue);
            // onFinished null -> otomatis kembali ke layar Koleksi lewat riwayat
            if (dialogue != null) dialogue.Play(character.encounterDialogue);
        }
    }
}
