using System;
using System.Collections.Generic;
using Scars.Core;

namespace Scars.Dialogue
{
    // 대화·심문 한 묶음을 진행한다. 대사에 들어가면 onEnter, 선택하면 effect를 GameState에 반영하고,
    // effect에 endingId가 있으면 EndingReached를 알리고 끝난다
    public class DialogueRunner
    {
        static readonly DialogueChoice[] NoChoices = new DialogueChoice[0];

        // 노드 id → 노드. Start 때 한 번 만든다 (같은 id가 여럿이면 앞의 것)
        readonly Dictionary<string, DialogueNode> _nodes = new Dictionary<string, DialogueNode>();
        GameState _state;

        public event Action<string> EndingReached;

        public DialogueNode Current { get; private set; }

        public bool IsFinished => Current == null;

        public void Start(DialogueData dialogue, GameState state)
        {
            _state = state;
            Current = null;
            _nodes.Clear();
            if (dialogue == null || state == null) return;
            if (dialogue.nodes != null)
                foreach (var node in dialogue.nodes)
                    if (node != null && !string.IsNullOrEmpty(node.id) && !_nodes.ContainsKey(node.id))
                        _nodes.Add(node.id, node);
            Enter(dialogue.startNodeId);
        }

        // 조건을 만족하는 선택지만
        public IReadOnlyList<DialogueChoice> AvailableChoices()
        {
            if (Current == null || Current.choices == null) return NoChoices;
            var available = new List<DialogueChoice>(Current.choices.Count);
            foreach (var choice in Current.choices)
                if (IsAvailable(choice)) available.Add(choice);
            return available;
        }

        // 고를 선택지가 없을 때만 nextId로 넘어간다. nextId가 비면 대화가 끝난다
        public bool Advance()
        {
            if (Current == null || HasAvailableChoice()) return false;
            Enter(Current.nextId);
            return true;
        }

        // index는 AvailableChoices() 기준. 증거 제시 선택지는 Present로만 고른다
        public bool Choose(int index)
        {
            var choice = AvailableAt(index);
            if (choice == null || NeedsClues(choice)) return false;
            Take(choice);
            return true;
        }

        // 증거 제시. 낸 카드 묶음(순서 무관)이 선택지의 presentClueIds와 같고 모두 가지고 있으면 그 선택지로 진행(true).
        // 맞지 않으면 아무것도 바뀌지 않는다(false)
        public bool Present(IEnumerable<string> clueIds)
        {
            if (Current == null || clueIds == null || Current.choices == null) return false;
            var given = new HashSet<string>();
            foreach (var id in clueIds)
            {
                if (string.IsNullOrEmpty(id)) continue;
                if (!_state.Clues.Has(id)) return false;
                given.Add(id);
            }
            if (given.Count == 0) return false;

            foreach (var choice in Current.choices)
            {
                if (!IsAvailable(choice) || !NeedsClues(choice) || !SameClues(given, choice.presentClueIds)) continue;
                Take(choice);
                return true;
            }
            return false;
        }

        bool IsAvailable(DialogueChoice choice)
        {
            return choice != null && (choice.condition == null || choice.condition.IsMet(_state));
        }

        bool HasAvailableChoice()
        {
            if (Current.choices == null) return false;
            foreach (var choice in Current.choices)
                if (IsAvailable(choice)) return true;
            return false;
        }

        // AvailableChoices()[index]와 같은 선택지를 리스트를 만들지 않고 찾는다. 범위 밖이면 null
        DialogueChoice AvailableAt(int index)
        {
            if (Current == null || Current.choices == null || index < 0) return null;
            foreach (var choice in Current.choices)
            {
                if (!IsAvailable(choice)) continue;
                if (index == 0) return choice;
                index--;
            }
            return null;
        }

        static bool NeedsClues(DialogueChoice choice)
        {
            if (choice.presentClueIds == null) return false;
            foreach (var id in choice.presentClueIds)
                if (!string.IsNullOrEmpty(id)) return true;
            return false;
        }

        // 빈 칸은 무시하고, 겹치는 id는 하나로 본다 (HashSet.SetEquals와 같은 결과). 제시 카드는 몇 장뿐이라 새 집합을 만들지 않는다
        static bool SameClues(HashSet<string> given, string[] required)
        {
            int distinct = 0;
            for (int i = 0; i < required.Length; i++)
            {
                var id = required[i];
                if (string.IsNullOrEmpty(id) || Array.IndexOf(required, id, 0, i) >= 0) continue;
                if (!given.Contains(id)) return false;
                distinct++;
            }
            return distinct == given.Count;
        }

        void Take(DialogueChoice choice)
        {
            if (ApplyAndCheckEnding(choice.effect)) return;
            Enter(choice.nextId);
        }

        // 없는 id면 대화가 끝난다
        void Enter(string nodeId)
        {
            Current = null;
            if (!string.IsNullOrEmpty(nodeId) && _nodes.TryGetValue(nodeId, out var found)) Current = found;
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
