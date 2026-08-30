# Current Status — Step 35.0.9 Null-Platform Constructor Callsite Localization

## Active candidate — Step 35.0.9 / 0.0.132 (132)

Steps 01–26 are closed; Step 27 is CLOSED NEGATIVE; Step 28 is CLOSED POSITIVE 5/5; Steps 29–34 are CLOSED POSITIVE 4/4. **Step 35 remains OPEN.**

Physical 0.0.126 remains the authoritative **exact transformed-byte** Step-35 runtime frontier. It proved exact transformed `ExecuteVeryEarly()` admission/binding/invocation and a hard termination during the initial synchronous portion of `<ExecuteVeryEarly>d__7::MoveNext`, before `C_INVOKE_RETURNED` and before the first incomplete await. Exact source authority remains `ExecuteVeryEarly` token `0x06007D02`, async `MoveNext` token `0x0600BC71`, and closed Step-32 transformed SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`.

Physical 0.0.127 and 0.0.128 failed normally during diagnostic-clone creation. Physical 0.0.129 failed normally with a managed `MissingMethodException`, exposing the malformed synthetic `Action<string>::Invoke(string)` MemberRef. Physical 0.0.130 corrected it to `Action<string>::Invoke(!0)` and localized the hard termination beneath `SaveManager.get_Instance` before either settings-init method.

## Physical 0.0.131 localization result

Step 35.0.8 / 0.0.131 completed Gate A and Gate B, armed Gate C, and physically advanced the durable managed frontier. Same-run Run ID `20260830T1920262569650Z-pid4122-2714d6840a8c49ef91a759bd03de1834` established:

- exact transformed source still matched SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`;
- the instrumented clone preserved identity/MVID and primary admission remained zero-resolution;
- `INMETHOD_001` — `ExecuteVeryEarly.MoveNext` entered;
- `INMETHOD_010` — `TestMode.get_IsOn` entered;
- `SaveManager..cctor` entered;
- `INMETHOD_020` — `SaveManager.get_Instance` entered;
- `INMETHOD_021` — `SaveManager.ConstructDefault` entered;
- the nested second `INMETHOD_010` appeared;
- `INMETHOD_022` — `UserDataPathProvider.GetAccountScopedBasePath` entered;
- `PlatformUtil..cctor` entered;
- `System.Text.Json 9.0.0.0` bound to the host;
- `INMETHOD_024` — `NullPlatformUtilStrategy..ctor` entered;
- the last durable event was the planned `System.Collections.Concurrent 8.0.0.0 -> host 9.0.0.0` binding;
- `INMETHOD_025` — `GodotFileIo..ctor(string)` never appeared;
- neither `INMETHOD_180` nor any later `Godot.DirAccess` pre/post marker appeared;
- no matching `.ips` is available.

Therefore the physical frontier is **inside work executed by `NullPlatformUtilStrategy..ctor()` after its entry marker and before `GodotFileIo..ctor(string)` begins**. The final resolver event is still a frontier/context marker, not attributed root cause.

This result falsifies the prior first-`DirExistsAbsolute` hypothesis at the level tested by 0.0.131: execution never reached `GodotFileIo` at all.

## Step 35.0.9 / 0.0.132 change

0.0.132 preserves every exact-source, writer-only resolver, runtime resolver, timeout, fresh-process, native and later-boundary prohibition from 0.0.131. It preserves all prior entry/Godot callsite markers and adds only an ordered callsite sweep inside the exact managed `NullPlatformUtilStrategy..ctor()` body:

- enumerate the constructor's original `call`, `callvirt`, and `newobj` instructions without Cecil `Resolve`;
- intentionally do **not** wrap the direct base-constructor call, so the diagnostic callback never runs while an uninitialized `this` value is sitting on the evaluation stack;
- wrap every other existing call-like instruction with a unique `INMETHOD_NPxxx_PRE` / `INMETHOD_NPxxx_POST` pair;
- preserve the constructor's original CALLSITE ordinal in each marker;
- refuse instrumentation if a selected callsite is itself a branch target;
- reopen the serialized clone under rejecting resolution and require every planned pair immediately around the same opcode/callee;
- require total serialized `INMETHOD_*` marker count to match the computed plan;
- immediately re-hash the exact transformed source unchanged.

The same-run static map is also extended with `[NULL PLATFORM CTOR IL]`, including the exact constructor token/IL and `CALLSITE#xxx` ordinals. This makes the next physical evidence self-describing without requiring a later re-analysis of proprietary game bytes.

The synthetic Cecil regression for `Action<string>::Invoke(!0)` remains. The existing selected Godot pre/post round-trip test remains. A new synthetic serialize/reopen regression exercises the NullPlatform constructor sweep across `newobj` and `call`, while proving the direct base constructor is not wrapped.

## Decision rule for the next physical run

Use the final durable `INMETHOD_NPxxx_PRE/POST` pair together with the same-run `[NULL PLATFORM CTOR IL]` section:

- `PRE` without matching `POST`: the physical frontier is inside/at that exact outgoing call/newobj;
- matching `POST` followed by the next `PRE`: the prior call returned normally; continue to the next ordinal;
- constructor entry with **no** `INMETHOD_NPxxx_PRE`: the failure is between constructor entry/base construction and the first swept outgoing call, requiring a narrower non-call IL experiment rather than resolver/startup broadening;
- if `INMETHOD_025` appears, the constructor completed far enough to begin `GodotFileIo`, so resume interpretation using the preserved downstream markers.

Even if 0.0.132 reaches Gate D and reports 4/4, that is **Step 35.0.9 diagnostic localization complete — NOT Step 35 closure**. Godot/game startup, native game loading, initializer-bearing `0Harmony`, arbitrary managed fallback, later `OneTimeInitialization` phases, the game entry point, and Harmony/MonoMod runtime patching remain forbidden.
