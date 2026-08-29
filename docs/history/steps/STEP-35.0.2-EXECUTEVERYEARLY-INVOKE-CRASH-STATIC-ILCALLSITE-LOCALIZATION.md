# Step 35.0.2 — ExecuteVeryEarly Invoke-Crash Static IL/Callsite Localization

Candidate: **0.0.125 (125)**

## Evidence entering this revision

Physical 0.0.124 closed the B→C ambiguity. Gate B passed fully. Gate C bound exact transformed `OneTimeInitialization::ExecuteVeryEarly()`, entered its first and only `MethodInfo.Invoke(null, null)`, successfully serviced planned `GodotSharp`, `Steamworks.NET` and host-framework resolutions, and then hard-terminated before `C_INVOKE_RETURNED`. The matching iOS crash report repeats the main-thread PC=`0x0` signature and essentially the same runtime stack shape as 0.0.123.

Therefore this revision does **not** modify resolver authority, transformed bytes, target method, invocation count, timeout, or forbidden boundaries.

## Diagnostic addition

Gate A already reopens the exact source/transformed images with rejecting Cecil resolvers and verifies semantic equality for the `ExecuteVeryEarly` wrapper and `<ExecuteVeryEarly>d__7::MoveNext`. Step 35.0.2 uses those already-bound transformed `MethodDefinition` objects to build an output-only static instruction map before any CLR admission.

`Documents/StS2Launcher/Reports/Step35-ExecuteVeryEarly-StaticMap.txt` records:

- exact wrapper and MoveNext tokens/full names;
- instruction, local and exception-handler counts;
- every IL instruction and operand;
- metadata scope for method/field/type operands without calling Cecil `Resolve`;
- numbered `call`/`callvirt`/`newobj` callsites;
- `AWAIT-CANDIDATE` tags on Async*MethodBuilder await-registration calls.

The map is diagnostic output only, is never consumed as trusted runtime input, and write failure must not alter compatibility decisions.

## Execution policy

After the map is written, Gates B–D remain the exact Step-35 experiment proven by 0.0.123/0.0.124:

- exact Step-32 transformed hash/MVID and semantic evidence;
- strict transformed-primary private ALC;
- exact prepared host/private resolver policy;
- initializer-bearing `0Harmony 2.4.2.0` remains forbidden;
- one exact reflected `ExecuteVeryEarly()` invocation;
- non-null exact `Task` required and awaited for at most 60 seconds;
- unplanned managed/native resolution remains fail-closed;
- `ExecuteEssential`, `ExecuteDeferred`, launcher-driven `PrewarmJit`, entry point, Harmony patching, Godot/game startup and native game loading remain forbidden.

## Expected evidence

Preserve both `Step35-CrashCheckpoint.txt` and `Step35-ExecuteVeryEarly-StaticMap.txt` after a hard termination. Correlate the final runtime resolver sequence with exact static callsites before authorizing a smaller execution discriminator. A repeated hard crash is still not Step-35 closure.
