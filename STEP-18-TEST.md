# Step 18 physical-iPhone test

Build Codemagic workflow:

```text
ios-step-18-4
```

Expected app header:

```text
STEP 18.4 — REAL ASSEMBLY REWRITE WORKSPACE
Version 0.0.51
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
Primary Cecil round-trip output reopens with explicit workspace resolver: YES
Neutral NOP rewrite still present after explicit-resolver reopen: YES
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


### Step 18.4 Gate B diagnostic expectation

The 0.0.50 physical failure returned to a plain `AssemblyResolutionException` for `GodotSharp` only after Step 18.3 had handled the preceding `System.Runtime 8.0.0.0 -> 9.0.0.0` writer-resolution boundary. Step 18.4 removes the unbound generated-output reopen path. A passing Gate B must now report `Generated-output reopen resolver explicitly bound to workspace identity catalog: YES` and `Generated-output verification uses deferred Cecil reading: YES`.

If Gate B still fails, the detail begins with `Stage:` and must identify whether the failure happened during the primary source read, writer, generated-output reopen, fingerprint verification, or hash postflight. Do not retry unchanged; capture that complete staged diagnostic. Exact identity remains preferred, the existing unambiguous version-only workspace rule remains available, and runtime/system/live-install/network fallback remains forbidden.
