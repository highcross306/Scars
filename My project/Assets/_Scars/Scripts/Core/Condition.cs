using System;

namespace Scars.Core
{
    // 무언가가 열리는 조건 (선택지, 메모, 가설 카드 등). 모두 만족해야 열린다
    [Serializable]
    public class Condition
    {
        public string[] requiredFlags = new string[0];
        public string[] requiredClueIds = new string[0];

        public bool IsMet(GameState state) { return false; }
    }
}
