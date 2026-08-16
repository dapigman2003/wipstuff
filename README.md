# StS2 Launcher iOS — Step 14 compatibility inventory

Experimental unofficial iOS launcher/compatibility-host project for users who legitimately own Slay the Spire 2 on Steam.

## Project state

**Steps 01–13 are complete and closed on a physical iPhone.** The physically proven Step 13 runtime is `0.0.40 (40)`: a valid Step 12 managed install can be re-hash-verified as `OfflineReady` with networking unavailable, while deliberately corrupted local content becomes `RepairRequired` and is recoverable through the existing online manager.

This archive is **Step 14 / `0.0.41 (41)`**.

## Step 14 boundary

Step 14 adds exactly one capability: a **read-only compatibility inventory** of the already-managed StS2 depot.

Before classifying anything, Step 14 reuses the Step 13 local inspector and requires the managed install to prove `OfflineReady`. It then reads the existing non-secret receipt and inventories the installed files without consulting Steam or modifying/launching the game.

The report includes:

- total files/bytes and broad asset counts;
- Godot content such as `.pck` / project-resource formats;
- managed assembly candidates recognized by CLR metadata signature;
- native binary candidates recognized by Mach-O/ELF/PE signatures and native-library paths;
- Godot/GodotSharp dependency indicators;
- FMOD indicators;
- Spine indicators;
- general reflection indicators;
- dynamic-code/JIT indicators such as `System.Reflection.Emit`, `DynamicMethod`, builder APIs and `Expression.Compile`;
- platform-specific file/API indicators;
- concise potential-iOS-blocker signals and dependency notes.

Managed assemblies are **not loaded or executed**. Step 14 scans only their raw metadata/string bytes for triage indicators. A hit means “inspect this in a later focused boundary”; it does not prove that the corresponding runtime path executes.

## Build

Use Codemagic workflow:

```text
ios-step-14
```

Expected app:

```text
0.0.41 (41)
STEP 14 — COMPATIBILITY INVENTORY
```

Expected IPA:

```text
artifacts/StS2-Launcher-Step-14.ipa
```

See `docs/STEP-14-TEST.md` for the physical-iPhone gate.

## Scope boundary

Step 14 does **not** add Mono.Cecil, rewrite assemblies, compose multiple depots, build/start Godot, load game assemblies, execute native/managed game code, launch StS2, or add Cloud/Workshop features.
