using System.Collections.Generic;
using Scars.Controls;
using Scars.Core;
using Scars.Data;
using Scars.Hypothesis;
using UnityEngine;
using UnityEngine.UI;

namespace Scars.UI
{
    // 가설 조립창 (프리팹 Prefabs/UI/HypothesisBoard). 판정·문장은 HypothesisBoard가 하고, 여기서는 그리기와 입력만 한다
    // 이 오브젝트는 켜 둔 채로 두고 root(창 그림)만 켜고 끈다 — 창이 닫혀 있어도 가설 단계 진입을 알아채야 해서
    // 그림·색·버튼 글자는 skin 에셋, 배치·크기는 프리팹에서 바꾼다
    public class HypothesisBoardView : MonoBehaviour, IScreen
    {
        [SerializeField] GameObject root;
        [SerializeField] HypothesisBoardSkin skin;
        [Tooltip("[행위자] · [수단] · [동기] (순서 상관없음, 각 슬롯의 Slot 값으로 찾는다)")]
        [SerializeField] HypothesisSlotView[] slots = new HypothesisSlotView[0];
        [Tooltip("손패 카드가 놓일 곳 (Layout Group)")]
        [SerializeField] Transform hand;
        [SerializeField] HypothesisCardView cardPrefab;
        [Tooltip("끄는 동안 카드가 올라갈 곳 (손패 Layout 밖)")]
        [SerializeField] Transform dragLayer;
        [SerializeField] Text sentenceText;
        [SerializeField] Text warningText;
        [SerializeField] Button submitButton;
        [SerializeField] Image submitImage;
        [SerializeField] Text submitLabel;

        readonly List<HypothesisCardView> _cards = new List<HypothesisCardView>();
        HypothesisBoard _board;
        HypothesisResult _preview;
        bool _dirty;

        // 시간 초과 뒤 강제 가설 단계에서는 닫을 수 없다
        public bool CanClose => Flow == null || !Flow.IsForcedHypothesis;

        public string PauseReason => "hypothesis";

        static GameFlow Flow => GameManager.Instance != null ? GameManager.Instance.Flow : null;

        void Awake()
        {
            foreach (var s in slots) s.Bind(this, skin);
            if (submitButton != null) submitButton.onClick.AddListener(Submit);
            if (root != null) root.SetActive(false);
        }

        void Start()
        {
            if (Flow == null) return;
            Flow.PhaseChanged += OnPhaseChanged;
            if (Flow.CurrentPhase != null) OnPhaseChanged(Flow.CurrentPhase);
        }

        void OnDestroy()
        {
            if (Flow != null) Flow.PhaseChanged -= OnPhaseChanged;
        }

        // 가설 단계에 들어오면 새 조립판으로 연다
        void OnPhaseChanged(PhaseData phase)
        {
            _board = null;
            if (phase.kind == PhaseKind.Hypothesis) Flow.Screens.Push(this);
        }

        public void Open()
        {
            if (_board == null) _board = new HypothesisBoard();
            if (root != null) root.SetActive(true);
            _dirty = true;
        }

        public void Close()
        {
            if (root != null) root.SetActive(false);
        }

        // 클릭: 카드마다 들어갈 슬롯이 정해져 있다
        public void PlaceCard(HypothesisCardData card)
        {
            if (card != null) PlaceCard(card, card.slot);
        }

        // 같은 슬롯에 있던 카드는 손패로 돌아간다
        public void PlaceCard(HypothesisCardData card, HypothesisSlot slot)
        {
            if (_board != null && _board.Place(card, slot, out _)) _dirty = true;
        }

        public void RemoveCard(HypothesisSlot slot)
        {
            if (_board != null && _board.Remove(slot) != null) _dirty = true;
        }

        // 잡은 카드가 들어갈 수 있는 슬롯만 하이라이트
        public void BeginDrag(HypothesisCardData card)
        {
            foreach (var s in slots) s.SetHighlight(_board != null && _board.CanPlace(card, s.Slot));
        }

        public void EndDrag()
        {
            foreach (var s in slots) s.SetHighlight(false);
            _dirty = true;
        }

        // 놓기(OnDrop) 처리 중에 카드 오브젝트를 지우지 않도록, 다시 그리기는 프레임 끝에 한 번
        void LateUpdate()
        {
            if (!_dirty || _board == null || Flow == null) return;
            _dirty = false;
            Redraw();
        }

        void Redraw()
        {
            foreach (var s in slots) s.Show(_board.Get(s.Slot));
            RedrawHand();

            var database = Flow.Database;
            _preview = _board.Preview(database != null ? database.All<HypothesisRuleData>() : null, Flow.IsForcedHypothesis);
            if (sentenceText != null) sentenceText.text = _board.BuildDraftSentence();
            if (warningText != null)
                warningText.text = _preview != null && _preview.Kind == HypothesisKind.Contradiction ? _preview.Warning ?? "" : "";
            RedrawSubmit();
        }

        // 조건을 만족한 카드 중 슬롯에 없는 것
        void RedrawHand()
        {
            foreach (var card in _cards) if (card != null) Destroy(card.gameObject);
            _cards.Clear();
            var database = Flow.Database;
            if (database == null || hand == null || cardPrefab == null) return;
            foreach (var card in HypothesisBoard.UnlockedCards(database.All<HypothesisCardData>(), Flow.State))
            {
                if (_board.Get(card.slot) == card) continue;
                var view = Instantiate(cardPrefab, hand);
                view.Bind(card, this, skin, dragLayer != null ? dragLayer : transform);
                _cards.Add(view);
            }
        }

        // 정합 = 황금 [가설 확정], 모순 = 붉은 [밀어붙이기], 시간 초과 뒤 빈 칸 = [이대로 제출]. 제출할 수 없으면 숨긴다
        void RedrawSubmit()
        {
            if (submitButton == null) return;
            submitButton.gameObject.SetActive(_preview != null);
            if (_preview == null || skin == null) return;
            bool consistent = _preview.Kind == HypothesisKind.Consistent;
            if (submitImage != null)
            {
                var sprite = consistent ? skin.confirmSprite : skin.pushSprite;
                if (sprite != null) submitImage.sprite = sprite;
                submitImage.color = consistent ? skin.confirmColor : skin.pushColor;
            }
            if (submitLabel != null)
                submitLabel.text = _preview.Incomplete ? skin.incompleteText : consistent ? skin.confirmText : skin.pushText;
        }

        void Submit()
        {
            if (_preview != null && GameManager.Instance != null) GameManager.Instance.ConfirmHypothesis(_preview);
        }
    }
}
