using System;
using System.Collections.Generic;

namespace Scroundel.Core
{
    /// <summary>Thrown when an action violates the rules. UI bugs surface loudly.</summary>
    public class InvalidMoveException : InvalidOperationException
    {
        public InvalidMoveException(string message) : base(message) { }
    }

    /// <summary>
    /// Tunable rule knobs (class twists and §3.5 modifiers layer over these —
    /// never as special cases inside <see cref="Rules"/> itself).
    /// </summary>
    public sealed class RulesConfig
    {
        public int StartingHealth { get; set; } = 20;
        public int MaxHealth { get; set; } = 20;

        /// <summary>
        /// Whether a used weapon may strike an enemy of *equal* value to the
        /// last one it slew (GAME_PLAN §2 plays ≤; set false for strict &lt;).
        /// </summary>
        public bool WeaponCanStrikeEqual { get; set; } = true;

        public static RulesConfig Default => new RulesConfig();
    }

    /// <summary>
    /// The complete Scroundel ruleset (GAME_PLAN §2). Stateless: every method
    /// validates, then mutates the given <see cref="GameState"/>.
    ///
    /// Room flow: the room holds up to 4 cards; when one card remains and the
    /// dungeon can refill, a new room begins around it ("resolve 3, carry 1").
    /// Once the dungeon is empty the final room is played out entirely.
    /// </summary>
    public static class Rules
    {
        // ---------- run setup ----------

        public static GameState NewRun(ulong seed) =>
            NewRun(Deck.CreateShuffledDungeon(seed), RulesConfig.Default);

        public static GameState NewRun(ulong seed, RulesConfig config) =>
            NewRun(Deck.CreateShuffledDungeon(seed), config);

        /// <summary>
        /// Start a run from an explicit deck order (top card first). Used by
        /// tests and, later, curated daily-challenge dungeons.
        /// </summary>
        public static GameState NewRun(IEnumerable<Card> dungeonTopFirst, RulesConfig config = null)
        {
            var pile = new List<Card>(dungeonTopFirst);
            var seen = new HashSet<Card>();
            foreach (var card in pile)
                if (!seen.Add(card))
                    throw new ArgumentException($"Duplicate card {card} in dungeon.");

            var state = new GameState(pile, config ?? RulesConfig.Default);
            DealRoom(state, fled: false);
            return state;
        }

        // ---------- queries (the combat preview is built on these) ----------

        public static bool CanFlee(GameState s) =>
            s.Status == GameStatus.InProgress
            && !s.FledLastRoom
            && s.ResolvedThisRoom == 0
            && s.RoomCards.Count > 0;

        /// <summary>Can the equipped weapon legally strike this enemy?</summary>
        public static bool CanUseWeaponOn(GameState s, Card enemy)
        {
            if (enemy.Kind != CardKind.Enemy || !s.EquippedWeapon.HasValue)
                return false;
            if (!s.WeaponLimit.HasValue)
                return true; // fresh weapon strikes anything
            return s.Config.WeaponCanStrikeEqual
                ? enemy.Value <= s.WeaponLimit.Value
                : enemy.Value < s.WeaponLimit.Value;
        }

        /// <summary>HP the hero will lose — exactly what the combat preview shows.</summary>
        public static int PreviewDamage(GameState s, Card enemy, bool useWeapon)
        {
            RequireKind(enemy, CardKind.Enemy);
            if (!useWeapon)
                return enemy.Value;
            if (!CanUseWeaponOn(s, enemy))
                throw new InvalidMoveException($"Weapon cannot strike {enemy}.");
            var weapon = s.EquippedWeapon.Value;
            return Math.Max(0, enemy.Value - weapon.Value);
        }

        // ---------- actions ----------

        /// <summary>
        /// Fight an enemy, barehanded or with the equipped weapon. Using the
        /// weapon degrades it: thereafter it may only strike enemies of value
        /// ≤ this one (see <see cref="RulesConfig.WeaponCanStrikeEqual"/>).
        /// Returns the damage taken.
        /// </summary>
        public static int Fight(GameState s, Card enemy, bool useWeapon)
        {
            RequireInProgress(s);
            RequireInRoom(s, enemy);
            RequireKind(enemy, CardKind.Enemy);
            if (useWeapon && !CanUseWeaponOn(s, enemy))
                throw new InvalidMoveException(
                    s.EquippedWeapon.HasValue
                        ? $"Weapon cannot strike {enemy} (limit {s.WeaponLimit})."
                        : "No weapon equipped.");

            int damage = PreviewDamage(s, enemy, useWeapon);
            if (useWeapon)
                s.WeaponLimit = enemy.Value; // degradation
            s.Health = Math.Max(0, s.Health - damage);
            Resolve(s, enemy);
            return damage;
        }

        /// <summary>Equip a weapon, replacing the current one (degradation resets).</summary>
        public static void Equip(GameState s, Card weapon)
        {
            RequireInProgress(s);
            RequireInRoom(s, weapon);
            RequireKind(weapon, CardKind.Weapon);

            s.EquippedWeapon = weapon;
            s.WeaponLimit = null;
            Resolve(s, weapon);
        }

        /// <summary>
        /// Drink an elixir. Only the first elixir per room heals; later ones in
        /// the same room are wasted (still discarded). Returns HP restored.
        /// </summary>
        public static int Drink(GameState s, Card elixir)
        {
            RequireInProgress(s);
            RequireInRoom(s, elixir);
            RequireKind(elixir, CardKind.Elixir);

            int healed = 0;
            if (!s.ElixirDrunkThisRoom)
            {
                healed = Math.Min(s.MaxHealth - s.Health, elixir.Value);
                s.Health += healed;
                s.ElixirDrunkThisRoom = true;
            }
            Resolve(s, elixir);
            return healed;
        }

        /// <summary>
        /// Flee the room before acting in it: all room cards go to the bottom
        /// of the dungeon (in display order) and a fresh room is dealt. Not
        /// allowed twice in a row.
        /// </summary>
        public static void Flee(GameState s)
        {
            RequireInProgress(s);
            if (!CanFlee(s))
                throw new InvalidMoveException(s.FledLastRoom
                    ? "Cannot flee two rooms in a row."
                    : "Cannot flee after acting in this room.");

            s.DungeonPile.AddRange(s.RoomCards);
            s.RoomCards.Clear();
            DealRoom(s, fled: true);
        }

        // ---------- internals ----------

        private static void Resolve(GameState s, Card card)
        {
            s.RoomCards.Remove(card);
            s.DiscardPile.Add(card);
            s.LastResolved = card;
            s.ResolvedThisRoom++;

            if (s.Health <= 0)
            {
                s.Status = GameStatus.Lost;
                return;
            }
            if (s.RoomCards.Count == 0 && s.DungeonPile.Count == 0)
            {
                s.Status = GameStatus.Won;
                return;
            }
            // "Resolve 3, carry 1": one card remains and the dungeon can refill.
            if (s.RoomCards.Count == 1 && s.DungeonPile.Count > 0)
                DealRoom(s, fled: false);
        }

        private static void DealRoom(GameState s, bool fled)
        {
            while (s.RoomCards.Count < GameState.RoomSize && s.DungeonPile.Count > 0)
            {
                s.RoomCards.Add(s.DungeonPile[0]);
                s.DungeonPile.RemoveAt(0);
            }
            s.ElixirDrunkThisRoom = false;
            s.FledLastRoom = fled;
            s.ResolvedThisRoom = 0;

            // Degenerate: an empty dungeon was supplied.
            if (s.RoomCards.Count == 0 && s.Status == GameStatus.InProgress)
                s.Status = GameStatus.Won;
        }

        private static void RequireInProgress(GameState s)
        {
            if (s.Status != GameStatus.InProgress)
                throw new InvalidMoveException($"Run is over ({s.Status}).");
        }

        private static void RequireInRoom(GameState s, Card card)
        {
            if (!s.RoomCards.Contains(card))
                throw new InvalidMoveException($"{card} is not in the room.");
        }

        private static void RequireKind(Card card, CardKind kind)
        {
            if (card.Kind != kind)
                throw new InvalidMoveException($"{card} is not a {kind}.");
        }
    }
}
