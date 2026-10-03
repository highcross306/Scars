using System;

namespace Scars.Core
{
    // 대사·선택·단계 진입 때 일어나는 일 (카드 지급, 플래그, 엔딩)
    [Serializable]
    public class GameEffect
    {
        public string[] giveClueIds = new string[0];
        public string[] setFlags = new string[0];
        public string endingId;

        public void Apply(GameState state) { }
    }
}
