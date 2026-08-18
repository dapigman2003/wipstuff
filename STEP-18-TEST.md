# Step 18 physical-iPhone test

Build Codemagic workflow:

```text
ios-step-18-3
```

Expected app header:

```text
STEP 18.3 — REAL ASSEMBLY REWRITE WORKSPACE
Version 0.0.50
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
Dependency resolver scope: SHA-1-verified Step 18 workspace ONLY
Resolved dependency file SHA-1 rechecked immediately before Cecil open: YES
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


### Step 18.3 Gate B diagnostic expectation

Gate B should continue resolving `GodotSharp` by verified metadata identity rather than filename. For the newly observed runtime-contract case, an exact identity remains preferred; if only the version differs and the verified workspace contains exactly one candidate with the same name/culture/public-key token, the resolution trace should show `[workspace version-unified]`. Multiple version-distinct identities or byte-distinct duplicates must still fail explicitly. No runtime/system/live-install/network fallback is permitted.
