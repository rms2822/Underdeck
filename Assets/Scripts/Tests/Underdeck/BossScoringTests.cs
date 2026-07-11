using NUnit.Framework;
using static Underdeck.Core.Tests.TestCards;

namespace Underdeck.Core.Tests
{
    public class BossScoringTests
    {
        private static RunState Fresh(ulong seed = 1) => GameEngine.NewRun(seed.ToString());

        private static RunState AtBossIntro(ulong seed = 1)
        {
            var s = Fresh(seed);
            s.Deck.Clear();
            s.Room.Clear();
            GameEngine.StartBoss(s);
            return s;
        }

        [Test]
        public void StartBoss_PicksTheBossForTheCurrentDepth()
        {
            var s = AtBossIntro();
            Assert.AreEqual(RunPhase.BossIntro, s.Phase);
            Assert.AreEqual("The Gaoler", s.Boss.Name);
            Assert.AreEqual(BossSpecial.Lock, s.Boss.Special);
            Assert.AreEqual(1, s.Boss.HitsLeft);
        }

        [Test]
        public void RotKing_HasTwoLives()
        {
            var s = Fresh();
            s.Depth = 2;
            s.Deck.Clear(); s.Room.Clear();
            GameEngine.StartBoss(s);
            Assert.AreEqual("The Rot King", s.Boss.Name);
            Assert.AreEqual(2, s.Boss.HitsLeft);
        }

        [Test]
        public void EnterBossFight_DrawsAHand_AndGaolerLocksOneSkill()
        {
            var s = AtBossIntro();
            GameEngine.EnterBossFight(s);

            Assert.AreEqual(RunPhase.Boss, s.Phase);
            Assert.AreEqual(3, s.Skills.Hand.Count);
            Assert.GreaterOrEqual(s.LockedIdx, 0);
            Assert.Less(s.LockedIdx, 3);
        }

        [Test]
        public void FightBoss_MultipleHits_UntilHitsLeftReachesZero()
        {
            var s = AtBossIntro(); // Gaoler, threat 12, 1 life
            GameEngine.EnterBossFight(s);

            var r = GameEngine.FightBoss(s, useWeapon: true); // dagger 4: take 8
            Assert.AreEqual(BossOutcome.Slain, r.BossOutcome);
            Assert.AreEqual(1, s.BossesSlain);
            Assert.AreEqual(8, s.Gold);
        }

        [Test]
        public void RotKing_RequiresTwoSeparateHits()
        {
            var s = Fresh();
            s.Depth = 2;
            s.Hp = 30; s.MaxHp = 30; // headroom for two 13-damage bare-handed hits
            s.Deck.Clear(); s.Room.Clear();
            GameEngine.StartBoss(s);
            GameEngine.EnterBossFight(s);

            var r1 = GameEngine.FightBoss(s, useWeapon: false);
            Assert.AreEqual(BossOutcome.Again, r1.BossOutcome);
            Assert.AreEqual(0, s.BossesSlain);

            var r2 = GameEngine.FightBoss(s, useWeapon: false);
            Assert.AreEqual(BossOutcome.Slain, r2.BossOutcome);
            Assert.AreEqual(1, s.BossesSlain);
        }

        [Test]
        public void DeepTyrant_PiercesThreePowerOfTheWeapon()
        {
            var s = Fresh();
            s.Depth = 3;
            s.Deck.Clear(); s.Room.Clear();
            GameEngine.StartBoss(s);
            GameEngine.EnterBossFight(s);

            var pv = GameEngine.PreviewFight(s, Underdeck.Core.Card.Monster('♠', s.Boss.Threat), useWeapon: true);
            Assert.AreEqual(15 - (4 - 3), pv.Dmg); // dagger 4, pierced to 1 effective power
        }

        [Test]
        public void FightBoss_LethalDamage_ReportsDeadWithoutCountingAsSlain()
        {
            var s = AtBossIntro();
            s.Hp = 3;
            GameEngine.EnterBossFight(s);

            var r = GameEngine.FightBoss(s, useWeapon: false); // 12 damage, way over 3 hp
            Assert.AreEqual(BossOutcome.Dead, r.BossOutcome);
            Assert.AreEqual(HpOutcome.Dead, r.HpOutcome);
            Assert.AreEqual(0, s.BossesSlain);
        }

        [Test]
        public void FightBoss_Phoenix_SurvivesAndStillProgressesTheFight()
        {
            var s = AtBossIntro();
            GameEngine.GainRelic(s, "phoenix");
            s.Hp = 2;
            GameEngine.EnterBossFight(s);

            var r = GameEngine.FightBoss(s, useWeapon: false); // lethal, but Phoenix saves at 1 hp
            Assert.AreEqual(HpOutcome.Phoenix, r.HpOutcome);
            Assert.AreEqual(1, s.Hp);
            Assert.AreEqual(BossOutcome.Slain, r.BossOutcome, "the fight still resolves normally once revived");
        }

        [Test]
        public void TryCompleteRun_OnlyEndsAtDepthThree()
        {
            var s = Fresh();
            s.Depth = 2;
            Assert.IsFalse(GameEngine.TryCompleteRun(s));
            Assert.AreEqual(RunPhase.Room, s.Phase);

            s.Depth = 3;
            Assert.IsTrue(GameEngine.TryCompleteRun(s));
            Assert.AreEqual(RunPhase.Over, s.Phase);
            Assert.IsTrue(s.Won);
        }

        [Test]
        public void AdvanceToNextDepth_IncrementsWeaponBonus_AndDealsANewDungeon()
        {
            var s = Fresh();
            GameEngine.AdvanceToNextDepth(s);

            Assert.AreEqual(2, s.Depth);
            Assert.AreEqual(1, s.WeaponBonus);
            Assert.AreEqual(5, GameEngine.EffPower(s)); // dagger 4 + 1
            Assert.AreEqual(RunPhase.Room, s.Phase);
            Assert.AreEqual(4, s.Room.Count);
        }

        [Test]
        public void ScoreParts_MatchesTheStatedFormula()
        {
            var s = Fresh();
            s.Hp = 13; s.Gold = 27; s.Depth = 2; s.BossesSlain = 1;

            var sc = GameEngine.ScoreParts(s);
            Assert.AreEqual(130, sc.Hp);
            Assert.AreEqual(27, sc.Gold);
            Assert.AreEqual(200, sc.Depth);
            Assert.AreEqual(50, sc.Boss);
            Assert.AreEqual(130 + 27 + 200 + 50, sc.Total);
        }

        [Test]
        public void EndRun_IsIdempotent()
        {
            var s = Fresh();
            GameEngine.EndRun(s, true);
            Assert.IsTrue(s.Won);

            GameEngine.EndRun(s, false); // a second, contradictory call must be ignored
            Assert.IsTrue(s.Won, "the first outcome sticks");
        }
    }
}
