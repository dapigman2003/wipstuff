# Testing strategy — Steps 01–06.3

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
- saved-session resume result/identity-match contracts;
- Step 06.3.1 persistent-token/session-retry contract;
- destructive clear for invalid local data, identity mismatch, `InvalidPassword`, `Revoked`, and `Expired`;
- preservation for transient/routing results, timeout, and cancellation.

They do not claim to prove live Steam authentication or real iOS Keychain behavior.

## 2. Repository/build validation

Run:

```text
bash scripts/validate-step06-3-1.sh
```

This preserves the Steps 01–05 foundation and Steps 06–06.2 authentication/persistence contracts, then verifies the Step 06.3.1 persistent-token settings, unique LoginID helper, JWT timing parser, automatic restore trigger, and conservative recovery policy. It also verifies no ownership/download or manual Guard-code scope was introduced.

Codemagic additionally runs the host unit tests, isolated SteamKit iOS compatibility patch, .NET iOS AOT/native link, IPA packaging, and IPA verification.

## 3. Physical-iPhone verification

The device should prove:

- the saved Step 06.2 Keychain session survives the app upgrade;
- first Active lifecycle automatically starts saved-session restore;
- automatic restore authenticates with no password and no new Guard prompt;
- returned identity still matches the saved SteamID;
- saved session remains present after successful restore;
- Steps 01–05 foundation remains 5/5.

See `STEP-06.3.1-TEST.md`.
