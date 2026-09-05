# StS2 Launcher — Step 35.0.30 / Step 36.0

Active candidate: **0.0.153 (153)** — Gate-D UIKit return fix + controlled exact `ExecuteEssential`.

Physical **0.0.152** is the strongest result so far. EXACT-CLOSURE CLR-admitted the exact closed Step-32 transformed `sts2` artifact and exact prepared GodotSharp, reproduced the proven source-built Godot 4.5.1 bidirectional bridge, invoked exact transformed `OneTimeInitialization.ExecuteVeryEarly()` once, and received a `RanToCompletion` Task. Gate D then re-proved OfflineReady **428/428**, exact transformed authority, prepared-plan/dependency hashes, resolver/native confinement, and context ownership; the core constructed `passed=true; exactAuthority=true`. The final durable marker was `D_TASK_RETURN_START`. The UIKit await continuation never emitted `D_TASK_AWAIT_RESUMED`, so the visible finalization timer ran indefinitely even though the core result had already passed.

0.0.153 fixes that narrow UI-return defect by running the Gate-D audit behind an **outer `Task.Run` completion boundary** and adding `D_WORKER_SCHEDULE`, `D_WORKER_RETURN`, and `D_TASK_AWAIT_RESUMED` telemetry. The Step-35 exact runtime/bridge/audit logic is otherwise unchanged.

The same build also adds **Step 36.0** as the next separately gated initialization boundary. It is available only after a clean same-process Step-35 EXACT-CLOSURE 4/4. Step 36 pins exact source `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization.ExecuteEssential()` at token `0x06007D03`, statically re-proves source/transformed semantic equality, requires state `1`, invokes exact transformed `ExecuteEssential` once on the main thread, requires state `2`, and finally re-proves OfflineReady/hashes/resolver/context isolation.

Still forbidden in Step 36: `ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry-point execution, Harmony/MonoMod runtime patching, arbitrary resolver fallback, and native game executable/library loading.

## Physical test sequence

Use a fresh process:

1. Step 15 Gates A-C.
2. Without force-quitting or backgrounding, Step 35 **EXACT-CLOSURE** once.
3. Wait for Step 35 to visibly finalize **4/4**.
4. Without force-quitting, press **Step 36.0 A-D** once.
5. Preserve Step35 and Step36 run-correlated checkpoint/static-map/report files.

Do not run diagnostic Step-35 modes before the exact sequence in the same process.
