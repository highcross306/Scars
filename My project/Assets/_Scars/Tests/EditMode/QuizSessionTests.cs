using NUnit.Framework;
using Scars.Quiz;

namespace Scars.Tests
{
    public class QuizSessionTests
    {
        static QuizSession Start(string[] solution, string[] options = null)
        {
            var session = new QuizSession();
            session.Begin(solution, options);
            return session;
        }

        [Test]
        public void Begin_SetsSlotCount_AllEmpty()
        {
            var s = Start(new[] { "1", "2", "3" });

            Assert.AreEqual(3, s.SlotCount);
            Assert.IsNull(s.GetAnswer(0));
            Assert.AreEqual(0, s.Submit());
        }

        [Test]
        public void Submit_AllCorrect_ReturnsSlotCount()
        {
            var s = Start(new[] { "1", "2", "3" });
            s.SetAnswer(0, "1");
            s.SetAnswer(1, "2");
            s.SetAnswer(2, "3");

            Assert.AreEqual(3, s.Submit());
        }

        [Test]
        public void Submit_CountsOnlyCorrectSlots()
        {
            var s = Start(new[] { "1", "2", "3" });
            s.SetAnswer(0, "1");
            s.SetAnswer(1, "9");

            Assert.AreEqual(1, s.Submit());
        }

        [Test]
        public void Submit_SameValueWrongPosition_NotCounted()
        {
            var s = Start(new[] { "A", "B" });
            s.SetAnswer(0, "B");
            s.SetAnswer(1, "A");

            Assert.AreEqual(0, s.Submit());
        }

        [Test]
        public void SetAnswer_OutOfRange_ReturnsFalse()
        {
            var s = Start(new[] { "1" });

            Assert.IsFalse(s.SetAnswer(-1, "1"));
            Assert.IsFalse(s.SetAnswer(1, "1"));
            Assert.IsNull(s.GetAnswer(1));
        }

        [Test]
        public void SetAnswer_NotInOptions_ReturnsFalse()
        {
            var s = Start(new[] { "1" }, new[] { "0", "1", "2" });

            Assert.IsFalse(s.SetAnswer(0, "7"));
            Assert.IsNull(s.GetAnswer(0));
            Assert.IsTrue(s.SetAnswer(0, "2"));
            Assert.AreEqual("2", s.GetAnswer(0));
        }

        [Test]
        public void SetAnswer_NoOptions_AcceptsAnyValue_ButNotEmpty()
        {
            var s = Start(new[] { "열쇠" });

            Assert.IsTrue(s.SetAnswer(0, "아무거나"));
            Assert.IsFalse(s.SetAnswer(0, ""));
            Assert.AreEqual("아무거나", s.GetAnswer(0));
        }

        [Test]
        public void SetAnswer_Overwrites()
        {
            var s = Start(new[] { "1" });
            s.SetAnswer(0, "5");
            s.SetAnswer(0, "1");

            Assert.AreEqual(1, s.Submit());
        }

        [Test]
        public void ClearAnswer_EmptiesSlot()
        {
            var s = Start(new[] { "1", "2" });
            s.SetAnswer(0, "1");
            s.ClearAnswer(0);
            s.ClearAnswer(5);

            Assert.IsNull(s.GetAnswer(0));
            Assert.AreEqual(0, s.Submit());
        }

        [Test]
        public void Begin_Again_ResetsAnswers()
        {
            var s = Start(new[] { "1", "2" });
            s.SetAnswer(0, "1");
            s.Begin(new[] { "x" }, null);

            Assert.AreEqual(1, s.SlotCount);
            Assert.IsNull(s.GetAnswer(0));
        }

        [Test]
        public void Begin_Null_HasNoSlots()
        {
            var s = new QuizSession();
            s.Begin(null);

            Assert.AreEqual(0, s.SlotCount);
            Assert.IsFalse(s.SetAnswer(0, "1"));
            Assert.AreEqual(0, s.Submit());
        }
    }
}
