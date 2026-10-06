using System;
using System.Collections.Generic;
using System.Linq;
using Scars.Core;

namespace Scars.Dialogue
{
    // 대화·심문 한 묶음을 진행한다. 대사에 들어가면 onEnter, 선택하면 effect를 GameState에 반영하고,
    // effect에 endingId가 있으면 EndingReached를 알리고 끝난다
    public class DialogueRunner
    {
        DialogueData _dialogue;
        GameState _state;

        public event Action<string> EndingReached;

        public DialogueNode Current { get; private set; }

        public bool IsFinished => Current == null;

        public void Start(DialogueData dialogue, GameState state)
        {
            _dialogue = dialogue;
            _state = state;
            Current = null;
            if (dialogue == null || state == null) return;
            Enter(dialogue.startNodeId);
        }

        // 조건을 만족하는 선택지만
        public IReadOnlyList<DialogueChoice> AvailableChoices()
        {
            if (Current == null || Current.choices == null) return new DialogueChoice[0];
            return Current.choices
                .Where(c => c != null && (c.condition == null || c.condition.IsMet(_state)))
                .ToList();
        }

        // 고를 선택지가 없을 때만 nextId로 넘어간다. nextId가 비면 대화가 끝난다
        public bool Advance()
        {
            if (Current == null || AvailableChoices().Count > 0) return false;
            Enter(Current.nextId);
            return true;
        }

        // index는 AvailableChoices() 기준. 증거 제시 선택지는 Present로만 고른다
        public bool Choose(int index)
        {
            var choices = AvailableChoices();
            if (index < 0 || index >= choices.Count || NeedsClues(choices[index])) return false;
            Take(choices[index]);
            return true;
        }

        // 증거 제시. 낸 카드 묶음(순서 무관)이 선택지의 presentClueIds와 같고 모두 가지고 있으면 그 선택지로 진행(true).
        // 맞지 않으면 아무것도 바뀌지 않는다(false)
        public bool Present(IEnumerable<string> clueIds)
        {
            if (Current == null || clueIds == null) return false;
            var given = new HashSet<string>(clueIds.Where(id => !string.IsNullOrEmpty(id)));
            if (given.Count == 0 || given.Any(id => !_state.Clues.Has(id))) return false;
            foreach (var choice in AvailableChoices())
            {
                if (!NeedsClues(choice)) continue;
                if (!given.SetEquals(choice.presentClueIds.Where(id => !string.IsNullOrEmpty(id)))) continue;
                Take(choice);
                return true;
            }
            return false;
        }

        static bool NeedsClues(DialogueChoice choice)
        {
            return choice.presentClueIds != null && choice.presentClueIds.Any(id => !string.IsNullOrEmpty(id));
        }

        void Take(DialogueChoice choice)
        {
            if (ApplyAndCheckEnding(choice.effect)) return;
            Enter(choice.nextId);
        }

        // 없는 id면 대화가 끝난다
        void Enter(string nodeId)
        {
            Current = string.IsNullOrEmpty(nodeId) || _dialogue.nodes == null
                ? null
                : _dialogue.nodes.FirstOrDefault(n => n != null && n.id == nodeId);
            if (Current != null) ApplyAndCheckEnding(Current.onEnter);
        }

        bool ApplyAndCheckEnding(GameEffect effect)
        {
            if (effect == null) return false;
            effect.Apply(_state);
            if (!effect.HasEnding) return false;
            Current = null;
            EndingReached?.Invoke(effect.endingId);
            return true;
        }
    }
}
