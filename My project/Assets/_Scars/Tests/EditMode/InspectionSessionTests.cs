using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Scars.Core;
using Scars.Data;
using Scars.Investigation;
using UnityEditor;
using UnityEngine;

namespace Scars.Tests
{
    public class InspectionSessionTests
    {
        readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created) Object.DestroyImmediate(obj);
            _created.Clear();
        }

        QuizData MakeJudgment(string solution, params string[] options)
        {
            var quiz = ScriptableObject.CreateInstance<QuizData>();
            var so = new SerializedObject(quiz);
            SetArray(so.FindProperty("solution"), new[] { solution });
            SetArray(so.FindProperty("options"), options);
            so.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(quiz);
            return quiz;
        }

        static void SetArray(SerializedProperty prop, string[] values)
        {
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) prop.GetArrayElementAtIndex(i).stringValue = values[i];
        }

        InspectableData MakeTarget(string grant = "base", string item = null, string[] steps = null,
            QuizData judgment = null, string upgrade = "up")
        {
            var target = ScriptableObject.CreateInstance<InspectableData>();
            var so = new SerializedObject(target);
            so.FindProperty("id").stringValue = "target";
            so.ApplyModifiedPropertiesWithoutUndo();
            target.grantClueId = grant;
            target.requiredItemId = item;
            target.steps = steps ?? new string[0];
            target.judgment = judgment;
            target.upgradeClueId = upgrade;
            _created.Add(target);
            return target;
        }

        static InspectionSession Start(InspectableData target, GameState state)
        {
            var session = new InspectionSession();
            session.Begin(target, state);
            return session;
        }

        [Test]
        public void Collect_GivesBaseCardOnce()
        {
            var state = new GameState();
            var s = Start(MakeTarget(), state);

            Assert.IsTrue(s.Collect());
            Assert.IsFalse(s.Collect());
            Assert.IsTrue(state.Clues.Has("base"));
        }

        [Test]
        public void DeepInspect_RequiresBaseCardFirst()
        {
            var state = new GameState();
            var s = Start(MakeTarget(judgment: MakeJudgment("y", "n", "y")), state);

            Assert.IsFalse(s.CanDeepInspect());
            Assert.IsFalse(s.Judge("y"));
            s.Collect();
            Assert.IsTrue(s.CanDeepInspect());
        }

        [Test]
        public void FullFlow_Item_Steps_DeepTruth_UpgradesInPlace()
        {
            var state = new GameState();
            state.Clues.Add("other");
            state.Clues.Add("tool");
            var s = Start(MakeTarget(item: "tool", steps: new[] { "s1", "s2" }, judgment: MakeJudgment("y", "n", "y")), state);
            s.Collect();

            Assert.IsFalse(s.ClickStep("s1"), "도구 전에는 조작 불가");
            Assert.IsTrue(s.UseItem("tool"));
            Assert.IsFalse(s.Judge("y"), "조작 전에는 판별 불가");
            Assert.IsTrue(s.ClickStep("s1"));
            Assert.IsTrue(s.ClickStep("s2"));
            Assert.IsTrue(s.StepsDone);
            Assert.IsTrue(s.Judge("y"));

            Assert.IsTrue(s.IsUpgraded);
            CollectionAssert.AreEqual(new[] { "other", "tool", "up" }, state.Clues.All.ToArray());
            Assert.IsFalse(s.CanDeepInspect());
        }

        [Test]
        public void UseItem_OtherOrNotOwned_IsNotUsed()
        {
            var state = new GameState();
            var s = Start(MakeTarget(item: "tool", judgment: MakeJudgment("y", "n", "y")), state);
            s.Collect();

            Assert.IsFalse(s.UseItem("tool"), "도구 없음");
            state.Clues.Add("wrong");
            Assert.IsFalse(s.UseItem("wrong"));
            Assert.IsFalse(s.ItemReady);
        }

        [Test]
        public void ClickStep_OtherTarget_KeepsProgress()
        {
            var state = new GameState();
            var s = Start(MakeTarget(steps: new[] { "s1", "s2", "s3" }, judgment: MakeJudgment("y", "y")), state);
            s.Collect();

            Assert.IsTrue(s.ClickStep("s1"));
            Assert.IsFalse(s.ClickStep("s3"));
            Assert.AreEqual(1, s.StepProgress);
            Assert.IsTrue(s.ClickStep("s2"));
        }

        [Test]
        public void Judge_SurfaceReading_KeepsBaseCard_EvenAfterReopen()
        {
            var state = new GameState();
            var target = MakeTarget(judgment: MakeJudgment("y", "n", "y"));
            var s = Start(target, state);
            s.Collect();

            Assert.IsFalse(s.Judge("n"));
            Assert.IsTrue(s.IsBaseCardKept);

            var reopened = Start(target, state);
            Assert.IsFalse(reopened.CanDeepInspect());
            Assert.IsFalse(reopened.Judge("y"));
            Assert.IsTrue(state.Clues.Has("base"));
        }

        [TestCase("없는 보기")]
        [TestCase("")]
        [TestCase(null)]
        public void Judge_InvalidAnswer_DoesNotLock(string answer)
        {
            var state = new GameState();
            var s = Start(MakeTarget(judgment: MakeJudgment("y", "n", "y")), state);
            s.Collect();

            Assert.IsFalse(s.Judge(answer));
            Assert.IsFalse(s.IsBaseCardKept, "잘못된 입력으로 판별 기회를 잃으면 안 된다");
            Assert.IsTrue(s.Judge("y"));
            Assert.IsTrue(s.IsUpgraded);
        }

        [Test]
        public void HiddenMechanism_NoBaseCard_StepsOnly_GivesUpgradeCard()
        {
            var state = new GameState();
            var s = Start(MakeTarget(grant: "", steps: new[] { "a", "b" }), state);

            Assert.IsFalse(s.Collect());
            Assert.IsTrue(s.CanDeepInspect());
            s.ClickStep("a");
            Assert.IsFalse(state.Clues.Has("up"));
            s.ClickStep("b");
            Assert.IsTrue(state.Clues.Has("up"));
            Assert.IsTrue(s.IsUpgraded);
        }

        [Test]
        public void Reopen_ResetsItemAndSteps()
        {
            var state = new GameState();
            state.Clues.Add("tool");
            var target = MakeTarget(item: "tool", steps: new[] { "s1", "s2" }, judgment: MakeJudgment("y", "y"));
            var s = Start(target, state);
            s.Collect();
            s.UseItem("tool");
            s.ClickStep("s1");

            var reopened = Start(target, state);
            Assert.IsFalse(reopened.ItemReady);
            Assert.AreEqual(0, reopened.StepProgress);
        }

        [Test]
        public void NoDeepInspection_CannotDeepInspect()
        {
            var state = new GameState();
            var s = Start(MakeTarget(upgrade: ""), state);
            s.Collect();

            Assert.IsFalse(s.HasDeepInspection);
            Assert.IsFalse(s.CanDeepInspect());
        }

        [Test]
        public void NullTargetOrState_IsSafe()
        {
            var s = Start(null, new GameState());

            Assert.IsFalse(s.Collect());
            Assert.IsFalse(s.CanDeepInspect());
            Assert.IsFalse(s.UseItem("x"));
            Assert.IsFalse(s.ClickStep("x"));
            Assert.IsFalse(s.Judge("x"));
            Assert.IsFalse(Start(MakeTarget(), null).Collect());
        }
    }
}
