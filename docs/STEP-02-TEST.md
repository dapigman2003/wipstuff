# Step 02 — Physical iPhone Test

## Purpose

Step 02 proves that the fresh UIKit launcher can render and transition through the states we will later connect to real services.

No Steam/network/game/runtime behavior is present.

## Expected first screen

```text
StS2 Launcher
STEP 02 — LAUNCHER UI SHELL
Version 0.0.3
DEMO STATE 1 OF 7
Signed out
Steam is not connected. This is mock UI only.
```

The app should remain open and responsive.

## State sequence

Tap `Next Demo State`.

Expected sequence:

### State 1
```text
Signed out
```

### State 2
```text
Signing in…
Pretending to authenticate with Steam.
```

### State 3
```text
Checking ownership…
Pretending to verify Slay the Spire 2 ownership.
```

### State 4
```text
Ready to install
Ownership verified. Game files are not installed.
```

### State 5
```text
Downloading…
Pretending to download game files. Progress should show 42%.
```

A visible progress bar should be present at roughly 42%.

### State 6
```text
Ready to play
Mock installation is ready. Play is intentionally disabled in Step 02.
```

### State 7
```text
Example error
TEST ERROR: This is a deliberate visible error state.
```

This is intentionally an error-looking state. It must **not** crash the app.

Tap `Next Demo State` once more.

Expected: back to `Signed out` / state 1 of 7.

## Primary-button check

In any state, tap the large state-specific primary button.

Expected status text begins with:

```text
PASS: primary action tapped
```

No network activity should occur.

## Reset check

Tap `Reset Demo`.

Expected: returns to state 1.

## Lifecycle check

Background and foreground the app.

Expected: app remains responsive.

Terminate and reopen.

Expected: app launches normally at state 1.

## Report back

```text
STEP 02 RESULT

Install: PASS / FAIL
First launch: PASS / FAIL
Stayed open: YES / NO
Initial state "Signed out": PASS / FAIL
State 2 "Signing in": PASS / FAIL
State 3 "Checking ownership": PASS / FAIL
State 4 "Ready to install": PASS / FAIL
State 5 "Downloading 42%": PASS / FAIL
State 6 "Ready to play": PASS / FAIL
State 7 deliberate error: PASS / FAIL
Wrapped 7 -> 1: PASS / FAIL
Primary button: PASS / FAIL
Reset Demo: PASS / FAIL
Background -> foreground: PASS / FAIL / NOT TESTED
Terminate -> reopen: PASS / FAIL / NOT TESTED

Exact error/status text if anything failed:
...

Other observations:
...
```

## Advancement rule

Do not connect Steam or Godot unless Step 02 passes on the physical iPhone.
