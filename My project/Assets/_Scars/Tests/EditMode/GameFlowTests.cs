using System.Collections.Generic;
using NUnit.Framework;
using Scars.Core;
using Scars.Data;
using Scars.Dialogue;
using Scars.Hypothesis;
using UnityEditor;
using UnityEngine;

namespace Scars.Tests
{
    public class GameFlowTests
    {
        readonly List<Object> _created = new List<Object>();
        GameDatabase _db;

        [SetUp]
        public void SetUp()
        {
            _db = ScriptableObject.CreateInstance<GameDatabase>();
            _created.Add(_db);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created) Object.DestroyImmediate(obj);
            _created.Clear();
        }

        T Make<T>(string id) where T : DataAsset
        {
            var asset = ScriptableObject.CreateInstance<T>();
            var so = new SerializedObject(asset);
            so.FindProperty("id").stringValue = id;
            so.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(asset);
            _db.Register(asset);
            return asset;
        }

        PhaseData Phase(string id, PhaseKind kind = PhaseKind.Dialogue, string next = null, string dialogue = null)
        {
            var phase = Make<PhaseData>(id);
            phase.kind = kind;
            phase.nextPhaseId = next;
            phase.startDialogueId = dialogue;
            return phase;
        }

        PhaseData TimedPhase(string id, float seconds, string next = "next", string timeoutDialogue = null, string timeoutPhase = "hypo")
        {
            var phase = Phase(id, PhaseKind.Investigation, next);
            phase.timeLimitSeconds = seconds;
            phase.timeoutDialogueId = timeoutDialogue;
            phase.timeoutPhaseId = timeoutPhase;
            return phase;
        }

        // 노드 하나짜리 대화. ending이 있으면 들어가자마자 엔딩
        DialogueData Talk(string id, string ending = null)
        {
            var dialogue = Make<DialogueData>(id);
            dialogue.startNodeId = "n";
            var node = new DialogueNode { id = "n" };
            node.onEnter.endingId = ending;
            dialogue.nodes.Add(node);
            return dialogue;
        }

        GameFlow Flow() { return new GameFlow(_db); }

        // ── 단계 진행 ──

        [Test]
        public void NewGame_EntersFirstPhase()
        {
            Phase("p1");
            var flow = Flow();
            PhaseData changed = null;
            flow.PhaseChanged += p => changed = p;

            Assert.IsTrue(flow.NewGame("p1"));
            Assert.AreEqual("p1", flow.CurrentPhase.Id);
            Assert.AreSame(flow.CurrentPhase, changed);
        }

        [Test]
        public void NewGame_ResetsState()
        {
            Phase("p1");
            var flow = Flow();
            flow.NewGame("p1");
            flow.State.SetFlag("old");

            flow.NewGame("p1");

            Assert.IsFalse(flow.State.HasFlag("old"));
        }

        [Test]
        public void GoTo_UnknownPhase_False()
        {
            var flow = Flow();
            Assert.IsFalse(flow.GoTo("none"));
            Assert.IsNull(flow.CurrentPhase);
        }

        [Test]
        public void GoTo_AppliesOnEnter()
        {
            var phase = Phase("p1");
            phase.onEnter.giveClueIds = new[] { "arthur" };
            var flow = Flow();
            flow.NewGame("p1");
            Assert.IsTrue(flow.State.Clues.Has("arthur"));
        }

        [Test]
        public void GoTo_OnEnterEnding_ReachesEnding()
        {
            var phase = Phase("p1");
            phase.onEnter.endingId = "e1";
            var flow = Flow();
            flow.NewGame("p1");
            Assert.AreEqual("e1", flow.EndingId);
        }

        [Test]
        public void GoTo_StartsPhaseDialogue()
        {
            Talk("d1");
            Phase("p1", dialogue: "d1");
            var flow = Flow();
            DialogueRunner started = null;
            flow.DialogueStarted += r => started = r;

            flow.NewGame("p1");

            Assert.IsNotNull(flow.Dialogue);
            Assert.AreSame(flow.Dialogue, started);
        }

        [Test]
        public void CompletePhase_GoesToNext()
        {
            Phase("p1", next: "p2");
            Phase("p2");
            var flow = Flow();
            flow.NewGame("p1");

            Assert.IsTrue(flow.CompletePhase());
            Assert.AreEqual("p2", flow.CurrentPhase.Id);
        }

        [Test]
        public void Tick_FinishedDialogueCleared()
        {
            Talk("d1");
            Phase("p1", dialogue: "d1");
            var flow = Flow();
            flow.NewGame("p1");

            flow.Dialogue.Advance();
            flow.Tick(0.1f);

            Assert.IsNull(flow.Dialogue);
        }

        [Test]
        public void DialogueEnding_ReachesEnding_ThenGoToBlocked()
        {
            Talk("d1", ending: "e2");
            Phase("p1", dialogue: "d1");
            Phase("p2");
            var flow = Flow();
            string ending = null;
            flow.EndingReached += e => ending = e;

            flow.NewGame("p1");

            Assert.AreEqual("e2", ending);
            Assert.IsTrue(flow.IsEnded);
            Assert.IsFalse(flow.GoTo("p2"));
        }

        [Test]
        public void ReachEnding_OnlyOnce()
        {
            Phase("p1");
            var flow = Flow();
            flow.NewGame("p1");
            int count = 0;
            flow.EndingReached += _ => count++;

            flow.ReachEnding("e1");
            flow.ReachEnding("e4");

            Assert.AreEqual(1, count);
            Assert.AreEqual("e1", flow.EndingId);
        }

        // ── 가설 확정 ──

        [Test]
        public void ConfirmHypothesis_SetsFlagsAndGoesNext()
        {
            Phase("hypo", PhaseKind.Hypothesis, next: "interro");
            Phase("interro", PhaseKind.Interrogation);
            var flow = Flow();
            flow.NewGame("hypo");

            Assert.IsTrue(flow.ConfirmHypothesis(new HypothesisResult { Kind = HypothesisKind.Consistent, ResultKey = "B" }));

            Assert.IsTrue(flow.State.HasFlag("hypothesis:Consistent"));
            Assert.IsTrue(flow.State.HasFlag("hypothesis:B"));
            Assert.AreEqual("interro", flow.CurrentPhase.Id);
        }

        // 서재(제한 시간) → 시간 초과 → 가설 단계
        GameFlow TimedOutIntoHypothesis()
        {
            TimedPhase("study", 10f);
            Phase("hypo", PhaseKind.Hypothesis, next: "interro");
            Phase("interro", PhaseKind.Interrogation);
            var flow = Flow();
            flow.NewGame("study");
            flow.Timer.Start();
            flow.Tick(11f);
            return flow;
        }

        [Test]
        public void IsForcedHypothesis_OnlyAfterTimeout()
        {
            var flow = TimedOutIntoHypothesis();
            Assert.IsTrue(flow.IsForcedHypothesis);

            Phase("hypo2", PhaseKind.Hypothesis);
            var normal = Flow();
            normal.NewGame("hypo2");
            Assert.IsFalse(normal.IsForcedHypothesis);
        }

        [Test]
        public void ConfirmHypothesis_IncompleteWhenForced_SetsFlagsAndGoesNext()
        {
            var flow = TimedOutIntoHypothesis();

            Assert.IsTrue(flow.ConfirmHypothesis(new HypothesisBoard().EvaluateForced(null)));

            Assert.IsTrue(flow.State.HasFlag("hypothesis:Contradiction"));
            Assert.IsTrue(flow.State.HasFlag("hypothesis:Incomplete"));
            Assert.AreEqual("interro", flow.CurrentPhase.Id);
            Assert.IsFalse(flow.IsForcedHypothesis);
        }

        [Test]
        public void ConfirmHypothesis_IncompleteWithoutTimeout_Rejected()
        {
            Phase("hypo", PhaseKind.Hypothesis, next: "interro");
            Phase("interro", PhaseKind.Interrogation);
            var flow = Flow();
            flow.NewGame("hypo");

            Assert.IsFalse(flow.ConfirmHypothesis(new HypothesisBoard().EvaluateForced(null)));

            Assert.AreEqual("hypo", flow.CurrentPhase.Id);
            Assert.IsFalse(flow.State.HasFlag("hypothesis:Contradiction"));
        }

        [Test]
        public void ConfirmHypothesis_ContradictionWithoutKey_KindFlagOnly()
        {
            Phase("hypo", PhaseKind.Hypothesis, next: "interro");
            Phase("interro", PhaseKind.Interrogation);
            var flow = Flow();
            flow.NewGame("hypo");

            flow.ConfirmHypothesis(new HypothesisResult { Kind = HypothesisKind.Contradiction });

            Assert.IsTrue(flow.State.HasFlag("hypothesis:Contradiction"));
        }

        [Test]
        public void ConfirmHypothesis_Null_False()
        {
            Phase("hypo", PhaseKind.Hypothesis);
            var flow = Flow();
            flow.NewGame("hypo");
            Assert.IsFalse(flow.ConfirmHypothesis(null));
        }

        // ── 조사 제한 시간 ──

        [Test]
        public void TimedPhase_CreatesTimer_NotStarted()
        {
            TimedPhase("study", 420f);
            var flow = Flow();
            flow.NewGame("study");

            Assert.IsNotNull(flow.Timer);
            Assert.AreEqual(420f, flow.Timer.Duration);
            Assert.IsFalse(flow.Timer.IsStarted);
            Assert.IsTrue(flow.CanInvestigate);
        }

        [Test]
        public void UntimedPhase_NoTimer()
        {
            Phase("p1");
            var flow = Flow();
            flow.NewGame("p1");
            Assert.IsNull(flow.Timer);
            Assert.IsFalse(flow.CanInvestigate);
        }

        [Test]
        public void Tick_RunsTimerAfterStart()
        {
            TimedPhase("study", 420f);
            var flow = Flow();
            flow.NewGame("study");

            flow.Tick(10f);
            Assert.AreEqual(420f, flow.Timer.Remaining);

            flow.Timer.Start();
            flow.Tick(10f);
            Assert.AreEqual(410f, flow.Timer.Remaining);
        }

        [Test]
        public void Dialogue_PausesTimer_ResumesWhenFinished()
        {
            TimedPhase("study", 420f);
            Talk("mono");
            var flow = Flow();
            flow.NewGame("study");
            flow.Timer.Start();

            flow.StartDialogue("mono");
            flow.Tick(10f);
            Assert.AreEqual(420f, flow.Timer.Remaining);

            flow.Dialogue.Advance();
            flow.Tick(10f);   // 끝난 대화 정리 → 재개 → 10초
            Assert.AreEqual(410f, flow.Timer.Remaining);
        }

        [Test]
        public void LeaveByDoor_StopsTimer_GoesNext()
        {
            TimedPhase("study", 420f, next: "next");
            Phase("next");
            var flow = Flow();
            flow.NewGame("study");
            flow.Timer.Start();
            var timer = flow.Timer;

            Assert.IsTrue(flow.CompletePhase());

            Assert.IsTrue(timer.IsStopped);
            Assert.AreEqual("next", flow.CurrentPhase.Id);
            Assert.IsFalse(flow.State.HasFlag(GameFlow.TimeoutFlag));
        }

        [Test]
        public void Timeout_WithoutDialogue_GoesStraightToHypothesis()
        {
            TimedPhase("study", 10f);
            Phase("hypo", PhaseKind.Hypothesis);
            var flow = Flow();
            bool timedOut = false;
            flow.TimedOut += () => timedOut = true;
            flow.NewGame("study");
            flow.Timer.Start();

            flow.Tick(11f);

            Assert.IsTrue(timedOut);
            Assert.AreEqual("hypo", flow.CurrentPhase.Id);
            Assert.IsTrue(flow.State.HasFlag(GameFlow.TimeoutFlag));
            Assert.IsNull(flow.Timer);
        }

        [Test]
        public void Timeout_PlaysDialogue_ThenHypothesis()
        {
            TimedPhase("study", 10f, timeoutDialogue: "raid");
            Talk("raid");
            Phase("hypo", PhaseKind.Hypothesis);
            var flow = Flow();
            flow.NewGame("study");
            flow.Timer.Start();

            flow.Tick(11f);

            // 타임아웃 연출 중: 아직 서재, 조사 불가, 스스로 나가기 불가
            Assert.AreEqual("study", flow.CurrentPhase.Id);
            Assert.IsNotNull(flow.Dialogue);
            Assert.IsTrue(flow.IsTimedOut);
            Assert.IsFalse(flow.CanInvestigate);
            Assert.IsFalse(flow.CompletePhase());

            flow.Dialogue.Advance();
            flow.Tick(0.1f);

            Assert.AreEqual("hypo", flow.CurrentPhase.Id);
            Assert.IsNull(flow.Dialogue);
        }

        [Test]
        public void Timeout_KeepsCluesCollectedSoFar()
        {
            TimedPhase("study", 10f);
            Phase("hypo", PhaseKind.Hypothesis);
            var flow = Flow();
            flow.NewGame("study");
            flow.Timer.Start();
            flow.State.Clues.Add("piano-wire");

            flow.Tick(11f);

            Assert.IsTrue(flow.State.Clues.Has("piano-wire"));
        }

        [Test]
        public void Ending_StopsTimer()
        {
            TimedPhase("study", 420f);
            var flow = Flow();
            flow.NewGame("study");
            flow.Timer.Start();

            flow.ReachEnding("e2");
            flow.Tick(500f);

            Assert.IsTrue(flow.Timer.IsStopped);
            Assert.IsFalse(flow.Timer.IsExpired);
        }
    }
}
