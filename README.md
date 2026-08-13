# StS2 Launcher iOS — Step 05

Step 04 passed on physical iPhone, including Keychain persistence/delete and Core 12/12.

Step 05 introduces **SteamKit2 and outbound Steam networking only**.

## Absolutely NOT in this build

- Steam username
- Steam password
- Steam Guard
- QR login
- refresh/access tokens
- ownership verification
- depot download
- Steam Cloud
- Godot
- Mono.Cecil
- game files

## Why SteamKit2 3.3.1

The launcher remains on the already-proven .NET 9 toolchain.

SteamKit2 `3.3.1` is intentionally pinned because the next SteamKit line (`3.4.0`) targets .NET 10. Runtime/toolchain migration is not being mixed into the first Steam network test.

## What the button does

`Run Steam Connection Probe`:

1. constructs `SteamClient`;
2. constructs `CallbackManager`;
3. subscribes to `ConnectedCallback`;
4. calls `SteamClient.Connect()`;
5. pumps SteamKit callbacks;
6. on connection, immediately calls `Disconnect()`;
7. waits for `DisconnectedCallback`;
8. reports `STEAM CONNECTION PASS — 3/3`.

No `SteamUser` login is performed.

## Build

Codemagic workflow:

```text
ios-step-05
```

Expected artifact:

```text
artifacts/StS2-Launcher-Step-05.ipa
```

See `docs/STEP-05-TEST.md`.
