# StS2 Launcher iOS — Step 23.4.2 First Real StS2 CLR Load Boundary

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


## Step 23.4.2 synthetic fixture correction

The Step 23.4.1 Codemagic run passed canonical static validation and Core compilation and reached **154/155 host tests**. The only failure was the synthetic dependency-module-initializer test. Cecil's `TypeSystem.Void` temporarily materialized a legacy `mscorlib, Version=4.0.0.0` AssemblyRef in that fake test assembly. Gate C correctly refused to alias that .NET Framework identity to `System.Private.CoreLib` on .NET 9.

Step 23.4.2 fixes only the synthetic fixture generator: it creates the initializer first, then normalizes the written fixture AssemblyRef table to exactly the references the test intentionally declared, and reopens the file to fail immediately if a legacy `mscorlib` reference reappears. **Production Step 23 binding remains strict and gains no core-library alias.**

## Codemagic

Use workflow:

`ios-step-23-4-2`

Expected app version: `0.0.71 (71)`.

## Documentation

Start with `docs/MASTER-PLAN.md` for the durable architecture/roadmap and `docs/CURRENT-STATUS.md` for the current physical boundary. Historical step records remain readable under `docs/history/steps/`. The optional `history.zip` is reference-only and is never a build dependency.
