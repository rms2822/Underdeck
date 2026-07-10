using System.Collections.Generic;

namespace Scroundel.Core
{
    public enum GameStatus { InProgress, Won, Lost }

    /// <summary>
    /// The authoritative state of a single run. Mutated only by
    /// <see cref="Rules"/> — presentation code reads it and calls Rules
    /// methods; it never writes (GAME_PLAN §5.3).
    /// </summary>
    public sealed class GameState
    {
        public const int RoomSize = 4;

        internal readonly List<Card> DungeonPile; // index 0 = top of the deck
        internal readonly List<Card> RoomCards;
        internal readonly List<Card> DiscardPile;

        internal GameState(List<Card> dungeonTopFirst, RulesConfig config)
        {
            Config = config;
            DungeonPile = dungeonTopFirst;
            RoomCards = new List<Card>(RoomSize);
            DiscardPile = new List<Card>(Deck.Size);
            Health = config.StartingHealth;
        }

        public RulesConfig Config { get; }
        public GameStatus Status { get; internal set; }

        public int Health { get; internal set; }
        public int MaxHealth => Config.MaxHealth;

        public Card? EquippedWeapon { get; internal set; }

        /// <summary>
        /// Highest enemy value the equipped weapon may still strike
        /// (degradation); null = weapon unused, may strike anything.
        /// </summary>
        public int? WeaponLimit { get; internal set; }

        /// <summary>The face-up cards of the current room (up to 4).</summary>
        public IReadOnlyList<Card> Room => RoomCards;

        /// <summary>Resolved cards, in resolution order.</summary>
        public IReadOnlyList<Card> Discard => DiscardPile;

        /// <summary>Cards left in the face-down dungeon pile.</summary>
        public int DungeonCount => DungeonPile.Count;

        /// <summary>True once an elixir has been drunk in the current room.</summary>
        public bool ElixirDrunkThisRoom { get; internal set; }

        /// <summary>True when the previous room was fled — no fleeing twice in a row.</summary>
        public bool FledLastRoom { get; internal set; }

        /// <summary>Cards resolved in the current room (fleeing requires 0).</summary>
        public int ResolvedThisRoom { get; internal set; }

        /// <summary>The most recently resolved card (used by scoring).</summary>
        public Card? LastResolved { get; internal set; }
    }
}
