using System.Collections.Generic;
using Scars.Core;
using Scars.Data;
using Scars.Dialogue;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scars.UI
{
    // 대화창 (프리팹 Prefabs/UI/DialogueView). 진행 규칙은 DialogueRunner가 하고, 여기서는 그리기와 입력만 한다
    // 이 오브젝트는 켜 둔 채로 두고 root만 켜고 끈다. 대화 중 조사 시간 멈춤은 GameFlow가 한다 (IScreen 아님)
    // 그림·색·고정 글자는 skin 에셋, 배치·크기는 프리팹, 인물 이름·초상화는 CharacterProfileData에서 바꾼다
    public class DialogueView : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] DialogueSkin skin;

        [Header("대사")]
        [SerializeField] Image box;
        [SerializeField] GameObject namePlate;
        [SerializeField] Image nameBackground;
        [SerializeField] Text nameText;
        [SerializeField] Text lineText;
        [SerializeField] Text nextMarker;
        [Tooltip("화자 초상화 (CharacterProfileData.portrait). 없으면 꺼진다")]
        [SerializeField] Image portrait;
        [Tooltip("화면 아무 곳 클릭 = 다음 대사 (선택지가 없을 때)")]
        [SerializeField] Button advanceArea;

        [Header("선택지")]
        [SerializeField] Transform choiceList;
        [SerializeField] LabelButton choicePrefab;

        [Header("증거 제시")]
        [SerializeField] GameObject evidencePanel;
        [SerializeField] Text evidenceTitle;
        [SerializeField] Transform evidenceList;
        [SerializeField] LabelButton evidencePrefab;
        [SerializeField] LabelButton presentButton;
        [SerializeField] LabelButton cancelButton;
        [SerializeField] Text evidenceMessage;

        readonly List<LabelButton> _choices = new List<LabelButton>();
        readonly Dictionary<string, LabelButton> _evidence = new Dictionary<string, LabelButton>();
        readonly HashSet<string> _selected = new HashSet<string>();
        DialogueRunner _runner;
        bool _dirty;

        static GameFlow Flow => GameManager.Instance != null ? GameManager.Instance.Flow : null;

        void Awake()
        {
            if (skin == null) skin = ScriptableObject.CreateInstance<DialogueSkin>();
            if (advanceArea != null) advanceArea.onClick.AddListener(Advance);
            if (presentButton != null) presentButton.Bind(skin.presentText, null, skin.evidenceSelectedColor, Present);
            if (cancelButton != null) cancelButton.Bind(skin.cancelText, null, skin.choiceColor, CloseEvidence);
            if (box != null)
            {
                if (skin.boxSprite != null) box.sprite = skin.boxSprite;
                box.color = skin.boxColor;
            }
            if (nameBackground != null)
            {
                if (skin.nameSprite != null) nameBackground.sprite = skin.nameSprite;
                nameBackground.color = skin.nameColor;
            }
            if (root != null) root.SetActive(false);
        }

        // 씬이 바뀌는 단계의 시작 대화는 이 씬이 로드되기 전에 시작된다 → 처음에 지금 대화를 읽는다
        void Start()
        {
            if (Flow == null) return;
            Flow.DialogueStarted += OnDialogueStarted;
            if (Flow.Dialogue != null) OnDialogueStarted(Flow.Dialogue);
        }

        void OnDestroy()
        {
            if (Flow != null) Flow.DialogueStarted -= OnDialogueStarted;
        }

        // 이 알림은 첫 대사에 들어가기 전에 온다 → 그리기는 프레임 끝에
        void OnDialogueStarted(DialogueRunner runner)
        {
            _runner = runner;
            CloseEvidence();
            _dirty = true;
        }

        void Update()
        {
            if (_runner == null || root == null || !root.activeSelf) return;
            // 단계가 바뀌어 대화가 정리됐으면 닫는다
            if (Flow == null || Flow.Dialogue != _runner)
            {
                _runner = null;
                _dirty = true;
                return;
            }
            if (evidencePanel != null && evidencePanel.activeSelf) return;
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) Advance();
        }

        void LateUpdate()
        {
            if (!_dirty) return;
            _dirty = false;
            Redraw();
        }

        // 선택지가 보이면 넘어가지 않는다
        public void Advance()
        {
            Deselect();
            if (_runner != null && _runner.Advance()) _dirty = true;
        }

        // 마지막에 누른 버튼이 선택된 채로 남으면 Space가 그 버튼을 한 번 더 누른다
        static void Deselect()
        {
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        void Redraw()
        {
            var node = _runner != null ? _runner.Current : null;
            if (node == null)
            {
                _runner = null;
                if (root != null) root.SetActive(false);
                return;
            }
            if (root != null) root.SetActive(true);

            var speaker = string.IsNullOrEmpty(node.speakerId) || Flow.Database == null ? null : Flow.Database.Find<CharacterProfileData>(node.speakerId);
            bool narration = string.IsNullOrEmpty(node.speakerId);
            if (namePlate != null) namePlate.SetActive(!narration);
            if (nameText != null) nameText.text = speaker != null ? speaker.displayName : node.speakerId;
            if (portrait != null)
            {
                portrait.sprite = speaker != null ? speaker.portrait : null;
                portrait.enabled = portrait.sprite != null;
            }
            if (lineText != null) lineText.text = node.text;
            if (nextMarker != null) nextMarker.text = _runner.HasChoices ? "" : skin.nextMarker;
            RedrawChoices();
        }

        void RedrawChoices()
        {
            foreach (var c in _choices) if (c != null) Destroy(c.gameObject);
            _choices.Clear();
            if (choiceList == null || choicePrefab == null) return;
            var choices = _runner.AvailableChoices();
            for (int i = 0; i < choices.Count; i++)
            {
                int index = i;
                bool evidence = DialogueRunner.IsEvidenceChoice(choices[i]);
                var button = Instantiate(choicePrefab, choiceList);
                button.Bind(choices[i].text, skin.choiceSprite, evidence ? skin.evidenceChoiceColor : skin.choiceColor,
                    evidence ? (System.Action)OpenEvidence : () => Choose(index));
                _choices.Add(button);
            }
        }

        void Choose(int index)
        {
            Deselect();
            if (_runner != null && _runner.Choose(index)) _dirty = true;
        }

        // ── 증거 제시: 가진 카드 중에서 골라 Present. 맞지 않으면 아무것도 바뀌지 않는다 ──

        void OpenEvidence()
        {
            if (evidencePanel == null) return;
            evidencePanel.SetActive(true);
            _selected.Clear();
            foreach (var b in _evidence.Values) if (b != null) Destroy(b.gameObject);
            _evidence.Clear();
            if (evidenceTitle != null) evidenceTitle.text = skin.evidenceTitle;

            var clues = Flow.State.Clues.All;
            if (evidenceMessage != null) evidenceMessage.text = clues.Count == 0 ? skin.noEvidenceText : "";
            if (evidenceList == null || evidencePrefab == null) return;
            foreach (var id in clues)
            {
                var clueId = id;
                var card = Flow.Database != null ? Flow.Database.Find<ClueCardData>(clueId) : null;
                var button = Instantiate(evidencePrefab, evidenceList);
                button.Bind(card != null && !string.IsNullOrEmpty(card.title) ? card.title : clueId, skin.evidenceSprite, skin.evidenceColor,
                    () => ToggleEvidence(clueId));
                _evidence[clueId] = button;
            }
        }

        void ToggleEvidence(string clueId)
        {
            if (!_selected.Remove(clueId)) _selected.Add(clueId);
            _evidence[clueId].SetColor(_selected.Contains(clueId) ? skin.evidenceSelectedColor : skin.evidenceColor);
            if (evidenceMessage != null) evidenceMessage.text = "";
        }

        void Present()
        {
            if (_runner == null) return;
            if (_runner.Present(_selected))
            {
                CloseEvidence();
                _dirty = true;
            }
            else if (evidenceMessage != null)
            {
                evidenceMessage.text = skin.noReactionText;
            }
        }

        void CloseEvidence()
        {
            if (evidencePanel != null) evidencePanel.SetActive(false);
            _selected.Clear();
        }
    }
}
