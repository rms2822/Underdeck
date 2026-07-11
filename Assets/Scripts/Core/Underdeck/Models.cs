using System;
using System.Collections.Generic;

namespace Underdeck.Core
{
    public enum CardKind { Monster, Potion, Treasure, Cache, Curse }
    public enum EliteKind { None, Armored, Brutal }
    public enum CurseKind { None, Blight, Thief, Rust }
    public enum RunPhase { Room, BossIntro, Boss, Over }
    public enum BossSpecial { Lock, Twice, Pierce }
    public enum HpOutcome { Ok, Dead, Phoenix }
    public enum BossOutcome { Dead, Slain, Again }
    public enum InputMode { None, Bash, Revenant, Knife, Snipe, Smoke }
    public enum FxTone { Blood, Verd, Steel, Bone, Gold }

    /// <summary>
    /// One dungeon card. Suit is cosmetic (both black suits are monsters,
    /// interchangeable for rules purposes) but kept for display fidelity.
    /// A cache's Rank is likewise cosmetic — opening one always offers a
    /// skill draft regardless of the printed number.
    /// </summary>
    public readonly struct Card : IEquatable<Card>
    {
        public CardKind Kind { get; }
        public char Suit { get; }
        public int Rank { get; }
        public EliteKind Elite { get; }
        public CurseKind Curse { get; }

        public Card(CardKind kind, char suit, int rank, EliteKind elite = EliteKind.None, CurseKind curse = CurseKind.None)
        {
            Kind = kind; Suit = suit; Rank = rank; Elite = elite; Curse = curse;
        }

        public static Card Monster(char suit, int rank, EliteKind elite = EliteKind.None) =>
            new Card(CardKind.Monster, suit, rank, elite);
        public static Card Potion(int rank) => new Card(CardKind.Potion, '♥', rank);
        public static Card Treasure(int rank) => new Card(CardKind.Treasure, '♦', rank);
        public static Card Cache(int rank) => new Card(CardKind.Cache, '♦', rank);
        public static Card CurseCard(CurseKind curse) => new Card(CardKind.Curse, '-', 0, EliteKind.None, curse);

        public bool Equals(Card other) =>
            Kind == other.Kind && Suit == other.Suit && Rank == other.Rank &&
            Elite == other.Elite && Curse == other.Curse;
        public override bool Equals(object obj) => obj is Card c && Equals(c);
        public override int GetHashCode() => HashCode.Combine(Kind, Suit, Rank, Elite, Curse);
        public static bool operator ==(Card a, Card b) => a.Equals(b);
        public static bool operator !=(Card a, Card b) => !a.Equals(b);
    }

    public sealed class MonsterDef { public string Name; public string Art; }
    public sealed class EliteDef { public string Name; public string Text; }
    public sealed class CurseDef { public string Name; public string Art; public string Text; }

    public sealed class WeaponDef
    {
        public string Id; public string Name; public int Power; public string Art; public string Text; public int Price;
    }
    public sealed class RelicDef
    {
        public string Id; public string Name; public string Art; public string Text; public int Price;
    }
    public sealed class SkillDef
    {
        public string Id; public string Name; public int Cost; public string Art; public string Text;
    }
    public sealed class DepthDef { public string Name; public string Numeral; }
    public sealed class BossDef
    {
        public string Name; public int Threat; public string Art; public BossSpecial Special; public string Text;
    }

    public sealed class BossFightState
    {
        public string Name; public int Threat; public string Art; public BossSpecial Special; public string Text;
        public int HitsLeft;
    }

    public readonly struct FightPreview { public readonly int Dmg, Blocked; public FightPreview(int d, int b) { Dmg = d; Blocked = b; } }
    public readonly struct FightResult { public readonly int Dmg; public readonly HpOutcome Outcome; public FightResult(int d, HpOutcome o) { Dmg = d; Outcome = o; } }
    public readonly struct BossFightResult
    {
        public readonly int Dmg; public readonly HpOutcome HpOutcome; public readonly BossOutcome BossOutcome;
        public BossFightResult(int d, HpOutcome h, BossOutcome b) { Dmg = d; HpOutcome = h; BossOutcome = b; }
    }
    public readonly struct ScoreBreakdown
    {
        public readonly int Hp, Gold, Depth, Boss, Total;
        public ScoreBreakdown(int hp, int gold, int depth, int boss)
        { Hp = hp; Gold = gold; Depth = depth; Boss = boss; Total = hp + gold + depth + boss; }
    }

    public readonly struct SkillFx
    {
        public readonly string Label; public readonly FxTone Tone;
        public SkillFx(string label, FxTone tone) { Label = label; Tone = tone; }
    }

    public readonly struct DrinkResult
    {
        public readonly int Healed; public readonly Card? Burned;
        public DrinkResult(int healed, Card? burned) { Healed = healed; Burned = burned; }
    }

    public readonly struct CurseResult
    {
        public readonly HpOutcome Outcome; public readonly int Amount;
        public CurseResult(HpOutcome outcome, int amount) { Outcome = outcome; Amount = amount; }
    }

    public sealed class ShopSlot { public string Id; public bool SoldOut; }
    public sealed class ShopStock
    {
        public List<ShopSlot> Weapons = new();
        public List<ShopSlot> Relics = new();
        public List<ShopSlot> Skills = new();
    }
}
