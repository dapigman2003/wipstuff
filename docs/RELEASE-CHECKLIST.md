# Release Checklist — Step 35.0.8 Save/Platform/Godot Native-Boundary Localization

## Candidate identity

- step/candidate: **Step 35.0.8**
- version: `0.0.131 (131)`
- workflow: `ios-canonical`
- expected IPA: `artifacts/StS2-Launcher-Step-35.ipa`
- expected host TRX: `artifacts/test-results/step35.trx`
- expected current-run manifest: `Documents/StS2Launcher/Reports/Step35-CurrentRun.txt`
- expected last checkpoint: `Documents/StS2Launcher/Reports/Step35-LastCheckpoint.txt`
- expected run-specific checkpoint: `Documents/StS2Launcher/Reports/Step35-CrashCheckpoint-<RunId>.txt`
- expected run-specific static map: `Documents/StS2Launcher/Reports/Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`

## Required before device testing

- [ ] canonical static validation passes;
- [ ] full host suite passes when a .NET SDK is available;
- [ ] release identity is exactly `0.0.131 (131)` in csproj, Info.plist, shell release constants, UI source, testing docs and this checklist;
- [ ] iOS publish/package succeeds under `MtouchLink=None`, `TrimMode=copy`, `MtouchInterpreter=-all`;
- [ ] IPA verification succeeds and advertises Step 35.0.8 Save/Platform/Godot localization;
- [ ] exact closed Step-32 transformed SHA-256 remains `39c0a89ad0d5c6eb1553e23dd8537a7b7ab8278fad4115d186db5751570211ef`;
- [ ] Gate-A clone source open uses Cecil `ReadingMode.Deferred`, sees zero requests before `Configure`, and uses only the audited writer-only metadata surrogates before `module.Write`;
- [ ] Gate A creates `sts2.step35.0.8.instrumented.dll`, preserves identity/MVID, verifies all entry/callsite markers after serialization, and immediately re-hashes the exact transformed source unchanged;
- [ ] synthetic host regression round-trips `Action<string>::Invoke(!0)` and rejects regression to concrete `Invoke(string)`;
- [ ] synthetic host regression round-trips pre/post callsite markers immediately around a target `Godot.DirAccess` call;
- [ ] production clone verification requires exactly one `DirExistsAbsolute` and one `MakeDirRecursiveAbsolute` target callsite in `GodotFileIo.CreateDirectory`;
- [ ] Gate B CLR-loads only the diagnostic clone, never the exact transformed source or receipt-backed/prepared original;
- [ ] Gate C arms the launcher-owned durable callback before the one diagnostic `ExecuteVeryEarly()` invocation;
- [ ] active summary/UI/report text states that diagnostic 4/4 is **NOT STEP 35 CLOSURE**;
- [ ] no proprietary game DLL/app bundle/native game library/raw user payload, credentials, device identifiers, signing secrets, or user game data are present in the source ZIP.

## Device run

Force-quit/relaunch first. Run Step 35.0.8 once. A telemetry-initialization failure must stop before Gate A. Once Gate B begins, the process is spent.

The desired sequence is the existing Step-35 markers followed by the newly added Save/Platform/Godot markers. After a hard termination, preserve the current-run manifest, last-checkpoint file, exact run-specific journal/static map, and a matching `.ips` if one exists. Do not combine artifacts from different Run IDs/PIDs.

The decisive interpretation is:

- `INMETHOD_180` with no `INMETHOD_181` -> first `DirExistsAbsolute` call is the physical boundary;
- `INMETHOD_181` then `INMETHOD_182` with no `INMETHOD_183` -> `MakeDirRecursiveAbsolute` is the physical boundary;
- both post markers present -> continue localization; do not attribute the termination to either directory call.

Cancellation is INCONCLUSIVE. A 0.0.131 A–D 4/4 result is **diagnostic completion only** and cannot close exact Step 35. Do not broaden resolver/native/Harmony/Godot authority in this candidate.
