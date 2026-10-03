using Scars.Core;
using Scars.Hypothesis;
using UnityEngine;

namespace Scars.Data
{
    // 가설 슬롯에 넣는 카드. 단서를 모으면 열린다
    [CreateAssetMenu(menuName = "Scars/Hypothesis Card")]
    public class HypothesisCardData : DataAsset
    {
        public HypothesisSlot slot;
        public string label;
        [Tooltip("문장에 들어갈 표현 (조사 은/는·을/를은 자동으로 붙음)")]
        public string phrase;
        public Condition unlock = new Condition();
    }
}
