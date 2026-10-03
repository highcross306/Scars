using NUnit.Framework;
using Scars.Core;

namespace Scars.Tests
{
    public class GameStateTests
    {
        [Test]
        public void NewState_IsEmpty()
        {
            var state = new GameState();

            Assert.AreEqual(0, state.Clues.Count);
            Assert.IsFalse(state.HasFlag("x"));
        }

        [Test]
        public void SetFlag_ThenHasFlag()
        {
            var state = new GameState();
            state.SetFlag("x");

            Assert.IsTrue(state.HasFlag("x"));
            Assert.IsFalse(state.HasFlag("y"));
        }

        [Test]
        public void ClearFlag_RemovesFlag()
        {
            var state = new GameState();
            state.SetFlag("x");
            state.ClearFlag("x");

            Assert.IsFalse(state.HasFlag("x"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void EmptyFlag_IsIgnored(string flag)
        {
            var state = new GameState();
            state.SetFlag(flag);

            Assert.IsFalse(state.HasFlag(flag));
        }

        [Test]
        public void Clues_KeepsAddedClues()
        {
            var state = new GameState();
            state.Clues.Add("a");

            Assert.IsTrue(state.Clues.Has("a"));
        }
    }
}
