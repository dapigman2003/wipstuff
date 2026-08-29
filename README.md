# StS2 Launcher iOS — Step 35.0.3 Run-Correlated Durable Telemetry

Steps 01–26 are physically closed. Step 27 is CLOSED NEGATIVE for runtime Harmony/MonoMod replacement. Steps 28–34 are CLOSED POSITIVE. Physical `0.0.122` closed Step 34 at 4/4 by invoking exact transformed `OneTimeInitialization::PrewarmJit()` once under the strict prepared resolver.

## Active candidate

**Step 35.0.3 / `0.0.126 (126)` — unchanged Step-35 ExecuteVeryEarly experiment with fail-visible same-run diagnostics**

Physical `0.0.124 (124)` proved Gate B PASS and localized the Step-35 hard termination inside synchronous execution initiated by exact transformed `ExecuteVeryEarly()` `MethodInfo.Invoke`, after planned `GodotSharp`, `Steamworks.NET`, and host-framework resolution but before `C_INVOKE_RETURNED`. Physical `0.0.125 (125)` reproduced the same main-thread PC=`0x0`, `CODESIGNING / Invalid Page` failure family. However, the available 0.0.125 static map was generated before the matching crash-report process launched, and no fixed-name crash checkpoint from that process was available.

Step 35 therefore remains **OPEN**. `0.0.126` does not broaden compatibility authority. It assigns one immutable Run ID/PID before Gate A and durably creates `Step35-CurrentRun.txt`, `Step35-LastCheckpoint.txt`, `Step35-CrashCheckpoint-<RunId>.txt`, and later `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`. Gate A is refused if the run journal cannot be established; Gate B is refused if the same-run static map cannot be durably written. Those are diagnostic stops, not compatibility failures.

The exact transformed bytes, target tokens, strict resolver, one `ExecuteVeryEarly()` invocation, <=60-second returned-Task await, initializer-bearing `0Harmony` refusal, unplanned managed/native refusal, and later-startup prohibitions remain unchanged.

The stable Codemagic workflow remains `ios-canonical`; `MtouchLink=None`, `TrimMode=copy`, and `MtouchInterpreter=-all` remain unchanged.
