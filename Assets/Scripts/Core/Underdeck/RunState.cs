using System.Collections.Generic;

namespace Underdeck.Core
{
    public sealed class SkillZones
    {
        public List<string> Draw = new();
        public List<string> Disc = new();
        public List<string> Hand = new();
    }

    /// <summary>
    /// The authoritative state of a run. Mutated only by <see cref="GameEngine"/>
    /// — presentation code reads it and calls GameEngine methods; it never
    /// writes, mirroring the classic-Scoundrel Core's design.
    /// </summary>
    public sealed class RunState
    {
        public string Seed;
        public ulong BaseSeedValue;
        public SplitMix64 Rng; // long-lived "ability" RNG stream

        public int Depth = 1;
        public RunPhase Phase = RunPhase.Room;
        public bool Won;
        public int RoomNo = 0;

        public int Hp = 20, MaxHp = 20, Gold = 0;
        public WeaponDef Weapon;
        public int? WeaponLimit = null;
        public int WeaponBonus = 0;

        public List<string> Relics = new();
        public bool PhoenixUsed = false;

        public List<Card> Deck = new();
        public List<Card> Room = new();
        public List<Card> Discard = new();

        public SkillZones Skills = new();
        public int Focus = 2;
        public int LockedIdx = -1;

        public bool PotionDrunk = false, SecondPotion = false;
        public bool FledLast = false, SlipActive = false;
        public int ActedThisRoom = 0;

        public int StrikeNext = 0, GritNext = 0, Block = 0;
        public bool PreserveNext = false;

        public int Slain = 0, GraveTick = 0, BossesSlain = 0, Removals = 0;
        public BossFightState Boss = null;

        // Targeting-mode state (a Core-side home for what the JS build kept
        // as a UI-layer `mode` variable — cleaner for a stateful presentation
        // layer to read across multiple taps).
        public InputMode Mode = InputMode.None;
        public List<Card> SmokeDrawn = new();
        public List<Card> SmokePicks = new();
        public bool SnipePending = false;
    }
}
