# Current Status — Step 35.0.5 In-Method Pre-First-Await Localization

## Active candidate — Step 35.0.5 / 0.0.128 (128)

Steps 01–26 are closed; Step 27 is CLOSED NEGATIVE; Step 28 is CLOSED POSITIVE 5/5; Steps 29–34 are CLOSED POSITIVE 4/4. Step 35 remains OPEN.

Milestones: **Step 32 CLOSED POSITIVE 4/4 • Step 33 CLOSED POSITIVE 4/4 • Step 34 CLOSED POSITIVE 4/4 • Step 35 OPEN**.

Physical 0.0.124 proved Gate B PASS and localized the hard termination inside synchronous execution started by exact transformed `OneTimeInitialization.ExecuteVeryEarly()` `MethodInfo.Invoke`, after planned `GodotSharp`, `Steamworks.NET`, and framework resolutions but before `C_INVOKE_RETURNED`. The matching iOS family faults the main thread at program counter **0x0** with `CODESIGNING / Invalid Page`. Physical 0.0.125 reproduced that family but exposed cross-run telemetry correlation.

Physical **0.0.126** fixed correlation: `Step35-CurrentRun.txt`, `Step35-LastCheckpoint.txt`, `Step35-CrashCheckpoint-<RunId>.txt`, and `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt` all shared the same Run ID/PID. The exact run reached Gate B PASS, entered Gate C, and the last durable event was the planned `System.Collections.Concurrent, Version=8.0.0.0` → host 9.0.0.0 resolution; there was still no `C_INVOKE_RETURNED`.

Physical **0.0.127** did **not** crash and did not reach the game boundary. It failed closed at Gate A during diagnostic-clone instrumentation with `AssemblyResolutionException` for exact `System.Runtime, Version=9.0.0.0`. Durable telemetry shows `A_RESULT passed=False` followed by the normal report/finally/`RUN_END` path. No Gate B admission or Gate C invocation occurred. This reproduces the Cecil writer-only constant-metadata resolution family already solved during Step 32; it does not change the 0.0.126 runtime frontier.

The verified static map keeps the exact source `ExecuteVeryEarly` token `0x06007D02` and source async `<ExecuteVeryEarly>d__7::MoveNext` token `0x0600BC71`. Its normal pre-first-await path is `TestMode.get_IsOn` → `SaveManager.InitSettingsData` → `ModManagerFileIo`/settings/version getters → `ModManager.Initialize` → first await. Resolver traffic cannot identify which of those game methods is the hard-kill frontier.

## 0.0.128 diagnostic change

Gate A re-runs the closed Step-32 transform and exact semantic checks, then emits a separate diagnostic clone. For clone serialization only, it uses the same exact audited in-memory `System.Runtime` + `Sentry` constant-metadata surrogate model physically proven by Step 32; every unapproved resolution still fails closed and no external assembly bytes are opened. The exact transformed source remains byte-identical and is never overwritten. After serialization, the clone is reopened with the rejecting resolver and its constant-metadata fingerprint must match the pre-write transformed image before Gate A can pass. The clone preserves assembly identity/MVID and injects a tiny bridge plus durable `INMETHOD_*` entry markers into `ExecuteVeryEarly.MoveNext`, the top-level pre-first-await callees, and relevant type initializers. Gate B admits only that diagnostic clone. Gate C reflects the same static parameterless Task-returning `ExecuteVeryEarly`, arms the bridge callback, and performs one invocation/await under the same strict prepared resolver and 60-second boundary.

`ExecuteEssential`, `ExecuteDeferred`, the game entry point, Harmony/MonoMod runtime patching, native game loading, and Godot/game startup remain forbidden. Cancellation remains INCONCLUSIVE and requires a fresh process.

On the next physical hard termination, the final durable `INMETHOD_*` checkpoint is the primary evidence for the next localization step.

## 0.0.128 evidence semantics

A successful 0.0.128 A–D run must be reported as **Step 35.0.5 diagnostic localization complete 4/4 — NOT Step 35 closure**. The instrumented clone may identify the active pre-first-await method/type-initializer frontier, but it is not byte-identical to the closed Step-32 transformed SHA-256. After localization, any compatibility correction must return to an explicitly defined authoritative transformed artifact and re-establish a physical closure contract.
