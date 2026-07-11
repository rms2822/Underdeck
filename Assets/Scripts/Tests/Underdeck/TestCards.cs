namespace Underdeck.Core.Tests
{
    /// <summary>Terse card factories and a hand-rigging helper for deterministic test setups.</summary>
    internal static class TestCards
    {
        public static Card M(int r, EliteKind elite = EliteKind.None) => Card.Monster('♠', r, elite);
        public static Card P(int r) => Card.Potion(r);
        public static Card T(int r) => Card.Treasure(r);
        public static Card C(int r) => Card.Cache(r);
        public static Card Curse(CurseKind k) => Card.CurseCard(k);

        /// <summary>Overwrites the room with exactly these cards (test-only convenience;
        /// production code never mutates Room except through GameEngine).</summary>
        public static void SetRoom(RunState s, params Card[] cards)
        {
            s.Room.Clear();
            s.Room.AddRange(cards);
        }

        public static void SetHand(RunState s, params string[] skillIds)
        {
            s.Skills.Hand.Clear();
            s.Skills.Hand.AddRange(skillIds);
            s.LockedIdx = -1;
        }
    }
}
