# Documentation — Step 35.0.8 / 0.0.131

Physical 0.0.130 proved the corrected `Action<string>::Invoke(!0)` bridge and emitted durable in-game markers through `SaveManager.get_Instance`, with a nested second `TestMode.get_IsOn` before the process hard-terminated. That run did not reach either settings-initialization method and has no matching `.ips`.

0.0.131 keeps the exact Step-35 source/resolver authority unchanged and adds only narrow entry/callsite localization along the statically verified Save/Platform/Godot path. It instruments `SaveManager.ConstructDefault`, `UserDataPathProvider.GetAccountScopedBasePath`, `PlatformUtil.get_PrimaryPlatform` plus its managed `.cctor`, `NullPlatformUtilStrategy..ctor`, `GodotFileIo..ctor`, and `GodotFileIo.CreateDirectory`, then places pre/post markers around `Godot.DirAccess.DirExistsAbsolute` and `MakeDirRecursiveAbsolute`.

See `CURRENT-STATUS.md` for the active frontier, `TESTING.md` for the physical acceptance contract, and `history/INDEX.md` for preserved step/physical records.
