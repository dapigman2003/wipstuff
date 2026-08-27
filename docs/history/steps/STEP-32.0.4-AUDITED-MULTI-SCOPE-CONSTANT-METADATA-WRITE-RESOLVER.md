# Step 32.0.4 — Audited Multi-Scope Constant-Metadata Write Resolver

Version: `0.0.119 (119)`

## Trigger

Physical 0.0.117 re-proved Step-32 Gate A and then failed closed in Gate B before any rewrite or `module.Write()` because the pre-write Constant-table inventory found exact `Sentry, Version=5.0.0.0, Culture=neutral, PublicKeyToken=fba2ec45388e2af0` in addition to the already-authorized exact `System.Runtime 9.0.0.0` scope.

The exact receipt-backed `sts2.dll` was then inspected statically. The audit is preserved in `docs/history/reports/STEP-32-STATIC-STS2-CONSTANT-METADATA-AUDIT.txt` and proves that, under the Step-32 non-null constant requirement rule, the exact DLL has only these three external type/storage requirements:

- exact `System.Runtime, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a` / `System.Reflection.BindingFlags` / `Int32` / non-nested;
- exact `Sentry, Version=5.0.0.0, Culture=neutral, PublicKeyToken=fba2ec45388e2af0` / `Sentry.BreadcrumbLevel` / `Int32` / non-nested;
- exact `Sentry, Version=5.0.0.0, Culture=neutral, PublicKeyToken=fba2ec45388e2af0` / `Sentry.SentryLevel` / `Int16` / non-nested.

The Sentry constants are default-parameter metadata on `SentryService`; they are not dependencies of `OneTimeInitialization::PrewarmJit()`.

User-confirmed Codemagic success for Step 32.0.3 / 0.0.118 establishes the lean active-source baseline before this semantic correction.

## Bounded correction

Step 32.0.4 keeps the 6+4 rewrite unchanged and changes only the Cecil write-time constant-metadata resolver.

Before mutation, `ConstantMetadataWriteResolver.Configure` now:

1. inventories non-null externally scoped Constant providers from the already verified source module without resolving dependencies;
2. requires the resulting distinct type/storage requirement set to equal the three audited requirements above exactly — missing, changed, nested, or additional requirements fail closed;
3. requires exactly one matching source `AssemblyRef` for each approved exact identity;
4. creates one in-memory surrogate per approved exact assembly identity;
5. synthesizes only the three audited enum definitions with their exact primitive `value__` storage types;
6. exposes only those exact surrogates to Cecil during `module.Write()`;
7. records every write-time assembly-resolution request and rejects every identity not represented by the configured audited surrogate set;
8. opens zero external framework/game assembly bytes.

No `DefaultAssemblyResolver`, search directory, filesystem probing, runtime `Assembly.Load`, dependency binding, or broad Sentry admission is introduced.

## Deliberately unauthorized scopes

The full Constant-table audit also found null-valued external Constant rows scoped to exact `GodotSharp 4.5.1.0` and `System.Collections 9.0.0.0`. They are **not** part of the current non-null requirement model and remain unauthorized. Any actual Cecil write-time request for either identity must still fail closed and become new evidence.

Likewise, another Sentry type, another Sentry version/token, a storage-type drift, a nested external constant type, or any additional external scope is rejected before the rewrite loop.

## Rewrite and verification invariants unchanged

The only semantic game-code transformation remains:

- 6 × `RuntimeHelpers.PrepareMethod(RuntimeMethodHandle)` → `Pop`;
- 4 × `RuntimeHelpers.PrepareMethod(RuntimeMethodHandle, RuntimeTypeHandle[])` → `Pop + Pop`.

All existing Step-32 evidence gates remain mandatory: exact receipt/source hash, MVID, token, body fingerprint and ten sites; no incoming branch target at a selected call; independent reopened semantic verification; unchanged full Constant-table semantic fingerprint; immutable trusted install; zero real-StS2 CLR admission/invocation; final OfflineReady reproof.

## Authority

Local static validation can prove source structure and fail-closed policy guards. Codemagic is the compile/full-host/iOS packaging authority. Physical iPhone evidence remains the final runtime authority.

Step 32 closes only when this candidate or a later evidence-driven correction passes Gates A–D **4/4** on the exact receipt-backed game image. Even then, transformed-real-StS2 CLR admission/execution remains a later separately gated boundary.
