using NUnit.Framework;
using Scars.Clues;

namespace Scars.Tests
{
    public class ClueInventoryTests
    {
        [Test]
        public void Add_NewClue_IsHeld()
        {
            var inv = new ClueInventory();

            Assert.IsTrue(inv.Add("a"));
            Assert.IsTrue(inv.Has("a"));
            Assert.AreEqual(1, inv.Count);
        }

        [Test]
        public void Add_SameClueTwice_CountsOnce()
        {
            var inv = new ClueInventory();
            inv.Add("a");

            Assert.IsFalse(inv.Add("a"));
            Assert.AreEqual(1, inv.Count);
        }

        [Test]
        public void Has_UnknownClue_IsFalse()
        {
            Assert.IsFalse(new ClueInventory().Has("a"));
        }

        [TestCase(null)]
        [TestCase("")]
        public void Add_EmptyId_IsIgnored(string id)
        {
            var inv = new ClueInventory();

            Assert.IsFalse(inv.Add(id));
            Assert.IsFalse(inv.Has(id));
            Assert.AreEqual(0, inv.Count);
        }
    }
}
