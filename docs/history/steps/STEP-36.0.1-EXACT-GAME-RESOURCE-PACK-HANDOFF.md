# Step 36.0.1 — Exact game resource-pack handoff

Candidate: **0.0.155 (155)**

## Why this substep exists

Physical 0.0.154 reached the exact transformed `OneTimeInitialization.ExecuteEssential()` invocation after exact Step-35 core closure. The invocation failed deterministically with:

`MegaCrit.Sts2.Core.Localization.LocException: Path does not exist: res://localization/eng`

The Step-36 run then wrote its report and reached `RUN_END` normally. This moves the active frontier away from UI completion and away from managed/native interop. The missing lifecycle is the game resource filesystem: the source-built Godot instance is alive from the Step-15 smoke project, but the receipt-backed game PCK has not been mounted into `res://`.

## Bounded architecture

Step 36.0.1 retains the existing four-gate contract and does not alter game IL.

Gate B performs one additional bounded resource handoff before Gate C:

1. Consume the managed-install root captured when exact Step-35 Gate D passed.
2. Read the already-verified Step-12 receipt and require current schema/app/depot identity.
3. Require exactly one receipt entry at `SlayTheSpire2.app/Contents/Resources/Slay the Spire 2.pck`.
4. Recheck PCK existence and exact receipt length. The receipt SHA-1 is shape-validated but the multi-gigabyte PCK is not redundantly rehashed here because Step-35 Gate D has just completed the full OfflineReady proof.
5. Use the exact prepared GodotSharp assembly already resident in the strict Step-35 AssemblyLoadContext.
6. Bind `Godot.ProjectSettings.LoadResourcePack(string,bool,int/long)` and call it once with the exact PCK absolute path, `replaceFiles=false`, `offset=0`.
7. Probe the exact prior failure boundary `res://localization/eng` through exact `Godot.DirAccess.Open`.
8. Refuse Gate C unless the mount returned true and the localization directory probe returned a live wrapper.

`replaceFiles=false` keeps the handoff additive: this candidate is supplying the game resource tree without intentionally replacing files already present from the smoke project.

## Authority and security boundaries

The exact transformed Step-32 sts2 assembly remains the game CLR authority. The exact prepared GodotSharp assembly and previously proven 225-pointer/37-pointer bridge remain unchanged. No diagnostic derivative is admitted as Step-36 authority.

The resource-pack mount is a same-process, one-shot mutation of the live Godot resource filesystem. A failed Gate-B mount/probe or any Gate-C failure requires a fresh process before retry.

Still forbidden:

- `ExecuteDeferred`
- launcher-driven `PrewarmJit`
- game entry-point execution
- Harmony/MonoMod runtime patching
- arbitrary managed resolver fallback
- native game executable or game-native-library loading

## Final isolation

If exact `ExecuteEssential` returns and reaches state 2, Gate D performs the normal full OfflineReady reproof. That post-invocation audit re-hashes all receipt-backed files, including the PCK, and re-proves exact source/transformed hashes, prepared-plan/dependency hashes, exact CLR ownership, resolver/native confinement, mounted-PCK file continuity, and final initialization state.
