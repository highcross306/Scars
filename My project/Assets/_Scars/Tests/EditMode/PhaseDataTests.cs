using NUnit.Framework;
using Scars.Data;
using UnityEngine;

namespace Scars.Tests
{
    public class PhaseDataTests
    {
        PhaseData _phase;

        [SetUp]
        public void SetUp() { _phase = ScriptableObject.CreateInstance<PhaseData>(); }

        [TearDown]
        public void TearDown() { Object.DestroyImmediate(_phase); }

        [Test]
        public void New_NoTimeLimit()
        {
            Assert.AreEqual(0f, _phase.timeLimitSeconds);
            Assert.IsFalse(_phase.HasTimeLimit);
        }

        [TestCase(420f, true)]
        [TestCase(0f, false)]
        [TestCase(-1f, false)]
        public void HasTimeLimit_OnlyWhenPositive(float seconds, bool expected)
        {
            _phase.timeLimitSeconds = seconds;
            Assert.AreEqual(expected, _phase.HasTimeLimit);
        }
    }
}
