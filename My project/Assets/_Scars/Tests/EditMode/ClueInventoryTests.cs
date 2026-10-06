using System.Collections.Generic;
using System.Linq;
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

        [Test]
        public void All_KeepsAcquisitionOrder()
        {
            var inv = new ClueInventory();
            inv.Add("b");
            inv.Add("a");
            inv.Add("c");

            CollectionAssert.AreEqual(new[] { "b", "a", "c" }, inv.All.ToArray());
        }

        [Test]
        public void Upgrade_ReplacesInSamePosition()
        {
            var inv = new ClueInventory();
            inv.Add("a");
            inv.Add("b");
            inv.Add("c");

            Assert.IsTrue(inv.Upgrade("b", "b+"));
            CollectionAssert.AreEqual(new[] { "a", "b+", "c" }, inv.All.ToArray());
            Assert.IsFalse(inv.Has("b"));
            Assert.IsTrue(inv.Has("b+"));
            Assert.AreEqual(3, inv.Count);
        }

        [Test]
        public void Upgrade_WithoutBaseCard_Fails()
        {
            var inv = new ClueInventory();

            Assert.IsFalse(inv.Upgrade("a", "a+"));
            Assert.IsFalse(inv.Has("a+"));
        }

        [Test]
        public void Upgrade_ToOwnedOrEmptyOrSameId_Fails()
        {
            var inv = new ClueInventory();
            inv.Add("a");
            inv.Add("x");

            Assert.IsFalse(inv.Upgrade("a", "x"));
            Assert.IsFalse(inv.Upgrade("a", ""));
            Assert.IsFalse(inv.Upgrade("a", null));
            Assert.IsFalse(inv.Upgrade("a", "a"));
            CollectionAssert.AreEqual(new[] { "a", "x" }, inv.All.ToArray());
        }

        [Test]
        public void Add_BaseCardAfterUpgrade_IsRejected()
        {
            var inv = new ClueInventory();
            inv.Add("a");
            inv.Upgrade("a", "a+");

            Assert.IsFalse(inv.Add("a"));
            Assert.IsFalse(inv.Has("a"));
            Assert.AreEqual(1, inv.Count);
        }

        [Test]
        public void Upgrade_BackToUpgradedAwayCard_IsRejected()
        {
            var inv = new ClueInventory();
            inv.Add("a");
            inv.Upgrade("a", "a+");

            Assert.IsFalse(inv.Upgrade("a+", "a"));
            Assert.IsTrue(inv.Has("a+"));
            Assert.IsFalse(inv.Has("a"));
        }

        [Test]
        public void Events_FireOnlyOnRealChanges()
        {
            var inv = new ClueInventory();
            var added = new List<string>();
            var upgraded = new List<string>();
            inv.Added += id => added.Add(id);
            inv.Upgraded += (from, to) => upgraded.Add(from + ">" + to);

            inv.Add("a");
            inv.Add("a");
            inv.Add("");
            inv.Upgrade("a", "a+");
            inv.Upgrade("a", "a+");

            CollectionAssert.AreEqual(new[] { "a" }, added);
            CollectionAssert.AreEqual(new[] { "a>a+" }, upgraded);
        }
    }
}
