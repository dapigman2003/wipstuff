# Current status

## Active candidate — Step 35.0.20 / 0.0.143 (143)

Steps 01–26 are closed. Step 27 is **CLOSED NEGATIVE**. Step 28 is **CLOSED POSITIVE 5/5**. Steps 29–34 are **CLOSED POSITIVE 4/4**. **Step 35 remains OPEN.**

The authoritative exact-transformed Step-35 execution frontier remains physical **0.0.126**: exact `ExecuteVeryEarly()` entered `MethodInfo.Invoke`, but no `C_INVOKE_RETURNED` was durably recorded. All later Step-35 binaries are diagnostic derivatives unless a separately defined closure candidate restores explicit exact-byte execution authority.

## Prior localization and CI provenance

0.0.129–0.0.136 localized the pre-first-await failure through SaveManager/UserDataPathProvider/Platform/NullPlatform into `CommandLineHelper..cctor`, with 0.0.136 placing the hard termination between `CL_CRITICAL_001_PRE` and `CL_CRITICAL_001_POST` around Godot `Dictionary<string,string>` construction. The retired live-stack CL/CLTV probes remain negative instrumentation evidence.

0.0.137 was a pre-device Codemagic failure at **208/209** host tests due solely to the GodotSharp derivative verifier checking the sts2 bridge type. 0.0.138 corrected that verifier. 0.0.139 did not produce an IPA: static validation passed, Codemagic executed **210** host tests, **209 passed / 1 failed**, and the only failure was the stale Step-35.0.15 gate-summary assertion while production emitted Step 35.0.16. 0.0.140 corrected that release/test consistency defect. 0.0.141 likewise stopped before IPA packaging: static validation passed **853/853**, Codemagic executed **211** host tests, **210 passed / 1 failed**, and the sole failure was the new negative callback-table regression expecting zero checkpoints even though production intentionally emitted one durable `CB_INITIALIZE_MANAGED_FAIL` before any preflight/CLR work. 0.0.142 corrected that contract, passed **855/855 static checks**, **211/211 host tests**, and the Step-15 standalone native-link preflight, then failed during iOS C# compilation with CS0103 because the Step-35 partial omitted `using StS2Launcher.iOS.Platform;` while referencing `GodotStep15NativeBridge`. 0.0.143 corrects only that compile-time namespace visibility defect.

## Physical 0.0.138 callback boundary

NATURAL entered the Godot dictionary native thunk and stopped after its then-current GS014 `CustomUnsafe.AsPointer` marker. COMPAT applied the exact four-reference BCL Dictionary substitution, emitted `CL_CRITICAL_001_POST` and `CL_CRITICAL_002_PRE`, entered `INMETHOD_GS033 — Godot.OS::.cctor()`, and terminated before `Godot.OS.GetCmdlineArgs` / GS032. Read-only reconnaissance tied both paths to `NativeFuncs._unmanagedCallbacks` calli thunks.

## Physical 0.0.140 three-mode proof

Three separate fresh-process runs were captured on 2026-09-02.

### NATURAL — `NaturalGodotDictionaryRecon`

Run `20260902T0314561308390Z-pid27225-dc1c7965503e49608e9885d4aaadf308` reached:

`CommandLineHelper..cctor` → `CL_CRITICAL_001_PRE` → generic/non-generic Godot Dictionary constructors → `NativeFuncs.godotsharp_dictionary_new()` → `NativeFuncs.godotsharp_dictionary_new(ref)` → `CustomUnsafe.AsPointer` → **GS031 `godot_dictionary::GetUnsafeAddress()`**, then hard termination.

### OS-RECON — `ManagedDictionaryCompatibility`

Run `20260902T0316052903150Z-pid27290-bea403a746e94110badf7ac0bdd64028` passed `CL_CRITICAL_001_POST`, reached `CL_CRITICAL_002_PRE`, then **GS041 `Godot.OS::.cctor()` → GS043 `StringName.op_Implicit(string)` → GS024 `NativeFuncs.godotsharp_string_name_new_from_string(string)`**, then hard termination. `GetCmdlineArgs()` body entry still did not occur.

### FORWARD — `ManagedCommandLineCompatibility`

Run `20260902T0316528641190Z-pid27300-269a3ecd1fbd4738a15fbc7c732b6726` passed both critical boundaries including **`CL_CRITICAL_002_POST`**, entered `INMETHOD_027 — CommandLineHelper.TryGetValue`, emitted **`NP002_POST`**, entered `GodotFileIo..ctor` and `GodotFileIo.CreateDirectory`, then reached **`Godot.DirAccess.DirExistsAbsolute` → GS043 StringName → GS024 NativeFuncs.godotsharp_string_name_new_from_string(string)`**, then hard termination.

This is the decisive architectural result: the same GodotSharp callback boundary reappears at a genuinely required filesystem API after the command-line dependency has been removed. It is therefore not a CommandLineHelper-specific defect.

## Native callback interpretation

GodotSharp `NativeFuncs.Initialize(IntPtr,int)` validates the callback table size and copies the entire supplied unmanaged callback struct into `NativeFuncs._unmanagedCallbacks`. Callback-backed wrappers later load fields from that struct and execute them through `calli`. No physical 0.0.140 run emitted GS025, the marker assigned to `NativeFuncs.Initialize`, before hitting the callback wrappers above.

The uploaded main game executable is 179,706,736 bytes with SHA-256 `7fadae8d46f0074ba745bc3beebe31a13df5fafed2f2ac69cd68b3c5dd8508e6`, matching the 0.0.140 reconnaissance inventory. Native inspection identifies the standard Godot 4.5.1 C# interop side including `godotsharp::get_runtime_interop_funcs(int&)` and Godot C#/Mono module symbols. This supports using the same source-built Godot 4.5.1 engine as the callback-table producer instead of fabricating callbacks or loading the game executable.

## Step 35.0.20 / 0.0.143 compile-integration correction and active design

0.0.143 preserves the 0.0.142 runtime experiment unchanged. The only Step-35.0.20 runtime-source integration correction is the explicit `using StS2Launcher.iOS.Platform;` import in the Step-35 iOS partial so `GodotStep15NativeBridge` resolves during iOS compilation. The 0.0.142 telemetry contract remains unchanged: invalid callback metadata must be rejected before preflight/CLR work while producing exactly one durable `CB_INITIALIZE_MANAGED_FAIL`, and `CB_INIT_ENTRY` remains unreachable.

The three prior controls are preserved unchanged. A fourth diagnostic mode, `GodotCoreCallbackHandoff`, is the sole exception to their no-Godot-state rule.

CORE-HANDOFF requires the project-owned Step-15 smoke engine to have already completed setup in the same process. The pinned Godot 4.5.1 iOS static build now enables `module_mono_enabled=yes` so native C# scaffolding and `runtime_interop.cpp` are present. The smoke project has no `dotnet` project feature. The native bridge refuses callback export unless Engine, ProjectSettings, CSharpLanguage and GDMono native state exist; it separately reports the `dotnet` feature and whether GDMono is initialized. The iOS UI refuses the handoff if either competing-runtime signal is true.

The bridge obtains `godotsharp::get_runtime_interop_funcs(size)`, rejects null/empty/non-pointer-aligned tables and null entries, and returns only pointer+size. The Step-35 strict load context then explicitly loads the already verified private GodotSharp diagnostic derivative, binds exact `Godot.NativeInterop.NativeFuncs.Initialize(IntPtr,int)`, requires its private static `initialized` field to be false, invokes Initialize exactly once, requires the field to become true, freezes resolver/native counters, and only then enters Gate C's natural ExecuteVeryEarly diagnostic path.

The game native executable is **not loaded**. No callback address is invented. ExecuteEssential, ExecuteDeferred, entry-point execution, native game resolution, arbitrary resolver fallback, and Harmony/MonoMod runtime patching remain forbidden. Exact source pins `0x06007D02` / `0x0600BC71` and the Step-32 transformed authority remain unchanged.

A 0.0.143 diagnostic 4/4, including CORE-HANDOFF, is **NOT Step-35 closure**.
