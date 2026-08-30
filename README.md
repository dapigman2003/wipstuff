# StS2 Launcher iOS — Step 35.0.8 Save/Platform/Godot Native-Boundary Localization

Active candidate: **Step 35.0.8 / `0.0.131 (131)`**.

Physical 0.0.130 proved the Step-35.0.7 generic-delegate bridge correction. Gate A/B passed, Gate C armed the diagnostic bridge, and durable game-body markers reached `ExecuteVeryEarly.MoveNext`, `TestMode.get_IsOn`, `SaveManager..cctor`, and `SaveManager.get_Instance`; a second `TestMode.get_IsOn` marker then appeared from work under that getter. `System.Text.Json` and `System.Collections.Concurrent 8.0.0.0 -> host 9.0.0.0` resolution followed, then the process hard-terminated before either `InitSettingsDataForTest()` or `InitSettingsData()` was entered. No matching `.ips` is available for that run.

Exact `sts2.dll` analysis narrows the normal non-test branch further: `SaveManager.ConstructDefault()` -> `UserDataPathProvider.GetAccountScopedBasePath(...)` -> `PlatformUtil.get_PrimaryPlatform()` / `PlatformUtil..cctor` -> `NullPlatformUtilStrategy..ctor` -> `GodotFileIo..ctor` -> `GodotFileIo.CreateDirectory(string)`. `CreateDirectory` contains one `Godot.DirAccess.DirExistsAbsolute(string)` call and, when the directory is absent, one `MakeDirRecursiveAbsolute(string)` call.

Step 35.0.8 adds only output-only entry markers on that path plus verified pre/post markers around those two Godot callsites. It does **not** initialize Godot, broaden runtime resolution, load native game code, invoke later `OneTimeInitialization` phases, or alter the exact closed Step-32 transformed source.

The 0.0.129 `Action<string>::Invoke(string)` bug remains protected by a synthetic serialize/reopen host regression that requires the ECMA-correct `Action<string>::Invoke(!0)` MemberRef. A second synthetic round-trip regression protects the new callsite-marker placement.

**Authority:** 0.0.131 is still a diagnostic derivative. Even a 4/4 result cannot close exact Step 35 because Gate B/C execute an instrumented clone rather than the exact closed transformed bytes. Step 35 remains OPEN until a separately defined authoritative compatibility artifact passes its physical closure contract.
