using NUnit.Framework;
using Scars.Core;

namespace Scars.Tests
{
    public class ConditionTests
    {
        static Condition Make(string[] flags = null, string[] clues = null)
        {
            return new Condition { requiredFlags = flags ?? new string[0], requiredClueIds = clues ?? new string[0] };
        }

        [Test]
        public void Empty_IsAlwaysMet()
        {
            Assert.IsTrue(Make().IsMet(new GameState()));
            Assert.IsTrue(Make().IsMet(null));
        }

        [Test]
        public void RequiredFlag_MetOnlyWhenSet()
        {
            var c = Make(flags: new[] { "f" });
            var state = new GameState();

            Assert.IsFalse(c.IsMet(state));
            state.SetFlag("f");
            Assert.IsTrue(c.IsMet(state));
        }

        [Test]
        public void RequiredClue_MetOnlyWhenOwned()
        {
            var c = Make(clues: new[] { "c" });
            var state = new GameState();

            Assert.IsFalse(c.IsMet(state));
            state.Clues.Add("c");
            Assert.IsTrue(c.IsMet(state));
        }

        [Test]
        public void AllRequirements_MustBeMet()
        {
            var c = Make(new[] { "f1", "f2" }, new[] { "c" });
            var state = new GameState();
            state.SetFlag("f1");
            state.Clues.Add("c");

            Assert.IsFalse(c.IsMet(state));
            state.SetFlag("f2");
            Assert.IsTrue(c.IsMet(state));
        }

        [Test]
        public void EmptyEntries_AreIgnored()
        {
            var c = Make(new[] { "", null }, new[] { "" });

            Assert.IsTrue(c.IsMet(new GameState()));
        }

        [Test]
        public void NullArrays_AreTreatedAsEmpty()
        {
            var c = new Condition { requiredFlags = null, requiredClueIds = null };

            Assert.IsTrue(c.IsMet(new GameState()));
        }

        [Test]
        public void NullState_WithRequirement_IsNotMet()
        {
            Assert.IsFalse(Make(flags: new[] { "f" }).IsMet(null));
            Assert.IsFalse(Make(clues: new[] { "c" }).IsMet(null));
        }
    }
}
