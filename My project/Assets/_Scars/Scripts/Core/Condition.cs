using System;

namespace Scars.Core
{
    // 무언가가 열리는 조건 (선택지, 메모, 가설 카드 등). 모두 만족해야 열린다
    [Serializable]
    public class Condition
    {
        public string[] requiredFlags = new string[0];
        public string[] requiredClueIds = new string[0];

        // 빈 칸(인스펙터에서 남은 빈 항목)은 무시한다. 조건이 하나도 없으면 항상 true
        public bool IsMet(GameState state)
        {
            if (requiredFlags != null)
                foreach (var flag in requiredFlags)
                    if (!string.IsNullOrEmpty(flag) && (state == null || !state.HasFlag(flag))) return false;
            if (requiredClueIds != null)
                foreach (var id in requiredClueIds)
                    if (!string.IsNullOrEmpty(id) && (state == null || !state.Clues.Has(id))) return false;
            return true;
        }
    }
}
