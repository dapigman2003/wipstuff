# StS2 Launcher — Step 35

Active candidate: **Step 35.0.18 / 0.0.141 (141)** — controlled Godot core callback-handoff probe.

Steps 01–26 are closed. Step 27 is CLOSED NEGATIVE. Step 28 is CLOSED POSITIVE 5/5. Steps 29–34 are CLOSED POSITIVE 4/4. **Step 35 remains OPEN.** The exact transformed authority is unchanged: the natural target remains the closed Step-32 `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()` image, source token `0x06007D02`, async MoveNext source token `0x0600BC71`. Diagnostic derivatives measure compatibility frontiers but cannot close exact Step 35.

Physical **0.0.140** completed the three control modes and materially changed the diagnosis. NATURAL reached `INMETHOD_GS031 — godot_dictionary::GetUnsafeAddress()` inside the dictionary native thunk. OS-RECON passed the managed Dictionary assignment and reached `Godot.OS::.cctor()` → `Godot.StringName::op_Implicit(string)` → `INMETHOD_GS024 — NativeFuncs::godotsharp_string_name_new_from_string(string)`. FORWARD passed `CL_CRITICAL_002_POST`, entered `CommandLineHelper.TryGetValue`, emitted `NP002_POST`, constructed `GodotFileIo`, and reached `GodotFileIo.CreateDirectory` → `Godot.DirAccess.DirExistsAbsolute` → StringName → GS024 before hard termination.

That independent filesystem reproduction makes the boundary general rather than command-line-specific. The GodotSharp map shows `NativeFuncs.Initialize(IntPtr,int)` copies the native callback table into `NativeFuncs._unmanagedCallbacks`; none of the physical controls reached its GS025 marker before touching callback-backed wrappers.

The owner-uploaded game executable used for native reconnaissance is 179,706,736 bytes, SHA-256 `7fadae8d46f0074ba745bc3beebe31a13df5fafed2f2ac69cd68b3c5dd8508e6`, exactly matching the main executable inventoried by the 0.0.140 report. Its Godot 4.5.1 native side exposes the standard `godotsharp::get_runtime_interop_funcs(int&)` producer rather than a game-specific callback format.

0.0.141 preserves all three prior controls and adds a fourth mode:

- **NATURAL** — unchanged original Godot Dictionary/OS path; fresh-process/no-Godot control.
- **OS-RECON** (`ManagedDictionaryCompatibility`) — exactly four BCL `Dictionary<string,string>` substitutions; natural `Godot.OS.GetCmdlineArgs()` retained; fresh-process/no-Godot control.
- **FORWARD** (`ManagedCommandLineCompatibility`) — the same four Dictionary substitutions plus exactly one local `new string[0]` provider replacing `Godot.OS.GetCmdlineArgs()` in `CommandLineHelper..cctor`; fresh-process/no-Godot control.
- **CORE-HANDOFF** (`GodotCoreCallbackHandoff`) — requires the already-proven project-owned Step-15 smoke engine in the same process. The native bridge requires Godot Engine/ProjectSettings/CSharpLanguage/GDMono scaffolding, rejects a `dotnet` project feature or an already initialized Godot .NET runtime, obtains the exact source-built Godot 4.5.1 runtime-interoperability callback pointer/size, verifies a non-null pointer-aligned table with no null entries, and passes it exactly once to the verified private GodotSharp derivative's `NativeFuncs.Initialize(IntPtr,int)` before the natural Step-35 diagnostic invocation.

CORE-HANDOFF does **not** load the game's native executable, does not fabricate function pointers, does not invoke the game entry point, and does not authorize ExecuteEssential/ExecuteDeferred, arbitrary resolver fallback, or Harmony/MonoMod runtime patching. The Step-15 smoke project itself has no `dotnet` feature. A 0.0.141 diagnostic 4/4 still cannot close exact Step 35.

Start with `docs/CURRENT-STATUS.md`, `docs/REGRESSION-CONTRACTS.md`, and `docs/TESTING.md`. Historical step/evidence records are under `docs/history/` and mirrored in `history.zip`. The source archive contains no proprietary game-managed/native payload.
