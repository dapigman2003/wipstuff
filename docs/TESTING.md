# Foundation testing strategy — Steps 01–05

The project uses three test layers because no single test type can prove all of the iOS foundation.

## 1. Host unit tests

Project:

```text
tests/StS2Launcher.Core.Tests/StS2Launcher.Core.Tests.csproj
```

Run with:

```text
bash scripts/run-unit-tests.sh
```

These tests execute on normal .NET and cover deterministic logic: launcher Core/state behavior, credential-store contract semantics with a fake store, Steam HTTP-handler policy, Steam connection result evaluation, and final foundation-gate aggregation.

They deliberately do **not** open a live Steam connection or access iOS Keychain/UIKit.

## 2. Repository/build validation

Run with:

```text
bash scripts/validate-foundation.sh
```

This statically verifies the iOS project contract and build safeguards, including app/scene/lifecycle wiring, version pins, trim roots, the DiskArbitration filter, the guarded SteamKit build patch, absence of later-stage/authentication code, and the existence of the required unit-test coverage.

Codemagic additionally executes the build-only SteamKit patch, .NET iOS AOT/native link, IPA packaging, and IPA verification.

## 3. Physical-iPhone verification

The app's single final verification action proves the platform/runtime boundaries host tests cannot:

- UIKit window actually becomes visible and lifecycle reaches Active;
- real iOS Keychain supports set/get/overwrite/delete with the selected accessibility policy;
- SteamKit2 constructs and connects over the proven WebSocket/SocketsHttpHandler route;
- ConnectedCallback and DisconnectedCallback are delivered on the real device.

The final success gate is:

```text
FOUNDATION PASS — 5/5
```

This layered approach prevents unit tests from pretending to prove device-only behavior while still making deterministic regressions fail automatically before an IPA is published.
