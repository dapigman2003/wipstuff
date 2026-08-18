# Step 18.1.1 — Host-Test Mono.Cecil API Compile Fix

Step 18.1 Codemagic stopped at the mandatory host-test compile gate before the iOS build.

The new workspace-only dependency-resolution regression test constructed `Mono.Cecil.TypeReference` using the named argument `isValueType: true`. The project is pinned to Mono.Cecil 0.11.6, whose public constructor does not expose a parameter with that name, causing `CS1739`.

Step 18.1.1 changes only the host regression test to pass the value-type flag as the fifth positional constructor argument. Runtime launcher code, the workspace-only resolver, app version `0.0.48 (48)`, and the physical Step 18 gates are unchanged.

`validate-step18.sh` now rejects the unsupported `isValueType:` form and requires the compile-safe positional construction.
