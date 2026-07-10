using NUnit.Framework;
using static Scroundel.Core.Tests.TestCards;

namespace Scroundel.Core.Tests
{
    public class ScoringTests
    {
        [Test]
        public void Win_ScoreIsRemainingHealth()
        {
            var s = Rules.NewRun(new[] { C(5), W(9), C(3), H(2) });
            Rules.Fight(s, C(5), useWeapon: false); // hp 15
            Rules.Equip(s, W(9));
            Rules.Fight(s, C(3), useWeapon: true);  // 0 dmg
            Rules.Drink(s, H(2));                   // hp 17 → Won

            Assert.AreEqual(GameStatus.Won, s.Status);
            Assert.AreEqual(17, RunResult.Score(s));
        }

        [Test]
        public void Win_AtFullHealth_FinalElixir_AddsBonus()
        {
            var s = Rules.NewRun(new[] { C(2), W(9), H(2), H(9) });
            Rules.Fight(s, C(2), useWeapon: false); // hp 18
            Rules.Equip(s, W(9));
            Rules.Drink(s, H(2));                   // hp 20
            Rules.Drink(s, H(9));                   // wasted, final card → Won

            Assert.AreEqual(GameStatus.Won, s.Status);
            Assert.AreEqual(20, s.Health);
            Assert.AreEqual(29, RunResult.Score(s), "20 hp + 9 wasted-final-potion bonus");
        }

        [Test]
        public void Loss_ScoreIsNegativeRemainingEnemyValues()
        {
            var s = Rules.NewRun(new[] { C(14), S(14), C(13), S(13), C(12) });
            Rules.Fight(s, C(14), useWeapon: false); // hp 6
            Rules.Fight(s, S(14), useWeapon: false); // dead

            Assert.AreEqual(GameStatus.Lost, s.Status);
            // Remaining: C13 + S13 in the room, C12 in the dungeon.
            Assert.AreEqual(-38, RunResult.Score(s));
        }

        [Test]
        public void Score_WhileInProgress_Throws()
        {
            var s = Rules.NewRun(1UL);
            Assert.Throws<InvalidMoveException>(() => RunResult.Score(s));
        }
    }
}
