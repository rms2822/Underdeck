using System;
using System.Collections.Generic;

namespace Underdeck.Core
{
    public static partial class GameEngine
    {
        private static readonly HashSet<string> NeedsTarget = new() { "bash", "revenant", "knifethrow", "snipe", "smokebomb" };

        /// <summary>Whether the hand card at <paramref name="id"/> can be played right now, and why not if it can't.</summary>
        public static (bool Usable, string Reason) SkillUsability(RunState s, string id)
        {
            var sk = Content.Skills[id];
            bool usable = s.Focus >= sk.Cost;
            string why = "not enough focus";
            if (usable)
            {
                if (id == "knifethrow" && s.Weapon == null) { usable = false; why = "you carry no weapon"; }
                if ((id == "bash" || id == "revenant") && s.Phase == RunPhase.Room)
                {
                    int max = id == "bash" ? 6 : 8;
                    bool hasTarget = false;
                    foreach (var c in s.Room) if (c.Kind == CardKind.Monster && ThreatOf(c) <= max) { hasTarget = true; break; }
                    if (!hasTarget) { usable = false; why = "no valid target in the room"; }
                }
                if (id == "smokebomb" && (s.Deck.Count < 2 || s.Room.Count < 2)) { usable = false; why = "needs 2 cards in deck and room"; }
                if (id == "necromancy" && s.Discard.Count == 0) { usable = false; why = "the discard is empty"; }
                if (id == "sharpen" && (s.Weapon == null || s.WeaponLimit == null))
                {
                    usable = false;
                    why = s.Weapon != null ? "your blade is already keen" : "you carry no weapon";
                }
                if (NeedsTarget.Contains(id) && s.Phase == RunPhase.Boss) { usable = false; why = "no use against the boss"; }
                if (id == "snipe" && s.Room.Count < 2) { usable = false; why = "nothing to banish"; }
            }
            return (usable, why);
        }

        /// <summary>Spends the hand card at <paramref name="idx"/>: deducts focus, discards it. Returns its id.</summary>
        public static string SpendSkill(RunState s, int idx)
        {
            string id = s.Skills.Hand[idx];
            s.Focus -= Content.Skills[id].Cost;
            s.Skills.Hand.RemoveAt(idx);
            if (s.LockedIdx > idx) s.LockedIdx--;
            s.Skills.Disc.Add(id);
            return id;
        }

        /// <summary>Plays a self-contained skill (no target, no sub-menu). Caller must have
        /// already confirmed <see cref="SkillUsability"/>.</summary>
        public static SkillFx PlayInstant(RunState s, int idx)
        {
            string id = SpendSkill(s, idx);
            switch (id)
            {
                case "strike": s.StrikeNext += 3; return new SkillFx("+3 ⚔", FxTone.Steel);
                case "guard": s.Block += 4; return new SkillFx("+4 🛡", FxTone.Steel);
                case "grit": s.GritNext += 5; return new SkillFx("grit", FxTone.Steel);
                case "slip": s.SlipActive = true; s.FledLast = false; return new SkillFx("slip", FxTone.Bone);
                case "mend":
                {
                    int n = Math.Min(s.MaxHp - s.Hp, 3);
                    s.Hp += n;
                    return new SkillFx("+" + n, FxTone.Verd);
                }
                case "pickpocket":
                {
                    int before = s.Gold;
                    GoldGain(s, 4);
                    return new SkillFx("+" + (s.Gold - before) + " 🪙", FxTone.Gold);
                }
                case "preserve": s.PreserveNext = true; return new SkillFx("preserved", FxTone.Steel);
                case "sharpen": s.WeaponLimit = null; return new SkillFx("keen!", FxTone.Steel);
                case "chalice": s.SecondPotion = true; return new SkillFx("chalice", FxTone.Verd);
                default: throw new InvalidOperationException($"{id} is not an instant skill.");
            }
        }

        /// <summary>Spends a targeting skill and arms <see cref="RunState.Mode"/>. For Smoke
        /// Bomb this also draws the two dungeon cards immediately, matching the source engine.</summary>
        public static void BeginTargetMode(RunState s, int idx)
        {
            string id = SpendSkill(s, idx);
            s.Mode = id switch
            {
                "bash" => InputMode.Bash,
                "revenant" => InputMode.Revenant,
                "knifethrow" => InputMode.Knife,
                "snipe" => InputMode.Snipe,
                "smokebomb" => InputMode.Smoke,
                _ => throw new InvalidOperationException($"{id} does not need a target."),
            };
            if (s.Mode == InputMode.Smoke)
            {
                int n = Math.Min(2, s.Deck.Count);
                s.SmokeDrawn = s.Deck.GetRange(0, n);
                s.Deck.RemoveRange(0, n);
                s.SmokePicks.Clear();
            }
        }

        /// <summary>Cancels an armed targeting mode. Unlike the source engine, a cancelled
        /// Smoke Bomb returns its two drawn cards to the bottom of the deck.</summary>
        public static void CancelMode(RunState s)
        {
            if (s.Mode == InputMode.Smoke && s.SmokeDrawn.Count > 0)
            {
                s.Deck.AddRange(s.SmokeDrawn);
                s.SmokeDrawn.Clear();
                s.SmokePicks.Clear();
            }
            s.Mode = InputMode.None;
        }

        /// <summary>Bash/Revenant target resolution. Returns false (mode stays armed) if the
        /// tapped card is an invalid target.</summary>
        public static bool ResolveBashRevenant(RunState s, Card card)
        {
            int max = s.Mode == InputMode.Bash ? 6 : 8;
            if (card.Kind != CardKind.Monster || ThreatOf(card) > max) return false;
            s.Mode = InputMode.None;
            RemoveFromRoom(s, card);
            OnSlain(s, card, false);
            AfterResolve(s);
            return true;
        }

        public static FightResult ResolveKnifeThrow(RunState s, Card card)
        {
            s.Mode = InputMode.None;
            int dmg = Math.Max(0, ThreatOf(card) - EffPower(s) - s.GritNext + (card.Elite == EliteKind.Armored ? 2 : 0));
            int blocked = Math.Min(s.Block, dmg);
            s.Block -= blocked;
            s.GritNext = 0;
            var outcome = LoseHp(s, dmg - blocked);
            s.Weapon = null; s.WeaponLimit = null;
            OnSlain(s, card, false);
            RemoveFromRoom(s, card);
            AfterResolve(s);
            return new FightResult(dmg - blocked, outcome);
        }

        /// <summary>Arms Snipe on a target. The caller then resolves the card exactly like a
        /// normal tap (Fight/Drink/Pocket/etc.) — <see cref="AfterResolve"/> banishes the rest
        /// of the room once that action commits. Cancelling (not resolving the card) keeps
        /// Snipe armed, matching the source engine.</summary>
        public static void ArmSnipe(RunState s, Card card) => s.SnipePending = true;

        /// <summary>Toggles a Smoke Bomb pick; performs the swap once two are chosen.
        /// Returns true when the swap completes.</summary>
        public static bool ToggleSmokePick(RunState s, Card card)
        {
            int i = s.SmokePicks.FindIndex(p => p.Equals(card));
            if (i >= 0) s.SmokePicks.RemoveAt(i);
            else if (s.SmokePicks.Count < 2) s.SmokePicks.Add(card);

            if (s.SmokePicks.Count == 2)
            {
                for (int j = 0; j < 2; j++)
                {
                    var pick = s.SmokePicks[j];
                    int idx = s.Room.FindIndex(x => x.Equals(pick));
                    s.Deck.Add(s.Room[idx]);
                    s.Room[idx] = s.SmokeDrawn[j];
                }
                s.Mode = InputMode.None;
                s.SmokeDrawn.Clear();
                s.SmokePicks.Clear();
                return true;
            }
            return false;
        }

        // ---------- Scout ----------

        public static void PlayScout(RunState s, int idx) => SpendSkill(s, idx);

        public static List<Card> PeekDeckTop(RunState s, int n = 2) => s.Deck.GetRange(0, Math.Min(n, s.Deck.Count));

        public static void SwapDeckTopTwo(RunState s)
        {
            if (s.Deck.Count < 2) return;
            (s.Deck[0], s.Deck[1]) = (s.Deck[1], s.Deck[0]);
        }

        // ---------- Necromancy ----------

        public static List<Card> PlayNecromancy(RunState s, int idx)
        {
            SpendSkill(s, idx);
            var pool = new List<Card>(s.Discard);
            RngUtil.Shuffle(pool, ref s.Rng);
            return pool.GetRange(0, Math.Min(6, pool.Count));
        }

        public static int NecromancyChoosePotion(RunState s, Card potionCard)
        {
            bool allowed = !s.PotionDrunk || s.SecondPotion;
            int n = 0;
            if (allowed)
            {
                n = Math.Min(s.MaxHp - s.Hp, potionCard.Rank);
                s.Hp += n;
                if (s.PotionDrunk) s.SecondPotion = false; else s.PotionDrunk = true;
            }
            return n;
        }

        public static int NecromancyChooseGold(RunState s, Card card)
        {
            int val = card.Kind == CardKind.Cache ? 6 : card.Rank;
            int before = s.Gold;
            GoldGain(s, val);
            return s.Gold - before;
        }
    }
}
