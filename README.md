# Scroundel / UNDERDECK

A mobile roguelike deckbuilder built on the chassis of the card game
**Scoundrel** (Zach Gage & Kurt Bieg).

📋 **[`UNDERDECK_GDD.md`](UNDERDECK_GDD.md) is the master design document** —
the Scoundrel-meets-Slay-the-Spire vision: skill decks, relics, depths,
bosses, and shops layered on the room-puzzle loop. `GAME_PLAN.md` holds the
earlier production planning (architecture, monetization research, retention
systems) and remains useful background.

## Play it now

`prototype/index.html` is the **UNDERDECK vertical slice** as a self-contained
web build — open it in any browser, no server needed. It implements the GDD's
core: the 3-of-4 room loop with strictly-lower weapon dulling, a Wanderer
skill deck (hand of 3, 2 Focus per room), 8 rule-breaking relics, kit weapons
with specials, gold/treasure economy with loot-cache drafts, 3 Depths
(curses in II, elites in III) each capped by a boss, between-depth shops with
deck-thinning, GDD scoring, and a seeded Daily Descent. The Red Court class
abilities (designed on the physical deck) live on as draftable skills and the
Phoenix Feather relic.

## What exists right now

The **pure-C# rules engine + full unit-test suite** (Phase 0 of the plan):

```
Assets/Scripts/Core/        # engine — no UnityEngine dependency, seedable RNG
  Card.cs                   #   suits/ranks → Enemy / Weapon / Elixir
  Deck.cs, Rng.cs           #   canonical 44-card dungeon, deterministic shuffle
  GameState.cs              #   authoritative run state (read-only for UI)
  Rules.cs                  #   combat, degradation, rooms, flee, elixirs
  RunResult.cs              #   scoring
Assets/Scripts/Tests/Core/  # NUnit tests — run in Unity's Test Runner AND headless
CoreTests/                  # dotnet harness that compiles the same files
```

## Running the tests (no Unity needed)

```sh
dotnet test CoreTests
```

Covers deck composition, combat math, weapon degradation, room flow
("resolve 3, carry 1"), flee rules, the one-elixir-per-room rule, scoring,
and 100-seed full-game simulations that verify termination, card conservation,
and cross-run determinism (the foundation for daily challenges).

## Opening in Unity

1. Install **Unity 2022 LTS** (or newer LTS) with iOS + Android build support.
2. In Unity Hub: **Add → this repository folder**, then open. Unity generates
   `ProjectSettings/`, `Packages/`, and `.meta` files on first import —
   commit those.
3. The asmdefs are already in place: `Scroundel.Core` (engine,
   `noEngineReferences`) and `Scroundel.Core.Tests` (Editor-only, NUnit).
   Run the suite via **Window → General → Test Runner → EditMode**.
4. Set the project to **2D (URP)**, portrait orientation, per GAME_PLAN §5.1.

## Architecture rules of the road

- **All game logic lives in `Core/`** and is mutated only through `Rules`
  methods. Presentation reads `GameState` and calls `Rules`; it never writes.
- **Determinism is sacred:** same seed ⇒ same dungeon ⇒ same outcome for the
  same inputs. Never introduce `System.Random` or frame-dependent logic into
  `Core/`.
- New mechanics (GAME_PLAN §3.5) must pass the three gates: one-sentence
  explainable, visible in the combat preview, and layered as data-driven
  modifiers — never special cases inside `Rules.cs`.
