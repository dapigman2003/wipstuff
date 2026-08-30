# Current Status — Step 35.0.8 Save/Platform/Godot Native-Boundary Localization

## Active candidate — Step 35.0.8 / 0.0.131 (131)

Steps 01–26 are closed; Step 27 is CLOSED NEGATIVE; Step 28 is CLOSED POSITIVE 5/5; Steps 29–34 are CLOSED POSITIVE 4/4. **Step 35 remains OPEN.**

Physical 0.0.126 remains the authoritative **exact transformed-byte** Step-35 runtime frontier. It proved exact transformed `ExecuteVeryEarly()` admission/binding/invocation and a hard termination during the initial synchronous portion of `<ExecuteVeryEarly>d__7::MoveNext`, before `C_INVOKE_RETURNED` and before the first incomplete await. Exact source authority remains `ExecuteVeryEarly` token `0x06007D02`, async `MoveNext` token `0x0600BC71`, and closed Step-32 transformed SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`.

Physical 0.0.127 and 0.0.128 failed normally during diagnostic-clone creation. Physical 0.0.129 fixed the deferred-open/configure/write ordering but exposed a managed `MissingMethodException` from the synthetic generic MemberRef bug `Action<string>::Invoke(string)` before any `INMETHOD_*` marker.

## Physical 0.0.130 localization result

Step 35.0.7 / 0.0.130 corrected the bridge to `Action<string>::Invoke(!0)` and physically proved that correction. Same-run Run ID `20260830T1534599351240Z-pid1600-14d8ad5a1dfe44359180c8fc25d7fcc6` established:

- Gate A PASS and Gate B PASS;
- exact transformed source remained SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`;
- the diagnostic clone was loaded into the strict Step-35 private context with preserved identity/MVID and zero primary-admission resolution activity;
- Gate C armed the diagnostic bridge and entered the one reflected `ExecuteVeryEarly()` invocation;
- durable game-body markers reached `INMETHOD_001 — ExecuteVeryEarly.MoveNext entered`;
- `INMETHOD_010 — TestMode.get_IsOn entered` followed;
- `INMETHOD_CCTOR — MegaCrit.Sts2.Core.Saves.SaveManager..cctor entered` followed;
- `INMETHOD_020 — SaveManager.get_Instance entered` followed;
- a second `INMETHOD_010 — TestMode.get_IsOn entered` then appeared from work under the getter;
- `System.Text.Json 9.0.0.0` host resolution followed;
- the last durable event was `System.Collections.Concurrent 8.0.0.0 -> host 9.0.0.0`;
- neither `InitSettingsDataForTest()` nor `InitSettingsData()` was entered;
- the process hard-terminated and no matching `.ips` is available.

The final `System.Collections.Concurrent` resolver event remains a **frontier marker, not an attributed root cause**.

## Exact game-path analysis used for 0.0.131

Static metadata/IL analysis of the supplied exact `sts2.dll` narrows the normal non-test path under `SaveManager.get_Instance` to:

`SaveManager.ConstructDefault()` -> `UserDataPathProvider.GetAccountScopedBasePath(string, Nullable<PlatformType>, Nullable<ulong>)` -> `PlatformUtil.get_PrimaryPlatform()` / `PlatformUtil..cctor` -> `NullPlatformUtilStrategy..ctor` -> `GodotFileIo..ctor(string)` -> `GodotFileIo.CreateDirectory(string)`.

`GodotFileIo.CreateDirectory(string)` contains exactly one call to `System.Boolean Godot.DirAccess::DirExistsAbsolute(System.String)` and, on the missing-directory branch, exactly one call to `Godot.Error Godot.DirAccess::MakeDirRecursiveAbsolute(System.String)`.

This path is consistent with the observed `System.Text.Json` and .NET-8 framework resolution traffic, but the exact native fault instruction is **not physically proven** without a matching `.ips` or a pre/post callsite boundary.

## Step 35.0.8 / 0.0.131 change

0.0.131 keeps all exact-source, writer-only resolver, runtime resolver, timeout, fresh-process and later-boundary prohibitions unchanged. The diagnostic clone adds only:

1. entry marker `INMETHOD_021` for `SaveManager.ConstructDefault()`;
2. entry marker `INMETHOD_022` for `UserDataPathProvider.GetAccountScopedBasePath(...)`;
3. entry marker `INMETHOD_023` for `PlatformUtil.get_PrimaryPlatform()` plus the existing automatic managed `.cctor` marker for `PlatformUtil`;
4. entry marker `INMETHOD_024` for `NullPlatformUtilStrategy..ctor()`;
5. entry marker `INMETHOD_025` for `GodotFileIo..ctor(string)`;
6. entry marker `INMETHOD_026` for `GodotFileIo.CreateDirectory(string)`;
7. `INMETHOD_180/181` immediately before/after `Godot.DirAccess.DirExistsAbsolute(string)`;
8. `INMETHOD_182/183` immediately before/after `Godot.DirAccess.MakeDirRecursiveAbsolute(string)`.

The clone still preserves assembly identity/MVID, is serialized only after the deferred-open bounded writer resolver is configured, reopens under rejecting resolution, rechecks every entry/callsite marker after serialization, and immediately re-hashes the exact transformed source unchanged.

The prior 0.0.129 generic MemberRef failure is additionally protected by a synthetic Cecil serialize/reopen host test requiring `Action<string>::Invoke(!0)`. A second synthetic serialize/reopen test protects the new Godot callsite-marker placement.

## Decision rule for the next physical run

If the final durable evidence is `INMETHOD_180` with no `INMETHOD_181`, the physical frontier is the first `Godot.DirAccess.DirExistsAbsolute` call. If `181` is present and `182` is the final marker, the frontier is `MakeDirRecursiveAbsolute`. If both post markers return, continue from the next durable entry marker rather than attributing the crash to Godot directory interop.

Even if 0.0.131 reaches Gate D and reports 4/4, that is **Step 35.0.8 diagnostic localization complete — NOT Step 35 closure**. Godot/game startup, native game loading, initializer-bearing `0Harmony`, arbitrary managed fallback, later `OneTimeInitialization` phases, the game entry point, and Harmony/MonoMod runtime patching remain forbidden in this candidate.
