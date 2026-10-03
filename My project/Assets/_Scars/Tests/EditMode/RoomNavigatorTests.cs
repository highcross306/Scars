using System.Collections.Generic;
using NUnit.Framework;
using Scars.Investigation;

namespace Scars.Tests
{
    public class RoomNavigatorTests
    {
        static RoomNavigator Start(int count, int start = 0)
        {
            var nav = new RoomNavigator();
            nav.Begin(count, start);
            return nav;
        }

        [Test]
        public void Begin_SetsCountAndStart()
        {
            var nav = Start(4, 2);

            Assert.AreEqual(4, nav.DirectionCount);
            Assert.AreEqual(2, nav.Current);
        }

        [TestCase(-1)]
        [TestCase(4)]
        public void Begin_InvalidStart_UsesZero(int start)
        {
            Assert.AreEqual(0, Start(4, start).Current);
        }

        [Test]
        public void TurnRight_WrapsToFirst()
        {
            var nav = Start(4, 2);
            nav.TurnRight();
            Assert.AreEqual(3, nav.Current);
            nav.TurnRight();
            Assert.AreEqual(0, nav.Current);
        }

        [Test]
        public void TurnLeft_WrapsToLast()
        {
            var nav = Start(4, 1);
            nav.TurnLeft();
            Assert.AreEqual(0, nav.Current);
            nav.TurnLeft();
            Assert.AreEqual(3, nav.Current);
        }

        [Test]
        public void GoTo_ValidMoves_InvalidIgnored()
        {
            var nav = Start(4);
            nav.GoTo(3);
            Assert.AreEqual(3, nav.Current);
            nav.GoTo(4);
            nav.GoTo(-1);
            Assert.AreEqual(3, nav.Current);
        }

        [Test]
        public void DirectionChanged_FiresOnlyOnRealChange()
        {
            var nav = Start(4);
            var seen = new List<int>();
            nav.DirectionChanged += d => seen.Add(d);

            nav.TurnRight();
            nav.TurnLeft();
            nav.GoTo(0);
            nav.GoTo(9);

            CollectionAssert.AreEqual(new[] { 1, 0 }, seen);
        }

        [Test]
        public void Begin_DoesNotFireEvent()
        {
            var nav = new RoomNavigator();
            int fired = 0;
            nav.DirectionChanged += _ => fired++;
            nav.Begin(4, 2);

            Assert.AreEqual(0, fired);
        }

        [Test]
        public void SingleDirection_TurningDoesNothing()
        {
            var nav = Start(1);
            int fired = 0;
            nav.DirectionChanged += _ => fired++;
            nav.TurnLeft();
            nav.TurnRight();

            Assert.AreEqual(0, nav.Current);
            Assert.AreEqual(0, fired);
        }

        [Test]
        public void NoDirections_IsSafe()
        {
            var nav = Start(0);
            nav.TurnLeft();
            nav.TurnRight();
            nav.GoTo(0);

            Assert.AreEqual(0, nav.DirectionCount);
            Assert.AreEqual(0, nav.Current);
            Assert.AreEqual(0, Start(-3).DirectionCount);
        }
    }
}
