# StS2 Launcher — Step 35.0.31 / Step 36.0.2

Active candidate: **0.0.156 (156)** — exact `ExecuteEssential` failure-chain capture with the physically proven receipt-backed game-PCK handoff retained unchanged.

Physical **0.0.155** closed the Step-36 resource-filesystem boundary: the exact Step-12-receipt-backed `SlayTheSpire2.app/Contents/Resources/Slay the Spire 2.pck` was located, exact prepared `GodotSharp` bound `ProjectSettings.LoadResourcePack`, the additive mount returned `true`, and `Godot.DirAccess.Open` proved `res://localization/eng` before Gate C. Gate C then invoked the unchanged exact transformed `OneTimeInitialization.ExecuteEssential()` once and failed through a nested `TargetInvocationException` chain. The 0.0.155 formatter exposed only `TargetInvocationException: Arg_TargetInvocationException`, so the first internal essential-initialization failure is still unknown.

**Step 36.0.2 changes observation only.** It preserves Gates A/B, the exact transformed method/token/semantic authority, the exact one-call Gate-C invocation, the PCK mount, resolver policy, and all forbidden boundaries. On a Gate-C throw it now durably captures:

- every `InnerException` depth with type, message, HResult, source, target method, and stack trace;
- `ReflectionTypeLoadException.LoaderExceptions`;
- `GetBaseException()` identity and stack;
- `OneTimeInitialization._state` immediately after failure;
- managed resolver, host-load, private-load, initializer-bearing, rejected-managed, and native-load deltas across the invocation;
- whether exact `sts2` and `GodotSharp` remain owned by the Step-35 private load context.

There is still **one** launcher `MethodInfo.Invoke(null, null)`, no retry, no state reset, and no direct child-initializer probes. `ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry, Harmony/MonoMod runtime patching, arbitrary resolver fallback, and native game loading remain forbidden.

## Physical test sequence

1. Fresh process: run Step 15 Gates A-C.
2. Without force-quitting/backgrounding, run Step 35 **EXACT-CLOSURE** once.
3. Run **Step 36.0.2 A-D** once.
4. Preserve the Step36 checkpoint journal, last checkpoint, static map, and final report.

Highest-value new markers: `E_C_EXCEPTION_CAPTURED`, `E_C_POST_FAILURE_CONTEXT`, `E_C_EXCEPTION_DEPTH`, `E_C_LOADER_EXCEPTION`, and `E_C_BASE_EXCEPTION`.
