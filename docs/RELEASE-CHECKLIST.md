# Release Checklist — Step 35.0.9 Null-Platform Constructor Callsite Localization

## Identity

- step/candidate: **Step 35.0.9**
- version: `0.0.132 (132)`
- IPA: `StS2-Launcher-Step-35.ipa`
- TRX: `step35.trx`

## Before packaging

- [ ] closed Step-32/33/34 manifests verify unchanged;
- [ ] active Step-35.0.9 candidate manifest verifies;
- [ ] canonical static validator passes completely;
- [ ] host tests pass, including generic MemberRef round-trip, selected Godot callsite round-trip and NullPlatform constructor callsite-sweep round-trip;
- [ ] release identity is exactly `0.0.132 (132)` in csproj, Info.plist, shell release constants, UI source, testing docs and this checklist;
- [ ] no supplied proprietary game DLLs/deps files are packaged into the launcher source archive;
- [ ] IPA verification succeeds and advertises `STEP 35.0.9 — NULL-PLATFORM CONSTRUCTOR CALLSITE LOCALIZATION`;
- [ ] Gate A creates `sts2.step35.0.9.instrumented.dll`, preserves identity/MVID, verifies prior markers plus every NullPlatform sweep pair after serialization, and immediately re-hashes the exact transformed source unchanged;
- [ ] same-run static map contains `[NULL PLATFORM CTOR IL]` and constructor CALLSITE ordinals;
- [ ] direct base constructor is intentionally not wrapped by the NP sweep;
- [ ] no Godot bootstrap/startup or resolver broadening is present.

## Physical run

Force-quit/relaunch first. Run Step 35.0.9 once. A telemetry-initialization failure must stop before Gate A. Once Gate B begins, the process is spent.

After a hard termination preserve at minimum:

- `Step35-CurrentRun.txt` if present;
- matching `Step35-CrashCheckpoint-<RunId>.txt`;
- matching `Step35-ExecuteVeryEarly-StaticMap-<RunId>.txt`;
- `Step35-LastCheckpoint.txt`.

Use the final `INMETHOD_NPxxx_PRE/POST` marker and the same-run constructor static map to localize the exact callsite. Do not attribute the final resolver event as root cause merely because it is last.

Cancellation is INCONCLUSIVE. A 0.0.132 A–D 4/4 result is **diagnostic completion only** and cannot close exact Step 35. Do not broaden resolver/native/Harmony/Godot authority in this candidate.
