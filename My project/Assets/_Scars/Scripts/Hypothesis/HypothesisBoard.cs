using System.Collections.Generic;
using Scars.Data;

namespace Scars.Hypothesis
{
    // 가설 조립창. 모순이어도 확정(밀어붙이기)할 수 있다
    public class HypothesisBoard
    {
        // 카드를 잡았을 때 하이라이트·드롭 허용에 쓴다
        public bool CanPlace(HypothesisCardData card, HypothesisSlot slot) { return false; }

        // 이미 있던 카드를 돌려준다 (없으면 null)
        public HypothesisCardData Place(HypothesisCardData card, HypothesisSlot slot) { return null; }

        public HypothesisCardData Remove(HypothesisSlot slot) { return null; }

        public HypothesisCardData Get(HypothesisSlot slot) { return null; }

        public bool IsComplete => false;

        // 실시간 명제
        public string BuildSentence() { return null; }

        public HypothesisResult Evaluate(IEnumerable<HypothesisRuleData> rules) { return null; }
    }
}
