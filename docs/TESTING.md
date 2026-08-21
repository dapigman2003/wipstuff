# Testing — Step 27 Controlled Launcher-Owned Harmony Patch + Unpatch

## Canonical static validation

Run:

`python3 tools/validate_current.py`

The validator must protect the physically closed Step 23, Step 24, Step 25, and Step 26 boundaries while separately pinning the active Step 27 candidate.

It must verify at least:

- iOS version/release wiring = Step 27 / `0.0.84 (84)`;
- `TrimMode=full` and `MtouchInterpreter=-all` remain unchanged;
- the exact Step-22 root policy, proven `System.Collections.Concurrent` root, and proven Step-25 Harmony-constructor preservation anchor remain present;
- protected Step 23.4.3, Step 24.0.6, Step 25.0.2, and Step 26.0 implementation manifests match;
- Step 27 has exactly twenty-five ordered fail-fast gates A–Y;
- Gates A–N replay the closed Step 26 behavior;
- Gate O metadata-audits the exact `AddPrefix(MethodInfo)`, `Patch()`, `Unpatch(MethodInfo)`, and `HarmonyMethod(MethodInfo)` surface before execution;
- Gate P resolves only launcher-owned `HarmonyPatchProbe.Target(int)` and `Prefix(int, ref int __result)`;
- Gate Q proves deterministic pre-patch behavior;
- Gate R constructs/registers only the exact prefix descriptor without `Patch()`;
- Gate S contains exactly one intentional `PatchProcessor.Patch()` invocation and no target invocation;
- Gate T re-hashes/re-proves OfflineReady before patched execution;
- Gate U verifies patched behavior through reflection and direct routes with original-body suppression;
- Gate V contains exactly one intentional `PatchProcessor.Unpatch(MethodInfo)` invocation;
- Gates W/X verify clean post-unpatch state and restored behavior;
- Gate Y performs the final byte/OfflineReady/context/native audit;
- no StS2 member reflection/patch/invocation, broad Harmony patch discovery, Godot startup, or native game-library admission is introduced.

## Host tests

Run `bash scripts/test.sh`.

Step 27 host tests retain the Step 24/25/26 safety fixtures, enforce full A–Y gate ordering, replay the synthetic closed A–N chain, and verify the launcher patch probe's exact original/prefix semantics and reflection signature. Host tests do not claim that actual Harmony replacement works on iOS; the physical device remains the authority for Gates S–X.

Expected TRX:

`artifacts/test-results/step27.trx`

## Codemagic

Workflow: `ios-step-27`

The pipeline runs static validation, host tests, Godot/native preflight, iOS publish, and IPA verification. Build/CI never bundles or loads the proprietary game payload.

Expected:

- version: `0.0.84 (84)`;
- IPA: `artifacts/StS2-Launcher-Step-27.ipa`;
- host TRX: `artifacts/test-results/step27.trx`.

## Physical acceptance

From a fresh process, run Step 27 A–Y and stop at the first failed gate.

Require:

- Gates A–N: exact closed Step 26 replay;
- Gate O: exact patch API/metadata resolution only; no descriptor construction or patching;
- Gate P: exact launcher-owned target/prefix metadata only; no StS2 reflection;
- Gate Q: direct + reflection baseline results = 42, target calls = 2, prefix calls = 0;
- Gate R: exact prefix descriptor retained; no `Patch()` invocation and counters unchanged;
- Gate S: exactly one `PatchProcessor.Patch()` completes, returns replacement MethodInfo, and counters remain unchanged;
- Gate T: hashes/OfflineReady/context/native/resolver state clean before patched invocation;
- Gate U: reflection + direct results = 1041, target calls remain 2, prefix calls = 2;
- Gate V: exact `Unpatch(prefix MethodInfo)` completes; counters unchanged;
- Gate W: clean post-unpatch state before restored invocation;
- Gate X: reflection + direct results = 42, target calls = 4, prefix calls remain 2;
- Gate Y: final hashes, OfflineReady, private context, retained identities, native attempts, and rejected requests all remain clean;
- Step 27 summary: **25/25 PASS**;
- OfflineReady afterward: **PASS**;
- Foundation afterward: **5/5 PASS**.

Share `Reports/Step27-ControlledHarmonyPatchExecution.txt` on any failure. After Gate B the managed context is process-resident; after Gate S or later assume patch state may also remain process-resident and force-quit before any retry.
