using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Scroundel.Core.Tests
{
    public class DeckTests
    {
        [Test]
        public void Dungeon_Has44Cards_26Enemies_9Weapons_9Elixirs()
        {
            var deck = Deck.CreateDungeon();

            Assert.AreEqual(Deck.Size, deck.Count);
            Assert.AreEqual(44, deck.Count);
            Assert.AreEqual(26, deck.Count(c => c.Kind == CardKind.Enemy));
            Assert.AreEqual(9, deck.Count(c => c.Kind == CardKind.Weapon));
            Assert.AreEqual(9, deck.Count(c => c.Kind == CardKind.Elixir));
        }

        [Test]
        public void Dungeon_HasNoRedFaceCardsOrRedAces()
        {
            var deck = Deck.CreateDungeon();
            foreach (var card in deck)
                if (card.Suit == Suit.Diamonds || card.Suit == Suit.Hearts)
                    Assert.LessOrEqual(card.Rank, 10, $"{card} should not exist");
        }

        [Test]
        public void Dungeon_AllCardsUnique()
        {
            var deck = Deck.CreateDungeon();
            Assert.AreEqual(deck.Count, new HashSet<Card>(deck).Count);
        }

        [Test]
        public void Shuffle_SameSeed_SameOrder()
        {
            var a = Deck.CreateShuffledDungeon(12345UL);
            var b = Deck.CreateShuffledDungeon(12345UL);
            CollectionAssert.AreEqual(a, b);
        }

        [Test]
        public void Shuffle_DifferentSeeds_DifferentOrder()
        {
            var a = Deck.CreateShuffledDungeon(1UL);
            var b = Deck.CreateShuffledDungeon(2UL);
            CollectionAssert.AreNotEqual(a, b);
        }

        [Test]
        public void Shuffle_IsPermutationOfCanonicalDeck()
        {
            var shuffled = Deck.CreateShuffledDungeon(99UL);
            CollectionAssert.AreEquivalent(Deck.CreateDungeon(), shuffled);
        }
    }

    public class CardTests
    {
        [Test]
        public void KindsFollowSuits()
        {
            Assert.AreEqual(CardKind.Enemy, new Card(Suit.Clubs, 5).Kind);
            Assert.AreEqual(CardKind.Enemy, new Card(Suit.Spades, Card.Ace).Kind);
            Assert.AreEqual(CardKind.Weapon, new Card(Suit.Diamonds, 5).Kind);
            Assert.AreEqual(CardKind.Elixir, new Card(Suit.Hearts, 5).Kind);
        }

        [Test]
        public void BlackAce_IsValue14()
        {
            Assert.AreEqual(14, new Card(Suit.Spades, Card.Ace).Value);
        }

        [TestCase(Suit.Hearts, 11)]
        [TestCase(Suit.Diamonds, 14)]
        [TestCase(Suit.Clubs, 1)]
        [TestCase(Suit.Spades, 15)]
        public void InvalidRanks_Throw(Suit suit, int rank)
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => _ = new Card(suit, rank));
        }
    }
}
