# Concept Decision & Core Mechanics — the Narrative Swipe Crawler

*Companion to [`GAME_PLAN.md`](GAME_PLAN.md). GAME_PLAN specs Concept #1 (the RPG
dungeon crawler, already built as the C# engine + web prototype). This document
answers the follow-up question: **of the three concepts, which is the best fit
for iOS, and what are its core mechanics?***

---

## 1. The decision

**Recommendation: Concept #2 — the narrative swipe crawler — is the best fit for
iOS.** Ship it as a distinct, art-forward product built on the *same* Scoundrel
survival math the repo already implements.

### The three candidates, scored for iOS specifically

| Criterion (weighted for iOS) | #1 RPG dungeon crawler | #2 Swipe crawler | #3 Tactical grid |
|---|---|---|---|
| **Fits iOS session shape** (one-handed, ≤2 min, on a train) | Medium — needs two-thumb precision taps + a combat-preview read | **High — one-thumb swipe, glanceable** | Medium — spatial puzzle wants focus |
| **Proven revenue at premium price** (App Store buyers) | High ceiling, but crowded | **High — Reigns: 2M+ at ~$3, a franchise** | Niche/loyal, small |
| **Time to a shippable prototype** | Already built | **Fastest of the three** | Slowest — pathing + grid gen |
| **Differentiation in a saturated store** | Low — Scoundrel-clone glut | Medium — Reigns format is recognizable but under-served with real survival math | **Highest** |
| **Leverages art / writing** | Medium | **Highest — art *is* the card, writing *is* the game** | High |
| **App Store featuring odds** (Apple loves distinctive, tactile, premium) | Medium | **High** | Medium-High |

**Why #2 wins for iOS, not just "wins":**

1. **The platform's session is a swipe.** iOS's highest-retention casual genre is
   the one-thumb, glance-and-go interaction. Reigns proved the swipe crawler is
   *native* to the phone in a way a tap-a-card-then-read-a-preview roguelike
   isn't. On iOS specifically — premium buyers, high polish bar, commuter
   sessions — that ergonomic fit converts to both retention and featuring.
2. **Proven price and franchise.** Reigns sold 2M+ at a ~$3 premium and spun out
   crossovers. That's a *demonstrated* iOS money path at a price point Apple's
   audience will actually pay, without the predatory monetization that tanks
   review scores.
3. **Fastest to a real prototype**, and the repo already has the hard part: a
   deterministic, unit-tested survival engine (see §6). We reskin the interaction,
   not rebuild the math.
4. **It's the art play.** The brief flags art as a strength. In this format the
   card art and the one-line writing on each card *are* the product surface —
   the highest return on that strength of the three.

**When to pick differently:** if the goal is the absolute *revenue ceiling* and
you're willing to compete in the crowded roguelike category, #1 (already built)
has the higher top end. If the goal is to *stand apart* above all, #3 is the most
distinct. For **"best suitable for iOS"** — ergonomics × proven premium revenue ×
ship speed × featuring odds — **#2 is the answer.**

### The unlock that makes this cheap to build

The Scoundrel decision *is already a binary*, which is exactly what a swipe game
needs. "Fight this monster barehanded (save your weapon) **or** with your weapon
(spend its durability)" maps one-to-one onto **swipe left / swipe right**. We are
not inventing new math — we are giving the repo's existing engine a thumb-native
skin. That is why #2 is both the best iOS fit *and* the cheapest to reach.

---

## 2. Core loop

> **One sentence:** Descend a dungeon one card at a time, swiping each room left
> or right, keeping four meters alive — **Health, Torch, Gold, Nerve** — until
> your torch gutters out, your heart stops, or you choose to climb back out rich.

```
        ┌─────────────────────────────────────────────┐
        │  A single card fills the screen (a room).    │
        │  Swipe LEFT  ── choice A ──►  meters shift    │
        │  Swipe RIGHT ── choice B ──►  meters shift    │
        └───────────────┬─────────────────────────────┘
                        │  every swipe: Torch −1
                        ▼
              next card is drawn from the floor
                        │
        ┌───────────────┴───────────────┐
        │  Health = 0            → death (run ends)      │
        │  Torch  = 0            → you're In The Dark      │
        │  floor cleared         → Descend or Climb Out    │
        └────────────────────────────────────────────────┘
```

A run is **5–10 minutes**, a single card is a **1–3 second** decision. The whole
game is legible with one thumb on a train — the iOS session shape from §1.

---

## 3. The four meters

Reigns juggles abstract kingdom meters. We keep four, but each is a *survival*
resource with hard, Scoundrel-honest math — this is the "survival tension married
to the Reigns format" from the brief.

| Meter | Range | What it is | Hits zero → |
|---|---|---|---|
| ❤️ **Health** | 0–20 | Classic Scoundrel HP. Damage is real and math-legible. | **Death.** Run over. |
| 🔥 **Torch** | 0–12 | The clock. **Every swipe costs 1.** Refilled by oil/torch cards. | **In The Dark** (danger spike, see §5.2) — not instant death, a pressure phase. |
| 🪙 **Gold** | 0–∞ | Score *and* spend currency. Buys at merchants, banks on Climb Out. | Nothing — it's the reward meter, never lethal. |
| 🧠 **Nerve** | 0–20 | Composure. Drops from horror, darkness, close calls. Low Nerve worsens outcomes. | **Panic** (a debuff phase, see §5.3) — not death, a spiral you can recover from. |

**Design intent:** only Health kills instantly. Torch and Nerve create *dread*
before they create death — they degrade your position, and the player feels the
walls closing in. That two-stage pressure (warn, then punish) is what makes the
swipe tense instead of arbitrary.

Meters render as four thin bars framing the card so they're readable at a glance
without leaving the swipe (colorblind-safe: icon + shape + fill, never color
alone — carried over from GAME_PLAN §4).

---

## 4. Card types & their swipes

Every card is one of five types. The card art fills the screen; a one-line label
on each edge tells you what each swipe does *before* you commit (the "clarity
moat" from GAME_PLAN §4 — a swipe game must telegraph both outcomes).

### 4.1 Monster (♣/♠, rank = threat) — the heart of the game

This is the Scoundrel weapon-preservation decision, rendered as a swipe.

| Swipe | Action | Effect |
|---|---|---|
| **← Left** | **Fight barehanded** | `Health −= rank`. Your weapon is untouched (durability preserved). |
| **→ Right** | **Fight with weapon** | `Health −= max(0, rank − weaponPower)`. **Weapon degrades:** it may now only be used on threats `≤ rank` (Scoundrel's degradation rule, GAME_PLAN §2). |

- If **Right would violate degradation** (this monster's rank exceeds the weapon's
  current threshold), the right edge shows the weapon **greyed with the reason**
  ("blade too dull for this") — swiping right then falls back to barehanded, or is
  blocked, per the confirm-on-lethal rule. This is *exactly* `Rules.cs` today.
- The card preview shows **both** numbers live: "← −12 ❤️" vs "→ −4 ❤️, blade
  locks to ≤12". No hidden math.

**Why this is the whole game:** every monster is the same beautiful tension
Scoundrel is built on — *spend your weapon's finite edge now, or take the hit raw
to save it for the boss?* — but now it's a thumb-flick, not a menu.

### 4.2 Weapon (♦ 2–10, power = rank)

| Swipe | Action | Effect |
|---|---|---|
| **← Left** | **Leave it** | Keep your current weapon and its remaining threshold. |
| **→ Right** | **Equip it** | New weapon, `power = rank`, **degradation resets to full**. Old weapon discarded. |

The core weapon puzzle survives intact: a fresh low weapon (reset threshold) vs. a
high weapon you've already dulled.

### 4.3 Elixir (♥ 2–10, heal = rank)

| Swipe | Action | Effect |
|---|---|---|
| **← Left** | **Save it / pour it out** | No heal. (Denies the "wasted overheal" trap.) |
| **→ Right** | **Drink** | `Health = min(20, Health + rank)`. |

**Diminishing-sips rule (Scoundrel's "first potion per room" reimagined):** each
successive elixir drunk *without a fight in between* heals less (rank, then rank−2,
then rank−4…). Chugging potions back-to-back is punished; you can't stall-heal
your way through. Legible and one-sentence, per the GAME_PLAN "three gates."

### 4.4 Torch / Oil (the clock resource)

| Swipe | Action | Effect |
|---|---|---|
| **← Left** | **Press on** | Skip it. `Torch` keeps ticking down. |
| **→ Right** | **Refuel** | `Torch = min(12, Torch + value)`. Sometimes costs a little Nerve or Gold (a real trade, telegraphed on the card). |

Torch is the deck-as-timer from Scoundrel made *tangible and spendable*. Skipping
a torch to push deeper on gold is a genuine push-your-luck bet.

### 4.5 Event / Room (the Reigns + writing surface)

Narrative cards — a shrine, a merchant, a beggar, a locked door, a whispering
wall. Each swipe is a **flavored trade across meters**, written in one line:

- *Merchant:* ← haggle (`Gold −5 → Health +6`) / → rob him (`Gold +8, Nerve −4`).
- *Shrine:* ← pray (`Nerve +5, Torch −2`) / → desecrate (`Gold +10, Nerve −6`).
- *Cursed chest:* ← leave it / → open (50/50: `Gold +15` **or** a mimic monster).

Events are where the **art and writing carry the game** (the strength from §1),
and where floor *theme* expresses itself. Keep them rare enough that the survival
math (monsters/weapons/elixirs) stays the spine — same discipline as GAME_PLAN
§3.5's "keep injections rare so base math dominates."

---

## 5. The three pressure systems (what makes it tense, not random)

### 5.1 The floor as a deck (structure, not chaos)

A floor is a **shuffled, seedable sequence** of ~44 cards drawn one at a time —
the repo's exact `Deck` + `Rng`. Same seed ⇒ same floor ⇒ **daily challenge and
bug-repro survive the reskin for free** (GAME_PLAN §5.3). Cards are *not* random
per-swipe; they're a known-composition deck being revealed, so a skilled player
reasons about what's left ("three big monsters unseen, and I've dulled my blade").

### 5.2 In The Dark (Torch = 0)

Not death — a **dread phase**. While the torch is out:

- Monster damage **+2** (you fight blind).
- **Nerve −1 per swipe** (the dark eats your composure).
- Card art dims to a vignette; haptics go heavy and slow.

Light a torch and you're out. This turns the clock into a *recoverable* crisis —
the player always has an out, so the pressure feels fair.

### 5.3 Panic (Nerve = 0)

Not death — a **control-loss phase**. While panicked:

- The next **two** monster cards *auto-resolve barehanded* if you hesitate
  (a visible countdown before the swipe locks) — you flinch.
- Elixir heals are halved (shaking hands).

Recover Nerve (shrines, safe rooms, a clean weapon kill) to break out. Panic is
the psychological layer the four-meter format buys us that pure Scoundrel lacks.

### 5.4 Descend or Climb Out (the money moment — reused wholesale)

Clear the floor and you choose, exactly like GAME_PLAN §3.5's flagship system:

- **Climb Out:** bank your Gold, run ends safe, score locked.
- **Descend:** next floor multiplies Gold rewards (×1.5 / ×2 / ×3…), but **dying
  forfeits most unbanked Gold** (salvage ~20%). Meters carry down; the torch does
  *not* fully refill.

This is the push-your-luck heartbeat and the honest home for the rewarded-ad
"second wind at death" offer (GAME_PLAN §6) — offered *at death*, opt-in, never
coercive.

---

## 6. Why this is cheap: it reuses the repo almost entirely

The swipe crawler is a **new interaction layer over the existing engine**, not a
new engine. Concretely:

| Repo asset today | Role in the swipe crawler | Change needed |
|---|---|---|
| `Card.cs` (suit→kind, rank=value) | Same cards, same values | **None.** |
| `Deck.cs` + `Rng.cs` (seedable 44-card floor) | The floor sequence + daily seed | **None.** |
| `Rules.cs` combat/degradation/heal | Resolves each swipe | **Reused;** wrap left/right calls onto existing methods. |
| `GameState.cs` (HP, weapon, threshold) | Add **Torch** + **Nerve** meters | **Extend** — two new ints + their tick rules, layered as data-driven modifiers (the GAME_PLAN "three gates"), *not* special cases in `Rules.cs`. |
| `prototype/index.html` | Fastest way to validate the swipe feel | Add swipe/drag input + meter bars; the JS engine port already exists. |

The genuinely new work is **input + presentation**: a drag-to-reveal card, the two
edge labels with live previews, four meter bars, and the In-The-Dark / Panic /
Descend states. That's a Phase-1-sized effort on top of a Phase-0 that's *done*.

**One-sentence-explainable gate check (GAME_PLAN §3.5):** every mechanic above —
each meter, each swipe, In The Dark, Panic, Descend — states its exact numeric
effect on the card face before you commit. Nothing hidden, nothing un-previewable.

---

## 7. Recommended build order

1. **Reskin the prototype to swipe** (`prototype/index.html`): drag input, four
   meter bars, monster left/right onto the existing engine. Validate the *feel* —
   is one-thumb Scoundrel fun? This is the Phase-0 gate, reused.
   **✅ Built as `prototype/swipe.html`** — swipe *and* tap controls, all four
   meters, In The Dark + Panic phases, event cards, Descend-or-Climb-Out, Daily
   Dungeon, and an interactive tutorial (§7.1). Playable in any browser.
2. **Add Torch as the clock** (In The Dark phase). Retune floor length for the
   ~5–10 min iOS session. **✅ in the prototype** (~39-card floor).
3. **Add Nerve + Panic.** Playtest that dread reads as tension, not noise.
   **✅ in the prototype** (flinch timer on panicked fights).
4. **Event/room cards + one floor theme** — the art & writing surface.
   **✅ Merchant / Shrine / Cursed Chest** in the prototype.
5. **Descend or Climb Out** — already spec'd and prototyped for #1; port it.
   **✅ in the prototype.**
6. **Port to Unity `Presentation/`** (GAME_PLAN §5.2) once the swipe feel is proven
   in web. Engine layer (`Core/`) crosses over unchanged.

### 7.1 What the prototype already taught us

Two findings from building `prototype/swipe.html`, both the kind of thing a
Phase-0 prototype exists to surface:

- **The swipe format has no *flee*, so the floor must be gentler than tabletop
  Scoundrel.** Scoundrel's 26-enemy deck assumes you can dodge ~a quarter of it;
  the swipe format resolves every card in draw order, so the full deck is
  unsurvivable (0/200 seeds cleared in a headless bot). The prototype uses a
  hand-tuned, enemy-lighter floor (14 enemies, richer weapons/elixirs, a
  starting Dagger) that a middling bot clears **~25%** of the time — leaving
  headroom for a skilled human and real "finish on 8 HP" tension. **Implication
  for Unity:** the swipe mode needs its own floor-composition data, distinct
  from the tap mode's canonical 44.
- **Weapon degradation stays faithful *and* interesting in forced draw order.**
  Because you can't reorder your weapon hits (no "pick 3 of 4"), degradation
  becomes a live bet — *"is this monster big enough to spend my blade's edge on,
  or do I take it barehanded and save the edge for something worse I can't see
  yet?"* That preserves Scoundrel's central decision without modification.

---

*Reference: interaction format inspired by **Reigns** (Nerial); survival math is
the repo's own Scoundrel engine (original game by Zach Gage & Kurt Bieg). This
document selects Concept #2 and specs its core loop; GAME_PLAN.md remains the
reference for the shared engine, monetization, and services.*
