# Step 04 — Physical iPhone Keychain Test

## Purpose

Prove the iOS Keychain adapter using dummy data before any real Steam credential is introduced.

## First launch

Expected:

```text
STEP 04 — KEYCHAIN PROBE
Version 0.0.5
CORE LINK: PASS
```

On a clean Step-04 test, the persistence line should normally say:

```text
PERSISTENCE: NOT SET — expected before first test
```

If the same Step-04 Keychain item already exists from an earlier run, it may immediately say:

```text
PERSISTENCE: PASS — STEP04-BETA found
```

Both are acceptable before the round-trip test.

The app must remain open and responsive.

## Round-trip test

Tap:

```text
Run Keychain Round-Trip
```

Expected exact result:

```text
KEYCHAIN ROUND-TRIP PASS — 6/6
```

The persistence line should then say:

```text
PERSISTENCE: STEP04-BETA stored — terminate and reopen next
```

Do **not** press Delete yet.

## Persistence test

Now:

1. terminate the app from the iOS app switcher;
2. reopen it.

Expected on launch:

```text
PERSISTENCE: PASS — STEP04-BETA found
```

This is the most important Step-04 result.

It proves the value is coming back from Keychain in a new process.

## Delete test

After the persistence check passes, tap:

```text
Delete Test Secret
```

Expected:

```text
PERSISTENCE: DELETED — test value absent
```

and:

```text
PASS: dummy Keychain value deleted and confirmed absent.
```

Terminate and reopen once more.

Expected:

```text
PERSISTENCE: NOT SET — expected before first test
```

## Core regression

Tap:

```text
Run Core Self-Test
```

Expected:

```text
CORE SELF-TEST PASS — 12/12
```

Tap `Next Core State` several times and `Reset Core State`.

Expected: state changes and reset still work.

## Report back

```text
STEP 04 RESULT

Install: PASS / FAIL
First launch: PASS / FAIL
Stayed open: YES / NO
CORE LINK: PASS / FAIL
Initial Keychain query: PASS / FAIL
Round-trip: 6/6 PASS / FAIL
Terminate -> reopen persistence: PASS / FAIL
Saw "STEP04-BETA found": YES / NO
Delete test: PASS / FAIL
Reopen after delete shows NOT SET: PASS / FAIL
Core self-test: 12/12 PASS / FAIL / NOT RUN
Core state regression: PASS / FAIL

Exact error/status text if anything failed:
...

Other observations:
...
```

## Advancement rule

Do not add Steam authentication until the Keychain round-trip, process-restart persistence and delete checks all pass.
