using NUnit.Framework;
using static Underdeck.Core.Tests.TestCards;

namespace Underdeck.Core.Tests
{
    public class CombatTests
    {
        private static RunState Fresh(ulong seed = 1) => GameEngine.NewRun(seed.ToString());

        [Test]
        public void BareHanded_TakesFullThreat()
        {
            var s = Fresh();
            var wolf = M(8);
            SetRoom(s, wolf);

            var r = GameEngine.FightMonster(s, wolf, useWeapon: false);

            Assert.AreEqual(8, r.Dmg);
            Assert.AreEqual(20 - 8, s.Hp);
            Assert.IsNull(s.WeaponLimit, "bare-handed must never dull the weapon");
        }

        [Test]
        public void Weapon_ReducesDamage_AndDulls_ToEqualOrLower()
        {
            var s = Fresh(); // dagger power 4
            var wolf = M(8);
            SetRoom(s, wolf);

            var r = GameEngine.FightMonster(s, wolf, useWeapon: true);

            Assert.AreEqual(4, r.Dmg); // 8 - 4
            Assert.AreEqual(8, s.WeaponLimit);
            Assert.IsTrue(GameEngine.CanUseWeapon(s, M(8)), "equal rank still cuttable (≤ rule)");
            Assert.IsTrue(GameEngine.CanUseWeapon(s, M(6)), "lower rank still cuttable");
            Assert.IsFalse(GameEngine.CanUseWeapon(s, M(9)), "higher rank blocked");
        }

        [Test]
        public void Sharpen_UndoesAllDulling()
        {
            var s = Fresh();
            SetRoom(s, M(8));
            GameEngine.FightMonster(s, M(8), useWeapon: true);
            Assert.AreEqual(8, s.WeaponLimit);

            SetHand(s, "sharpen", "guard", "scout");
            var fx = GameEngine.PlayInstant(s, 0);

            Assert.IsNull(s.WeaponLimit);
            Assert.AreEqual(FxTone.Steel, fx.Tone);
            Assert.IsTrue(GameEngine.CanUseWeapon(s, M(14)));
        }

        [Test]
        public void EliteArmored_ReducesWeaponPower()
        {
            var s = Fresh();
            var brute = M(10, EliteKind.Armored);
            SetRoom(s, brute);

            var pv = GameEngine.PreviewFight(s, brute, useWeapon: true);
            Assert.AreEqual(10 - (4 - 2), pv.Dmg); // dagger 4, -2 for armored
        }

        [Test]
        public void EliteBrutal_AddsTwoThreat()
        {
            Assert.AreEqual(10 + 2, GameEngine.ThreatOf(M(10, EliteKind.Brutal)));
        }

        [Test]
        public void RapierWeapon_DoesNotDull_WhenNoDamageTaken()
        {
            var s = Fresh();
            s.Weapon = Content.Weapons["rapier"]; // power 5
            var small = M(3);
            SetRoom(s, small);

            GameEngine.FightMonster(s, small, useWeapon: true);

            Assert.AreEqual(0, GameEngine.PreviewFight(s, M(3), true).Dmg);
            Assert.IsNull(s.WeaponLimit, "rapier stays keen on a no-damage kill");
        }

        [Test]
        public void KrisWeapon_HealsOnWeaponKill()
        {
            var s = Fresh();
            s.Weapon = Content.Weapons["kris"];
            s.Hp = 10;
            var m = M(2);
            SetRoom(s, m);

            GameEngine.FightMonster(s, m, useWeapon: true);

            Assert.AreEqual(11, s.Hp, "Kris heals 1 on a weapon kill");
        }

        [Test]
        public void WhisperBlade_IgnoresDulling_AtThreatFourOrBelow()
        {
            var s = Fresh();
            s.Weapon = Content.Weapons["whisper"]; // power 6
            SetRoom(s, M(4));
            GameEngine.FightMonster(s, M(4), useWeapon: true);

            Assert.IsNull(s.WeaponLimit, "whisper blade never dulls against threat ≤ 4");
            Assert.IsTrue(GameEngine.CanUseWeapon(s, M(14)));
        }

        [Test]
        public void Hammer_DullsTwoStepsAtATime()
        {
            var s = Fresh();
            s.Weapon = Content.Weapons["hammer"]; // power 9
            SetRoom(s, M(10));
            GameEngine.FightMonster(s, M(10), useWeapon: true);

            Assert.AreEqual(8, s.WeaponLimit); // 10 - 2
        }

        [Test]
        public void WeaponBonus_ScalesEffectivePower()
        {
            var s = Fresh();
            s.WeaponBonus = 2;
            Assert.AreEqual(6, GameEngine.EffPower(s)); // dagger 4 + 2
        }

        [Test]
        public void Whetstone_FloorsDullingAtFour()
        {
            var s = Fresh();
            GameEngine.GainRelic(s, "whetstone");
            s.WeaponLimit = 2;
            Assert.AreEqual(4, GameEngine.EffLimit(s));
        }

        [Test]
        public void Phoenix_SurvivesLethalHit_AtOneHealth_Once()
        {
            var s = Fresh();
            GameEngine.GainRelic(s, "phoenix");
            s.Hp = 5;
            var outcome = GameEngine.LoseHp(s, 20);

            Assert.AreEqual(HpOutcome.Phoenix, outcome);
            Assert.AreEqual(1, s.Hp);
            Assert.IsTrue(s.PhoenixUsed);

            // second lethal hit: no more feathers
            var outcome2 = GameEngine.LoseHp(s, 5);
            Assert.AreEqual(HpOutcome.Dead, outcome2);
        }

        [Test]
        public void FightingWithNoWeaponEquipped_TreatsAsUnarmed()
        {
            var s = Fresh();
            s.Weapon = null;
            Assert.IsFalse(GameEngine.CanUseWeapon(s, M(5)));
        }
    }
}
