# Current status

## Active candidate — Step 35.0.30 / Step 36.0 / 0.0.153 (153)

Steps 32–34 remain CLOSED POSITIVE. Physical 0.0.152 establishes **positive exact Step-35 core closure** under the explicitly defined source-built Godot 4.5.1 bridge prerequisite: exact transformed sts2 and exact prepared GodotSharp were the CLR inputs; exact ExecuteVeryEarly returned and awaited `RanToCompletion`; post-await resolver/native confinement passed; OfflineReady re-proved 428/428; exact authority/plan/dependency/context checks passed; and Gate D constructed `passed=True; exactAuthority=True`.

The final 0.0.152 durable marker was `D_TASK_RETURN_START`. No `D_TASK_AWAIT_RESUMED` followed, while the app remained responsive with the terminal 4/4/finalization UI visible. This localizes the remaining defect to the UIKit await/result-record continuation after the already-completed core Gate-D result.

### 0.0.153 Step-35 UI-return correction

Gate D is now scheduled behind an outer `Task.Run` worker boundary. The inner core audit retains its existing `ConfigureAwait(false)` behavior and exact authority checks. New durable markers distinguish:

- `D_WORKER_SCHEDULE`
- core `D_RESULT_CONSTRUCT_RETURNED`
- core `D_TASK_RETURN_START`
- outer `D_WORKER_RETURN`
- UI `D_TASK_AWAIT_RESUMED`
- `D_RESULT_RECORD_PASS`

No Step-35 bridge/resolver/game-member behavior is broadened by this fix.

## Step 36.0 — Controlled exact ExecuteEssential

Authority is pinned to the same exact transformed sts2 assembly already resident from Step 35. The exact source method is:

- `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteEssential()`
- source token `0x06007D03`
- static, parameterless, `System.Void`

Gate A statically re-proves exact source/transformed semantics with rejecting Cecil resolvers and requires no direct ExecuteVeryEarly/ExecuteDeferred/PrewarmJit/Harmony crossover. Gate B binds the exact transformed method and requires `OneTimeInitialization._state == 1`. Gate C invokes it once on the main thread and requires normal return plus state `2`, with the existing strict prepared-plan resolver and zero initializer-bearing/rejected/native escape. Gate D re-proves OfflineReady, hashes, plan/dependencies, exact CLR ownership, clean resolver state and state `2`.

`ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry, Harmony/MonoMod runtime patching, arbitrary resolver fallback, and native game loading remain forbidden.
