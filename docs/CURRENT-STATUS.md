# Current Status — Step 35.0.3 Run-Correlated Durable Telemetry

## Active candidate — Step 35.0.3 / 0.0.126 (126)

Physical baseline summary: Steps 01–26 closed; Step 27 CLOSED NEGATIVE; Step 28 CLOSED POSITIVE 5/5; Step 29 CLOSED POSITIVE 4/4; Step 30 CLOSED POSITIVE 4/4; Step 31 CLOSED POSITIVE 4/4; **Step 32 CLOSED POSITIVE 4/4**; **Step 33 CLOSED POSITIVE 4/4**; **Step 34 CLOSED POSITIVE 4/4**. Step 35 remains **OPEN**.

Physical Step 32.0.5 / **0.0.120** closed the first real-StS2 semantic rewrite at 4/4. Exact transformed SHA-256 is `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, MVID `518e4758-52d7-47c2-b776-471a0e29e49d`, transformed `PrewarmJit` token `0x0600AFEA`, semantic fingerprint `47fadf2a46eda098f310b7d0ee54e37d1e952ac272fc966d16d557ed46a0b74a`, with zero remaining `RuntimeHelpers.PrepareMethod` references.

Physical Step 33.0 / **0.0.121** closed exact transformed-primary CLR admission at 4/4 with zero primary-admission managed/private/native resolution and no game-member invocation.

Physical Step 34.0 / **0.0.122** closed exact transformed `OneTimeInitialization::PrewarmJit()` execution at 4/4. It returned normally after 8 managed resolver requests: 6 exact host-framework resolutions and 2 hash-pinned initializer-free private dependency loads, with zero initializer-bearing, unplanned managed, or native escape.

## Step 35 physical evidence

Physical Step 35.0 / **0.0.123 (123)** hard-terminated near the visible B→C boundary. Its matching iOS `.ips` reported `EXC_BAD_ACCESS / SIGKILL`, faulting main thread, program counter **0x0**, and no managed Step-35 report.

Physical Step 35.0.1 / **0.0.124 (124)** resolved that ambiguity. Gate B passed fully. Gate C bound exact transformed `ExecuteVeryEarly()`, wrote `C_INVOKE_START`, successfully serviced planned `GodotSharp 4.5.1.0`, `Steamworks.NET 1.0.0.0`, and host-framework resolutions, and then hard-terminated before `C_INVOKE_RETURNED`. Its `.ips` repeated the main-thread PC=`0x0` failure family. This remains the authoritative runtime frontier: **inside synchronous execution initiated by exact transformed `ExecuteVeryEarly()` MethodInfo.Invoke, after planned resolution and before Invoke returns the Task**.

Physical Step 35.0.2 / **0.0.125 (125)** reproduced the same `EXC_BAD_ACCESS / SIGKILL`, `KERN_PROTECTION_FAILURE at 0x0`, `CODESIGNING / Invalid Page`, faulting-main-thread, PC=`0x0` family. The available 0.0.125 static map, however, was generated at 01:31:19 -0500 while the attached crash-report process launched at 01:37:50 -0500. Those artifacts were from different runs. No fixed-name `Step35-CrashCheckpoint.txt` from the crash-report process was available. Therefore 0.0.125 confirms repeatability of the native failure family but does not safely advance callsite localization.

Running Step 34 and then Step 35 in the same process remains invalid because Step 34 leaves `sts2` resident in a non-collectible ALC. A fresh process is mandatory.

## Step 35.0.3 / 0.0.126 diagnostic candidate

Step 35.0.3 preserves the exact 0.0.125 compatibility experiment: same transformed bytes, exact source `ExecuteVeryEarly` token `0x06007D02`, source async `<ExecuteVeryEarly>d__7::MoveNext` token `0x0600BC71`, same strict private ALC/resolver policy, one exact reflected invocation, <=60-second Task await, and the same forbidden boundaries.

The change is diagnostic provenance. Before Gate A, 0.0.126 creates one immutable Run ID containing UTC/PID/GUID and durably establishes:

- `Documents/StS2Launcher/Reports/Step35-CurrentRun.txt` — manifest naming the exact same-run artifact files;
- `Documents/StS2Launcher/Reports/Step35-LastCheckpoint.txt` — independently flushed overwrite-on-each-event convenience record;
- `Documents/StS2Launcher/Reports/Step35-CrashCheckpoint-<RunId>.txt` — run-specific append journal;
- `Documents/StS2Launcher/Reports/Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt` — same-run static wrapper/MoveNext IL map written after Gate-A verification and before Gate B.

If the initial run journal cannot be created/flushed, the UI reports `TELEMETRY FAIL / NOT RUN` and Gate A is not entered. If Gate A passes but the same-run static map cannot be durably written, execution stops before Gate B. These are diagnostic stops, not compatibility failures.

Cancellation remains **CANCELLED = INCONCLUSIVE**. `ExecuteEssential`, `ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry-point execution, Harmony/MonoMod patching, initializer-bearing `0Harmony 2.4.2.0`, unplanned managed/native loading, and Godot/game startup remain forbidden.

## Immediate next evidence

Run exact 0.0.126 once from a fresh process. After a hard termination, preserve `Step35-CurrentRun.txt`, `Step35-LastCheckpoint.txt`, the exact run-specific crash journal and static map named by the manifest, and the matching `.ips` before any rerun. Only same-Run-ID evidence should be used to choose the next pre-first-await callsite discriminator.
