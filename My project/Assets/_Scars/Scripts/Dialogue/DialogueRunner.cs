using System;
using System.Collections.Generic;
using Scars.Core;

namespace Scars.Dialogue
{
    public class DialogueRunner
    {
        public event Action<string> EndingReached;

        public DialogueNode Current => null;

        public bool IsFinished => true;

        public void Start(DialogueData dialogue, GameState state) { }

        // 조건을 만족하는 선택지만
        public IReadOnlyList<DialogueChoice> AvailableChoices() { return new DialogueChoice[0]; }

        public void Advance() { }

        public void Choose(int index) { }

        // 증거 제시. 맞으면 true
        public bool Present(IEnumerable<string> clueIds) { return false; }
    }
}
