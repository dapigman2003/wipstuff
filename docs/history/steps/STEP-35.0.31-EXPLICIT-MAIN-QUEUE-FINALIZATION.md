# Step 35.0.31 — Explicit main-queue finalization

## Trigger

Physical 0.0.153 proved `D_WORKER_RETURN` with `passed=True; exactAuthority=True`, while the captured UIKit await continuation still never resumed.

## Change

The Gate-D outer `Task.Run` await now uses `ConfigureAwait(false)`. The post-worker continuation therefore does not depend on the UIKit `SynchronizationContext`. Final Gate-D result recording, progress completion, and user-visible labels are explicitly dispatched with `InvokeOnMainThread` and are bounded by durable `D_UI_DISPATCH_ENTER` / `D_UI_DISPATCH_RETURN` markers.

The deterministic report path remains output-only. Label snapshots and final `EndSteamOperation` teardown are explicitly marshaled to the main thread where required.

Step 36's prerequisite is the durable same-process exact Step-35 core-closure flag plus exact-closure mode, not the UI gate-sequence snapshot. Step-36 Gate D proactively uses the same noncapturing continuation plus explicit-main-thread finalization pattern.

## Non-goals

No Step-35 runtime authority, Godot bridge, resolver policy, exact `ExecuteVeryEarly` invocation, Gate-D integrity audit, or game/native-loading behavior is changed. Step 36 still invokes only exact `ExecuteEssential`; `ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry, Harmony/MonoMod runtime patching, and native game loading remain forbidden.
