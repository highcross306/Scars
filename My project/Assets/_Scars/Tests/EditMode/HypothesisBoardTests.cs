using System.Collections.Generic;
using NUnit.Framework;
using Scars.Data;
using Scars.Hypothesis;
using UnityEditor;
using UnityEngine;

namespace Scars.Tests
{
    public class HypothesisBoardTests
    {
        readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created) Object.DestroyImmediate(obj);
            _created.Clear();
        }

        HypothesisCardData Card(string id, HypothesisSlot slot, string phrase, string label = null)
        {
            var card = ScriptableObject.CreateInstance<HypothesisCardData>();
            var so = new SerializedObject(card);
            so.FindProperty("id").stringValue = id;
            so.ApplyModifiedPropertiesWithoutUndo();
            card.slot = slot;
            card.phrase = phrase;
            card.label = label ?? phrase;
            _created.Add(card);
            return card;
        }

        HypothesisRuleData Rule(string actor, string means, string motive, HypothesisKind kind, string key,
            string sentence = null, string warning = null)
        {
            var rule = ScriptableObject.CreateInstance<HypothesisRuleData>();
            rule.actorCardId = actor;
            rule.meansCardId = means;
            rule.motiveCardId = motive;
            rule.kind = kind;
            rule.resultKey = key;
            rule.sentence = sentence;
            rule.warning = warning;
            _created.Add(rule);
            return rule;
        }

        static void Put(HypothesisBoard board, HypothesisCardData card)
        {
            Assert.IsTrue(board.Place(card, card.slot, out _));
        }

        HypothesisBoard FullBoard(string actor, string means, string motive)
        {
            var board = new HypothesisBoard();
            Put(board, Card("a", HypothesisSlot.Actor, actor));
            Put(board, Card("m", HypothesisSlot.Means, means));
            Put(board, Card("o", HypothesisSlot.Motive, motive));
            return board;
        }

        // ── 실시간 명제: 콘솔에 문장으로 출력 ──

        [TestCase("고양이", "비밀 통로", "생선 독차지", "고양이는 비밀 통로를 통해 생선 독차지를 꾀했다.")]
        [TestCase("곰", "꿀단지 조작", "겨울잠 함정", "곰은 꿀단지 조작을 통해 겨울잠 함정을 꾀했다.")]
        [TestCase("집사", "열쇠 바꿔치기", "유산 가로채기", "집사는 열쇠 바꿔치기를 통해 유산 가로채기를 꾀했다.")]
        public void BuildSentence_PrintsKoreanSentenceToConsole(string actor, string means, string motive, string expected)
        {
            var sentence = FullBoard(actor, means, motive).BuildSentence();
            Debug.Log("[가설 문장] " + sentence);

            Assert.AreEqual(expected, sentence);
        }

        [Test]
        public void BuildSentence_EmptyPhrase_UsesLabel()
        {
            var board = new HypothesisBoard();
            Put(board, Card("a", HypothesisSlot.Actor, "", "탐정"));
            Put(board, Card("m", HypothesisSlot.Means, "변장"));
            Put(board, Card("o", HypothesisSlot.Motive, "잠입"));

            Assert.AreEqual("탐정은 변장을 통해 잠입을 꾀했다.", board.BuildSentence());
        }

        [Test]
        public void BuildSentence_Incomplete_IsNull()
        {
            var board = new HypothesisBoard();
            Put(board, Card("a", HypothesisSlot.Actor, "고양이"));

            Assert.IsFalse(board.IsComplete);
            Assert.IsNull(board.BuildSentence());
        }

        // ── 카드 UX 규칙 ──

        [Test]
        public void CanPlace_OnlyMatchingSlot()
        {
            var board = new HypothesisBoard();
            var actor = Card("a", HypothesisSlot.Actor, "고양이");

            Assert.IsTrue(board.CanPlace(actor, HypothesisSlot.Actor));
            Assert.IsFalse(board.CanPlace(actor, HypothesisSlot.Means));
            Assert.IsFalse(board.CanPlace(null, HypothesisSlot.Actor));
        }

        [Test]
        public void Place_OtherSlot_IsRejected()
        {
            var board = new HypothesisBoard();

            Assert.IsFalse(board.Place(Card("a", HypothesisSlot.Actor, "고양이"), HypothesisSlot.Motive, out var replaced));
            Assert.IsNull(replaced);
            Assert.IsNull(board.Get(HypothesisSlot.Motive));
        }

        [Test]
        public void Place_OnFilledSlot_ReturnsOldCard()
        {
            var board = new HypothesisBoard();
            var first = Card("a1", HypothesisSlot.Actor, "고양이");
            var second = Card("a2", HypothesisSlot.Actor, "곰");
            Put(board, first);

            Assert.IsTrue(board.Place(second, HypothesisSlot.Actor, out var replaced));
            Assert.AreSame(first, replaced);
            Assert.AreSame(second, board.Get(HypothesisSlot.Actor));
        }

        [Test]
        public void Remove_ReturnsCardAndEmptiesSlot()
        {
            var board = new HypothesisBoard();
            var card = Card("a", HypothesisSlot.Actor, "고양이");
            Put(board, card);

            Assert.AreSame(card, board.Remove(HypothesisSlot.Actor));
            Assert.IsNull(board.Get(HypothesisSlot.Actor));
            Assert.IsNull(board.Remove(HypothesisSlot.Actor));
        }

        [Test]
        public void Changed_FiresOnlyOnRealChange()
        {
            var board = new HypothesisBoard();
            var card = Card("a", HypothesisSlot.Actor, "고양이");
            int fired = 0;
            board.Changed += () => fired++;

            board.Place(card, HypothesisSlot.Actor, out _);
            board.Place(card, HypothesisSlot.Actor, out _);
            board.Place(card, HypothesisSlot.Means, out _);
            board.Remove(HypothesisSlot.Actor);
            board.Remove(HypothesisSlot.Actor);

            Assert.AreEqual(2, fired);
        }

        // ── 정합 가설 / 모순 가설 ──

        [Test]
        public void Evaluate_ConsistentRule_UsesRuleSentence()
        {
            var board = FullBoard("고양이", "비밀 통로", "생선 독차지");
            var rules = new[] { Rule("a", "m", "o", HypothesisKind.Consistent, "A", "고양이가 몰래 생선을 다 먹었다.") };

            var result = board.Evaluate(rules);
            Debug.Log("[정합 가설] " + result.Sentence);

            Assert.AreEqual(HypothesisKind.Consistent, result.Kind);
            Assert.AreEqual("A", result.ResultKey);
            Assert.AreEqual("고양이가 몰래 생선을 다 먹었다.", result.Sentence);
        }

        [Test]
        public void Evaluate_RuleWithoutSentence_UsesBuiltSentence()
        {
            var board = FullBoard("고양이", "비밀 통로", "생선 독차지");
            var result = board.Evaluate(new[] { Rule("a", "m", "o", HypothesisKind.Consistent, "A") });

            Assert.AreEqual("고양이는 비밀 통로를 통해 생선 독차지를 꾀했다.", result.Sentence);
        }

        [Test]
        public void Evaluate_ContradictionRule_HasWarning()
        {
            var board = FullBoard("고양이", "비밀 통로", "생선 독차지");
            var result = board.Evaluate(new[] { Rule("a", "m", "o", HypothesisKind.Contradiction, "C", warning: "모순 감지") });
            Debug.Log("[모순 가설] " + result.Sentence + " / " + result.Warning);

            Assert.AreEqual(HypothesisKind.Contradiction, result.Kind);
            Assert.AreEqual("C", result.ResultKey);
            Assert.AreEqual("모순 감지", result.Warning);
        }

        [Test]
        public void Evaluate_NoMatchingRule_IsContradiction()
        {
            var board = FullBoard("고양이", "비밀 통로", "생선 독차지");
            var result = board.Evaluate(new[] { Rule("a", "m", "x", HypothesisKind.Consistent, "A") });

            Assert.AreEqual(HypothesisKind.Contradiction, result.Kind);
            Assert.IsNull(result.ResultKey);
            Assert.AreEqual("고양이는 비밀 통로를 통해 생선 독차지를 꾀했다.", result.Sentence);
        }

        [Test]
        public void Evaluate_Incomplete_IsNull()
        {
            var board = new HypothesisBoard();
            Put(board, Card("a", HypothesisSlot.Actor, "고양이"));

            Assert.IsNull(board.Evaluate(new HypothesisRuleData[0]));
        }

        [Test]
        public void EvaluateForced_Incomplete_IsIncompleteContradiction()
        {
            var board = new HypothesisBoard();
            Put(board, Card("a", HypothesisSlot.Actor, "고양이"));

            var result = board.EvaluateForced(new HypothesisRuleData[0]);

            Assert.AreEqual(HypothesisKind.Contradiction, result.Kind);
            Assert.IsTrue(result.Incomplete);
            Assert.IsNull(result.ResultKey);
            Assert.IsNull(result.Sentence);
        }

        [Test]
        public void EvaluateForced_Empty_IsIncompleteContradiction()
        {
            var result = new HypothesisBoard().EvaluateForced(null);

            Assert.AreEqual(HypothesisKind.Contradiction, result.Kind);
            Assert.IsTrue(result.Incomplete);
        }

        [Test]
        public void EvaluateForced_Complete_SameAsEvaluate()
        {
            var board = FullBoard("고양이", "비밀 통로", "생선 독차지");
            var rules = new[] { Rule("a", "m", "o", HypothesisKind.Consistent, "A") };

            var result = board.EvaluateForced(rules);

            Assert.AreEqual(HypothesisKind.Consistent, result.Kind);
            Assert.AreEqual("A", result.ResultKey);
            Assert.IsFalse(result.Incomplete);
        }
    }
}
