# Testing strategy — Step 07

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

Coverage retains the proven foundation/auth/session contracts and adds Step 07 deterministic ownership rules:

- Core/state-machine regression;
- credential-store set/get/overwrite/delete semantics;
- Steam CM HTTP-handler policy/result contracts;
- five-gate foundation aggregation;
- Steam Guard mobile-confirmation policy;
- saved-session serialization/overwrite/clear/malformed-data handling;
- persistent token logon contract and fresh LoginID;
- session-recovery preservation/clear policy;
- target App ID is exactly `2868840`;
- ownership requires exact AppID + `EResult.OK` + non-empty ticket;
- non-OK, empty-ticket, and wrong-AppID responses do not prove ownership;
- ownership result objects cannot expose raw `byte[]` ticket data.

Host tests do not claim to prove live Steam ownership or real iOS Keychain behavior.

## 2. Repository/build validation

Run:

```text
bash scripts/validate-step07.sh
```

This first runs the Steps 01–05 foundation validator, then verifies that Steps 06–06.3.1 remain intact and that Step 07 adds only the ownership-ticket boundary.

It explicitly rejects PICS/depot/manifest/CDN/download APIs inside the Step 07 ownership implementation.

Codemagic additionally runs host tests, the isolated SteamKit iOS compatibility patch, .NET iOS AOT/native linking, IPA packaging, and IPA verification.

## 3. Physical-iPhone verification

The device should prove:

- saved-session authentication still succeeds with matching Steam identity;
- `GetAppOwnershipTicket(2868840)` receives `AppOwnershipTicketCallback`;
- callback AppID is exactly `2868840`;
- ownership result is `OK`;
- ticket length is non-zero;
- no PICS/depot/manifest/CDN/download request is made;
- Steps 01–05 foundation remains 5/5.

See `STEP-07-TEST.md`.
