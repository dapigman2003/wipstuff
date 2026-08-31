# Step 35.0.13 — Stack-neutral CommandLine / Godot boundary localization

Candidate: `0.0.136 (136)`.

## Why this exists

Physical 0.0.133 introduced live-stack PRE/POST callsite markers into `CommandLineHelper..cctor` and the CLR rejected the rewritten cctor with managed `InvalidProgramException` before instruction zero. Step 35.0.12 / physical 0.0.135 raised and post-write verified the cctor MaxStack header, but the same pre-instruction-zero `InvalidProgramException` reproduced before any cctor entry or critical marker. Therefore the 0.0.133 failure was not caused by MaxStack alone.

The authoritative exact-game frontier remains physical 0.0.132: `NullPlatformUtilStrategy..ctor` reaches the call to `CommandLineHelper.TryGetValue`, type initialization begins, and the process hard-terminates before that call returns.

## Diagnostic design

0.0.136 removes all live-stack `INMETHOD_CLxxx_PRE/POST` and `INMETHOD_CLTVxxx_PRE/POST` callbacks from the **runtime diagnostic clone**. Their exact-source CALLSITE maps remain output-only for correlation. Production runtime localization uses only callbacks inserted at empty evaluation-stack boundaries:

- the existing `CommandLineHelper..cctor` entry marker;
- `INMETHOD_CL_CRITICAL_001_PRE` immediately before the Godot dictionary constructor;
- `INMETHOD_CL_CRITICAL_001_POST` after the resulting dictionary has been assigned to `_args`;
- `INMETHOD_CL_CRITICAL_002_PRE` immediately before `Godot.OS.GetCmdlineArgs()`;
- `INMETHOD_CL_CRITICAL_002_POST` after its returned array has been stored;
- `INMETHOD_027` at `CommandLineHelper.TryGetValue` entry.

The outer corrected `INMETHOD_NP002_PRE/POST` pair remains around the `CommandLineHelper.TryGetValue` call. Therefore one physical run can distinguish: cctor rejected before instruction zero; dictionary constructor interval; `GetCmdlineArgs` interval; later cctor work; `TryGetValue` body; or successful return.

## Safety / authority

The exact Step-32 transformed artifact, resolver plan, fresh-process requirement, 60-second await boundary, initializer-bearing dependency prohibition, native-load prohibition, Godot/game-startup prohibition, and later-initialization prohibition remain unchanged. This is diagnostic-clone-only instrumentation and cannot by itself close Step 35.

## Regression contract

The candidate must fail static validation if production reintroduces either `InsertCommandLineHelperCctorCallsiteMarkers(commandLineCctor, ...)` or `InsertCommandLineHelperTryGetValueCallsiteMarkers(commandLineTryGetValue, ...)`. Serialized `CommandLineHelper..cctor` MaxStack must remain equal to the exact-source value because the production CommandLine markers are stack-neutral. Exact-source cctor and TryGetValue CALLSITE maps must still be emitted.
