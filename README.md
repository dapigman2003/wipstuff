# StS2 Launcher — Step 35

Active candidate: **Step 35.0.10 / 0.0.133 (133)** — Command-Line / Godot boundary localization.

Steps 01–26 are closed. Step 27 is CLOSED NEGATIVE. Step 28 is CLOSED POSITIVE 5/5. Steps 29–34 are CLOSED POSITIVE 4/4. **Step 35 remains OPEN.**

The authoritative exact-byte Step-35 frontier remains physical 0.0.126: exact transformed `ExecuteVeryEarly()` entered `MethodInfo.Invoke` and hard-terminated before `C_INVOKE_RETURNED`. Later builds are diagnostic derivatives only.

Physical 0.0.132 narrowed the live frontier to `NullPlatformUtilStrategy..ctor` calling `CommandLineHelper.TryGetValue`. It durably emitted `INMETHOD_NP003_PRE` with no POST. The same-run exact-source map identifies that call as `CALLSITE#002`, proving both the TryGetValue/type-initialization failure interval and a +1 diagnostic ordinal bug caused by counting the injected entry-marker bridge call.

0.0.133 fixes that ordinal accounting and adds output-only localization for `CommandLineHelper..cctor` and `CommandLineHelper.TryGetValue`: exact-source static-map sections plus ordered `INMETHOD_CLxxx_PRE/POST` and `INMETHOD_CLTVxxx_PRE/POST` markers. Gate A fails closed unless the cctor sweep contains `Godot.OS.GetCmdlineArgs`. No Godot bootstrap, native game load, resolver broadening, later initialization phase, or runtime Harmony/MonoMod patching is authorized.

Start with `docs/CURRENT-STATUS.md`, `docs/REGRESSION-CONTRACTS.md`, and `docs/TESTING.md`. Historical step/evidence records are under `docs/history/` and mirrored in `history.zip`.
