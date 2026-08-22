# Release Checklist — Step 27.0.2

## Source / policy

- Steps 01–26 remain closed/protected.
- Preserve physical Step 27.0 / 0.0.84 A–Q PASS / 17/25 evidence.
- Preserve physical Step 27.0.1 / 0.0.85 Gate-O 14/26 metadata evidence.
- Step 27.0.2 changes only the AccessTools measured metadata policy, bounded framework-preservation anchor, and associated diagnostics/tests; the launcher-only patch boundary is unchanged.
- No protected Step 23/24/25/26 behavior file is edited.
- `TrimMode=full`, `MtouchInterpreter=-all`, established roots/preservation policies remain active.
- StS2 reflection/patching/invocation, broad Harmony discovery, Godot startup, native game libraries remain absent.

## Build identity

- version: `0.0.86 (86)`
- workflow: `ios-step-27`
- IPA: `artifacts/StS2-Launcher-Step-27.ipa`
- TRX: `artifacts/test-results/step27.trx`

## Authority

Require static validation, host tests, iOS publish, and IPA verification PASS before installation. Fresh physical run A–Z, expected **26/26**, then OfflineReady PASS and Foundation 5/5.

Failure meaning: O = metadata/preservation preflight only; R = explicit AccessTools initialization remains open; S = descriptor construction; T = first patch engine; U–Z = post-patch behavior/removal/integrity. Force-quit after any Gate-T-or-later failure.
