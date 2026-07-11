using System.Collections.Generic;

namespace Underdeck.Core
{
    /// <summary>
    /// Builds a depth's dungeon deck — a direct port of the JS `buildDepthDeck`.
    /// The RNG consumption order (card construction, elite assignment, then
    /// shuffle) must match exactly for cross-engine seed parity.
    /// </summary>
    public static class DeckBuilder
    {
        public static List<Card> BuildDepthDeck(int depth, ulong seed)
        {
            var rng = new SplitMix64(seed);
            var cards = new List<Card>();

            void Mon(int r)
            {
                cards.Add(Card.Monster('♠', r));
                cards.Add(Card.Monster('♣', r));
            }

            if (depth == 1)
            {
                for (int r = 2; r <= 8; r++) Mon(r);                 // 14 monsters
                for (int r = 2; r <= 6; r++) cards.Add(Card.Treasure(r));
                cards.Add(Card.Cache(11));
                for (int r = 2; r <= 6; r++) cards.Add(Card.Potion(r));
            }
            else if (depth == 2)
            {
                for (int r = 5; r <= 11; r++) Mon(r);                // 14 monsters
                cards.Add(Card.CurseCard(CurseKind.Blight));
                cards.Add(Card.CurseCard(CurseKind.Thief));
                cards.Add(Card.CurseCard(CurseKind.Rust));
                for (int r = 4; r <= 8; r++) cards.Add(Card.Treasure(r));
                cards.Add(Card.Cache(12));
                for (int r = 4; r <= 8; r++) cards.Add(Card.Potion(r));
            }
            else
            {
                for (int r = 9; r <= 14; r++) Mon(r);                // 12 monsters
                cards.Add(Card.CurseCard(CurseKind.Blight));
                cards.Add(Card.CurseCard(CurseKind.Rust));
                for (int r = 6; r <= 10; r++) cards.Add(Card.Treasure(r));
                cards.Add(Card.Cache(13));
                for (int r = 7; r <= 10; r++) cards.Add(Card.Potion(r));

                // Three elites among the monsters — same RNG order as the JS engine:
                // pick a random monster index, reroll on collision, then roll the kind.
                var monsterIdx = new List<int>();
                for (int i = 0; i < cards.Count; i++)
                    if (cards[i].Kind == CardKind.Monster) monsterIdx.Add(i);

                var elited = new HashSet<int>();
                for (int i = 0; i < 3; i++)
                {
                    int pick;
                    do { pick = monsterIdx[rng.NextInt(monsterIdx.Count)]; }
                    while (elited.Contains(pick));
                    elited.Add(pick);
                    var kind = rng.NextInt(2) == 0 ? EliteKind.Armored : EliteKind.Brutal;
                    var c = cards[pick];
                    cards[pick] = new Card(c.Kind, c.Suit, c.Rank, kind, c.Curse);
                }
            }

            RngUtil.Shuffle(cards, ref rng);
            return cards;
        }
    }
}
