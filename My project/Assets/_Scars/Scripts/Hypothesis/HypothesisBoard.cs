using System;
using System.Collections.Generic;
using Scars.Data;

namespace Scars.Hypothesis
{
    // 가설 조립창: [행위자] 은/는 [수단] 을/를 통해 [동기] 을/를 꾀했다.
    // 정합 가설이면 [가설 확정], 모순 가설이어도 [밀어붙이기]로 제출할 수 있다 (막지 않음)
    public class HypothesisBoard
    {
        const int SlotCount = 3;

        // 슬롯 번호(HypothesisSlot) = 배열 칸. 슬롯이 3개로 고정이라 Dictionary 대신 배열을 쓴다
        readonly HypothesisCardData[] _slots = new HypothesisCardData[SlotCount];
        int _filled;

        // 슬롯 카드가 바뀔 때마다 (실시간 명제 갱신용)
        public event Action Changed;

        // 카드를 잡았을 때 들어갈 수 있는 슬롯만 하이라이트·드롭 허용
        public bool CanPlace(HypothesisCardData card, HypothesisSlot slot) { return card != null && card.slot == slot && IsValid(slot); }

        // 놓을 수 없으면 false. 같은 슬롯에 있던 카드는 replaced로 돌려준다 (자동 반환 후 교체)
        public bool Place(HypothesisCardData card, HypothesisSlot slot, out HypothesisCardData replaced)
        {
            replaced = null;
            if (!CanPlace(card, slot)) return false;
            var old = _slots[(int)slot];
            if (old == card) return true;
            if (ReferenceEquals(old, null)) _filled++;
            replaced = old;
            _slots[(int)slot] = card;
            Changed?.Invoke();
            return true;
        }

        // 슬롯 카드 해제 (우클릭). 뺀 카드, 비어 있었으면 null
        public HypothesisCardData Remove(HypothesisSlot slot)
        {
            if (!IsValid(slot) || ReferenceEquals(_slots[(int)slot], null)) return null;
            var card = _slots[(int)slot];
            _slots[(int)slot] = null;
            _filled--;
            Changed?.Invoke();
            return card;
        }

        public HypothesisCardData Get(HypothesisSlot slot) { return IsValid(slot) ? _slots[(int)slot] : null; }

        public bool IsComplete => _filled == SlotCount;

        // 데이터에 잘못된 슬롯 값이 들어 있어도 배열 밖을 읽지 않는다
        static bool IsValid(HypothesisSlot slot) { return (uint)slot < SlotCount; }

        // 실시간 명제. 3개 슬롯이 다 차지 않았으면 null
        public string BuildSentence()
        {
            if (!IsComplete) return null;
            return KoreanJosa.Attach(Phrase(HypothesisSlot.Actor), "은", "는") + " "
                 + KoreanJosa.Attach(Phrase(HypothesisSlot.Means), "을", "를") + " 통해 "
                 + KoreanJosa.Attach(Phrase(HypothesisSlot.Motive), "을", "를") + " 꾀했다.";
        }

        // 규칙에 있는 조합이면 그 판정, 없으면 모순 가설. 슬롯이 다 차지 않았으면 null
        public HypothesisResult Evaluate(IEnumerable<HypothesisRuleData> rules)
        {
            if (!IsComplete) return null;
            var rule = FindRule(rules);
            if (rule == null)
                return new HypothesisResult { Kind = HypothesisKind.Contradiction, Sentence = BuildSentence() };
            return new HypothesisResult
            {
                Kind = rule.kind,
                ResultKey = rule.resultKey,
                Sentence = string.IsNullOrEmpty(rule.sentence) ? BuildSentence() : rule.sentence,
                Warning = rule.warning,
            };
        }

        // 시간 초과로 강제 제출할 때. 다 찼으면 Evaluate와 같고, 빈 칸이 있으면 억지(모순) 가설로 제출된다
        public HypothesisResult EvaluateForced(IEnumerable<HypothesisRuleData> rules)
        {
            if (IsComplete) return Evaluate(rules);
            return new HypothesisResult { Kind = HypothesisKind.Contradiction, Incomplete = true };
        }

        string Phrase(HypothesisSlot slot)
        {
            var card = _slots[(int)slot];
            return string.IsNullOrEmpty(card.phrase) ? card.label : card.phrase;
        }

        HypothesisRuleData FindRule(IEnumerable<HypothesisRuleData> rules)
        {
            if (rules == null) return null;
            string actor = _slots[(int)HypothesisSlot.Actor].Id;
            string means = _slots[(int)HypothesisSlot.Means].Id;
            string motive = _slots[(int)HypothesisSlot.Motive].Id;
            foreach (var rule in rules)
                if (rule != null && rule.actorCardId == actor && rule.meansCardId == means && rule.motiveCardId == motive)
                    return rule;
            return null;
        }
    }
}
