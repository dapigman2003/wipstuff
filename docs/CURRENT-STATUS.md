# Current status

## Active candidate — Step 35.0.31 / Step 36.0.2 / 0.0.156 (156)

Physical **0.0.155** advances the Step-36 frontier again. Gate B located the exact receipt-backed `SlayTheSpire2.app/Contents/Resources/Slay the Spire 2.pck`, exact `Godot.ProjectSettings.LoadResourcePack(..., replaceFiles=false, offset=0)` returned true, and `res://localization/eng` was present. Gate C then invoked exact transformed `ExecuteEssential` once and failed through a nested `System.Reflection.TargetInvocationException`; the process recovered normally through `RUN_END`. The PCK/resource handoff is therefore positive, while the first internal essential-initialization failure remains unlocalized because 0.0.155 collapsed the deeper exception chain.

**0.0.156 / Step 36.0.2** retains that exact PCK handoff and exact `0x06007D03` ExecuteEssential boundary unchanged. It adds complete exception-chain/base/loader-exception telemetry, post-failure state capture, resolver/load deltas, and exact sts2/GodotSharp load-context continuity. It performs no retry, no state reset, and no child-method probe. `ExecuteDeferred` and launcher-driven `PrewarmJit` remain forbidden.

Steps 32–34 remain CLOSED POSITIVE. Physical 0.0.152 established positive exact Step-35 core closure under the explicitly defined source-built Godot 4.5.1 bridge prerequisite: exact transformed sts2 and exact prepared GodotSharp were the CLR inputs; exact `ExecuteVeryEarly` returned and awaited `RanToCompletion`; post-await resolver/native confinement passed; OfflineReady re-proved 428/428; exact authority/plan/dependency/context checks passed; and Gate D constructed `passed=True; exactAuthority=True`.

Physical 0.0.153 then proved the outer Gate-D worker itself also returned that already-passed result. Step 35.0.31 / 0.0.154 replaced the captured UIKit continuation with a noncapturing continuation plus explicit main-thread UI mutation. The subsequent physical 0.0.154 Step-36 run reached its own deterministic final report and `RUN_END`, so the new completion architecture is no longer the active Step-36 blocker.

## Physical 0.0.154 Step-36 result — exact ExecuteEssential localization-resource boundary

The same-process exact Step-35 core closure was present. Step 36 then produced:

- Gate A PASS: exact source token `0x06007D03`, transformed token `0x0600AFE8`, semantic SHA-256 `1df430b82462cd22c50be2ef0cf56bec66382421a8784620fb4ca015bb1fb832`, no direct ExecuteDeferred/PrewarmJit/Harmony crossover.
- Gate B PASS: exact transformed `ExecuteEssential` bound from the resident exact Step-35 authority with state `1` and unchanged resolver baseline.
- Gate C exact invocation START on the main thread.
- Gate C deterministic FAIL: `MegaCrit.Sts2.Core.Localization.LocException: Path does not exist: res://localization/eng`.
- deterministic report returned and `RUN_END` completed normally.

This localizes Step 36 to the live Godot resource filesystem. It is not evidence of a new managed resolver, callback-table, reverse-binding, or native-library failure.

## Step 36.0.1 — Exact game resource-pack handoff

The next candidate does not rewrite or bypass localization. It reproduces the missing resource-pack lifecycle before the unchanged exact `ExecuteEssential` call.

Gate A remains the exact source/transformed semantic reproof. Gate B now additionally locates the exact receipt-backed game PCK at `SlayTheSpire2.app/Contents/Resources/Slay the Spire 2.pck` from the managed-install root inherited from Step 35, verifies receipt/depot/path/length/SHA-1-shape continuity, then calls exact prepared GodotSharp `Godot.ProjectSettings.LoadResourcePack` with `replaceFiles=false` and offset `0`. Gate B must then prove the exact prior failure directory `res://localization/eng` is visible through exact `Godot.DirAccess.Open` before Gate C is allowed to run.

The huge PCK is not redundantly SHA-1 hashed in Gate B because Step-35 Gate D just completed the full receipt-backed OfflineReady proof. Step-36 Gate D still performs the normal full OfflineReady reproof after `ExecuteEssential`, so receipt hash authority is preserved.

Gate C still invokes only exact transformed `ExecuteEssential` once and requires state `1 -> 2` with strict resolver/native confinement. Gate D still re-proves source/transformed/plan/dependency hashes, exact CLR ownership, resource-pack file continuity, resolver state, and final state `2`.

`ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry, Harmony/MonoMod runtime patching, arbitrary resolver fallback, and native game loading remain forbidden.
