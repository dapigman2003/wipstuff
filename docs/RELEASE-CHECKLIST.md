# Release Checklist — Step 35.0 Controlled Transformed Real-StS2 Very-Early Initialization

## Candidate identity

- step/candidate: **Step 35.0**
- version: `0.0.123 (123)`
- workflow: `ios-canonical`
- expected IPA: `artifacts/StS2-Launcher-Step-35.ipa`
- expected host TRX: `artifacts/test-results/step35.trx`
- expected device report: `Documents/StS2Launcher/Reports/Step35-TransformedRealStS2VeryEarlyInitialization.txt`

## Required before device testing

- [ ] canonical static validation passes;
- [ ] full host suite passes;
- [ ] release identity is exactly `0.0.123 (123)`;
- [ ] iOS publish/package succeeds under `MtouchLink=None`, `TrimMode=copy`, `MtouchInterpreter=-all`;
- [ ] IPA verification succeeds and advertises Step 35.0;
- [ ] stable `ios-canonical` cache key and existing NuGet/Godot/iOS-arm64 obj cache paths remain intact;
- [ ] no proprietary `sts2.dll`, app bundle, native game library, credentials, or signing secrets are present in the source ZIP.

## Device run

Force-quit/relaunch first. Gate A must re-run Step-32 A–D, require the exact physically closed source/transformed evidence, validate source token `0x06007D02`, exact static parameterless Task signature, source `<ExecuteVeryEarly>d__7::MoveNext` token `0x0600BC71`, source/transformed semantic equality, zero direct later OneTimeInitialization calls, zero direct Harmony refs, and exact prepared resolver plan. Gate B must admit only exact transformed primary into `StS2Launcher-Step35-VeryEarly` with zero primary-admission resolution. Gate C must invoke exact transformed `ExecuteVeryEarly()` once and await its returned Task for at most 60 seconds; exact host bindings and hash-pinned initializer-free private dependencies only. Gate D must re-prove source/transformed/plan/dependency/context isolation.

Accept only **4/4 PASS**. Any first failure is authoritative. Do not broaden resolver/native/Harmony/Godot authority in the same candidate and do not rerun after Gate B in the same process.
