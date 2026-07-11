using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using static Underdeck.Core.Tests.TestCards;

namespace Underdeck.Core.Tests
{
    public class SkillTests
    {
        private static RunState Fresh(ulong seed = 1) => GameEngine.NewRun(seed.ToString());

        [Test]
        public void Strike_AddsThreeToTheNextFight_ThenClears()
        {
            var s = Fresh();
            SetHand(s, "strike", "guard", "scout");
            SetRoom(s, M(8));

            var fx = GameEngine.PlayInstant(s, 0);
            Assert.AreEqual(2, s.Focus, "strike is free — focus untouched");
            Assert.AreEqual(FxTone.Steel, fx.Tone);

            var r = GameEngine.FightMonster(s, M(8), useWeapon: true); // dagger 4 + strike 3 = 7
            Assert.AreEqual(1, r.Dmg);
            Assert.AreEqual(0, s.StrikeNext, "spent after the fight");
        }

        [Test]
        public void Guard_BlocksUpToFourDamage_ThenIsConsumed()
        {
            var s = Fresh();
            SetHand(s, "guard", "strike", "scout");
            SetRoom(s, M(5));

            GameEngine.PlayInstant(s, 0);
            Assert.AreEqual(1, s.Focus);

            var r = GameEngine.FightMonster(s, M(5), useWeapon: false); // 5 - 4 blocked = 1
            Assert.AreEqual(1, r.Dmg);
            Assert.AreEqual(0, s.Block);
        }

        [Test]
        public void Grit_AddsFiveDefense()
        {
            var s = Fresh();
            SetHand(s, "grit", "strike", "guard");
            SetRoom(s, M(6));
            GameEngine.PlayInstant(s, 0);

            var pv = GameEngine.PreviewFight(s, M(6), useWeapon: false);
            Assert.AreEqual(1, pv.Dmg); // 6 - 5
        }

        [Test]
        public void Slip_ReopensFleeing_AfterAFledRoom()
        {
            var s = Fresh();
            GameEngine.Flee(s);
            Assert.IsFalse(GameEngine.CanFleeNow(s));

            SetHand(s, "slip", "strike", "guard");
            GameEngine.PlayInstant(s, 0);

            Assert.IsTrue(GameEngine.CanFleeNow(s));
        }

        [Test]
        public void Mend_HealsThree_CappedAtMax()
        {
            var s = Fresh();
            s.Hp = 19;
            SetHand(s, "mend", "strike", "guard");
            var fx = GameEngine.PlayInstant(s, 0);
            Assert.AreEqual(20, s.Hp);
            Assert.AreEqual("+1", fx.Label);
        }

        [Test]
        public void Pickpocket_GrantsFourGold()
        {
            var s = Fresh();
            SetHand(s, "pickpocket", "strike", "guard");
            GameEngine.PlayInstant(s, 0);
            Assert.AreEqual(4, s.Gold);
        }

        [Test]
        public void Preserve_PreventsTheNextWeaponKillFromDulling()
        {
            var s = Fresh();
            SetHand(s, "preserve", "strike", "guard");
            SetRoom(s, M(9));
            GameEngine.PlayInstant(s, 0);

            GameEngine.FightMonster(s, M(9), useWeapon: true);
            Assert.IsNull(s.WeaponLimit, "preserved kill leaves the blade keen");
        }

        [Test]
        public void Bash_SlaysAWeakMonster_ForFree_UpToThresholdSix()
        {
            var s = Fresh();
            SetHand(s, "bash", "strike", "guard");
            SetRoom(s, M(6), M(9));

            GameEngine.BeginTargetMode(s, 0);
            Assert.AreEqual(InputMode.Bash, s.Mode);
            Assert.IsFalse(GameEngine.ResolveBashRevenant(s, M(9)), "9 exceeds Bash's threshold");
            Assert.AreEqual(InputMode.Bash, s.Mode, "invalid target keeps the mode armed");

            Assert.IsTrue(GameEngine.ResolveBashRevenant(s, M(6)));
            Assert.AreEqual(InputMode.None, s.Mode);
            CollectionAssert.DoesNotContain(s.Room, M(6));
            Assert.AreEqual(20, s.Hp, "Bash takes no damage");
        }

        [Test]
        public void KnifeThrow_IgnoresDulling_ButLosesTheWeapon()
        {
            var s = Fresh();
            s.Deck.Clear(); // keep the room from auto-refilling mid-test
            SetRoom(s, M(6), M(12));
            GameEngine.FightMonster(s, M(6), useWeapon: true); // dulls to ≤6
            SetHand(s, "knifethrow", "strike", "guard");

            GameEngine.BeginTargetMode(s, 0);
            var r = GameEngine.ResolveKnifeThrow(s, M(12));

            Assert.AreEqual(12 - 4, r.Dmg); // dagger power 4, dulling ignored
            Assert.IsNull(s.Weapon, "the weapon is lost");
        }

        [Test]
        public void Snipe_ResolvesOneCard_AndBanishesTheRestToTheBottom()
        {
            var s = Fresh();
            SetRoom(s, M(3), T(4), P(5), M(6));
            s.Deck = new List<Card> { M(7) };
            SetHand(s, "snipe", "strike", "guard");

            GameEngine.BeginTargetMode(s, 0);
            GameEngine.ArmSnipe(s, T(4));
            GameEngine.PocketTreasure(s, T(4)); // the normal action commits the snipe

            Assert.AreEqual(4, s.Room.Count, "fresh room dealt from the banished + remaining dungeon");
            Assert.AreEqual(InputMode.None, s.Mode);
            Assert.IsFalse(s.SnipePending);
            CollectionAssert.Contains(s.Discard, T(4));
        }

        [Test]
        public void Snipe_CancellingBeforeChoosingATarget_FullyClearsTheMode()
        {
            var s = Fresh();
            SetRoom(s, M(3), T(4), P(5), M(6));
            SetHand(s, "snipe", "strike", "guard");

            GameEngine.BeginTargetMode(s, 0);
            Assert.AreEqual(InputMode.Snipe, s.Mode);

            GameEngine.CancelMode(s);

            Assert.AreEqual(InputMode.None, s.Mode);
            Assert.IsFalse(s.SnipePending, "no target was ever armed");
            Assert.AreEqual(4, s.Room.Count, "nothing was resolved");
        }

        [Test]
        public void Snipe_ArmingATarget_ThenNotResolvingIt_LeavesTheRoomUntouched()
        {
            // Mirrors the source engine: once a target sheet is open (ArmSnipe fired),
            // declining to commit any action (fight/drink/pocket/etc.) simply leaves
            // SnipePending armed for the next tap — Presentation never calls a Core
            // "cancel the sheet" method for this because there's nothing to undo yet.
            var s = Fresh();
            SetRoom(s, M(3), T(4), P(5), M(6));
            SetHand(s, "snipe", "strike", "guard");

            GameEngine.BeginTargetMode(s, 0);
            GameEngine.ArmSnipe(s, T(4));

            Assert.IsTrue(s.SnipePending);
            Assert.AreEqual(4, s.Room.Count, "declining to act resolves nothing");
        }

        [Test]
        public void SmokeBomb_SwapsTwoRoomCards_ForTheTopTwoOfTheDungeon()
        {
            var s = Fresh();
            SetRoom(s, M(3), T(4), P(5), M(6));
            s.Deck = new List<Card> { T(9), P(10), M(11) };
            SetHand(s, "smokebomb", "strike", "guard");

            GameEngine.BeginTargetMode(s, 0);
            Assert.AreEqual(2, s.SmokeDrawn.Count);
            Assert.AreEqual(1, s.Deck.Count, "two cards drawn out of the dungeon");

            Assert.IsFalse(GameEngine.ToggleSmokePick(s, M(3)));
            Assert.IsTrue(GameEngine.ToggleSmokePick(s, T(4)));

            Assert.AreEqual(InputMode.None, s.Mode);
            CollectionAssert.Contains(s.Room, T(9));
            CollectionAssert.Contains(s.Room, P(10));
            CollectionAssert.DoesNotContain(s.Room, M(3));
            CollectionAssert.DoesNotContain(s.Room, T(4));
            CollectionAssert.Contains(s.Deck, M(3));
            CollectionAssert.Contains(s.Deck, T(4));
        }

        [Test]
        public void CancellingSmokeBomb_ReturnsTheDrawnCards_InsteadOfLosingThem()
        {
            var s = Fresh();
            SetRoom(s, M(3), T(4), P(5), M(6));
            s.Deck = new List<Card> { T(9), P(10) };
            SetHand(s, "smokebomb", "strike", "guard");

            GameEngine.BeginTargetMode(s, 0);
            int total = s.Deck.Count + s.Room.Count + s.SmokeDrawn.Count;

            GameEngine.CancelMode(s);

            Assert.AreEqual(InputMode.None, s.Mode);
            Assert.AreEqual(0, s.SmokeDrawn.Count);
            Assert.AreEqual(total, s.Deck.Count + s.Room.Count, "no cards vanish on cancel");
        }

        [Test]
        public void Scout_PeeksTheTopTwo_AndCanSwapTheirOrder()
        {
            var s = Fresh();
            s.Deck = new List<Card> { M(3), M(4), M(5) };
            SetHand(s, "scout", "strike", "guard");

            GameEngine.PlayScout(s, 0);
            var top = GameEngine.PeekDeckTop(s);
            CollectionAssert.AreEqual(new[] { M(3), M(4) }, top);

            GameEngine.SwapDeckTopTwo(s);
            Assert.AreEqual(M(4), s.Deck[0]);
            Assert.AreEqual(M(3), s.Deck[1]);
        }

        [Test]
        public void Necromancy_OffersSixShuffledDiscards_ChoosingARedCardAppliesItsEffect()
        {
            var s = Fresh();
            s.Hp = 10;
            s.Discard = new List<Card> { M(2), P(7), T(5), M(3), M(4), P(9), T(6) };
            SetHand(s, "necromancy", "necromancy", "necromancy"); // cost 2, plenty of focus at 2

            var six = GameEngine.PlayNecromancy(s, 0);
            Assert.AreEqual(6, six.Count);
            Assert.AreEqual(0, s.Focus);

            var potion = six.First(c => c.Kind == CardKind.Potion);
            int healed = GameEngine.NecromancyChoosePotion(s, potion);
            Assert.AreEqual(System.Math.Min(20 - 10, potion.Rank), healed);

            // the discard pile itself is untouched — necromancy only "consults" it
            Assert.AreEqual(7, s.Discard.Count);
        }

        [Test]
        public void SkillUsability_GatesOnFocusAndContext()
        {
            var s = Fresh();
            s.Deck.Clear(); // starve Smoke Bomb of cards to draw
            SetHand(s, "smokebomb", "necromancy", "sharpen");

            var (bombOk, bombWhy) = GameEngine.SkillUsability(s, "smokebomb");
            Assert.IsFalse(bombOk);
            Assert.AreEqual("needs 2 cards in deck and room", bombWhy);

            var (necroOk, necroWhy) = GameEngine.SkillUsability(s, "necromancy");
            Assert.IsFalse(necroOk);
            Assert.AreEqual("the discard is empty", necroWhy);

            var (sharpOk, sharpWhy) = GameEngine.SkillUsability(s, "sharpen");
            Assert.IsFalse(sharpOk);
            Assert.AreEqual("your blade is already keen", sharpWhy);
        }

        [Test]
        public void Gravebind_ReturnsARevenantSkill_EveryFifthKill()
        {
            var s = Fresh();
            GameEngine.GainRelic(s, "gravebind");
            for (int i = 0; i < 5; i++) GameEngine.OnSlain(s, M(2), false);

            CollectionAssert.Contains(s.Skills.Disc, "revenant");
        }

        [Test]
        public void Bloodpact_TradesTwoHealthForOneFocus()
        {
            var s = Fresh();
            GameEngine.GainRelic(s, "bloodpact");
            s.Hp = 10;

            Assert.IsTrue(GameEngine.UseBloodpact(s));
            Assert.AreEqual(8, s.Hp);
            Assert.AreEqual(3, s.Focus);

            s.Hp = 2;
            Assert.IsFalse(GameEngine.UseBloodpact(s), "refuses to drop below survival range");
        }
    }
}
