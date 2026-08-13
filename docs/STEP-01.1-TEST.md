# Step 01.1 — Physical iPhone Test

## Change under test

Step 01 used the wrong managed base class for its window-scene delegate.

Step 01.1 changes:

```csharp
UISceneDelegate
```

to:

```csharp
UIWindowSceneDelegate
```

and uses its exported `Window` property.

Nothing from Steam/Godot/game integration has been added.

## Expected result

After installing `StS2-Launcher-Step-01.1.ipa`, tap the icon.

### PASS

A white screen appears and contains:

```text
StS2 Launcher
STEP 01.1 — UI BOOTSTRAP PASS
Version 0.0.2
Status: UI rendered successfully.
Lifecycle: Active
Write Test Log
```

Tap `Write Test Log`.

Expected status:

```text
PASS: test log written at ...
```

Background and foreground the app, then terminate and reopen it.

The same screen must return.

### FAIL

Any of these is a failure:

- immediate termination;
- persistent black screen;
- red `STEP 01.1 STARTUP ERROR` screen;
- button reports `FAIL: ...`.

## Report back

```text
STEP 01.1 RESULT

Install: PASS / FAIL
First launch: PASS / FAIL
Stayed open: YES / NO
Screen color: white / black / other
"STEP 01.1 — UI BOOTSTRAP PASS" visible: YES / NO
Write Test Log: PASS / FAIL / NOT REACHED
Background -> foreground: PASS / FAIL / NOT TESTED
Terminate -> reopen: PASS / FAIL / NOT TESTED

Exact error/status text:
...

Other observations:
...
```

## Advancement rule

Do not proceed to Step 02 unless Step 01.1 passes on the physical iPhone.
