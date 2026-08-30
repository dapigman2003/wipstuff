# Testing — Step 35.0.6 In-Method Pre-First-Await Localization

Active candidate: Step 35.0.6 / `0.0.129 (129)`.

Physical baseline: Step 32 `0.0.120` CLOSED POSITIVE 4/4, Step 33 `0.0.121` CLOSED POSITIVE 4/4, Step 34 `0.0.122` CLOSED POSITIVE 4/4. Step 35 remains OPEN. Physical `0.0.124` proved Gate B PASS and localized the hard termination inside synchronous execution initiated by exact transformed `ExecuteVeryEarly()` `MethodInfo.Invoke`, after planned resolver activity but before `C_INVOKE_RETURNED`. Physical `0.0.125` repeated the same iOS hard-kill family but exposed an artifact-correlation gap. Physical `0.0.126` fixed that correlation defect: the manifest, last-checkpoint, run journal, and static map shared one Run ID/PID and the final durable event remained the planned `System.Collections.Concurrent 8.0.0.0 -> host 9.0.0.0` resolution with no `C_INVOKE_RETURNED`. Physical `0.0.127` and `0.0.128` then failed normally in Gate A before CLR admission on `System.Runtime 9.0.0.0`; 0.0.128 source analysis localized the repeat failure to `ReadingMode.Immediate` occurring before bounded writer-resolver configuration, so neither run changes the 0.0.126 game-runtime frontier.

Release identity: IPA `StS2-Launcher-Step-35.ipa`, TRX `step35.trx`, version `0.0.129 (129)`. The Codemagic workflow key remains `ios-canonical` so NuGet/Godot/iOS arm64 `obj`/AOT caches survive diagnostic revisions.

Running Step 34 and then Step 35 in the same process is invalid because Step 34 leaves `sts2` resident in a non-collectible private load context. Always force-quit before Step 35. Once Gate B begins, the 0.0.129 process is spent and must also be force-quit before another Step-35 run.

## Authority rule for 0.0.129

0.0.129 is a **diagnostic derivative**, not an exact Step-35 compatibility candidate. Gate A must re-create and verify the exact closed Step-32 transformed artifact, then create a separate instrumented clone. Gate B/C may CLR-admit and execute only that diagnostic clone. A 4/4 diagnostic result is useful localization evidence but **must not be recorded as Step-35 closure**. Exact Step-35 closure still requires a separately defined authoritative transformed artifact and physical acceptance contract.

## Host/static expectations

Static validation must protect the physically closed Step-32/33/34 manifests and the active Step-35.0.6 candidate manifest. Host tests protect ordered four-gate diagnostic completion, first-failure stopping, exact source target constants (`ExecuteVeryEarly` token `0x06007D02`, `<ExecuteVeryEarly>d__7::MoveNext` token `0x0600BC71`), initializer-free dependency admission, initializer-bearing dependency refusal, crash-checkpoint callback coverage, static-map callsite/await tagging, and the explicit diagnostic-only 4/4 summary.

The iOS source/static contract additionally requires:

- one immutable Run ID/PID created before Gate A;
- a unique `Step35-CrashCheckpoint-<RunId>.txt` journal;
- a unique `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt` map;
- `Step35-CurrentRun.txt` naming both exact files;
- independently flushed `Step35-LastCheckpoint.txt` updates;
- visible stop before Gate A if the initial journal cannot be durably established;
- visible diagnostic stop before Gate B if the same-run static map cannot be durably established;
- an exact transformed-source hash recheck immediately after diagnostic-clone emission and again before Gate-B admission;
- diagnostic-clone identity/MVID/signature/hash verification before CLR admission;
- no active UI/report text that describes a 0.0.129 diagnostic 4/4 as exact Step-35 PASS or closure.

## Diagnostic-output contract

All Step-35 telemetry is output-only and never trusted as runtime input. Every run-specific artifact records the same Run ID and PID. `Step35-CurrentRun.txt` is the correlation manifest. `Step35-LastCheckpoint.txt` is a convenience copy of the most recently durably written checkpoint for the current run; the run-specific journal remains the append history.

The run-specific static map must be written after Gate-A semantic verification and before Gate B. It is derived from the exact verified transformed wrapper/MoveNext using metadata-only Cecil objects without dependency resolution and records IL instructions/operands, metadata scopes, numbered call/callvirt/newobj callsites, and Async*MethodBuilder await-registration candidates.

For 0.0.129, the new decisive evidence is `C_DIAGNOSTIC_BRIDGE_ARMED` followed by zero or more durable `INMETHOD_*` markers emitted from the diagnostic clone. The last durable `INMETHOD_*` record identifies the last selected game method/type initializer entered before termination. If no `INMETHOD_*` appears after `C_DIAGNOSTIC_BRIDGE_ARMED`, the callback/bridge or first instrumented entry becomes the immediate frontier.

Telemetry creation failures are diagnostic stops, not compatibility FAILs. The candidate intentionally refuses to spend a physical execution run without same-run evidence.

## Physical Gate A — VerifiedExecutionPreflight

Require a fresh process with no resident `sts2`. Before Gate A, require successful durable creation of the run-specific journal/current-run/last-checkpoint set. Then re-run the physically closed Step-32 transform A–D. Require exact source SHA-256 `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`, transformed SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, transformed length 9,304,576, identity/MVID, source `ExecuteVeryEarly` token `0x06007D02`, exact static parameterless Task signature, source/transformed wrapper + MoveNext semantic equality, source MoveNext token `0x0600BC71`, zero direct later-boundary calls, zero direct Harmony method references, and zero Cecil dependency resolution.

After those exact checks, open the exact transformed module for diagnostic-clone creation using **Cecil `ReadingMode.Deferred` only**. Require zero resolver requests from the initial open, then audit/configure the bounded writer-only surrogate resolver before any serialization. `ReadingMode.Immediate` is forbidden here because physical 0.0.128 proved it can resolve `System.Runtime` before configuration. Then create `sts2.step35.0.6.instrumented.dll` in the launcher-private diagnostic workspace. The clone may add only the Step-35 diagnostic bridge and selected `INMETHOD_*` entry checkpoints; it must preserve assembly identity and MVID and retain the target signature. Immediately re-hash the exact transformed source after clone emission and require it to remain byte-identical. Requalify the persisted zero-blocker prepared plan and exact sole initializer-bearing `0Harmony 2.4.2.0` dependency. Then require a durably written same-Run-ID static map before any CLR admission.

## Physical Gate B — ExecutionCapableClrAdmission

Immediately re-hash **both** the exact closed transformed source and the diagnostic clone. The exact transformed source must remain unchanged and must not be CLR-loaded in 0.0.129. `LoadFromStream` only the separately verified diagnostic clone into `StS2Launcher-Step35-VeryEarly`; require preserved assembly identity/MVID/context ownership, unique resident `sts2`, and zero managed/private/initializer/rejected/native resolution during primary admission.

This reproduces the Step-33 *admission behavior* for the derivative; it is not execution of the exact Step-33/Step-35 transformed bytes.

## Physical Gate C — DiagnosticExecuteVeryEarlyInvocation

Reflect only the admitted diagnostic clone's `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()`. Require static, parameterless, exact `System.Threading.Tasks.Task` return, Gate-A-discovered diagnostic token, and preserved closed MVID. Bind the diagnostic bridge field as exact `Action<string>`, assign the launcher-owned durable checkpoint callback, and write `C_DIAGNOSTIC_BRIDGE_ARMED` before invocation.

Invoke exactly once and require a non-null Task if control returns; await for at most 60 seconds. Resolver authority remains limited to exact persisted host-framework bindings and exact hash-pinned initializer-free prepared private dependencies. Any initializer-bearing, unplanned managed, or native request fails closed.

The marker set covers `ExecuteVeryEarly.MoveNext`, `TestMode.get_IsOn`, the relevant `SaveManager` entry methods, `OneTimeInitialization.set_SettingsReadResult`, `ModManagerFileIo..ctor`, settings/version getters, `ModManager.Initialize`, and managed-IL static constructors on those selected declaring types when present. The final durable marker is localization evidence only.

Operator cancellation is **INCONCLUSIVE**, not PASS or compatibility FAIL. Once Gate B starts, force-quit before retry.

## Physical Gate D — FinalIsolationAudit

If Gate C returns successfully, re-prove OfflineReady, receipt-backed source SHA-256, exact transformed-source SHA-256, diagnostic-clone SHA-256, runtime-plan SHA-256, every resident private dependency hash, unique diagnostic-clone residency/context ownership, zero initializer-bearing/unplanned/native escape, and exactly one diagnostic `ExecuteVeryEarly` invocation. The launcher must not intentionally invoke `ExecuteEssential`, `ExecuteDeferred`, `PrewarmJit`, the entry point, Harmony APIs, or Godot/game startup.

A Gate-D 4/4 result means **Step 35.0.6 diagnostic localization completed 4/4; Step 35 remains OPEN**. Do not convert this derivative result into exact Step-35 closure evidence.

After a hard termination preserve `Step35-CurrentRun.txt`, `Step35-LastCheckpoint.txt`, the run-specific journal and static map named by the manifest, the normal Step-35 report when present, and the matching `.ips`. Do not combine artifacts from different Run IDs/PIDs.
