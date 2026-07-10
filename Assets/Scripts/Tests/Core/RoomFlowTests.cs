using NUnit.Framework;
using static Scroundel.Core.Tests.TestCards;

namespace Scroundel.Core.Tests
{
    public class RoomFlowTests
    {
        [Test]
        public void NewRun_Deals4Cards_FullHealth()
        {
            var s = Rules.NewRun(12345UL);

            Assert.AreEqual(GameStatus.InProgress, s.Status);
            Assert.AreEqual(20, s.Health);
            Assert.AreEqual(4, s.Room.Count);
            Assert.AreEqual(40, s.DungeonCount);
            Assert.IsTrue(Rules.CanFlee(s));
            Assert.IsFalse(s.EquippedWeapon.HasValue);
        }

        [Test]
        public void Resolving3Cards_RefillsRoom_CarryingTheFourth()
        {
            // Room: W2 W3 H2 C5 | Dungeon: C2 C3 C4 S2
            var s = Rules.NewRun(new[] { W(2), W(3), H(2), C(5), C(2), C(3), C(4), S(2) });

            Rules.Equip(s, W(2));
            Rules.Equip(s, W(3));
            Rules.Drink(s, H(2));

            Assert.AreEqual(4, s.Room.Count, "room should refill after 3 resolutions");
            CollectionAssert.Contains(s.Room, C(5)); // the carried card
            CollectionAssert.Contains(s.Room, C(2));
            Assert.AreEqual(1, s.DungeonCount);
            Assert.AreEqual(0, s.ResolvedThisRoom, "new room started");
            Assert.IsFalse(s.ElixirDrunkThisRoom, "elixir rule resets per room");
        }

        [Test]
        public void ResolvingWholeDeck_Wins()
        {
            var s = Rules.NewRun(new[] { C(2), S(2), C(3), S(3), C(4) });

            Rules.Fight(s, C(2), useWeapon: false); // 18
            Rules.Fight(s, S(2), useWeapon: false); // 16
            Rules.Fight(s, C(3), useWeapon: false); // 13 → refill: room = S3, C4
            Assert.AreEqual(2, s.Room.Count);
            Rules.Fight(s, S(3), useWeapon: false); // 10
            Rules.Fight(s, C(4), useWeapon: false); // 6

            Assert.AreEqual(GameStatus.Won, s.Status);
            Assert.AreEqual(6, s.Health);
        }

        [Test]
        public void Actions_AfterGameOver_Throw()
        {
            var s = Rules.NewRun(new[] { C(14), S(10), C(2), S(2) });
            Rules.Fight(s, C(14), useWeapon: false); // hp 6
            Rules.Fight(s, S(10), useWeapon: false); // hp 0 → Lost

            Assert.AreEqual(GameStatus.Lost, s.Status);
            Assert.Throws<InvalidMoveException>(() => Rules.Fight(s, C(2), false));
            Assert.Throws<InvalidMoveException>(() => Rules.Flee(s));
        }

        [Test]
        public void ActingOnCardNotInRoom_Throws()
        {
            var s = Rules.NewRun(new[] { C(2), C(3), C(4), C(5), C(6) });
            Assert.Throws<InvalidMoveException>(() => Rules.Fight(s, C(6), false));
        }

        [Test]
        public void ActingWithWrongKind_Throws()
        {
            var s = Rules.NewRun(new[] { C(2), W(5), H(4), C(3) });
            Assert.Throws<InvalidMoveException>(() => Rules.Drink(s, W(5)));
            Assert.Throws<InvalidMoveException>(() => Rules.Equip(s, H(4)));
            Assert.Throws<InvalidMoveException>(() => Rules.Fight(s, H(4), false));
        }

        [Test]
        public void DuplicateCardsInDungeon_Throw()
        {
            Assert.Throws<System.ArgumentException>(
                () => Rules.NewRun(new[] { C(5), C(5), C(6), C(7) }));
        }
    }
}
