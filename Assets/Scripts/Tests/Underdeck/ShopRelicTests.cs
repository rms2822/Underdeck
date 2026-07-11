using System.Linq;
using NUnit.Framework;
using static Underdeck.Core.Tests.TestCards;

namespace Underdeck.Core.Tests
{
    public class ShopRelicTests
    {
        private static RunState Fresh(ulong seed = 1) => GameEngine.NewRun(seed.ToString());

        [Test]
        public void ShopStock_OffersTwoWeaponsTwoRelicsTwoSkills_ExcludingOwned()
        {
            var s = Fresh();
            GameEngine.GainRelic(s, "whetstone");
            var stock = GameEngine.GenerateShopStock(s);

            Assert.AreEqual(2, stock.Weapons.Count);
            Assert.AreEqual(2, stock.Relics.Count);
            Assert.AreEqual(2, stock.Skills.Count);
            Assert.IsFalse(stock.Weapons.Any(w => w.Id == "dagger"), "current weapon never re-offered");
            Assert.IsFalse(stock.Relics.Any(r => r.Id == "whetstone"), "owned relics never re-offered");
        }

        [Test]
        public void BuyWeapon_DeductsGold_Equips_AndMarksSlotSoldOut()
        {
            var s = Fresh();
            s.Gold = 20;
            var stock = GameEngine.GenerateShopStock(s);
            var slot = stock.Weapons[0];
            int price = Content.Weapons[slot.Id].Price;

            Assert.IsTrue(GameEngine.BuyWeapon(s, slot));
            Assert.AreEqual(20 - price, s.Gold);
            Assert.AreEqual(slot.Id, s.Weapon.Id);
            Assert.IsNull(s.WeaponLimit, "a bought weapon starts keen");
            Assert.IsTrue(slot.SoldOut);
            Assert.IsFalse(GameEngine.BuyWeapon(s, slot), "can't buy a sold-out slot twice");
        }

        [Test]
        public void BuyRelic_FailsWhenGoldIsShort()
        {
            var s = Fresh();
            s.Gold = 0;
            var stock = GameEngine.GenerateShopStock(s);

            Assert.IsFalse(GameEngine.BuyRelic(s, stock.Relics[0]));
            Assert.IsFalse(stock.Relics[0].SoldOut);
            Assert.AreEqual(0, s.Relics.Count);
        }

        [Test]
        public void BuyTonic_HealsSix_CappedAtMax()
        {
            var s = Fresh();
            s.Gold = 10; s.Hp = 17;
            Assert.IsTrue(GameEngine.BuyTonic(s));
            Assert.AreEqual(20, s.Hp);
            Assert.AreEqual(4, s.Gold);
        }

        [Test]
        public void RemovalPrice_RisesByFourEachTime()
        {
            var s = Fresh();
            Assert.AreEqual(8, GameEngine.RemovalPrice(s));
            s.Removals = 3;
            Assert.AreEqual(20, GameEngine.RemovalPrice(s));
        }

        [Test]
        public void RemoveSkill_PrefersDrawPile_ThenDiscard_ThenHand()
        {
            var s = Fresh();
            s.Gold = 100;
            s.Skills.Draw.Clear(); s.Skills.Disc.Clear();
            s.Skills.Draw.Add("guard");
            s.Skills.Disc.Add("guard");
            SetHand(s, "guard", "strike", "scout");

            Assert.IsTrue(GameEngine.RemoveSkill(s, "guard"));
            CollectionAssert.DoesNotContain(s.Skills.Draw, "guard");
            CollectionAssert.Contains(s.Skills.Disc, "guard");
            CollectionAssert.Contains(s.Skills.Hand, "guard");
            Assert.AreEqual(1, s.Removals);
        }

        [Test]
        public void RemoveSkill_FailsWhenGoldIsShort()
        {
            var s = Fresh();
            s.Gold = 0;
            s.Skills.Draw.Add("guard");
            Assert.IsFalse(GameEngine.RemoveSkill(s, "guard"));
            CollectionAssert.Contains(s.Skills.Draw, "guard");
        }

        [Test]
        public void SkillDraft_Offers3FromThePool_AddingOneGoesToDiscard()
        {
            var s = Fresh();
            var picks = GameEngine.GenerateSkillDraft(s);
            Assert.AreEqual(3, picks.Count);
            Assert.IsTrue(picks.All(id => Content.DraftPool.Contains(id)));

            GameEngine.AddSkillFromDraft(s, picks[0]);
            CollectionAssert.Contains(s.Skills.Disc, picks[0]);
        }

        [Test]
        public void RelicDraft_NeverOffersAnAlreadyOwnedRelic()
        {
            var s = Fresh();
            GameEngine.GainRelic(s, "boots");
            var picks = GameEngine.GenerateRelicDraft(s);
            Assert.IsFalse(picks.Contains("boots"));
        }

        [Test]
        public void GlassSigil_DoublesGoldGains_AcrossTreasureDraftAndShop()
        {
            var s = Fresh();
            GameEngine.GainRelic(s, "glasssigil");
            Assert.AreEqual(12, s.MaxHp, "max health drops to 12");

            Assert.AreEqual(10, GameEngine.TakeDraftGold(s)); // 5 base, doubled
            Assert.AreEqual(10, s.Gold);
        }
    }
}
