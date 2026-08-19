# Step 23 — First Real StS2 CLR Load Boundary

## Why this step exists

Step 20 proved post-publish IL execution with project-owned fixtures. Steps 21/22 produced and physically closed a zero-blocker host/private runtime binding plan for the real StS2 payload. Step 22.4.2 then completed the canonical foundation cleanup and passed every current regression on the physical iPhone.

Step 23 is therefore the first justified point to let the real prepared `sts2.dll` enter the CLR.

## Boundary

Step 23 is intentionally **load-only**. It does not intentionally inspect game types/members, invoke an entry point or method, instantiate game objects, run class constructors, initialize Godot/game state, or resolve native game libraries.

A dedicated custom `AssemblyLoadContext` owns the prepared private/game assemblies. Host framework contracts are returned only from the default context according to the persisted Step 21/22 plan. Private dependency bytes must come only from the receipt-hashed prepared set. Before loading, Gate A also proves that the persisted plan's source/request edges exactly cover the Cecil `AssemblyRef` metadata of every prepared assembly, so a stale/incomplete plan cannot silently authorize the load.

## Module initializer safety

Assembly loading can cross an initialization boundary when a module defines `<Module>..cctor`. Gate A therefore uses Cecil to inspect every prepared private assembly before any CLR load and refuses to continue if a module initializer exists. This is deliberately conservative; a future initialization subsystem may handle such code explicitly if real evidence requires it.

## Native safety

The Step 23 custom load context overrides unmanaged-library resolution and throws rather than falling through. P/Invoke and ModuleRef metadata are counted diagnostically at Gate A, but native libraries are not resolved in this step.

## Expected result

A 4/4 Step 23 result proves:

- the exact real `sts2.dll` can enter the interpreter-backed CLR context;
- every managed dependency identity in the physically closed plan can bind at runtime;
- host/private load-context ownership matches the plan;
- no native resolution was needed merely to establish the managed load graph;
- the trusted install/prepared bytes remain unchanged.

It does **not** prove game initialization, Harmony runtime patching, GodotSharp behavioral integration, native dependencies, rendering, audio, saves, or playability.
