namespace Scroundel.Core
{
    /// <summary>
    /// Scoring (GAME_PLAN §2):
    ///  - Won  → remaining HP; if at full health and the final card resolved
    ///           was an elixir, its value is added (the classic "wasted final
    ///           potion" bonus).
    ///  - Lost → negative sum of every enemy value still in the dungeon + room.
    /// </summary>
    public static class RunResult
    {
        public static int Score(GameState s)
        {
            switch (s.Status)
            {
                case GameStatus.Won:
                    int score = s.Health;
                    if (s.Health == s.MaxHealth
                        && s.LastResolved.HasValue
                        && s.LastResolved.Value.Kind == CardKind.Elixir)
                        score += s.LastResolved.Value.Value;
                    return score;

                case GameStatus.Lost:
                    int remaining = 0;
                    foreach (var card in s.DungeonPile)
                        if (card.Kind == CardKind.Enemy)
                            remaining += card.Value;
                    foreach (var card in s.RoomCards)
                        if (card.Kind == CardKind.Enemy)
                            remaining += card.Value;
                    return -remaining;

                default:
                    throw new InvalidMoveException("Run is still in progress.");
            }
        }
    }
}
