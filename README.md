# StS2 Launcher iOS — Step 02

Step 01.1 passed on a physical iPhone.

Step 02 tests the **launcher UI/state shell** while deliberately keeping every real subsystem out.

## Still NOT included

- SteamKit2 / networking
- Steam credentials or Keychain
- ownership verification
- depot downloads
- Godot
- Mono.Cecil
- native libraries
- game files
- runtime patching

## What Step 02 adds

One native UIKit launcher screen with seven deterministic mock states:

1. Signed out
2. Signing in…
3. Checking ownership…
4. Ready to install
5. Downloading… (42%)
6. Ready to play
7. Example error

Use `Next Demo State` to cycle through them.

## Build

Codemagic workflow:

```text
ios-step-02
```

Expected artifact:

```text
artifacts/StS2-Launcher-Step-02.ipa
```

## Expected device result

First launch must show:

```text
StS2 Launcher
STEP 02 — LAUNCHER UI SHELL
Version 0.0.3
DEMO STATE 1 OF 7
Signed out
```

Tap `Next Demo State` six times.

Each state must render, and state 5 must show a visible progress bar at 42%.

State 7 must show the deliberate text:

```text
TEST ERROR: This is a deliberate visible error state.
```

One more tap must wrap back to state 1.

See `docs/STEP-02-TEST.md`.
