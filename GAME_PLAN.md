# Scroundel — Design & Development Plan

An RPG dungeon-crawler for iOS & Android, built on the core gameplay of the
single-player card game **Scoundrel** (Zach Gage & Kurt Bieg).

- **Engine:** Unity (2D, C#)
- **Scope:** Solo / indie MVP — ship a polished, replayable core loop first
- **Monetization:** Free download, rewarded ads + optional IAP (no pay-to-win)
- **Platforms:** iOS (App Store) + Android (Google Play)

---

## 1. Vision

Scoundrel is a brilliant 44-card solitaire roguelike: you descend through a
"dungeon" of cards, fighting monsters, swapping weapons that degrade as you
use them, and sipping potions to survive. It's tense, fast (a run is ~5–10
minutes), and 100% skill — no luck-based deckbuilding, just risk management.

**Scroundel** keeps that exact decision engine but dresses it as an RPG
dungeon crawl: cards become *rooms, enemies, loot, and elixirs*; the abstract
"health" becomes a *hero* with a class; the deck becomes a *procedurally
themed dungeon floor*. The mathematical purity stays; the presentation and
meta-progression give players a reason to come back.

**Pillars**
1. **One-more-run tension** — every room is a real decision, runs are short.
2. **Readable at a glance** — you always understand the board and the math.
3. **Grows with you** — unlocks and characters add depth without breaking balance.
4. **Respects the player** — monetization never sells power or interrupts a run.

---

## 2. Core mechanic mapping (Scoundrel → Scroundel)

The rules below are the *canonical Scoundrel ruleset* reskinned. **MVP ships
these rules 1:1** — do not add systems until this loop is fun.

| Scoundrel concept | Scroundel reskin |
|---|---|
| 44-card dungeon deck | A **dungeon floor** — a shuffled sequence of encounters |
| Health (start 20, cap 20) | **Hero HP** (class-dependent base, e.g. 20) |
| ♣/♠ 2–14 (Monsters) | **Enemies**, threat value = card rank |
| ♦ 2–10 (Weapons) | **Weapons / gear**, power = rank |
| ♥ 2–10 (Potions) | **Elixirs**, heal = rank |
| Room of 4 cards | An **encounter** of 4 rooms/doors |
| Resolve 3, carry 1 | Clear 3 of 4, the last carries into the next encounter |
| Avoid a room | **Flee** the encounter (send all 4 to the bottom) — not twice in a row |
| Weapon degradation | Weapon can only strike enemies **≤** the last enemy it hit |
| One potion per room heals | Only the **first** elixir per encounter heals; rest are wasted |
| Clear the deck = win | Clear the floor = **descend / floor complete** |
| HP hits 0 = lose | Hero dies = run over |

### The deck (exact composition — keep this for balance)
- Remove jokers.
- Enemies: **all clubs & spades**, 2–A → 26 cards.
- Weapons: **diamonds 2–10** → 9 cards.
- Elixirs: **hearts 2–10** → 9 cards.
- Remove red face cards & red aces. **Total: 44 cards.**

Rank values: 2–10 face value, **J=11, Q=12, K=13, A=14** (enemies only).

### Combat math
- **Barehanded:** take full enemy value as damage.
- **With weapon:** damage taken = `max(0, enemyValue − weaponPower)`. The
  weapon "absorbs" its power in damage.
- **Degradation:** after a weapon hits an enemy of value `N`, it may only be
  used on enemies of value `≤ N` thereafter. Equipping a new weapon resets this.
- Choosing barehanded vs weapon (to preserve a weapon's threshold) is the
  central tactical decision — preserve this exactly.

> **Design rule:** The MVP is a faithful, great-feeling digital Scoundrel with
> RPG skin. Everything in Sections 5–7 is layered *on top* only after the base
> loop is proven fun in playtests.

---

## 3. RPG layer (post-MVP depth)

Introduce these **one at a time**, playtesting balance after each. None are
required for the first playable/soft-launch build.

### 3.1 Hero classes (replaces "one abstract player")
Each class = a small twist on the base rules, giving replay variety.

| Class | Base HP | Passive twist |
|---|---|---|
| **Knight** | 20 | Baseline / tutorial class. Balanced. |
| **Rogue** | 16 | May flee even after fleeing last encounter (once per floor). |
| **Berserker** | 24 | Ignores 2 damage when fighting barehanded — weapons feel optional (risk/reward). |
| **Cleric** | 18 | First elixir each encounter heals +2 (rounding tension differently). |

Keep twists *small and math-legible*. Unlock classes via meta-progression.

### 3.2 Enemy identity (reskin of card ranks)
Rank bands get creatures + light flavor. **No new mechanics in MVP** — purely
visual so the player reads "big number = scary."

- 2–5: rats, goblins, slimes
- 6–9: skeletons, bandits, wolves
- 10–J: ogres, wraiths
- Q–K: knights, golems
- A (14): **floor boss** — the deck's apex threat

*(Optional later: elite variants with a single, readable modifier such as
"armored: −1 to weapon effectiveness.")*

### 3.3 Weapons & elixirs
- MVP: pure numeric gear/potions with themed art (rusty dagger → 2, greatsword → 10).
- Later: **weapon traits** (e.g. "Serrated: ignores degradation once per floor").
  Add sparingly — each trait must be explainable in one sentence.

### 3.4 Meta-progression (the retention engine)
Runs are short and often lost — give permanence between runs.
- **Gold / relics** earned per run (based on depth + remaining HP).
- **Unlocks:** classes, cosmetic themes, starting-perk relics.
- **Daily Challenge:** everyone plays the same seeded floor; leaderboard.
- **Ascension / depth tiers:** deeper floors raise stakes (multi-floor runs).

> Meta-progression should add *content and goals*, not raw power that trivializes
> the core math. Prefer sideways unlocks (new classes/challenges) over vertical
> power creep.

### 3.5 Retention & replayability upgrade pack

Scoundrel's fixed 44-card deck is its elegance **and** its replayability
ceiling — every run has the same texture. The systems below attack that
directly. Each is tagged with build cost (**S**mall / **M**edium) and the loop
it feeds: *in-run* systems make every run feel different (replayability);
*between-run* systems give a reason to come back tomorrow (retention).

**In-run (replayability)**

1. **Descend or Retreat — S, highest value.** After clearing a floor, choose:
   *bank* your gold and end the run safely, or *descend* — deeper floors
   multiply rewards (×1.5 / ×2 / ×3...) but dying forfeits most unbanked gold.
   Converts every win into a fresh push-your-luck decision, creates natural
   "one more floor" tension, and gives the rewarded-ad second wind its
   highest-stakes moment. Cheapest system on this list; build it first.
2. **Floor themes (deck mutators) — M.** Each descent offers **two doors**
   with visible modifiers — *Crypt:* +2 high enemies, −2 elixirs; *Armory:*
   +2 weapons, all weapons −1 power; *Fungal Garden:* +2 elixirs, elixirs
   heal −1. Same core math, different textures — and because modifiers are
   telegraphed at the door, it's strategy, not a slot machine.
3. **Relic drafts — M.** Draft 1-of-3 relics after each floor, **hard cap 3
   active**. Every relic is one sentence and math-legible: *Whetstone* (once
   per floor, reset weapon degradation), *Lucky Coin* (first flee each floor
   is always allowed), *Scrying Orb* (see the next card of the carried-over
   room), *Iron Flask* (second elixir per encounter heals half). Relics are
   the "build variety" engine the fixed deck otherwise lacks.
4. **Special encounter cards — M.** Inject 2–3 uncommon cards per floor:
   *Merchant* (spend run-gold on healing or weapon repair), *Shrine*
   (sacrifice HP to bless your weapon), *Cursed Chest* (big elixir or a mimic
   elite — reveal to find out). Keep injections rare so base math dominates.
5. **Elite affixes — S.** One-word modifiers on a few deep-run enemies, max
   3 affixes at launch: *Armored* (your weapon counts as 1 less), *Thorned*
   (always deals at least 1 damage), *Hexed* (defeating it locks healing for
   the rest of the encounter). Each must render exactly in the combat preview.

**Between-run (retention)**

6. **Quest board — S.** 3 dailies + 1 weekly contract ("defeat 5 enemies
   barehanded", "clear a floor without fleeing", "finish with an unbroken
   weapon"). Quests deliberately push *off-meta* play — they teach depth while
   paying out gold/cosmetic currency.
7. **Daily Challenge streaks — S.** Same seeded floor for everyone + a streak
   calendar; a streak-saver token is earnable in play or via rewarded ad
   (clean monetization synergy). Friends leaderboard first, global later.
8. **Class mastery — M.** Per-class XP levels with cosmetic unlocks and
   challenge badges — never raw power. "Win with every class" is the long-arc
   goal that makes class unlocks matter.
9. **Bestiary / codex — S.** Defeat-count collection: art + one line of lore
   per creature, completion % surfaced. Cheap content that makes the RPG
   reskin itself do retention work.
10. **Weekly Gauntlet — M.** Fixed-seed 3-floor run, preset class + relics,
    one attempt per day, leaderboard resets weekly. The competitive skeleton
    for the endgame crowd, and a reason lapsed players get pinged back.
11. **Ascension ladder — S.** Concrete tiers unlocked by winning, one line
    each: A1 start at 18 HP · A2 elixirs heal −2 · A3 fleeing shuffles the
    room instead of bottoming it · A4 face-card enemies +1 value. Vertical
    *difficulty*, never vertical power.

> **Three gates for every new mechanic:** (1) explainable in one sentence,
> (2) its exact numeric effect shows in the combat preview, (3) implemented as
> a data-driven modifier layered over the pure-C# engine — never a special
> case inside `Rules.cs`. If a mechanic fails a gate, cut it.

**Suggested order:** Descend-or-Retreat → Quest board + streaks + bestiary
(the cheap retention trio) → Relics → Floor themes → Gauntlet → Elites/Ascension.

---

## 4. Screens & UX flow

```
Splash → Main Menu ──► Play (class select) ──► Run (Encounter loop) ──► Result
   │                                                     │
   ├─► Daily Challenge                                   └─► Death / Floor clear
   ├─► Collection / Unlocks (meta)                       └─► Rewards → back to Menu
   ├─► Settings (audio, haptics, accessibility)
   └─► Store (cosmetics / IAP)
```

**The run screen is 90% of the game — invest here.**
- Top: hero HP bar, equipped weapon + its degradation threshold, floor depth.
- Center: the 4 encounter cards, tappable, with clear affordances.
- On tapping an enemy: show a **combat preview** ("Fight barehanded: −12 HP" vs
  "Use Greatsword: −4 HP, weapon locks to ≤12"). *This is the game's clarity moat.*
- Bottom: **Flee** button (disabled/greyed with reason when unavailable).
- Feedback: damage numbers, screen shake on big hits, weapon-shatter cue when a
  weapon becomes useless.

**Accessibility & feel**
- Colorblind-safe suit/type indicators (icon + shape, not just color).
- Haptics on hit/heal/flee. One-handed portrait layout. Undo-last-tap optional
  (design decision — undo reduces tension; consider "confirm on lethal only").

---

## 5. Technical architecture (Unity)

### 5.1 Project setup
- Unity **LTS** (e.g. 2022 LTS / latest LTS at start), **2D URP** template.
- Target: iOS (IL2CPP, ARM64), Android (IL2CPP, ARM64 + ARMv7 as needed).
- Version control: Git + **Git LFS** for art/audio. `.gitignore` for Unity.
- Portrait orientation, multiple aspect ratios (safe-area handling for notches).

### 5.2 Code structure (keep gameplay engine-agnostic)
Separate **pure C# game logic** from Unity/MonoBehaviour presentation so the
rules are unit-testable and could be ported later.

```
Assets/
  Scripts/
    Core/            # Pure C#, no UnityEngine dependency
      Deck.cs        # deck build, shuffle (seedable RNG)
      Card.cs        # suit, rank, category (Enemy/Weapon/Elixir)
      GameState.cs   # HP, weapon, threshold, flee-eligibility, room state
      Rules.cs       # combat resolution, degradation, potion-per-room logic
      RunResult.cs   # scoring
    Meta/            # unlocks, currency, save data
    Presentation/    # MonoBehaviours, UI, animation, input
      RunView, CardView, HeroHUD, CombatPreview, FleeButton
    Services/        # Save, Audio, Analytics, Ads, IAP (interfaces + impls)
    Tests/           # EditMode unit tests over Core/
```

**Why:** The core ruleset is small and deterministic. A seedable RNG + pure
logic means: reproducible daily challenges, trivial unit tests, and confidence
that balance changes don't break rules.

### 5.3 State model
- Single authoritative `GameState` mutated only through `Rules` methods.
- UI subscribes to state-change events (observer pattern) — no game logic in views.
- **Deterministic seed** per run → enables Daily Challenge + bug reproduction.

### 5.4 Save & data
- Local save (JSON, encrypted-at-rest for tamper resistance) for meta-progression.
- Cloud save later (iCloud / Google Play Games) — MVP can be local-only.
- Content (classes, enemy flavor, cosmetics) as **ScriptableObjects / JSON** so
  balance/flavor is data-driven, not hardcoded.

### 5.5 Third-party SDKs
- **Ads:** Unity LevelPlay / AdMob (rewarded + interstitial only). Mediation later.
- **IAP:** Unity IAP (cross-platform receipt validation).
- **Analytics:** Unity Analytics or GameAnalytics (funnel + balance telemetry).
- **Crash reporting:** Unity Cloud Diagnostics / Firebase Crashlytics.

---

## 6. Monetization design (free + ads + IAP, non-predatory)

**Principle: sell time, cosmetics, and convenience — never in-run power.**

- **Rewarded ads (opt-in only):**
  - "Second wind" — one revive per run at low HP (offered *at death*, player choice).
  - Double end-of-run gold.
  - Extra Daily Challenge attempt.
- **Interstitials:** sparingly, e.g. every N runs, never mid-run, always after the
  result screen. Frequency-capped.
- **IAP:**
  - **Remove ads** (one-time, high value — respect players who pay).
  - **Cosmetic packs:** card backs, board themes, hero skins, VFX.
  - **Unlock-all classes** convenience bundle (also earnable free via play).
- **Never:** paid stat boosts, energy/lives gating the core loop, loot boxes with power.

Instrument ad-view vs churn carefully — the "second wind at death" offer is the
retention/revenue workhorse and must feel generous, not coercive.

---

## 7. Art & audio direction (solo-friendly)

- **Style:** clean 2D, high-contrast, "premium minimalist dungeon" — readable
  cards first, flavor second. Achievable solo with asset packs + light custom work.
- **Sources:** Unity Asset Store / itch.io packs for enemies & UI; commission
  key art (icon, hero) if budget allows.
- **Audio:** licensed loops (calm dungeon ambience) + crisp SFX (hit, heal,
  shatter, flee). Audio "juice" massively raises perceived quality for low cost.
- **Juice budget:** card flips, damage pops, HP-bar tween, weapon-shatter, subtle
  parallax. This is where a solo dev wins — mechanics are done, polish sells it.

---

## 8. Development roadmap (milestone-based)

Estimates assume ~solo, part-to-full-time. Adjust to your pace.

### Phase 0 — Prototype the loop (≈2–3 weeks)
- Unity project, 2D setup, Git + LFS.
- Implement `Core/` rules **with unit tests** — deck, combat, degradation, flee,
  potion-per-room, win/lose. **No art.** Ugly buttons + text are fine.
- Playtest: *is the base Scoundrel loop fun on a phone?* **Gate: if no, stop and fix.**

### Phase 1 — Playable MVP (≈4–6 weeks)
- Real run UI: card views, HP HUD, combat preview, flee button, result screen.
- Basic art pass (placeholder→decent), core SFX, haptics.
- One class (Knight), single-floor runs, local best-score save.
- Onboarding/tutorial for first run. Settings (audio/haptics/colorblind).

### Phase 2 — Depth & retention (≈4–6 weeks)
- Meta-progression: gold, unlocks, 2–3 more classes.
- Daily Challenge (seeded) + local/simple leaderboard.
- **Retention pack (small items from §3.5):** Descend-or-Retreat, quest board,
  daily streaks, bestiary.
- Enemy/weapon/elixir art & flavor bands. Juice pass.

### Phase 3 — Monetization & services (≈2–4 weeks)
- Integrate Ads (rewarded second-wind, opt-in), IAP (remove-ads, cosmetics).
- Analytics + crash reporting. Balance telemetry dashboards.
- Cloud save (optional).

### Phase 4 — Soft launch & polish (≈3–4 weeks)
- TestFlight (iOS) + Google Play internal/closed testing.
- Tune balance from telemetry (win rate, run length, churn points).
- Store assets: icon, screenshots, trailer, descriptions, keywords (ASO).

### Phase 5 — Launch & live-ops (ongoing)
- Global release. Monitor crashes, reviews, funnels.
- Cadence of light content: new classes, weekly challenge themes, cosmetics.
- **Replayability drops (§3.5, medium items):** relic packs, floor themes,
  elite affixes, Weekly Gauntlet seasons — one system per update, playtested.

---

## 9. Publishing checklist

**Apple App Store**
- Apple Developer Program ($99/yr). App Store Connect record.
- Privacy nutrition labels, ATT prompt if ads track, age rating.
- TestFlight for beta. Review guideline compliance (ads, IAP restore).

**Google Play**
- Play Console ($25 one-time). Data safety form, content rating (IARC).
- Internal → closed → open testing tracks. Target API level compliance.
- AAB build, Play App Signing.

**Both**
- Privacy policy + terms (required with ads/IAP/analytics).
- "Restore purchases" and "remove ads" must work reliably (review blockers).
- Localize at least store listing (ASO) even if game is text-light.

---

## 10. Testing & quality

- **Unit tests** on `Core/` rules (highest ROI — the game *is* the ruleset).
- **Deterministic replay** via seeds for bug repro and balance regression tests.
- Device testing across a few low/mid/high phones (perf, aspect ratios, notches).
- Balance telemetry: win rate by class, avg depth, death causes, ad-offer accept rate.

**Retention KPIs (what "working" looks like)**
- **D1 ≥ 35%, D7 ≥ 12%, D30 ≥ 5%** — mid-core mobile roguelike benchmarks.
- Median session ≥ 2 runs; median run 5–10 min (protect this — it's the hook).
- Daily Challenge participation ≥ 25% of DAU; watch streak-length distribution.
- **Descend take-rate 40–60%** — if players always (or never) descend, the
  multipliers are mistuned; the choice must stay genuinely hard.
- Second-wind ad accept ≥ 30% without review-score damage.

---

## 11. Risks & mitigations

| Risk | Mitigation |
|---|---|
| Core loop isn't fun on mobile | Prototype-first (Phase 0 gate) before any art spend. |
| Scope creep from RPG features | Ship faithful Scoundrel MVP; layer depth one system at a time. |
| Balance broken by unlocks | Prefer sideways (classes/challenges) over vertical power. |
| Discoverability (crowded stores) | Daily Challenge + leaderboards for retention; ASO + trailer; devlog/community. |
| Monetization feels predatory → bad reviews | Opt-in rewarded ads, cosmetics-only IAP, generous "remove ads." |
| Legal/IP | Rules aren't copyrightable, but don't copy Scoundrel's exact name/art/text. "Scroundel" + original theme = safe; credit inspiration. |
| Solo burnout | Milestone gates, playable at every phase, cut features not fun. |

---

## 12. Immediate next steps

1. **Set up the Unity project** (2D URP, iOS+Android build targets, Git + LFS, `.gitignore`).
2. **Build `Core/` rules with unit tests** — the deterministic engine described
   in §2 and §5. This is buildable *today* and independent of art/engine polish.
3. **Grey-box run UI** to play it on a device and validate the fun (Phase 0 gate).

> The single highest-leverage first task is the pure-C# rules engine + tests.
> It's small, it's the heart of the game, and getting it exactly right (especially
> weapon degradation and the "3-of-4 carry" flow) makes everything after it easier.

---

*Reference: original game is **Scoundrel** by Zach Gage & Kurt Bieg. Scroundel
is an original RPG reinterpretation built on its public game rules.*
