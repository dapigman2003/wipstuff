# StS2 Launcher — Step 35

Active candidate: **Step 35.0.14 / 0.0.137 (137)** — managed Command-Line dictionary compatibility probe.

Steps 01–26 are closed. Step 27 is CLOSED NEGATIVE. Step 28 is CLOSED POSITIVE 5/5. Steps 29–34 are CLOSED POSITIVE 4/4. **Step 35 remains OPEN.** The authoritative exact-byte Step-35 frontier remains physical 0.0.126: the exact closed transformed `ExecuteVeryEarly()` entered `MethodInfo.Invoke` and hard-terminated before `C_INVOKE_RETURNED`. All later Step-35 builds are diagnostic derivatives unless explicitly promoted by a new closure contract.

Physical 0.0.132 localized the natural game path to `NullPlatformUtilStrategy..ctor -> CommandLineHelper.TryGetValue`. 0.0.133 and 0.0.135 exposed diagnostic IL rejection (`InvalidProgramException`) from live-stack CommandLine instrumentation. 0.0.136 removed those live-stack callbacks and finally entered `CommandLineHelper..cctor`; its final in-method checkpoint was `INMETHOD_CL_CRITICAL_001_PRE` before `_args` dictionary construction, with no matching POST. The last durable resolver event was the already-planned `System.Collections.Concurrent 8.0.0.0 -> host 9.0.0.0` binding, which remains contextual rather than causal.

0.0.137 preserves the exact-source static maps, corrected NP ordinals, stack-neutral CommandLine markers, writer-only Cecil resolver, strict runtime resolver, 60-second Task boundary, native-load refusal, and Godot-startup prohibition. The diagnostic clone changes only `CommandLineHelper`'s private string dictionary contract: `_args`, its constructor, `set_Item`, and `TryGetValue` are rewritten from `Godot.Collections.Dictionary<string,string>` to the existing `System.Collections.Generic.Dictionary<string,string>` contract. `Godot.OS.GetCmdlineArgs()` remains natural so the next physical run can show whether the frontier advances there.

A 4/4 result from 0.0.137 is diagnostic compatibility evidence only and **cannot close exact Step 35**. The source archive contains no proprietary game-managed payload.

Start with `docs/CURRENT-STATUS.md`, `docs/REGRESSION-CONTRACTS.md`, and `docs/TESTING.md`. Historical step/evidence records are under `docs/history/` and mirrored in `history.zip`.
