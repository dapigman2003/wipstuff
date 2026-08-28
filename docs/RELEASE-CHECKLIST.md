# Release Checklist — Step 35.0.1 Very-Early B→C Hard-Termination Crash Localization

## Candidate identity

- step/candidate: **Step 35.0.1**
- version: `0.0.124 (124)`
- workflow: `ios-canonical`
- expected IPA: `artifacts/StS2-Launcher-Step-35.ipa`
- expected host TRX: `artifacts/test-results/step35.trx`
- expected normal device report: `Documents/StS2Launcher/Reports/Step35-TransformedRealStS2VeryEarlyInitialization.txt`
- expected crash-localization telemetry: `Documents/StS2Launcher/Reports/Step35-CrashCheckpoint.txt`

## Required before device testing

- [ ] canonical static validation passes;
- [ ] full host suite passes;
- [ ] release identity is exactly `0.0.124 (124)`;
- [ ] iOS publish/package succeeds under `MtouchLink=None`, `TrimMode=copy`, `MtouchInterpreter=-all`;
- [ ] IPA verification succeeds and advertises Step 35.0.1;
- [ ] stable `ios-canonical` cache key and existing NuGet/Godot/iOS-arm64 obj cache paths remain intact;
- [ ] no proprietary `sts2.dll`, app bundle, native game library, credentials, raw device crash report, device identifiers, or signing secrets are present in the source ZIP.

## Device run

Force-quit/relaunch first. Run Step 35 once. Gate A must re-run Step-32 A–D and exact target/plan verification. Gate B must admit only exact transformed primary into `StS2Launcher-Step35-VeryEarly` with zero primary-admission resolution. Gate C must invoke exact transformed `ExecuteVeryEarly()` once and await its returned Task for at most 60 seconds; exact host bindings and hash-pinned initializer-free private dependencies only. Gate D must re-prove source/transformed/plan/dependency/context isolation.

If the app hard-terminates, **do not immediately rerun**. First preserve `Step35-CrashCheckpoint.txt` and the matching iOS `.ips` if available. The last durable checkpoint is authoritative for localization. If the last line is `C_INVOKE_START` with no `C_INVOKE_RETURNED`, the crash frontier is inside the first controlled ExecuteVeryEarly reflection invocation/runtime entry. If it is `B_LOADFROMSTREAM_START` with no PASS, the frontier remains inside primary admission.

Cancellation is INCONCLUSIVE rather than a compatibility failure, but the process is still spent after Gate B/C begins.

Accept physical Step-35 closure only on **4/4 PASS**. Any first managed failure is authoritative. Do not broaden resolver/native/Harmony/Godot authority in this diagnostic candidate and do not rerun after Gate B in the same process.
