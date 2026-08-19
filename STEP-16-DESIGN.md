# Step 16 — Managed Preparation Foundation — Design

Step 16 is the second accelerated multi-gate subsystem release.

It introduces Mono.Cecil **0.11.6** into the launcher runtime for one tightly bounded purpose: prove that the physical iPhone can inspect/write managed assembly files as metadata/IL data under full iOS AOT/trimming, before any real StS2 rewrite is attempted.

## Ordered gates

### Gate A — FixtureRead

- Build a tiny launcher-owned managed fixture assembly during CI.
- Bundle that DLL as inert raw data under `Step16Fixtures/`.
- Open it with Mono.Cecil on-device.
- Verify its assembly/type/constant/method IL identity.
- Do not load or execute it.

### Gate B — FixtureRoundTrip

- Read the bundled fixture with Cecil.
- Write a copy only under launcher-private `Documents/StS2Launcher/Step16-ManagedPreparation/` scratch storage.
- Reopen that output with Cecil.
- Verify the original `RewriteMe()` constant remains `7`.
- Verify the bundled source fixture did not change.

### Gate C — ControlledIlRewrite

- Read the launcher-owned fixture.
- Replace only `RewriteMe()` with the deterministic body `ldc.i4 42; ret`.
- Write to launcher-private Step 16 scratch storage.
- Reopen and verify the rewritten IL is exactly the expected constant-return shape.
- Verify the fixture identity remains intact and the bundled source did not change.

### Gate D — RealStS2MetadataInspection

- Re-prove the Step 13 `OfflineReady` exact managed tree first.
- Read the Step 12 receipt.
- Revalidate the re-read receipt against the just-proven OfflineReady app/depot/manifest/branch/file-count/byte contract.
- Open each receipt-backed `.dll`/`.exe` candidate one at a time as a single Cecil `ModuleDefinition` using `ReadingMode.Deferred`.
- Deliberately avoid `AssemblyDefinition.Modules` and never call `Resolve()`, so Gate D does not follow multi-module sidecars or dependency assemblies.
- Enumerate types/methods, P/Invoke metadata, assembly references, and `System.Reflection.Emit` type references.
- Locate and inspect the real receipt-backed `sts2.dll`.
- Re-hash **every** `.dll`/`.exe` candidate against the trusted receipt after Cecil inspection, including `sts2.dll`.
- Never call `Assembly.Load`, resolve/execute the game, or write any real managed-install file.

## Scope boundary

Step 16 proves the **mechanism** needed for future offline managed compatibility rewriting. It does not yet decide which StS2 APIs need rewriting and does not modify any StS2 assembly.

The only Step 16 writes are project-owned fixture outputs under:

```text
Documents/StS2Launcher/Step16-ManagedPreparation/
```

The receipt-backed Step 12 managed install remains read-only.

## Step 15 regression

The physically proven Godot 4.5.1 host remains linked and its smoke project remains bundled. Step 16 does not change the known minor initial-orientation/presentation quirk from Step 15.
