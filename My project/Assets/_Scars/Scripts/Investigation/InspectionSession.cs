using Scars.Core;
using Scars.Data;

namespace Scars.Investigation
{
    // 조사 대상 하나: 기본 카드 획득 → 도구 사용 → 조작 순서 → 판별 → 업그레이드
    public class InspectionSession
    {
        public void Begin(InspectableData target, GameState state) { }

        public bool Collect() { return false; }

        public bool CanDeepInspect() { return false; }

        public bool UseItem(string itemId) { return false; }

        // 순서가 맞으면 true
        public bool ClickStep(string stepId) { return false; }

        public bool StepsDone => false;

        // 판별에 성공하면 카드 업그레이드
        public bool Judge(string answer) { return false; }
    }
}
