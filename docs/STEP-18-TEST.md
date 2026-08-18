# Step 18 physical-iPhone test

Build Codemagic workflow:

```text
ios-step-18
```

Expected app header:

```text
STEP 18 — REAL ASSEMBLY REWRITE WORKSPACE
Version 0.0.47
```

Start from a fresh launcher process if the Step 15 Godot host has been started in the current process.

Tap:

```text
Run Gates A–D — Clone ARM64 → Real Roundtrip → Neutral NOP → Isolation Audit
```

Stop at the first failing gate.

Final target:

```text
REAL ASSEMBLY REWRITE WORKSPACE PASS — 4/4
```

Important Gate D lines:

```text
Original managed-install receipt SHA-1s reverified: <all>/<all>
Primary Cecil round-trip output reopens: YES
Neutral NOP rewrite still present after reopen: YES
Only launcher-private Step18-RealAssemblyRewrite outputs were written: YES
Original Step 12 install unchanged: YES
Assembly dependency resolution attempted: NO
Steam session consulted: NO
Network attempted: NO
Game assembly loaded/executed: NO
```

After 4/4, run:

```text
Verify Offline-Ready Install (Local Only)
Run Foundation 5/5 Regression
```

Step 18 is not a game-execution or behavioral compatibility-fix boundary.
