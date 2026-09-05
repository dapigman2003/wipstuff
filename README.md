# StS2 Launcher — Step 35.0.31 / Step 36.0

Active candidate: **0.0.154 (154)** — explicit main-queue Gate-D finalization + controlled exact `ExecuteEssential`.

Physical **0.0.153** narrows the Step-35 UI defect to one final framework boundary. Exact Gate D again re-proved OfflineReady **428/428**, exact transformed authority, prepared-plan/dependency hashes, resolver/native confinement, and context ownership; the core constructed `passed=true; exactAuthority=true`, returned that result, and the outer worker durably recorded `D_WORKER_RETURN`. The captured UIKit await continuation still never emitted, so the visible finalization timer continued despite a completed core result.

0.0.154 removes that dependency instead of wrapping it again. The Gate-D `Task.Run` await now uses `ConfigureAwait(false)` so completion resumes on the thread pool; only final UIKit mutations are explicitly marshaled with `InvokeOnMainThread`. Report teardown and `EndSteamOperation` likewise no longer require the captured UIKit continuation. Step 36 uses the durable exact Step-35 core-closure flag as its prerequisite rather than the historical UI gate snapshot, and its own Gate-D completion uses the same explicit main-queue pattern.

The same build retains **Step 36.0** as the next separately gated initialization boundary. It pins exact source `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization.ExecuteEssential()` at token `0x06007D03`, statically re-proves source/transformed semantic equality, requires state `1`, invokes exact transformed `ExecuteEssential` once on the main thread, requires state `2`, and finally re-proves OfflineReady/hashes/resolver/context isolation.

Still forbidden in Step 36: `ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry-point execution, Harmony/MonoMod runtime patching, arbitrary resolver fallback, and native game executable/library loading.

## Physical test sequence

Use a fresh process:

1. Step 15 Gates A-C.
2. Without force-quitting or backgrounding, Step 35 **EXACT-CLOSURE** once.
3. Wait for Step 35 to visibly finalize **4/4**.
4. Without force-quitting, press **Step 36.0 A-D** once.
5. Preserve Step35 and Step36 run-correlated checkpoint/static-map/report files.

Do not run diagnostic Step-35 modes before the exact sequence in the same process.
