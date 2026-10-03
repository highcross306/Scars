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

        public bool HasEnding => !string.IsNullOrEmpty(endingId);

        // 카드·플래그만 반영한다. 엔딩 진입은 부른 쪽(DialogueRunner·GameManager)이 endingId를 보고 처리한다
        public void Apply(GameState state)
        {
            if (state == null) return;
            if (giveClueIds != null)
                foreach (var id in giveClueIds) state.Clues.Add(id);
            if (setFlags != null)
                foreach (var flag in setFlags) state.SetFlag(flag);
        }
    }
}
