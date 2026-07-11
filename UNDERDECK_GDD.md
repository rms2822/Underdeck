# UNDERDECK — Game Design Document

*Working title. Alternates: **Undercard**, **Grave Hand**, **The 44**, **Delve & Deal**.*

A single-player roguelike deckbuilder built on Scoundrel's chassis. The **dungeon is a deck of cards that wants you dead**; your **kit is a deck you build** to survive it. Short runs, permadeath, deep build variety, "one more run" meta-progression. Premium, one-time purchase, iPhone-first.

---

## 1. The one-sentence pitch

*Scoundrel meets Slay the Spire: descend through a dungeon dealt as cards, resolving rooms one decision at a time, while drafting weapons, spells, and relics that rewrite the rules of the dungeon itself.*

---

## 2. The core fantasy

You are a **Delver**. The dungeon deck is the enemy — every card is a threat, a temptation, or a trap. You don't get stronger by grinding stats; you get stronger by **assembling a machine of rules-breaking cards and relics** and then watching the dungeon's own logic collapse under a build you engineered. The joy is the same as Balatro's: the run where your kit "comes online" and you start doing absurd things the base rules never intended.

---

## 3. Moment-to-moment loop — the Room

This is the Scoundrel skeleton, kept deliberately intact because it's elegant and readable.

**The Dungeon** is a face-down deck. Reveal a **Room** = 4 cards face-up. Card meaning by suit:

| Suit | Type | Effect |
|------|------|--------|
| ♠ / ♣ | **Monster** | Has a **Threat** value (2–14). Deals that much damage unless mitigated. |
| ♥ | **Potion** | Heals its value. One potion per room by default. |
| ♦ | **Treasure** | Gold equal to value; face cards are loot caches (relic/card drafts). |

**Resolving a room:** you face **3 of the 4 cards** in an order you choose, then **carry the 4th over** into the next room (Scoundrel's signature tension — you're always shaping what you drag forward). 

**Fleeing:** instead of facing a room, shuffle all 4 back into the dungeon and deal a fresh room. **You cannot flee two rooms in a row.** (Kept from Scoundrel — it's the rule that makes the whole thing a puzzle.)

**Fighting a monster (base rules):**
- **With your equipped Weapon:** you take damage = `Monster Threat − Weapon Power` (min 0).
- **Barehanded:** you take the full Threat.
- **The dulling rule (Scoundrel's core constraint):** after a weapon kills a monster, it can only be used on monsters of *strictly lower* Threat than the last one it killed. Break this rule and you fight barehanded.

That dulling rule is the pressure the *entire engine* is built to subvert. Every strong build is, in some way, an answer to "how do I stop my weapon from dulling / stop caring that it does."

**Player state:** HP (start 20, max 20), equipped Weapon (Power + special), Gold, a hand of Skill cards, and Relics (passives).

---

## 4. The engine — how you build a deck

This is the layer Scoundrel doesn't have, and it's where the money is.

### 4.1 The Delver Deck (your Skill cards)
You carry a **Delver Deck** of Skill cards. Each **room**, you draw a **hand of 3** and have **2 Focus** to spend. Cards cost 0–2 Focus. Unused cards discard; reshuffle when the deck runs out (Slay-the-Spire draw loop). Skills augment or replace the base combat: block damage, kill outright, retrigger, manipulate the room, dig for treasure.

Starting deck (the "Wanderer" class), 10 cards:
- 4× **Strike** (0 Focus) — this fight, +3 Weapon Power.
- 3× **Guard** (1 Focus) — prevent the next 4 damage this room.
- 2× **Scout** (1 Focus) — peek the top 2 dungeon cards; you may reorder them.
- 1× **Slip** (0 Focus) — this room only, you may flee even if you fled last room.

### 4.2 Weapons
Weapons live in your kit, not the dungeon (a clean break from Scoundrel that lets you *build* around a weapon). Sample:

| Weapon | Power | Special |
|--------|-------|---------|
| Rusty Dagger | 3 | Starting weapon. |
| Duelist's Rapier | 5 | Does not dull if you kill in a single hit. |
| Warhammer | 9 | Dulls by 2 steps instead of 1 (big but brittle). |
| Kris of Thirst | 4 | Heal 1 HP per monster slain. |
| Whisper Blade | 6 | Ignores the dulling rule against Threat ≤ 4. |

### 4.3 Relics (the build-defining passives)
Relics are your Jokers. They're the "rewrite the rules" pieces — the thing players screenshot. Sample set showing the design space:

| Relic | Effect | Enables the archetype |
|-------|--------|----------------------|
| **Whetstone** | Your weapon never dulls below Power 4. | Weapon / Duelist |
| **Ember Heart** | ♥ potions also deal their value to one monster in the room. | Burn / aggro |
| **Coward's Boots** | You may flee two rooms in a row. | Evasion / hoarder |
| **Glass Sigil** | +100% gold, but max HP is 12. | Greed / glass cannon |
| **Gravebind** | Every 5th monster slain returns as a Skill card you can play. | Death-engine |
| **Torchbearer** | Always see the next room before choosing to flee. | Control / planning |
| **Bloodpact** | Spend HP as Focus (2 HP = 1 Focus). | Spellcaster / combo |

The design rule for relics: each one should make a player say *"oh, now I want to draft completely differently."* Aim for 40+ at launch, with clear synergy clusters (Weapon, Spell, Lifesteal/Blood, Greed, Death/Undead, Evasion).

### 4.4 How the deck grows during a run
- **After clearing a room** with a loot card: draft 1 of 3 Skill cards (or skip for gold).
- **At Shops** (floor end): buy Skills, Relics, Weapons, Potions; pay to **remove** a card (deck-thinning — the most important lever in any deckbuilder; keep it available but priced).
- **After a Boss:** guaranteed Relic choice (1 of 3) + a rare card.

---

## 5. Run structure

A **Descent** = 3 **Depths**, each ~3 floors + a Depth Boss. Each Depth reshuffles a larger, nastier dungeon and introduces a new mechanic.

| Depth | Theme | New mechanic introduced |
|-------|-------|-------------------------|
| I — The Cellars | rats, thieves, low threat | Learn the base loop. |
| II — The Flooded Vaults | curses & hazards | **Curse cards** shuffled into the dungeon (negative effects you must play around). |
| III — The Throat | elites & the deep | **Elite monsters** with special abilities + escalating Threat. |

**Bosses** are unique encounter cards with rule-bending fights, e.g.:
- **The Gaoler** — locks 1 random Skill card each room (can't be drawn). Reward: a key relic.
- **The Rot King** — heals unless you defeat him in one turn. Reward: powerful weapon.

**Win:** clear the Depth III boss. **Lose:** HP hits 0. Permadeath — the run ends, meta-progression persists.

**Scoring** (extends Scoundrel's "HP = score"): `Score = remaining HP × 10 + gold + (depth reached × 100) + boss bonuses`. Score feeds leaderboards and unlock thresholds.

---

## 6. Economy & tension

- **Gold** is earned from ♦ treasure, slain monsters, and clean room clears; spent at shops.
- The central tension is **greed vs. survival**: bank gold to power-spike at the next shop, or spend HP-saving potions now. Relics like *Glass Sigil* push this to the extreme.
- **Deck-thinning cost** rises each time you remove a card in a run, so you can't trivially perfect your deck.

---

## 7. Meta-progression — the "one more run" engine

This is what turns a good game into a sticky one. Directly modeled on what retains players in Balatro and Slay the Spire.

1. **Delver Classes** — distinct starting decks + a signature relic + a unique mechanic. Ship 3, plan more:
   - **Wanderer** — balanced, the tutorial class.
   - **Reaver** — lifesteal/blood; pays HP for power, heals on kills.
   - **Hexer** — spell-focused; weak weapon, strong Skill deck and Focus economy.
2. **Unlock ladder** — new cards, relics, weapons, and classes unlocked via milestones ("slay 100 monsters," "win with the Reaver," "reach Depth III without a potion"). Every run makes progress toward *something*.
3. **Ascension ladder** (StS-style) — beat the game to unlock harder modifiers (tougher dulling, pricier shops, elite-dense dungeons). This is the hardcore long-tail retention.
4. **Daily Run** — one fixed seed per day, global leaderboard. Cheap to build, huge for retention, streaks, and word-of-mouth. **Priority feature — do not cut it.**
5. **Codex / Collection** — every monster, relic, and card gets an entry you unlock by encountering it. Completionist hook + a natural place for art to shine.

---

## 8. Why this isn't just another clone

- Most Slay-the-Spire-likes are **linear fight → fight → fight**. Underdeck keeps Scoundrel's **spatial room puzzle** (resolve 3 of 4, carry 1, no double-flee), which almost no deckbuilder has. That's the mechanical fingerprint.
- Most Scoundrel clones have **zero build customization**. Underdeck adds a full engine.
- The fusion is the moat. Lean into a **strong, cohesive art identity** (your stated strength) — a single illustrated deck with a distinct hand-drawn style does more for differentiation than any mechanic.

---

## 9. Monetization

- **Primary: premium, one-time ~$5–7.** This is the proven model — both Balatro and Slay the Spire are premium and print money on mobile. It protects the "clean, respectful" feel that this audience pays for.
- **Optional funnel:** free download with Depth I fully playable, one IAP (~$5) to unlock the full Descent + classes + daily. This lowers install friction on the App Store while keeping premium economics. Tradeoff: more work, and you must make Depth I genuinely satisfying alone.
- **Do not** add ads, energy timers, or gacha. They'd repel the exact players who buy this genre and would erode review scores.
- **Post-launch:** paid content packs (new classes/Depths) rather than microtransactions.

---

## 10. UX notes (iPhone-first)

- **One-handed, portrait.** A room is 4 cards; tap a card to see options, tap an action to resolve. Your hand of Skills sits along the bottom.
- **Everything on one screen.** No menus mid-room. HP, Focus, weapon, gold always visible.
- **Undo-until-committed** for the current card's targeting (mispresses kill mobile roguelikes — this was a real early complaint about Balatro on mobile).
- **Fast runs.** Target 12–20 minutes per run. Snappy animations, skip-holds, no forced waits.
- **Juice the payoff.** When a build combos, make it *feel* huge — screen shake, sound, escalating number pops. That feedback loop is 50% of why Balatro is addictive.

---

## 11. Scope — a realistic staged plan

You're one person. Build in slices; validate fun before content.

**Phase 0 — Paper prototype (days).** Play it with a real deck of cards + index-card relics. If the loop isn't fun on paper, no engine will save it. Tune the dulling rule and Focus economy here for free.

**Phase 1 — Digital MVP / "vertical slice" (weeks).** One Depth, one class (Wanderer), ~20 Skill cards, ~12 relics, ~15 monsters, 1 boss, 1 shop, core room loop. Goal: is the *build variety* fun across 5–6 runs? Playtest with strangers.

**Phase 2 — v1 / launch (months).** 3 Depths, 3 classes, ~60 cards, ~40 relics, ~40 monsters, 3 bosses (+ variants), full economy, Daily Run, Ascension ladder, Codex, settings/accessibility. This is the shippable App Store product.

**Phase 3 — post-launch.** New classes, weekly modifiers, events, more relics. Content drops keep the game in the charts and give press hooks.

Cut list if time-boxed: extra classes, Ascension beyond level 5, Codex art polish. **Never cut:** the core room loop's game-feel, the Daily Run, deck-thinning.

---

## 12. Tech recommendation

- **Engine:** **Godot 4** (free, excellent 2D, one-click iOS export, no revenue share) or **Unity** (mature, huge asset ecosystem, what many deckbuilders use). For a card game that's 100% 2D, either is fine; Godot keeps costs at zero. *(Balatro itself was built in LÖVE/Lua — a tiny 2D framework — proof you don't need a heavy engine.)*
- **Native alternative:** SwiftUI + SpriteKit gives the best iOS integration and App Store fit, but slower to reach "game juice." Reasonable if you're already a Swift developer.
- **Data-driven content.** Define cards/relics/monsters in JSON or a spreadsheet, not code. You'll be balancing constantly; you want to tweak a number without recompiling.
- **Seeded RNG** from day one (required for Daily Run + reproducible bug reports).

---

## 13. First three things to build this week

1. The **room resolver**: deal 4 cards, face 3, carry 1, no double-flee, weapon + dulling math. Text-only is fine.
2. A **hand of 3 Skills / 2 Focus** draw loop layered on top.
3. **Five relics** that each break a different rule. Play 10 runs. If you feel the pull to start again — you have a game. If not, tune the loop before drawing a single piece of art.

---

*Design targets to protect throughout: runs stay short, the dulling rule stays central, builds should "come online" and feel broken, and the payoff should feel loud. Everything else is negotiable.*
