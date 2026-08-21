# Testing — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Canonical static validation

Run `python3 tools/validate_current.py`.

The validator must protect the physically closed Step 23–26 boundaries while separately pinning Step 27.0.1 / `0.0.85 (85)`. It must require `TrimMode=full`, `MtouchInterpreter=-all`, the Step-22 roots, proven `System.Collections.Concurrent` root, proven Step-25 constructor preservation anchor, and the protected Step-26 implementation.

Step 27 must have exactly **26 ordered fail-fast gates A–Z**. Gates A–N replay Step 26. Gate O metadata-audits the exact patch APIs and the bounded `HarmonyLib.AccessTools::.cctor`/`all`/`allDeclared` surface without execution. Gates P/Q resolve and baseline-test only the launcher probe. Gate R explicitly completes the measured AccessTools initializer and verifies exact BindingFlags values. Gate S registers the exact prefix descriptor without patching. Gate T contains the first intentional `PatchProcessor.Patch()` call. Gates U–Z audit, prove patched behavior, unpatch, audit, prove restoration, and perform the final isolation audit. No StS2 member may be reflected, patched, or invoked.

## Host tests

Run `bash scripts/test.sh`.

Host tests enforce A–Z ordering, preserve the synthetic A–N Step-26 replay, verify the launcher patch probe, and include a metadata fixture proving the AccessTools audit accepts only the bounded static BindingFlags initializer shape. Host tests do not establish real iOS method replacement.

Expected TRX: `artifacts/test-results/step27.trx`.

## Codemagic

Workflow: `ios-step-27`.

Expected version: `0.0.85 (85)`.

Expected IPA: `artifacts/StS2-Launcher-Step-27.ipa`.

## Physical acceptance

From a fresh process, run Step 27 A–Z and stop on the first failure.

Require:

- A–N: exact closed Step 26 replay;
- O: exact patch API + AccessTools metadata admission only;
- P/Q: launcher target/prefix metadata and baseline result 42 with target calls 2 / prefix calls 0;
- R: explicit `RunClassConstructor(HarmonyLib.AccessTools.TypeHandle)` PASS, exact `all` / `allDeclared`, no descriptor/patch;
- S: exact prefix descriptor retained; no `Patch()`;
- T: exactly one `PatchProcessor.Patch()` completes with no target invocation;
- U: clean hashes/OfflineReady/context/native/resolver state;
- V: reflection + direct patched results = 1041, target body still 2, prefix calls = 2;
- W: exact prefix unpatch;
- X: clean post-unpatch state;
- Y: reflection + direct restored results = 42, target body = 4, prefix calls remain 2;
- Z: final isolation/integrity PASS;
- summary **26/26 PASS**;
- OfflineReady afterward **PASS**;
- Foundation afterward **5/5 PASS**.

Share `Reports/Step27-ControlledHarmonyPatchExecution.txt` on failure. After Gate T or later assume patch state may remain process-resident and force-quit before retrying.
