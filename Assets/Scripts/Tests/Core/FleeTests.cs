using NUnit.Framework;
using static Scroundel.Core.Tests.TestCards;

namespace Scroundel.Core.Tests
{
    public class FleeTests
    {
        private static GameState EightCardRun() =>
            // Room: C2 C3 C4 C5 | Dungeon: W2 W3 H2 H3
            Rules.NewRun(new[] { C(2), C(3), C(4), C(5), W(2), W(3), H(2), H(3) });

        [Test]
        public void Flee_SendsRoomToBottom_DealsFreshRoom()
        {
            var s = EightCardRun();

            Rules.Flee(s);

            CollectionAssert.AreEquivalent(
                new[] { W(2), W(3), H(2), H(3) }, s.Room, "next 4 cards become the room");
            Assert.AreEqual(4, s.DungeonCount, "fled cards went to the bottom, not away");
            Assert.AreEqual(20, s.Health, "fleeing costs nothing");
        }

        [Test]
        public void CannotFlee_TwiceInARow()
        {
            var s = EightCardRun();
            Rules.Flee(s);

            Assert.IsFalse(Rules.CanFlee(s));
            Assert.Throws<InvalidMoveException>(() => Rules.Flee(s));
        }

        [Test]
        public void CanFleeAgain_AfterPlayingARoom_AndFledCardsReturn()
        {
            var s = EightCardRun();
            Rules.Flee(s); // room: W2 W3 H2 H3; bottom: C2 C3 C4 C5

            Rules.Equip(s, W(2));
            Rules.Equip(s, W(3));
            Rules.Drink(s, H(2)); // 3rd resolution → new room deals

            Assert.IsTrue(Rules.CanFlee(s), "flee unblocks after playing a room through");
            CollectionAssert.Contains(s.Room, H(3), "carried card");
            CollectionAssert.Contains(s.Room, C(2), "fled cards cycle back from the bottom");
            CollectionAssert.Contains(s.Room, C(3));
            CollectionAssert.Contains(s.Room, C(4));
        }

        [Test]
        public void CannotFlee_AfterActingInTheRoom()
        {
            var s = EightCardRun();
            Rules.Fight(s, C(2), useWeapon: false);

            Assert.IsFalse(Rules.CanFlee(s));
            Assert.Throws<InvalidMoveException>(() => Rules.Flee(s));
        }
    }
}
