# Current status

## Active candidate — Step 35.0.26 / 0.0.149 (149)

Steps 01–26 are closed. Step 27 is **CLOSED NEGATIVE**. Step 28 is **CLOSED POSITIVE 5/5**. Steps 29–34 are **CLOSED POSITIVE 4/4**. **Step 35 remains OPEN.**


## Step 35.0.26 / 0.0.149 Gate-D observability + Codemagic cache

The current device observation has reached Gate D on 0.0.148, but Gate D can appear stationary because its nested `SteamOfflineInstallInspection` did not forward progress and a single large PCK hash could consume a long interval without changing the file count. 0.0.149 preserves the proven Godot bridge and Gate-C resolver contract while forwarding the existing OfflineReady file/byte telemetry and displaying a dedicated Gate-D progress bar, latest verified file, byte totals, observed throughput, and a one-second elapsed/last-progress heartbeat so a long single-file SHA-1 no longer looks dead.

Codemagic already restores the NuGet caches, source-built Godot archive, and the large iOS arm64 `obj` cache. 0.0.149 additionally caches `$HOME/.dotnet`, verifies the exact pinned SDK/workload marker, and skips the network workload install only when `ios` is actually registered for workload set `9.0.314.3`. This is build-time only and does not change the game/bridge execution contract.


## Physical 0.0.144 reverse-binding frontier

Run `20260902T1749100715980Z-pid32517-8963408799ff47c69d2afb674c3817cf` preserved the successful 1,800 bytes / 225 pointers CORE-HANDOFF and `initialized=true`. The natural path entered `Godot.OS.GetCmdlineArgs()` -> `Godot.OS.get_Singleton()` -> `InteropUtils.EngineGetSingleton` -> `godotsharp_engine_get_singleton` -> `UnmanagedGetManaged`. The script-instance query returned far enough for `GodotBoolExtensions.ToBool`, then the final durable marker was **GS035 `NativeFuncs.godotsharp_internal_unmanaged_get_instance_binding_managed(IntPtr)`**. GS036 `godotsharp_internal_unmanaged_instance_binding_create_managed` was not reached. This proves native singleton lookup succeeds and localizes the next hard boundary to Godot native->managed instance-binding association state.

## Physical 0.0.145 reverse-binding readiness result

Run `20260903T0329320479790Z-pid37654-cb70acd45f2d48049455249a539c60bd` re-proved the private GodotSharp 1,800-byte / 225-pointer handoff (`NativeFuncs.Initialize` returned with `initialized=true`) and then recorded `csharpLanguage=True`, `godotApiCacheUpdated=False`, `createManagedBindingCallback=False`, `reverseBindingReady=False`, and `godotDotNetInitialized=False`. 0.0.145 stopped at `CB_REVERSE_BINDING_NOT_READY_STOP`, did not invoke Gate C / ExecuteVeryEarly, and reached normal `RUN_END`. This is positive diagnostic evidence that the GS035 frontier is missing Godot native->managed managed-plugin/cache initialization rather than another managed->native callback-table defect.

## Physical 0.0.146 managed-plugin bootstrap success / resolver-guard stop

Run `20260903T1603566955510Z-pid41300-dfbd3965930c42f7b5dc70da9a2cffbb` reproduced the proven 1,800-byte / 225-pointer managed->native handoff and then completed the generated reverse bridge. `ManagedCallbacks.Create(IntPtr)` returned a **296-byte table with 37 non-null pointers**; `ScriptManagerBridge.LookupScriptsInAssembly` returned; `GDMonoCache::update_godot_api_cache` adoption returned with `godotApiCacheUpdated=True`, `createManagedBindingCallback=True`, `reverseBindingReady=True`, `externalBridgeInstalled=True`; and the deliberate native->managed `GD_OnCoreApiAssemblyLoaded` signal returned successfully.

The bridge preparation itself caused exactly eight additional planned framework resolver requests / eight host loads and zero additional private loads, with initializer-bearing, rejected, and native activity all remaining zero. Immediately afterward Gate C failed normally with `InvalidDataException` **before target type binding** because the old callback-handoff snapshot still expected the pre-bootstrap resolver counts (`managed=2; host=1; private=1`) rather than the post-bootstrap state (`managed=10; host=9; private=1`). This is a resolver-contract defect in 0.0.146, not a failure of the managed-plugin bridge.

## Step 35.0.24 / 0.0.147 post-bootstrap resolver baseline correction

0.0.147 leaves the physically successful 0.0.146 bridge bootstrap unchanged. After `CB_MANAGED_PLUGIN_BOOTSTRAP_PASS`, it accepts only the exact measured eight-request/eight-host-load/zero-private-load bootstrap delta, requires zero initializer-bearing/rejected/native escape, records `CB_POST_BOOTSTRAP_RESOLVER_BASELINE_PASS`, and freezes those counters. Gate C then requires the resolver/native state to remain exactly unchanged from that post-bootstrap baseline before binding natural diagnostic `ExecuteVeryEarly`. Any extra request still fails closed.

## 0.0.147 Codemagic host-regression message failure / 0.0.148 correction

Codemagic for 0.0.147 passed **881/881 static checks** and reached **212/213 host tests** (213 total: 212 passed / 1 failed). The sole failure was `GodotManagedPluginResolverBaselineRejectsMissingPreflightWithDurableFailure`: production correctly threw `Step 35 Gate A must pass before Gate B.`, but the negative test incorrectly required the literal word `preflight`. The workflow stopped at host tests, so no iOS compile/link/IPA result was produced.

Step 35.0.25 / 0.0.148 changes only that negative host-test assertion to pin the actual production exception contract. The 0.0.147 managed-plugin bridge bootstrap and post-bootstrap resolver-seal runtime are unchanged.

The launcher still does **not** start another CLR, load the game native executable, fabricate individual callbacks, or claim/set Godot `GDMono::runtime_initialized`. This remains diagnostic compatibility work, not exact Step-35 closure.

The authoritative exact-transformed Step-35 execution frontier remains physical **0.0.126**: exact `ExecuteVeryEarly()` entered `MethodInfo.Invoke`, but no `C_INVOKE_RETURNED` was durably recorded. All later Step-35 binaries are diagnostic derivatives unless a separately defined closure candidate restores explicit exact-byte execution authority.

## Prior localization and CI provenance

0.0.129–0.0.136 localized the pre-first-await failure through SaveManager/UserDataPathProvider/Platform/NullPlatform into `CommandLineHelper..cctor`, with 0.0.136 placing the hard termination between `CL_CRITICAL_001_PRE` and `CL_CRITICAL_001_POST` around Godot `Dictionary<string,string>` construction. The retired live-stack CL/CLTV probes remain negative instrumentation evidence.

0.0.137 was a pre-device Codemagic failure at **208/209** host tests due solely to the GodotSharp derivative verifier checking the sts2 bridge type. 0.0.138 corrected that verifier. 0.0.139 did not produce an IPA: static validation passed, Codemagic executed **210** host tests, **209 passed / 1 failed**, and the only failure was the stale Step-35.0.15 gate-summary assertion while production emitted Step 35.0.16. 0.0.140 corrected that release/test consistency defect. 0.0.141 likewise stopped before IPA packaging: static validation passed **853/853**, Codemagic executed **211** host tests, **210 passed / 1 failed**, and the sole failure was the new negative callback-table regression expecting zero checkpoints even though production intentionally emitted one durable `CB_INITIALIZE_MANAGED_FAIL` before any preflight/CLR work. 0.0.142 corrected that contract, passed **855/855 static checks**, **211/211 host tests**, and the Step-15 standalone native-link preflight, then failed during iOS C# compilation with CS0103 because the Step-35 partial omitted `using StS2Launcher.iOS.Platform;` while referencing `GodotStep15NativeBridge`. 0.0.143 corrected that compile-time namespace visibility defect and reached device execution.

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

## Physical 0.0.143 CORE-HANDOFF callback-table success and singleton frontier

Run `20260902T0723583787590Z-pid29589-9282faa3f81c4671894cabcfb6a117ec` was performed after Step 15 Gates A-C passed in the same process. The Step-15 engine reported `engineStarted=True`, `setup=True`, `interopReady=True`, `dotnetFeature=False`, and `godotDotNetInitialized=False`. It returned the exact source-built runtime interop table as **1,800 bytes / 225 pointers**. Private GodotSharp `NativeFuncs.Initialize(IntPtr,int)` entered at GS025, returned, and the launcher verified `initialized=true`.

The NATURAL sts2 path then passed `CL_CRITICAL_001_POST`, proving the former Godot dictionary callback crash is gone. `Godot.OS::.cctor()` progressed through repeated StringName and method-bind compatibility callbacks and completed far enough for `Godot.OS.GetCmdlineArgs()` to enter. The last durable checkpoint was **GS039 `Godot.OS::get_Singleton()`**; `CL_CRITICAL_002_POST` was not reached.

This closes the original callback-table hypothesis as positive diagnostic evidence: the exact source-built table is usable by the private GodotSharp derivative. The remaining failure is later in managed singleton acquisition/wrapping, not in dictionary/StringName callback initialization.

## Step 35.0.21 / 0.0.144 singleton-acquisition localization design

0.0.144 preserves CORE-HANDOFF behavior exactly and adds no compatibility bypass. The only runtime-diagnostic expansion is the entry-marker/reconnaissance closure around `Godot.NativeInterop.InteropUtils.EngineGetSingleton`, `UnmanagedGetManaged`, `Marshaling.ConvertStringToNative`, `NativeFuncs.godotsharp_engine_get_singleton`, and the native-to-managed script/instance-binding callbacks. Relevant `OSInstance`/GodotObject construction methods are also included when present.



The three prior controls are preserved unchanged. A fourth diagnostic mode, `GodotCoreCallbackHandoff`, is the sole exception to their no-Godot-state rule.

CORE-HANDOFF requires the project-owned Step-15 smoke engine to have already completed setup in the same process. The pinned Godot 4.5.1 iOS static build now enables `module_mono_enabled=yes` so native C# scaffolding and `runtime_interop.cpp` are present. The smoke project has no `dotnet` project feature. The native bridge refuses callback export unless Engine, ProjectSettings, CSharpLanguage and GDMono native state exist; it separately reports the `dotnet` feature and whether GDMono is initialized. The iOS UI refuses the handoff if either competing-runtime signal is true.

The bridge obtains `godotsharp::get_runtime_interop_funcs(size)`, rejects null/empty/non-pointer-aligned tables and null entries, and returns only pointer+size. The Step-35 strict load context then explicitly loads the already verified private GodotSharp diagnostic derivative, binds exact `Godot.NativeInterop.NativeFuncs.Initialize(IntPtr,int)`, requires its private static `initialized` field to be false, invokes Initialize exactly once, requires the field to become true, freezes resolver/native counters, and only then enters Gate C's natural ExecuteVeryEarly diagnostic path.

The game native executable is **not loaded**. No callback address is invented. ExecuteEssential, ExecuteDeferred, entry-point execution, native game resolution, arbitrary resolver fallback, and Harmony/MonoMod runtime patching remain forbidden. Exact source pins `0x06007D02` / `0x0600BC71` and the Step-32 transformed authority remain unchanged.

A 0.0.144 diagnostic 4/4, including CORE-HANDOFF, is **NOT Step-35 closure**.
