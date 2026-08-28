# StS2 Launcher iOS — Step 35.0.1 Very-Early B→C Hard-Termination Crash Localization

Steps 01–26 are physically closed. Step 27 is CLOSED NEGATIVE for runtime Harmony/MonoMod replacement. Steps 28–34 are CLOSED POSITIVE. Physical `0.0.122` closed Step 34 at 4/4 by invoking the exact transformed `OneTimeInitialization::PrewarmJit()` once on iPhone under the strict prepared resolver; it returned normally with 8 managed resolver requests (6 exact host-framework loads + 2 initializer-free prepared private loads), zero initializer-bearing/unplanned/native escape, and no entry-point or Godot startup.

## Active candidate

**Step 35.0.1 / `0.0.124 (124)` — unchanged Step-35 ExecuteVeryEarly experiment with durable B→C crash localization**

Physical Step 35.0 / `0.0.123 (123)` was attempted on iPhone. The app hard-terminated around the visible Gate-B region and produced no managed Step-35 report. The matching iOS `.ips` identifies the exact 0.0.123 build and reports `EXC_BAD_ACCESS / SIGKILL`, faulting main thread, program counter `0x0`, with `CODESIGNING / Invalid Page` termination text. Because the Gate-B work itself runs on `Task.Run` and UIKit may still display “Gate B” after that await has resumed while synchronous Gate-C reflection/invocation is already executing on the main thread, the crash is not yet proven to be inside Gate B.

Step 35.0.1 does **not** broaden compatibility authority. It preserves the exact Step-35 static target, transform, strict resolver, transformed-primary admission, one `ExecuteVeryEarly()` invocation, and <=60-second Task await. It adds synchronously flushed, output-only `Step35-CrashCheckpoint.txt` telemetry around Gate B, the B→C handoff, exact type/method binding, `MethodInfo.Invoke`, Task await, and managed/native resolver callbacks. Each record carries UTC time, process ID, managed thread ID, and source-pinned release identity so a native/runtime hard kill can be localized even when managed `catch/finally` never runs.

Gate A still re-manufactures/reverifies the exact physically closed Step-32 transformed image (SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, MVID `518e4758-52d7-47c2-b776-471a0e29e49d`) and requalifies the existing prepared runtime plan. Gate B admits only the exact transformed primary into `StS2Launcher-Step35-VeryEarly`. Gate C reflects exact transformed `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()` (source token `0x06007D02`; async `<ExecuteVeryEarly>d__7::MoveNext` source token `0x0600BC71`), invokes it once, and awaits its returned Task for at most 60 seconds. Gate D re-proves source/transformed/plan/dependency/context isolation.

Cancellation is recorded as **INCONCLUSIVE**, not a compatibility FAIL. Once Gate B has begun the process is spent and must be force-quit before retry; once Gate C invocation has begun, cancellation cannot undo code already executed.

The stable Codemagic workflow remains `ios-canonical`; NuGet, Godot, and iOS arm64 publish/AOT intermediates remain cached. `MtouchLink=None`, `TrimMode=copy`, and `MtouchInterpreter=-all` remain unchanged.
