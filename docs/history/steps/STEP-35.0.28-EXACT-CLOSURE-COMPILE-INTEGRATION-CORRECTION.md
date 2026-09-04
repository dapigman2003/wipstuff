# Step 35.0.28 — Exact-closure compile-integration correction

Release: 0.0.151 (151)

## Trigger

Codemagic 0.0.150 passed 895/895 static validation, all 214 host tests, and the Step-15 standalone native-link preflight, then failed iOS compilation on one `CS0103` in `RootViewController.Step35ManagedPluginBootstrap.cs`: the partial used `Step35DiagnosticMode` without importing `StS2Launcher.Core.Runtime`.

## Correction

Add `using StS2Launcher.Core.Runtime;` to the managed-plugin bootstrap partial and pin that import in `tools/validate_current.py`. Advance release/candidate identity to Step 35.0.28 / 0.0.151 and preserve the 0.0.150 CI failure as immutable provenance.

## Runtime boundary

No Step-35 runtime behavior is broadened or changed. `GodotCoreExactClosure`, the 225-pointer managed→native handoff, 37-pointer reverse ManagedCallbacks bootstrap, post-bootstrap resolver seal, exact transformed sts2 authority, exact prepared GodotSharp authority, bounded ExecuteVeryEarly invocation/await, and Gate-D finalization telemetry remain the 0.0.150 design.

Later OneTimeInitialization phases, game entry-point execution, native game loading, arbitrary resolver fallback, and Harmony/MonoMod runtime patching remain forbidden.

## Expected proof

Codemagic must advance beyond the former CS0103 point and prove the current host suite, iOS compile/link, and IPA verification. Only then should the physical Step-15 → Step-35 EXACT-CLOSURE run be attempted.
