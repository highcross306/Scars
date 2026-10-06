using System.Collections.Generic;
using NUnit.Framework;
using Scars.Core;
using Scars.Dialogue;
using UnityEngine;

namespace Scars.Tests
{
    public class DialogueRunnerTests
    {
        readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created) Object.DestroyImmediate(obj);
            _created.Clear();
        }

        DialogueData MakeDialogue(string start, params DialogueNode[] nodes)
        {
            var dialogue = ScriptableObject.CreateInstance<DialogueData>();
            dialogue.startNodeId = start;
            dialogue.nodes.AddRange(nodes);
            _created.Add(dialogue);
            return dialogue;
        }

        static DialogueNode Node(string id, string next = null, params DialogueChoice[] choices)
        {
            var node = new DialogueNode { id = id, nextId = next };
            node.choices.AddRange(choices);
            return node;
        }

        static DialogueChoice Choice(string next, string[] flags = null, string[] present = null,
            string ending = null, string[] needFlags = null)
        {
            var choice = new DialogueChoice { nextId = next };
            choice.effect.setFlags = flags ?? new string[0];
            choice.effect.endingId = ending;
            choice.presentClueIds = present ?? new string[0];
            choice.condition.requiredFlags = needFlags ?? new string[0];
            return choice;
        }

        static DialogueRunner Run(DialogueData dialogue, GameState state)
        {
            var runner = new DialogueRunner();
            runner.Start(dialogue, state);
            return runner;
        }

        [Test]
        public void Start_EntersStartNode()
        {
            var r = Run(MakeDialogue("b", Node("a"), Node("b")), new GameState());

            Assert.AreEqual("b", r.Current.id);
            Assert.IsFalse(r.IsFinished);
        }

        [Test]
        public void Start_WithoutDialogueOrMissingStart_IsFinished()
        {
            Assert.IsTrue(Run(null, new GameState()).IsFinished);
            Assert.IsTrue(Run(MakeDialogue("none", Node("a")), new GameState()).IsFinished);
        }

        [Test]
        public void Advance_FollowsNextIdUntilEnd()
        {
            var r = Run(MakeDialogue("a", Node("a", "b"), Node("b")), new GameState());

            Assert.IsTrue(r.Advance());
            Assert.AreEqual("b", r.Current.id);
            Assert.IsTrue(r.Advance());
            Assert.IsTrue(r.IsFinished);
            Assert.IsFalse(r.Advance());
        }

        [Test]
        public void Advance_RefusedWhileChoicesAreShown()
        {
            var r = Run(MakeDialogue("a", Node("a", "b", Choice("b")), Node("b")), new GameState());

            Assert.IsFalse(r.Advance());
            Assert.AreEqual("a", r.Current.id);
        }

        [Test]
        public void OnEnter_AppliesEffect()
        {
            var node = Node("a");
            node.onEnter.giveClueIds = new[] { "card" };
            node.onEnter.setFlags = new[] { "met" };
            var state = new GameState();

            Run(MakeDialogue("a", node), state);

            Assert.IsTrue(state.Clues.Has("card"));
            Assert.IsTrue(state.HasFlag("met"));
        }

        [Test]
        public void AvailableChoices_HidesChoicesWithUnmetCondition()
        {
            var hidden = Choice("x", needFlags: new[] { "truth" });
            var shown = Choice("y");
            var r = Run(MakeDialogue("a", Node("a", null, hidden, shown)), new GameState());

            CollectionAssert.AreEqual(new[] { shown }, r.AvailableChoices());
        }

        [Test]
        public void AvailableChoices_ShowsChoiceOnceConditionIsMet()
        {
            var state = new GameState();
            state.SetFlag("truth");
            var r = Run(MakeDialogue("a", Node("a", null, Choice("x", needFlags: new[] { "truth" }))), state);

            Assert.AreEqual(1, r.AvailableChoices().Count);
        }

        [Test]
        public void Advance_AllowedWhenEveryChoiceIsHidden()
        {
            var r = Run(MakeDialogue("a", Node("a", "b", Choice("x", needFlags: new[] { "truth" })), Node("b")),
                new GameState());

            Assert.IsTrue(r.Advance());
            Assert.AreEqual("b", r.Current.id);
        }

        [Test]
        public void Choose_IndexIsAmongAvailableChoices()
        {
            var r = Run(MakeDialogue("a",
                Node("a", null, Choice("x", needFlags: new[] { "truth" }), Choice("y")),
                Node("x"), Node("y")), new GameState());

            Assert.IsTrue(r.Choose(0));
            Assert.AreEqual("y", r.Current.id);
        }

        [Test]
        public void Choose_AppliesEffectThenMoves()
        {
            var state = new GameState();
            var r = Run(MakeDialogue("a", Node("a", null, Choice("b", flags: new[] { "asked" })), Node("b")), state);

            r.Choose(0);

            Assert.IsTrue(state.HasFlag("asked"));
            Assert.AreEqual("b", r.Current.id);
        }

        [Test]
        public void Choose_OutOfRange_ReturnsFalse()
        {
            var r = Run(MakeDialogue("a", Node("a", null, Choice("b")), Node("b")), new GameState());

            Assert.IsFalse(r.Choose(-1));
            Assert.IsFalse(r.Choose(1));
            Assert.AreEqual("a", r.Current.id);
        }

        [Test]
        public void Choose_ReactionCanLoopBackToSameQuestion()
        {
            // 다른 선택지를 고르면 반응 후 같은 질문으로 돌아와 다시 고르게 하는 구조
            var r = Run(MakeDialogue("q",
                Node("q", null, Choice("answer"), Choice("reaction")),
                Node("reaction", "q"), Node("answer")), new GameState());

            r.Choose(1);
            Assert.AreEqual("reaction", r.Current.id);
            r.Advance();
            Assert.AreEqual("q", r.Current.id);
        }

        [Test]
        public void Choose_WithEnding_RaisesEndingAndFinishes()
        {
            var state = new GameState();
            var r = Run(MakeDialogue("a", Node("a", null, Choice("b", flags: new[] { "f" }, ending: "e1")), Node("b")),
                state);
            string reached = null;
            r.EndingReached += id => reached = id;

            Assert.IsTrue(r.Choose(0));

            Assert.AreEqual("e1", reached);
            Assert.IsTrue(r.IsFinished);
            Assert.IsTrue(state.HasFlag("f"));
        }

        [Test]
        public void OnEnterWithEnding_RaisesEndingAndFinishes()
        {
            var node = Node("b", "c");
            node.onEnter.endingId = "e2";
            var r = Run(MakeDialogue("a", Node("a", "b"), node, Node("c")), new GameState());
            string reached = null;
            r.EndingReached += id => reached = id;

            r.Advance();

            Assert.AreEqual("e2", reached);
            Assert.IsTrue(r.IsFinished);
        }

        [Test]
        public void Choose_RefusesPresentChoice()
        {
            var state = new GameState();
            state.Clues.Add("letter");
            var r = Run(MakeDialogue("a", Node("a", null, Choice("b", present: new[] { "letter" })), Node("b")), state);

            Assert.IsFalse(r.Choose(0));
            Assert.AreEqual("a", r.Current.id);
        }

        [Test]
        public void Present_MatchingCards_TakesChoice()
        {
            var state = new GameState();
            state.Clues.Add("letter");
            var r = Run(MakeDialogue("a",
                Node("a", null, Choice("b", flags: new[] { "shown" }, present: new[] { "letter" })), Node("b")), state);

            Assert.IsTrue(r.Present(new[] { "letter" }));
            Assert.AreEqual("b", r.Current.id);
            Assert.IsTrue(state.HasFlag("shown"));
        }

        [Test]
        public void Present_SeveralCards_OrderDoesNotMatter()
        {
            var state = new GameState();
            foreach (var id in new[] { "c1", "c2", "c3" }) state.Clues.Add(id);
            var r = Run(MakeDialogue("a",
                Node("a", null, Choice("b", present: new[] { "c1", "c2", "c3" })), Node("b")), state);

            Assert.IsTrue(r.Present(new[] { "c3", "c1", "c2" }));
        }

        [Test]
        public void Present_DuplicatesAndBlanks_AreIgnored()
        {
            var state = new GameState();
            foreach (var id in new[] { "c1", "c2" }) state.Clues.Add(id);
            var r = Run(MakeDialogue("a",
                Node("a", null, Choice("b", present: new[] { "c1", "", "c2", "c1" })), Node("b")), state);

            Assert.IsTrue(r.Present(new[] { "c2", "c1", "c2", null }));
            Assert.AreEqual("b", r.Current.id);
        }

        [Test]
        public void Start_DuplicateNodeId_FirstWins()
        {
            var first = Node("a", "b");
            var second = Node("a");
            var r = Run(MakeDialogue("a", first, second, Node("b")), new GameState());

            Assert.AreSame(first, r.Current);
        }

        [TestCase("c1,c2")]
        [TestCase("c1,c2,c3,c4")]
        [TestCase("c4")]
        public void Present_WrongSet_ChangesNothing(string given)
        {
            var state = new GameState();
            foreach (var id in new[] { "c1", "c2", "c3", "c4" }) state.Clues.Add(id);
            var r = Run(MakeDialogue("a",
                Node("a", null, Choice("b", flags: new[] { "shown" }, present: new[] { "c1", "c2", "c3" })),
                Node("b")), state);

            Assert.IsFalse(r.Present(given.Split(',')));
            Assert.AreEqual("a", r.Current.id);
            Assert.IsFalse(state.HasFlag("shown"));
        }

        [Test]
        public void Present_CardNotHeld_ReturnsFalse()
        {
            var r = Run(MakeDialogue("a", Node("a", null, Choice("b", present: new[] { "letter" })), Node("b")),
                new GameState());

            Assert.IsFalse(r.Present(new[] { "letter" }));
            Assert.AreEqual("a", r.Current.id);
        }

        [Test]
        public void Present_PicksChoiceMatchingTheCards()
        {
            var state = new GameState();
            state.Clues.Add("x");
            state.Clues.Add("y");
            var r = Run(MakeDialogue("a",
                Node("a", null, Choice("bx", present: new[] { "x" }), Choice("by", present: new[] { "y" })),
                Node("bx"), Node("by")), state);

            Assert.IsTrue(r.Present(new[] { "y" }));
            Assert.AreEqual("by", r.Current.id);
        }

        [Test]
        public void Present_HiddenChoiceCannotBeTaken()
        {
            var state = new GameState();
            state.Clues.Add("letter");
            var r = Run(MakeDialogue("a",
                Node("a", null, Choice("b", present: new[] { "letter" }, needFlags: new[] { "truth" })), Node("b")),
                state);

            Assert.IsFalse(r.Present(new[] { "letter" }));
        }

        [Test]
        public void Present_WithEnding_RaisesEnding()
        {
            var state = new GameState();
            state.Clues.Add("letter");
            var r = Run(MakeDialogue("a", Node("a", null, Choice("b", present: new[] { "letter" }, ending: "e4")),
                Node("b")), state);
            string reached = null;
            r.EndingReached += id => reached = id;

            r.Present(new[] { "letter" });

            Assert.AreEqual("e4", reached);
            Assert.IsTrue(r.IsFinished);
        }
    }
}
