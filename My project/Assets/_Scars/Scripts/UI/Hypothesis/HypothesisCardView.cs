using Scars.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scars.UI
{
    // 손패의 카드 한 장 (프리팹). 클릭하면 제 슬롯에 들어가고, 끌어서 슬롯에 놓을 수도 있다
    [RequireComponent(typeof(CanvasGroup))]
    public class HypothesisCardView : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] Image background;
        [Tooltip("카드 그림 (HypothesisCardData.image). 그림이 없으면 꺼진다")]
        [SerializeField] Image picture;
        [SerializeField] Text label;

        HypothesisBoardView _board;
        Transform _dragLayer;

        public HypothesisCardData Card { get; private set; }

        public void Bind(HypothesisCardData card, HypothesisBoardView board, HypothesisBoardSkin skin, Transform dragLayer)
        {
            Card = card;
            _board = board;
            _dragLayer = dragLayer;
            CardVisual.Apply(card, picture, label);
            if (background != null && skin != null)
            {
                if (skin.cardSprite != null) background.sprite = skin.cardSprite;
                background.color = skin.cardColor;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) _board.PlaceCard(Card);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) return;
            // 슬롯이 놓기(OnDrop)를 받도록 카드는 레이캐스트에서 빠진다
            GetComponent<CanvasGroup>().blocksRaycasts = false;
            transform.SetParent(_dragLayer, true);
            transform.SetAsLastSibling();
            _board.BeginDrag(Card);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) transform.position = eventData.position;
        }

        // 손패는 창이 다시 그린다 (슬롯에 놓지 못했으면 제자리로)
        public void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) _board.EndDrag();
        }
    }

    // 카드 그림·글자 채우기 (손패 카드와 슬롯이 같이 쓴다)
    static class CardVisual
    {
        public static void Apply(HypothesisCardData card, Image picture, Text label)
        {
            if (picture != null)
            {
                picture.sprite = card != null ? card.image : null;
                picture.enabled = picture.sprite != null;
            }
            if (label != null) label.text = card != null ? card.label : "";
        }
    }
}
