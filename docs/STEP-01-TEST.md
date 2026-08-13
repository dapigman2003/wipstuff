# Step 01 — Physical iPhone Test

## Purpose

This test answers one question only:

> Can a fresh .NET iOS/UIKit application start on the physical iPhone, create its scene/window, and render a working native UI?

No other subsystem is being tested.

## Expected result

### A. First launch

Within a few seconds of tapping the app icon, you must see a **white** screen.

The screen must contain:

```text
StS2 Launcher

STEP 01 — UI BOOTSTRAP PASS

Version 0.0.1

Status: UI rendered successfully.

[ Write Test Log ]
```

A persistent black screen is a **FAIL**.

An immediate crash is a **FAIL**.

A screen that never reaches `STEP 01 — UI BOOTSTRAP PASS` is a **FAIL**.

### B. Button test

Tap:

```text
Write Test Log
```

Expected:

The status text changes to something beginning with:

```text
PASS: test log written
```

If it changes to `FAIL: ...`, report the complete message.

### C. Lifecycle test

1. Put the app in the background.
2. Wait a few seconds.
3. Return to the app.

Expected:

The same Step 01 screen is visible and responsive.

Then:

1. terminate the app from the app switcher;
2. reopen it.

Expected:

The Step 01 screen appears again.

## What to report back

Copy this and fill it in:

```text
STEP 01 RESULT

Install: PASS / FAIL
First launch: PASS / FAIL
Screen color: white / black / other
"STEP 01 — UI BOOTSTRAP PASS" visible: YES / NO
Write Test Log button: PASS / FAIL / NOT REACHED
Background -> foreground: PASS / FAIL / NOT TESTED
Terminate -> reopen: PASS / FAIL / NOT TESTED

Exact status/error text:
...

Anything else observed:
...
```

## Advancement rule

We do not add Steam, Godot, Keychain, Cecil, native archives, or old launcher code until this test passes on the physical iPhone.
