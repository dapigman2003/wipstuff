# StS2 Launcher iOS — Step 03

Step 01.1 proved the UIKit application lifecycle.
Step 02 proved the launcher UI/state shell.

Step 03 introduces exactly one new architectural boundary:

```text
StS2Launcher.Step03.iOS
        ↓
StS2Launcher.Core
```

`StS2Launcher.Core` is a plain `net9.0` managed class library. It owns the launcher state machine and contains **no UIKit references**.

## Still NOT included

- SteamKit2
- real network traffic
- Keychain
- ownership verification
- depot downloading
- Godot
- Mono.Cecil
- native libraries
- game files
- runtime patching

## What must appear on device

At launch:

```text
STEP 03 — CORE STATE MACHINE
Version 0.0.4
CORE LINK: PASS
CORE SELF-TEST: NOT RUN
CORE STATE 1 OF 7
Signed out
```

`CORE LINK: PASS` means the iOS application successfully loaded and executed the separate `StS2Launcher.Core` assembly.

Tap:

```text
Run Core Self-Test
```

Expected:

```text
CORE SELF-TEST PASS — 12/12
```

The existing state buttons are now driven by `LauncherController` in Core rather than an enum/state table inside the UIKit app.

See `docs/STEP-03-TEST.md`.

## Build

Codemagic workflow:

```text
ios-step-03
```

Expected artifact:

```text
artifacts/StS2-Launcher-Step-03.ipa
```
