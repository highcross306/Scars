using UnityEngine;

namespace Scars.Data
{
    // 조사 대상 하나. 클릭하면 기본 카드, 심층 조사(도구 사용 → 조작 순서 → 판별)에 성공하면 업그레이드 카드
    [CreateAssetMenu(menuName = "Scars/Inspectable")]
    public class InspectableData : DataAsset
    {
        [Tooltip("처음 조사하면 받는 카드")]
        public string grantClueId;

        [Header("심층 조사 (비우면 없음)")]
        [Tooltip("필요한 도구 카드")]
        public string requiredItemId;
        [Tooltip("확대 화면에서 눌러야 하는 대상, 순서대로")]
        public string[] steps = new string[0];
        [Tooltip("판별 질문")]
        public QuizData judgment;
        [Tooltip("판별에 성공하면 grantClueId를 이 카드로 바꾼다")]
        public string upgradeClueId;
    }
}
