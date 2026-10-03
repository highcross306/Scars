using Scars.Hypothesis;
using UnityEngine;

namespace Scars.Data
{
    // 카드 3장 조합 하나의 판정. 규칙에 없는 조합은 모순으로 본다
    [CreateAssetMenu(menuName = "Scars/Hypothesis Rule")]
    public class HypothesisRuleData : DataAsset
    {
        public string actorCardId;
        public string meansCardId;
        public string motiveCardId;
        public HypothesisKind kind;
        [Tooltip("심문 분기에 쓰는 결과 이름")]
        public string resultKey;
        [Tooltip("완성 문장. 비우면 카드 표현으로 자동 조립")]
        [TextArea] public string sentence;
        [Tooltip("모순일 때 보여줄 경고")]
        [TextArea] public string warning;
    }
}
