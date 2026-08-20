# StS2 Launcher iOS — Step 23.4.3 First Real StS2 CLR Load Boundary

This repository is the canonical launcher source after the Step 22.4.2 foundation closure.

Step 23 crosses the first real managed-game boundary. The real receipt-backed `sts2.dll` may enter one dedicated private `AssemblyLoadContext`, but no game entry point, type/member reflection, game method, Godot startup, or unmanaged game library is invoked.

## Step 23.4 correction

Physical Step 23.3 Gate A found exactly one prepared dependency with a `<Module>..cctor`: `0Harmony, Version=2.4.2.0`. The primary `sts2.dll` itself was not identified as an initializer offender.

Step 23.4 preserves the automatic-execution boundary instead of deleting the guard:

- Gate A still requires the primary `sts2.dll` to be module-initializer-free.
- Initializer-bearing *dependencies* are statically audited and explicitly deferred.
- Gate B loads only the real primary assembly.
- Gate C loads the maximal initializer-free private closure and all planned host bindings, while refusing any resolver request that would load a deferred initializer-bearing dependency.
- Gate D proves the deferred dependency never entered the CLR and that all prepared/live bytes remain unchanged.
- Gate A writes a compact Cecil IL audit for every deferred `<Module>..cctor` into the normal Step 23 text report so Step 24 can target the real initialization behavior rather than guessing.

The current known physical frontier is one deferred assembly: `0Harmony 2.4.2.0`.


## Step 23.4.3 synthetic fixture correction

The Step 23.4.2 Codemagic run passed canonical static validation and Core compilation and reached **153/155 host tests**. Both failures were synthetic module-initializer fixtures that still persisted a legacy `mscorlib, Version=4.0.0.0` AssemblyRef.

The prior fix attempted to clear that AssemblyRef after constructing the initializer. That was too late: Cecil had already embedded the legacy core-library scope in the initializer's `System.Void` TypeReference and recreated the reference during serialization.

Step 23.4.3 fixes the fixture **by construction**. For initializer-bearing synthetic assemblies it adds the real host `System.Runtime` AssemblyRef before accessing Cecil `TypeSystem.Void`; Cecil therefore uses that recognized modern core-library contract for primitive void. After write/reopen the fixture requires an exact declared-vs-persisted AssemblyRef set, forbids `mscorlib`, and verifies the initializer return type is primitive `MetadataType.Void` scoped to `System.Runtime`.

**Production Step 23 binding remains strict and unchanged. No legacy core-library alias is added.**

## Codemagic

Use workflow:

`ios-step-23-4-3`

Expected app version: `0.0.72 (72)`.

## Documentation

Start with `docs/MASTER-PLAN.md` for the durable architecture/roadmap and `docs/CURRENT-STATUS.md` for the current physical boundary. Historical step records remain readable under `docs/history/steps/`. The optional `history.zip` is reference-only and is never a build dependency.
