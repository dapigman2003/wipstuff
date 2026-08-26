# Canonical Architecture

The source tree is organized by responsibility rather than by the historical step that first introduced a component.

## Live projects

### `src/StS2Launcher.Core`

Platform-neutral logic. Subsystems:

- `Foundation` — state/controller/foundation primitives;
- `Steam` — authentication, ownership, content, download, install/update/repair, offline state;
- `Compatibility` — Cecil preparation/analysis/rewrite boundaries, including the physically closed Step-28 ahead-of-load transformation pipeline and the active Step-29 exact real-StS2 target audit;
- `Godot` — managed gate model for the source-built Godot host;
- `Runtime` — dynamic managed execution, runtime binding/framework closure, the physically proven first real game assembly load boundary, physically proven controlled dependency/Harmony initialization boundaries, and the preserved closed-negative Step-27 Harmony patch experiment;
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

Physical Step 31.0 / 0.0.114 closed at 4/4 and established the exact `OneTimeInitialization::PrewarmJit()` / ten-`RuntimeHelpers.PrepareMethod` family as eligible for explicit rewrite design. Step 32.0 / 0.0.115 introduced the first real-game semantic write under the already-proven Step-28 architecture; Codemagic exposed a Gate-C pre-serialization offset-fingerprint verifier defect, corrected by Step 32.0.1 / 0.0.116 without changing rewrite semantics. Physical 0.0.116 then passed Gate A and exposed the next serialization-specific boundary: Cecil `module.Write` needed exact `System.Runtime 9.0.0.0` enum metadata while rebuilding an unrelated Constant-table row. Step 32.0.2 / 0.0.117 keeps read/verification resolution fully rejecting and gives only the writer an in-memory, exact-identity constant-metadata surrogate synthesized from source constant values; it opens no external dependency bytes and rejects every other assembly identity. The same private `sts2.dll` clone receives only the ten stack-neutral `Pop` replacements, then the reopened image must preserve both the planned method semantic fingerprint and the complete Constant-table semantic fingerprint before any CLR admission. The receipt-backed install remains immutable, and transformed-real-StS2 CLR admission/execution, game startup, Godot bridging, and native game loading remain later separately gated boundaries.
