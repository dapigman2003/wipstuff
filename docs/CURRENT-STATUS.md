# Current status

## Active candidate — Step 35.0.14 / 0.0.137 (137)

Steps 01–26 are closed. Step 27 is **CLOSED NEGATIVE**. Step 28 is **CLOSED POSITIVE 5/5**. Steps 29–34 are **CLOSED POSITIVE 4/4**. **Step 35 remains OPEN.**

The authoritative exact transformed Step-35 compatibility frontier remains physical **0.0.126**: exact `ExecuteVeryEarly()` entered `MethodInfo.Invoke`, but no `C_INVOKE_RETURNED` was durably recorded. Later Step-35 builds are diagnostic derivatives unless a new exact-byte closure contract explicitly says otherwise.

## Physical localization history

- **0.0.129:** deferred Cecil writing worked, but the synthetic bridge encoded `Action<string>.Invoke(string)` and failed normally. 0.0.130 corrected it to `Action<string>::Invoke(!0)`.
- **0.0.130–0.0.131:** durable markers advanced through `SaveManager`, `UserDataPathProvider`, `PlatformUtil..cctor`, and `NullPlatformUtilStrategy..ctor`.
- **0.0.132:** `INMETHOD_NP003_PRE` appeared before `CommandLineHelper.TryGetValue`; the exact-source map proved a +1 diagnostic ordinal defect and identified the physical interval as work triggered by `TryGetValue`.
- **0.0.133:** corrected NP002, but live-stack CommandLine instrumentation produced managed `InvalidProgramException` before cctor instruction zero and reached normal `RUN_END`.
- **0.0.135:** verified MaxStack headroom reproduced the same pre-zero `InvalidProgramException`, disproving MaxStack-only causation.
- **0.0.136:** with all live-stack CL/CLTV callbacks removed, `CommandLineHelper..cctor` finally executed. The last in-method checkpoint was `INMETHOD_CL_CRITICAL_001_PRE` before `_args` construction. No POST, `CL_CRITICAL_002_PRE`, `INMETHOD_027`, `NP002_POST`, or `C_INVOKE_RETURNED` followed. The exact-source map identifies the instruction as `Godot.Collections.Dictionary<string,string>::.ctor()`. The final planned `System.Collections.Concurrent 8 -> 9` bind is context only; the PRE/no-POST pair is the localization evidence.

## Step 35.0.14 / rebuilt 0.0.137

The original narrow 0.0.137 draft tested only a managed dictionary substitution. It was superseded before physical testing by this comprehensive 0.0.137 source candidate so one IPA can answer more than one compatibility question.

Gate A still recreates and verifies the exact closed Step-32 transformed image, writes the exact-source static map, and keeps the exact source outside the CLR. It additionally performs **read-only reconnaissance** over the exact OfflineReady managed depot:

- GodotSharp assembly refs and selected critical/local IL;
- GodotSharp P/Invoke declarations and `calli` sites;
- `NativeFuncs` / `UnmanagedCallbacks` field-use sites;
- native/Mach-O candidates under the managed depot, including architecture, linked dylibs, rpaths, bounded interesting symbols, and bounded printable strings relevant to Godot/.NET/native bootstrap;
- no native binary is loaded or executed by reconnaissance.

Gate A also emits a separate `GodotSharp.step35.0.14.instrumented.dll` derivative with same identity/MVID and **entry-only** markers in the bounded Dictionary/OS/NativeCalls/NativeFuncs call graph. Entry-only instrumentation is deliberate: the live-stack CL/CLTV sweep family remains retired after 0.0.133/0.0.135.

The same app offers two modes, each requiring a separate fresh process:

- **NATURAL — `NaturalGodotDictionaryRecon`:** preserves the original `Godot.Collections.Dictionary<string,string>` contract in `CommandLineHelper`. Its purpose is to let the instrumented GodotSharp derivative emit inner method-entry evidence before the physically proven constructor hard-kill.
- **COMPAT — `ManagedDictionaryCompatibility`:** applies exactly the bounded `_args` field / constructor / `set_Item` / `TryGetValue` substitution to `System.Collections.Generic.Dictionary<string,string>`. `Godot.OS.GetCmdlineArgs()` remains natural, allowing the same IPA to advance to that boundary if the dictionary bypass succeeds.

For both modes, the prepared original GodotSharp is re-hashed before the diagnostic derivative is selected; the derivative is separately hash-pinned, preserves planned identity/MVID, and its callback bridge is armed before the resolver returns it. Runtime resolver authority is unchanged: exact planned host bindings and hash-pinned initializer-free private dependencies only. The known initializer-bearing `0Harmony 2.4.2.0` stays forbidden. Native load requests still fail closed. No Godot/game startup, entry-point execution, later OneTimeInitialization phase, arbitrary resolver fallback, or Harmony/MonoMod runtime patching is authorized.

The exact Step-35 target remains `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()`, source token `0x06007D02`, async MoveNext source token `0x0600BC71`. A 0.0.137 diagnostic 4/4 from either mode remains **NOT Step-35 closure**.
