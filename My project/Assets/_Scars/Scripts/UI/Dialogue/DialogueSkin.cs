using UnityEngine;

namespace Scars.UI
{
    // 대화창의 그림·색·고정 글자. 실제 UI 이미지가 오면 이 에셋의 칸만 바꾼다 (그림이 비면 색만 쓴다)
    [CreateAssetMenu(menuName = "Scars/UI/Dialogue Skin")]
    public class DialogueSkin : ScriptableObject
    {
        [Header("대사 상자")]
        public Sprite boxSprite;
        public Color boxColor = new Color(0.08f, 0.08f, 0.1f, 0.95f);
        public Sprite nameSprite;
        public Color nameColor = new Color(0.3f, 0.25f, 0.18f);
        [Tooltip("선택지가 없을 때 대사 끝에 보이는 표시")]
        public string nextMarker = "▼";

        [Header("선택지")]
        public Sprite choiceSprite;
        public Color choiceColor = new Color(0.2f, 0.2f, 0.24f, 0.95f);
        [Tooltip("증거 제시 선택지")]
        public Color evidenceChoiceColor = new Color(0.35f, 0.25f, 0.12f, 0.95f);

        [Header("증거 제시")]
        public Sprite evidenceSprite;
        public Color evidenceColor = new Color(0.28f, 0.28f, 0.32f);
        public Color evidenceSelectedColor = new Color(0.75f, 0.55f, 0.15f);
        public string evidenceTitle = "제시할 단서를 고르세요";
        public string presentText = "제시하기";
        public string cancelText = "취소";
        public string noEvidenceText = "가진 단서가 없다";
        [Tooltip("맞지 않는 카드를 냈을 때")]
        public string noReactionText = "반응이 없다";
    }
}
