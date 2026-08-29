# Release Checklist — Step 35.0.2 ExecuteVeryEarly Invoke-Crash Static IL/Callsite Localization

## Candidate identity

- step/candidate: **Step 35.0.2**
- version: `0.0.125 (125)`
- workflow: `ios-canonical`
- expected IPA: `artifacts/StS2-Launcher-Step-35.ipa`
- expected host TRX: `artifacts/test-results/step35.trx`
- expected runtime checkpoint: `Documents/StS2Launcher/Reports/Step35-CrashCheckpoint.txt`
- expected static map: `Documents/StS2Launcher/Reports/Step35-ExecuteVeryEarly-StaticMap.txt`
- expected normal managed report when control survives: `Documents/StS2Launcher/Reports/Step35-TransformedRealStS2VeryEarlyInitialization.txt`

## Required before device testing

- [ ] canonical static validation passes;
- [ ] full host suite passes;
- [ ] release identity is exactly `0.0.125 (125)`;
- [ ] iOS publish/package succeeds under `MtouchLink=None`, `TrimMode=copy`, `MtouchInterpreter=-all`;
- [ ] IPA verification succeeds and advertises Step 35.0.2;
- [ ] stable `ios-canonical` cache key and existing NuGet/Godot/iOS-arm64 obj cache paths remain intact;
- [ ] no proprietary `sts2.dll`, app bundle, native game library, raw static map, credentials, raw device crash report, device identifiers, or signing secrets are present in the source ZIP.

## Device run

Force-quit/relaunch first. Run Step 35 once. Gate A must re-run Step-32 A–D, exact target/plan verification, and create the static map before CLR admission. Gate B must reproduce the physically proven 0.0.124 exact transformed-primary admission. Gate C remains one exact transformed `ExecuteVeryEarly()` invocation and <=60-second returned-Task await under exact resolver policy. Gate D remains the final isolation reproof.

If the app hard-terminates, **do not immediately rerun**. Preserve `Step35-ExecuteVeryEarly-StaticMap.txt`, `Step35-CrashCheckpoint.txt`, and the matching `.ips` first. The expected repeated runtime frontier is after `C_INVOKE_START` and before `C_INVOKE_RETURNED`; the purpose of 0.0.125 is to correlate that frontier to exact static IL/callsites, not to prove Step-35 compatibility.

Cancellation is INCONCLUSIVE rather than compatibility failure, but the process is spent after Gate B/C begins.

Accept physical Step-35 closure only on ordered A–D **4/4 PASS**. Do not broaden resolver/native/Harmony/Godot authority in this diagnostic candidate.
