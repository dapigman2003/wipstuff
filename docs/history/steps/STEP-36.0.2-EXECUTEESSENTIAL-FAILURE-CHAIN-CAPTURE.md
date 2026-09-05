# Step 36.0.2 — ExecuteEssential failure-chain capture

Candidate: **0.0.156 (156)**

## Physical basis

Physical 0.0.155 proves the exact game-resource-pack handoff. The receipt-backed PCK is located from the inherited Step-12 authority, exact prepared GodotSharp `ProjectSettings.LoadResourcePack` returns true with `replaceFiles=false` and offset 0, and `Godot.DirAccess.Open` proves `res://localization/eng`. The unchanged exact transformed `ExecuteEssential()` invocation then throws; 0.0.155 exposes only a nested `TargetInvocationException: Arg_TargetInvocationException` and therefore does not identify the first internal failing initializer.

## Authorized change

No game IL or execution ordering changes. Preserve Gate A, Gate B, exact token `0x06007D03`, exact source/transformed semantic equality, exact PCK handoff, same-process Step-35 authority, and one `MethodInfo.Invoke(null, null)`.

When that invocation throws, durably capture every `InnerException` depth (type/message/HResult/source/target/stack), `ReflectionTypeLoadException.LoaderExceptions`, `GetBaseException()`, post-failure `OneTimeInitialization._state`, invocation-time resolver/load deltas, and exact sts2/GodotSharp load-context continuity. Then fail Gate C normally.

## Still forbidden

No retry, no state reset, no direct child-initializer invocation, no `ExecuteDeferred`, no launcher-driven `PrewarmJit`, no game entry, no Harmony/MonoMod runtime patching, no arbitrary resolver fallback, and no native game loading.

## Success criterion

A physical 0.0.156 failure report must identify the true base exception and enough stack/target/context evidence to select the next **narrow** Step-36 correction without splitting ExecuteEssential speculatively. If ExecuteEssential instead returns, the existing state-2 and Gate-D isolation contracts remain authoritative.
