using Scars.Controls;
using Scars.Data;
using Scars.Hypothesis;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scars.UI
{
    // 가설 슬롯 하나 ([행위자] · [수단] · [동기]). 카드를 놓으면 들어가고, 우클릭하면 손패로 돌아간다
    public class HypothesisSlotView : MonoBehaviour, IDropHandler, IPointerClickHandler, IRightClickTarget
    {
        [SerializeField] HypothesisSlot slot;
        [SerializeField] Image background;
        [SerializeField] Image picture;
        [SerializeField] Text cardLabel;
        [Tooltip("비어 있을 때 보일 글자")]
        [SerializeField] string emptyText = "";

        HypothesisBoardView _board;
        HypothesisBoardSkin _skin;

        public HypothesisSlot Slot => slot;

        public void Bind(HypothesisBoardView board, HypothesisBoardSkin skin)
        {
            _board = board;
            _skin = skin;
            if (background != null && skin != null && skin.slotSprite != null) background.sprite = skin.slotSprite;
            SetHighlight(false);
        }

        public void Show(HypothesisCardData card)
        {
            CardVisual.Apply(card, picture, cardLabel);
            if (card == null && cardLabel != null) cardLabel.text = emptyText;
        }

        public void SetHighlight(bool on)
        {
            if (background != null && _skin != null) background.color = on ? _skin.slotHighlightColor : _skin.slotColor;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var card = eventData.pointerDrag != null ? eventData.pointerDrag.GetComponent<HypothesisCardView>() : null;
            if (card != null) _board.PlaceCard(card.Card, slot);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right) _board.RemoveCard(slot);
        }
    }
}
