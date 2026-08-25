# Testing — Step 28 Ahead-of-Load Managed Transformation

## Static validation

Run `bash scripts/validate.sh`.

For Step 28.0.2 / `0.0.111 (111)`, validation preserves all protected earlier runtime/build policies while retaining the Step-28 architecture invariants:

- runtime Harmony/MonoMod replacement remains historical only; Step 28 production code contains no Harmony patch API calls;
- `MtouchLink=None`, `TrimMode=copy`, and `MtouchInterpreter=-all` remain the active dynamic-payload host policy;
- `fixtures/StS2Launcher.Step28.AheadOfLoadFixture` exists as a standalone `net9.0` project;
- neither the iOS project nor host-test project references the Step-28 fixture project;
- `scripts/build-ios.sh` builds it separately and copies it into `Step28AheadOfLoadFixture/` only after `dotnet publish` returns;
- `scripts/verify-ipa.sh` requires exactly one byte-identical Step-28 fixture DLL and a valid SHA-256 manifest;
- source fixture IL is exact: `Adjustment()=>1`, `Target` directly calls `Adjustment`, and `InvokeTarget` directly calls `Target`;
- production Step 28 rewrites only a launcher-private transformed copy before CLR load, then reopens/verifies that image;
- the original bundled/private-source fixture identity must not already be CLR-loaded before Gate D;
- Gate D loads only transformed bytes and requires `Adjustment()==1000`, `Target(41)==1041`, and `InvokeTarget(41)==1041`;
- no real StS2 member reflection, rewrite, or invocation occurs in this mechanism-closing candidate;
- the private `CallbackProgress<T> : IProgress<T>` helper required by Gate A exists, stores a non-null `Action<T>`, and forwards `Report(T)` to that callback;
- Step-28 fixture metadata reads use `ReadingMode.Deferred`, retain the deliberately rejecting resolver, and contain no `ReadingMode.Immediate`.

The raw physical Step-27.0.24 negative report remains preserved as the architecture-decision evidence. The raw 0.0.109 Codemagic compile stop is preserved at `docs/history/reports/STEP-28.0-CODEMAGIC-CORE-COMPILE-FAILURE.txt`.

Expected local static result for 0.0.111: **859/859 PASS**.

Codemagic 0.0.110 authority: **compile PASS; 216/217 host tests PASS**. The sole failure was the end-to-end Step-28 test at Gate A with `AssemblyResolutionException` for `System.Runtime, Version=9.0.0.0`, caused by Immediate Cecil custom-attribute decoding before rewrite/load. This failure must remain preserved and must not be reclassified as transformed-execution evidence.

## 0.0.109 compile evidence

Codemagic 0.0.109 passed **845/845** static validation and built all external managed fixtures. Core compilation then failed before MSTest with `CS0246` at `AheadOfLoadManagedTransformation.cs(88,23)` because `CallbackProgress<>` was referenced but not declared. This is compile-only evidence: it is not a host-test, iOS publish, IPA, or physical runtime failure.

0.0.110 proved the callback-adapter compile correction, then produced a single host failure at Gate A because Cecil `ReadingMode.Immediate` eagerly requested `System.Runtime` through the rejecting resolver. 0.0.111 changes only Step-28 fixture reads to `ReadingMode.Deferred`, retains the rejecting resolver, and adds static guards against reintroducing Immediate mode. Gate ordering and semantic acceptance are unchanged.

## Host tests

Run `bash scripts/test.sh`.

The full historical regression suite still runs, including the quarantined/hash-pinned Step-27 Harmony normalizer tests. Step 28 additionally builds its fixture separately, copies it to `artifacts/host-step28-ahead-of-load-fixture/`, exports only that directory through `STS2_STEP28_AHEAD_OF_LOAD_FIXTURE_ROOT`, and runs the Step-28 host regression without adding a ProjectReference.

The Step-28 host regression creates a synthetic OfflineReady install, verifies source metadata/hash, performs the deterministic `1 -> 1000` rewrite into launcher-private storage, reopens both images, loads only transformed bytes, and requires both reflection and the fixture's own direct IL call to return the transformed behavior.

The current local environment has no `dotnet`; therefore a local `scripts/test.sh` run is expected to stop with `ERROR: dotnet is required to run host tests.` That is an environment limitation, not a candidate test verdict. Codemagic is the compile/full-host-test authority.

## Codemagic / device

Workflow: `ios-step-28`.

Codemagic must pass, in order: canonical static validation, Core/test compilation, the complete host suite, iOS publish, and IPA verification. The host suite must be **217/217 PASS** before publish. Then install `0.0.111 (111)` from a fresh process.

Run Step 28 A–E in order. A physical **5/5 PASS** closes only the combined ahead-of-load transformation/execution mechanism. Gate D must report **1000 / 1041 / 1041** and Gate E must include OfflineReady re-verification. The next candidate may then select a narrowly audited real StS2 compatibility transformation.

After any run that reaches Gate D, force-quit before any Step-28 retry because the fixture identity remains resident in its non-collectible private `AssemblyLoadContext`.
