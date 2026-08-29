# Release Checklist — Step 35.0.4 In-Method Pre-First-Await Localization

## Candidate identity

- step/candidate: **Step 35.0.4**
- version: `0.0.127 (127)`
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
- [ ] full host suite passes when a .NET SDK is available;
- [ ] release identity is exactly `0.0.127 (127)`;
- [ ] iOS publish/package succeeds under `MtouchLink=None`, `TrimMode=copy`, `MtouchInterpreter=-all`;
- [ ] IPA verification succeeds and advertises Step 35.0.4 diagnostic localization;
- [ ] stable `ios-canonical` cache key and existing NuGet/Godot/iOS-arm64 obj cache paths remain intact;
- [ ] the exact closed Step-32 transformed SHA-256 remains `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`;
- [ ] Gate A creates a separate `sts2.step35.0.4.instrumented.dll`, preserves identity/MVID, verifies its hash/signature/marker bridge, and immediately re-hashes the exact transformed source unchanged;
- [ ] Gate B CLR-loads only the diagnostic clone, never the exact transformed source or receipt-backed/prepared original;
- [ ] Gate C arms `C_DIAGNOSTIC_BRIDGE_ARMED` before the one diagnostic-clone `ExecuteVeryEarly()` invocation;
- [ ] active summary/UI/report text states that diagnostic 4/4 is **NOT STEP 35 CLOSURE**;
- [ ] no proprietary `sts2.dll`, app bundle, native game library, raw device payload, credentials, device identifiers, signing secrets, or user game data are present in the source ZIP.

## Device run

Force-quit/relaunch first. Run Step 35.0.4 once. Before Gate A, the UI must successfully establish durable run-correlated telemetry; a telemetry failure must stop visibly and perform no CLR admission/invocation.

Gate A must re-run Step-32 A–D, verify exact `ExecuteVeryEarly` wrapper/MoveNext metadata, create and verify the separate diagnostic clone, re-hash the exact transformed source unchanged after clone emission, and write the same-Run-ID exact-source static map before Gate B.

Gate B must re-hash both artifacts and admit **only the diagnostic clone** under the existing strict Step-35 resolver. Gate C must bind that clone's target, arm the launcher-owned `Action<string>` checkpoint bridge, perform one reflected invocation, and await a returned Task for at most 60 seconds if control returns. Gate D remains an isolation reproof for source, clone, plan, private dependencies, and context ownership.

If the app hard-terminates, **do not immediately rerun**. Preserve `Step35-CurrentRun.txt`, `Step35-LastCheckpoint.txt`, the exact run-specific journal/static-map filenames named by the current-run manifest, and the matching `.ips` first. Do not combine artifacts with different Run IDs/PIDs. The final durable `INMETHOD_*` line is the primary localization result.

Cancellation is INCONCLUSIVE rather than compatibility failure, but the process is spent after Gate B/C begins.

A 0.0.127 A–D 4/4 result is **diagnostic completion only** and cannot close exact Step 35 because the executed image is an instrumented derivative. Step 35 remains OPEN until a separately defined authoritative transformed artifact passes its physical closure contract. Do not broaden resolver/native/Harmony/Godot authority in this diagnostic candidate.
