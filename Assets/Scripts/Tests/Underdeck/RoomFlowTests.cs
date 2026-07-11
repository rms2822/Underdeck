using System.Collections.Generic;
using NUnit.Framework;
using static Underdeck.Core.Tests.TestCards;

namespace Underdeck.Core.Tests
{
    public class RoomFlowTests
    {
        private static RunState Fresh(ulong seed = 1) => GameEngine.NewRun(seed.ToString());

        [Test]
        public void NewRun_DealsA4CardRoom_FullHealth_TwoFocus()
        {
            var s = Fresh();
            Assert.AreEqual(RunPhase.Room, s.Phase);
            Assert.AreEqual(20, s.Hp);
            Assert.AreEqual(20, s.MaxHp);
            Assert.AreEqual(4, s.Room.Count);
            Assert.AreEqual(21, s.Deck.Count); // depth 1 has 25 cards total, minus the 4 dealt
            Assert.AreEqual(3, s.Skills.Hand.Count);
            Assert.AreEqual(2, s.Focus);
            Assert.AreEqual("dagger", s.Weapon.Id);
        }

        [Test]
        public void ResolvingDownToOne_WithDungeonRemaining_RefillsCarryingIt()
        {
            var s = Fresh();
            SetRoom(s, T(2), T(3), T(4), M(5));
            s.Deck = new List<Card> { M(6), M(7) };

            GameEngine.PocketTreasure(s, T(2));
            GameEngine.PocketTreasure(s, T(3));
            GameEngine.PocketTreasure(s, T(4));

            Assert.AreEqual(3, s.Room.Count, "carried card + both remaining dungeon cards");
            CollectionAssert.Contains(s.Room, M(5));
            CollectionAssert.Contains(s.Room, M(6));
            CollectionAssert.Contains(s.Room, M(7));
            Assert.AreEqual(0, s.Deck.Count);
            Assert.AreEqual(0, s.ActedThisRoom, "refilling starts a fresh room");
            Assert.AreEqual(2, s.Focus, "fresh room redraws focus");
        }

        [Test]
        public void ExhaustingTheDungeon_ThenTheRoom_StartsTheBoss()
        {
            var s = Fresh();
            SetRoom(s, M(5), M(6), M(7));
            s.Deck = new List<Card>(); // dungeon already spent

            GameEngine.FightMonster(s, M(5), useWeapon: false);
            Assert.AreEqual(2, s.Room.Count, "no refill possible — dungeon is empty");
            Assert.AreEqual(RunPhase.Room, s.Phase);

            GameEngine.FightMonster(s, M(6), useWeapon: false);
            Assert.AreEqual(1, s.Room.Count, "stays at 1 — nothing left to refill with");
            Assert.AreEqual(RunPhase.Room, s.Phase);

            GameEngine.FightMonster(s, M(7), useWeapon: false);
            Assert.AreEqual(0, s.Room.Count);
            Assert.AreEqual(RunPhase.BossIntro, s.Phase);
            Assert.AreEqual("The Gaoler", s.Boss.Name);
            Assert.AreEqual(12, s.Boss.Threat);
        }

        [Test]
        public void LethalDamage_EndsTheRunImmediately_EvenMidRoom()
        {
            var s = Fresh();
            s.Hp = 5;
            SetRoom(s, M(14));

            GameEngine.FightMonster(s, M(14), useWeapon: false);

            Assert.AreEqual(0, s.Hp);
            Assert.AreEqual(RunPhase.Over, s.Phase);
            Assert.IsFalse(s.Won);
        }

        [Test]
        public void Flee_CyclesTheRoomToTheBottom_AndLocksTheNextRoom()
        {
            var s = Fresh();
            int deckBefore = s.Deck.Count;
            var roomBefore = new List<Card>(s.Room);

            Assert.IsTrue(GameEngine.CanFleeNow(s));
            Assert.IsTrue(GameEngine.Flee(s));

            Assert.IsTrue(s.FledLast);
            Assert.AreEqual(deckBefore, s.Deck.Count, "fled cards go to the bottom, not away");
            Assert.AreEqual(4, s.Room.Count);
            foreach (var c in roomBefore) CollectionAssert.Contains(s.Deck, c);

            Assert.IsFalse(GameEngine.CanFleeNow(s), "no fleeing two rooms in a row");
            Assert.IsFalse(GameEngine.Flee(s));
        }

        [Test]
        public void CowardsBoots_AllowsFleeingTwiceInARow()
        {
            var s = Fresh();
            GameEngine.GainRelic(s, "boots");
            GameEngine.Flee(s);
            Assert.IsTrue(GameEngine.CanFleeNow(s));
        }

        [Test]
        public void CannotFlee_AfterActingInTheRoom()
        {
            var s = Fresh();
            var c = s.Room[0];
            if (c.Kind == CardKind.Monster) GameEngine.FightMonster(s, c, false);
            else if (c.Kind == CardKind.Potion) GameEngine.DrinkPotion(s, c);
            else if (c.Kind == CardKind.Treasure) GameEngine.PocketTreasure(s, c);
            else GameEngine.OpenCache(s, c);

            Assert.IsFalse(GameEngine.CanFleeNow(s));
        }

        [Test]
        public void Potion_HealsOnce_SecondInSameRoomIsWasted()
        {
            var s = Fresh();
            s.Hp = 5;
            SetRoom(s, P(6), P(9), T(2), T(3)); // a realistic 4-card room — avoids an auto-refill between drinks

            var first = GameEngine.DrinkPotion(s, P(6));
            Assert.AreEqual(6, first.Healed);
            Assert.AreEqual(11, s.Hp);

            var second = GameEngine.DrinkPotion(s, P(9));
            Assert.AreEqual(0, second.Healed, "second potion in a room heals nothing");
            Assert.AreEqual(11, s.Hp);
            CollectionAssert.Contains(s.Discard, P(9), "the wasted potion is still discarded");
        }

        [Test]
        public void SecondChalice_AllowsASecondHealInTheSameRoom()
        {
            var s = Fresh();
            s.Hp = 5;
            SetRoom(s, P(6), P(9));
            SetHand(s, "chalice", "strike", "guard");

            GameEngine.DrinkPotion(s, P(6));
            GameEngine.PlayInstant(s, 0); // chalice
            var second = GameEngine.DrinkPotion(s, P(9));

            Assert.AreEqual(9, second.Healed);
        }

        [Test]
        public void Treasure_BanksGold_DoubledByGlassSigil()
        {
            var s = Fresh();
            SetRoom(s, T(7));
            GameEngine.GainRelic(s, "glasssigil");

            int gained = GameEngine.PocketTreasure(s, T(7));

            Assert.AreEqual(14, gained);
            Assert.AreEqual(14, s.Gold);
        }

        [TestCase(CurseKind.Blight, 17)]
        public void Blight_DealsThreeDamage(CurseKind kind, int expectedHp)
        {
            var s = Fresh();
            SetRoom(s, Curse(kind));
            GameEngine.SufferCurse(s, Curse(kind));
            Assert.AreEqual(expectedHp, s.Hp);
        }

        [Test]
        public void ThiefsCurse_TakesFiveGold_NeverBelowZero()
        {
            var s = Fresh();
            s.Gold = 3;
            SetRoom(s, Curse(CurseKind.Thief));
            var r = GameEngine.SufferCurse(s, Curse(CurseKind.Thief));

            Assert.AreEqual(3, r.Amount, "only 3 could actually be taken");
            Assert.AreEqual(0, s.Gold);
        }

        [Test]
        public void Rustwind_DullsTheBladeToItsBasePower()
        {
            var s = Fresh(); // dagger power 4
            s.WeaponBonus = 3; // effective power 7, but rust ignores the bonus
            SetRoom(s, Curse(CurseKind.Rust));
            GameEngine.SufferCurse(s, Curse(CurseKind.Rust));

            Assert.AreEqual(4, s.WeaponLimit);
        }
    }
}
