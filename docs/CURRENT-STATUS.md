# Current status

## Active candidate — Step 35.0.14 / 0.0.137 (137)

Steps 01–26 are closed. Step 27 is **CLOSED NEGATIVE**. Step 28 is **CLOSED POSITIVE 5/5**. Steps 29–34 are **CLOSED POSITIVE 4/4**. **Step 35 remains OPEN.**

The authoritative exact transformed Step-35 compatibility frontier remains physical **0.0.126**: exact `ExecuteVeryEarly()` entered `MethodInfo.Invoke`, but no `C_INVOKE_RETURNED` was durably recorded. Diagnostic derivatives after that do not supersede exact-byte authority.

## Physical localization history

- **0.0.129:** bounded deferred Cecil writer path worked, but the synthetic `Action<string>.Invoke(string)` MemberRef failed. 0.0.130 corrected it to `Action<string>::Invoke(!0)`.
- **0.0.130–0.0.131:** durable markers advanced through `SaveManager`, `UserDataPathProvider`, `PlatformUtil..cctor`, and `NullPlatformUtilStrategy..ctor`; `GodotFileIo..ctor` had not begun.
- **0.0.132:** `INMETHOD_NP003_PRE` appeared before `CommandLineHelper.TryGetValue`; the exact-source map showed this was CALLSITE#002, proving a +1 diagnostic ordinal defect. No POST appeared.
- **0.0.133:** corrected NP002, but live-stack CommandLine instrumentation produced managed `InvalidProgramException` before cctor instruction zero and reached normal `RUN_END`.
- **0.0.135:** verified MaxStack headroom reproduced the same pre-zero `InvalidProgramException`, disproving MaxStack-only causation.
- **0.0.136:** with all live-stack CL/CLTV runtime callbacks removed, the cctor finally executed. The run emitted `INMETHOD_CCTOR — ...CommandLineHelper..cctor entered` and `INMETHOD_CL_CRITICAL_001_PRE — ... before _args dictionary construction`, then stopped before the matching POST, `CL_CRITICAL_002_PRE`, `INMETHOD_027`, `NP002_POST`, or `C_INVOKE_RETURNED`. The final resolver event was the planned `System.Collections.Concurrent 8.0.0.0 -> host 9.0.0.0` binding; that event is contextual, while the PRE/no-POST pair localizes the physical interval to `Godot.Collections.Dictionary<string,string>` construction before assignment.

## Step 35.0.14 / 0.0.137 change

0.0.137 is a **diagnostic compatibility derivative**. Gate A still recreates and re-verifies the exact closed Step-32 transformed image and writes the exact-source static maps before producing a separate clone. The clone preserves all prior telemetry and runtime authority but applies one narrow managed substitution inside `CommandLineHelper`:

- `_args`: `Godot.Collections.Dictionary<string,string>` -> `System.Collections.Generic.Dictionary<string,string>`;
- cctor `newobj`: Godot dictionary constructor -> BCL dictionary constructor;
- cctor `set_Item(!0,!1)`: Godot dictionary -> BCL dictionary;
- `TryGetValue(!0,!1&)`: Godot dictionary -> BCL dictionary.

The rewrite uses the existing `System.Collections` AssemblyRef and preserves ECMA-335 generic VAR signatures. Gate A reopens the serialized clone and verifies all four substitutions, zero residual Godot string-dictionary call references in the affected methods, unchanged cctor MaxStack, and exactly one untouched `Godot.OS.GetCmdlineArgs()` call.

The stack-neutral critical markers remain. In the derivative, `CL_CRITICAL_001_PRE/POST` brackets managed `Dictionary<string,string>` construction/assignment. `CL_CRITICAL_002_PRE/POST` still brackets the natural `Godot.OS.GetCmdlineArgs()` call. No Godot bootstrap, native game loading, resolver broadening, later `OneTimeInitialization` phase, entry-point execution, or runtime Harmony/MonoMod patching is authorized.

The exact Step-35 target remains `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()`, source token `0x06007D02`, async MoveNext source token `0x0600BC71`. A 0.0.137 diagnostic 4/4 result remains **NOT Step-35 closure**.
