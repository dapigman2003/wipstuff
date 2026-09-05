# Current status

## Active candidate — Step 35.0.31 / Step 36.0 / 0.0.154 (154)

Steps 32–34 remain CLOSED POSITIVE. Physical 0.0.152 established **positive exact Step-35 core closure** under the explicitly defined source-built Godot 4.5.1 bridge prerequisite: exact transformed sts2 and exact prepared GodotSharp were the CLR inputs; exact ExecuteVeryEarly returned and awaited `RanToCompletion`; post-await resolver/native confinement passed; OfflineReady re-proved 428/428; exact authority/plan/dependency/context checks passed; and Gate D constructed `passed=True; exactAuthority=True`.

Physical 0.0.153 then proved the outer worker itself also returns that already-passed result. The final durable sequence is `D_RESULT_CONSTRUCT_RETURNED` → `D_TASK_RETURN_START` → `D_WORKER_RETURN`, with `passed=True; exactAuthority=True`. The captured UIKit await continuation still never ran. That isolates the defect to continuation capture/dispatch rather than Gate D, the game, or the worker task.

### 0.0.154 Step-35 explicit-main-queue correction

Gate D still runs behind the outer worker, but the outer await now uses `ConfigureAwait(false)`. After `D_WORKER_RETURN`, completion remains on a thread-pool continuation and records `D_THREADPOOL_CONTINUATION`. Final Gate-D UI mutation is then explicitly marshaled with `InvokeOnMainThread`, bounded by `D_UI_DISPATCH_ENTER` / `D_UI_DISPATCH_RETURN`. Report snapshotting already marshals label reads; final heartbeat teardown and `EndSteamOperation` are explicitly sent to the main thread as needed.

Step 36 no longer requires the Step-35 UI gate-sequence snapshot to say 4/4. It requires the durable core closure flag plus exact-closure mode; the Step-36 core performs its own same-process authority continuity checks before any new invocation. Step-36 Gate D uses the same `ConfigureAwait(false)` + explicit-main-queue finalization pattern proactively.

## Step 36.0 — Controlled exact ExecuteEssential

Authority is pinned to the same exact transformed sts2 assembly already resident from Step 35. The exact source method is:

- `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteEssential()`
- source token `0x06007D03`
- static, parameterless, `System.Void`

Gate A statically re-proves exact source/transformed semantics with rejecting Cecil resolvers and requires no direct ExecuteVeryEarly/ExecuteDeferred/PrewarmJit/Harmony crossover. Gate B binds the exact transformed method and requires `OneTimeInitialization._state == 1`. Gate C invokes it once on the main thread and requires normal return plus state `2`, with the existing strict prepared-plan resolver and zero initializer-bearing/rejected/native escape. Gate D re-proves OfflineReady, hashes, plan/dependencies, exact CLR ownership, clean resolver state and state `2`.

`ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry, Harmony/MonoMod runtime patching, arbitrary resolver fallback, and native game loading remain forbidden.
