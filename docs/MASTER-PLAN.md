# StS2 Launcher iOS — Master Plan

## Purpose

This is the long-lived technical plan for the project. It is intentionally written to remain useful across many releases and should change only when architecture, scope, safety rules, or the major roadmap changes.

A new engineer or a new ChatGPT session should be able to resume the project without a bespoke handoff by reading, in order:

1. `docs/MASTER-PLAN.md` — architecture, invariants, roadmap, and engineering rules.
2. `docs/CURRENT-STATUS.md` — the current physically proven boundary and active candidate.
3. `docs/ARCHITECTURE.md` — canonical source/runtime structure.
4. `docs/REGRESSION-CONTRACTS.md` — current capability-level regression semantics when later steps intentionally change an earlier intermediate runtime state.
5. `docs/TESTING.md` and `docs/REPORTS.md` — authoritative validation loop and diagnostics.
6. `docs/history/INDEX.md` — chronological evidence and step-specific records when deeper context is needed.

A handoff document may still be generated for convenience, but it is not an authoritative project dependency.

## Product objective

Build an experimental, unofficial iOS launcher/compatibility host for Slay the Spire 2 (Steam App ID 2868840) for legitimate owners. The intended end state is:

1. sideload the launcher onto an iPhone;
2. authenticate with Steam, including Steam Guard when required;
3. verify ownership;
4. discover/download/update the legitimate game depot;
5. install and verify it atomically and support offline-ready use afterward;
6. prepare the managed game/runtime payload for iOS's no-JIT environment;
7. run the game through the embedded Godot 4.5.1 iOS host;
8. support save persistence and Steam Cloud synchronization when online;
9. optionally support compatible Workshop content and launcher-level display/performance controls;
10. harden the launcher for repeatable updates, repair, diagnostics, and recovery.

## Non-negotiable security and content boundaries

The repository/source archive must never contain:

- Slay the Spire 2 game payloads or a copied `sts2.dll`;
- Steam passwords, Steam Guard secrets/codes, reusable refresh tokens, or session secrets;
- Apple signing certificates, provisioning secrets, or private signing keys;
- proprietary FMOD/Spine assets or game-sourced proprietary native binaries;
- a mechanism that mutates the trusted Step 12 install in place during compatibility preparation.

Reusable Steam authentication material belongs only in iOS Keychain/runtime storage. The user-owned downloaded install is the source of game data. Compatibility work happens in private copied/prepared workspaces.

## Authority model

Different environments answer different questions:

- **Static local validation**: source structure, policy, manifests, shell/XML/plist/YAML correctness, source cleanliness.
- **Codemagic/macOS**: C# compile/unit tests, .NET iOS AOT/linker behavior, Godot native source build, final IPA structure/native-link verification.
- **Physical iPhone**: runtime truth. A capability is not closed until the required physical-device gate/regression sequence passes.

Never fabricate an IPA or claim physical success from source inspection alone.

## Engineering cadence

Use one tightly related subsystem per release with ordered gates. Closely related sequential questions may share one candidate/device run when each meaningful proof boundary has its own gate; this is how the project moves faster without sacrificing causal attribution or rigor. Stop at the first failing gate inside that subsystem. Preserve every physically proven fix unless evidence requires changing it.

The normal loop is:

1. inspect the newest source, build artifacts, and device text reports;
2. identify the first unproven/failing boundary;
3. modify the project directly;
4. run local static validation and source-archive audits;
5. return a Codemagic-ready source ZIP;
6. build in Codemagic;
7. install on a physical iPhone;
8. run gates in order and report the first failure using text files where practical;
9. after subsystem success, run OfflineReady and Foundation 5/5 regressions before formal closure.

## Canonical source architecture

The live source is intentionally small and must not be constrained by historical filenames:

- `src/StS2Launcher.Core/` — platform-neutral launcher, Steam, install, compatibility, runtime planning, Godot gate model, diagnostics.
- `src/StS2Launcher.iOS/` — the one live iOS application project and iOS-only UI/Keychain/native-bridge integration.
- `tests/StS2Launcher.Core.Tests/` — host tests organized by subsystem.
- `fixtures/` — project-owned regression/dynamic-IL fixtures. These are not game payloads.
- `native/` — project-owned Godot host module/smoke source plus build inputs.
- `scripts/` — current build/test/validation entry points only.
- `tools/` — build-time patcher and static validation support.
- `docs/` — authoritative plan, current documentation, and readable historical records.
- `history.zip` — optional, inert reference archive. Active code, tests, CI, and validation must never depend on it.

The live iOS project is `src/StS2Launcher.iOS/StS2Launcher.iOS.csproj`. The old `StS2Launcher.Step05.iOS` name is historical only.

## External StS2/mobile reference implementations

The project may use external Android StS2 launcher/compatibility repositories as **advisory references only**:

- `https://github.com/Ekyso/StS2-Launcher` — the earlier Android/mobile StS2 launcher reference used for architecture, compatibility ideas, and target discovery.
- `https://github.com/SocialHummingbird/StS2-Launcher-Overhaul` — a later/evolved Android ARM64 implementation with broader real-StS2 Harmony patch coverage, current startup/main-menu evidence, mod/runtime compatibility work, and Godot/native mobile findings.

These repositories are not authorities for this iOS runtime. Android/Mono/custom-Godot success never closes a .NET 9 iOS AOT, trimming, resolver, native-link, or physical-device boundary. Their most valuable uses are to suggest StS2 types/members worth measuring, known patch signatures, startup ordering, GodotSharp behavior, platform/native incompatibilities, and later mod-loader compatibility risks. Before this project reflects, patches, or invokes any suggested StS2 member, re-verify the exact type/member/signature and relevant IL against the receipt-backed macOS ARM64 payload used by this launcher, then prove the iOS boundary through this project’s own gated physical-device sequence. Never copy game payloads or treat an externally patched `sts2.dll` as trusted input.

## Physically proven platform policies that remain protected

These are architecture, not temporary step hacks:

- target `net9.0-ios`, `ios-arm64`, minimum iOS 18;
- bundle ID `com.community.sts2launcher`;
- dynamic-payload-compatible iOS managed host: `MtouchLink=None` + `TrimMode=copy`, so framework/user assemblies shipped with the host are not member-trimmed before receipt-backed StS2/Harmony/mod assemblies arrive after publish;
- SteamKit2 3.4.0;
- Steam CM WebSocket transport;
- dedicated `SocketsHttpHandler` only for the CM WebSocket purpose while WebAPI/CDN remain on platform-default HTTP handling;
- historical SteamKit/protobuf trimmer-root descriptors remain recorded, but copy/no-link is now the authoritative preservation mechanism for dynamic post-publish managed payload compatibility;
- build-only SteamKit iOS `Process.StartTime` compatibility patch against an isolated NuGet copy;
- remove only the generated `DiskArbitration` linker framework from the iOS link;
- source-built Godot 4.5.1-stable iOS host with the proven native bridge/link policy;
- Mono.Cecil 0.11.6 for controlled metadata/IL work;
- `MtouchInterpreter=-all`, with broad `UseInterpreter=true` and NativeAOT prohibited;
- the measured 22 direct host framework identities established by Step 22 remain the authoritative binding frontier; their root descriptors are retained as evidence, not as the complete member-preservation mechanism;
- the separately classified `System.Collections.Concurrent` and `System.Linq` preservation failures/roots are retained as physical evidence that publish-time trimming is unsafe for the post-publish managed payload model;
- the bounded Step-25 `DynamicDependency` preservation anchor remains historical/protection evidence for the measured `Harmony(string)` surface; `MtouchInterpreter=-all` remains enabled under the copy/no-link host policy;
- iOS Files access enabled for shareable diagnostic reports;
- no real StS2 assembly CLR load before the explicit first-load subsystem;
- no initializer-bearing prepared dependency is admitted automatically outside an explicit controlled-initialization subsystem with a measured target and fail-closed resolver/native policy.
- runtime Harmony/MonoMod method replacement is **not** an active iOS compatibility mechanism: physical Step 27.0.24 / 0.0.108 failed the exact public `PatchProcessor.Patch()` boundary on a genuine post-publish interpreted target after trimming ambiguity was removed; no further Harmony-internal workaround iteration is planned.
- compatibility behavior changes are now applied deterministically **ahead of CLR load** to verified launcher-private copies with Mono.Cecil, then the transformed image is reopened/verified and only that image may enter the private runtime context. The receipt-backed Step 12 source remains immutable.

## Data/workspace trust model

The Step 12 managed install and receipt are the trusted local game copy. Later compatibility/runtime subsystems must:

1. verify receipt-backed file identity before use;
2. clone/copy into a launcher-private workspace or prepared runtime set;
3. never resolve Cecil/runtime dependencies from arbitrary system paths, network locations, or the mutable live install unless a subsystem explicitly defines and verifies that source;
4. keep preparation deterministic and auditable;
5. preserve source/live-install bytes unless the install subsystem itself is doing an authenticated update/repair;
6. for behavior-changing compatibility work, verify the transformed output before load and do not CLR-load the immutable source copy in the same execution path merely to compare behavior.

## Diagnostic policy

Long results should be files, not screenshots.

- Device test/regression reports live under `Documents/StS2Launcher/Reports/*.txt` and are visible in the iOS Files app.
- Larger specialized diagnostics may live directly under `Documents/StS2Launcher/` with a stable descriptive filename.
- Codemagic/local build reports live under `artifacts/reports/`; detailed logs and test artifacts live under `artifacts/logs/` and `artifacts/test-results/`.
- Reports must exclude credential UI values, Steam reusable tokens/Guard secrets, Apple signing secrets, and unnecessary absolute host paths.

## Major roadmap

### Phase A — launcher/content foundation

Authentication, ownership, depot discovery/download/resume, atomic install/update/repair, offline readiness, compatibility inventory. This phase is physically proven.

### Phase B — iOS execution/compatibility foundation

Godot iOS host, Cecil runtime viability, real call-site analysis, controlled rewrite workspace, expression/no-dynamic-code behavior, post-publish managed IL execution, runtime/framework binding and host framework closure. This phase is physically proven through Step 22.

### Phase C — first real managed game load

Load the prepared real `sts2.dll` into the private execution context with exact dependency resolution active, verify identity and load-context behavior, and **do not intentionally invoke the game entry point or trigger broad initialization yet**. This phase is physically proven: Step 23 closed the first-real-load boundary while keeping initializer-bearing dependencies deferred.

### Phase D — ahead-of-load managed compatibility + controlled initialization

This is the active major phase. Steps 24–26 physically proved that the real `0Harmony 2.4.2.0` assembly can be admitted and initialized in the private iOS managed context and that inert `Harmony` / `PatchProcessor` objects can be constructed safely. Step 27 then isolated the first real replacement boundary. Physical 0.0.105 and 0.0.106 exposed two independent publish-time trimming failures (`Enumerable.Union<T>` and `DebuggableAttribute`), which established the architectural need for `MtouchLink=None` + `TrimMode=copy` because receipt-backed StS2/Harmony/mod assemblies arrive after publish and are invisible to ILLink. Physical 0.0.107 removed those trimming failures and reached `PatchProcessor.Patch()`, which threw `NotImplementedException` from `PatchFunctions.UpdateWrapper`. The final Step-27 stop-rule candidate, physical 0.0.108, repeated that failure against a genuine post-publish interpreted target whose direct in-fixture IL execution was proven immediately beforehand. That removed the AOT-target ambiguity and **closed runtime Harmony/MonoMod replacement as a negative architecture result**.

Physical Step 28.0.2 / 0.0.111 closed deterministic ahead-of-load managed transformation positively at 5/5: immutable source bytes were cloned, one exact Cecil semantic change was applied before CLR admission, source/transformed images were reopened and hash-verified, only transformed bytes entered the private context, both reflection and an in-fixture direct managed IL call observed 1000 / 1041 / 1041, and final OfflineReady/isolation passed. Physical Step 29.0 / 0.0.112 then closed exact receipt-backed target selection at 4/4 without writes or CLR admission and selected `ModManager.TryLoadMod(Mod) -> Harmony.PatchAll(Assembly)`. Because that highest-priority site is structurally in the mod-loading path, the active frontier is a read-only semantic-context/product-scope audit before any rewrite; priority rank alone does not override the rule that Workshop/Harmony-mod compatibility is later and must not block base-game startup. After that disposition, the next non-mod compatibility family must receive its own exact semantic audit before one narrowly predeclared real-StS2 transformation is authorized. Runtime Harmony detours are not to be revived. Keep real StS2 transformation, real member invocation, Godot/game startup, native game loading, and broad mod compatibility separately gated.

### Phase E — Godot/game integration

Connect the real managed game side to the already-proven embedded Godot 4.5.1 host, resolve GodotSharp/native ownership boundaries, and reach first controlled game scene/render-loop behavior.

### Phase F — native/platform compatibility

Address real native dependencies and platform-specific assumptions only when the execution path reaches them. Preserve the rule against bundling proprietary native components unless their licensing/distribution and iOS viability are explicitly established.

### Phase G — playability and persistence

Input, lifecycle, audio/platform behavior, saves, suspend/resume, crash recovery, and offline play.

### Phase H — online features

Steam Cloud first, then optional Workshop compatibility. Keep authentication/session security isolated from game runtime concerns.

### Phase I — performance and release hardening

Startup time, memory, prepared-payload caching, deterministic repair/update migration, diagnostics, user controls, and final regression coverage.

## Gate design rules for future subsystems

- A gate must test one named boundary and produce actionable diagnostics.
- A candidate may include several adjacent sequential gates when that saves build/device cycles, provided each passed gate establishes a distinct claim and later gates cannot obscure the first failure.
- Gate A should normally validate prerequisites/inputs, not perform the riskiest mutation/action immediately.
- If Gate N fails, Gates N+1 onward are unproven.
- A diagnostic-only failure that intentionally reports readiness `NO` can still close a subsystem if that was the defined purpose, but runtime actions must honor the readiness state.
- Avoid tests whose success depends on framework implementation details that are not actual consumer binding requirements.
- Prefer proving host/platform behavior directly over rewriting copied desktop framework binaries.
- Do not re-enable trimming, broaden fallbacks/reflection preservation, add native frameworks, or widen resolver search paths without measured evidence and an explicit architecture review.

## Definition of a closed step

A step/subsystem is closed only when:

1. Codemagic compilation/unit/native/IPA verification succeeds;
2. all required physical-device gates pass according to the step definition;
3. the requested post-step regressions pass (normally OfflineReady + Foundation 5/5 for runtime/compatibility work);
4. no protected earlier capability has regressed;
5. the evidence is recorded in `docs/CURRENT-STATUS.md` and, when useful, a step record under `docs/history/steps/`.

## Documentation maintenance model

- `MASTER-PLAN.md`: change rarely; only architecture, scope, rules, or major roadmap changes.
- `CURRENT-STATUS.md`: change every meaningful candidate/physical closure.
- `REGRESSION-CONTRACTS.md`: change only when a later subsystem intentionally changes the current meaning of an earlier regression.
- `ARCHITECTURE.md`, `TESTING.md`, `REPORTS.md`, `RELEASE-CHECKLIST.md`: change when the canonical implementation/process changes.
- `docs/history/steps/`: append evidence/design records; do not rewrite old records to pretend they were created with newer knowledge.
- `history.zip`: optional reference only; never authoritative.

## Resumption rule

If conversation state is lost, do **not** infer the current boundary from version numbers or old scripts. Read `CURRENT-STATUS.md`, then `REGRESSION-CONTRACTS.md`, then use the latest physical text reports/artifacts. The current source and physical-device evidence override historical plans.

### Current post-transform progression

Physical 0.0.120 closed the first exact real-StS2 ahead-of-load semantic rewrite at Step 32. Physical 0.0.121 closed Step 33 transformed-primary CLR admission at 4/4. Physical 0.0.122 closed Step 34 by invoking exact transformed `OneTimeInitialization::PrewarmJit()` once under the strict prepared resolver and returning normally with 6 exact host-framework + 2 initializer-free private dependency loads and no initializer-bearing/unplanned/native escape. Step 35 advances only to exact transformed static parameterless Task-returning `OneTimeInitialization::ExecuteVeryEarly()` once under a 60-second boundary, preserving `ExecuteEssential`, `ExecuteDeferred`, initializer-bearing `0Harmony`, unplanned managed/native loading, game-entry execution and Godot/game startup as later separately gated boundaries. Physical 0.0.124 proved Gate B PASS and localized the repeated main-thread PC=`0x0` hard kill inside synchronous execution initiated by exact `ExecuteVeryEarly` `MethodInfo.Invoke`, after planned dependency/framework resolutions and before Invoke returned. 0.0.125 is diagnostic-only Step 35.0.2: execution authority stays frozen while a resolution-free static IL/callsite map of the verified transformed wrapper + MoveNext is emitted before CLR admission so the next discriminator can target the actual callsite rather than a guessed dependency.
