using System.Collections.Generic;

namespace Scroundel.Core
{
    /// <summary>
    /// Builds the canonical 44-card dungeon deck (GAME_PLAN §2):
    /// 26 enemies (clubs + spades 2–A), 9 weapons (diamonds 2–10),
    /// 9 elixirs (hearts 2–10). Red face cards and red aces are excluded.
    /// </summary>
    public static class Deck
    {
        public const int Size = 44;

        /// <summary>All 44 cards in canonical (unshuffled) order.</summary>
        public static List<Card> CreateDungeon()
        {
            var cards = new List<Card>(Size);
            for (int rank = 2; rank <= Card.Ace; rank++)
            {
                cards.Add(new Card(Suit.Clubs, rank));
                cards.Add(new Card(Suit.Spades, rank));
            }
            for (int rank = 2; rank <= 10; rank++)
            {
                cards.Add(new Card(Suit.Diamonds, rank));
                cards.Add(new Card(Suit.Hearts, rank));
            }
            return cards;
        }

        /// <summary>The dungeon deck shuffled deterministically by seed.</summary>
        public static List<Card> CreateShuffledDungeon(ulong seed)
        {
            var cards = CreateDungeon();
            Shuffle(cards, seed);
            return cards;
        }

        /// <summary>Seeded Fisher–Yates shuffle.</summary>
        public static void Shuffle<T>(IList<T> list, ulong seed)
        {
            var rng = new Rng(seed);
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.NextInt(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }
    }
}
