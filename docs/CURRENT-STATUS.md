# Current Status — Step 35.0.2 ExecuteVeryEarly Invoke-Crash Static IL/Callsite Localization

## Active candidate — Step 35.0.2 / 0.0.125 (125)

Physical baseline summary: Steps 01–26 closed; Step 27 CLOSED NEGATIVE; Step 28 CLOSED POSITIVE 5/5; Step 29 CLOSED POSITIVE 4/4; Step 30 CLOSED POSITIVE 4/4; Step 31 CLOSED POSITIVE 4/4; **Step 32 CLOSED POSITIVE 4/4**; **Step 33 CLOSED POSITIVE 4/4**; **Step 34 CLOSED POSITIVE 4/4**. Step 35 remains **OPEN**.

Physical Step 32.0.5 / **0.0.120** closed the first real-StS2 semantic rewrite at 4/4. Exact transformed SHA-256 is `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, MVID `518e4758-52d7-47c2-b776-471a0e29e49d`, transformed `PrewarmJit` token `0x0600AFEA`, semantic fingerprint `47fadf2a46eda098f310b7d0ee54e37d1e952ac272fc966d16d557ed46a0b74a`, with zero remaining `RuntimeHelpers.PrepareMethod` references.

Physical Step 33.0 / **0.0.121** closed exact transformed-primary CLR admission at 4/4 with zero primary-admission managed/private/native resolution and no game-member invocation.

Physical Step 34.0 / **0.0.122** closed exact transformed `OneTimeInitialization::PrewarmJit()` execution at 4/4. It returned normally after 8 managed resolver requests: 6 exact host-framework resolutions and 2 hash-pinned initializer-free private dependency loads, with zero initializer-bearing, unplanned managed, or native escape.

## Step 35 physical evidence

Physical Step 35.0 / **0.0.123 (123)** hard-terminated while the UI still appeared near Gate B. Its matching iOS `.ips` reported `EXC_BAD_ACCESS / SIGKILL`, faulting main thread, program counter **0x0**, and no managed Step-35 report survived. That run alone could not prove whether the crash was Gate B or early synchronous Gate C.

Physical Step 35.0.1 / **0.0.124 (124)** resolved that ambiguity with durable checkpoints:

- Gate A passed.
- Gate B passed fully: fresh-process check, transformed hash rechecks, strict ALC construction, `LoadFromStream`, context ownership, identity, MVID, zero primary-admission resolver activity, global unique `sts2` residency, and private-context enumeration.
- Gate C entered on the main thread and bound exact transformed `ExecuteVeryEarly()`.
- reflected transformed token matched `0x0600AFE7`; MVID matched the closed transformed MVID.
- `C_INVOKE_START` was written for the first/only `MethodInfo.Invoke(null, null)`.
- during that invocation, planned resolution successfully loaded initializer-free `GodotSharp 4.5.1.0` and `Steamworks.NET 1.0.0.0` plus exact host framework bindings including `System.Runtime`, `System.Collections`, `System.Collections.Concurrent`, and `System.Text.Json`.
- the final durable event was a successful `System.Collections.Concurrent` host binding; `C_INVOKE_RETURNED` never occurred.
- the matching `.ips` again faults the main thread at PC=`0x0` and shows essentially the same runtime-heavy application stack shape as 0.0.123.

Therefore the current hard-termination frontier is **inside synchronous execution initiated by exact transformed `ExecuteVeryEarly()` MethodInfo.Invoke, after multiple planned resolver operations and before Invoke returns the Task**. Gate B is no longer suspect.

Running Step 34 and then Step 35 in the same process is expected to fail Step-35 Gate A normally because Step 34 leaves `sts2` resident in a non-collectible ALC. A fresh process is mandatory.

## Step 35.0.2 / 0.0.125 diagnostic candidate

Step 35.0.2 preserves the exact 0.0.123/0.0.124 compatibility experiment. It does not change transformed bytes, target method, resolver authority, invocation count, Task timeout, or forbidden boundaries.

Gate A still re-runs Step-32 A–D, pins source/transformed hashes/MVID, binds source `ExecuteVeryEarly` token `0x06007D02` and source async `<ExecuteVeryEarly>d__7::MoveNext` token `0x0600BC71`, proves source/transformed semantic equality, requires zero direct calls to `ExecuteEssential`/`ExecuteDeferred`/`PrewarmJit`, zero direct Harmony method references, and requalifies the prepared runtime plan.

New in 0.0.125: after those checks, Gate A builds an output-only static map from the exact verified transformed wrapper and MoveNext and writes `Documents/StS2Launcher/Reports/Step35-ExecuteVeryEarly-StaticMap.txt` before any CLR admission. The map contains each IL instruction/operand, metadata scope, numbered call/callvirt/newobj callsites, and `AWAIT-CANDIDATE` markers. It must not call Cecil `Resolve`, and it is never runtime input.

Gate B remains the exact physically proven 0.0.124 admission path. Gate C remains one exact reflected `ExecuteVeryEarly()` invocation and <=60-second await of the returned non-null Task. `Step35-CrashCheckpoint.txt` remains synchronously flushed runtime telemetry.

Cancellation is **CANCELLED = INCONCLUSIVE**, not PASS or compatibility FAIL. Once Gate B starts the process is spent; once Gate C invocation starts, cancellation cannot undo code already executed.

Step 35 still forbids intentional `ExecuteEssential`, `ExecuteDeferred`, launcher-driven `PrewarmJit`, the entry point, Harmony/MonoMod patching, Godot/game startup, initializer-bearing `0Harmony 2.4.2.0`, unplanned managed loading, and native loading.

## Immediate next evidence

Run exact 0.0.125 once from a fresh process. Preserve `Step35-ExecuteVeryEarly-StaticMap.txt`, `Step35-CrashCheckpoint.txt`, and the matching `.ips` after any hard termination. Correlate the runtime resolver frontier with the static callsite map, then design the smallest method/type/callsite discriminator rather than broadening execution authority.
