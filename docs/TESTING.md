# Testing — Step 28 Ahead-of-Load Managed Transformation

## Static validation

Run `bash scripts/validate.sh`.

For Step 28.0 / `0.0.109 (109)`, validation preserves all protected earlier runtime/build policies while adding the architecture-pivot invariants:

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
- no real StS2 member reflection, rewrite, or invocation occurs in Step 28.0.

The raw physical Step-27.0.24 negative report must remain preserved as the architecture-decision evidence.

## Host tests

Run `bash scripts/test.sh`.

The full historical regression suite still runs, including the quarantined/hash-pinned Step-27 Harmony normalizer tests. Step 28 additionally builds its fixture separately, copies it to `artifacts/host-step28-ahead-of-load-fixture/`, exports only that directory through `STS2_STEP28_AHEAD_OF_LOAD_FIXTURE_ROOT`, and runs the Step-28 host regression without adding a ProjectReference.

The Step-28 host regression creates a synthetic OfflineReady install, verifies source metadata/hash, performs the deterministic `1 -> 1000` rewrite into launcher-private scratch, reopens the transformed image, loads only transformed bytes, proves 1041 through both reflection and an in-fixture direct managed IL call, and verifies the synthetic trusted install plus bundled source remain unchanged.

## Codemagic / physical run

Codemagic must pass static validation, the complete host suite, iOS publish, and IPA verification. Then install `0.0.109 (109)` from a fresh process.

Run Step 28 A–E in order. A physical **5/5 PASS** closes only the combined ahead-of-load transformation/execution mechanism. The next candidate may then select a narrowly audited real StS2 compatibility transformation.

If Gate D has run, force-quit before any Step-28 retry because the fixture identity remains resident in its non-collectible private `AssemblyLoadContext`.
