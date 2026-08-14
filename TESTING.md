# Testing strategy — Steps 01–06.2

The project uses host unit tests, repository/build validation, and physical-iPhone verification because no single layer can prove all boundaries.

## 1. Host unit tests

Project:

```text
tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj
```

Run with:

```text
bash scripts/run-unit-tests.sh
```

These cover deterministic behavior including:

- Core/state-machine regression;
- credential-store set/get/overwrite/delete semantics;
- Steam CM HTTP-handler policy/result contracts;
- five-gate foundation aggregation;
- Steam Guard mobile-confirmation policy;
- Step 06.2 saved-session serialization, overwrite, clear and malformed-data handling;
- refresh-token redaction from `SteamSavedSession.ToString()`;
- saved-session resume result/identity-match contracts.

They do not claim to prove live Steam authentication or real iOS Keychain behavior.

## 2. Repository/build validation

Run:

```text
bash scripts/validate-step06-2.sh
```

This preserves the Steps 01–05 foundation checks and verifies the Step 06.2 source contract: persistent auth requested, minimal Keychain payload, save after successful logon, password-free token resume, stored/returned SteamID comparison, explicit clear/sign-out, no manual Guard-code entry, and no ownership/download work.

Codemagic additionally runs the host unit tests, the isolated SteamKit iOS compatibility patch, .NET iOS AOT/native link, IPA packaging and IPA verification.

## 3. Physical-iPhone verification

The device must prove the platform/runtime boundaries:

- foundation remains 5/5;
- Step 06.1 credential/mobile-Guard flow still succeeds;
- persistent refresh token can be written to real iOS Keychain;
- force-close/relaunch still finds the saved session;
- saved refresh token authenticates without password entry/new Guard flow;
- returned Steam identity matches the stored identity;
- explicit sign-out removes the Keychain session and it remains absent after another relaunch.

See `STEP-06.2-TEST.md`.
