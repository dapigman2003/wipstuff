# Step 35.0.29 — Exact-closure declared-namespace correction

Release: 0.0.152 (152)

## Trigger

Codemagic 0.0.151 passed 899/899 static validation, 214/214 host tests, and the Step-15 native-link preflight, then iOS compilation stopped on one CS0234 in `RootViewController.Step35ManagedPluginBootstrap.cs`.

## Root cause

`Step35DiagnosticMode.cs` is stored under `src/StS2Launcher.Core/Runtime/` but declares `namespace StS2Launcher.Core;`. Candidate 0.0.151 incorrectly imported `StS2Launcher.Core.Runtime`, treating the directory name as the namespace.

## Change

- replace `using StS2Launcher.Core.Runtime;` with `using StS2Launcher.Core;`
- pin the declared namespace in `tools/validate_current.py`
- advance release/provenance identity to Step 35.0.29 / 0.0.152
- preserve the exact-authority closure, 225-pointer/37-pointer bridge bootstrap, resolver policy, and Gate-D finalization runtime unchanged

## Runtime policy

No broader startup is authorized. `ExecuteEssential`, `ExecuteDeferred`, game entry-point execution, native game loading, Harmony/MonoMod runtime patching, arbitrary resolver fallback, and fabrication of Godot runtime ownership remain forbidden.

## Next proof

Codemagic must first produce a clean iOS compile/link/IPA. Only then run a fresh physical process: Step 15 Gates A-C, keep the Step-15 engine alive, then Step 35 EXACT-CLOSURE once.
