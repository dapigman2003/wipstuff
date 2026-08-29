# StS2 Launcher iOS — Step 35.0.2 ExecuteVeryEarly Invoke-Crash Static IL/Callsite Localization

Steps 01–26 are physically closed. Step 27 is CLOSED NEGATIVE for runtime Harmony/MonoMod replacement. Steps 28–34 are CLOSED POSITIVE. Physical `0.0.122` closed Step 34 at 4/4 by invoking the exact transformed `OneTimeInitialization::PrewarmJit()` once on iPhone under the strict prepared resolver.

## Active candidate

**Step 35.0.2 / `0.0.125 (125)` — unchanged Step-35 ExecuteVeryEarly experiment plus pre-CLR static IL/callsite evidence**

Physical `0.0.124 (124)` resolved the prior B→C ambiguity. Gate A passed. Gate B passed end-to-end, including exact transformed hash, private `AssemblyLoadContext.LoadFromStream`, identity/MVID, zero primary-admission resolver activity, and unique transformed-primary residency. Gate C then bound exact transformed `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()`, entered the first and only `MethodInfo.Invoke(null, null)`, successfully serviced planned `GodotSharp`, `Steamworks.NET`, `System.Runtime`, `System.Collections`, `System.Collections.Concurrent`, and `System.Text.Json` resolution, and hard-terminated before `C_INVOKE_RETURNED`. The matching iOS `.ips` repeats the main-thread PC=`0x0` crash signature and effectively the same runtime-heavy stack shape as `0.0.123`.

Step 35 therefore remains **OPEN**. Nothing in `0.0.125` broadens compatibility authority. The exact Step-32 transformed hash/MVID, target tokens, strict resolver, one `ExecuteVeryEarly()` invocation, <=60-second returned-Task await, initializer-bearing `0Harmony` refusal, unplanned managed/native refusal, and later-startup prohibitions remain unchanged.

The added diagnostic is `Documents/StS2Launcher/Reports/Step35-ExecuteVeryEarly-StaticMap.txt`. Gate A generates it from the already-verified transformed `ExecuteVeryEarly` wrapper and `<ExecuteVeryEarly>d__7::MoveNext` before any CLR admission. It records exact IL instructions, metadata operands/scopes, numbered `call`/`callvirt`/`newobj` callsites, and async-await registration candidates without Cecil dependency resolution. It is output-only and never trusted as runtime input.

`Step35-CrashCheckpoint.txt` remains the durable runtime frontier log. Preserve both files after an abrupt termination. Cancellation remains **INCONCLUSIVE**; once Gate B/C begins, force-quit before retry.

The stable Codemagic workflow remains `ios-canonical`; `MtouchLink=None`, `TrimMode=copy`, and `MtouchInterpreter=-all` remain unchanged.
