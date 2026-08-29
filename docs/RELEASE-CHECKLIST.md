# Release Checklist — Step 35.0.3 Run-Correlated Durable Telemetry

## Candidate identity

- step/candidate: **Step 35.0.3**
- version: `0.0.126 (126)`
- workflow: `ios-canonical`
- expected IPA: `artifacts/StS2-Launcher-Step-35.ipa`
- expected host TRX: `artifacts/test-results/step35.trx`
- expected current-run manifest: `Documents/StS2Launcher/Reports/Step35-CurrentRun.txt`
- expected last checkpoint: `Documents/StS2Launcher/Reports/Step35-LastCheckpoint.txt`
- expected run-specific checkpoint: `Documents/StS2Launcher/Reports/Step35-CrashCheckpoint-<RunId>.txt`
- expected run-specific static map: `Documents/StS2Launcher/Reports/Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`
- expected normal managed report when control survives: `Documents/StS2Launcher/Reports/Step35-TransformedRealStS2VeryEarlyInitialization.txt`

## Required before device testing

- [ ] canonical static validation passes;
- [ ] full host suite passes;
- [ ] release identity is exactly `0.0.126 (126)`;
- [ ] iOS publish/package succeeds under `MtouchLink=None`, `TrimMode=copy`, `MtouchInterpreter=-all`;
- [ ] IPA verification succeeds and advertises Step 35.0.3;
- [ ] stable `ios-canonical` cache key and existing NuGet/Godot/iOS-arm64 obj cache paths remain intact;
- [ ] no proprietary `sts2.dll`, app bundle, native game library, raw static map, credentials, raw device crash report, device identifiers, or signing secrets are present in the source ZIP.

## Device run

Force-quit/relaunch first. Run Step 35 once. Before Gate A, the UI must successfully establish durable run-correlated telemetry; a telemetry failure must stop visibly and perform no CLR admission/invocation. Gate A must re-run Step-32 A–D and write the same-Run-ID static map before Gate B. Gate B must reproduce the physically proven 0.0.124 exact transformed-primary admission. Gate C remains one exact transformed `ExecuteVeryEarly()` invocation and <=60-second returned-Task await under exact resolver policy. Gate D remains the final isolation reproof.

If the app hard-terminates, **do not immediately rerun**. Preserve `Step35-CurrentRun.txt`, `Step35-LastCheckpoint.txt`, the exact run-specific journal/static-map filenames named by the current-run manifest, and the matching `.ips` first. Do not combine artifacts with different Run IDs/PIDs.

Cancellation is INCONCLUSIVE rather than compatibility failure, but the process is spent after Gate B/C begins.

Accept physical Step-35 closure only on ordered A–D **4/4 PASS**. Do not broaden resolver/native/Harmony/Godot authority in this diagnostic candidate.
