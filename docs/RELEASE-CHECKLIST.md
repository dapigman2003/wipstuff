# Release Checklist — Step 27.0.5

## Source / policy

- Steps 01–26 remain closed/protected.
- Preserve physical Step 27.0 / 0.0.84 A–Q PASS / 17/25 evidence.
- Preserve physical 0.0.85–0.0.87 Gate-O metadata evidence.
- Record physical 0.0.88 unstable N–Q termination observation and the expected stale-process Gate-A rejection.
- Step 27.0.5 keeps the 26-gate launcher-only patch boundary unchanged.
- Gate O no longer invokes `RuntimeInformation.FrameworkDescription`; Gate R owns that first reflected getter invocation plus AccessTools initialization.
- Synchronous crash checkpoints cover every gate transition and sensitive O/R/S/T substages.
- No protected Step 23/24/25/26 behavior file is edited.
- `TrimMode=full`, `MtouchInterpreter=-all`, established roots/preservation policies remain active.
- StS2 reflection/patching/invocation, broad Harmony discovery, Godot startup, native game libraries remain absent.

## Build identity / visible app identity

- version: `0.0.89 (89)`
- workflow: `ios-step-27`
- IPA: `artifacts/StS2-Launcher-Step-27.ipa`
- TRX: `artifacts/test-results/step27.trx`
- top launcher banner: **Step 27.0.5**, bundle-derived **Version 0.0.89**, current short description/status.
- validator rejects stale Step-26 and prior Step-27 banner identity.

## Device-run discipline

- Force-quit/relaunch before the run.
- Once Gate B starts, force-quit before any retry, regardless of failure gate.
- If the app terminates without a managed report, preserve `Documents/StS2Launcher/Reports/Step27-CrashCheckpoint.txt` before another attempt.
- If Gate T or later runs, additionally assume launcher probe patch state remains process-resident.

## Authority

Require static validation, host tests, iOS publish, and IPA verification PASS before installation. Physical A–Z expected **26/26**, then OfflineReady PASS and Foundation 5/5.
