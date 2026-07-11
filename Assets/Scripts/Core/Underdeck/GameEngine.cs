using System;
using System.Collections.Generic;

namespace Underdeck.Core
{
    /// <summary>
    /// The complete Underdeck ruleset — a faithful, verified port of the
    /// reference JS engine in prototype/index.html. Stateless: every method
    /// validates, then mutates the given <see cref="RunState"/>.
    ///
    /// Three deliberate small fixes over the source JS (each is a one-line,
    /// clearly-scoped correction, not a design change):
    ///  - Whetstone's dulling floor is 4 (matching its own flavor text,
    ///    "never dulls below 4"); the JS accidentally floors at 5.
    ///  - Ember Heart's tie-break for "strongest burnable monster" keeps the
    ///    first-found card on a tie, matching the JS's Array.reduce semantics
    ///    (a naive port using `>=` would silently invert this).
    ///  - Cancelling an armed Smoke Bomb returns the two drawn cards to the
    ///    bottom of the deck instead of discarding them from the game.
    /// </summary>
    public static partial class GameEngine
    {
        // ---------- run / room lifecycle ----------

        public static RunState NewRun(string seed)
        {
            ulong baseSeed = Seeding.Parse(seed);
            var s = new RunState
            {
                Seed = seed,
                BaseSeedValue = baseSeed,
                Rng = new SplitMix64(Seeding.AbilityStream(baseSeed)),
                Weapon = Content.Weapons["dagger"],
                Deck = DeckBuilder.BuildDepthDeck(1, baseSeed),
            };
            s.Skills.Draw = new List<string>(Content.StarterDeck);
            RngUtil.Shuffle(s.Skills.Draw, ref s.Rng);
            DealRoom(s, false);
            return s;
        }

        public static bool HasRelic(RunState s, string id) => s.Relics.Contains(id);

        public static void GoldGain(RunState s, int n) => s.Gold += HasRelic(s, "glasssigil") ? n * 2 : n;

        public static void DrawHand(RunState s)
        {
            s.Skills.Disc.AddRange(s.Skills.Hand);
            s.Skills.Hand.Clear();
            for (int i = 0; i < 3; i++)
            {
                if (s.Skills.Draw.Count == 0)
                {
                    s.Skills.Draw = s.Skills.Disc;
                    s.Skills.Disc = new List<string>();
                    RngUtil.Shuffle(s.Skills.Draw, ref s.Rng);
                }
                if (s.Skills.Draw.Count > 0)
                {
                    s.Skills.Hand.Add(s.Skills.Draw[0]);
                    s.Skills.Draw.RemoveAt(0);
                }
            }
            s.Focus = 2;
            s.LockedIdx = -1;
        }

        public static void DealRoom(RunState s, bool fled)
        {
            while (s.Room.Count < 4 && s.Deck.Count > 0)
            {
                s.Room.Add(s.Deck[0]);
                s.Deck.RemoveAt(0);
            }
            s.FledLast = fled;
            s.SlipActive = false;
            s.SnipePending = false;
            s.ActedThisRoom = 0;
            s.PotionDrunk = false; s.SecondPotion = false;
            s.StrikeNext = 0; s.GritNext = 0; s.Block = 0; s.PreserveNext = false;
            s.RoomNo++;
            DrawHand(s);
        }

        public static bool CanFleeNow(RunState s) =>
            s.Phase == RunPhase.Room && s.Room.Count > 0 && s.ActedThisRoom == 0 &&
            (!s.FledLast || HasRelic(s, "boots") || s.SlipActive);

        public static bool Flee(RunState s)
        {
            if (!CanFleeNow(s) || s.Mode != InputMode.None) return false;
            s.Deck.AddRange(s.Room);
            s.Room.Clear();
            DealRoom(s, true);
            return true;
        }

        // ---------- combat ----------

        public static int EffLimit(RunState s)
        {
            if (s.WeaponLimit == null) return int.MaxValue;
            // Whetstone: "never dulls below 4" — floors at 4, matching its own text.
            return HasRelic(s, "whetstone") ? Math.Max(s.WeaponLimit.Value, 4) : s.WeaponLimit.Value;
        }

        public static int EffPower(RunState s) => s.Weapon != null ? s.Weapon.Power + s.WeaponBonus : 0;

        public static bool CanUseWeapon(RunState s, Card c)
        {
            if (s.Weapon == null || c.Kind != CardKind.Monster) return false;
            if (s.Weapon.Id == "whisper" && c.Rank <= 4) return true;
            return c.Rank <= EffLimit(s); // equal-or-lower: finishes off a matching rank
        }

        public static int ThreatOf(Card c) => c.Rank + (c.Elite == EliteKind.Brutal ? 2 : 0);

        public static FightPreview PreviewFight(RunState s, Card c, bool useWeapon)
        {
            int power = 0;
            if (useWeapon && s.Weapon != null)
            {
                power = EffPower(s);
                if (c.Elite == EliteKind.Armored) power -= 2;
                if (s.Phase == RunPhase.Boss && s.Boss.Special == BossSpecial.Pierce) power -= 3;
            }
            power += s.StrikeNext;
            int dmg = Math.Max(0, ThreatOf(c) - Math.Max(0, power) - s.GritNext);
            int blocked = Math.Min(s.Block, dmg);
            return new FightPreview(dmg - blocked, blocked);
        }

        public static HpOutcome LoseHp(RunState s, int n)
        {
            s.Hp = Math.Max(0, s.Hp - n);
            if (s.Hp == 0 && HasRelic(s, "phoenix") && !s.PhoenixUsed)
            {
                s.PhoenixUsed = true;
                s.Hp = 1;
                return HpOutcome.Phoenix;
            }
            return s.Hp == 0 ? HpOutcome.Dead : HpOutcome.Ok;
        }

        public static void OnSlain(RunState s, Card c, bool weaponUsed)
        {
            s.Slain++;
            GoldGain(s, 1);
            if (weaponUsed && s.Weapon != null && s.Weapon.Id == "kris") s.Hp = Math.Min(s.MaxHp, s.Hp + 1);
            if (HasRelic(s, "gravebind"))
            {
                s.GraveTick++;
                if (s.GraveTick >= 5) { s.GraveTick = 0; s.Skills.Disc.Add("revenant"); }
            }
        }

        public static void ApplyDull(RunState s, Card c, bool noDull, bool tookNoDamage)
        {
            if (s.Weapon == null || noDull) return;
            if (s.Weapon.Id == "whisper" && c.Rank <= 4) return;
            if (s.Weapon.Id == "rapier" && tookNoDamage) return;
            if (s.PreserveNext) { s.PreserveNext = false; return; }
            int dullTo = s.Weapon.Id == "hammer" ? c.Rank - 2 : c.Rank;
            s.WeaponLimit = s.WeaponLimit == null ? dullTo : Math.Min(s.WeaponLimit.Value, dullTo);
        }

        /// <summary>
        /// Resolves a monster fight from the room: applies damage/dulling, removes
        /// the card, and advances the room (<see cref="AfterResolve"/>) — a single
        /// self-contained action, consistent with <see cref="DrinkPotion"/>,
        /// <see cref="PocketTreasure"/>, and <see cref="SufferCurse"/>.
        /// </summary>
        public static FightResult FightMonster(RunState s, Card c, bool useWeapon, bool noDull = false, bool loseWeapon = false)
        {
            var pv = PreviewFight(s, c, useWeapon);
            s.Block -= pv.Blocked;
            s.StrikeNext = 0; s.GritNext = 0;
            var outcome = LoseHp(s, pv.Dmg);
            if (useWeapon) ApplyDull(s, c, noDull, pv.Dmg == 0);
            if (loseWeapon) { s.Weapon = null; s.WeaponLimit = null; }
            OnSlain(s, c, useWeapon);
            RemoveFromRoom(s, c);
            AfterResolve(s);
            return new FightResult(pv.Dmg, outcome);
        }

        // ---------- room card resolution ----------

        public static void RemoveFromRoom(RunState s, Card c)
        {
            int i = s.Room.FindIndex(x => x.Equals(c));
            if (i < 0) throw new InvalidOperationException($"{c.Kind} is not in the room.");
            s.Room.RemoveAt(i);
            s.Discard.Add(c);
            s.ActedThisRoom++;
        }

        public static void AfterResolve(RunState s)
        {
            if (s.SnipePending)
            {
                s.SnipePending = false;
                s.Mode = InputMode.None;
                if (s.Hp > 0)
                {
                    s.Deck.AddRange(s.Room); // banish the rest of the room to the bottom
                    s.Room.Clear();
                    if (s.Deck.Count > 0) DealRoom(s, false); else StartBoss(s);
                    return;
                }
                // hp == 0: fall through to the standard end-of-resolve handling below.
            }
            if (s.Hp == 0) { EndRun(s, false); return; }
            if (s.Room.Count == 1 && s.Deck.Count > 0) { DealRoom(s, false); return; }
            if (s.Room.Count == 0 && s.Deck.Count == 0) { StartBoss(s); return; }
        }

        // ---------- room-card actions ----------

        public static DrinkResult DrinkPotion(RunState s, Card potion)
        {
            bool allowed = !s.PotionDrunk || s.SecondPotion;
            int n = 0;
            if (allowed)
            {
                n = Math.Min(s.MaxHp - s.Hp, potion.Rank);
                s.Hp += n;
                if (s.PotionDrunk) s.SecondPotion = false; else s.PotionDrunk = true;
            }
            RemoveFromRoom(s, potion);

            Card? burned = null;
            if (HasRelic(s, "emberheart"))
            {
                Card? best = null;
                foreach (var m in s.Room)
                    if (m.Kind == CardKind.Monster && ThreatOf(m) <= potion.Rank)
                        if (best == null || ThreatOf(m) > ThreatOf(best.Value)) best = m;
                if (best != null)
                {
                    burned = best;
                    RemoveFromRoom(s, best.Value);
                    OnSlain(s, best.Value, false);
                }
            }
            AfterResolve(s);
            return new DrinkResult(n, burned);
        }

        public static int PocketTreasure(RunState s, Card treasure)
        {
            int before = s.Gold;
            GoldGain(s, treasure.Rank);
            int gained = s.Gold - before;
            RemoveFromRoom(s, treasure);
            AfterResolve(s);
            return gained;
        }

        /// <summary>
        /// Removes the cache from the room. The player then chooses a skill
        /// draft (<see cref="GenerateSkillDraft"/> / <see cref="AddSkillFromDraft"/>)
        /// or gold (<see cref="TakeDraftGold"/>) — the caller must call
        /// <see cref="AfterResolve"/> once that choice is made.
        /// </summary>
        public static void OpenCache(RunState s, Card cache) => RemoveFromRoom(s, cache);

        public static CurseResult SufferCurse(RunState s, Card curseCard)
        {
            var outcome = HpOutcome.Ok;
            int amount = 0;
            switch (curseCard.Curse)
            {
                case CurseKind.Blight:
                    outcome = LoseHp(s, 3);
                    amount = 3;
                    break;
                case CurseKind.Thief:
                    amount = Math.Min(5, s.Gold);
                    s.Gold = Math.Max(0, s.Gold - 5);
                    break;
                case CurseKind.Rust:
                    if (s.Weapon != null)
                        s.WeaponLimit = s.WeaponLimit == null ? s.Weapon.Power : Math.Min(s.WeaponLimit.Value, s.Weapon.Power);
                    break;
            }
            RemoveFromRoom(s, curseCard);
            AfterResolve(s);
            return new CurseResult(outcome, amount);
        }

        public static bool UseBloodpact(RunState s)
        {
            if (!HasRelic(s, "bloodpact") || s.Hp <= 2) return false;
            s.Hp -= 2; s.Focus += 1;
            return true;
        }

        // ---------- boss ----------

        public static void StartBoss(RunState s)
        {
            s.Phase = RunPhase.BossIntro;
            var b = Content.Bosses[s.Depth - 1];
            s.Boss = new BossFightState
            {
                Name = b.Name, Threat = b.Threat, Art = b.Art, Special = b.Special, Text = b.Text,
                HitsLeft = b.Special == BossSpecial.Twice ? 2 : 1,
            };
        }

        public static void EnterBossFight(RunState s)
        {
            s.Phase = RunPhase.Boss;
            DrawHand(s);
            if (s.Boss.Special == BossSpecial.Lock && s.Skills.Hand.Count > 0)
                s.LockedIdx = s.Rng.NextInt(s.Skills.Hand.Count);
            s.StrikeNext = 0; s.GritNext = 0; s.Block = 0;
        }

        /// <summary>
        /// Resolves one blow against the current boss. Unlike <see cref="FightMonster"/>,
        /// this does NOT end the run on <see cref="BossOutcome.Dead"/> — the caller must
        /// call <see cref="EndRun"/>(s, false) itself when that outcome comes back
        /// (mirrors the source engine, where that's a UI-driven side effect).
        /// </summary>
        public static BossFightResult FightBoss(RunState s, bool useWeapon)
        {
            var c = Card.Monster('♠', s.Boss.Threat);
            var pv = PreviewFight(s, c, useWeapon);
            s.Block -= pv.Blocked;
            s.StrikeNext = 0; s.GritNext = 0;
            var hpOutcome = LoseHp(s, pv.Dmg);
            if (useWeapon) ApplyDull(s, c, false, pv.Dmg == 0);
            if (hpOutcome == HpOutcome.Dead)
                return new BossFightResult(pv.Dmg, hpOutcome, BossOutcome.Dead);

            s.Boss.HitsLeft--;
            if (s.Boss.HitsLeft <= 0)
            {
                s.BossesSlain++;
                GoldGain(s, 8);
                s.Slain++;
                return new BossFightResult(pv.Dmg, hpOutcome, BossOutcome.Slain);
            }
            return new BossFightResult(pv.Dmg, hpOutcome, BossOutcome.Again);
        }

        /// <summary>True if the run is won (Depth III boss just fell) — ends the run.
        /// Otherwise the caller drives a relic draft then a shop, finally calling
        /// <see cref="AdvanceToNextDepth"/>.</summary>
        public static bool TryCompleteRun(RunState s)
        {
            if (s.Depth < 3) return false;
            EndRun(s, true);
            return true;
        }

        public static void AdvanceToNextDepth(RunState s)
        {
            s.Depth++;
            s.WeaponBonus++; // the blade grows keener as you descend
            s.Deck = DeckBuilder.BuildDepthDeck(s.Depth, Seeding.DepthDeck(s.BaseSeedValue, s.Depth));
            s.Phase = RunPhase.Room;
            s.Boss = null;
            s.Room.Clear();
            DealRoom(s, false);
        }

        // ---------- scoring / end ----------

        public static ScoreBreakdown ScoreParts(RunState s) =>
            new ScoreBreakdown(s.Hp * 10, s.Gold, s.Depth * 100, s.BossesSlain * 50);

        public static void EndRun(RunState s, bool won)
        {
            if (s.Phase == RunPhase.Over) return;
            s.Phase = RunPhase.Over;
            s.Won = won;
        }
    }
}
