# StS2 Launcher — Step 35

Active candidate: **Step 35.0.13 / 0.0.136 (136)** — stack-neutral Command-Line / Godot boundary localization.

Steps 01–26 are closed. Step 27 is CLOSED NEGATIVE. Step 28 is CLOSED POSITIVE 5/5. Steps 29–34 are CLOSED POSITIVE 4/4. **Step 35 remains OPEN.**

The authoritative exact-byte Step-35 frontier remains physical 0.0.126: exact transformed `ExecuteVeryEarly()` entered `MethodInfo.Invoke` and hard-terminated before `C_INVOKE_RETURNED`. Later builds are diagnostic derivatives only.

Physical 0.0.132 narrowed the game frontier to `NullPlatformUtilStrategy..ctor` invoking `CommandLineHelper.TryGetValue`; the call never returned. Its runtime NP ordinal was +1 because an injected bridge call had been counted, and 0.0.133 corrected that mapping to `NP002`.

0.0.134 was stopped before physical testing: its host suite was 205/206 because the new MaxStack regression incorrectly required Cecil's serialized MaxStack to equal 4 rather than be at least the required minimum. 0.0.136 fixes only that test contract while retaining the executable CLR check.

Physical 0.0.133 then exposed a new **diagnostic instrumentation defect** rather than a new game frontier: the process did not hard-kill. `MethodInfo.Invoke` returned a faulted Task with nested `CommandLineHelper` `TypeInitializationException` → `InvalidProgramException`, no CommandLine cctor/CL marker executed, and the launcher reached normal `RUN_END`. The 0.0.133 live-stack PRE/POST sweep could raise transient evaluation-stack depth without increasing the cctor `MaxStack` header.

0.0.136 reserves and post-write verifies MaxStack headroom, keeps corrected exact-source NP/CL/CLTV ordinals, and adds four redundant stack-neutral critical markers around `_args` dictionary construction/assignment and `Godot.OS.GetCmdlineArgs()` invocation/result storage. No Godot bootstrap, native game load, resolver broadening, later initialization phase, or runtime Harmony/MonoMod patching is authorized.

Start with `docs/CURRENT-STATUS.md`, `docs/REGRESSION-CONTRACTS.md`, and `docs/TESTING.md`. Historical step/evidence records are under `docs/history/` and mirrored in `history.zip`.

Physical 0.0.135 observation: the MaxStack-hardened diagnostic clone still returned a faulted Task with nested System.InvalidProgramException before any CommandLineHelper cctor entry/critical marker executed, despite serialized cctor MaxStack 3 / 4 verification. The launcher reached normal RUN_END. This disproves the MaxStack-only diagnosis; Step 35.0.13 retires all live-stack CL/CLTV runtime callbacks and keeps only stack-neutral CommandLine boundaries.
