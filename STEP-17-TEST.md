# Step 17 — Physical iPhone Test

Build Codemagic workflow:

```text
ios-step-17
```

Expected app:

```text
STEP 17 — COMPATIBILITY CALL-SITE ANALYSIS
Version 0.0.46
```

## Before testing

Use the existing good Step 12 managed StS2 install. Start from a fresh process if the Step 15 Godot host was run in the current process.

## Run

Tap:

```text
Run Gates A–D — ARM64 Scope → Actual IL Calls → Native/Platform → Dependency Map
```

Stop at the first failed gate.

Gate A will re-hash the full OfflineReady tree and may take some time. Gate B then scans only the iOS-relevant managed scope one module at a time. Gate D re-hashes the much smaller scanned managed scope after analysis.

## Target

```text
COMPATIBILITY CALL-SITE ANALYSIS PASS — 4/4
```

For a 4/4 pass, capture screenshots of Gate B, Gate C and Gate D details. The exact counts are intentionally not predetermined; they are the evidence used to choose the next compatibility target.

Important expected safety lines:

```text
Assembly dependency resolution attempted: NO
Steam session consulted: NO
Network attempted: NO
Real managed install modified: NO
Game assembly loaded/executed: NO
Primary sts2.dll receipt SHA-1 preserved: YES
All Step 17 scan candidates receipt SHA-1 preserved: YES
```

After 4/4, run:

```text
Verify Offline-Ready Install (Local Only)
Run Foundation 5/5 Regression
```

No real StS2 rewrite or execution is part of Step 17.
