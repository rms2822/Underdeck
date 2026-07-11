using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Underdeck.Core;

namespace Underdeck.Core.Tests
{
    /// <summary>
    /// Cross-engine parity: the C# port must produce bit-identical dungeons
    /// and skill-deck shuffles to the reference JS engine in
    /// prototype/index.html, for the same seeds. Reference sequences were
    /// captured by running window.__ud.fns.buildDepthDeck and a fresh
    /// NewRun("7") directly in the JS engine.
    /// </summary>
    public class ParityTests
    {
        private static string Fmt(Card c) =>
            c.Kind == CardKind.Curse
                ? "curse:" + c.Curse.ToString().ToLowerInvariant()
                : c.Kind.ToString().ToLowerInvariant() + ":" + c.Suit + c.Rank +
                  (c.Elite != EliteKind.None ? ":" + c.Elite.ToString().ToLowerInvariant() : "");

        [Test]
        public void Depth1Deck_MatchesJsEngine_ForSeed7()
        {
            var expected = new[]
            {
                "treasure:♦6", "monster:♠7", "monster:♠3", "treasure:♦2", "treasure:♦5",
                "potion:♥3", "monster:♠5", "monster:♣5", "monster:♣2", "monster:♠2",
                "potion:♥4", "monster:♠4", "monster:♣3", "treasure:♦4", "monster:♣8",
                "monster:♣6", "monster:♠6", "potion:♥5", "treasure:♦3", "monster:♣4",
                "cache:♦11", "monster:♣7", "potion:♥2", "potion:♥6", "monster:♠8",
            };

            var deck = DeckBuilder.BuildDepthDeck(1, 7UL);

            Assert.AreEqual(25, deck.Count, "depth 1: 14 monsters + 5 treasure + 1 cache + 5 potions");
            CollectionAssert.AreEqual(expected, deck.Select(Fmt).ToList());
        }

        [Test]
        public void Depth3Deck_MatchesJsEngine_ForSeed424242_IncludingEliteAssignment()
        {
            var expected = new[]
            {
                "monster:♣12:armored", "treasure:♦10", "monster:♠14", "monster:♠12", "monster:♠11",
                "monster:♠9", "treasure:♦7", "treasure:♦6", "potion:♥9", "curse:rust",
                "curse:blight", "monster:♠13:brutal", "monster:♣13", "treasure:♦8", "monster:♣9",
                "potion:♥8", "monster:♣10", "monster:♣11", "cache:♦13", "treasure:♦9",
                "monster:♠10", "potion:♥7", "monster:♣14:brutal", "potion:♥10",
            };

            var deck = DeckBuilder.BuildDepthDeck(3, 424242UL);

            Assert.AreEqual(24, deck.Count, "depth 3: 12 monsters + 2 curses + 5 treasure + 1 cache + 4 potions");
            CollectionAssert.AreEqual(expected, deck.Select(Fmt).ToList());
        }

        [Test]
        public void StarterSkillDeck_ShuffleOrder_MatchesJsEngine_ForSeed7()
        {
            var expected = new List<string>
            {
                "guard", "strike", "strike", "sharpen", "strike",
                "guard", "slip", "scout", "guard", "strike",
            };

            var s = GameEngine.NewRun("7");
            // NewRun deals the first room, which draws a hand of 3 from the front
            // of the shuffled starter deck; hand + remaining draw pile reconstructs
            // the full shuffled order (mirrors how the reference sequence was captured).
            var fullOrder = s.Skills.Hand.Concat(s.Skills.Draw).ToList();

            CollectionAssert.AreEqual(expected, fullOrder);
        }

        [Test]
        public void NewRun_DungeonMatchesDirectBuildDepthDeck_ForSeed7()
        {
            var direct = DeckBuilder.BuildDepthDeck(1, 7UL);
            var s = GameEngine.NewRun("7");
            var fromRun = s.Room.Concat(s.Deck).ToList();

            CollectionAssert.AreEqual(direct, fromRun, "NewRun's dungeon must equal BuildDepthDeck(1, seed) directly");
        }
    }
}
