using NUnit.Framework;
using static Scroundel.Core.Tests.TestCards;

namespace Scroundel.Core.Tests
{
    public class CombatTests
    {
        [Test]
        public void Barehanded_TakesFullDamage()
        {
            var s = Rules.NewRun(new[] { C(10), W(2), W(3), H(2) });

            int damage = Rules.Fight(s, C(10), useWeapon: false);

            Assert.AreEqual(10, damage);
            Assert.AreEqual(10, s.Health);
            Assert.IsFalse(s.WeaponLimit.HasValue, "barehanded must not degrade the weapon");
        }

        [Test]
        public void Weapon_ReducesDamage_AndDegrades()
        {
            var s = Rules.NewRun(new[] { W(7), C(10), C(2), C(3) });
            Rules.Equip(s, W(7));

            int damage = Rules.Fight(s, C(10), useWeapon: true);

            Assert.AreEqual(3, damage);
            Assert.AreEqual(17, s.Health);
            Assert.AreEqual(10, s.WeaponLimit, "weapon locks to the value it just slew");
        }

        [Test]
        public void Weapon_Overkill_DealsZero_NeverHeals()
        {
            var s = Rules.NewRun(new[] { W(7), C(3), C(2), C(4) });
            Rules.Equip(s, W(7));

            Assert.AreEqual(0, Rules.Fight(s, C(3), useWeapon: true));
            Assert.AreEqual(20, s.Health);
        }

        [Test]
        public void Degradation_BlocksBiggerEnemies_BarehandedStillAllowed()
        {
            var s = Rules.NewRun(new[] { W(7), C(5), C(6), C(2) });
            Rules.Equip(s, W(7));
            Rules.Fight(s, C(5), useWeapon: true); // limit = 5

            Assert.IsFalse(Rules.CanUseWeaponOn(s, C(6)));
            Assert.Throws<InvalidMoveException>(() => Rules.Fight(s, C(6), useWeapon: true));

            Assert.AreEqual(6, Rules.Fight(s, C(6), useWeapon: false)); // barehanded works
            Assert.AreEqual(14, s.Health);
        }

        [Test]
        public void Degradation_AllowsEqualValue_ByDefault()
        {
            var s = Rules.NewRun(new[] { W(7), C(5), S(5), C(6) });
            Rules.Equip(s, W(7));
            Rules.Fight(s, C(5), useWeapon: true);

            Assert.IsTrue(Rules.CanUseWeaponOn(s, S(5)));
            Assert.AreEqual(0, Rules.Fight(s, S(5), useWeapon: true));
        }

        [Test]
        public void Degradation_StrictConfig_BlocksEqualValue()
        {
            var config = new RulesConfig { WeaponCanStrikeEqual = false };
            var s = Rules.NewRun(new[] { W(7), C(5), S(5), C(6) }, config);
            Rules.Equip(s, W(7));
            Rules.Fight(s, C(5), useWeapon: true);

            Assert.IsFalse(Rules.CanUseWeaponOn(s, S(5)), "equal value blocked under strict config");
            Assert.IsTrue(Rules.CanUseWeaponOn(s, C(4)), "strictly lower value still allowed");
        }

        [Test]
        public void EquippingNewWeapon_ResetsDegradation()
        {
            var s = Rules.NewRun(new[] { W(3), C(12), W(7), C(13) });
            Rules.Equip(s, W(3));
            Rules.Fight(s, C(12), useWeapon: true); // 9 dmg, hp 11, limit 12
            Rules.Equip(s, W(7));                   // fresh weapon

            Assert.IsFalse(s.WeaponLimit.HasValue);
            Assert.AreEqual(6, Rules.Fight(s, C(13), useWeapon: true)); // 13 > old limit 12: only legal because reset
            Assert.AreEqual(5, s.Health);
        }

        [Test]
        public void FightWithWeapon_NoneEquipped_Throws()
        {
            var s = Rules.NewRun(new[] { C(5), C(2), C(3), C(4) });
            Assert.IsFalse(Rules.CanUseWeaponOn(s, C(5)));
            Assert.Throws<InvalidMoveException>(() => Rules.Fight(s, C(5), useWeapon: true));
        }

        [Test]
        public void LethalDamage_LosesRun_HealthClampsAtZero()
        {
            var s = Rules.NewRun(new[] { C(14), S(10), C(2), S(2) });
            Rules.Fight(s, C(14), useWeapon: false); // hp 6
            Rules.Fight(s, S(10), useWeapon: false); // 10 > 6

            Assert.AreEqual(0, s.Health);
            Assert.AreEqual(GameStatus.Lost, s.Status);
        }

        [Test]
        public void Preview_MatchesActualDamage()
        {
            var s = Rules.NewRun(new[] { W(7), C(10), C(2), C(3) });
            Rules.Equip(s, W(7));

            int previewWeapon = Rules.PreviewDamage(s, C(10), useWeapon: true);
            int previewBare = Rules.PreviewDamage(s, C(10), useWeapon: false);

            Assert.AreEqual(10, previewBare);
            Assert.AreEqual(previewWeapon, Rules.Fight(s, C(10), useWeapon: true));
        }
    }
}
