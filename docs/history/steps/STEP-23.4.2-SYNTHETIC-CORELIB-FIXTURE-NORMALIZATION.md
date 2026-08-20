# Step 23.4.2 — Synthetic Core-Library Fixture Normalization

## Trigger

Step 23.4.1 reached Core compilation and **154/155 host tests**. The only failing test was `DependencyModuleInitializerIsDeferredWhilePrimaryAndSafeClosureLoad`. Gate C attempted to resolve `mscorlib, Version=4.0.0.0` and .NET 9 rejected that legacy identity.

## Diagnosis

The reference was not part of the real StS2 prepared runtime. It was introduced by the Mono.Cecil host-test fixture when `TypeSystem.Void` was used to construct a synthetic `<Module>..cctor`. The Step 23 planner correctly included the emitted AssemblyRef and the production resolver correctly refused to alias it to `System.Private.CoreLib`.

## Correction

The synthetic assembly generator now constructs the initializer first, then normalizes its AssemblyRef table to exactly the intentionally declared fixture references before writing. The written fixture is reopened and the test setup fails immediately if a legacy `mscorlib` AssemblyRef remains. The synthetic binding-plan builder no longer contains a special `mscorlib`→`System.Private.CoreLib` mapping.

## Protected behavior

No production Step 23 load, resolver, binding-plan, deferred-initializer, Steam, install, Godot, or Step 21/22 behavior changed. The strict production rule remains: no cross-simple-name core-library aliasing.
