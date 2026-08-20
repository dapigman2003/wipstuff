# Release Checklist — Step 23.4.3

## Source/package

- canonical live iOS project is `src/StS2Launcher.iOS/StS2Launcher.iOS.csproj`;
- no live legacy `StS2Launcher.Step05.iOS` path;
- `history.zip` is optional/inert and not needed by validation or build;
- no game payload, Steam reusable secrets, Apple signing secrets, or proprietary native game binaries in source/archive;
- Step 22.4.2 protected behavior remains intact;
- Step 23 adds only the explicit first-real-load subsystem, host tests, UI/reporting, and current docs/tooling updates.

## Static/host build

- `bash scripts/validate.sh` passes;
- `bash scripts/test.sh` passes;
- Godot build/preflight passes on Codemagic/macOS;
- iOS publish succeeds with `MtouchInterpreter=-all`, `UseInterpreter!=true`, `PublishAot!=true`;
- IPA verification passes;
- expected version is 0.0.72 (72);
- workflow is `ios-step-23-4-3`.

## Device

- header `STEP 23.4.3 — FIRST REAL STS2 CLR LOAD BOUNDARY`;
- start from a fresh process;
- Step 23 Gate A requires zero **primary** module initializers and zero binding blockers; initializer-bearing dependencies must be explicitly audited and deferred;
- Gate B first real `sts2.dll` CLR load passes;
- Gate C planned managed dependency closure resolves with zero rejected/unplanned and zero native requests;
- Gate D load isolation/byte/OfflineReady audit passes;
- Step 23 = 4/4;
- `Reports/Step23-FirstRealGameLoad.txt` exists;
- OfflineReady = PASS;
- Foundation 5/5 = PASS.

## Stop conditions

- If Gate A reports a module initializer on the primary `sts2.dll`, stop before loading and send the report. Dependency module initializers are expected to be audited/deferred, not loaded.
- If Gate B fails, do not continue to dependency probes; send the report and force-quit before retrying.
- If Gate C produces an unplanned managed request or native request, stop; do not broaden resolver/native search paths speculatively.
- If any gate after B fails, force-quit before rerunning Step 21/22 pre-load regressions.
