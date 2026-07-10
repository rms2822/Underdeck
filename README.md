# Scroundel

A mobile RPG dungeon-crawler (iOS + Android, Unity) built on the core gameplay
of the card game **Scoundrel** by Zach Gage & Kurt Bieg.

📋 **Read [`GAME_PLAN.md`](GAME_PLAN.md) first** — the full design & development
plan: mechanic mapping, RPG layer, retention systems, architecture,
monetization, and roadmap.

## Play it now

`prototype/index.html` is the complete game as a self-contained web build —
open it in any browser (phone or desktop, no server needed). It runs a
line-for-line JavaScript port of the C# engine, verified to produce
bit-identical dungeons and outcomes for the same seeds, and includes the
Daily Dungeon (everyone gets the same date-seeded deck) and best-score
tracking. Use it for Phase 0 playtesting while the Unity app is built.

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
