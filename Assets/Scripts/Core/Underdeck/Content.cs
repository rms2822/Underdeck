using System.Collections.Generic;

namespace Underdeck.Core
{
    /// <summary>
    /// All game content — the exact numbers from prototype/index.html.
    /// Keep this file in lockstep with the JS engine's content blocks.
    /// </summary>
    public static class Content
    {
        public static readonly Dictionary<int, MonsterDef> Monsters = new()
        {
            [2] = new MonsterDef { Name = "Rat", Art = "🐀" },
            [3] = new MonsterDef { Name = "Cave Bat", Art = "🦇" },
            [4] = new MonsterDef { Name = "Slime", Art = "👾" },
            [5] = new MonsterDef { Name = "Goblin", Art = "👺" },
            [6] = new MonsterDef { Name = "Skeleton", Art = "💀" },
            [7] = new MonsterDef { Name = "Bandit", Art = "🥷" },
            [8] = new MonsterDef { Name = "Dire Wolf", Art = "🐺" },
            [9] = new MonsterDef { Name = "Cultist", Art = "🧙" },
            [10] = new MonsterDef { Name = "Ogre", Art = "👹" },
            [11] = new MonsterDef { Name = "Wraith", Art = "👻" },
            [12] = new MonsterDef { Name = "Dark Knight", Art = "🤺" },
            [13] = new MonsterDef { Name = "Stone Golem", Art = "🗿" },
            [14] = new MonsterDef { Name = "Dragon", Art = "🐉" },
        };

        public static readonly Dictionary<EliteKind, EliteDef> Elites = new()
        {
            [EliteKind.Armored] = new EliteDef { Name = "Armored", Text = "your weapon counts as 2 less" },
            [EliteKind.Brutal] = new EliteDef { Name = "Brutal", Text = "deals 2 extra damage" },
        };

        public static readonly Dictionary<CurseKind, CurseDef> Curses = new()
        {
            [CurseKind.Blight] = new CurseDef { Name = "Blight", Art = "🕷️", Text = "Take 3 damage." },
            [CurseKind.Thief] = new CurseDef { Name = "Thief's Curse", Art = "🐍", Text = "Lose 5 gold." },
            [CurseKind.Rust] = new CurseDef { Name = "Rustwind", Art = "🌫️", Text = "Your blade dulls to its own power." },
        };

        public static readonly List<WeaponDef> WeaponList = new()
        {
            new WeaponDef { Id = "dagger", Name = "Rusty Dagger", Power = 4, Art = "🔪", Text = "A delver's first friend.", Price = 0 },
            new WeaponDef { Id = "rapier", Name = "Duelist's Rapier", Power = 5, Art = "🗡️", Text = "Doesn't dull when you take no damage.", Price = 8 },
            new WeaponDef { Id = "kris", Name = "Kris of Thirst", Power = 4, Art = "⚔️", Text = "Heal 1 whenever it slays.", Price = 10 },
            new WeaponDef { Id = "whisper", Name = "Whisper Blade", Power = 6, Art = "🌬️", Text = "Ignores dulling against threat 4 or less.", Price = 12 },
            new WeaponDef { Id = "hammer", Name = "Warhammer", Power = 9, Art = "🔨", Text = "Dulls two steps at a time.", Price = 14 },
        };
        public static readonly Dictionary<string, WeaponDef> Weapons = BuildLookup(WeaponList, w => w.Id);

        public static readonly List<RelicDef> RelicList = new()
        {
            new RelicDef { Id = "whetstone", Name = "Whetstone", Art = "🪨", Text = "Your blade never dulls below 4.", Price = 10 },
            new RelicDef { Id = "emberheart", Name = "Ember Heart", Art = "❤️‍🔥", Text = "Potions also slay the strongest monster they can.", Price = 12 },
            new RelicDef { Id = "boots", Name = "Coward's Boots", Art = "👢", Text = "You may flee twice in a row.", Price = 9 },
            new RelicDef { Id = "glasssigil", Name = "Glass Sigil", Art = "💎", Text = "Gold gains doubled. Max health becomes 12.", Price = 8 },
            new RelicDef { Id = "gravebind", Name = "Gravebind", Art = "⛓️", Text = "Every 5th slain monster returns as a Revenant skill.", Price = 11 },
            new RelicDef { Id = "torchbearer", Name = "Torchbearer", Art = "🕯️", Text = "The top card of the dungeon is always revealed.", Price = 9 },
            new RelicDef { Id = "bloodpact", Name = "Bloodpact", Art = "🩸", Text = "Trade 2 health for 1 focus.", Price = 10 },
            new RelicDef { Id = "phoenix", Name = "Phoenix Feather", Art = "🪶", Text = "Once per run, survive death at 1 health.", Price = 13 },
        };
        public static readonly Dictionary<string, RelicDef> Relics = BuildLookup(RelicList, r => r.Id);

        public static readonly List<SkillDef> SkillList = new()
        {
            new SkillDef { Id = "strike", Name = "Strike", Cost = 0, Art = "⚔️", Text = "+3 power on your next fight this room." },
            new SkillDef { Id = "guard", Name = "Guard", Cost = 1, Art = "🛡️", Text = "Block the next 4 damage this room." },
            new SkillDef { Id = "scout", Name = "Scout", Cost = 1, Art = "👁️", Text = "Peek the top 2 dungeon cards; you may swap them." },
            new SkillDef { Id = "slip", Name = "Slip", Cost = 0, Art = "💨", Text = "This room you may flee, even after fleeing." },
            new SkillDef { Id = "bash", Name = "Bash", Cost = 1, Art = "💥", Text = "Slay a monster of threat 6 or less." },
            new SkillDef { Id = "mend", Name = "Mend", Cost = 1, Art = "🌿", Text = "Heal 3." },
            new SkillDef { Id = "pickpocket", Name = "Pickpocket", Cost = 0, Art = "💰", Text = "Gain 4 gold." },
            new SkillDef { Id = "preserve", Name = "Preserve", Cost = 1, Art = "🧿", Text = "Your next weapon kill doesn't dull the blade." },
            new SkillDef { Id = "sharpen", Name = "Sharpen", Cost = 1, Art = "⚒️", Text = "Restore your weapon to a keen edge — undo all dulling." },
            new SkillDef { Id = "snipe", Name = "Snipe", Cost = 1, Art = "🏹", Text = "Resolve one card; banish the rest of the room to the bottom." },
            new SkillDef { Id = "knifethrow", Name = "Knife Throw", Cost = 1, Art = "🔪", Text = "Fight a monster ignoring dullness; the weapon is lost." },
            new SkillDef { Id = "grit", Name = "Grit", Cost = 1, Art = "🐲", Text = "+5 defense on your next fight this room." },
            new SkillDef { Id = "chalice", Name = "Second Chalice", Cost = 1, Art = "✨", Text = "You may drink a second potion this room." },
            new SkillDef { Id = "smokebomb", Name = "Smoke Bomb", Cost = 2, Art = "💣", Text = "Swap two room cards with the top two of the dungeon." },
            new SkillDef { Id = "necromancy", Name = "Necromancy", Cost = 2, Art = "🔮", Text = "Unearth 6 discarded cards; claim one red." },
            new SkillDef { Id = "revenant", Name = "Revenant", Cost = 1, Art = "🦴", Text = "Slay a monster of threat 8 or less." },
        };
        public static readonly Dictionary<string, SkillDef> Skills = BuildLookup(SkillList, s => s.Id);

        public static readonly List<string> StarterDeck = new()
        {
            "strike", "strike", "strike", "strike", "guard", "guard", "guard", "scout", "sharpen", "slip",
        };
        public static readonly List<string> DraftPool = new()
        {
            "bash", "mend", "pickpocket", "preserve", "sharpen", "snipe", "knifethrow",
            "grit", "chalice", "smokebomb", "necromancy", "strike", "guard",
        };

        public static readonly List<DepthDef> Depths = new()
        {
            new DepthDef { Name = "The Cellars", Numeral = "I" },
            new DepthDef { Name = "The Flooded Vaults", Numeral = "II" },
            new DepthDef { Name = "The Throat", Numeral = "III" },
        };

        public static readonly List<BossDef> Bosses = new()
        {
            new BossDef { Name = "The Gaoler", Threat = 12, Art = "⛓️", Special = BossSpecial.Lock,
                Text = "He locks one of your skills away for the fight." },
            new BossDef { Name = "The Rot King", Threat = 13, Art = "🧟", Special = BossSpecial.Twice,
                Text = "He must be struck down twice." },
            new BossDef { Name = "The Deep Tyrant", Threat = 15, Art = "🐙", Special = BossSpecial.Pierce,
                Text = "He ignores 3 of your weapon's power." },
        };

        private static Dictionary<string, T> BuildLookup<T>(List<T> list, System.Func<T, string> key)
        {
            var d = new Dictionary<string, T>();
            foreach (var item in list) d[key(item)] = item;
            return d;
        }
    }

    /// <summary>Display names/art for cards — a straight port of the JS `cardName`/`cardArt` helpers.</summary>
    public static class CardInfo
    {
        public static string Name(Card c) => c.Kind switch
        {
            CardKind.Monster => (c.Elite != EliteKind.None ? Content.Elites[c.Elite].Name + " " : "") + Content.Monsters[c.Rank].Name,
            CardKind.Potion => c.Rank <= 4 ? "Weak Potion" : c.Rank <= 7 ? "Potion" : "Greater Potion",
            CardKind.Treasure => "Treasure",
            CardKind.Cache => "Loot Cache",
            CardKind.Curse => Content.Curses[c.Curse].Name,
            _ => "?",
        };

        public static string Art(Card c) => c.Kind switch
        {
            CardKind.Monster => Content.Monsters[c.Rank].Art,
            CardKind.Potion => c.Rank <= 4 ? "🧪" : c.Rank <= 7 ? "⚗️" : "🏺",
            CardKind.Treasure => "🪙",
            CardKind.Cache => "🎁",
            CardKind.Curse => Content.Curses[c.Curse].Art,
            _ => "?",
        };
    }
}
