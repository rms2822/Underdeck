namespace Scroundel.Core.Tests
{
    /// <summary>Terse card factories for building explicit test dungeons.</summary>
    internal static class TestCards
    {
        /// <summary>Club enemy.</summary>
        public static Card C(int rank) => new Card(Suit.Clubs, rank);

        /// <summary>Spade enemy.</summary>
        public static Card S(int rank) => new Card(Suit.Spades, rank);

        /// <summary>Diamond weapon.</summary>
        public static Card W(int rank) => new Card(Suit.Diamonds, rank);

        /// <summary>Heart elixir.</summary>
        public static Card H(int rank) => new Card(Suit.Hearts, rank);
    }
}
