# Step 19 physical-iPhone test

Build Codemagic workflow:

```text
ios-step-19
```

Expected app header:

```text
STEP 19 — EXPRESSION INTERPRETER COMPATIBILITY
Version 0.0.52
```

Codemagic must first pass the complete host-test suite and IPA verification. Do not treat a static source-validation pass as a substitute for those checks.

Start from a fresh launcher process if the Step 15 Godot host has been started in the current process.

Tap:

```text
Run Gates A–D — Interpreter Probe → Real Compile Targets → Rewrite → Isolation Audit
```

Stop at the first failing gate and capture the complete detail. Do not retry an unchanged IPA merely to seek a different result.

## Gate A target

Required evidence includes:

```text
Compile(preferInterpretation: true) probe result: 42 (expected 42)
Every workspace source copy receipt SHA-1 verified: YES
Game assembly loaded/executed: NO
Steam session consulted: NO
Network attempted by Step 19: NO
Real managed install modified: NO
```

The `RuntimeFeature` dynamic-code flags are diagnostics only. The functional interpreted-expression result is the capability proof.

## Gate B target

Gate B must report the real managed-module scan and at least one:

```text
Writable supported sites selected: >0
```

It should separately report parameterless-safe, literal-false, already-true, dynamic/non-literal, structurally unsafe, and strong-named counts.

If Gate B fails because there are no safe unsigned real targets, **do not broaden the matcher or modify arbitrary expression calls**. That result means this incompatibility class is not a suitable first real rewrite for the current depot and should be replaced by the next evidence-backed class.

## Gate C target

Required lines include:

```text
Total real call sites rewritten: >0
Every rewritten assembly reopened with explicit workspace assembly + metadata resolvers: YES
Every rewritten assembly preserves structural metadata; instruction-count delta equals only inserted bool arguments: YES
Every rewritten assembly has zero remaining structurally-safe parameterless/literal-false target sites: YES
Dynamic Compile(bool), unsafe branch/EH insertion sites and strong-named assemblies preserved: YES
Source workspace receipt SHA-1 preserved for every rewritten source: YES
Actual Step 12 install modified: NO
Game assembly loaded/executed: NO
```

## Gate D / final target

```text
EXPRESSION INTERPRETER COMPATIBILITY PASS — 4/4
```

Important Gate D lines:

```text
Source workspace receipt SHA-1s reverified: <all>/<all>
Original managed-install receipt SHA-1s reverified: <all>/<all>
Prepared files unchanged byte-for-byte: <non-target count>
Prepared assemblies intentionally rewritten: <target count>
Total Compile sites forced to interpreter preference: <rewrite count>
Every rewritten prepared assembly reopens with the explicit verified-workspace resolver: YES
No selected non-interpreted direct Compile target remains in rewritten outputs: YES
Original Step 12 install unchanged: YES
Only launcher-private Step19-ExpressionInterpreterCompatibility source/prepared files were written: YES
Fallback to runtime/system/live-install/network resolver paths: NO
Game assembly loaded/executed: NO
```

After 4/4, run the existing local-only OfflineReady verification and Foundation 5/5 regression once more before formally closing Step 19. This is intentionally conservative even though Gate D independently re-hashes the live install.

Step 19 remains a prepared-payload boundary. A pass does not claim that StS2 starts or that every dynamic-code incompatibility has been solved.
