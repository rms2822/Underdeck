using System.Linq;
using NUnit.Framework;

namespace Underdeck.Core.Tests
{
    public class DeckBuilderTests
    {
        [Test]
        public void Depth1_Composition()
        {
            var deck = DeckBuilder.BuildDepthDeck(1, 1UL);
            Assert.AreEqual(25, deck.Count);
            Assert.AreEqual(14, deck.Count(c => c.Kind == CardKind.Monster));
            Assert.AreEqual(5, deck.Count(c => c.Kind == CardKind.Treasure));
            Assert.AreEqual(1, deck.Count(c => c.Kind == CardKind.Cache));
            Assert.AreEqual(5, deck.Count(c => c.Kind == CardKind.Potion));
            Assert.AreEqual(0, deck.Count(c => c.Kind == CardKind.Curse));
            Assert.IsTrue(deck.All(c => c.Elite == EliteKind.None), "no elites before Depth III");
        }

        [Test]
        public void Depth2_Composition_IntroducesCurses()
        {
            var deck = DeckBuilder.BuildDepthDeck(2, 1UL);
            Assert.AreEqual(28, deck.Count);
            Assert.AreEqual(14, deck.Count(c => c.Kind == CardKind.Monster));
            Assert.AreEqual(3, deck.Count(c => c.Kind == CardKind.Curse));
            CollectionAssert.AreEquivalent(
                new[] { CurseKind.Blight, CurseKind.Thief, CurseKind.Rust },
                deck.Where(c => c.Kind == CardKind.Curse).Select(c => c.Curse));
        }

        [Test]
        public void Depth3_Composition_IntroducesElites()
        {
            var deck = DeckBuilder.BuildDepthDeck(3, 1UL);
            Assert.AreEqual(24, deck.Count);
            Assert.AreEqual(12, deck.Count(c => c.Kind == CardKind.Monster));
            Assert.AreEqual(2, deck.Count(c => c.Kind == CardKind.Curse));
            Assert.AreEqual(3, deck.Count(c => c.Elite != EliteKind.None), "exactly 3 elite monsters");
            Assert.IsTrue(deck.Where(c => c.Elite != EliteKind.None).All(c => c.Kind == CardKind.Monster));
        }

        [Test]
        public void Shuffle_SameSeed_SameOrder()
        {
            var a = DeckBuilder.BuildDepthDeck(1, 555UL);
            var b = DeckBuilder.BuildDepthDeck(1, 555UL);
            CollectionAssert.AreEqual(a, b);
        }

        [Test]
        public void Shuffle_DifferentSeeds_DifferentOrder()
        {
            var a = DeckBuilder.BuildDepthDeck(1, 1UL);
            var b = DeckBuilder.BuildDepthDeck(1, 2UL);
            CollectionAssert.AreNotEqual(a, b);
        }

        [TestCase(1UL)][TestCase(42UL)][TestCase(999999UL)]
        public void Depth3_EliteAssignment_NeverDoublesUpOnOneMonster(ulong seed)
        {
            var deck = DeckBuilder.BuildDepthDeck(3, seed);
            var elites = deck.Where(c => c.Elite != EliteKind.None).ToList();
            Assert.AreEqual(3, elites.Count);
            Assert.AreEqual(3, elites.Select(c => (c.Suit, c.Rank)).Distinct().Count(), "3 distinct monsters, no double-tagging");
        }
    }
}
