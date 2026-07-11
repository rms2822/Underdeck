using System.Collections.Generic;

namespace Underdeck.Core
{
    public static partial class GameEngine
    {
        // ---------- loot cache draft ----------

        public static List<string> GenerateSkillDraft(RunState s)
        {
            var pool = new List<string>(Content.DraftPool);
            RngUtil.Shuffle(pool, ref s.Rng);
            return pool.GetRange(0, System.Math.Min(3, pool.Count));
        }

        public static void AddSkillFromDraft(RunState s, string skillId) => s.Skills.Disc.Add(skillId);

        public static int TakeDraftGold(RunState s)
        {
            int before = s.Gold;
            GoldGain(s, 5);
            return s.Gold - before;
        }

        // ---------- relics ----------

        public static void GainRelic(RunState s, string id)
        {
            s.Relics.Add(id);
            if (id == "glasssigil") { s.MaxHp = 12; s.Hp = System.Math.Min(s.Hp, 12); }
        }

        public static List<string> GenerateRelicDraft(RunState s)
        {
            var unowned = new List<string>();
            foreach (var r in Content.RelicList) if (!HasRelic(s, r.Id)) unowned.Add(r.Id);
            RngUtil.Shuffle(unowned, ref s.Rng);
            return unowned.GetRange(0, System.Math.Min(3, unowned.Count));
        }

        // ---------- the between-depths shop ----------

        public const int TonicPrice = 6, TonicHeal = 6, ShopSkillPrice = 5;

        public static ShopStock GenerateShopStock(RunState s)
        {
            var weaponPool = new List<string>();
            foreach (var w in Content.WeaponList)
                if (w.Id != "dagger" && (s.Weapon == null || w.Id != s.Weapon.Id)) weaponPool.Add(w.Id);
            RngUtil.Shuffle(weaponPool, ref s.Rng);

            var relicPool = new List<string>();
            foreach (var r in Content.RelicList) if (!HasRelic(s, r.Id)) relicPool.Add(r.Id);
            RngUtil.Shuffle(relicPool, ref s.Rng);

            var skillPool = new List<string>(Content.DraftPool);
            RngUtil.Shuffle(skillPool, ref s.Rng);

            static ShopSlot Slot(string id) => new ShopSlot { Id = id };
            var stock = new ShopStock();
            foreach (var id in weaponPool.GetRange(0, System.Math.Min(2, weaponPool.Count))) stock.Weapons.Add(Slot(id));
            foreach (var id in relicPool.GetRange(0, System.Math.Min(2, relicPool.Count))) stock.Relics.Add(Slot(id));
            foreach (var id in skillPool.GetRange(0, System.Math.Min(2, skillPool.Count))) stock.Skills.Add(Slot(id));
            return stock;
        }

        public static bool BuyWeapon(RunState s, ShopSlot slot)
        {
            if (slot.SoldOut) return false;
            var def = Content.Weapons[slot.Id];
            if (s.Gold < def.Price) return false;
            s.Gold -= def.Price;
            s.Weapon = def; s.WeaponLimit = null;
            slot.SoldOut = true;
            return true;
        }

        public static bool BuyRelic(RunState s, ShopSlot slot)
        {
            if (slot.SoldOut) return false;
            var def = Content.Relics[slot.Id];
            if (s.Gold < def.Price) return false;
            s.Gold -= def.Price;
            GainRelic(s, slot.Id);
            slot.SoldOut = true;
            return true;
        }

        public static bool BuySkill(RunState s, ShopSlot slot)
        {
            if (slot.SoldOut) return false;
            if (s.Gold < ShopSkillPrice) return false;
            s.Gold -= ShopSkillPrice;
            s.Skills.Disc.Add(slot.Id);
            slot.SoldOut = true;
            return true;
        }

        public static bool BuyTonic(RunState s)
        {
            if (s.Gold < TonicPrice) return false;
            s.Gold -= TonicPrice;
            s.Hp = System.Math.Min(s.MaxHp, s.Hp + TonicHeal);
            return true;
        }

        public static int RemovalPrice(RunState s) => 8 + s.Removals * 4;

        public static Dictionary<string, int> RemovableSkillCounts(RunState s)
        {
            var counts = new Dictionary<string, int>();
            void Count(List<string> zone)
            {
                foreach (var id in zone) counts[id] = counts.TryGetValue(id, out var n) ? n + 1 : 1;
            }
            Count(s.Skills.Draw); Count(s.Skills.Disc); Count(s.Skills.Hand);
            return counts;
        }

        /// <summary>Removes one copy of <paramref name="id"/> — from the draw pile, then the
        /// discard, then the hand, in that priority order (matches the source engine).</summary>
        public static bool RemoveSkill(RunState s, string id)
        {
            int price = RemovalPrice(s);
            if (s.Gold < price) return false;
            foreach (var zone in new[] { s.Skills.Draw, s.Skills.Disc, s.Skills.Hand })
            {
                int i = zone.IndexOf(id);
                if (i >= 0)
                {
                    zone.RemoveAt(i);
                    s.Gold -= price;
                    s.Removals++;
                    return true;
                }
            }
            return false;
        }
    }
}
