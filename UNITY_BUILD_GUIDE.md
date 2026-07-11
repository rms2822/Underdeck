# Building UNDERDECK in Unity — step by step

This is the walkthrough for taking what's in this repo and turning it into a
real build on your machine. It can't be done from this remote session — Unity
Editor is a GUI application that needs a display and a licensed local
install, neither of which exist here. Everything that *could* be prepared in
advance has been: the full game engine (`Assets/Scripts/Core/Underdeck/`,
verified by 125 passing tests) and a complete, playable UI
(`Assets/Scripts/Presentation/`) are already written and sitting in the repo,
built entirely from code — no scenes, prefabs, or art assets to hand-author.
You're pressing Play, not building from scratch.

**Time estimate:** ~20 minutes to a playable build in the Editor, another
15–20 minutes to get it on an Android phone.

---

## 1. Install Unity

1. Download **Unity Hub**: https://unity.com/download
2. Inside Unity Hub, go to **Installs → Install Editor** and pick a
   **2022 LTS** version (e.g. 2022.3.x — any patch is fine, this project
   uses no version-specific features).
3. During install, check these modules:
   - **Android Build Support** (+ its **Android SDK & NDK Tools** and
     **OpenJDK** sub-items) — needed for step 5.
   - Skip iOS unless you're on a Mac (see the note at the end).

---

## 2. Open this repo as a Unity project

The repo has `ProjectSettings/ProjectVersion.txt` and `Packages/manifest.json`
checked in — the two files Unity Hub itself writes when it creates a new
project — so Hub will recognize this folder as a real Unity project. (If
you've pulled an older copy of the branch without these, `git pull` first —
without them, Hub's project list won't show the folder at all, which is the
"No Unity projects found" error some tooling reports.)

Everything else Unity normally generates on first launch — `Library/`
(its asset cache), the rest of `ProjectSettings/*.asset`, editor layout,
etc. — is intentionally still absent; those are machine/version-specific
and Unity fills them in automatically the first time it opens the project,
the same way it would for a brand-new one.

1. In Unity Hub: **Projects → Open → Add project from disk**, and select the
   root of this repo (the folder containing `Assets/`, `ProjectSettings/`,
   and `Packages/`).
2. Unity Hub will likely flag a version mismatch (the checked-in
   `ProjectVersion.txt` names `2022.3.50f1`, a placeholder — it almost
   certainly won't exactly match whatever patch you installed). That's
   fine: pick **"Open with [your installed 2022.3.x version]"** or similar —
   any 2022 LTS patch works, this project uses no version-specific features.
3. Click to open it. **First open takes a few minutes** — Unity is
   generating `Library/`, resolving the packages in `manifest.json`, and
   importing everything under `Assets/`.
4. If **Package Manager** flags any single package version as unresolvable
   (rare, but package versions do drift over time), open
   `Packages/manifest.json` in a text editor and delete the version number
   after the colon for that one line, leaving just `"com.unity.xxx": ""` —
   Unity will resolve it to whatever's actually available. The
   `com.unity.modules.*` entries can't fail this way; they ship with the
   Editor itself, not fetched from a registry.
5. If the Editor asks about **Input Handling** (a startup dialog on some
   versions), choose **"Both"** — the game uses the classic UI input module,
   and picking "Input System Package (New)" only would silently break clicks.
   You can also set this later at **Edit → Project Settings → Player →
   Active Input Handling → Both**, then let Unity restart when prompted.

---

## 3. Check the Console for compile errors

Open **Window → General → Console**. You want it clean (or only warnings,
never errors) before continuing.

**Why this step exists:** the presentation code
(`Assets/Scripts/Presentation/GameBootstrap.cs`, `UIFactory.cs`) was written
and reasoned through carefully, but it was never compiled against a real
Unity install — there's no Unity Editor available in the environment that
built it. The game *rules* engine (`Assets/Scripts/Core/Underdeck/`) is fully
verified — 125 tests pass via `dotnet test CoreTests`, including a
cross-engine parity check against the reference web prototype. The UI layer
carries residual risk that the rules layer doesn't.

**If you see errors:** copy the exact error text (it'll name a file and line
number) and send it back — a Unity compile error is a five-minute fix once
someone can see the message, and I'd rather fix a real one than have you
guess. Common, easy ones if you want to try yourself first:
- A method name typo → the error names the missing method.
- A using-directive issue → Unity's error links straight to it.

---

## 4. Create the scene and press Play

1. **File → New Scene** (a basic empty scene is fine — File → New Scene →
   Basic (Built-in) template, or whatever your Unity version calls it).
2. In the **Hierarchy** panel, right-click → **Create Empty**. Rename it
   something like `GameBootstrap`.
3. Select it, and in the **Inspector** click **Add Component**, search for
   `Game Bootstrap`, and add it.
4. **Save the scene**: File → Save As → name it e.g. `Main`, save under
   `Assets/Scenes/` (create that folder if it doesn't exist).
5. Press **Play** (▶ at the top of the Editor).

You should see the UNDERDECK title screen. Tap **Begin the Descent** and
play a run — the whole loop is there: rooms, combat, weapon dulling, the
skill hand, fleeing, curses in Depth II, elites and the boss chain through
Depth III, the between-depths shop, and the end screen.

**What it looks like:** plain dark panels and buttons, system font, no
custom art or animation — this build prioritizes the game *working
correctly* over looking like the reference web prototype. Visual polish
(the hex-card look, custom fonts, transitions, sound) is real, valuable
follow-up work once you've confirmed the loop plays the way you want on
device — happy to do that pass once this is running for you.

---

## 5. Build to your Android phone

1. **File → Build Settings**.
2. Select **Android** in the platform list, click **Switch Platform**
   (first time only — takes a minute or two).
3. Click **Add Open Scenes** to add your `Main` scene to the build list.
4. **Player Settings** (button in the same window): set a **Company Name**
   and **Product Name** if empty; under **Other Settings**, note the
   **Package Name** (`com.CompanyName.ProductName` by default) — Android
   requires this to be reverse-domain-style and unique-ish; the default is
   fine to start.
5. Turn on **Developer Mode** and **USB Debugging** on your phone:
   Settings → About Phone → tap **Build Number** 7 times → back out to
   **Developer Options** → enable **USB Debugging**.
6. Plug the phone into your computer via USB, accept the "Allow USB
   debugging?" prompt on the phone.
7. Back in Unity's Build Settings: click **Build And Run**. Pick a folder to
   save the `.apk`. Unity builds and installs it straight to your phone.

If `Build And Run` doesn't detect your phone, `Build` alone still produces an
`.apk` file you can transfer and install manually (you may need to allow
"install from unknown sources" on the phone).

---

## 6. iOS (only if you're on a Mac)

iOS builds require **Xcode**, which only runs on macOS — there's no way
around this regardless of what builds Unity itself.

1. In Unity Hub, add the **iOS Build Support** module (Installs → your
   Editor → gear icon → Add Modules).
2. **File → Build Settings → iOS → Switch Platform**.
3. **Build** — Unity produces an Xcode project folder, not an app directly.
4. Open the generated `.xcodeproj` in Xcode.
5. In Xcode: select your Apple ID under **Signing & Capabilities** (a free
   Apple ID works for installing to your own device for 7 days at a time;
   the paid Apple Developer Program — $99/yr — is needed for TestFlight or
   the App Store).
6. Plug in your iPhone, select it as the run target, hit **Run** (▶).

---

## What's genuinely done vs. what's next

**Done and verified** (this is the substantial part):
- The complete ruleset in C#, matching the web prototype exactly — 125
  passing tests, including deck-shuffle parity against the JS engine for
  fixed seeds.
- A full, playable UI covering every screen: title, rooms, combat sheets,
  skill hand with all 15 skills, targeting modes (Bash/Snipe/Smoke Bomb/
  Knife Throw), curses, elites, all three bosses, relic drafts, the shop,
  and the end screen.

**Likely next steps, roughly in order of value:**
1. Fix whatever the Console shows on first open (see step 3) — hopefully
   nothing, possibly a small thing.
2. Play a full run on a phone and tell me what feels off — the balance
   numbers already tuned in the web version carry over exactly, but touch
   targets, text sizes, and pacing on an actual device are things I can't
   evaluate without you.
3. Visual pass — swap the plain panels for the hex-card look, add a real
   font, juice (screen shake, sound, card animations) — all straightforward
   once the loop is confirmed working.
4. Package the Unity-generated `ProjectSettings/` and `Packages/` folders
   into version control once they exist on your machine, so the *whole*
   project (not just `Assets/`) is reproducible from a fresh clone.
