# Canonical Architecture

The source tree is organized by responsibility rather than by the historical step that first introduced a component.

## Live projects

### `src/StS2Launcher.Core`

Platform-neutral logic. Subsystems:

- `Foundation` — state/controller/foundation primitives;
- `Steam` — authentication, ownership, content, download, install/update/repair, offline state;
- `Compatibility` — Cecil preparation/analysis/rewrite boundaries;
- `Godot` — managed gate model for the source-built Godot host;
- `Runtime` — dynamic managed execution, runtime binding/framework closure, the physically proven first real game assembly load boundary, the physically proven controlled dependency-initialization boundary, and the active targeted Harmony API/object-construction boundary;
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

- full trimming;
- SteamKit/protobuf roots and iOS build-only patch;
- DiskArbitration linker filtering;
- Godot 4.5.1 source/native link policy;
- Mono.Cecil 0.11.6 behavior;
- `MtouchInterpreter=-all`;
- the exact measured 22 Step 22 direct framework roots;
- the separately classified `System.Collections.Concurrent` preservation root is physically proven by Step 24.0.6 for the post-publish MonoMod/Harmony initialization path. It remains distinct from the exact Step-22 22-root direct framework set;
- Files-based diagnostic sharing;
- the Step 23 rule that real StS2 CLR loading occurs only in the explicit dedicated private load subsystem;
- initializer-bearing prepared dependencies remain excluded until an explicit controlled-initialization boundary admits a measured exact identity; Step 24 physically proved that exact admission and module-constructor completion for `0Harmony 2.4.2.0`;
- strict managed-plan resolution and native-load refusal remain in force while crossing controlled initialization; Step 25 may resolve only the exact measured Harmony API/type-initializer surface, explicitly complete only the measured `HarmonyLib.Harmony` type initializer under its own gate, and construct one inert Harmony object; patch/processor APIs, StS2 member invocation, Godot/game startup, and native game loading remain later boundaries.

See `MASTER-PLAN.md` for rationale and roadmap.
