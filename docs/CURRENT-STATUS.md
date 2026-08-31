# Current status

## Active candidate — Step 35.0.10 / 0.0.133 (133)

Steps 01–26 are closed. Step 27 is **CLOSED NEGATIVE**. Step 28 is **CLOSED POSITIVE 5/5**. Steps 29, 30, 31, 32, 33 and 34 are **CLOSED POSITIVE 4/4**. **Step 35 remains OPEN.**

The authoritative exact transformed Step-35 compatibility frontier remains physical **0.0.126**: exact `ExecuteVeryEarly()` entered `MethodInfo.Invoke` but no `C_INVOKE_RETURNED` was durably recorded. Diagnostic 0.0.127 and 0.0.128 failed in Gate A Cecil handling. 0.0.129 corrected deferred-open writer behavior but physically produced `MissingMethodException` from synthetic `Action<string>.Invoke(string)`. 0.0.130 corrected that MemberRef to `Action<string>::Invoke(!0)` and reached `SaveManager.get_Instance`. 0.0.131 reached `SaveManager.ConstructDefault`, `UserDataPathProvider.GetAccountScopedBasePath`, `PlatformUtil..cctor`, and `NullPlatformUtilStrategy..ctor`, but never `GodotFileIo..ctor`.

## Physical 0.0.132 finding

The 0.0.132 Step 35.0.9 run established:

- `ExecuteVeryEarly.MoveNext`, `TestMode.get_IsOn`, `SaveManager..cctor`, `SaveManager.get_Instance`, `SaveManager.ConstructDefault`, `UserDataPathProvider.GetAccountScopedBasePath`, `PlatformUtil..cctor`, and `NullPlatformUtilStrategy..ctor` all entered;
- `INMETHOD_NP003_PRE` was durably emitted immediately before `System.Boolean MegaCrit.Sts2.Core.Helpers.CommandLineHelper::TryGetValue(System.String,System.String&)`;
- no matching `INMETHOD_NP003_POST` appeared;
- `INMETHOD_027` for the actual `TryGetValue` body did not exist in that candidate, so the hard-kill interval includes CLR type initialization triggered before the static method body can enter;
- the last durable resolver event was `System.Collections.Concurrent, Version=8.0.0.0` binding to host 9.0.0.0. That is contextual evidence, not proof of causation.

The same-run exact-source `[NULL PLATFORM CTOR IL]` map labels the base constructor `CALLSITE#001` and `CommandLineHelper.TryGetValue` **`CALLSITE#002`**. Runtime called the latter `NP003`, physically proving the +1 marker defect: the entry-marker bridge `Emit` call was inserted before the sweep and incorrectly consumed an ordinal.

Static inspection of the supplied matching managed assemblies shows `TryGetValue` is a thin dictionary lookup but its type initializer runs first and calls `Godot.OS.GetCmdlineArgs()`. That makes the Godot command-line wrapper/native callback boundary the leading hypothesis; 0.0.133 is designed to prove or disprove it physically rather than treating that inference as closure evidence.

## Step 35.0.10 / 0.0.133 change

0.0.133 preserves every exact-source, writer-only resolver, runtime resolver, timeout, fresh-process, native and later-boundary prohibition from 0.0.132. It changes only output-only diagnostic instrumentation:

- fix NullPlatform callsite accounting so injected bridge calls are ignored **before** exact-source ordinal counting;
- keep the direct base constructor in ordinal accounting but do not wrap it;
- add `INMETHOD_027 — CommandLineHelper.TryGetValue entered`;
- keep the automatic `INMETHOD_CCTOR — MegaCrit.Sts2.Core.Helpers.CommandLineHelper..cctor entered` marker;
- add `INMETHOD_CLxxx_PRE/POST` around eligible original `call`/`callvirt`/`newobj` instructions in `CommandLineHelper..cctor`;
- require the CL plan to contain the `Godot.OS.GetCmdlineArgs` call, otherwise Gate A fails closed before CLR admission;
- add `INMETHOD_CLTVxxx_PRE/POST` around eligible original call-like instructions in `CommandLineHelper.TryGetValue`;
- for the new CommandLine sweeps, unrelated branch-target callsites may be skipped rather than aborting Gate A, but their exact-source ordinals are still consumed so later marker numbers remain aligned with the static map; the required `Godot.OS.GetCmdlineArgs` call may not be skipped;
- extend the same-run exact-source map with `[COMMAND LINE HELPER CCTOR IL]` and `[COMMAND LINE HELPER TRYGETVALUE IL]` sections;
- reproduce the real production ordering in host regressions so the physical 0.0.132 +1 bug cannot recur silently.

The exact Step-35 source target remains `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()`, source token `0x06007D02`, state-machine MoveNext token `0x0600BC71`. Gate B/C still execute only a separately verified diagnostic clone; exact Step-32 transformed bytes remain outside the CLR during the diagnostic run.

## How to read the next physical run

- `INMETHOD_CCTOR` followed by `INMETHOD_CLxxx_PRE` with no matching POST identifies the exact cctor outgoing call that did not return. If its callee is `Godot.OS.GetCmdlineArgs`, the Godot command-line boundary is physically established.
- matching CL PRE/POST through cctor completion followed by `INMETHOD_027` proves type initialization returned and the actual `TryGetValue` body entered.
- `INMETHOD_CLTVxxx_PRE` without matching POST then localizes inside the actual method-body call.
- a managed Gate C failure is evidence; a hard kill is correlated by Run ID/PID through the durable journal and static map.

Even if 0.0.133 reaches Gate D and reports 4/4, that is **Step 35.0.10 diagnostic localization complete — NOT Step 35 closure**. Godot/game startup, native game loading, initializer-bearing `0Harmony`, arbitrary managed fallback, later `OneTimeInitialization` phases, the game entry point, and Harmony/MonoMod runtime patching remain forbidden.
