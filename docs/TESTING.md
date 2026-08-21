# Testing — Step 26 Controlled Empty Harmony PatchProcessor Creation

## Canonical static validation

Run:

`python3 tools/validate_current.py`

The validator must protect the physically closed Step 23, Step 24, and Step 25 boundaries while separately pinning the active Step 26 candidate.

It must verify at least:

- iOS version/release wiring = Step 26 / `0.0.83 (83)`;
- `TrimMode=full` and `MtouchInterpreter=-all` remain unchanged;
- the exact Step-22 root policy and physically proven `System.Collections.Concurrent` root remain present;
- the physically proven Step-25 Harmony-constructor `DynamicDependency` preservation anchor remains present;
- protected Step 23.4.3, Step 24.0.6, and Step 25.0.2 implementation manifests match;
- Step 26 has exactly fourteen ordered fail-fast gates A–N;
- Gates A–I retain the closed Step 25 replay behavior;
- Gate J metadata-audits exact `Harmony.CreateProcessor(MethodBase)`, `PatchProcessor::.cctor`, `PatchProcessor::.ctor(Harmony,MethodBase)`, and retained `instance`/`original` fields before processor execution;
- Gate K is the only explicit `PatchProcessor` type-initialization barrier;
- Gate L resolves only launcher-owned `HarmonyProcessorProbe.Target(int)` and does not invoke it;
- Gate M invokes `CreateProcessor(MethodBase)` but contains no `Patch()` / `Harmony.Patch` / `PatchAll` invocation;
- Gate N re-hashes/re-proves OfflineReady and exact context/native/resolver state;
- no StS2 member reflection/invocation, Godot startup, or native game-library admission is introduced.

## Host tests

Run `bash scripts/test.sh`.

Step 26 host tests retain all Step 24/25 safety fixtures and add synthetic coverage for the exact `CreateProcessor` / `PatchProcessor` shape and full A–N ordering. The synthetic positive path must complete all fourteen gates in a collectible private context while `PatchProcessor.Patch()` remains uncalled.

Expected TRX:

`artifacts/test-results/step26.trx`

## Codemagic

Workflow: `ios-step-26`

The pipeline runs static validation, host tests, Godot/native preflight, iOS publish, and IPA verification. Build/CI never bundles or loads the proprietary game payload.

Expected:

- version: `0.0.83 (83)`;
- IPA: `artifacts/StS2-Launcher-Step-26.ipa`;
- host TRX: `artifacts/test-results/step26.trx`.

## Physical acceptance

From a fresh process, run Step 26 A–N and stop at the first failed gate.

Require:

- Gates A–I: exact closed Step 25 replay;
- Gate J: exact processor API/metadata resolution only, with no PatchProcessor type initialization or object construction;
- Gate K: `RuntimeHelpers.RunClassConstructor(HarmonyLib.PatchProcessor.TypeHandle)` completes with unchanged hashes/context and zero native/unplanned resolver events;
- Gate L: exact launcher-owned `HarmonyProcessorProbe.Target(int)` MethodInfo resolves in the default host context and is not invoked;
- Gate M: exact `Harmony.CreateProcessor(MethodBase)` returns exact `HarmonyLib.PatchProcessor`, whose measured private retained fields reference the exact Harmony object and exact launcher probe MethodBase; no `Patch()` invocation;
- Gate N: final plan/prepared/live hashes, OfflineReady, private context, retained processor state, native attempts, and rejected requests all remain clean;
- Step 26 summary: **14/14 PASS**;
- OfflineReady afterward: **PASS**;
- Foundation afterward: **5/5 PASS**.

Share `Reports/Step26-ControlledHarmonyProcessorCreation.txt` on any failure. Once Gate B has loaded the managed context, force-quit before rerunning earlier fresh-process runtime regressions.
