# Step 19.1 physical-iPhone test

Build Codemagic workflow:

```text
ios-step-19-1
```

Expected app header:

```text
STEP 19.1 — EXPRESSION INTERPRETER COMPATIBILITY
Version 0.0.53
```

Codemagic must first pass the complete host-test suite and IPA verification. A static source-validation pass is not a substitute for either.

Start from a fresh launcher process if the Step 15 Godot host has been started in the current process.

Tap:

```text
Run Gates A–D — Interpreter Probe → Real Compile Targets → Rewrite → Isolation Audit
```

Stop at the first failing gate and capture the complete detail. Do not retry an unchanged IPA merely to seek a different result.

## Prior physical evidence from 0.0.52

The first Step 19 run already proved:

```text
Gate A: PASS
Compile(preferInterpretation: true) probe result: 42 (expected 42)
RuntimeFeature.IsDynamicCodeSupported: False
RuntimeFeature.IsDynamicCodeCompiled: False

Gate B observed:
Structurally-safe parameterless Compile() sites: 8
Literal Compile(false) sites: 0
Parameterless sites skipped for branch/EH/prefix safety: 2
Supported sites carrying strong-name identity: 8
```

Gate B failed only because `0.0.52` required an unsigned target. Step 19.1 removes that blanket exclusion while preserving strong-name identity and the source/install trust boundary.

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

The `RuntimeFeature` dynamic-code flags are diagnostics. The functional interpreted-expression result is the capability proof.

## Gate B target

Gate B must report the real managed-module scan and at least one:

```text
Eligible supported sites selected: >0
Malformed StrongNameSigned-without-public-key supported sites: 0
```

It also reports parameterless-safe, literal-false, already-true, dynamic/non-literal, structurally unsafe, strong-name identity, and selected signed-assembly counts.

If the installed depot is unchanged from the `0.0.52` physical run, seeing the same eight safe parameterless sites selected is expected. The implementation does not hard-code that number.

If Gate B instead finds malformed signed-without-public-key metadata, or no structurally-safe direct targets at all, stop and capture the complete diagnostic; do not broaden the matcher.

## Gate C target

Required lines include:

```text
Total real call sites rewritten: >0
Every rewritten assembly reopened with explicit workspace assembly + metadata resolvers: YES
Every rewritten assembly preserves structural metadata; instruction-count delta equals only inserted bool arguments: YES
Every rewritten assembly has zero remaining structurally-safe parameterless/literal-false target sites: YES
Modified assemblies with StrongNameSigned cleared in prepared copy: <count>
Strong-name public key/token/full assembly identity preserved across every rewritten output: YES
Private strong-name signing key used: NO
Dynamic Compile(bool) and unsafe branch/EH insertion sites preserved: YES
Source workspace receipt SHA-1 preserved for every rewritten source: YES
Actual Step 12 install modified: NO
Game assembly loaded/executed: NO
```

Given the prior `0.0.52` observation that all eight safe sites carried strong-name identity, at least one signed target assembly may need its prepared-copy `StrongNameSigned` flag cleared. The exact assembly count is discovered at runtime rather than assumed.

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
Receipt-backed source strong-name state + prepared public keys/tokens/full identities/signature dispositions reverified: YES
Original Step 12 install unchanged: YES
Only launcher-private Step19-ExpressionInterpreterCompatibility source/prepared files were written: YES
Fallback to runtime/system/live-install/network resolver paths: NO
Game assembly loaded/executed: NO
```

After 4/4, run the existing local-only OfflineReady verification and Foundation 5/5 regression once more before formally closing Step 19.

Step 19.1 remains a prepared-payload boundary. A pass does not claim StS2 starts, that the prepared assemblies have been CLR-loaded, or that every dynamic-code incompatibility is solved.
