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
