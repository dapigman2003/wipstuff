# StS2 Launcher iOS — Step 05.1

Step 05 did **not** fail in C# compilation. It reached the final iOS native link and failed with:

```text
ld: framework 'DiskArbitration' not found
```

The Step-05 app introduced one generic third-party assembly: SteamKit2 3.3.1.

.NET 9 iOS device builds use `TrimMode=partial` by default. Partial trimming does not trim every third-party assembly. Step 05.1 changes only the publish/link boundary by setting:

```xml
<TrimMode>full</TrimMode>
```

This lets the iOS trimmer remove unreachable desktop/macOS code before the Apple native linker scans the surviving P/Invoke/native dependencies.

## What did NOT change

The Steam test is still exactly the same network-only test:

1. construct `SteamClient`;
2. construct `CallbackManager`;
3. call `Connect()`;
4. wait for `ConnectedCallback`;
5. immediately call `Disconnect()`;
6. wait for `DisconnectedCallback`.

There is still:

- no username;
- no password;
- no Steam Guard;
- no Steam token;
- no ownership check;
- no depot download;
- no Godot;
- no Cecil.

## Same-repository note

This package deliberately keeps the same project path as Step 05:

```text
src/StS2Launcher.Step05.iOS/
```

So if you are replacing Step-05 files in the same Git repository, overwrite those files rather than creating another iOS project folder.

The version shown on-device is:

```text
STEP 05.1 — STEAM LINK FIX
Version 0.0.7
```

## Build

Codemagic workflow:

```text
ios-step-05-1
```

Expected artifact:

```text
artifacts/StS2-Launcher-Step-05.1.ipa
```

If native linking still fails, this version preserves extra diagnostics in:

```text
artifacts/logs/step05-1-native-preflight.log
artifacts/logs/step05-1-publish.log
artifacts/logs/step05-1-failure-scan.log
artifacts/logs/step05-1-dotnet-ios.binlog
```

See `docs/STEP-05.1-TEST.md`.
