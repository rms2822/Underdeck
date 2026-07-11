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

## Publishing to the App Store

**📋 [`APPSTORE_SETUP.md`](APPSTORE_SETUP.md)** — the game is wrapped as a
real native iOS app (`ios/`, via Capacitor) with a GitHub Actions pipeline
(`.github/workflows/ios-release.yml`) that builds, signs, and uploads to
TestFlight/App Store Connect on a cloud macOS runner. No local Mac or Xcode
needed — trigger it from GitHub's website. The setup doc covers the Apple
Developer account and API key steps only you can do.

## What exists right now

Two parallel C# modules under `Assets/Scripts/`, both pure engine + tests
(no UnityEngine dependency), plus a working Unity UI for UNDERDECK:

```
Assets/Scripts/Core/
  (Card/Deck/GameState/Rules/RunResult.cs)  # classic-Scoundrel engine (earlier prototype)
  Underdeck/                                # UNDERDECK engine — the current game
    Content.cs         #   monsters, weapons, relics, skills, bosses — exact JS-parity data
    Models.cs           #   Card, enums, result structs
    Rng.cs               #   SplitMix64, matches the JS engine bit-for-bit
    DeckBuilder.cs       #   per-depth dungeon construction
    RunState.cs          #   the authoritative mutable run state
    GameEngine.cs(.Skills/.Shop)  # the full ruleset — combat, rooms, skills, shop, bosses
Assets/Scripts/Tests/Underdeck/   # 81 NUnit tests incl. cross-engine parity vs. the JS prototype
Assets/Scripts/Presentation/      # a complete, playable Unity UI (procedural uGUI, no scenes/art needed)
  GameBootstrap.cs      #   every screen: title, room, sheets, boss, shop, end
  UIFactory.cs           #   runtime UI-building helpers
CoreTests/                # dotnet harness — compiles and runs ALL Core+Tests headless
```

## Running the tests (no Unity needed)

```sh
dotnet test CoreTests
```

125 tests total. The Underdeck suite covers deck composition per depth,
combat math, the equal-or-lower weapon-dulling rule, all 15 skills, curses,
elites, all three bosses, the shop economy, scoring, and 100-seed
full-run bot simulations — plus a **cross-engine parity check** asserting
the C# port produces byte-identical dungeons and starter-deck shuffles to
`prototype/index.html`'s JS engine for fixed seeds.

## Building in Unity

**📋 See [`UNITY_BUILD_GUIDE.md`](UNITY_BUILD_GUIDE.md) for the full
step-by-step walkthrough** — installing Unity, opening this repo as a
project, pressing Play, and building to an Android phone (or iOS from a
Mac). Short version: open the repo in Unity Hub, add an empty GameObject
with the `GameBootstrap` component to a new scene, press Play — the entire
game builds its own UI at runtime.

## Architecture rules of the road

- **All game logic lives in `Core/`** and is mutated only through
  `GameEngine`/`Rules` methods. Presentation reads state and calls engine
  methods; it never writes state directly.
- **Determinism is sacred:** same seed ⇒ same dungeon ⇒ same outcome for the
  same inputs. Never introduce `System.Random` or frame-dependent logic into
  `Core/`.
- The web prototype (`prototype/index.html`) is the reference implementation
  for UNDERDECK — when the two disagree, treat it as a bug in whichever side
  is wrong, not an intentional divergence (three small, explicitly-documented
  rule fixes aside — see the class doc comment on `GameEngine.cs`).
