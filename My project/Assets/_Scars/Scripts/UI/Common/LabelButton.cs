using System;
using UnityEngine;
using UnityEngine.UI;

namespace Scars.UI
{
    // 글자 하나짜리 버튼 프리팹 (대화 선택지, 증거 카드 등). 그림·색은 부르는 쪽 스킨이 넣는다
    [RequireComponent(typeof(Button))]
    public class LabelButton : MonoBehaviour
    {
        [SerializeField] Image background;
        [SerializeField] Text label;

        Action _onClick;

        void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => _onClick?.Invoke());
        }

        // sprite가 비면 프리팹 그림 그대로
        public void Bind(string text, Sprite sprite, Color color, Action onClick)
        {
            if (label != null) label.text = text;
            if (background != null)
            {
                if (sprite != null) background.sprite = sprite;
                background.color = color;
            }
            _onClick = onClick;
        }

        public void SetColor(Color color)
        {
            if (background != null) background.color = color;
        }
    }
}
