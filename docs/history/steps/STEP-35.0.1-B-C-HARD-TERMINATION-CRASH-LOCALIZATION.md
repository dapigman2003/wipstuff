# Step 35.0.1 — B→C Hard-Termination Crash Localization

Candidate: **0.0.124 (124)**

## Trigger

Physical Step 35.0 / 0.0.123 opened on iPhone, reached the visible Gate-B region, and then terminated abruptly. No managed Step-35 report survived. A matching iOS `.ips` identifies the exact build and records `EXC_BAD_ACCESS / SIGKILL`, faulting main thread, PC=`0x0`, with `CODESIGNING / Invalid Page` termination text.

The screen observation is insufficient to assign the crash to Gate B. Gate B runs inside `Task.Run`; after its await completes, the continuation resumes on the main thread, changes the UI to Gate C, and immediately enters synchronous Gate-C reflection/invocation work. UIKit is not guaranteed to repaint before that synchronous prefix. The crash report's main-thread fault therefore makes early Gate C at least as important a hypothesis as Gate-B `LoadFromStream`.

## Scope

No compatibility behavior is broadened or relaxed. 0.0.124 preserves:

- exact Step-32 transformed bytes/hash/MVID;
- exact Step-35 `ExecuteVeryEarly` source target and async-state-machine audits;
- transformed-primary-only Gate-B CLR admission;
- exact-plan managed resolver and initializer-bearing/native refusal;
- exactly one `MethodInfo.Invoke(null, null)` for `ExecuteVeryEarly`;
- non-null Task requirement and <=60-second await;
- Step-35 forbidden later startup/Harmony/Godot/native boundaries.

Only observability changes.

## Durable checkpoint design

`Documents/StS2Launcher/Reports/Step35-CrashCheckpoint.txt` is reset at the start of a valid fresh-process run and synchronously flushed after each record. The header pins actual/expected app version/build and the Step-35 implementation marker. Each progress line carries UTC timestamp, process ID, managed thread ID, and one frontier marker.

Key Gate-B markers include fresh-process pass, transformed hash rechecks, ALC construction, `B_LOADFROMSTREAM_START/PASS`, identity/MVID checks, zero-resolution proof, residency checks, `B_PASS_RETURN`, and UI-side `B_TASK_AWAIT_RESUMED`.

Key Gate-C markers include `C_UI_SELECTED`, type/method binding starts/returns, signature/token/MVID proof, `C_INVOKE_START`, `C_INVOKE_RETURNED`, `C_TASK_CONFIRMED`, `C_WAIT_START/COMPLETED`, post-await confinement, and Gate-C PASS return. Resolver callbacks emit `RESOLVE_*` records around planned host/private resolution and rejected initializer-bearing/unplanned/native requests.

Checkpoint writing is output-only. Errors are printed to stderr and never alter compatibility/resolver decisions.

## Cancellation semantics

Operator cancellation is **INCONCLUSIVE**, not a compatibility FAIL. A cancelled run does not demonstrate either compatibility or incompatibility. Once Gate B has started, the process is still spent. Once `C_INVOKE_START` has occurred, cancellation cannot undo target code that may already have executed.

## Physical acceptance

Run only from a fresh process. If the app hard-terminates, preserve `Step35-CrashCheckpoint.txt` and the matching OS crash report before any retry. The last durable checkpoint is the next evidence boundary. Do not change resolver/native/startup authority until that boundary is localized.

Step 35 closes only on ordered Gates A–D **4/4 PASS**. A diagnostic hard-termination localization is evidence, not closure.
