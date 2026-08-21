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

## Physically proven platform policies that remain protected

These are architecture, not temporary step hacks:

- target `net9.0-ios`, `ios-arm64`, minimum iOS 18;
- bundle ID `com.community.sts2launcher`;
- global `TrimMode=full`;
- SteamKit2 3.4.0;
- Steam CM WebSocket transport;
- dedicated `SocketsHttpHandler` only for the CM WebSocket purpose while WebAPI/CDN remain on platform-default HTTP handling;
- trimmer roots `SteamKit2`, `protobuf-net`, `protobuf-net.Core`;
- build-only SteamKit iOS `Process.StartTime` compatibility patch against an isolated NuGet copy;
- remove only the generated `DiskArbitration` linker framework from the iOS link;
- source-built Godot 4.5.1-stable iOS host with the proven native bridge/link policy;
- Mono.Cecil 0.11.6 for controlled metadata/IL work;
- `MtouchInterpreter=-all`, with broad `UseInterpreter=true` and NativeAOT prohibited;
- the measured 22 direct host framework roots established by Step 22;
- the separately classified `System.Collections.Concurrent` preservation root physically proven by Step 24 for post-publish MonoMod/Harmony initialization while full trimming remains enabled;
- the bounded Step-25 `DynamicDependency` preservation anchor for framework types referenced by the physically measured `Harmony(string)` constructor IL, while full trimming and `MtouchInterpreter=-all` remain enabled;
- iOS Files access enabled for shareable diagnostic reports;
- no real StS2 assembly CLR load before the explicit first-load subsystem;
- no initializer-bearing prepared dependency is admitted automatically outside an explicit controlled-initialization subsystem with a measured target and fail-closed resolver/native policy.

## Data/workspace trust model

The Step 12 managed install and receipt are the trusted local game copy. Later compatibility/runtime subsystems must:

1. verify receipt-backed file identity before use;
2. clone/copy into a launcher-private workspace or prepared runtime set;
3. never resolve Cecil/runtime dependencies from arbitrary system paths, network locations, or the mutable live install unless a subsystem explicitly defines and verifies that source;
4. keep preparation deterministic and auditable;
5. preserve source/live-install bytes unless the install subsystem itself is doing an authenticated update/repair.

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

### Phase D — controlled managed initialization

This is the active major phase. Step 24 physically closed the first known automatic-initialization boundary: exact `0Harmony 2.4.2.0` can enter the dedicated private context and complete its module constructor under strict managed-plan resolution/native refusal, with the separately measured `System.Collections.Concurrent` preservation root. Step 25 then physically closed exact `HarmonyLib.Harmony` API resolution, explicit Harmony type initialization, and one inert `Harmony(string)` object construction, including the bounded framework-surface preservation required by the post-publish constructor IL. Step 26 physically closed exact `Harmony.CreateProcessor(MethodBase)` / `HarmonyLib.PatchProcessor` admission, explicit PatchProcessor type initialization, launcher-owned inert target metadata resolution, and empty processor construction without method replacement. The active frontier is now first real Harmony replacement on launcher-owned deterministic probes, including explicit pre-patch audit, observed patched behavior, exact unpatch, and observed restoration before any StS2 member is reflected or patched. After that launcher-only patch-engine boundary is physically characterized, the next major sub-boundary is targeted StS2 member reflection without invocation or patching. Keep StS2 game-member invocation, broad game reflection, Godot/game startup, and native game loading separately gated. Identify AOT/reflection/Harmony/runtime-service issues with one causal class per subsystem.

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
- Do not broaden roots, fallbacks, reflection preservation, native frameworks, or resolver search paths without measured evidence.

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
