# Step 35.0.8 — Save/Platform/Godot Native-Boundary Localization

Version: `0.0.131 (131)`

## Evidence basis

Physical 0.0.130 proved the diagnostic bridge and emitted durable markers through `SaveManager.get_Instance`, then a nested second `TestMode.get_IsOn`, before hard termination. Neither settings-init method was entered. No matching `.ips` is available.

Static analysis of the exact supplied `sts2.dll` identified the narrow normal branch under the getter:

`SaveManager.ConstructDefault()` -> `UserDataPathProvider.GetAccountScopedBasePath(...)` -> `PlatformUtil.get_PrimaryPlatform()` / `PlatformUtil..cctor` -> `NullPlatformUtilStrategy..ctor()` -> `GodotFileIo..ctor(string)` -> `GodotFileIo.CreateDirectory(string)`.

`GodotFileIo.CreateDirectory` contains exactly one `Godot.DirAccess.DirExistsAbsolute(string)` and one conditional `Godot.DirAccess.MakeDirRecursiveAbsolute(string)` call.

## Change

Keep exact transformed source/resolver/startup authority frozen. Extend only the diagnostic clone with entry markers `021`-`026` for the path above and callsite pre/post pairs `180/181` and `182/183` around the two Godot calls.

The callsite helper refuses a selected call that is a branch target before instrumentation. After Cecil serialization the clone reopens under rejecting resolution and requires the marker pairs to remain immediately adjacent to exactly one matching callsite each. Exact transformed source is immediately re-hashed unchanged.

## Regression protection

- Synthetic Cecil serialize/reopen test for `Action<string>::Invoke(!0)` prevents recurrence of the physical 0.0.129 bridge bug.
- Synthetic Cecil serialize/reopen test proves pre/post marker adjacency around a target Godot-style callsite.
- Production verification counts every serialized `INMETHOD_*` marker and requires the computed expected count.

## Physical interpretation

- `INMETHOD_180` without `181`: boundary at/in `DirExistsAbsolute`.
- `181` then `182` without `183`: boundary at/in `MakeDirRecursiveAbsolute`.
- both post markers present: continue localization; do not attribute the crash to those calls.

This step does not authorize Godot bootstrap/startup, native game loading, later initialization phases, or any resolver broadening. A 4/4 result remains diagnostic-only and cannot close exact Step 35.
