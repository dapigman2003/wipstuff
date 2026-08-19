# Step 14 — physical-iPhone compatibility-inventory gate

## Boundary

Inspect the already-legitimate, already-verified local StS2 managed install **read-only** and produce a useful compatibility inventory before attempting any runtime integration.

Step 14 does not launch the game, load its assemblies, rewrite IL, build Godot, or contact Steam.

## Prerequisite

Start with the repaired/valid Step 12 managed install that passed the Step 13 `OfflineReady` gate.

## Gate A — build/install regression

1. Build Codemagic workflow `ios-step-14`.
2. Install the IPA on the physical iPhone.
3. Confirm the header is `STEP 14 — COMPATIBILITY INVENTORY` and the version is `0.0.41`.
4. The existing Step 13 **Verify Offline-Ready Install (Local Only)** action should remain available as a regression control.

## Gate B — read-only inventory

1. Networking may be on or off; Step 14 itself must not use it.
2. Tap **Inventory Installed Game Compatibility (Read Only)**.
3. Allow the initial local Step 13 hash verification and the subsequent classification/managed-metadata scan to finish.

Required result contract:

```text
COMPATIBILITY INVENTORY PASS
OfflineReady precondition re-proven: YES
Total installed files/bytes: > 0 / > 0
Steam session consulted: NO
Network access attempted by Step 14: NO
Managed install modified by Step 14: NO
Game launch attempted: NO
```

The report must also provide the actual counts for:

- assets;
- Godot content;
- managed assemblies;
- native binaries;
- Godot/GodotSharp indicators;
- FMOD indicators;
- Spine indicators;
- reflection indicators;
- dynamic-code/JIT indicators;
- platform-specific indicators;
- potential iOS blocker signals and dependency notes.

**Do not require a particular count in advance.** Step 14 exists to discover the actual shape of the current Steam depot rather than encode assumptions about it.

Take screenshots (or transcribe the detail text) showing the counts, blocker signals, dependency notes, and the managed/native/dynamic-code evidence samples. Those findings determine what later compatibility steps should be isolated first.

## Gate C — integrity/read-only regression

After the inventory completes:

1. Run **Verify Offline-Ready Install (Local Only)** again.
2. It must still report `OFFLINE READY PASS` with the same exact-tree verification semantics.

This proves the Step 14 inventory did not mutate the managed game tree.

## Gate D — foundation regression

Run **Foundation 5/5 Regression** and require the existing pass.

## Interpretation rule

Metadata/path matches are **triage signals**, not proof that an API is executed. In particular:

- general reflection does not automatically mean “impossible on iOS”;
- a dynamic-code marker must be localized to real call sites later before deciding the required rewrite;
- a native desktop binary may be irrelevant if the future iOS host replaces that function;
- FMOD/Spine/Godot indicators establish dependencies that need later isolated handling, not permission to bundle proprietary assets.

## Completion rule

Step 14 is complete only after the physical iPhone produces a successful read-only inventory, Step 13 remains `OfflineReady` afterward, Foundation 5/5 remains green, and the actual inventory output has been reviewed.

Do not move into Step 15 or Mono.Cecil compatibility rewriting as part of this step.
