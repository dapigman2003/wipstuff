# Testing strategy — Step 08

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

Coverage retains the foundation/auth/session/ownership contracts and adds deterministic Step 08 metadata tests:

- target App ID remains exactly `2868840`;
- PICS `depots` parsing accepts only numeric depot nodes;
- optional depot platform metadata (`oslist`, `osarch`, `language`) is retained;
- direct manifest IDs and nested `gid` forms are parsed;
- invalid/zero manifest IDs are ignored;
- the discovery result cannot expose raw ownership-ticket bytes or a PICS access-token value;
- discovery summary counts depots and visible branch manifest IDs.

Host tests do not claim to prove live Steam PICS behavior, native iOS AOT/linking, or real iOS Keychain behavior.

## 2. Repository/build validation

Run:

```text
bash scripts/validate-step08.sh
```

The validator first protects the Steps 01–05 foundation, then verifies the Steps 06–07 regressions and the narrow Step 08 operation.

It requires PICS access-token + product-info calls and explicitly rejects depot-key, manifest-body, CDN, chunk, and file-download APIs in the Step 08 discovery implementation.

Codemagic additionally runs host tests, the isolated SteamKit iOS compatibility patch, .NET iOS AOT/native linking, IPA packaging, and IPA verification.

## 3. Physical-iPhone verification

The device must prove:

- saved-session authentication still works with matching Steam identity;
- Step 07 ownership is re-proven;
- the PICS token callback arrives;
- target app product info arrives without a missing-token flag;
- at least one numeric depot ID is discovered;
- at least one visible branch manifest ID is discovered;
- no depot key, manifest body, CDN, chunk, or file request is made;
- Steps 01–05 foundation still passes 5/5.

See `STEP-08-TEST.md`.
