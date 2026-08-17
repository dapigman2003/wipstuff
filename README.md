# StS2 Launcher iOS — Step 15.0.1 Godot Foundation Build Hotfix

Experimental unofficial iOS launcher/compatibility-host project for users who legitimately own Slay the Spire 2 on Steam.

## Project state

**Steps 01–14 are complete and closed on a physical iPhone.** Step 14 physically classified the installed public depot read-only: 428 files / 2,323,747,842 bytes, including 370 managed assemblies and 39 native binaries. It identified three broad iOS-risk classes for later work: desktop-native binaries, dynamic-code/JIT indicators, and platform-specific indicators. Those are triage signals, not proof that every marked path executes.

This archive is **Step 15.0.1 source hotfix / runtime `0.0.42 (42)`** and starts the accelerated testing model agreed after Step 14: one tightly related subsystem per version, several ordered gates, and no advancement past the first failing gate.

## Step 15 subsystem boundary — Godot Foundation

Step 15 builds **Godot 4.5.1-stable from source on the Codemagic macOS runner** as an arm64 iOS static archive and embeds a tiny project-owned Objective-C++ bridge. The normal Godot iOS `main()` symbol is renamed at build time so it cannot compete with the existing .NET/UIKit launcher entry point; Godot's `apple_embedded_main` embedded entry remains available.

The physical-device gates are:

- **Gate A — Native availability:** managed `DllImport("__Internal")` resolves the statically linked bridge and it reports exactly Godot `4.5.1-stable`.
- **Gate B — Engine/render-loop control:** initialize Godot against the launcher-owned smoke project, prove the CADisplayLink render loop can stop and restart, then leave it active.
- **Gate C — Metal render:** wait for Godot setup to finish, require a Metal-backed rendering layer, and require the smoke scene's fresh render marker while the scene is visibly rendered.
- **Gate D — Touch/lifecycle:** require a real `InputEventScreenTouch` marker plus focus/background/foreground callbacks forwarded into `OS_AppleEmbedded`.

The Step 15 bridge mirrors the lifecycle calls used by Godot's normal Apple-embedded app-delegate service, because this project intentionally keeps the already-proven .NET/UIKit app delegate instead of replacing it.

## Build

Use Codemagic workflow:

```text
ios-step-15
```

Expected app:

```text
0.0.42 (42)
STEP 15 — GODOT FOUNDATION
```

Expected IPA:

```text
artifacts/StS2-Launcher-Step-15.ipa
```

The first Codemagic run may take materially longer than previous steps because it compiles Godot from source. A fingerprinted Godot static archive is cached for later identical builds.

See `docs/STEP-15-TEST.md` for the ordered physical-iPhone gates.

## Scope boundary

Step 15 uses only the launcher-owned GDScript smoke project. It does **not** load, rewrite, or execute StS2 managed assemblies; it does not link the desktop StS2 native libraries; it does not add Mono.Cecil; and it does not implement FMOD, Spine, Steamworks-in-game integration, Cloud, Workshop, or actual game launch.


## Step 15.0.1 build hotfix

The first Step 15 Codemagic attempt completed the pinned Godot iOS source build, then failed only in the project-owned archive-symbol validator. Step 15.0.1 corrects C++ symbol-mangling validation for `apple_embedded_main` and removes a `grep -q`/`pipefail` SIGPIPE hazard. Runtime code/version and the physical Gate A–D contract are unchanged.
