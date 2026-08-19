# StS2 Launcher iOS — Step 22.3 Foundation Consolidation

Experimental unofficial iOS launcher/compatibility host for **Slay the Spire 2** for legitimate owners. This repository does not contain Slay the Spire 2 payloads, Steam credentials/tokens, Apple signing credentials, or proprietary FMOD/Spine binaries.

## Current boundary

Steps 01–22 are physically closed on iPhone. Step 22.2 proved the prepared runtime/framework binding frontier with **zero explicit binding blockers**, `Runtime closure ready for first real CLR load: YES`, followed by OfflineReady and Foundation 5/5 passes.

Step 22.3 deliberately adds **no new StS2 compatibility behavior and no StS2 CLR load**. It is a consolidation build before Step 23:

- the complete physically proven Step 22.2 Core implementation is hash-protected byte-for-byte;
- Core, iOS UI, tests, docs, and tooling are organized by subsystem/current-vs-history;
- the 220 KB monolithic RootViewController is split into focused partial files;
- duplicated test temporary-directory infrastructure is consolidated;
- Codemagic has one current workflow and seven active scripts;
- host tests, static validation, IPA verification, and current on-device verification paths all emit shareable text reports;
- device reports are written atomically under `Documents/StS2Launcher/Reports` and are visible through the iOS Files app.

## Build

Use Codemagic workflow:

`ios-step-22-3`

The authoritative entry point is:

```sh
bash scripts/codemagic.sh
```

Local/macOS entry points are documented in `docs/TESTING.md`.

## Source layout

- `src/StS2Launcher.Core/Foundation` — launcher/controller/foundation primitives
- `src/StS2Launcher.Core/Steam` — Steam auth/content/download/install/offline logic
- `src/StS2Launcher.Core/Compatibility` — Steps 16–19 preparation/analysis/rewrite-expression boundaries
- `src/StS2Launcher.Core/Godot` — Step 15 gate model
- `src/StS2Launcher.Core/Runtime` — Steps 20–22 execution/binding/framework closure
- `src/StS2Launcher.Core/Diagnostics` — additive shareable report infrastructure
- `src/StS2Launcher.Step05.iOS/UI` — split iOS controller partials
- `tests/StS2Launcher.Core.Tests/*` — tests organized by subsystem
- `scripts` — current build/test/validation entry points only
- `history/scripts/steps` and `history/docs/steps` — retained historical step archaeology

See `docs/ARCHITECTURE.md` and `docs/REPORTS.md`.
