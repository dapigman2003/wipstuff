# Step 20 physical test

## Build

Codemagic workflow:

```text
ios-step-20
```

Expected installed UI:

```text
STEP 20 — DYNAMIC MANAGED EXECUTION FOUNDATION
Version 0.0.55
```

The Codemagic run must first pass the host unit suite, then the iOS publish, then final IPA verification. The final verifier requires exactly three Step 20 fixture DLLs under `Step20DynamicFixtures/`, byte-compares each against the fixture built earlier in the same run, and verifies the bundled SHA-256 manifest.

## Device procedure

Start from a fresh launcher process if the Step 15 process-global Godot host was started in the current process.

Tap:

```text
Run Gates A–D — Fixture Integrity → External IL Execute → Private Dependency → Isolation Audit
```

Stop at the first failed gate and capture the complete detail screen.

### Gate A expected

- OfflineReady: YES.
- 3/3 bundled fixture SHA-256s verified.
- 3/3 launcher-private copies SHA-256 verified.
- exact managed identities displayed.
- fixture metadata boundary reports pure IL and expected references.
- no StS2 load/network/live-install mutation.

### Gate B expected

```text
Dynamic fixture result: 42 (expected 42)
Private dependency loads: 0
Execution mechanism proven: runtime-loaded IL can execute ... without JIT code generation.
StS2 assembly loaded/executed: NO
```

A Gate B failure is the most important result: it means the Release host still cannot execute the post-publish managed IL fixture through the intended interpreter path, or the dynamic load/reflection mechanism needs a narrower correction. Do not move to runtime/framework binding if B fails.

### Gate C expected

```text
Dependent fixture result: 42 (expected 42)
Verified private dependency loads: 1
Dependency fallback to live StS2 install: NO
Dependency fallback to network: NO
StS2 assembly loaded/executed: NO
```

### Gate D expected

```text
Post-execution OfflineReady exact-tree verification: YES
Managed install identity unchanged: YES
StS2 assembly loaded/executed: NO
Writes to receipt-backed managed install: NO
```

Final subsystem target:

```text
DYNAMIC MANAGED EXECUTION FOUNDATION PASS — 4/4
```

After 4/4 run:

```text
Verify Offline-Ready Install (Local Only)
Run Foundation 5/5 Regression
```

Step 20 is closed only when both closure regressions also pass.
