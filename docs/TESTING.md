# Testing — Step 35.0.3 Run-Correlated Durable Telemetry

Active candidate: Step 35.0.3 / `0.0.126 (126)`.

Physical baseline: Step 32 `0.0.120` CLOSED POSITIVE 4/4, Step 33 `0.0.121` CLOSED POSITIVE 4/4, Step 34 `0.0.122` CLOSED POSITIVE 4/4. Step 35 remains OPEN. Physical `0.0.124` proved Gate B PASS and localized the hard termination inside synchronous execution initiated by exact transformed `ExecuteVeryEarly()` `MethodInfo.Invoke`, after planned resolver activity but before `C_INVOKE_RETURNED`. Physical `0.0.125` repeated the same iOS hard-kill family but exposed an artifact-correlation gap.

Release identity: IPA `StS2-Launcher-Step-35.ipa`, TRX `step35.trx`, version `0.0.126 (126)`. The Codemagic workflow key remains `ios-canonical` so NuGet/Godot/iOS arm64 `obj`/AOT caches survive diagnostic revisions.

Running Step 34 and then Step 35 in the same process is expected to fail Gate A normally because Step 34 leaves `sts2` resident in a non-collectible private load context. Always force-quit before Step 35.

## Host/static expectations

Static validation must protect the physically closed Step-32/33/34 manifests and the active Step-35.0.3 candidate manifest. Host tests continue to protect ordered four-gate completion, first-failure stopping, exact source target constants (`ExecuteVeryEarly` token `0x06007D02`, `<ExecuteVeryEarly>d__7::MoveNext` token `0x0600BC71`), initializer-free dependency admission, initializer-bearing dependency refusal, crash-checkpoint callback coverage, and static-map callsite/await tagging.

The iOS source/static contract additionally requires:

- one immutable Run ID/PID created before Gate A;
- a unique `Step35-CrashCheckpoint-<RunId>.txt` journal;
- a unique `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt` map;
- `Step35-CurrentRun.txt` naming both exact files;
- independently flushed `Step35-LastCheckpoint.txt` updates;
- visible stop before Gate A if the initial journal cannot be durably established;
- visible diagnostic stop before Gate B if the same-run static map cannot be durably established.

## Diagnostic-output contract

All Step-35 telemetry is output-only and never trusted as runtime input. Every run-specific artifact records the same Run ID and PID. `Step35-CurrentRun.txt` is the authoritative correlation manifest. `Step35-LastCheckpoint.txt` is a convenience copy of the most recently durably written checkpoint for the current run; the run-specific journal remains the append history.

The run-specific static map must be written after Gate-A semantic verification and before Gate B. It is derived from the exact verified transformed wrapper/MoveNext using metadata-only Cecil objects without dependency resolution and records IL instructions/operands, metadata scopes, numbered call/callvirt/newobj callsites, and Async*MethodBuilder await-registration candidates.

Telemetry creation failures are not compatibility FAILs. The candidate intentionally refuses to spend a physical execution run without same-run evidence.

## Physical Gate A — VerifiedExecutionPreflight

Require a fresh process with no resident `sts2`. Before Gate A, require successful durable creation of the run-specific journal/current-run/last-checkpoint set. Then re-run the physically closed Step-32 transform A–D. Require exact source SHA-256 `e7ceb80669bfaf5c8fccabaa126ae2bb283aba514be5b5b55612579cfd285f18`, transformed SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, transformed length 9,304,576, identity/MVID, source `ExecuteVeryEarly` token `0x06007D02`, exact static parameterless Task signature, source/transformed wrapper + MoveNext semantic equality, source MoveNext token `0x0600BC71`, zero direct later-boundary calls, zero direct Harmony method references, and zero Cecil dependency resolution. Requalify the persisted zero-blocker prepared plan and exact sole initializer-bearing `0Harmony 2.4.2.0` dependency. Then require a durably written same-Run-ID static map before any CLR admission.

## Physical Gate B — ExecutionCapableClrAdmission

Reproduce the physically proven 0.0.124 path: immediately re-hash and `LoadFromStream` only the exact transformed primary into `StS2Launcher-Step35-VeryEarly`; require exact identity/MVID/context ownership, unique resident `sts2`, and zero managed/private/initializer/rejected/native resolution during primary admission.

## Physical Gate C — ExactExecuteVeryEarlyInvocation

Reflect only exact transformed `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()`. Require static, parameterless, exact `System.Threading.Tasks.Task` return, Gate-A-discovered transformed token and exact MVID. Invoke exactly once and require a non-null Task; await for at most 60 seconds. Resolver authority remains limited to exact persisted host-framework bindings and exact hash-pinned initializer-free prepared private dependencies. Any initializer-bearing, unplanned managed, or native request fails closed.

Physical 0.0.124 reached this gate, entered `MethodInfo.Invoke`, successfully loaded planned `GodotSharp`/`Steamworks.NET` and host frameworks, and then hard-terminated before Invoke returned. Physical 0.0.125 repeated the same native failure family but did not provide a same-run journal/map pair.

Operator cancellation is **INCONCLUSIVE**, not PASS or FAIL. Once Gate B starts, force-quit before retry.

## Physical Gate D — FinalIsolationAudit

If Gate C ever returns successfully, re-prove OfflineReady 428/428, receipt-backed source SHA-256, transformed SHA-256, runtime-plan SHA-256, every resident private dependency hash, unique transformed-primary residency/context ownership, zero initializer-bearing/unplanned/native escape, and exactly one Step-35 `ExecuteVeryEarly` invocation. The launcher must not intentionally invoke `ExecuteEssential`, `ExecuteDeferred`, `PrewarmJit`, the entry point, Harmony APIs, or Godot/game startup.

After a hard termination preserve `Step35-CurrentRun.txt`, `Step35-LastCheckpoint.txt`, the run-specific journal and static map named by the manifest, the normal Step-35 report when present, and the matching `.ips`. Accept physical closure only on ordered A–D **4/4 PASS**.
