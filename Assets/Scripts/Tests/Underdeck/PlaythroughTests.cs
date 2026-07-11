using NUnit.Framework;

namespace Underdeck.Core.Tests
{
    /// <summary>
    /// Whole-run simulations: a simple bot plays full seeded descents through
    /// all three depths. These catch state-machine bugs no unit test
    /// anticipates and confirm the engine always terminates cleanly.
    /// </summary>
    public class PlaythroughTests
    {
        private static void BotStep(RunState s)
        {
            switch (s.Phase)
            {
                case RunPhase.Room:
                {
                    Assert.Greater(s.Room.Count, 0, "Room phase must always have at least one card");
                    var c = s.Room[0];
                    switch (c.Kind)
                    {
                        case CardKind.Monster:
                            GameEngine.FightMonster(s, c, GameEngine.CanUseWeapon(s, c));
                            break;
                        case CardKind.Potion:
                            GameEngine.DrinkPotion(s, c);
                            break;
                        case CardKind.Treasure:
                            GameEngine.PocketTreasure(s, c);
                            break;
                        case CardKind.Cache:
                            GameEngine.OpenCache(s, c);
                            GameEngine.TakeDraftGold(s); // bot always takes the coin
                            GameEngine.AfterResolve(s);
                            break;
                        case CardKind.Curse:
                            GameEngine.SufferCurse(s, c);
                            break;
                    }
                    break;
                }
                case RunPhase.BossIntro:
                    GameEngine.EnterBossFight(s);
                    break;
                case RunPhase.Boss:
                {
                    bool useWeapon = GameEngine.CanUseWeapon(s, Card.Monster('♠', s.Boss.Threat));
                    var r = GameEngine.FightBoss(s, useWeapon);
                    if (r.BossOutcome == BossOutcome.Dead)
                    {
                        GameEngine.EndRun(s, false);
                    }
                    else if (r.BossOutcome == BossOutcome.Slain)
                    {
                        if (!GameEngine.TryCompleteRun(s))
                        {
                            var relics = GameEngine.GenerateRelicDraft(s);
                            if (relics.Count > 0) GameEngine.GainRelic(s, relics[0]);
                            GameEngine.AdvanceToNextDepth(s);
                        }
                    }
                    break;
                }
            }
        }

        private static RunState PlayOut(ulong seed)
        {
            var s = GameEngine.NewRun(seed.ToString());
            int guard = 0;
            while (s.Phase != RunPhase.Over)
            {
                BotStep(s);
                Assert.Less(++guard, 2000, "a run must terminate");
                Assert.That(s.Hp, Is.InRange(0, s.MaxHp));
                Assert.GreaterOrEqual(s.Focus, 0);
                Assert.That(s.Depth, Is.InRange(1, 3));
            }
            return s;
        }

        [Test]
        public void OneHundredSeededRuns_TerminateWithValidState()
        {
            int won = 0, lost = 0;
            for (ulong seed = 0; seed < 100; seed++)
            {
                var s = PlayOut(seed);
                if (s.Won) won++; else lost++;
                var score = GameEngine.ScoreParts(s);
                Assert.GreaterOrEqual(score.Total, s.Hp * 10 - 1000, "score is computable without throwing");
            }
            Assert.AreEqual(100, won + lost);
            // The naive bot should see a mix of outcomes — not every seed is a foregone conclusion.
            Assert.Greater(lost, 0, "some runs should be lost");
        }

        [Test]
        public void SameSeed_SameOutcome_SameDiscardSequence()
        {
            var a = PlayOut(777777UL);
            var b = PlayOut(777777UL);

            Assert.AreEqual(a.Won, b.Won);
            Assert.AreEqual(a.Depth, b.Depth);
            CollectionAssert.AreEqual(a.Discard, b.Discard);
        }
    }
}
