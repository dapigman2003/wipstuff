# Step 23.1 — Host-Test Isolation Fix

Date: 2026-08-19

## Codemagic evidence

The initial Step 23 candidate passed canonical static validation (187/187), compiled Core successfully, and executed 154 host tests. 153 passed. The single failure was `GateARejectsModuleInitializerBeforeAnyRealClrLoad`.

The previous positive synthetic test had loaded a fake assembly named `sts2` into a collectible `AssemblyLoadContext`. Although the test disposed/unloaded that context, the async helper's state machine could retain the foundation strongly long enough that the next test still observed the collectible `sts2` assembly. Gate A correctly stopped on its production fresh-process guard before reaching the intended module-initializer assertion.

## Correction

No Step 23 production load/binding logic changed. The host-test helper now explicitly clears its `FirstRealGameAssemblyLoad` reference in `finally`, and collectible cleanup waits until no collectible synthetic `sts2` assembly remains instead of assuming four GC cycles are sufficient. If cleanup cannot complete, the responsible test fails immediately with an isolation-specific diagnostic rather than contaminating a later test.

Physical iOS production contexts remain non-collectible and retain the strict fresh-process requirement.
