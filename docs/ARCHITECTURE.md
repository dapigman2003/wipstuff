# Canonical Architecture

The source tree is organized by responsibility rather than by the historical step that first introduced a component.

## Live projects

### `src/StS2Launcher.Core`

Platform-neutral logic. Subsystems:

- `Foundation` — state/controller/foundation primitives;
- `Steam` — authentication, ownership, content, download, install/update/repair, offline state;
- `Compatibility` — Cecil preparation/analysis/rewrite boundaries, including the physically closed Step-28 ahead-of-load transformation pipeline and the active Step-29 exact real-StS2 target audit;
- `Godot` — managed gate model for the source-built Godot host;
- `Runtime` — dynamic managed execution, runtime binding/framework closure, the physically proven first real game assembly load boundary, and the physically proven Step-24 controlled dependency initialization boundary; closed Step-25–27 runtime-Harmony experiment implementation is historical evidence rather than active code;
- `Diagnostics` — shareable report infrastructure.

### `src/StS2Launcher.iOS`

The single live iOS application project. It owns UIKit lifecycle/UI, iOS Keychain integration, and the managed side of the Godot native bridge.

The historical directory/project name `StS2Launcher.Step05.iOS` is not a live source path anymore. A pre-rename snapshot is retained only inside optional `history.zip`.

## Tests and fixtures

`tests/StS2Launcher.Core.Tests` mirrors Core subsystems and uses shared test infrastructure under `TestSupport`.

`fixtures/` contains project-owned regression fixtures. Step-numbered fixture assembly names are intentionally retained where tests/runtime gates use those exact identities; they are evidence fixtures rather than primary source architecture.

## Native source

`native/step15` retains the proven Godot 4.5.1 bridge/smoke source. The directory and exported C symbols remain step-labeled because they are an already-proven native ABI/test boundary; renaming them is not required for the canonical managed project layout and would add unnecessary native-link churn.

The active wrappers are generic:

- `scripts/build-godot.sh`
- `scripts/preflight-godot-link.sh`

## Current tooling

Only these shell entry points are active:

- `scripts/validate.sh`
- `scripts/test.sh`
- `scripts/build-godot.sh`
- `scripts/preflight-godot-link.sh`
- `scripts/build-ios.sh`
- `scripts/verify-ipa.sh`
- `scripts/codemagic.sh`

Historical wrappers are optional reference data inside `history.zip` and are never invoked by current tooling.

## Documentation

`docs/` is authoritative:

- `MASTER-PLAN.md` — long-lived architecture/roadmap/rules;
- `CURRENT-STATUS.md` — active physical/candidate state;
- `ARCHITECTURE.md` — this document;
- `TESTING.md` — current validation workflow;
- `REPORTS.md` — diagnostic file model;
- `RELEASE-CHECKLIST.md` — candidate acceptance checklist;
- `history/INDEX.md` + `history/steps/*.md` — readable chronological records.

`history.zip` is not documentation authority and may be absent from a build checkout without affecting CI.

## Protected runtime policies

The canonicalization does not change:

- dynamic-payload-compatible iOS host preservation: `MtouchLink=None` + `TrimMode=copy` (managed trimming disabled);
- SteamKit/protobuf roots and iOS build-only patch;
- DiskArbitration linker filtering;
- Godot 4.5.1 source/native link policy;
- Mono.Cecil 0.11.6 behavior;
- `MtouchInterpreter=-all`;
- the exact measured 22 Step 22 direct framework identities remain the proven binding frontier; their TrimmerRootAssembly descriptors are retained as historical/protection evidence but are not relied upon while copy/no-link is active;
- the separately classified `System.Collections.Concurrent` and later `System.Linq` roots remain physical evidence of why trimming was unsafe for post-publish payloads; copy/no-link supersedes them as the active preservation mechanism;
- Files-based diagnostic sharing;
- the Step 23 rule that real StS2 CLR loading occurs only in the explicit dedicated private load subsystem;
- initializer-bearing prepared dependencies remain excluded until an explicit controlled-initialization boundary admits a measured exact identity; Step 24 physically proved that exact admission and module-constructor completion for `0Harmony 2.4.2.0`;
- strict managed-plan resolution and native-load refusal remain in force across managed runtime boundaries. Steps 25–26 physically proved inert Harmony/PatchProcessor admission, while physical Step 27.0.24 / 0.0.108 closed runtime Harmony replacement negatively: the final representative post-publish interpreted target still failed at public `PatchProcessor.Patch()` with `NotImplementedException` from `PatchFunctions.UpdateWrapper`. Physical Step 28.0.2 / 0.0.111 then closed the replacement mechanism positively at A–E 5/5: verified immutable source -> private clone -> deterministic Cecil transformation before CLR load -> transformed-image verification -> transformed-only private runtime admission, including 1000 / 1041 / 1041 execution and post-execution OfflineReady 428/428. Physical Step 29.0 / 0.0.112 closed the read-only target-selection boundary at 4/4 and selected `ModManager.TryLoadMod(Mod) -> Harmony.PatchAll(Assembly)` without writing or CLR-loading game bytes. Physical Step 30.0 / 0.0.113 then closed the selected-site semantic-context boundary at 4/4 and formally deferred that Harmony/mod-loading site from the base-game frontier. Step 31 is the next read-only evidence boundary: bind the exact physical `OneTimeInitialization::PrewarmJit()` token/body fingerprint and all ten Step-29 `RuntimeHelpers.PrepareMethod` sites, inspect per-site IL/control-flow/exception context, and record rewrite-design eligibility only. Real StS2 semantic transformation/invocation, Godot/game startup, and native game loading remain later separately gated boundaries.

See `MASTER-PLAN.md` for rationale and roadmap.

## Step 31 physical closure / Step 32 active real-game transformation frontier

Physical Step 31.0 / 0.0.114 closed at 4/4 and established the exact `OneTimeInitialization::PrewarmJit()` / ten-`RuntimeHelpers.PrepareMethod` family as eligible for explicit rewrite design. Step 32.0 / 0.0.115 introduced the first real-game semantic write under the already-proven Step-28 architecture; Step 32.0.1 / 0.0.116 corrected the offset-sensitive verifier and then physically exposed exact `System.Runtime 9.0.0.0` Constant-table write resolution. Step 32.0.2 / 0.0.117 added only a write-time in-memory surrogate for that exact identity. Physical 0.0.117 re-proved Gate A and then failed closed before mutation on exact Sentry 5.0.0.0 metadata. Step 32.0.3 / 0.0.118 shrank the active compile/package surface without changing Step-32 semantics, and the user confirmed that lean baseline in Codemagic. Static analysis of the exact DLL then proved exactly three non-null external type/storage requirements: System.Runtime/BindingFlags/Int32 plus Sentry/BreadcrumbLevel/Int32 and Sentry/SentryLevel/Int16. Step 32.0.4 / 0.0.119 bounded the writer to exactly those requirements and physically advanced Step 32 to 2/4: Gate A passed and Gate B successfully serialized the first real-StS2 private semantic rewrite with nine approved write-time resolver requests and zero external dependency-byte reads. Gate C then failed before semantic verification because it treated the physical source MethodDef token `0x06007D05` as a post-Cecil-write transformed locator. Step 32.0.5 / 0.0.120 keeps that token authoritative for source admission/pre-write binding only; transformed reopen now binds by exact declaring type + full signature and then applies the existing semantic fingerprint, Constant-table fingerprint, instruction/EH, Pop-count, hash, isolation, and zero-CLR-load checks. The receipt-backed install remains immutable, and transformed-real-StS2 CLR admission/execution, game startup, Godot bridging, and native game loading remain later separately gated boundaries.

## Step 32 physical closure / Step 33 transformed-primary admission frontier

Physical Step 32.0.5 / 0.0.120 closed positively at 4/4. The receipt-backed real `sts2.dll` remained immutable and outside the CLR while a launcher-private clone received only the ten predeclared stack-neutral `RuntimeHelpers.PrepareMethod` suppressions. The transformed image was independently reopened and verified at SHA-256 `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`, with exact transformed PrewarmJit semantic fingerprint `47fadf2a46eda098f310b7d0ee54e37d1e952ac272fc966d16d557ed46a0b74a`, zero remaining PrepareMethod references, unchanged constant metadata, and unchanged assembly identity/MVID. This closes the first real-game semantic transform-before-load boundary.

Step 33 is the first transformed-real-game **CLR admission** boundary. It deliberately does not execute a game member. Gate A re-runs the closed Step-32 manufacture/verification contract and the existing Step-21/22 prepared-plan preflight. Gate B loads only the exact verified transformed primary bytes into a dedicated private `AssemblyLoadContext`. Gate C keeps the private context to transformed `sts2` only; private game dependency admission is not part of Step 33 and any such request fails closed, while only exact preplanned host-framework requests may be serviced from `AssemblyLoadContext.Default`. Gate D re-proves original/transformed/plan hashes, OfflineReady, unique transformed-context residency, and zero native/game-execution expansion. A Step-33 physical 4/4 pass would authorize design of a separate controlled transformed-site execution boundary.

## Step 33 physical closure / Step 34 exact transformed-site execution frontier

Physical Step 33.0 / 0.0.121 closed positively at 4/4. The exact Step-32 transformed image entered `StS2Launcher-Step33-TransformedGame`; admission caused zero managed resolver requests, zero host/private dependency loads, zero rejected managed requests and zero native attempts. The receipt-backed/prepared original primary never entered the CLR, and no game member was reflected or invoked. The authoritative report is `docs/history/reports/STEP-33.0-PHYSICAL-CLOSURE-4OF4.txt`.

Step 34.0 / 0.0.122 is the first controlled transformed-real-game execution boundary. It re-manufactures/reverifies the exact closed transformed image and prepared runtime plan, then admits only those transformed primary bytes into a new strict `StS2Launcher-Step34-PrewarmJit` context. The resolver may service only exact persisted host-framework bindings and hash-pinned initializer-free prepared private dependencies. The previously measured sole initializer-bearing private dependency, exact `0Harmony 2.4.2.0`, remains forbidden in this boundary. Gate C binds the exact transformed `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::PrewarmJit()` static parameterless void method at transformed token `0x0600AFEA` and invokes it exactly once. Any initializer-bearing dependency request, unplanned managed request, native request, or target exception fails closed and becomes the next evidence boundary. Game entry-point execution, broader managed startup, Harmony/MonoMod patching, Godot/game startup, and native game loading remain unauthorized.


## Step 34 physical closure / Step 35 very-early initialization frontier

Physical 0.0.122 closed Step 34 at 4/4: the exact transformed `OneTimeInitialization::PrewarmJit()` executed once and returned normally in `StS2Launcher-Step34-PrewarmJit`. Binding/invocation caused 8 managed requests, serviced as 6 exact persisted host-framework bindings plus 2 hash-pinned initializer-free prepared private dependencies. No initializer-bearing dependency, unplanned managed request, native request, receipt-backed original, entry point, Harmony patching, or Godot startup crossed the boundary.

The exact Step-35 compatibility authority established through 0.0.126 targets one measured method in the natural managed startup sequence: exact transformed `MegaCrit.Sts2.Core.Helpers.OneTimeInitialization::ExecuteVeryEarly()`. The exact source metadata binds this method at token `0x06007D02` as static/parameterless/`System.Threading.Tasks.Task`, with nested async state-machine `<ExecuteVeryEarly>d__7::MoveNext` at source token `0x0600BC71`. Under that exact experiment, Gate A distrusts serialization and proves source/transformed wrapper + MoveNext semantic equivalence with rejecting Cecil resolvers before any CLR load; Gate B admits only the exact transformed primary into `StS2Launcher-Step35-VeryEarly`; Gate C invokes only exact `ExecuteVeryEarly()` once and awaits its Task for at most 60 seconds; Gate D re-proves isolation. The resolver remains exact-plan only; initializer-bearing `0Harmony`, unplanned managed requests and native loads fail closed. `ExecuteEssential`, `ExecuteDeferred`, broader startup, Harmony patching and Godot/game startup remain separate future boundaries. Step 35.0.5 below is a diagnostic derivative of this still-open exact authority contract, not a replacement closure contract.

Physical Step 35.0 / 0.0.123 hard-terminated while the UI still appeared near Gate B. Step 35.0.1 / 0.0.124 added output-only crash checkpoints and physically resolved that ambiguity: Gate B passed completely; Gate C bound exact transformed `ExecuteVeryEarly()`, entered its first and only `MethodInfo.Invoke`, successfully serviced planned `GodotSharp`, `Steamworks.NET` and host-framework requests, and then hard-terminated before `C_INVOKE_RETURNED`. The matching `.ips` again faults the main thread at PC=`0x0` and shows effectively the same runtime-heavy stack path as 0.0.123. Step 35.0.2 / 0.0.125 kept execution authority frozen and added a pre-CLR output-only static IL/callsite map. Physical 0.0.125 reproduced the same native failure family, but the available static map and crash report were from different process runs and the expected fixed-name checkpoint was absent. Step 35.0.3 / 0.0.126 therefore changes only diagnostic provenance: one immutable Run ID/PID is established before Gate A, a unique run-specific journal and same-run static map carry that identity, `Step35-CurrentRun.txt` names the exact pair, and `Step35-LastCheckpoint.txt` is independently flushed after each checkpoint. If the initial journal cannot be durably established, Gate A is refused; if the map cannot be durably established after Gate A, Gate B is refused. These are diagnostic stops and do not broaden or redefine compatibility authority.


### Step 35.0.5 diagnostic clone
0.0.126 established trustworthy same-run correlation but still ended after the final planned `System.Collections.Concurrent` 8→9 resolver pass before `C_INVOKE_RETURNED`. 0.0.128 therefore adds a separate diagnostic clone layer after exact transformed-source verification. The clone preserves identity/MVID and adds a bridge plus `INMETHOD_*` entry calls. The bridge is armed only immediately before Gate-C invocation. This is instrumentation, not a compatibility transform, and does not authorize later startup boundaries.

### Step 35.0.6 deferred diagnostic-clone open
Physical 0.0.128 repeated the normal Gate-A `System.Runtime 9.0.0.0` failure before CLR admission. Review against the exact trusted game assembly localized the defect to the diagnostic clone source open: 0.0.128 used Cecil `ReadingMode.Immediate` while the bounded writer resolver was still unconfigured. Immediate metadata materialization can force enum/constant resolution before `Configure` can synthesize the audited surrogates. Step 35.0.6 / 0.0.129 therefore mirrors the physically proven Step-32 writer ordering exactly: file-backed deferred open with zero resolution, audit/configure the bounded in-memory constant-metadata surrogates, then serialize, validate all write-time requests, reopen under rejecting resolution, and reverify constant metadata/identity/MVID/markers. This changes no runtime resolver or game-execution authority.


### Step 35.0.7 generic delegate MemberRef correction
Physical 0.0.129 proved the deferred writer path and advanced the diagnostic clone through Gate A/B into the armed Gate-C invocation. The bridge callback field reflected and accepted a launcher `Action<string>`, but the first callback call returned `MissingMethodException` because the synthetic MemberRef was encoded as `Action<string>::Invoke(string)`. The constructed generic declaring type must retain the declaring type generic variable in the member signature. 0.0.130 therefore models open `Action<T>`, constructs the field as `Action<string>`, encodes `Invoke(!0)`, and post-write verifies that exact VAR(0) shape under rejecting resolution. No runtime resolver or exact-game execution authority changes.

### Step 35.0.8 Save/Platform/Godot callsite localization
Physical 0.0.130 proved the Step-35.0.7 `Action<string>::Invoke(!0)` bridge correction and emitted durable game-body markers through `SaveManager.get_Instance`, including the nested second `TestMode.get_IsOn` call reached while constructing the default manager. No settings-init marker was reached. Exact `sts2.dll` analysis narrows the normal branch to `SaveManager.ConstructDefault` -> `UserDataPathProvider.GetAccountScopedBasePath` -> `PlatformUtil.get_PrimaryPlatform` / `PlatformUtil..cctor` -> `NullPlatformUtilStrategy..ctor` -> `GodotFileIo..ctor` -> `GodotFileIo.CreateDirectory`. The latter contains one `Godot.DirAccess.DirExistsAbsolute` call and one conditional `MakeDirRecursiveAbsolute` call. Step 35.0.8 / 0.0.131 therefore adds only entry markers on that path plus serialized pre/post markers around those two callsites. It does not initialize Godot or broaden resolver/native/game-startup authority. A diagnostic 4/4 remains derivative evidence and cannot close exact Step 35.

### Step 35.0.9 NullPlatform constructor callsite localization
Physical 0.0.131 disproved the prior assumption that the first physically reached Godot directory call was the immediate frontier. The run advanced through `SaveManager.ConstructDefault`, `UserDataPathProvider.GetAccountScopedBasePath`, `PlatformUtil..cctor`, and the entry of `NullPlatformUtilStrategy..ctor`, then terminated before `GodotFileIo..ctor` emitted its entry marker. The final visible `System.Collections.Concurrent 8 -> 9` binding remains context only.

Step 35.0.9 / 0.0.132 therefore narrows instrumentation **inside the already-entered constructor**. It preserves all prior markers and wraps every original non-base `call`, `callvirt`, and `newobj` in `NullPlatformUtilStrategy..ctor` with ordered pre/post marker pairs. The base constructor is intentionally skipped so the diagnostic callback never runs while uninitialized `this` is held on the evaluation stack. The exact-source static map also includes the constructor's IL/CALLSITE ordinals so a physical marker can be mapped without reopening proprietary bytes. Any selected branch-target callsite fails instrumentation rather than silently changing coverage.

This remains diagnostic-only. No Godot bootstrap, native game startup, resolver broadening, Harmony/MonoMod runtime patching or later initialization authority is introduced.

## Step 35.0.15 comprehensive GodotSharp/native diagnostic architecture

Physical 0.0.133 and 0.0.135 proved that live-stack CommandLine PRE/POST callbacks could invalidate `CommandLineHelper..cctor` before instruction zero; raising and verifying MaxStack did not cure the failure. Physical 0.0.136 therefore removed runtime CL/CLTV live-stack sweeps, retained their exact-source maps only as output, and used four stack-neutral critical markers. That build entered the cctor and hard-terminated after `INMETHOD_CL_CRITICAL_001_PRE`, localizing the measured interval to `Godot.Collections.Dictionary<string,string>` construction before `_args` assignment.

The 0.0.137 comprehensive design added two derivative modes plus read-only GodotSharp/Mach-O reconnaissance, but Codemagic stopped at 208/209 host tests because the post-write GodotSharp entry-marker verifier checked calls against the sts2 diagnostic bridge type. No IPA/device evidence resulted.

Step 35.0.15 / 0.0.138 fixes only that verification boundary. `HasInjectedEntryMarkerAtStart` takes the expected bridge type; sts2 verification uses the existing default `ExecuteVeryEarlyCheckpointBridge`, while GodotSharp verification explicitly supplies `GodotSharpCheckpointBridge`. The runtime instrumentation itself remains entry-only for GodotSharp and stack-neutral at the CommandLine critical boundaries. NATURAL preserves the Godot dictionary contract; COMPAT rewrites only the four bounded dictionary references and leaves `Godot.OS.GetCmdlineArgs()` natural. Exact-source maps and native reconnaissance remain pre-CLR, output-only, and non-authorizing. This is localization architecture, not startup authorization.

## Step 35.0.16 callback-boundary / managed-command-line diagnostic architecture

Physical 0.0.138 established that the natural Godot Dictionary path reaches `NativeFuncs` callback-table plumbing and that the four-reference BCL Dictionary compatibility path advances into `Godot.OS::.cctor()` before `GetCmdlineArgs()` body entry. Step 35.0.16 treats those as two manifestations of the same GodotSharp/native callback boundary while preserving the existing no-Godot-bootstrap and native-load-refusal architecture.

The sts2 diagnostic derivative now has three explicit modes. NATURAL performs no CommandLine compatibility rewrite. OS-RECON applies exactly four substitutions to the private CommandLine Dictionary contract and retains the natural `Godot.OS.GetCmdlineArgs()` call. FORWARD applies the same four substitutions and rewrites exactly that one already-localized `GetCmdlineArgs()` call to a static method on the existing diagnostic bridge. The bridge method is deliberately self-contained (`new string[0]`) and introduces no new AssemblyRef or native dependency. Post-write verification proves its signature/body, the exact call counts, and the absence of a residual natural GetCmdlineArgs call in FORWARD.

The separate GodotSharp diagnostic derivative remains identity/MVID-preserving and entry-only. Its marker closure now includes roots at `Godot.OS::.cctor()` and `Godot.OS/MethodName::.cctor()` and explicitly includes StringName, `GodotObject.ClassDB_get_method_with_compatibility`, and relevant `NativeFuncs` callback thunks/cctors. No live-stack probes are added. Read-only reconnaissance follows the same expanded local graph while retaining P/Invoke/calli/callback-field and Mach-O inventory.

This is still localization/compatibility architecture, not Godot startup authorization. A future architecture decision must explicitly choose between establishing a legitimate Godot native callback/bootstrap path and continuing narrowly justified managed substitutions; Step 35.0.16 intentionally does neither globally.


## Step 35.0.17 release/test consistency architecture

Step 35.0.17 / 0.0.140 preserves the Step 35.0.16 three-mode runtime architecture exactly. The only change is candidate/provenance consistency: production gate summaries, host assertions, diagnostic filenames, UI/report labels, CI/IPA labels, and bundle identity advance together. Static validation now rejects a stale Step-35.0.15 success-summary assertion before Codemagic host execution. No resolver, rewrite, GodotSharp, native-callback, bootstrap, or invocation policy is broadened.

## Step 35.0.20 Godot core callback-handoff compile-integration correction

0.0.144 does not change the 0.0.142 callback-handoff runtime architecture. 0.0.142 already proved 855/855 static checks, 211/211 host tests, and the native-link preflight; its iOS compile then exposed one integration defect: the Step-35 partial omitted the `StS2Launcher.iOS.Platform` import required to resolve `GodotStep15NativeBridge`. 0.0.144 adds only that import plus provenance/static guards.

Physical 0.0.140 moved the problem out of CommandLineHelper: FORWARD passes both command-line boundaries and reaches `GodotFileIo.CreateDirectory`, where `Godot.DirAccess.DirExistsAbsolute` immediately enters the same StringName/NativeFuncs callback pipeline that killed OS-RECON. The active architecture therefore stops extending individual managed substitutes.

0.0.144 preserves NATURAL/OS-RECON/FORWARD as no-Godot-state controls and adds one explicit `GodotCoreCallbackHandoff` mode. The pinned Godot 4.5.1 iOS archive is rebuilt with `module_mono_enabled=yes`. The project-owned Step-15 smoke scene still has no `dotnet` feature, so the native C# language/GDMono scaffolding may exist while the Godot-managed runtime remains uninitialized. Runtime checks fail closed if that assumption is false.

The Step-15 native bridge exposes only readiness/state plus the pointer/size returned by upstream `godotsharp::get_runtime_interop_funcs`. It does not call a callback or initialize managed GodotSharp. Step 35 then loads only the separately verified private GodotSharp diagnostic derivative through its strict ALC, binds exact `NativeFuncs.Initialize(IntPtr,int)`, proves `initialized=false`, invokes once, proves `initialized=true`, and records the resolver baseline. Gate C proceeds only if no resolver/native activity changed between the handoff and the natural ExecuteVeryEarly binding.

The game native executable remains data-only evidence and is never loaded by the launcher. CORE-HANDOFF is a diagnostic of a legitimate engine prerequisite, not permission for broad Godot/game startup and not Step-35 closure authority.


## Step 35.0.21 singleton acquisition boundary

Physical 0.0.143 proves the project-owned Godot callback table can drive the private GodotSharp derivative through callback-backed dictionary, StringName, and method-bind operations. The next boundary is reverse object association: `OS.get_Singleton()` must obtain the native OS singleton and map it to a managed `OSInstance`. 0.0.144 instruments that boundary without creating a surrogate singleton or bypassing Godot semantics.

## Step 35.0.22 reverse-binding readiness boundary

The Step-15 source-built Godot native module now exposes two distinct interop-state observations. The already proven 225-pointer runtime interop table supplies managed->native GodotSharp callbacks. Physical 0.0.144 shows `UnmanagedGetManaged` then requires native->managed instance-binding state. 0.0.145 reads CSharpLanguage/GDMonoCache readiness only; it does not populate the managed callback cache. Gate C is fail-closed when reverse binding is absent.

