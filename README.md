# StS2 Launcher — Step 35.0.31 / Step 36.0.1

Active candidate: **0.0.155 (155)** — exact receipt-backed game resource-pack handoff before controlled exact `ExecuteEssential`.

Physical **0.0.154** advanced beyond the Step-35 exact-authority closure into Step 36 and also demonstrated that the explicit-main-thread teardown architecture can reach a normal `RUN_END`. Step 36 Gate A re-proved exact source/transformed `ExecuteEssential` semantics, Gate B bound the exact transformed method with `OneTimeInitialization._state == 1`, and Gate C entered the first exact invocation. That invocation failed deterministically with `MegaCrit.Sts2.Core.Localization.LocException: Path does not exist: res://localization/eng`.

This is now a resource-filesystem lifecycle boundary rather than a managed resolver/native bridge boundary. The live source-built Godot process is still rooted in the Step-15 smoke project, while the receipt-backed game PCK has not been added to its `res://` resource filesystem.

**Step 36.0.1** addresses that architecture directly. Before `ExecuteEssential` is invoked, Gate B now:

- locates `SlayTheSpire2.app/Contents/Resources/Slay the Spire 2.pck` through the already-verified Step-12 managed-install receipt inherited from the exact Step-35 closure;
- rechecks receipt identity, depot-root identity, PCK receipt membership, file length, and SHA-1 shape without redundantly re-hashing the multi-gigabyte PCK immediately after Step-35 OfflineReady;
- binds the exact prepared `GodotSharp` `Godot.ProjectSettings.LoadResourcePack` API from the existing Step-35 load context;
- mounts the PCK additively with `replaceFiles=false` and `offset=0`;
- probes the exact prior failure path `res://localization/eng` through exact `Godot.DirAccess.Open`;
- refuses Gate C if either the pack mount or localization-directory probe fails.

Only after that bounded handoff does Gate C invoke the **unchanged exact transformed** `OneTimeInitialization.ExecuteEssential()` once. Gate D still performs the full OfflineReady reproof, including hashing the receipt-backed PCK, plus authority/plan/dependency/resolver/context/state checks.

Still forbidden: `ExecuteDeferred`, launcher-driven `PrewarmJit`, game entry-point execution, Harmony/MonoMod runtime patching, arbitrary resolver fallback, and native game executable/library loading.

## Physical test sequence

Use a fresh process:

1. Step 15 Gates A-C.
2. Without force-quitting or backgrounding, run Step 35 **EXACT-CLOSURE** once.
3. When the exact Step-35 core closure is complete, press **Step 36.0.1 A-D** once.
4. Preserve Step35 and Step36 run-correlated checkpoint/static-map/report files.

High-value new Step-36 markers are `E_B_PACK_RECEIPT_PASS`, `E_B_PACK_BIND_PASS`, `E_B_PACK_LOAD_RETURNED`, `E_B_LOCALIZATION_DIR_PROBE_RETURNED`, `E_B_GAME_RESOURCE_PACK_PASS`, and then the existing `E_C_INVOKE_START` / return-or-failure frontier.
