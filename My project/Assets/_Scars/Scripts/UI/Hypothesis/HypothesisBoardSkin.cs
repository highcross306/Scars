using UnityEngine;

namespace Scars.UI
{
    // 가설 조립창의 그림·색·버튼 글자. 실제 UI 이미지가 오면 이 에셋의 칸만 바꾼다 (그림이 비면 색만 쓴다)
    [CreateAssetMenu(menuName = "Scars/UI/Hypothesis Board Skin")]
    public class HypothesisBoardSkin : ScriptableObject
    {
        [Header("슬롯")]
        public Sprite slotSprite;
        public Color slotColor = new Color(0.25f, 0.25f, 0.25f);
        [Tooltip("잡은 카드가 들어갈 수 있는 슬롯")]
        public Color slotHighlightColor = new Color(0.45f, 0.65f, 0.9f);

        [Header("카드")]
        public Sprite cardSprite;
        public Color cardColor = new Color(0.9f, 0.88f, 0.8f);

        [Header("제출 버튼")]
        public Sprite confirmSprite;
        [Tooltip("정합 가설: 황금")]
        public Color confirmColor = new Color(0.85f, 0.68f, 0.2f);
        public string confirmText = "가설 확정";
        public Sprite pushSprite;
        [Tooltip("모순 가설: 붉은")]
        public Color pushColor = new Color(0.7f, 0.15f, 0.15f);
        public string pushText = "밀어붙이기";
        [Tooltip("시간 초과 뒤 빈 칸이 있는 채로 제출")]
        public string incompleteText = "이대로 제출";
    }
}
