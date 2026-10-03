using NUnit.Framework;
using Scars.Core;

namespace Scars.Tests
{
    public class GameEffectTests
    {
        [Test]
        public void Apply_GivesCluesAndSetsFlags()
        {
            var effect = new GameEffect { giveClueIds = new[] { "c1", "c2" }, setFlags = new[] { "f" } };
            var state = new GameState();
            effect.Apply(state);

            Assert.IsTrue(state.Clues.Has("c1"));
            Assert.IsTrue(state.Clues.Has("c2"));
            Assert.IsTrue(state.HasFlag("f"));
        }

        [Test]
        public void Apply_Twice_DoesNotDuplicate()
        {
            var effect = new GameEffect { giveClueIds = new[] { "c" } };
            var state = new GameState();
            effect.Apply(state);
            effect.Apply(state);

            Assert.AreEqual(1, state.Clues.Count);
        }

        [Test]
        public void Apply_IgnoresEmptyEntries()
        {
            var effect = new GameEffect { giveClueIds = new[] { "", null }, setFlags = new[] { "" } };
            var state = new GameState();
            effect.Apply(state);

            Assert.AreEqual(0, state.Clues.Count);
            Assert.IsFalse(state.HasFlag(""));
        }

        [Test]
        public void Apply_NullArraysOrState_DoesNotThrow()
        {
            var effect = new GameEffect { giveClueIds = null, setFlags = null };

            Assert.DoesNotThrow(() => effect.Apply(new GameState()));
            Assert.DoesNotThrow(() => new GameEffect().Apply(null));
        }

        [Test]
        public void Apply_DoesNotTouchEnding_HasEndingReflectsId()
        {
            var effect = new GameEffect { endingId = "e" };
            var state = new GameState();
            effect.Apply(state);

            Assert.IsTrue(effect.HasEnding);
            Assert.IsFalse(new GameEffect().HasEnding);
            Assert.AreEqual(0, state.Clues.Count);
        }

        [Test]
        public void Apply_ThenCondition_IsMet()
        {
            var effect = new GameEffect { giveClueIds = new[] { "c" }, setFlags = new[] { "f" } };
            var condition = new Condition { requiredFlags = new[] { "f" }, requiredClueIds = new[] { "c" } };
            var state = new GameState();

            Assert.IsFalse(condition.IsMet(state));
            effect.Apply(state);
            Assert.IsTrue(condition.IsMet(state));
        }
    }
}
