# Step 20 — Dynamic Managed Execution Foundation

## Objective

Prove on a physical iPhone that the Release Mono/.NET iOS host can execute a managed assembly that was **not a build-time/AOT input**, then prove one controlled transitive dependency load from launcher-private verified storage. Do this without loading any real StS2 assembly and without modifying the receipt-backed managed install.

This is intentionally placed before the runtime/framework binding subsystem. A correct StS2 dependency plan would not be useful if the host could only execute assemblies known to the AOT compiler at IPA-build time.

## Build/runtime policy

Step 20 uses:

```xml
<MtouchInterpreter>-all</MtouchInterpreter>
```

This keeps build-time assemblies AOT-compiled while retaining interpreter support for runtime/dynamic managed code. `UseInterpreter=true` is deliberately **not** used because it is equivalent to interpreting all build-time assemblies and would broaden the runtime change unnecessarily.

The three fixture projects target `net9.0` and are not referenced by the iOS app, Core project, or test project. `build-step20.sh` builds them separately and copies only their DLLs into `Step20DynamicFixtures/` **after** `dotnet publish` has completed, then generates a SHA-256 manifest. This ordering is a build invariant checked by `validate-step20.sh` and the final IPA verifier.

## Fixture graph

```text
DynamicFixture
  -> framework contracts only
  -> Run() = 42 using loop + generic Identity<T> + try/finally

RootFixture
  -> DependencyFixture
  -> framework contracts

DependencyFixture
  -> framework contracts only
  -> Add(40, 2) = 42
```

Gate A uses Mono.Cecil read-only metadata inspection to require each fixture to be IL-only, reject P/Invoke metadata, and reject unexpected non-framework assembly references before any runtime load.

## Gate A — FixtureIntegrityAndOfflineReady

1. Reset Step 20 evidence.
2. Re-prove Step 13 OfflineReady from the receipt/local files only.
3. Require the bundled fixture directory + 3-entry SHA-256 manifest.
4. For each fixture:
   - verify bundled SHA-256;
   - Cecil-read exact assembly identity;
   - require `ILOnly`;
   - reject P/Invoke metadata;
   - validate the expected reference boundary;
   - copy to `Documents/StS2Launcher/Step20-DynamicManagedExecution/fixtures`;
   - re-hash the private copy.
5. Copy/re-hash the manifest.
6. Record `RuntimeFeature.IsDynamicCodeSupported` and `IsDynamicCodeCompiled` diagnostically.
7. Do not load any game assembly.

## Gate B — DynamicFixtureExecution

1. Require Gate A evidence.
2. Assert no assembly named `sts2` is loaded.
3. Create a new noncollectible named `AssemblyLoadContext`.
4. Re-hash `DynamicFixture.dll` immediately before load.
5. Load from a memory stream containing the exact verified bytes.
6. Prove runtime assembly identity equals the Cecil-probed identity.
7. Reflect only the exact project-owned `DynamicFixtureProbe.Run()` method.
8. Require result `42`.
9. Require zero private fixture dependency loads.
10. Re-assert no `sts2` assembly is loaded.

The dedicated fresh load context prevents an assembly left resident by an earlier diagnostic run from implicitly satisfying the new Gate B.

## Gate C — PrivateDependencyResolution

1. Require Gates A and B evidence.
2. Create a second fresh load context.
3. Make only `StS2Launcher.Step20.DependencyFixture` available as a private dependency.
4. Load the root fixture from verified bytes.
5. On the dependency request:
   - require exact simple name/version/culture/public-key-token match against the Cecil-probed identity;
   - re-hash the dependency immediately before load;
   - load it into the same Gate C context;
   - reject any other non-framework fallback.
6. Execute `RootFixtureProbe.Run()` and require `42`.
7. Require exactly one verified private dependency load and prove it belongs to Gate C's load context.
8. Re-assert no `sts2` assembly is loaded.

Known platform/framework contracts can be delegated to the host runtime; arbitrary non-framework fallback is not allowed.

## Gate D — IsolationAudit

1. Re-hash all three launcher-private fixture DLLs.
2. Re-hash the private fixture manifest.
3. Re-run complete OfflineReady exact-tree verification.
4. Require the same depot ID, manifest ID and managed-install path as Gate A.
5. Require all three Step 20 fixture assembly identities to have executed/loaded during B/C.
6. Require the private Step 20 directory to contain exactly 3 DLLs + manifest.
7. Explicitly prove no `sts2` CLR assembly load.

## Explicitly out of scope

- real `sts2.dll` CLR loading or execution;
- StS2 static constructors;
- game/host `System.*` version unification or runtime binding;
- GodotSharp managed binding;
- Harmony/MonoMod detours;
- Reflection.Emit compatibility work;
- game native libraries, FMOD or Spine;
- Cloud or Workshop.

If Step 20 closes, the next subsystem should use this execution mechanism to design and prove the prepared **runtime/framework binding set** before first meaningful game initialization.
