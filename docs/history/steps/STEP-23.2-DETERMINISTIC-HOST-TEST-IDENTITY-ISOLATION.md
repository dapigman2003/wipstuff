# Step 23.2 — Deterministic Host-Test Identity Isolation

## Trigger

The Step 23.1 Codemagic run passed static validation and compiled the Core, but the host suite finished **147/154**. Seven failures shared one cause: a collectible synthetic assembly named `sts2` remained visible in the test process despite disposal, `Unload()`, and forced GC. That contaminated four Step 23 tests and three earlier Step 21 binding tests whose production guards correctly reject a loaded real-game assembly.

## Diagnosis

Collectible `AssemblyLoadContext` reclamation is intentionally GC-driven. A unit-test suite must not require the runtime to reclaim a collectible context on a particular schedule. Increasing GC loops is therefore not a deterministic isolation strategy.

## Correction

- Production Step 23 retains the exact physical fresh-process policy: the public constructor recognizes `sts2` and `SlayTheSpire2`.
- An internal test-only constructor accepts an expected synthetic primary identity and fresh-process identity set.
- `StS2Launcher.Core.Tests` receives friend access only for this seam.
- Every synthetic Step 23 test generates a unique assembly simple name.
- Synthetic contexts remain collectible, but tests no longer wait for or assert collectible-context reclamation.
- Negative Gate A tests still prove that their own unique synthetic primary was not loaded before the intended CLR-load boundary.

## Runtime scope

No physical-iPhone Step 23 resolver/load semantics changed. This is host-test architecture only.
