# Step 03 — Physical iPhone Test

## Purpose

Step 03 proves that the working UIKit app can reference, load and execute a separate pure-managed Core assembly.

The state machine moved out of UIKit.

No real Steam, game or runtime functionality exists yet.

## Expected first screen

After installing `StS2-Launcher-Step-03.ipa`:

```text
StS2 Launcher
STEP 03 — CORE STATE MACHINE
Version 0.0.4

CORE LINK: PASS
CORE SELF-TEST: NOT RUN

CORE STATE 1 OF 7
Signed out
```

The app must remain open.

If `CORE LINK: FAIL` appears, stop and report the complete status line.

## Core self-test

Tap:

```text
Run Core Self-Test
```

Expected exact result:

```text
CORE SELF-TEST PASS — 12/12
```

The status should also say that the separate Core assembly self-test completed.

## State-machine test

Tap `Next Demo State`.

Expected states:

1. Signed out
2. Signing in…
3. Checking ownership…
4. Ready to install
5. Downloading… with 42% progress
6. Ready to play
7. Example error
8. wraps back to Signed out

These transitions are now calculated by `StS2Launcher.Core.LauncherController`.

## Primary action

Tap the state-specific primary button.

Expected status begins:

```text
PASS: Core handled primary action
```

## Reset

Move to any later state and tap `Reset Demo`.

Expected:

```text
CORE STATE 1 OF 7
Signed out
```

and status:

```text
PASS: Core reset returned to SignedOut.
```

## Lifecycle

Background -> foreground, then terminate -> reopen.

Expected: normal operation and state 1 on a fresh process launch.

## Report back

```text
STEP 03 RESULT

Install: PASS / FAIL
First launch: PASS / FAIL
Stayed open: YES / NO
CORE LINK: PASS / FAIL
Core self-test: 12/12 PASS / FAIL
All 7 states: PASS / FAIL
42% progress state: PASS / FAIL
7 -> 1 wrap: PASS / FAIL
Primary action from Core: PASS / FAIL
Reset through Core: PASS / FAIL
Background -> foreground: PASS / FAIL / NOT TESTED
Terminate -> reopen: PASS / FAIL / NOT TESTED

Exact error/status text if anything failed:
...

Other observations:
...
```

## Advancement rule

Step 04 is blocked until:

```text
CORE LINK: PASS
CORE SELF-TEST PASS — 12/12
```

are both proven on the physical iPhone.
