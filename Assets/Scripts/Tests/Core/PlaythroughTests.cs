using System.Linq;
using NUnit.Framework;

namespace Scroundel.Core.Tests
{
    /// <summary>
    /// Whole-game simulations: a simple bot plays full seeded runs. These catch
    /// state-machine bugs no unit test anticipates and pin down the determinism
    /// that daily challenges (GAME_PLAN §3.5) depend on.
    /// </summary>
    public class PlaythroughTests
    {
        /// <summary>Always resolves the first room card; fights with the weapon when legal.</summary>
        private static void BotStep(GameState s)
        {
            var card = s.Room[0];
            switch (card.Kind)
            {
                case CardKind.Weapon:
                    Rules.Equip(s, card);
                    break;
                case CardKind.Elixir:
                    Rules.Drink(s, card);
                    break;
                default:
                    Rules.Fight(s, card, Rules.CanUseWeaponOn(s, card));
                    break;
            }
        }

        private static GameState PlayOut(ulong seed)
        {
            var s = Rules.NewRun(seed);
            int guard = 0;
            while (s.Status == GameStatus.InProgress)
            {
                BotStep(s);
                Assert.Less(++guard, 100, "runs must terminate");

                int total = s.DungeonCount + s.Room.Count + s.Discard.Count;
                Assert.AreEqual(Deck.Size, total, "cards must be conserved");
                Assert.That(s.Health, Is.InRange(0, s.MaxHealth));
            }
            return s;
        }

        [Test]
        public void HundredSeededRuns_TerminateWithValidState()
        {
            int won = 0, lost = 0;
            for (ulong seed = 0; seed < 100; seed++)
            {
                var s = PlayOut(seed);
                if (s.Status == GameStatus.Won) won++; else lost++;
                Assert.DoesNotThrow(() => RunResult.Score(s));
            }
            // The naive bot should at least see both outcomes across 100 dungeons.
            Assert.Greater(lost, 0, "some runs should be lost");
            Assert.AreEqual(100, won + lost);
        }

        [Test]
        public void SameSeed_SameOutcome_SameCardSequence()
        {
            var a = PlayOut(424242UL);
            var b = PlayOut(424242UL);

            Assert.AreEqual(a.Status, b.Status);
            Assert.AreEqual(RunResult.Score(a), RunResult.Score(b));
            CollectionAssert.AreEqual(
                a.Discard.Select(c => c.ToString()).ToList(),
                b.Discard.Select(c => c.ToString()).ToList());
        }
    }
}
