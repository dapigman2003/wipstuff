# Current status

## Active candidate — Step 35.0.13 / 0.0.136 (136)

Steps 01–26 are closed. Step 27 is **CLOSED NEGATIVE**. Step 28 is **CLOSED POSITIVE 5/5**. Steps 29, 30, 31, 32, 33 and 34 are **CLOSED POSITIVE 4/4**. **Step 35 remains OPEN.**

The authoritative exact transformed Step-35 compatibility frontier remains physical **0.0.126**: exact `ExecuteVeryEarly()` entered `MethodInfo.Invoke` but no `C_INVOKE_RETURNED` was durably recorded. Diagnostic builds after that are localization derivatives and do not supersede exact-byte authority.

0.0.129 fixed deferred Cecil writer opening but physically proved `MissingMethodException` from a bad synthetic `Action<string>.Invoke(string)` MemberRef. 0.0.130 corrected it to `Action<string>::Invoke(!0)` and reached `SaveManager.get_Instance`. 0.0.131 reached `SaveManager.ConstructDefault`, `UserDataPathProvider.GetAccountScopedBasePath`, `PlatformUtil..cctor`, and `NullPlatformUtilStrategy..ctor`, but never `GodotFileIo..ctor`.

## Physical 0.0.132 game-frontier finding

The 0.0.132 Step 35.0.9 run established:

- the known chain through `NullPlatformUtilStrategy..ctor` entered;
- `INMETHOD_NP003_PRE` was durably emitted immediately before `CommandLineHelper.TryGetValue`;
- no matching POST appeared;
- the same-run exact-source `[NULL PLATFORM CTOR IL]` map identifies `TryGetValue` as **CALLSITE#002**, physically proving the +1 NP diagnostic ordinal defect;
- `GodotFileIo..ctor` was not reached;
- the last `System.Collections.Concurrent 8.0.0.0 → host 9.0.0.0` resolver event remains contextual, not causal evidence.

Static inspection of the supplied matching `sts2.dll` shows `TryGetValue` is a thin dictionary lookup but triggers `CommandLineHelper..cctor` first. The cctor begins by constructing the `_args` Godot dictionary and then calls `Godot.OS.GetCmdlineArgs()`.

## 0.0.134 was stopped before physical testing: its host suite was 205/206 because the new MaxStack regression incorrectly required Cecil's serialized MaxStack to equal 4 rather than be at least the required minimum. 0.0.136 fixes only that test contract while retaining the executable CLR check.

Physical 0.0.133 diagnostic result

0.0.133 Step 35.0.10 corrected the NP runtime marker to `INMETHOD_NP002_PRE`, matching exact-source CALLSITE#002. It then behaved differently from 0.0.132:

- no `CommandLineHelper..cctor` entry marker executed;
- no `INMETHOD_CLxxx_PRE/POST`, `INMETHOD_027`, or `INMETHOD_CLTVxxx_PRE/POST` marker executed;
- `MethodInfo.Invoke` returned;
- the returned `Task` was already faulted;
- the exception chain was `PlatformUtil` `TypeInitializationException` → `CommandLineHelper` `TypeInitializationException` → `System.InvalidProgramException`;
- the launcher wrote its normal report and durable `RUN_END`.

This is **CLOSED DIAGNOSTIC NEGATIVE — instrumentation defect**. It does not advance or retreat the physical game frontier from 0.0.132. The generic cctor callsite sweep placed `ldstr; call Emit` around original calls while original arguments/results could remain live, raising transient stack depth by one without increasing the serialized method `MaxStack` header. Cecil write/reopen tests did not execute the rewritten method and therefore missed the invalid IL header.

## Step 35.0.13 / 0.0.136 change

0.0.136 preserves every exact-source, writer-only resolver, runtime resolver, timeout, fresh-process, native and later-boundary prohibition from 0.0.133. It changes only output-only diagnostic instrumentation and verification:

- corrected NullPlatform ordinal accounting remains;
- every generic live-stack PRE/POST sweep reserves one additional `MaxStack` slot;
- the targeted callsite-marker helper is hardened the same way for future deeper GodotFileIo reachability;
- Gate A captures the exact-source `CommandLineHelper..cctor` MaxStack, requires the diagnostic cctor to be exactly source+1, serializes the clone, reopens it, and verifies that header again;
- the same-run static map prints the exact-source CommandLine cctor MaxStack in `[COMMAND LINE HELPER CCTOR IL]`, while `[COMMAND LINE HELPER TRYGETVALUE IL]` retains the thin body map;
- four **stack-neutral** critical markers are inserted at empty-stack boundaries: `INMETHOD_CL_CRITICAL_001_PRE/POST` around `_args` dictionary construction/assignment and `INMETHOD_CL_CRITICAL_002_PRE/POST` around `Godot.OS.GetCmdlineArgs()` invocation/result storage;
- the full corrected `INMETHOD_CLxxx_PRE/POST` cctor sweep and `INMETHOD_CLTVxxx_PRE/POST` `TryGetValue` sweep remain;
- Gate A still fails closed unless the CL plan includes `Godot.OS.GetCmdlineArgs`;
- host regression coverage now includes actual CLR loading/execution of a generated tight-MaxStack rewritten cctor, not only Cecil round-trip inspection.

The exact Step-35 source target remains `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()`, source token `0x06007D02`, state-machine MoveNext token `0x0600BC71`. Gate B/C execute only a separately verified diagnostic clone; exact Step-32 transformed bytes remain outside the CLR during the diagnostic run.

## How to read the next physical run

The redundant critical markers are the primary high-value evidence:

- no CommandLine cctor entry marker: rejection/JIT/type-init failure before instruction zero;
- cctor entry but no dictionary critical PRE: failure immediately after entry instrumentation;
- dictionary critical PRE without POST: failure during Godot dictionary construction/assignment;
- dictionary POST followed by `Godot.OS.GetCmdlineArgs` critical PRE without POST: physical localization to `Godot.OS.GetCmdlineArgs()`;
- GetCmdlineArgs critical POST: that Godot call returned; subsequent CL markers localize parser work;
- cctor completion followed by `INMETHOD_027`: type initialization returned and the actual `TryGetValue` body entered;
- `INMETHOD_CLTVxxx_PRE` without POST: localizes inside the thin method-body dictionary lookup.

Even if 0.0.136 reaches Gate D and reports 4/4, that is **Step 35.0.13 diagnostic localization complete — NOT Step 35 closure**. Godot/game startup, native game loading, initializer-bearing `0Harmony`, arbitrary managed fallback, later `OneTimeInitialization` phases, the game entry point, and Harmony/MonoMod runtime patching remain forbidden.

Physical 0.0.135 observation: the MaxStack-hardened diagnostic clone still returned a faulted Task with nested System.InvalidProgramException before any CommandLineHelper cctor entry/critical marker executed, despite serialized cctor MaxStack 3 / 4 verification. The launcher reached normal RUN_END. This disproves the MaxStack-only diagnosis; Step 35.0.13 retires all live-stack CL/CLTV runtime callbacks and keeps only stack-neutral CommandLine boundaries.
