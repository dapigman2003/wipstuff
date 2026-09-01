# Step 35.0.15 — GodotSharp diagnostic bridge-verifier correction

Candidate: `0.0.138 (138)`.

## Trigger

The 0.0.137 Codemagic attempt passed static validation but stopped at 208/209 host tests. `ComprehensiveGodotSharpDiagnosticCloneUsesEntryOnlyMarkersAndPreservesIdentity` threw from post-write verification while checking `INMETHOD_GS001` in `Godot.Collections.Dictionary`2::.ctor()`.

Source inspection showed the inserted marker itself used the intended `GodotSharpCheckpointBridge.Emit` reference. The shared `HasInjectedEntryMarkerAtStart` helper, however, hard-coded `ExecuteVeryEarlyCheckpointBridge` when verifying the following `call Emit`. That helper was valid for sts2 derivative markers but invalid for the separate GodotSharp derivative.

## Correction

`HasInjectedEntryMarkerAtStart` now accepts `expectedBridgeTypeFullName`, defaulting to `DiagnosticBridgeTypeFullName` so existing sts2 verification behavior is unchanged. `CreateInstrumentedGodotSharpDiagnosticClone` explicitly supplies `GodotSharpDiagnosticBridgeTypeFullName` during serialized marker verification.

This is a verifier-only correction. It does not change:

- the exact closed Step-32 source/transformed authority;
- NATURAL or COMPAT transform semantics;
- the GodotSharp marker insertion plan;
- resolver authority or initializer-bearing rejection;
- native-load refusal;
- one-invocation / <=60-second await rules;
- Godot/game startup prohibition;
- the fact that any A–D 4/4 result is diagnostic rather than exact Step-35 closure.

## Additional cleanup

0.0.138 synchronizes active docs, Codemagic labels, IPA verification text, release identity, and the Step-35 candidate manifest. The stale architecture/regression text that still described live-stack CL/CLTV sweeps as active was replaced with the physically established rule: those runtime callbacks remain retired after 0.0.133/0.0.135, while exact-source maps remain output-only.

## Input verification during preparation

Owner-supplied `sts2.dll` SHA-256: `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`, matching the closed Step-32 source pin.

Owner-supplied `GodotSharp.dll` SHA-256: `0e4897ecdfb31456a97c7d8028dfb8d7dbdc632e2f73fc9b438d7b266a139289`. This is recorded as observed evidence only; the runtime design continues to derive/reverify the prepared dependency hash from the trusted plan rather than introducing a new global hard-coded GodotSharp hash.

## Acceptance

Before physical testing, Codemagic must pass static validation, the full host suite, iOS build, and IPA verification. NATURAL and COMPAT then require separate fresh-process device runs, NATURAL first.
