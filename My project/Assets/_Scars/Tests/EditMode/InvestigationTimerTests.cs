using System.Collections.Generic;
using NUnit.Framework;
using Scars.Investigation;

namespace Scars.Tests
{
    public class InvestigationTimerTests
    {
        static InvestigationTimer Started(float seconds = 420f)
        {
            var timer = new InvestigationTimer(seconds);
            timer.Start();
            return timer;
        }

        [Test]
        public void New_FullTimeNormalNotRunning()
        {
            var timer = new InvestigationTimer(420f);

            Assert.AreEqual(420, timer.DisplaySeconds);
            Assert.AreEqual(TimerStage.Normal, timer.Stage);
            Assert.IsFalse(timer.IsLastMinute);
            Assert.IsFalse(timer.IsRunning);
        }

        [Test]
        public void Tick_BeforeStart_Ignored()
        {
            var timer = new InvestigationTimer(420f);
            timer.Tick(10f);
            Assert.AreEqual(420f, timer.Remaining);
        }

        [Test]
        public void Tick_AfterStart_CountsDown()
        {
            var timer = Started();
            timer.Tick(1.5f);
            Assert.AreEqual(418.5f, timer.Remaining, 0.0001f);
            Assert.AreEqual(419, timer.DisplaySeconds);
        }

        [Test]
        public void Tick_ZeroOrNegative_Ignored()
        {
            var timer = Started();
            timer.Tick(0f);
            timer.Tick(-5f);
            Assert.AreEqual(420f, timer.Remaining);
        }

        // 7분(420초)에서 흐른 시간 → 단계. 경계는 보이는 초(올림) 기준
        [TestCase(300f, TimerStage.Normal)]   // 남은 120 = 02:00
        [TestCase(300.5f, TimerStage.Normal)] // 남은 119.5 → 보이는 02:00
        [TestCase(301f, TimerStage.Warning)]  // 01:59
        [TestCase(390f, TimerStage.Warning)]  // 00:30
        [TestCase(391f, TimerStage.Danger)]   // 00:29
        [TestCase(420f, TimerStage.Danger)]   // 00:00
        public void Stage_FollowsShownSeconds(float elapsed, TimerStage expected)
        {
            var timer = Started();
            timer.Tick(elapsed);
            Assert.AreEqual(expected, timer.Stage);
        }

        [Test]
        public void StageChanged_FiresOncePerChange()
        {
            var timer = Started(121f);
            var stages = new List<TimerStage>();
            timer.StageChanged += stages.Add;

            for (int i = 0; i < 121; i++) timer.Tick(1f);

            CollectionAssert.AreEqual(new[] { TimerStage.Warning, TimerStage.Danger }, stages);
        }

        [Test]
        public void StageChanged_BigTick_JumpsToFinalStageOnce()
        {
            var timer = Started();
            var stages = new List<TimerStage>();
            timer.StageChanged += stages.Add;

            timer.Tick(400f);

            CollectionAssert.AreEqual(new[] { TimerStage.Danger }, stages);
        }

        [Test]
        public void LastMinute_StartsAt59()
        {
            var timer = Started(61f);
            int count = 0;
            timer.LastMinuteStarted += () => count++;

            timer.Tick(1f);    // 60 = 01:00
            Assert.IsFalse(timer.IsLastMinute);
            timer.Tick(1f);    // 59 = 00:59
            Assert.IsTrue(timer.IsLastMinute);
            timer.Tick(30f);
            Assert.AreEqual(1, count);
        }

        [Test]
        public void Expired_FiresOnce_ThenTickIgnored()
        {
            var timer = Started(10f);
            int count = 0;
            timer.Expired += () => count++;

            timer.Tick(30f);
            timer.Tick(1f);

            Assert.AreEqual(1, count);
            Assert.AreEqual(0f, timer.Remaining);
            Assert.AreEqual(0, timer.DisplaySeconds);
            Assert.IsTrue(timer.IsExpired);
            Assert.IsFalse(timer.IsRunning);
        }

        [Test]
        public void EventOrder_StageThenLastMinuteThenExpired()
        {
            var timer = Started();
            var log = new List<string>();
            timer.StageChanged += s => log.Add("stage:" + s);
            timer.LastMinuteStarted += () => log.Add("lastMinute");
            timer.Expired += () => log.Add("expired");

            timer.Tick(500f);

            CollectionAssert.AreEqual(new[] { "stage:Danger", "lastMinute", "expired" }, log);
        }

        [Test]
        public void Pause_StopsTime_ResumeContinues()
        {
            var timer = Started();
            timer.Pause("journal");
            timer.Tick(10f);
            Assert.AreEqual(420f, timer.Remaining);
            Assert.IsTrue(timer.IsPaused);

            timer.Resume("journal");
            timer.Tick(10f);
            Assert.AreEqual(410f, timer.Remaining);
        }

        [Test]
        public void Pause_Overlapping_RunsOnlyWhenAllResumed()
        {
            var timer = Started();
            timer.Pause("journal");
            timer.Pause("judgment");

            timer.Resume("journal");
            timer.Tick(10f);
            Assert.AreEqual(420f, timer.Remaining);

            timer.Resume("judgment");
            timer.Tick(10f);
            Assert.AreEqual(410f, timer.Remaining);
        }

        [Test]
        public void Pause_SameReasonTwice_OneResumeEnough()
        {
            var timer = Started();
            timer.Pause("dialogue");
            timer.Pause("dialogue");
            timer.Resume("dialogue");
            Assert.IsFalse(timer.IsPaused);
        }

        [Test]
        public void Pause_EmptyReason_Ignored()
        {
            var timer = Started();
            timer.Pause(null);
            timer.Pause("");
            Assert.IsFalse(timer.IsPaused);
        }

        [Test]
        public void Pause_BeforeStart_StillHoldsAfterStart()
        {
            var timer = new InvestigationTimer(420f);
            timer.Pause("dialogue");
            timer.Start();
            timer.Tick(10f);
            Assert.AreEqual(420f, timer.Remaining);
        }

        [Test]
        public void Stop_FreezesWithoutExpired()
        {
            var timer = Started();
            bool expired = false;
            timer.Expired += () => expired = true;

            timer.Tick(100f);
            timer.Stop();
            timer.Tick(500f);

            Assert.AreEqual(320f, timer.Remaining);
            Assert.IsTrue(timer.IsStopped);
            Assert.IsFalse(timer.IsRunning);
            Assert.IsFalse(expired);
        }

        [Test]
        public void Start_Twice_KeepsRemaining()
        {
            var timer = Started();
            timer.Tick(100f);
            timer.Start();
            Assert.AreEqual(320f, timer.Remaining);
        }

        [Test]
        public void NegativeDuration_ClampedToZero()
        {
            var timer = new InvestigationTimer(-5f);
            Assert.AreEqual(0f, timer.Duration);
            Assert.AreEqual(TimerStage.Danger, timer.Stage);
        }
    }
}
