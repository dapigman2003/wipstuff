# StS2 Launcher iOS — Step 35.0 Controlled Transformed Real-StS2 Very-Early Initialization

Steps 01–26 are physically closed. Step 27 is CLOSED NEGATIVE for runtime Harmony/MonoMod replacement. Steps 28–34 are CLOSED POSITIVE. Physical `0.0.122` closed Step 34 at 4/4 by invoking the exact transformed `OneTimeInitialization::PrewarmJit()` once on iPhone under the strict prepared resolver; it returned normally with 8 managed resolver requests (6 exact host-framework loads + 2 initializer-free prepared private loads), zero initializer-bearing/unplanned/native escape, and no entry-point or Godot startup.

## Active candidate

**Step 35.0 / `0.0.123 (123)` — exact transformed `OneTimeInitialization::ExecuteVeryEarly()` once, then await its returned Task**

Step 35 re-manufactures/reverifies the exact physically closed Step-32 transformed image (SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, MVID `518e4758-52d7-47c2-b776-471a0e29e49d`) and requalifies the existing prepared runtime plan. Static inspection of the exact receipt-backed source identifies `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()` as source MethodDef `0x06007D02`, static/parameterless and returning `System.Threading.Tasks.Task`, with async state-machine `<ExecuteVeryEarly>d__7::MoveNext` at source token `0x0600BC71`.

Gate A independently proves the Step-32 serialization did not change the `ExecuteVeryEarly` wrapper or its async `MoveNext` semantics and that the `MoveNext` contains no direct call to the later `ExecuteEssential`, `ExecuteDeferred`, or `PrewarmJit` boundaries and no direct Harmony method reference. Gate B admits only the exact transformed primary into `StS2Launcher-Step35-VeryEarly`. Gate C reflects only exact transformed `ExecuteVeryEarly()`, invokes it once, and awaits the returned `Task` for at most 60 seconds. Exact persisted host-framework bindings and hash-pinned initializer-free prepared private dependencies may resolve; initializer-bearing `0Harmony` 2.4.2.0, unplanned managed resolution, and native loading remain fail-closed. Gate D re-proves source/transformed/plan/dependency/context isolation.

The stable Codemagic workflow remains `ios-canonical`; NuGet, Godot, and iOS arm64 publish/AOT intermediates remain cached. `MtouchLink=None`, `TrimMode=copy`, and `MtouchInterpreter=-all` remain unchanged.
