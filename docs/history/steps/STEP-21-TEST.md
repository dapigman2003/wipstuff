# Step 21 — Physical iPhone Test

## Build

Use Codemagic workflow:

```text
ios-step-21
```

Expected app:

```text
STEP 21 — PREPARED RUNTIME / FRAMEWORK BINDING
Version 0.0.56
```

## Before testing

Use a fresh launcher process if the Step 15 Godot host was started in the current process.

The managed StS2 install should already be OfflineReady from the closed Step 20 baseline.

## Run

Tap:

```text
Run Gates A–D — Classify Runtime → Bind Host Frameworks → Prepare IL Set → Closure Audit
```

Stop at the first failing gate.

## Gate A expected meaning

A pass proves:

- OfflineReady is still valid;
- ARM64/shared receipt-backed managed scope was freshly copied;
- every copy was SHA-1 verified;
- real assembly identities and IL-only/non-IL-only shape were cataloged;
- x86_64 duplicates were excluded;
- the real primary ARM64 `sts2.dll` was found and is IL-only;
- no StS2 assembly was CLR-loaded.

If Gate A fails, send the complete screen. Do not proceed.

## Gate B expected meaning

A pass means the complete reachable AssemblyRef graph was authoritatively classified.

Read these two lines carefully:

```text
Explicit binding blockers: N
Runtime closure ready for first real CLR load: YES/NO
```

`NO` is not itself a Step 21 gate failure. It means Step 21 has successfully found concrete work for Step 22.

Capture the blocker sample if `N > 0`.

Particularly useful blocker kinds include:

```text
HostFrameworkUnavailable
MissingWorkspaceAssembly
WorkspaceIdentityMismatch
WorkspaceVersionTooLow
WorkspaceVersionAmbiguity
WorkspaceByteAmbiguity
NonIlOnlyWorkspaceAssembly
HostPrivateSimpleNameConflict
```

## Gate C expected meaning

A pass should report evidence including:

```text
Cecil assembly writes performed by Step 21 Gate C: 0
Prepared assembly bytes remain receipt-identical: YES
Strong-name/public-key metadata modified: NO
```

and the plan path:

```text
Step21-PreparedRuntimeBinding/plan/runtime-binding-plan.json
```

## Gate D expected meaning

A pass should include:

```text
Original Step 12 managed install unchanged: YES
Post-preparation OfflineReady exact-tree verification: YES
StS2 assembly loaded/executed: NO
```

and the same final readiness signal:

```text
Runtime closure ready for first real CLR load: YES/NO
```

Final subsystem result:

```text
PREPARED RUNTIME / FRAMEWORK BINDING PASS — 4/4
```

## Closure checks

After 4/4, run:

```text
Verify Offline-Ready Install (Local Only)
Run Foundation 5/5 Regression
```

If both pass, Step 21 is closed.

## What to send back

If A–D pass, send either the full Gate D screen or at minimum:

```text
A-D: PASS
Explicit binding blockers: N
Runtime closure ready for first real CLR load: YES/NO
```

If blockers are non-zero, include their sample lines.

If any gate fails, send the complete failing detail screen and do not retry the same IPA blindly.
