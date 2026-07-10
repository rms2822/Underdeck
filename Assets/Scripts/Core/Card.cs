using System;

namespace Scroundel.Core
{
    public enum Suit { Clubs, Diamonds, Hearts, Spades }

    public enum CardKind { Enemy, Weapon, Elixir }

    /// <summary>
    /// One dungeon card. Immutable. Ranks are 2–10 plus J=11, Q=12, K=13, A=14.
    /// Clubs and Spades are Enemies (ranks 2–14); Diamonds are Weapons and
    /// Hearts are Elixirs (ranks 2–10 only — the red face cards and red aces
    /// are removed from the dungeon deck, per GAME_PLAN §2).
    /// </summary>
    public readonly struct Card : IEquatable<Card>
    {
        public const int Jack = 11;
        public const int Queen = 12;
        public const int King = 13;
        public const int Ace = 14;

        public Suit Suit { get; }
        public int Rank { get; }

        public Card(Suit suit, int rank)
        {
            bool isBlack = suit == Suit.Clubs || suit == Suit.Spades;
            int maxRank = isBlack ? Ace : 10;
            if (rank < 2 || rank > maxRank)
                throw new ArgumentOutOfRangeException(
                    nameof(rank), $"{suit} rank must be 2–{maxRank}, got {rank}.");
            Suit = suit;
            Rank = rank;
        }

        public CardKind Kind => Suit switch
        {
            Suit.Diamonds => CardKind.Weapon,
            Suit.Hearts => CardKind.Elixir,
            _ => CardKind.Enemy,
        };

        /// <summary>Enemy threat, weapon power, or elixir healing — always the rank.</summary>
        public int Value => Rank;

        public bool Equals(Card other) => Suit == other.Suit && Rank == other.Rank;
        public override bool Equals(object obj) => obj is Card other && Equals(other);
        public override int GetHashCode() => ((int)Suit << 4) | Rank;
        public static bool operator ==(Card a, Card b) => a.Equals(b);
        public static bool operator !=(Card a, Card b) => !a.Equals(b);

        public override string ToString()
        {
            string rank = Rank switch
            {
                Jack => "J",
                Queen => "Q",
                King => "K",
                Ace => "A",
                _ => Rank.ToString(),
            };
            string suit = Suit switch
            {
                Suit.Clubs => "♣",
                Suit.Diamonds => "♦",
                Suit.Hearts => "♥",
                _ => "♠",
            };
            return rank + suit;
        }
    }
}
