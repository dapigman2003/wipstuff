# StS2 Launcher iOS — Step 05.3

Step 05.2 solved the native iPhoneOS link boundary. Its IPA built, installed,
launched, stayed open, loaded SteamKit2 3.3.1.0, and retained the 12/12 Core
regression test. The Steam connection probe then failed immediately at 0/3 with
`PlatformNotSupportedException: Arg_PlatformNotSupported`.

The 0/3 boundary means the exception occurs while constructing `SteamClient`,
before the probe reaches any network operation. SteamKit2 3.3.1's constructor
reads `System.Diagnostics.Process.StartTime`; .NET marks that property unsupported
on iOS.

Step 05.3 makes one platform-compatibility change before the iOS AOT/link stage:

1. restore SteamKit2 3.3.1 into a repository-local, disposable NuGet cache;
2. run a build-only Mono.Cecil tool against that local SteamKit assembly;
3. require exactly one `Process.StartTime` call in `SteamKit2.SteamClient`;
4. replace that call with `DateTime.UtcNow` while preserving SteamKit's surrounding
   `Process` lifetime/disposal code;
5. remove the third-party strong-name publisher signature from the modified local
   build copy rather than pretending to re-sign it;
6. compile the launcher against that already-patched local assembly;
7. retain Step 05.2's proven `DiskArbitration` framework filter.

The patcher is a **build-only compatibility tool** under `tools/`. Mono.Cecil is
not referenced by Core or by the iOS application and is not packaged in the IPA.
This is not the later StS2 RuntimePatch subsystem.

## Scope remains unchanged

The physical-device test still performs no authentication. It only:

1. constructs `SteamClient`;
2. constructs `CallbackManager`;
3. calls `Connect()`;
4. waits for `ConnectedCallback`;
5. requests `Disconnect()`;
6. waits for `DisconnectedCallback`.

Still excluded:

- Steam login/authentication;
- passwords/tokens/Steam Guard;
- ownership checks;
- depot downloads;
- runtime/game Mono.Cecil patching;
- Godot;
- game files.

## Device marker

```text
STEP 05.3 — IOS STEAMCLIENT COMPAT
Version 0.0.9
```

## Build artifact

```text
artifacts/StS2-Launcher-Step-05.3.ipa
```

## Key diagnostics

```text
artifacts/logs/step05-3-steamkit-patch.log
artifacts/logs/step05-3-framework-filter.log
artifacts/logs/step05-3-generated-linker-frameworks.txt
artifacts/logs/step05-3-native-symbols.log
artifacts/logs/step05-3-publish.log
artifacts/logs/step05-3-dotnet-ios.binlog
```

The SteamKit patch log must contain:

```text
STEP05.3 STEAMKIT IOS PATCH: PASS
Replacement count: 1
Unsupported call removed: System.Diagnostics.Process.StartTime
Replacement value: System.DateTime.UtcNow
```

See `docs/STEP-05.3-TEST.md` for the report format.
