# Release Checklist — Step 27.0.24

## Source / policy

- Steps 01–26 remain closed/protected.
- Preserve physical 0.0.105/106 trimming evidence and physical 0.0.107 `PatchFunctions.UpdateWrapper` `NotImplementedException` evidence.
- `MtouchLink=None`, `TrimMode=copy`, and `MtouchInterpreter=-all` remain the active host policy; broad `UseInterpreter=true` and NativeAOT remain prohibited.
- Raw Harmony normalization remains restricted to exact `0Harmony` 2.4.2 and changes only the admitted `HarmonySharedState::.cctor` slot in the in-memory image.
- The new Step-27 interpreted fixture is launcher-owned, IL-only, unsigned, framework-only, and absent from the iOS/test project-reference graph.
- Build/copy ordering must prove the fixture is placed into the `.app` only after `dotnet publish`.
- Gate P must create a fresh PatchProcessor for the exact interpreted Target through public `Harmony.CreateProcessor(MethodBase)`; do not mutate `PatchProcessor.original`.
- Gate Q must establish 42/42 baseline through Target reflection and the in-fixture InvokeTarget direct-call path.
- Gate T invokes public `PatchProcessor.Patch()` exactly once.
- If Patch() succeeds, Gate V must prove 1041/1041 with TargetCalls unchanged, Gate W unpatchs exactly the interpreted prefix once, and Gate Y restores 42/42.
- Do not force MonoMod DMD/backend environment switches.
- Source/live/prepared game/Harmony files remain immutable.
- No StS2 member reflection, patching, or invocation; no Godot/game startup; no native game-library loading.
- `MASTER-PLAN.md` remains unchanged from 0.0.107 for this candidate. A physical interpreted-fixture Patch() failure triggers the Step-28 architecture revision; no further Harmony-internal candidate is admitted.

## Build identity

- version: `0.0.108 (108)`
- workflow: `ios-step-27`
- IPA: `artifacts/StS2-Launcher-Step-27.ipa`
- TRX: `artifacts/test-results/step27.trx`
- top launcher banner: **Step 27.0.24**, bundle-derived **Version 0.0.108**, physical 0.0.107 NotImplemented diagnosis and the single post-publish interpreted decision experiment.

## Device-run discipline

- Force-quit/relaunch before the run. Once Gate B starts, force-quit before any retry.
- Preserve `Step27-CrashCheckpoint.txt` before another attempt after abrupt termination.
- Gate P must report the exact post-publish fixture SHA-256 and a fresh processor retaining its Target.
- Gate Q must report baseline TargetCalls=2 / PrefixCalls=0.
- T7 is the single public PatchProcessor.Patch() entry.
- If Patch() returns, finish V–Y; do not stop after installation because actual interpreted behavior and unpatch/restoration are the acceptance proof.
- If Patch() fails, preserve the report; that result ends Harmony-internal iteration and triggers Step 28.

## Authority

Require static validation, the full Codemagic host suite including the hash-pinned real-Harmony normalizer test and post-publish interpreted-fixture test, iOS publish, and IPA verification PASS before installation. Physical A–Z expected **26/26**, then OfflineReady PASS and Foundation 5/5.
