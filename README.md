# StS2 Launcher iOS — Step 35.0.9 Null-Platform Constructor Callsite Localization

Active candidate: **Step 35.0.9 / `0.0.132 (132)`**.

Steps 32–34 remain physically CLOSED POSITIVE 4/4. Step 35 remains OPEN, with physical 0.0.126 still the authoritative exact transformed-byte frontier.

Physical 0.0.131 advanced the diagnostic frontier through `SaveManager.ConstructDefault`, `UserDataPathProvider.GetAccountScopedBasePath`, `PlatformUtil..cctor`, and `NullPlatformUtilStrategy..ctor`. The process then hard-terminated after the planned `System.Collections.Concurrent 8.0.0.0 -> host 9.0.0.0` binding and before `GodotFileIo..ctor` emitted its marker. Therefore the previous first-`DirAccess` hypothesis is not supported by the physical path actually reached.

Step 35.0.9 keeps the exact Step-35 source/resolver/startup authority unchanged and adds only an ordered pre/post sweep around every existing non-base `call`, `callvirt`, and `newobj` in `NullPlatformUtilStrategy..ctor`. The run-specific static map also includes that constructor's exact IL and `CALLSITE#xxx` ordinals, allowing the last durable NP marker to identify the outgoing call that did not return.

It does **not** initialize Godot, broaden runtime resolution, load native game code, invoke later `OneTimeInitialization` phases, or alter the exact closed Step-32 transformed source.

**Authority:** 0.0.132 is still a diagnostic derivative. Even a 4/4 result cannot close exact Step 35 because Gate B/C execute an instrumented clone rather than the exact closed transformed bytes.

Start with `docs/CURRENT-STATUS.md`, `docs/TESTING.md`, and `docs/REGRESSION-CONTRACTS.md` before changing Step-35 code.
