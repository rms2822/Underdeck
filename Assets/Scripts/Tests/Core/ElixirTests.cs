using NUnit.Framework;
using static Scroundel.Core.Tests.TestCards;

namespace Scroundel.Core.Tests
{
    public class ElixirTests
    {
        [Test]
        public void Elixir_HealsItsValue()
        {
            var s = Rules.NewRun(new[] { C(10), H(7), W(2), W(3) });
            Rules.Fight(s, C(10), useWeapon: false); // hp 10

            Assert.AreEqual(7, Rules.Drink(s, H(7)));
            Assert.AreEqual(17, s.Health);
        }

        [Test]
        public void Healing_CapsAtMaxHealth()
        {
            var s = Rules.NewRun(new[] { C(2), H(9), W(2), W(3) });
            Rules.Fight(s, C(2), useWeapon: false); // hp 18

            Assert.AreEqual(2, Rules.Drink(s, H(9)));
            Assert.AreEqual(20, s.Health);
        }

        [Test]
        public void SecondElixir_SameRoom_IsWasted()
        {
            var s = Rules.NewRun(new[] { C(10), H(5), H(9), W(2) });
            Rules.Fight(s, C(10), useWeapon: false); // hp 10
            Rules.Drink(s, H(5));                    // hp 15

            Assert.AreEqual(0, Rules.Drink(s, H(9)), "second elixir in a room heals nothing");
            Assert.AreEqual(15, s.Health);
            CollectionAssert.Contains(s.Discard, H(9), "wasted elixir is still discarded");
        }

        [Test]
        public void NewRoom_ResetsTheOneElixirRule()
        {
            var s = Rules.NewRun(new[] { C(10), H(5), W(2), C(2), H(9), W(3), W(4), H(2) });
            Rules.Fight(s, C(10), useWeapon: false); // hp 10
            Rules.Drink(s, H(5));                    // hp 15
            Rules.Equip(s, W(2));                    // 3rd resolution → new room

            Assert.AreEqual(5, Rules.Drink(s, H(9)), "fresh room, elixir heals again");
            Assert.AreEqual(20, s.Health);
        }

        [Test]
        public void DrinkingAtFullHealth_IsWasted_ButCountsAsTheRoomsElixir()
        {
            var s = Rules.NewRun(new[] { H(2), C(4), H(9), W(2) });

            Assert.AreEqual(0, Rules.Drink(s, H(2)));   // full hp: heals 0
            Rules.Fight(s, C(4), useWeapon: false);     // hp 16
            Assert.AreEqual(0, Rules.Drink(s, H(9)), "room's one elixir already used");
            Assert.AreEqual(16, s.Health);
        }
    }
}
